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
using MathModule.Primitives;

namespace WellCalculations2010.ViewModel
{
    internal class SectionDrawer_ViewModel : INotifyPropertyChanged
    {
        
        public static ObservableCollection<string> Scales { get; set; }
        //public static ObservableCollection<Well> Wells { get; set; }
        public static ObservableCollection<Section> Sections { get; set; }
        public ObservableCollection<Section> LongitudinalSection { get; set; }


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
            Sections = new ObservableCollection<Section>()
            {
                new Section()
            };

            SelectedSection = Sections[0];
            SelectedSection.Wells.Add(new Well());
            SelectedWell = SelectedSection.Wells[0];

            LongitudinalSection = new ObservableCollection<Section>
            {
                new Section()
            };
            LongitudinalSection[0].FileName = "Продольник.xml";
        }




        private Section selectedSection;
        public Section SelectedSection
        {
            get { return selectedSection; }
            set
            {
                selectedSection = value;
                OnPropertyChanged(nameof(SelectedSection));
            }
        }


        private Well selectedWell;
        public Well SelectedWell
        {
            get { return selectedWell; }
            set
            {
                selectedWell = value;
                OnPropertyChanged(nameof(SelectedWell));
            }
        }


        private GoldData selectedGoldData;
        public GoldData SelectedGoldData
        {
            get { return selectedGoldData; }
            set
            {
                selectedGoldData = value;
                OnPropertyChanged(nameof(SelectedGoldData));
            }
        }


        private EarthData selectedEarthData;
        public EarthData SelectedEarthData
        {
            get { return selectedEarthData; }
            set
            {
                selectedEarthData = value;
                OnPropertyChanged(nameof(SelectedEarthData));
            }
        }


        private int mainSelectionIndex = 1;
        public int MainSelectionIndex
        {
            get { return mainSelectionIndex; }
            set
            {
                mainSelectionIndex = value;
                OnPropertyChanged(nameof(MainSelectionIndex));
            }
        }





        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        //Функции добавления скважин, содержаний, пород.

        #region [Add commands]
        private SimpleCommand addSection;
        public SimpleCommand AddSection
        {
            get
            {
                return addSection == null ?
                    (addSection = new SimpleCommand(obj =>
                    {
                        if (Sections.Count == 0)
                        {
                            Sections.Add(new Section());
                            return;
                        }
                        Sections.Insert(Sections.IndexOf(SelectedSection) + 1, new Section());

                    })) : addSection;
            }
        }

        private SimpleCommand addWell;
        public SimpleCommand AddWell
        {
            get
            {
                return addWell == null ?
                    (addWell = new SimpleCommand(obj =>
                    {
                        if (SelectedSection.Wells.Count == 0)
                        {
                            SelectedSection.Wells.Add(new Well());
                            return;
                        }
                        SelectedSection.Wells.Insert(SelectedSection.Wells.IndexOf(selectedWell) + 1, new Well());

                    })) : addWell;
            }
        }

        private SimpleCommand addGoldData;
        public SimpleCommand AddGoldData
        {
            get
            {
                return addGoldData == null ?
                    (addGoldData = new SimpleCommand(obj => { if (SelectedWell != null) SelectedWell.GoldDatas.Add(new GoldData()); })) : addGoldData;
            }
        }

        private SimpleCommand addEarthData;
        public SimpleCommand AddEarthData
        {
            get
            {
                return addEarthData == null ?
                    (addEarthData = new SimpleCommand(obj => { if (SelectedWell != null) SelectedWell.EarthDatas.Add(new EarthData()); })) : addEarthData;
            }
        }
        private SimpleCommand addGoldLayer;
        public SimpleCommand AddGoldLayer
        {
            get
            {
                return addGoldLayer == null ?
                    (addGoldLayer = new SimpleCommand(obj => { if (SelectedWell != null) SelectedWell.GoldLayers.Add(new GoldLayer()); })) : addGoldLayer;
            }
        }
        #endregion

        #region [Delete commands]
        private SimpleCommand deleteSection;
        public SimpleCommand DeleteSection
        {
            get
            {
                return deleteSection == null ?
                    (deleteSection = new SimpleCommand(obj =>
                    {
                        Sections.Remove((Section) obj);
                        if (Sections.Count > 0) SelectedSection = Sections[0];
                    })) : deleteSection;
            }
        }


        private SimpleCommand deleteWell;
        public SimpleCommand DeleteWell
        {
            get
            {
                return deleteWell == null ?
                    (deleteWell = new SimpleCommand(obj =>
                    {
                        SelectedSection.Wells.Remove(SelectedWell);
                        if (SelectedSection.Wells.Count > 0) SelectedWell = SelectedSection.Wells[0];
                    })) : deleteWell;
            }
        }


        private SimpleCommand deleteGoldData;
        public SimpleCommand DeleteGoldData
        {
            get
            {
                return deleteGoldData == null ?
                    (deleteGoldData = new SimpleCommand(obj =>
                    {
                        SelectedWell.GoldDatas.Remove((GoldData)obj);
                    })) : deleteGoldData;
            }
        }


        private SimpleCommand deleteEarthData;
        public SimpleCommand DeleteEarthData
        {
            get
            {
                return deleteEarthData == null ?
                    (deleteEarthData = new SimpleCommand(obj =>
                    {
                        SelectedWell.EarthDatas.Remove((EarthData)obj);
                    })) : deleteEarthData;
            }
        }


        private SimpleCommand deleteGoldLayer;
        public SimpleCommand DeleteGoldLayer
        {
            get
            {
                return deleteGoldLayer == null ?
                    (deleteGoldLayer = new SimpleCommand(obj =>
                    {
                        if (selectedWell.GoldLayers.Count != 1)
                            SelectedWell.GoldLayers.Remove((GoldLayer)obj);
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
                    (swapWellsUp = new SimpleCommand(obj =>
                    {
                        int index = SelectedSection.Wells.IndexOf(SelectedWell);
                        if (index > 0 && index != -1)
                        {
                            SelectedSection.Wells.Insert(index - 1, SelectedWell);
                            SelectedSection.Wells.RemoveAt(index + 1);
                            SelectedWell = SelectedSection.Wells[index - 1];
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
                    (swapWellsDown = new SimpleCommand(obj =>
                    {
                        int index = SelectedSection.Wells.IndexOf(SelectedWell);
                        if (index < SelectedSection.Wells.Count - 1 && index != -1)
                        {
                            SelectedSection.Wells.Insert(index + 2, SelectedWell);
                            SelectedSection.Wells.RemoveAt(index);
                            SelectedWell = SelectedSection.Wells[index + 1];
                        }


                    })) : swapWellsDown;
            }
        }
        #endregion

        private SimpleCommand test;
        public SimpleCommand Test
        {
            get
            {
                return test == null ?
                    (test = new SimpleCommand(obj =>
                    {
                        //TestClass.testSave();
                    })) : test;
            }
        }

        #region [Section interactions]

        private SimpleCommand saveSegment;
        public SimpleCommand SaveSegment
        {
            get
            {
                return saveSegment == null ?
                    (saveSegment = new SimpleCommand(obj =>
                    {
                        try
                        {



                            //OpenFileDialog fileDialog = new OpenFileDialog();
                            //fileDialog.Filter = "Файл сохранения (.xml)|*.xml";
                            //fileDialog.CheckFileExists = false;
                            //fileDialog.CheckPathExists = true;
                            //if (fileDialog.ShowDialog() == true)
                            //{
                                SelectedSection.SaveSection();
                                OnPropertyChanged("FileName");
                            //}
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
                        

                        //OpenFileDialog fileDialog = new OpenFileDialog();
                        //fileDialog.Filter = "Файл сохранения (.xml)|*.xml";
                        //fileDialog.CheckFileExists = false;
                        //fileDialog.CheckPathExists = true;
                        //if (fileDialog.ShowDialog() == true)
                        //{
                            Section section = Section.LoadSection();
                            Sections.Add(section);
                            SelectedSection = section;
                        //}
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
                        try
                        {

                            ((Window)obj).WindowState = WindowState.Minimized;
                            SectionDrawer2d.DrawSection((Section)SelectedSection.Clone());
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }

                    })) : drawSegment;
            }
        }


        private SimpleCommand drawSegments3d;
        public SimpleCommand DrawSegments3d
        {
            get
            {
                return drawSegments3d == null ?
                    (drawSegments3d = new SimpleCommand(obj =>
                    {

                        try
                        {
                            ((Window)obj).WindowState = WindowState.Minimized;
                            List<Section> sectionsClone = new List<Section>();
                            foreach (Section section in Sections)
                            {
                                sectionsClone.Add((Section)section.Clone());
                            }
                            SectionDrawer3d.DrawSectionModel(sectionsClone);
                            return;
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show(ex.ToString());
                        }


                    })) : drawSegments3d;
            }
        }
        #endregion


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


        private SimpleCommand addWellToLongitudinalSection;
        public SimpleCommand AddWellToLongitudinalSection
        {
            get
            {

                return addWellToLongitudinalSection == null ?
                (addWellToLongitudinalSection = new SimpleCommand(obj =>
                {
                    Well OriginalWell = (Well)obj;
                    Well wellClone = (Well)OriginalWell.Clone();

                    if (LongitudinalSection[0].Wells.Count > 0)
                    {
                        Well prewWell = LongitudinalSection[0].Wells[LongitudinalSection[0].Wells.Count - 1];
                        MathVector3d vector = new MathVector3d(prewWell.WellHeadPoint, wellClone.WellHeadPoint);
                        prewWell.DistanceToNextWell = Math.Round(vector.Dist2d,4);
                    }
                    LongitudinalSection[0].Wells.Add(wellClone);
                })) : addWellToLongitudinalSection;
            }
        }


        private SimpleCommand setSelectionOnLongitudinalSection;
        public SimpleCommand SetSelectionOnLongitudinalSection
        {
            get
            {

                return setSelectionOnLongitudinalSection == null ?
                (setSelectionOnLongitudinalSection = new SimpleCommand(obj =>
                {
                    MainSelectionIndex = -1;
                    SelectedSection = LongitudinalSection[0];

                })) : setSelectionOnLongitudinalSection;
            }
        }












        private bool isEarthTypePresent(string earthType)
        {
            foreach (Well well in SelectedSection.Wells)
            {
                foreach (EarthData earthData in well.EarthDatas)
                {
                    if (earthData.earthType.Equals(earthType)) return true;
                }
            }
            return false;
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
