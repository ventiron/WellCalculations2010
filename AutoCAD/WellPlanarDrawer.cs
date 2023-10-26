using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using TriangulationAutoCAD;
using WellCalculations2010.Model;
using WellCalculations2010.Properties;
using MathModule;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Section = WellCalculations2010.Model.Section;
using Point = WellCalculations2010.Model.Point;


namespace WellCalculations2010.AutoCAD
{
    public class WellPlanarDrawer
    {
        private static double rotation = 0;

        public static void ImportWells(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dat = doc.Database;

            SectionDrawer.FormatSectionStrings(section);

            using (doc.LockDocument())
            {
                using (Transaction tr = dat.TransactionManager.StartTransaction())
                {
                    try
                    {
                        BlockTable bt = tr.GetObject(dat.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = tr.GetObject(dat.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        if (section.Wells.Count > 1)
                            CalculateRotation(section.Wells[0].WellHeadPoint, section.Wells[section.Wells.Count - 1].WellHeadPoint);
                        foreach (Well well in section.Wells)
                        {
                            DrawWellText(well, tr, btr);
                        }

                        tr.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private static void DrawWellText(Well well, Transaction tr, BlockTableRecord btr)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            CoordinateSystem3d coordinateSystem = doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Matrix3d rotationMatrix = Matrix3d.Rotation(rotation, coordinateSystem.Zaxis, well.WellHeadPoint);
            double textDist = Settings.Default.WellMarkSize + Settings.Default.PlanarTextDistFromMark;
            double textSize = Settings.Default.WellPlanarTextSize;


            AutoInitial.Initialize(tr, btr, AutoInitial.CreateCircle(well.WellHeadPoint, Settings.Default.WellMarkSize));

            //Текст сверху с названием скважины
            AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, well.WellName,
                new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y + textDist, well.WellHeadPoint.Z),
                textHeight: textSize, atPoint: AttachmentPoint.BottomCenter));
            //Текст слева с высотой скважины
            AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, SectionDrawer.ApplyAutoCADFont(well.WellHeadPoint.Z.ToString("0.0").Replace('.', ',')),
                new Point3d(well.WellHeadPoint.X - textDist, well.WellHeadPoint.Y + Settings.Default.WellPlanarTextSize * 0.55, well.WellHeadPoint.Z),
                textHeight: textSize, atPoint: AttachmentPoint.MiddleRight));
            //Текст слева с глубиной скважины
            AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, SectionDrawer.ApplyAutoCADFont(well.WellDepth.ToString("0.0").Replace('.', ',')),
                new Point3d(well.WellHeadPoint.X - textDist, well.WellHeadPoint.Y - Settings.Default.WellPlanarTextSize * 0.55, well.WellHeadPoint.Z),
                textHeight: textSize, atPoint: AttachmentPoint.MiddleRight));

            double TextSizeToDistMod = 3.3333;
            double dist = 0;

            foreach (GoldLayer layer in well.GoldLayers)
            {
                //Текст с глубиной слоя
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, SectionDrawer.ApplyAutoCADFont(layer.depth.ToString("0.0").Replace('.', ',')),
                    new Point3d(well.WellHeadPoint.X + textDist + dist, well.WellHeadPoint.Y + Settings.Default.WellPlanarTextSize * 1.05, well.WellHeadPoint.Z),
                    textHeight: textSize, atPoint: AttachmentPoint.MiddleLeft));
                //Текст с содержанием по слою
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, SectionDrawer.ApplyAutoCADFont(layer.goldContent),
                    new Point3d(well.WellHeadPoint.X + textDist + dist, well.WellHeadPoint.Y, well.WellHeadPoint.Z),
                    textHeight: textSize, atPoint: AttachmentPoint.MiddleLeft));
                //Текст с мощностью слоя
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(rotationMatrix, SectionDrawer.ApplyAutoCADFont(layer.thickness.ToString("0.0").Replace('.', ',')),
                    new Point3d(well.WellHeadPoint.X + textDist + dist, well.WellHeadPoint.Y - Settings.Default.WellPlanarTextSize * 1.05, well.WellHeadPoint.Z),
                    textHeight: textSize, atPoint: AttachmentPoint.MiddleLeft));


                dist += Settings.Default.WellPlanarTextSize * TextSizeToDistMod;
            }
        }

        private static void CalculateRotation(Point p1, Point p2)
        {
            rotation = BMF.AngleByCoordinates_UnClockwise(new Point(p1.Y, p1.X, p1.Z), new Point(p2.Y, p2.X, p2.Z));
        }
    }
}
