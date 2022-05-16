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
    internal class PortOutSetting : BindableBase
    {
        #region field

        private bool _isShow = true;

        #endregion

        #region property

        [JsonProperty]
        public bool IsShow
        {
            get => _isShow;
            set => SetProperty(ref _isShow, value);
        }

        #endregion
    }
}
