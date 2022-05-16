using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bCoreDriver2.Models;
using bCoreDriver2.Views;
using bCoreDriver2.Views.CustomViews;
using BcoreLib;
using Prism.Commands;
using Prism.Mvvm;

namespace bCoreDriver2.ViewModels
{
    internal class ControlPageViewModel : BindableBase
    {
        #region const



        #endregion

        #region static property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.CoreWindow.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        #endregion

        #region field

        private BcoreManager _manager;

        private BcoreController _controller;

        private bool _isConnecting = true;

        private DelegateCommand _moveSettingCommand;

        #endregion

        #region property

        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }

        public BcoreController Controller
        {
            get => _controller;
            set => SetProperty(ref _controller, value);
        }

        public ICommand MoveSettingCommand => _moveSettingCommand ?? (_moveSettingCommand = new DelegateCommand(MoveSetting));

        #endregion

        #region constructor

        public ControlPageViewModel()
        {
            _controller = new BcoreController();
        }

        #endregion

        #region method

        public async Task Init(string deviceId)
        {
            Final();

            _controller.ConnectionChanged += OnBcoreConnectionStatusChanged;
            await _controller.Init(deviceId);
        }

        public void Final(bool toDispose = false)
        {
            _controller.ConnectionChanged -= OnBcoreConnectionStatusChanged;
            if (toDispose)
            {
                _controller?.Finish();
                GC.Collect();
            }
        }

        public void UpdateVisibility()
        {
            _controller.ConnectionChanged += OnBcoreConnectionStatusChanged;
            _controller.UpdateVisibility();
        }

        private async void OnBcoreConnectionStatusChanged(object sender, BcoreConnectionStatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                switch (e.Status)
                {
                    case BcoreConnectionStatus.Connected:
                        IsConnecting = false;
                        break;
                    case BcoreConnectionStatus.Disconnected:
                        if (AppFrame.CanGoBack)
                        {
                            AppFrame.GoBack();
                        }
                        break;
                }
            });
        }

        private void MoveSetting()
        {
            AppFrame.Navigate(typeof(SettingPage), Controller);
        }

        #endregion
    }
}
