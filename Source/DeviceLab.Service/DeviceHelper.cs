using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InfoSpace.DeviceLab.Service.Log;
using InfoSpace.DeviceLab.Status;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Service
{
    public class DeviceHelper
    {
        private readonly static ExecutorFactory executorFactory = new ExecutorFactory();
        private IExecutor executor;

        public async Task<JobStatusStep> PopulateDeviceData(DeviceId device)
        {
            var model = await GetDeviceModel(device.HardwareId);
            var brand = await GetDeviceBrand(device.HardwareId);
            var buildVersion = await GetDeviceBuildVersion(device.HardwareId);

            device.Model = model.Result;
            device.Brand = brand.Result;
            device.BuildVersion = buildVersion.Result;

            return GroupStatus(
                model,
                brand,
                buildVersion
            );
        }

        private async Task<JobStatusStep<string>> GetDeviceModel(string deviceId)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell getprop ro.product.model", deviceId));

            var result = string.IsNullOrWhiteSpace(executor.Output) ? executor.Error : executor.Output;

            return Status(
                result: result,
                success: !string.IsNullOrWhiteSpace(result)
            );
        }

        private async Task<JobStatusStep<string>> GetDeviceBrand(string deviceId)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell getprop ro.product.brand", deviceId));

            var result = string.IsNullOrWhiteSpace(executor.Output) ? executor.Error : executor.Output;

            return Status(
                result: result,
                success: !string.IsNullOrWhiteSpace(result)
            );
        }

        private async Task<JobStatusStep<string>> GetDeviceBuildVersion(string deviceId)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell getprop ro.build.version.release", deviceId));

            var result = string.IsNullOrWhiteSpace(executor.Output) ? executor.Error : executor.Output;

            return Status(
                result: result,
                success: !string.IsNullOrWhiteSpace(result)
            );
        }

        public async Task<JobStatusStep<string[]>> GetDevices()
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute("adb devices");

            var devices = ParseDeviceList(executor.Output);

            return Status(
                result: devices,
                success: devices.Length > 0
            );
        }

        private static string[] ParseDeviceList(string str)
        {
            var lines = str
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return lines
                .SkipWhile(x => x.StartsWith("*"))
                .Skip(1)
                .Select(x => x.Split('\t', ' ').First())
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();
        }

        public async Task<JobStatusStep> InstallApk(string deviceId, string apkPath, PackageInfo packageInfo)
        {
            return GroupStatus(
                await UninstallExistingPackage(deviceId, packageInfo),
                await InstallNewPackage(deviceId, apkPath)
            );
        }

        private async Task<JobStatusStep> UninstallExistingPackage(string deviceId, PackageInfo packageInfo)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} uninstall '{1}'", deviceId, packageInfo.Id));

            return Status(
                success: !HasErrors()
            );
        }

        private async Task<JobStatusStep> InstallNewPackage(string deviceId, string apkPath)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} install '{1}'", deviceId, apkPath));

            return Status(
                success: HasOutput() && Regex.IsMatch(executor.Output, "success", RegexOptions.IgnoreCase)
            );
        }

        public async Task<JobStatusStep> LaunchPackage(string deviceId, PackageInfo packageInfo)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell am start -n '{1}/{2}'", deviceId, packageInfo.Id, packageInfo.LaunchableActivity));

            return Status(
                success: HasOutput() && !HasErrors()
            );
        }

        public async Task<JobStatusStep> LaunchMonkey(string deviceId, PackageInfo packageInfo)
        {
            // Need to launch via 'sh -c' otherwise it blocks the command from completing.

            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell '(sh -c \"monkey -p {1} 999999\" >/dev/null 2>&1) &'", deviceId, packageInfo.Id));

            return Status(
                success: !HasErrors()
            );
        }

        public async Task<JobStatusStep> KillMonkey(string deviceId)
        {
            var status = await GetMonkeyProcessId(deviceId);
            var processId = status.Result;

            if (status.Success != true)
            {
                return new JobStatusStep
                {
                    Steps = new[]
                    {
                        status
                    },
                    OutputLog = "Could not find monkey process.",
                    Success = false
                };
            }

            return GroupStatus(
                status,
                await KillProcess(deviceId, processId)
            );
        }

        private async Task<JobStatusStep<string>> GetMonkeyProcessId(string deviceId)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell ps | grep com.android.commands.monkey | tr -s ' ' ' ' | cut -d ' ' -f 2", deviceId));

            return Status(
                result: executor.Output,
                success: HasOutput() && !HasErrors()
            );
        }

        private async Task<JobStatusStep> KillProcess(string deviceId, string processId)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell kill {1}", deviceId, processId));

            return Status(
                success: !HasErrors()
            );
        }

        public async Task<JobStatusStep> LaunchUrl(string deviceId, string url)
        {
            // This causes the URLs to reopen in the same tab. Removing this opens each URL in a new tab.
            // -e com.android.browser.application_id com.android.browser

            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("adb -s {0} shell \"am start -a android.intent.action.VIEW -e com.android.browser.application_id com.android.browser -d '{1}'\"", deviceId, url));

            return Status(
                success: !HasErrors()
            );
        }

        public JobStatusStep<PackageInfo> GetPackageInfo(string apkPath)
        {
            AndroidManifestProvider provider = new AndroidManifestProvider(apkPath);

            var packageInfo = provider.GetPackageInfo();

            return new JobStatusStep<PackageInfo>
            {
                Result = packageInfo,
                Success = !String.IsNullOrWhiteSpace(packageInfo.Id) && !String.IsNullOrWhiteSpace(packageInfo.LaunchableActivity)
            };
        }

        public async Task<JobStatusStep<PackageInfo>> GetPackageInfoAapt(string apkPath)
        {
            executor = executorFactory.CreateExecutor();
            await executor.Execute(string.Format("aapt dump badging {0}", apkPath));

            var packageInfo = new PackageInfo
            {
                Id = ParsePackageId(executor.Output),
                LaunchableActivity = ParseLaunchableActivity(executor.Output),
            };

            return Status(
                result: packageInfo,
                success: !HasErrors() && !String.IsNullOrWhiteSpace(packageInfo.Id) && !String.IsNullOrWhiteSpace(packageInfo.LaunchableActivity)
            );
        }

        private string ParsePackageId(string aaptOutput)
        {
            const string pkgPrefixString = "package: name='";
            const string pkgSuffixString = "'";

            int packageStart = aaptOutput.IndexOf(pkgPrefixString) + pkgPrefixString.Length;
            if (packageStart < 0)
            {
                return null;
            }
            int packageEnd = aaptOutput.IndexOf(pkgSuffixString, packageStart);
            if (packageEnd <= packageStart)
            {
                return null;
            }

            return aaptOutput.Substring(packageStart, packageEnd - packageStart);
        }

        private string ParseLaunchableActivity(string aaptOutput)
        {
            const string pkgPrefixString = "launchable-activity: name='";
            const string pkgSuffixString = "'";

            int packageStart = aaptOutput.IndexOf(pkgPrefixString) + pkgPrefixString.Length;
            if (packageStart < 0)
            {
                return null;
            }
            int packageEnd = aaptOutput.IndexOf(pkgSuffixString, packageStart);
            if (packageEnd <= packageStart)
            {
                return null;
            }

            return aaptOutput.Substring(packageStart, packageEnd - packageStart);
        }

        #region Status Helpers

        private JobStatusStep GroupStatus(params JobStatusStep[] statuses)
        {
            return new JobStatusStep
            {
                Steps = statuses,
                Success = statuses.All(x => !x.Success.HasValue || x.Success == true)
            };
        }

        private JobStatusStep<T> Status<T>(T result, bool success, JobStatusStep[] steps = null)
        {
            var status = new JobStatusStep<T>
            {
                Result = result
            };

            PopulateStatusData(status, success, steps);

            Logger.Debug(() => GetType().Name + "::Status " + JsonConvert.SerializeObject(status));

            return status;
        }

        private void PopulateStatusData(JobStatusStep status, bool success, JobStatusStep[] steps)
        {
            status.Command = executor.Command;
            status.OutputLog = executor.Output;
            status.ErrorLog = executor.Error;
            status.Steps = steps;
            status.Success = success;
        }

        private JobStatusStep Status(bool success, JobStatusStep[] steps = null)
        {
            var status = new JobStatusStep();

            PopulateStatusData(status, success, steps);

            Logger.Debug(() => GetType().Name + "::Status " + JsonConvert.SerializeObject(status));

            return status;
        }

        private bool HasErrors()
        {
            string output = executor.Output ?? "";

            return !String.IsNullOrWhiteSpace(executor.Error) &&
                !output.StartsWith("android error:", StringComparison.OrdinalIgnoreCase) &&
                !output.StartsWith("error:", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasOutput()
        {
            return !String.IsNullOrWhiteSpace(executor.Output);
        }

        #endregion
    }
}
