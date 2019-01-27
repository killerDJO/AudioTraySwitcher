using Squirrel;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AudioTraySwitcher
{
    public partial class App
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: v => CreateShortcut(),
                onAppUpdate: v => CreateShortcut(),
                onAppUninstall: v => RemoveShortcut());
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.ToString(), "Error Occurred",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            Shutdown();
        }

        private void RunWithUpdateManager(Action<UpdateManager> action)
        {
            using (var manager = new UpdateManager(String.Empty))
            {
                action(manager);
            }
        }

        private void CreateShortcut()
        {
            RunWithUpdateManager(manager =>
            {
                manager.CreateShortcutsForExecutable(
                    GetExeFileName(),
                    GetShortcutLocations(),
                    !Environment.CommandLine.Contains("squirrel-install"));
            });
        }

        private void RemoveShortcut()
        {
            RunWithUpdateManager(manager =>
            {
                manager.RemoveShortcutsForExecutable(GetExeFileName(), GetShortcutLocations());
            });
        }

        private string GetExeFileName()
        {
            return Path.GetFileName(Assembly.GetEntryAssembly().Location);
        }

        private ShortcutLocation GetShortcutLocations()
        {
            return ShortcutLocation.StartMenu | ShortcutLocation.Startup;
        }
    }
}
