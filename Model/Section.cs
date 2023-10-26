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




                 return (Section)xmlSerializer.Deserialize(fs);
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
                }
            }
        }
    }
}
