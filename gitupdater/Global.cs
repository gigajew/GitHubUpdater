using System;

namespace gitupdater
{
    public static class Global
    {
        public static Version CurrentAppVersion { get; private set; } = new Version(1, 5);
        public static string Repository { get; private set; } = "gigajew/GitHubUpdater";
    }
}
