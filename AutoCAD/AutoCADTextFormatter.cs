using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WellCalculations2010.Model;

namespace WellCalculations2010.AutoCAD
{
    internal class AutoCADTextFormatter
    {
        public static string ApplyAutoCADFont(string text)
        {

            string fontChange = "{\\fTimes new Roman|b0|i0|c204|p18;";
            return fontChange + text + "}";
        }

        public static void FormatSectionStrings(Section section)
        {
            foreach (Well well in section.Wells)
            {
                well.SoftEarthThickness = well.SoftEarthThickness.Trim().Replace(',', '.');
                well.DestHardEarthThickness = well.DestHardEarthThickness.Trim().Replace(',', '.');
                well.SolidHardEarthThickness = well.SolidHardEarthThickness.Trim().Replace(',', '.');
                well.TurfThickness = well.TurfThickness.Trim().Replace(',', '.');
                well.GoldLayerThickness = well.GoldLayerThickness.Trim().Replace(',', '.');
                well.GoldLayerContentSlip = well.GoldLayerContentSlip.Trim().Replace(',', '.');
                well.VerticalGoldContent = well.VerticalGoldContent.Trim().Replace(',', '.');


                if (double.TryParse(well.SoftEarthThickness, out double softEarthThickness))
                {
                    well.SoftEarthThickness = softEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.DestHardEarthThickness, out double destHardEarthThickness))
                {
                    well.DestHardEarthThickness = destHardEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.SolidHardEarthThickness, out double solidHardEarthThickness))
                {
                    well.SolidHardEarthThickness = solidHardEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.TurfThickness, out double turfThickness))
                {
                    well.TurfThickness = turfThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.GoldLayerThickness, out double goldLayerThickness))
                {
                    well.GoldLayerThickness = goldLayerThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.GoldLayerContentSlip, out double goldLayerContentSlip))
                {
                    well.GoldLayerContentSlip = goldLayerContentSlip.ToString("0.000").Replace('.', ',');
                }
                if (double.TryParse(well.VerticalGoldContent, out double verticalGoldContent))
                {
                    well.VerticalGoldContent = verticalGoldContent.ToString("0.000").Replace('.', ',');
                }

                foreach (GoldData goldData in well.GoldDatas)
                {
                    if (double.TryParse(goldData.goldContent, out double goldContent))
                    {
                        goldData.goldContent = goldContent.ToString("0.000").Replace('.', ',');
                    }
                }
                foreach (GoldLayer goldLayer in well.GoldLayers)
                {
                    if (double.TryParse(goldLayer.goldContent, out double content))
                    {
                        goldLayer.goldContent = content.ToString("0.000").Replace('.', ',');
                    }
                }
            }
        }
    }
}
