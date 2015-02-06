using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using InfoSpace.DeviceLab.Jobs;
using InfoSpace.DeviceLab.Service.Log;
using InfoSpace.DeviceLab.Status;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Service
{
    public class DeviceService : IDisposable, IStatusConsumer
    {
        private readonly object jobLock = new object();

        private readonly ConcurrentQueue<ServiceStatus> statusUpdates;
        private readonly JsonSerializer serializer;
        private readonly DeviceWatcher watcher;
        private readonly ICoreFeatures coreFeatures;
        private readonly ServiceAdapter service = new ServiceAdapter();

        private bool isRunning;
        private ServiceJob currentJob;
        private string jobRefreshToken;

        public DeviceService()
        {
            statusUpdates = new ConcurrentQueue<ServiceStatus>();
            serializer = new JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };

            watcher = new DeviceWatcher
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            watcher.onAttach += DeviceWatcher_OnAttach;
            watcher.onDetach += DeviceWatcher_OnDetach;
            watcher.Watch();
            coreFeatures = new CoreFeatures(watcher, this);
        }

        public Task Start()
        {
            isRunning = true;

            Task[] tasks =
            {
                Task.Factory.StartNew(GetJobBackgroundThread, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(PostStatusBackgroundThread, TaskCreationOptions.LongRunning)
            };

            return new Task(() =>
            {
                Task.WaitAll(tasks);
            });
        }

        private void DeviceWatcher_OnDetach(DeviceId deviceInfo)
        {
            var status = new DeviceAttachServiceStatus
            {
                Device = deviceInfo,
                DeviceId = deviceInfo.HardwareId,
                IsAttached = false,
                Time = DateTime.UtcNow
            };
            statusUpdates.Enqueue(status);
        }

        private void DeviceWatcher_OnAttach(DeviceId deviceInfo)
        {
            var status = new DeviceAttachServiceStatus
            {
                Device = deviceInfo,
                DeviceId = deviceInfo.HardwareId,
                IsAttached = true,
                Time = DateTime.UtcNow
            };
            statusUpdates.Enqueue(status);
        }

        private void PostStatusBackgroundThread()
        {
            while (isRunning)
            {
                try
                {
                    PostStatus();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }

                Thread.Sleep(1000);
            }
        }

        private void PostStatus()
        {
            List<ServiceStatus> status;

            while ((status = GetStatusBatch()).Count > 0)
            {
                try
                {
                    PostBatch(status);
                }
                catch
                {
                    foreach (var item in status)
                    {
                        statusUpdates.Enqueue(item);
                    }
                }
            }
        }

        private void PostBatch(List<ServiceStatus> status)
        {
            service.Request("api/service/status", request =>
            {
                request.Method = "POST";
                request.ContentType = "application/json";

                using (var stream = request.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, status);
                }

                using (request.GetResponse())
                {
                }
            });
        }

        private List<ServiceStatus> GetStatusBatch()
        {
            List<ServiceStatus> statuses = new List<ServiceStatus>();
            ServiceStatus item;

            for (var i = 0; i < 5 && statusUpdates.TryDequeue(out item); i++)
            {
                statuses.Add(item);
            }

            return statuses;
        }

        private void GetJobBackgroundThread()
        {
            while (isRunning)
            {
                try
                {
                    GetJob();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }

                Thread.Sleep(500);
            }
        }

        private void GetJob()
        {
            service.Request("api/service/job?timeout=20&refreshToken=" + jobRefreshToken, request =>
            {
                request.Method = "GET";

                using (HttpWebResponse response = GetHttpResponse(request))
                {
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        Logger.Debug("Job request returned 304 unmodified");
                    }

                    using (Stream responseStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream))
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        var jobRefresh = serializer.Deserialize<ServiceJobRefresh>(jsonReader);

                        if (jobRefresh != null)
                        {
                            jobRefreshToken = jobRefresh.RefreshToken;

                            ExecuteJob(jobRefresh.Job);
                        }
                    }
                }
            });
        }

        private static HttpWebResponse GetHttpResponse(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;

                if (response != null && (int)response.StatusCode < 400)
                {
                    return response;
                }

                throw;
            }
        }

        private void ExecuteJob(ServiceJob job)
        {
            lock (jobLock)
            {
                if (job != null && job.Equals(currentJob))
                {
                    return;
                }
                currentJob = job;
            }

            if (job == null)
            {
                Logger.Info("Received null job");
                return;
            }

            Logger.Info("Started job: " + job.Id);

            if (job is RunAppServiceJob)
            {
                RunApp((RunAppServiceJob)job);
            }
            else if (job is RunStressTestServiceJob)
            {
                RunStressTest((RunStressTestServiceJob)job);
            }
            else if (job is RunUrlServiceJob)
            {
                RunOpenUrl((RunUrlServiceJob)job);
            }
            else
            {
                Logger.Info("Skipping unimplemented job type {0}. If intentional, update service to explicitly ignore job type." + job.GetType().FullName);
            }

            Logger.Info("Done job: " + job.Id);
        }

        private void RunApp(RunAppServiceJob job)
        {
            coreFeatures.LaunchApk(job);
        }

        private void RunStressTest(RunStressTestServiceJob job)
        {
            coreFeatures.LaunchStressTest(job);
        }

        private void RunOpenUrl(RunUrlServiceJob job)
        {
            coreFeatures.LaunchUrl(job);
        }

        public void Dispose()
        {
            watcher.Dispose();
            isRunning = false;
        }

        public void ReportStatus(ServiceStatus status)
        {
            statusUpdates.Enqueue(status);
        }
    }
}
