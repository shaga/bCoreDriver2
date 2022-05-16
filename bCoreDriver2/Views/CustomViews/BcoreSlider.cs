using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using BcoreLib;
using Prism.Commands;

namespace bCoreDriver2.Views.CustomViews
{
    public enum ESliderType
    {
        Motor,
        Servo,
    }

    public class BcoreSlierUpdateParam
    {
        public ESliderType SlierType { get; }

        public int Index { get; }

        public int Value { get; }

        public BcoreSlierUpdateParam(ESliderType sliderType, int index, int value)
        {
            SlierType = sliderType;
            Index = index;
            Value = value;
        }
    }

    internal class BcoreSlider : Slider
    {
        #region property

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(nameof(Index), typeof(int), typeof(BcoreSlider), new PropertyMetadata(0));

        public int Index
        {
            get => (int) GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly DependencyProperty IsAutoResetProperty = DependencyProperty.Register(nameof(IsAutoReset), typeof(bool), typeof(BcoreSlider), new PropertyMetadata(false));

        public bool IsAutoReset
        {
            get => (bool) GetValue(IsAutoResetProperty);
            set => SetValue(IsAutoResetProperty, value);
        }

        public static readonly DependencyProperty SliderTypeProperty = DependencyProperty.Register(nameof(SliderType), typeof(ESliderType), typeof(BcoreSlider), new PropertyMetadata(ESliderType.Motor));

        public ESliderType SliderType
        {
            get => (ESliderType) GetValue(SliderTypeProperty);
            set => SetValue(SliderTypeProperty, value);
        }

        public static readonly DependencyProperty UpdateCommandProperty = DependencyProperty.Register(nameof(UpdateCommand), typeof(DelegateCommand<BcoreSlierUpdateParam>), typeof(BcoreSlider), new PropertyMetadata(null));

        public DelegateCommand<BcoreSlierUpdateParam> UpdateCommand
        {
            get => GetValue(UpdateCommandProperty) as DelegateCommand<BcoreSlierUpdateParam>;
            set => SetValue(UpdateCommandProperty, value);
        }

        #endregion

        #region constructor

        public BcoreSlider() : base()
        {
            PointerCaptureLost += OnPointerCaptureLost;
        }

        #endregion

        #region method

        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (IsAutoReset) Value = SliderType == ESliderType.Motor ? Bcore.StopMotorPwm : Bcore.CenterServoPos;

            UpdateCommand?.Execute(new BcoreSlierUpdateParam(SliderType, Index, (int)Value));
        }

        #endregion
    }
}
