using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellCalculations2010.Model
{
    [Serializable]
    public class EarthData : ICloneable
    {
        public EarthData()
        {
            earthHeight = 0;
            earthType = "";
        }
        public EarthData(double earthHeight, string earthType)
        {
            this.earthHeight = earthHeight;
            this.earthType = earthType;
        }

        public double earthHeight { get; set; }
        public string earthType { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
