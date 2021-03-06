# GitHubUpdater
Update your app using GitHub releases

# Donate
Donations: bitcoin:12FP1JisjYCsgfteTLMQQMLnVBs65wZD8G

# Usage 

Push updates in the /releases section of your repository and let users update the app at their conveniance of their desktop. Add a tag to each release specifying version number and make sure you update the version number in your app.

Describe the current app version and repository name of your app
```c#
	// describe your current app version etc
    public static class Global
    {
		// current version
        public static Version CurrentAppVersion { get; private set; } = new Version(1, 4);
		
		// repository name
        public static string Repository { get; private set; } = "gigajew/GitHubUpdater";
    }
```

Instantiate the updater class
```c#
	// create a new instance of the updater
	var GitUpdater updater = new GitUpdater("gigajew/GitHubUpdater");
```

Subscribe to the UpdateInitiated event that provides your release artifact in a byte-array format
```c#
	// subscribe to event to handle update as you want
	updater.UpdateInitiated += (rawArtifactData) => {
		var currentLocation = Assembly.GetEntryAssembly().Location;
		var archiveLocation = Assembly.GetEntryAssembly().Location + ".old";
		File.Move(currentLocation, archiveLocation);
		File.WriteAllBytes(currentLocation, asset);
	};
```

Check if a new update is available
```c#
	// prompt update
	if (updater.IsNewVersionAvailable(Global.CurrentAppVersion))
	{
	}
```

Update your application
```c#
	// update the application from the current version (statically described in your application)
	if (updater.IsNewVersionAvailable(Global.CurrentAppVersion))
	{
		updater.Update(Global.CurrentAppVersion); // handle the update artifact as you will with the event you subscribed to
	}
```
