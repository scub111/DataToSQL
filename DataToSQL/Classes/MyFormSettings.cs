using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DataToSQL
{
    class MyProjectSettings : ApplicationSettingsBase
    {
        [DefaultSettingValue("5000")]
        public int MainThreadPeriod
        {
            get
            {
                return ((int)this["MainThreadPeriod"]);
            }
            set
            {
                this["MainThreadPeriod"] = value;
            }
        }
    }
}
