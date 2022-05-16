using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace bCoreDriver2.Models.Settings
{
    [JsonObject]
    internal class ServoSetting : BindableBase
    {
        #region field

        private bool _isShow = true;

        private bool _isFlip = false;

        private bool _isSync = false;

        private int _trim = 0;

        #endregion

        #region property

        [JsonProperty]
        public bool IsShow
        {
            get => _isShow;
            set => SetProperty(ref _isShow, value);
        }

        [JsonProperty]
        public bool IsFlip
        {
            get => _isFlip;
            set => SetProperty(ref _isFlip, value);
        }

        [JsonProperty]
        public int Trim
        {
            get => _trim;
            set => SetProperty(ref _trim, value);
        }
        #endregion
    }
}
