using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TriangulationAutoCAD;
using WellCalculations2010.Model;
using MathModule.Primitives;

using Section = WellCalculations2010.Model.Section;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using WellCalculations2010.Properties;

namespace WellCalculations2010.AutoCAD
{
    internal class SectionDrawer3d
    {

        public static void DrawSectionModel(List<Section> sections)
        {
            foreach (Section section in sections)
            {
                DrawSection(section);
            }
        }

        public static void DrawSection(Section section)
        {
            if (section.Wells.Count == 0)
            {
                return;
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;


            try
            {
                //Блочим документ потому что так надо
                using (doc.LockDocument())
                {
                    WellPlanarDrawer.ImportWells(section);
                    DrawWells(section);
                    DrawEarthTypes(section);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void DrawWells(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;



                //Основной цикл отрисовки
                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];
                    Point3d bottomPoint = new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, well.WellHeadPoint.Z - well.WellDepth);
                    double markSize = Settings.Default.WellMarkSize;


                    //Отрисовываем скважину
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(well.WellHeadPoint, bottomPoint,
                        0, LineWeight.LineWeight015));
                    //Отрисовываем нижнюю метку скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(bottomPoint.X - markSize, bottomPoint.Y - markSize, bottomPoint.Z),
                        new Point3d(bottomPoint.X + markSize, bottomPoint.Y + markSize, bottomPoint.Z)));
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(bottomPoint.X - markSize, bottomPoint.Y + markSize, bottomPoint.Z),
                        new Point3d(bottomPoint.X + markSize, bottomPoint.Y - markSize, bottomPoint.Z)));


                }

                tr.Commit();
            }
        }


        private static void DrawEarthTypes(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database bd = doc.Database;

            using (Transaction tr = bd.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Point3dCollection surfacePoints = new Point3dCollection();
                List<String> earthDatas = new List<String>();

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    if (i == 0)
                    {
                        Well nextWell = section.Wells.Count > 1 ? section.Wells[i + 1] : new Well();
                        MathVector3d vector = new MathVector3d(well.WellHeadPoint, nextWell.WellHeadPoint);
                        surfacePoints.Add(new Point3d(well.WellHeadPoint.X - vector.X / 2, well.WellHeadPoint.Y - vector.Y / 2, well.WellHeadPoint.Z));
                    }

                    //Отрисовываем породы
                    foreach (EarthData earthData in well.EarthDatas)
                    {
                        if (!earthDatas.Contains(earthData.earthType))
                        {
                            earthDatas.Add(earthData.earthType);
                            Point3dCollection earthSurface = new Point3dCollection();
                            //Отрисовываем экстраполяцию в начало если скважина -первая в буровой линии
                            if (i == 0)
                            {
                                Well nextWell = section.Wells.Count > 1 ? section.Wells[i + 1] : new Well();
                                MathVector3d vector = new MathVector3d(well.WellHeadPoint, nextWell.WellHeadPoint);
                                earthSurface.Add(new Point3d(well.WellHeadPoint.X - vector.X / 2, well.WellHeadPoint.Y - vector.Y / 2, well.WellHeadPoint.Z - earthData.earthHeight));
                            }
                            else
                            {
                                Well prevWell = section.Wells[i - 1];
                                MathVector3d vector = new MathVector3d(prevWell.WellHeadPoint, well.WellHeadPoint);
                                earthSurface.Add(new Point3d(well.WellHeadPoint.X - vector.X / 2, well.WellHeadPoint.Y - vector.Y / 2, well.WellHeadPoint.Z - earthData.earthHeight));
                            }
                            //Отрисовываем первую точку в первой скважине
                            InterpolateAndAddPoint(earthSurface, new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, well.WellHeadPoint.Z - earthData.earthHeight));

                            bool isInterrupted = false;
                            for (int k = i + 1; k < section.Wells.Count; k++)
                            {
                                int index = -1;
                                foreach (EarthData data in section.Wells[k].EarthDatas)
                                {
                                    if (data.earthType.Equals(earthData.earthType)) index = section.Wells[k].EarthDatas.IndexOf(data);
                                }
                                if (index != -1)
                                {
                                    EarthData nextEarthData = section.Wells[k].EarthDatas[index];

                                    //При прерывании если находим в последующих скважинах нужную породу сначала добавляем точку посередине 2 скважин
                                    if (earthSurface.Count == 0)
                                    {
                                        Well prevWell = section.Wells[k - 1];
                                        Well curWell = section.Wells[k];
                                        MathVector3d vector = new MathVector3d(curWell.WellHeadPoint, prevWell.WellHeadPoint);
                                        earthSurface.Add(
                                            new Point3d(curWell.WellHeadPoint.X + vector.X / 2, curWell.WellHeadPoint.Y + vector.Y / 2, curWell.WellHeadPoint.Z - nextEarthData.earthHeight));
                                    }
                                    //Добавляем найденную точку в коллекцию точек поверхности
                                    InterpolateAndAddPoint(earthSurface, 
                                        new Point3d(section.Wells[k].WellHeadPoint.X, section.Wells[k].WellHeadPoint.Y, section.Wells[k].WellHeadPoint.Z - nextEarthData.earthHeight));

                                    if (k == section.Wells.Count - 1)
                                    {
                                        Well prevWell = section.Wells[k - 1];
                                        Well curWell = section.Wells[k];
                                        MathVector3d vector = new MathVector3d(prevWell.WellHeadPoint, curWell.WellHeadPoint);
                                        InterpolateAndAddPoint(earthSurface, 
                                            new Point3d(curWell.WellHeadPoint.X + vector.X / 2, curWell.WellHeadPoint.Y + vector.Y / 2, curWell.WellHeadPoint.Z - nextEarthData.earthHeight));
                                        break;
                                    }
                                }
                                else if (earthSurface.Count != 0)
                                {
                                    isInterrupted = true;

                                    Well prevWell = section.Wells[k - 1];
                                    Well curWell = section.Wells[k];
                                    MathVector3d vector = new MathVector3d(prevWell.WellHeadPoint, curWell.WellHeadPoint);

                                    InterpolateAndAddPoint(earthSurface, 
                                        new Point3d(prevWell.WellHeadPoint.X + vector.X / 2, prevWell.WellHeadPoint.Y + vector.Y / 2, earthSurface[earthSurface.Count - 1].Z));
                                }

                                if (isInterrupted && earthSurface.Count != 0)
                                {
                                    AutoInitial.Initialize(tr, btr, new Spline(earthSurface, 6, 0.0));
                                    earthSurface = new Point3dCollection();
                                    isInterrupted = false;
                                }
                            }
                            if (earthSurface.Count != 0)
                            {
                                AutoInitial.Initialize(tr, btr, new Spline(earthSurface, 6, 0.0));
                            }
                        }
                    }

                    //Отрисовываем сплайн поверхности
                    InterpolateAndAddPoint(surfacePoints, well.WellHeadPoint);
                    if (i == section.Wells.Count - 1)
                    {
                        Well prevWell = section.Wells[i - 1];
                        MathVector3d vector = new MathVector3d(prevWell.WellHeadPoint, well.WellHeadPoint);
                        InterpolateAndAddPoint(surfacePoints, new Point3d(well.WellHeadPoint.X + vector.X / 2, well.WellHeadPoint.Y + vector.Y / 2, well.WellHeadPoint.Z));
                    }

                }

                Spline surface = new Spline(surfacePoints, 6, 0.0);
                surface.ColorIndex = 42;
                surface.LineWeight = LineWeight.LineWeight015;
                AutoInitial.Initialize(tr, btr, surface);

                //Spline surfaceTemp = CreateSplineCopyByY(surface, 2);
                //AutoInitial.Initialize(tr, btr, surfaceTemp);
                //HatchTwoSplines(surface, surfaceTemp, "EARTH", 0.4, 40);
                //surfaceTemp.Erase();

                tr.Commit();
            }
        }

        private static List<Point3d> InterpolateBetweenPoints(Point3d firstPoint, Point3d secondPoint, int numberOfPoints)
        {
            List<Point3d> result = new List<Point3d>();
            double dist = Math.Sqrt(Math.Pow(secondPoint.X - firstPoint.X, 2) + Math.Pow(firstPoint.Y - secondPoint.Y, 2));

            double dX = (secondPoint.X - firstPoint.X) / dist;
            double dY = (secondPoint.Y - firstPoint.Y) / dist;
            double dZ = (secondPoint.Z - firstPoint.Z) / dist;

            dist = dist / (numberOfPoints + 1);

            for (int i = 1; i <= numberOfPoints; i++)
            {
                result.Add(new Point3d(firstPoint.X + dX * dist * i, firstPoint.Y + dY * dist * i, firstPoint.Z + dZ * dist * i));
            }
            return result;
        }
        private static void InterpolateAndAddPoint(Point3dCollection points, Point3d point)
        {
            List<Point3d> InterpolatedPoints = InterpolateBetweenPoints(points[points.Count - 1], point, Settings.Default.InterpolatedPointsNumber);

            foreach (Point3d Interpoint in InterpolatedPoints)
                points.Add(Interpoint);

            points.Add(point);
        }

    }
}
