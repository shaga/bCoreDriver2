using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using bCoreDriver2.ViewModels;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace bCoreDriver2.Views
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ControlPage : Page
    {
        public ControlPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.SetAppBackButtonVisibility();

            if (!(DataContext is ControlPageViewModel viewModel) || !(e.Parameter is string deviceId))
            {
                var frame = Window.Current.Content as Frame;
                frame.GoBack();
                return;
            }

            if (e.NavigationMode == NavigationMode.Back)
            {
                // 設定ページから戻る
                if (!viewModel.Controller.IsConnected)
                {
                    var frame = Window.Current.Content as Frame;
                    frame?.GoBack();
                    return;
                }
            }
            else
            {
                // スキャンページから生成
                await viewModel.Init(deviceId);
            }

            viewModel.UpdateVisibility();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (!(DataContext is ControlPageViewModel viewModel))
            {
                return;
            }

            var isBack = e.NavigationMode == NavigationMode.Back;

            viewModel.Final(isBack);

            if (isBack && Window.Current.Content is Frame frame)
            {
                var size = frame.CacheSize;
                frame.CacheSize = 0;
                frame.CacheSize = size;
            }
        }
    }
}
