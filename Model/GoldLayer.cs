using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WellCalculations2010.Model
{
    [Serializable]
    public class GoldLayer : ICloneable
    {
        public GoldLayer()
        {
            goldContent = String.Empty;
            depth = 0.0d;
            thickness = 0.0d;
        }

        public GoldLayer(string goldContent, double depth, double thickness)
        {
            this.goldContent = goldContent;
            this.depth = depth;
            this.thickness = thickness;
        }

        public double depth { get; set; }
        public string goldContent { get; set; }
        public double thickness { get; set; }
        

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
