using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellCalculations2010.Model
{
    internal class SiteData : ICloneable
    {
        public string site { get; set; }
        public DateTime year { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
