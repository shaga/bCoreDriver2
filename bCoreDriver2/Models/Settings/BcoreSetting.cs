using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BcoreLib;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace bCoreDriver2.Models.Settings
{
    [JsonObject]
    internal class BcoreSetting : BindableBase
    {
        #region const

        private const string FileExt = ".json";

        #endregion

        #region field

        private string _deviceName;

        private string _displayName;

        #endregion

        #region static property

        [JsonIgnore]
        private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

        #endregion

        #region property

        [JsonIgnore]
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }

        [JsonProperty]
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        [JsonProperty]
        public IList<MotorSetting> MotorSettings { get; }

        [JsonProperty]
        public IList<ServoSetting> ServoSettings { get; }

        [JsonProperty]
        public IList<PortOutSetting> PortOutSettings { get; }

        #endregion

        #region constructor

        [JsonConstructor]
        public BcoreSetting()
        {
            MotorSettings = new List<MotorSetting>();
            ServoSettings = new List<ServoSetting>();
            PortOutSettings = new List<PortOutSetting>();
        }

        public BcoreSetting(string deviceName) : this()
        {
            DeviceName = deviceName;
            DisplayName = deviceName;

            for (var i = 0; i < Bcore.MaxFunctionCount; i++)
            {
                MotorSettings.Add(new MotorSetting());
                ServoSettings.Add(new ServoSetting());
                PortOutSettings.Add(new PortOutSetting());
            }
        }

        #endregion

        #region static method

        public static async Task<BcoreSetting> Load(string deviceName)
        {
            BcoreSetting setting = null;

            try
            {
                var file = await LocalFolder.GetFileAsync(deviceName + FileExt);
                var jsonText = await FileIO.ReadTextAsync(file);
                setting = JsonConvert.DeserializeObject<BcoreSetting>(jsonText);
                setting.DeviceName = deviceName;
            }
            catch 
            {
                setting = new BcoreSetting(deviceName);
            }

            return setting;
        }

        public static async Task<bool> Save(BcoreSetting setting)
        {
            try
            {
                var jsonText = JsonConvert.SerializeObject(setting);
                var file = await LocalFolder.CreateFileAsync(setting.DeviceName + FileExt, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, jsonText);
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
