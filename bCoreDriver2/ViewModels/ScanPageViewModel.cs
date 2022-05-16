using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bCoreDriver2.Models.Settings;
using bCoreDriver2.Views;
using BcoreLib;
using Prism.Commands;
using Prism.Mvvm;

namespace bCoreDriver2.ViewModels
{
    internal class ScanPageViewModel : BindableBase
    {
        #region inner class

        public class DeviceInfo
        {
            public string Name { get; }

            public string DeviceId { get; }


            public DeviceInfo(string deviceId, string name)
            {
                DeviceId = deviceId;
                Name = name;
            }
        }

        #endregion

        #region const

        private static readonly TimeSpan ScanTimeoutSpan = TimeSpan.FromSeconds(30);

        private const string ButtonTextStartScan = "Start Scan";

        private const string ButtonTextStopScan = "Stop Scan";

        #endregion

        #region static property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.CoreWindow.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        #endregion

        #region field

        private BcoreScanner _scanner;

        private bool _isScaning;

        private ObservableCollection<DeviceInfo> _foundDevices = new ObservableCollection<DeviceInfo>();

        private DeviceInfo _selectedDevice;

        private DispatcherTimer _scanTimeoutTimer = null;

        private DelegateCommand _scanCommand;

        #endregion

        #region property

        public bool IsScanning
        {
            get => _isScaning;
            set
            {
                SetProperty(ref _isScaning, value);
                RaisePropertyChanged(nameof(Button));
            }
        }

        public string ButtonText => IsScanning ? ButtonTextStopScan : ButtonTextStartScan;

        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                SetProperty(ref _selectedDevice, value);
                if (_selectedDevice == null) return;
                MotToControl();
            }
        }

        public ObservableCollection<DeviceInfo> FoundDevices => _foundDevices;

        #region command

        public ICommand ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand(OnClickScan));

        #endregion

        #endregion

        #region constructor

        public ScanPageViewModel()
        {
            _scanner = new BcoreScanner();
            _scanner.FoundDevice += OnFoundDevice;

            _scanTimeoutTimer = new DispatcherTimer()
            {
                Interval = ScanTimeoutSpan
            };
            _scanTimeoutTimer.Tick += (s, e) => StopScan();
        }

        #endregion

        #region method

        private async void OnFoundDevice(object sender, BcoreFoundEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                if (!FoundDevices.Any(d => d.DeviceId == e.Id))
                {
                    var setting = await BcoreSetting.Load(e.Name);
                    FoundDevices.Add(new DeviceInfo(e.Id, setting.DisplayName));
                }
            });
        }

        private void OnClickScan()
        {
            if (IsScanning)
            {
                StopScan();
            }
            else
            {
                StartScan();
            }
        }

        private void StartScan()
        {
            if (IsScanning) return;

            FoundDevices.Clear();
            IsScanning = true;
            _scanTimeoutTimer.Start();
            _scanner.Start();
        }

        private void StopScan()
        {
            if (!IsScanning) return;

            _scanner.Stop();
            if (_scanTimeoutTimer.IsEnabled) _scanTimeoutTimer.Stop();
            IsScanning = false;
        }

        private void MotToControl()
        {
            StopScan();

            AppFrame.Navigate(typeof(ControlPage), _selectedDevice.DeviceId);
        }

        #endregion
    }
}
