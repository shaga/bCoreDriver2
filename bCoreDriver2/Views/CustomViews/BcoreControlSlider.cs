using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using bCoreDriver2.Annotations;
using BcoreLib;
using Prism.Commands;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace bCoreDriver2.Views.CustomViews
{
    public sealed class BcoreControlSlider : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register(nameof(SliderValue),
            typeof(int), typeof(BcoreControlSlider), new PropertyMetadata(0));

        public int SliderValue
        {
            get => (int)GetValue(SliderValueProperty);
            set => SetValue(SliderValueProperty, value);
        }

        public static readonly DependencyProperty SliderTypeProperty = DependencyProperty.Register(nameof(SliderType),
            typeof(ESliderType), typeof(BcoreControlSlider), new PropertyMetadata(ESliderType.Motor, OnChangedSliderType));

        public ESliderType SliderType
        {
            get => (ESliderType) GetValue(SliderTypeProperty);
            set => SetValue(SliderTypeProperty, value);
        }

        private static void OnChangedSliderType(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is BcoreControlSlider controller)) return;
            controller.UpdatePortName();
            controller.UpdateResetButtonText();
            controller.UpdateSliderStyle();
            controller.UpdateAutoReset();
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(nameof(Index),
            typeof(int), typeof(BcoreControlSlider), new PropertyMetadata(0, OnChangedIndex));

        public int Index
        {
            get => (int) GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        private static void OnChangedIndex(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is BcoreControlSlider controller)) return;
            controller.UpdatePortName();
        }

        private static readonly DependencyProperty UpdateCommandProperty =
            DependencyProperty.Register(nameof(UpdateCommand), typeof(DelegateCommand<BcoreSlierUpdateParam>),
                typeof(BcoreControlSlider), new PropertyMetadata(null));

        public DelegateCommand<BcoreSlierUpdateParam> UpdateCommand
        {
            get => GetValue(UpdateCommandProperty) as DelegateCommand<BcoreSlierUpdateParam>;
            set => SetValue(UpdateCommandProperty, value);
        }

        private string _portName;

        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
                OnPropertyChanged();
            }
        }

        private string _resetButtonText;

        public string ResetButtonText
        {
            get => _resetButtonText;
            set
            {
                _resetButtonText = value;
                OnPropertyChanged();
            }
        }

        private Style _sliderStyle;

        public Style SliderStyle
        {
            get => _sliderStyle;
            set
            {
                _sliderStyle = value;
                OnPropertyChanged();
            }
        }

        private bool _isAutoReset;

        public bool IsAutoReset
        {
            get => _isAutoReset;
            set
            {
                _isAutoReset = value;
                OnPropertyChanged();
                if (value)
                {
                    Reset();
                }
            }
        }

        private DelegateCommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand =
            new DelegateCommand(Reset, () => IsEnabled && !IsAutoReset).ObservesProperty(() => IsAutoReset)
                .ObservesProperty(() => IsEnabled));

        public BcoreControlSlider()
        {
            this.DefaultStyleKey = typeof(BcoreControlSlider);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdatePortName()
        {
            PortName = $"{(SliderType == ESliderType.Motor ? "Mot" : "Svr")}{Index + 1}";
        }

        private void UpdateResetButtonText()
        {
            ResetButtonText = SliderType == ESliderType.Motor ? "STOP" : "CENTER";
        }

        private void UpdateSliderStyle()
        {
            var key = SliderType == ESliderType.Motor ? "MotorSliderStyle" : "ServoSliderStyle";

            SliderStyle = App.Current.Resources.ContainsKey(key) ? App.Current.Resources[key] as Style : null;
        }

        private void UpdateAutoReset()
        {
            IsAutoReset = SliderType == ESliderType.Motor;
        }

        private void Reset()
        {
            SliderValue = SliderType == ESliderType.Motor ? Bcore.StopMotorPwm : Bcore.CenterServoPos;
            UpdateCommand?.Execute(new BcoreSlierUpdateParam(SliderType, Index, SliderValue));
        }
    }
}
