using Microsoft.Win32;
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
        public ObservableCollection<Well> Wells { get; set; }


        public WellPlanarImport_ViewModel()
        {
            Wells = new ObservableCollection<Well>();
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

        private SimpleCommand deleteGoldLayer;
        public SimpleCommand DeleteGoldLayer
        {
            get
            {
                return deleteGoldLayer == null ? (deleteGoldLayer = new SimpleCommand(obj =>
                {
                    try
                    {
                        object[] prop = (object[])obj;

                        Well well = (Well)prop[0];
                        GoldLayer layer = (GoldLayer)prop[1];
                        well.GoldLayers.Remove(layer);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                })) : deleteGoldLayer;
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
                    Wells.Add(well);
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
                    foreach (var well in Wells)
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


        private SimpleCommand saveSegment;
        public SimpleCommand SaveSegment
        {
            get
            {
                return saveSegment == null ?
                    (saveSegment = new SimpleCommand(obj => {
                        try
                        {
                            Section section = new Section(Wells.ToList());

                            OpenFileDialog fileDialog = new OpenFileDialog();
                            fileDialog.Filter = "Файл сохранения (.xml)|*.xml";
                            fileDialog.CheckFileExists = false;
                            fileDialog.CheckPathExists = true;
                            if (fileDialog.ShowDialog() == true)
                            {
                                section.SaveSection(fileDialog.FileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    })) : saveSegment;
            }
        }

        private SimpleCommand loadSegment;
        public SimpleCommand LoadSegment
        {
            get
            {

                return loadSegment == null ?
                (loadSegment = new SimpleCommand(obj =>
                {
                    try
                    {
                        Section section = new Section();

                        OpenFileDialog fileDialog = new OpenFileDialog();
                        fileDialog.Filter = "Файл сохранения (.xml)|*.xml";
                        fileDialog.CheckFileExists = false;
                        fileDialog.CheckPathExists = true;
                        if (fileDialog.ShowDialog() == true)
                        {
                            section = Section.LoadSection(fileDialog.FileName);
                            Wells.Clear();
                            foreach (Well well in section.Wells) Wells.Add(well);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                })) : loadSegment;
            }
        }
    }
}
