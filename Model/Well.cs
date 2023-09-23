using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using WellCalculations2010.ViewModel;

namespace WellCalculations2010.Model
{

    [Serializable]
    public class Section
    {
        public Section() 
        {
            Wells = new List<Well>();
        }
        public Section(List<Well> Wells)
        {
            this.Wells = Wells;
        }
        public List<Well> Wells { get; set; }
        public string HorizontalScale { get; set; }
        public string VerticalScale { get; set; }
    }

    [Serializable]
    public class Well : ICloneable
    {
        public Well() 
        {
            GoldDatas = new ObservableCollection<GoldData>();
            EarthDatas = new ObservableCollection<EarthData>();
            WellName = "0";
            DistanceToNextWell = 0;
            WellHeight = 0;
            WellDepth = 0;
            SoftEarthThickness = "";
            DestHardEarthThickness = "";
            SolidHardEarthThickness = "";
            TurfThickness = "";
            GoldLayerThickness = "";
            GoldLayerContentSlip = "";
            VerticalGoldContent = "";
        }

        public Well(ObservableCollection<GoldData> GoldDatas, ObservableCollection<EarthData> EarthDatas)
        {
            this.GoldDatas = GoldDatas;
            this.EarthDatas = EarthDatas;
            WellName = "0";
            DistanceToNextWell = 0;
            WellHeight = 0;
            WellDepth = 0;
            SoftEarthThickness = "";
            DestHardEarthThickness = "";
            SolidHardEarthThickness = "";
            TurfThickness = "";
            GoldLayerThickness = "";
            GoldLayerContentSlip = "";
            VerticalGoldContent = "";
        }

        public ObservableCollection<GoldData> GoldDatas { get; set; }
        public ObservableCollection<EarthData> EarthDatas { get; set; }


        public string WellName { get; set; }
        public double DistanceToNextWell { get; set; }
        public double WellHeight { get; set; }
        public double WellDepth { get; set; }
        public string SoftEarthThickness { get; set; }
        public string DestHardEarthThickness { get; set; }
        public string SolidHardEarthThickness { get; set; }
        public string TurfThickness { get; set; }
        public string GoldLayerThickness { get; set; }
        public string GoldLayerContentSlip { get; set; }
        public string VerticalGoldContent { get; set; }

        public object Clone()
        {
            Well clone = new Well();
            clone.WellName = this.WellName;
            clone.DistanceToNextWell = this.DistanceToNextWell;
            clone.WellHeight = this.WellHeight;
            clone.WellDepth = this.WellDepth;
            clone.SoftEarthThickness = this.SoftEarthThickness;
            clone.DestHardEarthThickness = this.DestHardEarthThickness;
            clone.SolidHardEarthThickness= this.SolidHardEarthThickness;
            clone.TurfThickness = this.TurfThickness;
            clone.GoldLayerThickness = this.GoldLayerThickness;
            clone.GoldLayerContentSlip = this.GoldLayerContentSlip;
            clone.VerticalGoldContent = this.VerticalGoldContent;

            ObservableCollection<GoldData> goldDatas = new ObservableCollection<GoldData>();
            ObservableCollection<EarthData> earthDatas = new ObservableCollection<EarthData>();

            foreach(GoldData goldData in this.GoldDatas)
            {
                goldDatas.Add((GoldData)goldData.Clone());
            }
            foreach (EarthData earthData in this.EarthDatas)
            {
                earthDatas.Add((EarthData)earthData.Clone());
            }

            clone.GoldDatas = goldDatas;
            clone.EarthDatas = earthDatas;

            return clone;
        }
    }
    [Serializable]
    public class GoldData : ICloneable
    {
        public GoldData()
        {

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
    [Serializable]
    public class EarthData : ICloneable
    {
        public EarthData()
        {

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
