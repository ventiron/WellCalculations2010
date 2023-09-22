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

namespace WellCalculations2010.ViewModel
{
    internal class MainViewModel
    {
        
        public static ObservableCollection<string> Scales { get; set; }
        public static ObservableCollection<Well> Wells { get; set; }




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

        public int SelectedIndex
        {
            get { return Wells.Contains(SelectedItem) ? Wells.IndexOf(SelectedItem) : 0; }
        }
        public MainViewModel()
        {
            Scales = new ObservableCollection<string>
            {
                "1:10",
                "1:20",
                "1:50",
                "1:100",
                "1:200",
                "1:500",
                "1:1000",
                "1:2000"
            };

            Wells = new ObservableCollection<Well>
            {
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>()),
                new Well(new ObservableCollection<GoldData>(), new ObservableCollection<EarthData>())
            };
            Wells[0].WellName = "one";
            Wells[0].WellHeight = 485.3;
            Wells[0].WellDepth = 15;
            Wells[0].DistanceToNextWell = 30;
            Wells[0].GoldDatas.Add(new GoldData(10, "зн"));
            Wells[0].EarthDatas.Add(new EarthData(3, "hello1"));
            Wells[0].EarthDatas.Add(new EarthData(10, "hello"));

            Wells[1].WellName = "two";
            Wells[1].WellHeight = 482.6;
            Wells[1].WellDepth = 10;
            Wells[1].DistanceToNextWell = 40;
            Wells[1].GoldDatas.Add(new GoldData(5, "6"));
            Wells[1].EarthDatas.Add(new EarthData(3, "hello1"));
            Wells[1].EarthDatas.Add(new EarthData(5, "hello"));

            Wells[2].WellName = "three";
            Wells[2].WellHeight = 486.9;
            Wells[2].WellDepth = 30;
            Wells[2].DistanceToNextWell = 20;
            Wells[2].GoldDatas.Add(new GoldData(17, "6"));
            Wells[2].GoldDatas.Add(new GoldData(17.2, "6"));
            Wells[2].GoldDatas.Add(new GoldData(17.4, "6"));
            Wells[2].GoldDatas.Add(new GoldData(17.6, "6"));
            Wells[2].GoldDatas.Add(new GoldData(17.8, "6"));
            Wells[2].GoldDatas.Add(new GoldData(18, "6"));
            Wells[2].GoldDatas.Add(new GoldData(26, "6"));
            Wells[2].EarthDatas.Add(new EarthData(13, "hello1"));
            Wells[2].EarthDatas.Add(new EarthData(18, "hello"));

            Wells[3].WellName = "four";
            Wells[3].WellHeight = 482.9;
            Wells[3].WellDepth = 30;
            Wells[3].DistanceToNextWell = 40;
            Wells[3].GoldDatas.Add(new GoldData(17, "6"));
            Wells[3].GoldDatas.Add(new GoldData(26, "6"));
            Wells[3].EarthDatas.Add(new EarthData(5, "hello1"));
            Wells[3].EarthDatas.Add(new EarthData(8, "hello"));

            Wells[4].WellName = "five";
            Wells[4].WellHeight = 485.9;
            Wells[4].WellDepth = 35;
            Wells[4].DistanceToNextWell = 30;
            Wells[4].GoldDatas.Add(new GoldData(17, "6"));
            Wells[4].GoldDatas.Add(new GoldData(26, "6"));
            Wells[4].EarthDatas.Add(new EarthData(15, "hello1"));
            Wells[4].EarthDatas.Add(new EarthData(17, "hello"));

            Wells[5].WellName = "six";
            Wells[5].WellHeight = 487.3;
            Wells[5].WellDepth = 28;
            Wells[5].DistanceToNextWell = 20;
            Wells[5].GoldDatas.Add(new GoldData(17, "6"));
            Wells[5].GoldDatas.Add(new GoldData(26, "6"));
            Wells[5].EarthDatas.Add(new EarthData(10, "hello1"));
            Wells[5].EarthDatas.Add(new EarthData(12, "hello"));

            Wells[6].WellName = "seven";
            Wells[6].WellHeight = 490.3;
            Wells[6].WellDepth = 40;
            Wells[6].DistanceToNextWell = 28;
            Wells[6].GoldDatas.Add(new GoldData(17, "6"));
            Wells[6].GoldDatas.Add(new GoldData(26, "6"));
            Wells[6].EarthDatas.Add(new EarthData(15, "hello1"));
            Wells[6].EarthDatas.Add(new EarthData(17, "hello"));

            Wells[7].WellName = "eight";
            Wells[7].WellHeight = 493.6;
            Wells[7].WellDepth = 26;
            Wells[7].DistanceToNextWell = 15;
            Wells[7].GoldDatas.Add(new GoldData(17, "6"));
            Wells[7].GoldDatas.Add(new GoldData(26, "6"));
            Wells[7].EarthDatas.Add(new EarthData(18, "hello1"));
            Wells[7].EarthDatas.Add(new EarthData(24, "hello"));

            Wells[8].WellName = "nine";
            Wells[8].WellHeight = 498.6;
            Wells[8].WellDepth = 35;
            Wells[8].DistanceToNextWell = 24;
            Wells[8].GoldDatas.Add(new GoldData(17, "6"));
            Wells[8].GoldDatas.Add(new GoldData(26, "6"));
            Wells[8].EarthDatas.Add(new EarthData(18, "hello1"));
            Wells[8].EarthDatas.Add(new EarthData(24, "hello"));

            Wells[9].WellName = "ten";
            Wells[9].WellHeight = 499.1;
            Wells[9].WellDepth = 38;
            Wells[9].DistanceToNextWell = 24;
            Wells[9].GoldDatas.Add(new GoldData(17, "6"));
            Wells[9].GoldDatas.Add(new GoldData(26, "6"));
            Wells[9].EarthDatas.Add(new EarthData(20, "hello1"));
            Wells[9].EarthDatas.Add(new EarthData(28, "hello"));
            Wells[9].EarthDatas.Add(new EarthData(30, "single"));

            Wells[10].WellName = "eleven";
            Wells[10].WellHeight = 501;
            Wells[10].WellDepth = 32;
            Wells[10].DistanceToNextWell = 36;
            Wells[10].GoldDatas.Add(new GoldData(17, "6"));
            Wells[10].GoldDatas.Add(new GoldData(26, "6"));
            Wells[10].EarthDatas.Add(new EarthData(20, "hello1"));
            Wells[10].EarthDatas.Add(new EarthData(26, "hello"));
            Wells[10].EarthDatas.Add(new EarthData(15, "break"));
            Wells[10].EarthDatas.Add(new EarthData(5, "breakEnd"));

            Wells[11].WellName = "twelve";
            Wells[11].WellHeight = 493.5;
            Wells[11].WellDepth = 20;
            Wells[11].DistanceToNextWell = 37;
            Wells[11].EarthDatas.Add(new EarthData(15, "hello1"));
            Wells[11].EarthDatas.Add(new EarthData(18, "hello"));
            Wells[11].EarthDatas.Add(new EarthData(10, "break"));
            Wells[11].EarthDatas.Add(new EarthData(3, "breakEnd"));

            Wells[12].WellName = "thirtheen";
            Wells[12].WellHeight = 493.5;
            Wells[12].WellDepth = 18;
            Wells[12].DistanceToNextWell = 24;
            Wells[12].EarthDatas.Add(new EarthData(12, "hello1"));
            Wells[12].EarthDatas.Add(new EarthData(14, "hello"));
            Wells[12].EarthDatas.Add(new EarthData(5, "breakEnd"));
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


        private SimpleCommand addWell;
        public SimpleCommand AddWell
        {
            get
            {
                return addWell == null ?
                    (addWell = new SimpleCommand(obj => { Wells.Add(new Well()); })) : addWell;
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
                        //SelectedItem.GoldDatas.delete(new GoldData());
                        if (SelectedGoldData != null && SelectedItem.GoldDatas.Contains(SelectedGoldData))
                        {
                            SelectedItem.GoldDatas.Remove(SelectedGoldData);

                        }
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
                        //SelectedItem.EarthDatas.delete(new EarthData()); 
                        if (SelectedEarthData != null && SelectedItem.EarthDatas.Contains(SelectedEarthData))
                        {
                            SelectedItem.EarthDatas.Remove(SelectedEarthData);

                        }
                    })) : deleteEarthData;
            }
        }


        private SimpleCommand swapWellsUp;
        public SimpleCommand SwapWellsUp
        {
            get
            {
                return swapWellsUp == null ?
                    (swapWellsUp = new SimpleCommand(obj => {
                        int index = Wells.IndexOf(SelectedItem);
                        if (index > 0)
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
                        if (index < Wells.Count-1)
                        {
                            Wells.Insert(index + 2, SelectedItem);
                            Wells.RemoveAt(index);
                            SelectedItem = Wells[index + 1];
                        }


                    })) : swapWellsDown;
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
                                using (FileStream fs = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate))
                                {
                                    //string text = JsonConvert.SerializeObject(section);
                                    //fs.Write(Encoding.Default.GetBytes(text),0,0);
                                    using (StreamWriter writer = new StreamWriter(fs, Encoding.GetEncoding(1251)))
                                    {
                                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section));
                                        xmlSerializer.Serialize(writer, section);
                                    }
                                }
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
                                using (StreamReader fs = new StreamReader(fileDialog.FileName, Encoding.GetEncoding(1251)))
                                {
                                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section));
                                    section = (Section)xmlSerializer.Deserialize(fs);
                                }
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
                    (drawSegment = new SimpleCommand(obj => {
                        
                        Section section = new Section(Wells.ToList());
                        section.HorizontalScale = selectedHorScale;
                        section.VerticalScale = selectedVertScale;
                        if (ValidateSection(section))
                        {
                            ((Window)obj).WindowState = WindowState.Minimized;
                            AutoCAD_Commands.DrawSection(section);
                            return;
                        }

                    })) : drawSegment;
            }
        }


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
                if(goldData.goldHeight > well.WellDepth || goldData.goldHeight < 0 || data.Contains(goldData.goldHeight))
                {
                    return false;
                }
                data.Add(goldData.goldHeight);
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
    }

}
