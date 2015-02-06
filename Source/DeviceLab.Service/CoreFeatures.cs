using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfoSpace.DeviceLab.Jobs;
using InfoSpace.DeviceLab.Service.Log;
using InfoSpace.DeviceLab.Status;

namespace InfoSpace.DeviceLab.Service
{
    public class CoreFeatures : ICoreFeatures
    {
        private readonly IDeviceList deviceList;
        private readonly IStatusConsumer statusConsumer;

        private PackageInfo lastLaunchedPackage;

        public CoreFeatures(IDeviceList deviceList, IStatusConsumer statusConsumer)
        {
            if (deviceList == null)
            {
                throw new ArgumentNullException("deviceList");
            }
            if (statusConsumer == null)
            {
                throw new ArgumentNullException("statusConsumer");
            }

            this.deviceList = deviceList;
            this.statusConsumer = statusConsumer;
        }

        public async Task LaunchApk(RunAppServiceJob job)
        {
            string path = null;

            try
            {
                path = await DownloadApk(job.ApkUrl);
                await LaunchApk(job, path);
            }
            catch (Exception e)
            {
                Logger.Error("Error launching APK ({0}): {1}", job.ApkUrl, e);
            }
            finally
            {
                if (path != null)
                {
                    File.Delete(path);
                }
            }
        }

        private async Task<string> DownloadApk(string apkUrl)
        {
            string tempPath = Path.GetTempFileName();

            Logger.Info("Downloading apk {0} to {1}...", apkUrl, tempPath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apkUrl);
            request.Accept = "application/octet-stream";

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var fileStream = File.OpenWrite(tempPath))
            {
                await stream.CopyToAsync(fileStream);
            }

            Logger.Info("Done downloading apk.", apkUrl, tempPath);

            return tempPath;
        }

        private async Task LaunchApk(RunAppServiceJob job, string apkPath)
        {
            Logger.Debug("Launching APK: {0}", apkPath);

            var packageStatus = new DeviceHelper().GetPackageInfo(apkPath);
            PackageInfo packageInfo = packageStatus.Result;

            lastLaunchedPackage = packageInfo;

            if (packageInfo != null)
            {
                Logger.Info("Loaded apk package info: {0}. Activity: {1}", packageInfo.Id, packageInfo.LaunchableActivity);
            }
            else
            {
                Logger.Error("FAILED to load apk package info: {0}", apkPath);
            }

            var tasks = deviceList
                .Devices
                .Select(device => LaunchApkOnDevice(job, apkPath, packageStatus, packageInfo, device));

            await Task.WhenAll(tasks);
        }

        private async Task LaunchApkOnDevice(RunAppServiceJob job, string apkPath, JobStatusStep<PackageInfo> packageStatus, PackageInfo packageInfo, DeviceId device)
        {
            Logger.Info("Launching APK on device {0}", device.HardwareId);

            try
            {
                var adb = new DeviceHelper();
                JobStatusStep[] steps =
                {
                    packageStatus,
                    await adb.KillMonkey(device.HardwareId),
                    await adb.InstallApk(device.HardwareId, apkPath, packageInfo),
                    await adb.LaunchPackage(device.HardwareId, packageInfo)
                };

                statusConsumer.ReportStatus(new JobStatus
                {
                    Steps = steps,
                    Success = steps[0].Success == true && steps.Skip(2).All(x => x.Success == true || !x.Success.HasValue),
                    DeviceId = device.HardwareId,
                    Time = DateTime.UtcNow,
                    JobId = job.Id
                });

                Logger.Info("Success launching APK on device {0}.", device.HardwareId);
            }
            catch (Exception e)
            {
                Logger.Error("FAILED to launch APK: " + e.ToString());
            }
        }

        public async Task LaunchStressTest(RunStressTestServiceJob job)
        {
            var tasks = deviceList
                .Devices
                .Select(device => RunMonkeyOnDevice(job, device));

            await Task.WhenAll(tasks);
        }

        private async Task RunMonkeyOnDevice(RunStressTestServiceJob job, DeviceId device)
        {
            var adb = new DeviceHelper();
            JobStatusStep[] steps =
            {
                await adb.KillMonkey(device.HardwareId),
                await adb.LaunchMonkey(device.HardwareId, lastLaunchedPackage)
            };

            statusConsumer.ReportStatus(new JobStatus
            {
                Steps = steps,
                Success = steps.Skip(1).All(x => x.Success == true),
                DeviceId = device.HardwareId,
                Time = DateTime.UtcNow,
                JobId = job.Id
            });
        }

        public async Task LaunchUrl(RunUrlServiceJob job)
        {
            var tasks = deviceList
                .Devices
                .Select(device => LaunchUrlOnDevice(job, device));

            await Task.WhenAll(tasks);
        }

        private async Task LaunchUrlOnDevice(RunUrlServiceJob job, DeviceId device)
        {
            var adb = new DeviceHelper();
            JobStatusStep[] steps =
            {
                await adb.KillMonkey(device.HardwareId),
                await adb.LaunchUrl(device.HardwareId, job.Url)
            };

            statusConsumer.ReportStatus(new JobStatus
            {
                Steps = steps,
                Success = steps.Skip(1).All(x => x.Success == true),
                DeviceId = device.HardwareId,
                Time = DateTime.UtcNow,
                JobId = job.Id
            });
        }

        public async Task KillMonkey()
        {
            var tasks = deviceList
                .Devices
                .Select(device => KillMonkeyOnDevice(device));

            await Task.WhenAll(tasks);
        }

        private async Task KillMonkeyOnDevice(DeviceId device)
        {
            var adb = new DeviceHelper();
            JobStatusStep[] steps =
            {
                await adb.KillMonkey(device.HardwareId)
            };

            statusConsumer.ReportStatus(new JobStatus
            {
                Steps = steps,
                Success = steps.All(x => x.Success == true),
                DeviceId = device.HardwareId,
                Time = DateTime.UtcNow
            });
        }
    }
}
