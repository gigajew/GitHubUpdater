using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace gitupdater
{
    class Program
    {
        static GitUpdater updater = new GitUpdater("gigajew/GitHubUpdater");
        static void Main(string[] args)
        {
            // subscribe to event to handle update as you want
            updater.UpdateInitiated += Updater_UpdateInitiated;

            // prompt update
            if (updater.IsNewVersionAvailable(Global.CurrentAppVersion))
            {
                Console.WriteLine("Current version is v{1}. A new version is a available v({0}). Would you like to update now (Y/n)?", updater.LatestVersion, Global.CurrentAppVersion);
          
                // read response
                var response = Console.ReadKey();
                Console.WriteLine();

                if(response.Key == ConsoleKey.Y )
                {
                    // perform update
                    updater.Update(Global.CurrentAppVersion);
                } else
                {
                    Console.WriteLine("Skipped update");
                }
                
            }

            // do program logic
            Console.WriteLine("Hello, world");
            Console.ReadLine();
        }

        private static void Updater_UpdateInitiated(byte[] asset)
        {
            var currentLocation = Assembly.GetEntryAssembly().Location;
            File.WriteAllBytes(currentLocation, asset);
            Process.Start(currentLocation);
            Environment.Exit(0);
        }
    }

    public class GitUpdater
    {

        public delegate void UpdateInitiatedDelegate(byte[] asset);
        public event UpdateInitiatedDelegate UpdateInitiated;

        public string Repository { get; protected set; }
        public Version LatestVersion { get; protected set; }
        public string AssetUrl { get; protected set; }

        private JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public GitUpdater(string repository)
        {
            this.Repository = repository;
        }

        public bool IsNewVersionAvailable(Version currentVersion)
        {
            dynamic versionInformation = DeserializeJson<dynamic>(GetVersionUri());
            this.LatestVersion = Version.Parse(versionInformation["tag_name"]);
            this.AssetUrl = versionInformation["assets_url"];
            return this.LatestVersion >= currentVersion;
        }

        public void Update(Version currentVersion)
        {
            if (!IsNewVersionAvailable(currentVersion))
                return;

            // proceed updating
            dynamic[] updateInformation = DeserializeJson<dynamic[]>(this.AssetUrl);
            Dictionary<string, dynamic> entry = updateInformation.First();
            string binaryLocation = entry["browser_download_url"];

            byte[] asset = DownloadAsset(binaryLocation);
            UpdateInitiated?.Invoke(asset);
        }

        private string GetVersionUri()
        {
            return string.Format("https://api.github.com/repos/{0}/releases/latest", Repository);
        }

        private void EnableSsl()
        {
            if (_sslEnabled) return;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            _sslEnabled = !_sslEnabled;
        }

        private T DeserializeJson<T>(string url)
        {
            EnableSsl();

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.UserAgent = USER_AGENT;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader responseStreamReader = new StreamReader(responseStream))
            {
                T deserializedData = _serializer.Deserialize<T>(responseStreamReader.ReadToEnd());
                return deserializedData;
            }
        }

        private byte[] DownloadAsset(string url)
        {
            EnableSsl();

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.UserAgent = USER_AGENT;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memory = new MemoryStream())
            {
                byte[] buffer = new byte[8192];
                int read = 0;
                while ((read = responseStream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    memory.Write(buffer, 0, read);
                }
                return memory.ToArray();
            }
        }

        private bool _sslEnabled;
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0";
    }

    public static class Global
    {
        public static Version CurrentAppVersion { get; private set; } = new Version(1, 4);
        public static string Repository { get; private set; } = "gigajew/GitHubUpdater";
    }
}
