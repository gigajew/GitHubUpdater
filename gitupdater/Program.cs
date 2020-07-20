using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace gitupdater
{
    class Program
    {
        static GitUpdater updater = new GitUpdater(Global.Repository );
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

                    // stop execution
                    return;
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
            var archiveLocation = Assembly.GetEntryAssembly().Location + ".old";
            File.Move(currentLocation, archiveLocation);
            File.WriteAllBytes(currentLocation, asset);

            Application.Restart();
        }
    }
}
