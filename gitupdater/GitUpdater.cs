using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace gitupdater
{
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
                responseStream.CopyTo(memory);
                return memory.ToArray();
            }
        }

        private bool _sslEnabled;
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0";
    }
}
