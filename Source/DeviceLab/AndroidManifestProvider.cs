using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AndroidXml;
using Ionic.Zip;

namespace InfoSpace.DeviceLab
{
    public class AndroidManifestProvider
    {
        private readonly static XNamespace androidNamespace = "http://schemas.android.com/apk/res/android";
        private readonly string filePath;

        public AndroidManifestProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public PackageInfo GetPackageInfo()
        {
            using (var stream = GetAndroidManifestStream())
            using (var reader = new AndroidXmlReader(stream))
            {
                return GetPackageInfo(reader);
            }
        }

        private Stream GetAndroidManifestStream()
        {
            MemoryStream stream = new MemoryStream();

            using (ZipFile file = new ZipFile(this.filePath))
            {
                ZipEntry entry = file["AndroidManifest.xml"];

                entry.Extract(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private Stream GetAndroidIconFile(string iconPath)
        {
            if (String.IsNullOrEmpty(iconPath))
            {
                return null;
            }

            MemoryStream stream = new MemoryStream();

            using (ZipFile file = new ZipFile(this.filePath))
            {
                ZipEntry entry = file[iconPath];

                entry.Extract(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }


        private PackageInfo GetPackageInfo(AndroidXmlReader reader)
        {
            try
            {
                XDocument document = XDocument.Load(reader);

                var info = new PackageInfo
                {
                    Id = GetPackageId(document),
                    Version = GetVersionCode(document),
                    VersionName = GetVersionName(document),
                    Label = GetLabel(document),
                    LaunchableActivity = GetLaunchableActivity(document)
                    //Icon = GetIcon(document)
                };

                return info;
            }
            catch
            {
                return null;
            }
        }

        private string GetLaunchableActivity(XDocument document)
        {
            var launchableActivity = document
                .Root
                .Element("application")
                .Elements("activity")
                .FirstOrDefault(activity => activity
                    .Elements("intent-filter")
                    .Elements("action")
                    .Select(a => a.Attribute(androidNamespace + "name").GetValueOrDefault())
                    .Any(a => a == "android.intent.action.MAIN")
                );

            string value = null;

            if (launchableActivity != null)
            {
                value = launchableActivity
                    .Attribute(androidNamespace + "name")
                    .GetValueOrDefault();
            }

            return ResolveStringReference(value);
        }

        private string GetLabel(XDocument document)
        {
            string value = document
                .Root
                .Element("application")
                .Attribute(androidNamespace + "label")
                .GetValueOrDefault();

            return ResolveStringReference(value);
        }

        private Stream GetIcon(XDocument document)
        {
            string iconPath = GetIconPath(document);
            return GetAndroidIconFile(iconPath);
        }

        private string GetIconPath(XDocument document)
        {
            string value = document
                .Root
                .Elements("application")
                .First()
                .Attribute(androidNamespace + "icon")
                .GetValueOrDefault();

            return ResolveStringReference(value);
        }

        private string GetPackageId(XDocument document)
        {
            string value = document
                .Root
                .Attribute("package")
                .GetValueOrDefault();

            return ResolveStringReference(value);
        }

        private int GetVersionCode(XDocument document)
        {
            return document
                .Root
                .Attribute(androidNamespace + "versionCode")
                .GetValueAsInt();
        }

        private string GetVersionName(XDocument document)
        {
            string value = document
                .Root
                .Attribute(androidNamespace + "versionName")
                .GetValueOrDefault();

            return ResolveStringReference(value);
        }

        private string ResolveStringReference(string value)
        {
            // TODO: parse string resource by id from resource.arsc in APK
            return value;
        }
    }
}
