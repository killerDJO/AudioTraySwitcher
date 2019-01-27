using Squirrel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioTraySwitcher
{
    internal class Updater
    {
        public void CheckForUpdates()
        {
            Task.Run(async () =>
            {
                string updateUrl = ConfigurationManager.AppSettings["updateUrl"];
                using (var updateManager = await UpdateManager.GitHubUpdateManager(updateUrl))
                {
                    var releaseEntry = await updateManager.UpdateApp();

                    if (releaseEntry != default(ReleaseEntry))
                    {
                        var result = MessageBox.Show("New version is available. Restart now?", "Update downloaded", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes)
                        {
                            UpdateManager.RestartApp($"\"{Path.GetFileName(Assembly.GetEntryAssembly().Location)}\"");
                        }
                    }
                }
            });
        }
    }
}
