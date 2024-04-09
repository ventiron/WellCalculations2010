using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WellCalculations2010.ViewModel;



namespace WellCalculations2010.Model
{



    [Serializable]
    public class Well : ICloneable
    {
        public Well() 
        {
            GoldDatas = new ObservableCollection<GoldData>();
            EarthDatas = new ObservableCollection<EarthData>();
            GoldLayers = new ObservableCollection<GoldLayer>();
            WellHeadPoint = new Point();
            WellName = "0";
            DistanceToNextWell = 0;
            WellDepth = 0;
            DestHardEarthThickness = "";
            SolidHardEarthThickness = "";
            TurfThickness = "";
            GoldLayerThickness = "";
            GoldLayerContentSlip = "";
            VerticalGoldContent = "";
            IsUnfinished = false;
        }

        public ObservableCollection<GoldData> GoldDatas { get; set; }
        public ObservableCollection<EarthData> EarthDatas { get; set; }
        public ObservableCollection<GoldLayer> GoldLayers { get; set; }


        public Point WellHeadPoint { get; set; }

        public string WellName { get; set; }
        public double DistanceToNextWell { get; set; }
        public double WellDepth { get; set; }
        public string SoftEarthThickness {
            get
            {
                double.TryParse(DestHardEarthThickness.Replace(',','.'), out double destHardEarth);
                double.TryParse(SolidHardEarthThickness.Replace(',', '.'), out double solidHardEarth);

                return (WellDepth - destHardEarth - solidHardEarth).ToString("0.0").Replace('.', ',');
            }
        }
        public string DestHardEarthThickness { get; set; }
        public string SolidHardEarthThickness { get; set; }
        public string TurfThickness { get; set; }
        public string GoldLayerThickness { get; set; }
        public string GoldLayerContentSlip { get; set; }
        public string VerticalGoldContent { get; set; }

        [XmlIgnore]
        public bool IsUnfinished { get; set; }

        public object Clone()
        {
            Well clone = new Well();
            clone.WellHeadPoint = (Point)this.WellHeadPoint.Clone();
            clone.WellName = this.WellName.ToString();
            clone.DistanceToNextWell = this.DistanceToNextWell;
            clone.WellDepth = this.WellDepth;
            clone.DestHardEarthThickness = this.DestHardEarthThickness.ToString();
            clone.SolidHardEarthThickness = this.SolidHardEarthThickness.ToString();
            clone.TurfThickness = this.TurfThickness.ToString();
            clone.GoldLayerThickness = this.GoldLayerThickness.ToString();
            clone.GoldLayerContentSlip = this.GoldLayerContentSlip.ToString();
            clone.VerticalGoldContent = this.VerticalGoldContent.ToString();
            clone.IsUnfinished = this.IsUnfinished;

            ObservableCollection<GoldData> goldDatas = new ObservableCollection<GoldData>();
            ObservableCollection<EarthData> earthDatas = new ObservableCollection<EarthData>();
            ObservableCollection<GoldLayer> goldLayers = new ObservableCollection<GoldLayer>();

            foreach (GoldData goldData in this.GoldDatas)
            {
                goldDatas.Add((GoldData)goldData.Clone());
            }
            foreach (EarthData earthData in this.EarthDatas)
            {
                earthDatas.Add((EarthData)earthData.Clone());
            }
            foreach (GoldLayer goldLayer in this.GoldLayers)
            {
                goldLayers.Add((GoldLayer)goldLayer.Clone());
            }

            clone.GoldDatas = goldDatas;
            clone.EarthDatas = earthDatas;
            clone.GoldLayers = goldLayers;

            return clone;
        }
    }



}
