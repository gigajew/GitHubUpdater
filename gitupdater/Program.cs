using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace gitupdater
{
    class Program
    {
        static GitUpdater updater = new GitUpdater("gigajew/GitHubUpdater");
        static void Main(string[] args)
        {
            if (updater.IsNewVersionAvailable(Global.CurrentAppVersion))
            {
                updater.Update(Global.CurrentAppVersion );
            }
        }
    }

    public class GitUpdater
    {
        public string Repository { get; protected set; }
        public Version LatestVersion { get; protected set; }

        private JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public GitUpdater(string repository)
        {
            this.Repository = repository;
        }

        public bool IsNewVersionAvailable(Version currentVersion)
        {
            HttpWebRequest request = WebRequest.Create(GetVersionUri()) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader responseStreamReader = new StreamReader(responseStream))
            {
                dynamic versionInformation = _serializer.Deserialize<dynamic>(responseStreamReader.ReadToEnd());
                this.LatestVersion = Version.Parse(versionInformation["tagName"]);
                return this.LatestVersion >= currentVersion;
            }

        }

        public void Update(Version currentVersion)
        {
            if (!IsNewVersionAvailable(currentVersion))
                return;

            // proceed updating
        }

        private string GetVersionUri()
        {
            return string.Format("https://api.github.com/repos/{0}/releases/all", Repository);
        }
    }

    public static class Global
    {
        public static Version CurrentAppVersion { get; private set; } = new Version(1, 0);
        public static string Repository { get; private set; } = "gigajew/GitHubUpdater";
    }
}
