using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellCalculations2010.Model
{
    [Serializable]
    public class GoldData : ICloneable
    {
        public GoldData()
        {
            goldContent = "0";
            goldHeight = 0;
        }
        public GoldData(double goldHeight, string goldContent)
        {
            this.goldContent = goldContent;
            this.goldHeight = goldHeight;
        }

        public string goldContent { get; set; }
        public double goldHeight { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
