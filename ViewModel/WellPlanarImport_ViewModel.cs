using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using WellCalculations2010.AutoCAD;
using WellCalculations2010.Commands;
using WellCalculations2010.Model;

using Point = WellCalculations2010.Model.Point;

namespace WellCalculations2010.ViewModel
{
    internal class WellPlanarImport_ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Well> wells { get; set; }


        public WellPlanarImport_ViewModel()
        {
            wells = new ObservableCollection<Well>()
            {
                new Well(),
                new Well()
            };

            wells[0].GoldLayers.Add(new GoldLayer("2",2,2));
            wells[0].WellHeadPoint = new Point(1, 2, 3);

            wells[1].GoldLayers.Add(new GoldLayer("4", 1, 3));
            wells[1].WellHeadPoint = new Point(15, 6, 1);
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private SimpleCommand addGoldLayer;
        public SimpleCommand AddGoldLayer
        {
            get
            {
                return addGoldLayer == null ? (addGoldLayer = new SimpleCommand(obj =>
                {
                    Well well = (Well)obj;
                    well.GoldLayers.Add(new GoldLayer());
                })) : addGoldLayer;
            }
        }

        private SimpleCommand addWell;
        public SimpleCommand AddWell
        {
            get
            {
                return addWell == null ? (addWell = new SimpleCommand(obj =>
                {
                    Well well = new Well();
                    wells.Add(well);
                })) : addWell;
            }
        }

        private SimpleCommand importWells;
        public SimpleCommand ImportWells
        {
            get
            {
                return importWells == null ? (importWells = new SimpleCommand(obj =>
                {
                    Window window = (Window)obj;
                    window.WindowState = WindowState.Minimized;

                    List<Well> YXwells = new List<Well>();
                    foreach (var well in wells)
                    {
                        Well clone = (Well)well.Clone();
                        double temp = clone.WellHeadPoint.X;
                        clone.WellHeadPoint.X = clone.WellHeadPoint.Y;
                        clone.WellHeadPoint.Y = temp;
                        YXwells.Add(clone);
                    }
                    WellPlanarDrawer.ImportWells(new Section(YXwells));
                })) : importWells;
            }
        }
    }
}
