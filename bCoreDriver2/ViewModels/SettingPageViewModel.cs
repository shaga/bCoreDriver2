
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bCoreDriver2.Models;
using BcoreLib;
using Prism.Mvvm;

namespace bCoreDriver2.ViewModels
{
    internal class SettingPageViewModel : BindableBase
    {

        #region static property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.CoreWindow.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        #endregion
        #region field

        private BcoreController _controller;

        #endregion

        #region property

        public BcoreController Controller
        {
            get => _controller;
            set => SetProperty(ref _controller, value);
        }

        #endregion

        #region method

        public void Init(BcoreController controller)
        {
            Controller = controller;
            Controller.ConnectionChanged += OnConnectionStatusChanged;
            Controller.UpdateVisibility();
        }

        public void Final()
        {
            Controller.SaveSetting();
            Controller.ConnectionChanged -= OnConnectionStatusChanged;
        }

        private async void OnConnectionStatusChanged(object sender, BcoreConnectionStatusChangedEventArgs e)
        {
            if (e.Status == BcoreConnectionStatus.Disconnected)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    AppFrame?.GoBack();
                });
            }
        }

        #endregion
    }
}
