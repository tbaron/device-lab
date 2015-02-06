using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using InfoSpace.DeviceLab.Jobs;
using InfoSpace.DeviceLab.Web.Auth;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    [AccessTokenAuthorize]
    public class UploadApiController : ApiController
    {
        private readonly JobManager jobManager;

        public UploadApiController()
        {
            jobManager = JobManager.Instance;
        }

        // POST api/upload
        /// <summary>
        /// Submit one or more files via multipart form submission to process (up to 50 MB total).
        /// Puts it in the Content/uploads folder
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> PostAsync()
        {
            ValidateUploadInput();

            string uploadPath = GetUploadDirectory();
            var resultFileStreamProvider = await UploadFilesAsync(uploadPath);

            IEnumerable<string> pathsToApks = ProcessUploadedFiles(resultFileStreamProvider, uploadPath);

            foreach (var apkPath in pathsToApks)
            {
                jobManager.SetCurrentJob(new RunAppServiceJob
                {
                    ApkUrl = GetAbsolutePath(apkPath)
                });
            }

            return pathsToApks;
        }

        private string GetAbsolutePath(string apkPath)
        {
            return new UriBuilder(Request.RequestUri)
            {
                Scheme = "https",
                Path = "upload/apk",
                Query = "name=" + apkPath,
                Port = -1,
                Fragment = null,
            }.Uri.ToString();
        }

        private static string GetUploadDirectory()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/apks/");

            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }

            return path;
        }

        private void ValidateUploadInput()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!  Expecting multi-part mime files.");
                throw new HttpResponseException(response);
            }
        }

        private async Task<MultipartFileStreamProvider> UploadFilesAsync(string fullUploadPath)
        {
            var destinationFileStreamProvider = new MultipartFileStreamProvider(fullUploadPath);
            return await Request.Content.ReadAsMultipartAsync(destinationFileStreamProvider);
        }

        private IEnumerable<string> ProcessUploadedFiles(MultipartFileStreamProvider resultFileStreamProvider, string fullUploadPath)
        {
            var messages = new List<string>();
            foreach (var file in resultFileStreamProvider.FileData)
            {
                string sourceFilenameWithQuotes = file.Headers.ContentDisposition.FileName;
                string sourceFilename = sourceFilenameWithQuotes.Substring(1, sourceFilenameWithQuotes.Length - 2);

                string finalPath = MoveFilesToFinalDestination(sourceFilename, file.LocalFileName, fullUploadPath);

                messages.Add(finalPath);

            }
            return messages;
        }

        private string MoveFilesToFinalDestination(string originalSourceFilename, string intermediateFilepath, string destinationRootFolder)
        {
            var manifestProvider = new AndroidManifestProvider(intermediateFilepath);
            var packageInfo = manifestProvider.GetPackageInfo();

            var package = packageInfo.Id;
            var version = packageInfo.Version.ToString(); // could use packageInfo.VersionName
            Stream icon = packageInfo.Icon;

            var destinationFolder = MakeDestinationFolder(package, version, destinationRootFolder);
            var destinationFolderPath = GetServerFolderPath(destinationRootFolder, destinationFolder);
            MoveAPKToDestinationFolder(originalSourceFilename, intermediateFilepath, destinationFolderPath);
            string iconFilename = WriteIconStreamToDestinationFolder(icon, "icon.ico", destinationFolderPath);

            return String.Format("{0}/{1}", destinationFolder.Replace("\\", "/"), originalSourceFilename);
        }

        private string MakeDestinationFolder(string package, string version, string fullUploadPath)
        {
            var folderName = String.Format("{0}\\{1}", package, version);
            var folderPath = GetServerFolderPath(fullUploadPath, folderName);
            if (!File.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            RemoveExistingFilesFromDestinationFolder(folderPath);

            return folderName;
        }

        private static string GetServerFolderPath(string fullUploadPath, string folderName)
        {
            var folderPath = String.Format("{0}\\{1}", fullUploadPath, folderName);
            return folderPath;
        }

        private void RemoveExistingFilesFromDestinationFolder(string folderPath)
        {
            foreach (var filename in Directory.EnumerateFiles(folderPath))
            {
                File.Delete(filename);
            }
        }

        private void MoveAPKToDestinationFolder(string originalSourceFilename, string intermediateFilepath, string destinationFolderPath)
        {
            var destinationFilepath = String.Format("{0}\\{1}", destinationFolderPath, originalSourceFilename);
            File.Move(intermediateFilepath, destinationFilepath);
        }

        private string WriteIconStreamToDestinationFolder(Stream icon, string iconFilename, string destinationFolderPath)
        {
            if (icon == null || String.IsNullOrEmpty(iconFilename))
            {
                return null;
            }

            var destinationFilepath = String.Format("{0}\\{1}", destinationFolderPath, iconFilename);
            FileStream outputStream = new FileStream(destinationFilepath, FileMode.Create);
            using (var fileStream = File.Create(destinationFilepath))
            {
                icon.CopyTo(fileStream);
            }

            return destinationFilepath;
        }
    }
}
