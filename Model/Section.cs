using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using MathModule;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MathModule.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WellCalculations2010.Model
{
    [Serializable]
    public class Section : ICloneable, INotifyPropertyChanged
    {
        public Section()
        {
            HorizontalScale = "1:500";
            VerticalScale = "1:100";
            Wells = new ObservableCollection<Well>();
            FileName = "Новый.xml";
        }

        public Section(ObservableCollection<Well> Wells)
        {
            HorizontalScale = "1:500";
            VerticalScale = "1:100";
            this.Wells = Wells;
        }
        public ObservableCollection<Well> Wells { get; set; }
        public string HorizontalScale { get; set; }
        public string VerticalScale { get; set; }
        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public static Section LoadSection(string path)
        {
            try
            {
                using (StreamReader fs = new StreamReader(path, Encoding.GetEncoding(1251)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section),new Type[] {typeof(MathPoint), typeof(Point3d), typeof(PolylineVertex3d)});



                    xmlSerializer.UnknownElement += (sender, e) =>
                    {
                        var sec = (Well)e.ObjectBeingDeserialized;

                        if (e.Element.Name == "WellHeight")
                        {
                            sec.WellHeadPoint.Z = double.Parse(e.Element.InnerText);
                        }
                    };



                    Section result = (Section)xmlSerializer.Deserialize(fs);
                    result.FileName = new FileInfo(path).Name;

                    return result;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new Section();
            }
        }

        public void SaveSection(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs, Encoding.GetEncoding(1251)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section), new Type[] { typeof(MathPoint), typeof(Point3d), typeof(PolylineVertex3d) });
                    xmlSerializer.Serialize(writer, this);

                    this.FileName = new FileInfo(path).Name;
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        public object Clone()
        {
            Section clone = new Section();
            foreach (Well well in Wells)
            {
                clone.Wells.Add((Well)well.Clone());
            }
            clone.HorizontalScale = HorizontalScale.ToString();
            clone.VerticalScale = VerticalScale.ToString();
            clone.FileName = FileName.ToString();
            return clone;
        }
    }
}
