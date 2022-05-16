using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Power;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bCoreDriver2.Models.Settings;
using bCoreDriver2.Views.CustomViews;
using BcoreLib;
using Prism.Commands;
using Prism.Mvvm;

namespace bCoreDriver2.Models
{
    internal class BcoreController : BindableBase, IDisposable
    {
        #region const

        private const int MinUpdateSpanMillisecond = 30;

        #endregion

        #region static property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        #endregion

        #region field

        private BcoreManager _manager;

        private BcoreFunctionInfo _functionInfo;

        private BcoreSetting _setting;

        private bool _isConnectiong = true;

        private int _batteryVoltage = 0;

        private DateTime[] _updateMotor = new[]
        {
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
        };

        private DateTime[] _updateServo = new[]
        {
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
        };

        private int[] _motorValue = new[]
        {
            Bcore.StopMotorPwm, Bcore.StopMotorPwm, Bcore.StopMotorPwm, Bcore.StopMotorPwm,
        };

        private int[] _servoValue = new[]
        {
            Bcore.CenterServoPos, Bcore.CenterServoPos, Bcore.CenterServoPos, Bcore.CenterServoPos,
        };

        private bool[] _portOutValue = new[]
        {
            false, false, false, false
        };

        private DelegateCommand<BcoreSlierUpdateParam> _updateCommand;

        #endregion

        #region property

        public bool IsConnecting
        {
            get => _isConnectiong;
            set => SetProperty(ref _isConnectiong, value);
        }

        public bool IsConnected => !_isConnectiong && (_manager?.IsInitialized ?? false);

        public string DeviceName
        {
            get => _setting?.DisplayName ?? "";
            set
            {
                _setting.DisplayName = value;
                RaisePropertyChanged();
            }
        }

        public int BatteryVoltage
        {
            get => _batteryVoltage;
            set
            {
                SetProperty(ref _batteryVoltage, value);
                RaisePropertyChanged(nameof(BatteryVoltageStr));
            }
        }

        public string BatteryVoltageStr => $"Battery: {_batteryVoltage / 1000:F2} [V]";

        public ICommand UpdateCommand => _updateCommand ?? (_updateCommand = new DelegateCommand<BcoreSlierUpdateParam>(OnUpdatedSlider));

        #region motor

        public int MotorValue1
        {
            get => _motorValue[0];
            set => MotorValue(value);
        }

        public int MotorValue2
        {
            get => _motorValue[1];
            set => MotorValue(value);
        }

        public int MotorValue3
        {
            get => _motorValue[2];
            set => MotorValue(value);
        }

        public int MotorValue4
        {
            get => _motorValue[3];
            set => MotorValue(value);
        }

        public bool IsVisibleMotor1 => IsVisibleMotor();
        public bool IsVisibleMotor2 => IsVisibleMotor();
        public bool IsVisibleMotor3 => IsVisibleMotor();
        public bool IsVisibleMotor4 => IsVisibleMotor();

        public bool IsVisibleSettingMotor1 => IsVisibleSettingMotor();
        public bool IsVisibleSettingMotor2 => IsVisibleSettingMotor();
        public bool IsVisibleSettingMotor3 => IsVisibleSettingMotor();
        public bool IsVisibleSettingMotor4 => IsVisibleSettingMotor();


        public MotorSetting MotorSetting1 => _setting?.MotorSettings[0];
        public MotorSetting MotorSetting2 => _setting?.MotorSettings[1];
        public MotorSetting MotorSetting3 => _setting?.MotorSettings[2];
        public MotorSetting MotorSetting4 => _setting?.MotorSettings[3];

        #endregion

        #region servo

        public int ServoValue1
        {
            get => _servoValue[0];
            set => ServoValue(value);
        }

        public int ServoValue2
        {
            get => _servoValue[1];
            set => ServoValue(value);
        }

        public int ServoValue3
        {
            get => _servoValue[2];
            set => ServoValue(value);
        }

        public int ServoValue4
        {
            get => _servoValue[3];
            set => ServoValue(value);
        }

        public bool IsVisibleServo1 => IsVisibleServo();
        public bool IsVisibleServo2 => IsVisibleServo();
        public bool IsVisibleServo3 => IsVisibleServo();
        public bool IsVisibleServo4 => IsVisibleServo();

        public bool IsVisibleSettingServo1 => IsVisibleSettingServo();
        public bool IsVisibleSettingServo2 => IsVisibleSettingServo();
        public bool IsVisibleSettingServo3 => IsVisibleSettingServo();
        public bool IsVisibleSettingServo4 => IsVisibleSettingServo();

        public ServoSetting ServoSetting1 => _setting?.ServoSettings[0];
        public ServoSetting ServoSetting2 => _setting?.ServoSettings[1];
        public ServoSetting ServoSetting3 => _setting?.ServoSettings[2];
        public ServoSetting ServoSetting4 => _setting?.ServoSettings[3];

        #endregion

        #region PortOut

        public bool PortOutCheck1
        {
            get => _portOutValue[0];
            set => PortOutCheck(value);
        }

        public bool PortOutCheck2
        {
            get => _portOutValue[1];
            set => PortOutCheck(value);
        }

        public bool PortOutCheck3
        {
            get => _portOutValue[2];
            set => PortOutCheck(value);
        }

        public bool PortOutCheck4
        {
            get => _portOutValue[3];
            set => PortOutCheck(value);
        }

        public bool IsVisiblePortOut1 => IsVisiblePortOut();
        public bool IsVisiblePortOut2 => IsVisiblePortOut();
        public bool IsVisiblePortOut3 => IsVisiblePortOut();
        public bool IsVisiblePortOut4 => IsVisiblePortOut();

        public bool IsVisibleSettingPortOut1 => IsVisibleSettingPortOut();
        public bool IsVisibleSettingPortOut2 => IsVisibleSettingPortOut();
        public bool IsVisibleSettingPortOut3 => IsVisibleSettingPortOut();
        public bool IsVisibleSettingPortOut4 => IsVisibleSettingPortOut();

        public PortOutSetting PortOutSetting1 => _setting?.PortOutSettings[0];

        public PortOutSetting PoutOutSetting2 => _setting?.PortOutSettings[1];

        public PortOutSetting PoutOutSetting3 => _setting?.PortOutSettings[2];

        public PortOutSetting PoutOutSetting4 => _setting?.PortOutSettings[3];

        #endregion

        #endregion

        #region event

        public event EventHandler<BcoreConnectionStatusChangedEventArgs> ConnectionChanged;

        #endregion

        #region method

        public void Dispose()
        {
            Finish();
        }

        public async Task Init(string deviceId)
        {
            _manager = new BcoreManager(deviceId);
            _manager.ConnectionStatusChanged += OnConnectionStatusChanged;
            await _manager.Init();

            _setting = await BcoreSetting.Load(_manager.DeviceName);
        }

        public void Finish()
        {
            if (_manager != null)
            {
                _manager.ConnectionStatusChanged -= OnConnectionStatusChanged;
                _manager.Dispose();
            }
            _manager = null;
        }

        public async void UpdateBatteryVoltage()
        {
            if (_manager == null) return;

            var value = await _manager.ReadBattery();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => BatteryVoltage = value);
        }

        public async void UpdateVisibility()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RaisePropertyChanged(nameof(DeviceName));

                RaisePropertyChanged(nameof(IsVisibleMotor1));
                RaisePropertyChanged(nameof(IsVisibleMotor2));
                RaisePropertyChanged(nameof(IsVisibleMotor3));
                RaisePropertyChanged(nameof(IsVisibleMotor4));

                RaisePropertyChanged(nameof(IsVisibleServo1));
                RaisePropertyChanged(nameof(IsVisibleServo2));
                RaisePropertyChanged(nameof(IsVisibleServo3));
                RaisePropertyChanged(nameof(IsVisibleServo4));

                RaisePropertyChanged(nameof(IsVisiblePortOut1));
                RaisePropertyChanged(nameof(IsVisiblePortOut2));
                RaisePropertyChanged(nameof(IsVisiblePortOut3));
                RaisePropertyChanged(nameof(IsVisiblePortOut4));

                RaisePropertyChanged(nameof(IsVisibleSettingMotor1));
                RaisePropertyChanged(nameof(IsVisibleSettingMotor2));
                RaisePropertyChanged(nameof(IsVisibleSettingMotor3));
                RaisePropertyChanged(nameof(IsVisibleSettingMotor4));

                RaisePropertyChanged(nameof(IsVisibleSettingServo1));
                RaisePropertyChanged(nameof(IsVisibleSettingServo2));
                RaisePropertyChanged(nameof(IsVisibleSettingServo3));
                RaisePropertyChanged(nameof(IsVisibleSettingServo4));

                RaisePropertyChanged(nameof(IsVisiblePortOut1));
                RaisePropertyChanged(nameof(IsVisiblePortOut2));
                RaisePropertyChanged(nameof(IsVisiblePortOut3));
                RaisePropertyChanged(nameof(IsVisiblePortOut4));
            });
        }

        public async void SaveSetting()
        {
            await BcoreSetting.Save(_setting);
        }

        private async void OnConnectionStatusChanged(object sender, BcoreConnectionStatusChangedEventArgs e)
        {
            if (e.Status == BcoreConnectionStatus.Connected)
            {
                _functionInfo = await _manager.ReadFunctionInfo();

                UpdateBatteryVoltage();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsConnecting = false;

                    UpdateVisibility();
                });
            }

            ConnectionChanged?.Invoke(this, e);
        }

        private int GetPortNum(string name, [CallerMemberName] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(prefix) || !name.StartsWith(prefix)) return 0;

            var numStr = name.Replace(prefix, "");

            if (int.TryParse(numStr, out var value))
            {
                return value;
            }

            return 0;
        }

        private  void OnUpdatedSlider(BcoreSlierUpdateParam param)
        {
            if (_manager == null) return;

            if (param.SlierType == ESliderType.Motor)
            {
                WriteMotorValue(param.Index, true);
            }
            else if (param.SlierType == ESliderType.Servo && _functionInfo.IsEnableServoPort(param.Index))
            {
                WriteServoValue(param.Index, true);
            }
        }

        #region motor

        public void WriteMotorValue(int index, bool isForce = false)
        {
            if (_functionInfo == null || !_functionInfo.IsEnableMotorPort(index)) return;

            var now = DateTime.Now;
            var span = (now - _updateMotor[index]).TotalMilliseconds;

            if (!isForce && span < MinUpdateSpanMillisecond)
            {
                return;
            }

            _updateMotor[index] = now;

            _manager?.WriteMotorPwm(index, _motorValue[index], _setting.MotorSettings[index].IsFlip);
        }

        private void MotorValue(int value, [CallerMemberName] string name = null)
        {
            var index = GetPortNum(name) - 1;

            if (_functionInfo == null || !_functionInfo.IsEnableMotorPort(index))
            {
                return;
            }

            _motorValue[index] = value;
            RaisePropertyChanged(name);

            WriteMotorValue(index);
        }

        private bool IsVisibleMotor([CallerMemberName] string name = null)
        {
            if (IsConnecting || _functionInfo == null || _setting == null)
            {
                return false;
            }

            var index = GetPortNum(name) - 1;

            if (index < 0 || _setting.MotorSettings.Count <= index) return false;

            return _functionInfo.IsEnableMotorPort(index) && _setting.MotorSettings[index].IsShow;
        }

        private bool IsVisibleSettingMotor([CallerMemberName] string name = null)
        {
            if (_functionInfo == null) return false;

            var index = GetPortNum(name) - 1;

            return _functionInfo.IsEnableMotorPort(index);
        }

        #endregion

        #region servo

        public void WriteServoValue(int index, bool isForce = false)
        {
            if (_functionInfo == null || !_functionInfo.IsEnableServoPort(index)) return;

            var now = DateTime.Now;
            var span = (now - _updateServo[index]).TotalMilliseconds;

            if (!isForce && span < MinUpdateSpanMillisecond)
            {
                return;
            }

            _updateServo[index] = now;

            _manager?.WriteServoPos(index, _servoValue[index], _setting.ServoSettings[index].IsFlip, _setting.ServoSettings[index].Trim);
        }

        private void ServoValue(int value, [CallerMemberName] string name = null)
        {
            var index = GetPortNum(name) - 1;

            if (_functionInfo == null || !_functionInfo.IsEnableServoPort(index))
            {
                return;
            }

            _servoValue[index] = value;
            RaisePropertyChanged(name);

            WriteServoValue(index);
        }

        private bool IsVisibleServo([CallerMemberName] string name = null)
        {
            if (IsConnecting || _functionInfo == null || _setting == null)
            {
                return false;
            }

            var index = GetPortNum(name) - 1;

            if (index < 0 || _setting.ServoSettings.Count <= index) return false;

            return _functionInfo.IsEnableServoPort(index) && _setting.ServoSettings[index].IsShow;
        }

        public bool IsVisibleSettingServo([CallerMemberName] string name = null)
        {
            if (_functionInfo == null) return false;

            var index = GetPortNum(name) - 1;

            return _functionInfo.IsEnableServoPort(index);
        }

        #endregion

        #region port out

        private void PortOutCheck(bool value, [CallerMemberName] string name = null)
        {
            var index = GetPortNum(name) - 1;

            if (_functionInfo == null || !_functionInfo.IsEnablePortOut(index))
            {
                return;
            }

            _portOutValue[index] = value;
            RaisePropertyChanged(name);

            _manager?.WritePortOut(index, _portOutValue[index]);
        }

        private bool IsVisiblePortOut([CallerMemberName] string name = null)
        {
            if (IsConnecting || _functionInfo == null || _setting == null) return false;

            var index = GetPortNum(name) - 1;

            return _functionInfo.IsEnablePortOut(index) && _setting.PortOutSettings[index].IsShow;
        }

        private bool IsVisibleSettingPortOut([CallerMemberName] string name = null)
        {
            if (_functionInfo == null) return false;

            var index = GetPortNum(name) - 1;

            return _functionInfo.IsEnablePortOut(index);
        }

        #endregion

        #endregion
    }
}
