using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AudioTraySwitcher
{
    public partial class MainWindow
    {
        private readonly CoreAudioController controller = new CoreAudioController();
        private readonly Updater updater = new Updater();
        private readonly NotifyIcon trayIcon;

        public MainWindow()
        {
            InitializeComponent();

            trayIcon = new NotifyIcon
            {
                Icon = new Icon("audio.ico"),
                ContextMenu = BuildContextMenu(),
                Text = "Switch Audio Device",
                Visible = true
            };

            trayIcon.Click += (sender, args) =>
            {
                if (args is MouseEventArgs mouseEventArgs && mouseEventArgs.Button == MouseButtons.Left)
                {
                    MethodInfo methodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo.Invoke(trayIcon, null);
                }
            };

            controller.AudioDeviceChanged.Subscribe(new DeviceChangeObserver(() =>
            {
                trayIcon.ContextMenu = BuildContextMenu();
            }));
        }

        private ContextMenu BuildContextMenu()
        {
            return new ContextMenu(BuildMenuItems().ToArray());
        }

        private void ShowAbout()
        {
            string aboutContent = $"Version: {Assembly.GetEntryAssembly().GetName().Version}";
            MessageBox.Show(aboutContent, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private IReadOnlyCollection<MenuItem> BuildMenuItems()
        {
            var result = new List<MenuItem>();
            var devices = GetAudioDevices();

            result.AddRange(CreateMenuItems(devices, DeviceType.Playback));

            result.Add(CreateSeparator());

            result.AddRange(CreateMenuItems(devices, DeviceType.Capture));

            result.Add(CreateSeparator());

            var moreMenuItem = new MenuItem("More");
            moreMenuItem.MenuItems.Add(new MenuItem("Check For Updates", (sender, args) => updater.CheckForUpdates()));
            moreMenuItem.MenuItems.Add(new MenuItem("About", (sender, args) => ShowAbout()));
            moreMenuItem.MenuItems.Add(CreateExitItem());
            result.Add(moreMenuItem);

            return result;
        }

        private MenuItem CreateSeparator()
        {
            return new MenuItem("-");
        }

        private MenuItem CreateExitItem()
        {
            return new MenuItem("Exit", (sender, args) =>
            {
                trayIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            });
        }

        private IReadOnlyCollection<MenuItem> CreateMenuItems(IReadOnlyCollection<CoreAudioDevice> devices, DeviceType deviceType)
        {
            return devices
                .Where(device => device.DeviceType == deviceType)
                .Select(CreateMenuItem)
                .ToList();
        }

        private MenuItem CreateMenuItem(CoreAudioDevice device)
        {
            return new MenuItem(device.FullName, (sender, args) => SetDeviceAsDefault(device))
            {
                Checked = device.IsDefaultDevice
            };
        }

        private void SetDeviceAsDefault(CoreAudioDevice device)
        {
            device.SetAsDefaultCommunications();
            device.SetAsDefault();
        }

        private IReadOnlyCollection<CoreAudioDevice> GetAudioDevices()
        {
            return controller.GetDevices(DeviceType.All, DeviceState.Active).ToList();
        }

        private class DeviceChangeObserver : IObserver<DeviceChangedArgs>
        {
            private readonly Action onDeviceChanged;

            public DeviceChangeObserver(Action onDeviceChanged)
            {
                this.onDeviceChanged = onDeviceChanged;
            }

            public void OnNext(DeviceChangedArgs value)
            {
                onDeviceChanged();
            }

            public void OnError(Exception error)
            {
            }

            public void OnCompleted()
            {
            }
        }
    }
}
