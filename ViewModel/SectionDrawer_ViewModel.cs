using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WellCalculations2010.AutoCAD;
using WellCalculations2010.Commands;
using WellCalculations2010.Model;
using Microsoft.Win32;
using System.Diagnostics;
using System.Xml.Serialization;
using ClosedXML.Excel;



namespace WellCalculations2010.ViewModel
{
    internal class SectionDrawer_ViewModel : INotifyPropertyChanged
    {
        
        public static ObservableCollection<string> Scales { get; set; }
        public static ObservableCollection<Well> Wells { get; set; }
        public static ObservableCollection<String> Hatchpat { get; set; } = new ObservableCollection<string>();


        public SectionDrawer_ViewModel()
        {
            Scales = new ObservableCollection<string>
            {
                "1:10",
                "1:20",
                "1:50",
                "1:100",
                "1:150",
                "1:200",
                "1:250",
                "1:500",
                "1:1000",
                "1:2000",
                "1:5000",
                "1:10000",
                "1:20000",
                "1:25000",
                "1:50000"
            };

            Wells = new ObservableCollection<Well>()
            {
                new Well()
            };

            //Wells = CreateRandomData(1000);
            SelectedItem = Wells[0];
        }

        private static string selectedVertScale = "1:100";
        public string SelectedVertScale
        {
            get { return selectedVertScale; }
            set
            {
                selectedVertScale = value;
                OnPropertyChanged("SelectedVertScale");
            }
        }


        private static string selectedHorScale = "1:500";
        public string SelectedHorScale
        {
            get { return selectedHorScale; }
            set
            {
                selectedHorScale = value;
                OnPropertyChanged("SelectedHorScale");
            }
        }


        private static Well selectedItem;
        public Well SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        private static GoldData selectedGoldData;
        public GoldData SelectedGoldData
        {
            get { return selectedGoldData; }
            set
            {
                selectedGoldData = value;
                OnPropertyChanged("SelectedGoldData");
            }
        }
        private static EarthData selectedEarthData;
        public EarthData SelectedEarthData
        {
            get { return selectedEarthData; }
            set
            {
                selectedEarthData = value;
                OnPropertyChanged("SelectedEarthData");
            }
        }

        private static bool goldContentIsBottom = true;
        public bool GoldContentIsBottom
        {
            get { return goldContentIsBottom; }
            set
            {
                goldContentIsBottom = value;
                OnPropertyChanged("GoldContentIsBottom");
            }
        }
  

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            //this.VerifyPropertyName(prop);

            //PropertyChangedEventHandler handler = this.PropertyChanged;
            //if (handler != null)
            //{
            //    var e = new PropertyChangedEventArgs(prop);
            //    handler(this, e);
            //}
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        //public void VerifyPropertyName(string propertyName)
        //{
        //    // Verify that the property name matches a real,  
        //    // public, instance property on this object.
        //    if (TypeDescriptor.GetProperties(this)[propertyName] == null)
        //    {
        //        string msg = "Invalid property name: " + propertyName;

        //        if (this.ThrowOnInvalidPropertyName)
        //            throw new Exception(msg);
        //        else
        //            Debug.Fail(msg);
        //    }
        //}


        //Функции добавления скважин, содержаний, пород.

        #region [Add commands]
        private SimpleCommand addWell;
        public SimpleCommand AddWell
        {
            get
            {
                return addWell == null ?
                    (addWell = new SimpleCommand(obj => {
                        if (Wells.Count == 0)
                        {
                            Wells.Add(new Well());
                            return;
                        }
                        Wells.Insert(Wells.IndexOf(selectedItem) + 1, new Well());

                    })) : addWell;
            }
        }

        private SimpleCommand addGoldData;
        public SimpleCommand AddGoldData
        {
            get
            {
                return addGoldData == null ?
                    (addGoldData = new SimpleCommand(obj => { if (SelectedItem != null) SelectedItem.GoldDatas.Add(new GoldData()); })) : addGoldData;
            }
        }

        private SimpleCommand addEarthData;
        public SimpleCommand AddEarthData
        {
            get
            {
                return addEarthData == null ?
                    (addEarthData = new SimpleCommand(obj => { if(SelectedItem!=null) SelectedItem.EarthDatas.Add(new EarthData()); })) : addEarthData;
            }
        }
        private SimpleCommand addGoldLayer;
        public SimpleCommand AddGoldLayer
        {
            get
            {
                return addGoldLayer == null ?
                    (addGoldLayer = new SimpleCommand(obj => { if (SelectedItem != null) SelectedItem.GoldLayers.Add(new GoldLayer()); })) : addGoldLayer;
            }
        }
        #endregion

        #region [Delete commands]
        private SimpleCommand deleteWell;
        public SimpleCommand DeleteWell
        {
            get
            {
                return deleteWell == null ?
                    (deleteWell = new SimpleCommand(obj => {
                        Wells.Remove(SelectedItem);
                        if (Wells.Count > 0) SelectedItem = Wells[0];
                    })) : deleteWell;
            }
        }

        private SimpleCommand deleteGoldData;
        public SimpleCommand DeleteGoldData
        {
            get
            {
                return deleteGoldData == null ?
                    (deleteGoldData = new SimpleCommand(obj => {
                        SelectedItem.GoldDatas.Remove((GoldData)obj);
                    })) : deleteGoldData;
            }
        }

        private SimpleCommand deleteEarthData;
        public SimpleCommand DeleteEarthData
        {
            get
            {
                return deleteEarthData == null ?
                    (deleteEarthData = new SimpleCommand(obj => {
                        SelectedItem.EarthDatas.Remove((EarthData) obj);
                    })) : deleteEarthData;
            }
        }
        private SimpleCommand deleteGoldLayer;
        public SimpleCommand DeleteGoldLayer
        {
            get
            {
                return deleteGoldLayer == null ?
                    (deleteGoldLayer = new SimpleCommand(obj => {
                        if(selectedItem.GoldLayers.Count != 1)
                            SelectedItem.GoldLayers.Remove((GoldLayer)obj);
                    })) : deleteGoldLayer;
            }
        }
        #endregion

        #region [Well move commands]
        private SimpleCommand swapWellsUp;
        public SimpleCommand SwapWellsUp
        {
            get
            {
                return swapWellsUp == null ?
                    (swapWellsUp = new SimpleCommand(obj => {
                        int index = Wells.IndexOf(SelectedItem);
                        if (index > 0 && index != -1)
                        {
                            Wells.Insert(index - 1, SelectedItem);
                            Wells.RemoveAt(index + 1);
                            SelectedItem = Wells[index - 1];
                        }
                        
                    
                    })) : swapWellsUp;
            }
        }
        private SimpleCommand swapWellsDown;
        public SimpleCommand SwapWellsDown
        {
            get
            {
                return swapWellsDown == null ?
                    (swapWellsDown = new SimpleCommand(obj => {
                        int index = Wells.IndexOf(SelectedItem);
                        if (index < Wells.Count-1 && index != -1)
                        {
                            Wells.Insert(index + 2, SelectedItem);
                            Wells.RemoveAt(index);
                            SelectedItem = Wells[index + 1];
                        }


                    })) : swapWellsDown;
            }
        }
        #endregion

        private SimpleCommand print;
        public SimpleCommand Print
        {
            get
            {
                return saveSegment == null ?
                    (saveSegment = new SimpleCommand(obj => {
                        try
                        {
                            MessageBox.Show(obj.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    })) : saveSegment;
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
                            section.HorizontalScale = selectedHorScale;
                            section.VerticalScale = selectedVertScale;



                            OpenFileDialog fileDialog = new OpenFileDialog();
                            fileDialog.Filter = "Файл сохранения (.xml)|*.xml";
                            fileDialog.CheckFileExists = false;
                            fileDialog.CheckPathExists = true;
                            if (fileDialog.ShowDialog() == true)
                            {
                                section.SaveSection(fileDialog.FileName);
                            }
                        }
                        catch(Exception ex)
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
                            //SelectedItem.EarthDatas.delete(new EarthData()); 
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


        private SimpleCommand drawSegment;
        public SimpleCommand DrawSegment
        {
            get
            {
                return drawSegment == null ?
                    (drawSegment = new SimpleCommand(obj =>
                    {
                        List<Well> wells = new List<Well>();
                        foreach (Well well in Wells)
                        {
                            wells.Add((Well)well.Clone());
                        }
                        Section section = new Section(wells);
                        section.HorizontalScale = selectedHorScale.ToString();
                        section.VerticalScale = selectedVertScale.ToString();
                        if (true)
                        {
                            ((Window)obj).WindowState = WindowState.Minimized;
                            SectionDrawer2d.DrawSection(section);
                            return;
                        }

                    })) : drawSegment;
            }
        }


        private SimpleCommand saveSettings;
        public SimpleCommand SaveSettings
        {
            get
            {
                return saveSettings == null ?
                (saveSettings = new SimpleCommand(obj =>
                {
                    Properties.Settings.Default.Save();
                })) : saveSettings;
            }
        }




        //private SimpleCommand loadFromExcel;
        //public SimpleCommand LoadFromExcel
        //{
        //    get
        //    {
        //        return loadFromExcel == null ?
        //            (loadFromExcel = new SimpleCommand(obj =>
        //            {
        //                try
        //                {
        //                    OpenFileDialog fileDialog = new OpenFileDialog();
        //                    fileDialog.Filter = "Файл Excel (.xlsx)|*.xlsx";
        //                    fileDialog.CheckFileExists = true;
        //                    fileDialog.CheckPathExists = true;
        //                    if (fileDialog.ShowDialog() == true)
        //                    {
        //                        XLWorkbook wb = new XLWorkbook(fileDialog.FileName);
        //                        IXLWorksheet worksheet = wb.Worksheets.Worksheet("Скважины");
        //                        IXLTable table = worksheet.Table("Well");
        //                        IXLTableRows rows = table.DataRange.Rows();

        //                        ObservableCollection<Well> newWells = new ObservableCollection<Well>();
        //                        foreach(IXLTableRow row in rows)
        //                        {
        //                            Well well = new Well();
        //                            well.WellName = row.Cell(2).GetValue<string>();
        //                            newWells.Add(well);
        //                        }

        //                        StringBuilder sb = new StringBuilder();
        //                        foreach (Well well1 in newWells)
        //                            sb.Append(well1.WellName);
        //                        MessageBox.Show(sb.ToString() + "\n");

        //                        wb.Save();

        //                    }
        //                } catch (Exception ex)
        //                {
        //                    MessageBox.Show(ex.Message);
        //                }
        //            })) : loadFromExcel;
        //    }
        //}







        private bool ValidateSection(Section section)
        {
            for(int i = 0; i < Wells.Count; i++)
            {
                if(!(ValidateGoldData(section.Wells[i]) && ValidateEarthData(section.Wells[i]))) return false;
            }
            return true;
        }
        private bool ValidateGoldData(Well well)
        {
            List<double> data = new List<double>();
            foreach(GoldData goldData in well.GoldDatas)
            {
                if(goldData.goldHeight > well.WellDepth || goldData.goldHeight < 0)
                {
                    return false;
                }
            }
            return true;
        }
        private bool ValidateEarthData(Well well)
        {
            List<double> data = new List<double>();
            foreach (EarthData earthData in well.EarthDatas)
            {
                if (earthData.earthHeight > well.WellDepth || earthData.earthHeight < 0 || data.Contains(earthData.earthHeight))
                {
                    return false;
                }
                data.Add(earthData.earthHeight);
            }
            return true;
        }

        //private static ObservableCollection<Well> CreateRandomData(int counter)
        //{
        //    double wellHeight = 100;
        //    double wellDepth = 10;
        //    double distNext = 20;

        //    string tag1 = "1";
        //    string tag2 = "2";
        //    string tag3 = "3";

        //    Random Rand = new Random();

        //    ObservableCollection<Well> data = new ObservableCollection<Well>();
        //    for(int i = 0 ; i < counter; i++)
        //    {
        //        Well well = new Well();



        //        wellHeight += Rand.Next(-2, 2) >= 0 ? Rand.NextDouble() * 5 : - Rand.NextDouble() * 5;
        //        wellDepth += Rand.Next(-2, 2) >= 0 ? Rand.NextDouble() * 5 : - Rand.NextDouble() * 5;
        //        distNext += Rand.Next(-2, 2) >= 0 ? Rand.NextDouble() * 5 : -Rand.NextDouble() * 5;

        //        if (wellDepth < 5) wellDepth += 50;
        //        if (distNext < 5) distNext += 30;

        //        if (wellDepth > 50) wellDepth -= 30;
        //        if (distNext > 50) distNext -= 30;

        //        well.WellName = i.ToString();
        //        well.WellHeight = wellHeight;
        //        well.WellDepth = wellDepth;
        //        well.DistanceToNextWell = distNext;

        //        well.EarthDatas.Add(new EarthData(Rand.Next((int)wellDepth/2), tag1));

        //        if (Rand.Next(1) == 1)
        //        {
        //            well.EarthDatas.Add(new EarthData(Rand.Next((int)wellDepth / 3, (int)wellDepth), tag2));
        //        }

        //        data.Add(well);
        //    }
        //    return data;
        //}
    }

}
