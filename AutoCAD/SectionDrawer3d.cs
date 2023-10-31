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
using WellCalculations2010.Properties;
using MathModule.Primitives;

using Section = WellCalculations2010.Model.Section;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Point = WellCalculations2010.Model.Point;
using WellCalculations2010.Properties;
using System.Diagnostics;

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

            DrawMeshFromSectionPoints(GetSurfacePointsByLines(sections));
            DrawMeshFromSectionPoints(GetGoldBlockTopPointsByLines(sections));
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
                    DrawGoldContents(section);
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
        private static void DrawGoldContents(Section section)
        {
            double goldContentTextHeight = 0.15;
            double goldContentDepthTextHeight = 0.1;
            double perpRotationAngle = Math.PI / 2;
            

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            CoordinateSystem3d coordinateSystem = doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;

            

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                double currentDist = 0;
                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    

                    double bottomHeight;
                    double topHeight = 10000000000000;
                    double rotation = 0;

                    if (i != section.Wells.Count - 1) 
                        rotation = WellPlanarDrawer.CalculateRotationSwapXY(well.WellHeadPoint, section.Wells[i + 1].WellHeadPoint) - perpRotationAngle;

                    for (int j = 0; j < well.GoldDatas.Count; j++)
                    {

                        Matrix3d XrotationMatrix;
                        Matrix3d ZrotationMatrix;


                        bottomHeight = well.WellHeadPoint.Z - well.GoldDatas[j].goldHeight;

                        double markSecondX = well.WellHeadPoint.X - Math.Cos(rotation) * 0.25;
                        double marksSecondY = well.WellHeadPoint.Y - Math.Sin(rotation) * 0.25;

                        double contentTextX = well.WellHeadPoint.X - Math.Cos(rotation) * 0.35;
                        double contentTextY = well.WellHeadPoint.Y - Math.Sin(rotation) * 0.35;

                        double heightTextX = well.WellHeadPoint.X + Math.Cos(rotation) * 0.3;
                        double heightTextY = well.WellHeadPoint.Y + Math.Sin(rotation) * 0.3;

                        

                        //Отрисовываем нижнюю риску содержания
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                            new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, bottomHeight),
                            new Point3d(markSecondX, marksSecondY, bottomHeight)));


                        //Отрисовываем надпись высоты нижней риски содержания
                        MText bottomText = AutoInitial.CreateMtext(
                            AutoCADTextFormatter.ApplyAutoCADFont($"{well.GoldDatas[j].goldHeight.ToString("0.0").Replace('.', ',')}"),
                            new Point3d(heightTextX, heightTextY, bottomHeight),
                            textHeight: goldContentDepthTextHeight, atPoint: AttachmentPoint.MiddleRight);

                        XrotationMatrix = Matrix3d.Rotation(Math.PI * 1.5, coordinateSystem.Xaxis, bottomText.Location);
                        bottomText.TransformBy(XrotationMatrix);
                        ZrotationMatrix = Matrix3d.Rotation(rotation, coordinateSystem.Zaxis, bottomText.Location);
                        bottomText.TransformBy(ZrotationMatrix);

                        bottomText.Rotation = 0d;

                        AutoInitial.Initialize(tr, btr, bottomText);

                        //Отрисовываем верхнюю риску содержания при нужде
                        if (topHeight - bottomHeight > 0.5)
                        {
                            topHeight = bottomHeight + 0.5;
                            //Отрисовываем верхнюю риску содержания
                            AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                                new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, topHeight),
                                new Point3d(markSecondX, marksSecondY, topHeight)));


                            //Отрисовываем надпись высоты верхней риски содержания
                            MText topText = AutoInitial.CreateMtext(
                                AutoCADTextFormatter.ApplyAutoCADFont($"{(well.GoldDatas[j].goldHeight - 0.5d).ToString("0.0").Replace('.', ',')}"),
                                new Point3d(heightTextX, heightTextY, topHeight),
                                textHeight: goldContentDepthTextHeight, atPoint: AttachmentPoint.MiddleRight);

                            XrotationMatrix = Matrix3d.Rotation(Math.PI * 1.5, coordinateSystem.Xaxis, topText.Location);
                            topText.TransformBy(XrotationMatrix);
                            ZrotationMatrix = Matrix3d.Rotation(rotation, coordinateSystem.Zaxis, topText.Location);
                            topText.TransformBy(ZrotationMatrix);

                            topText.Rotation = 0d;

                            AutoInitial.Initialize(tr, btr, topText);
                        }

                        //Отрисовываем само содержание
                        MText contentText = AutoInitial.CreateMtext(AutoCADTextFormatter.ApplyAutoCADFont($"{well.GoldDatas[j].goldContent}"),
                            new Point3d(contentTextX, contentTextY, bottomHeight + (topHeight - bottomHeight) / 2),
                            textHeight: goldContentTextHeight, atPoint: AttachmentPoint.MiddleLeft);

                        XrotationMatrix = Matrix3d.Rotation(Math.PI * 1.5, coordinateSystem.Xaxis, contentText.Location);
                        contentText.TransformBy(XrotationMatrix);
                        ZrotationMatrix = Matrix3d.Rotation(rotation, coordinateSystem.Zaxis, contentText.Location);
                        contentText.TransformBy(ZrotationMatrix);

                        contentText.Rotation = 0d;

                        AutoInitial.Initialize(tr, btr, contentText);

                        topHeight = bottomHeight;
                    }
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
                        surfacePoints.Add(ExtrapolatePointWithInputZ(nextWell.WellHeadPoint, well.WellHeadPoint, well.WellHeadPoint.Z));
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
                                earthSurface.Add(ExtrapolatePointWithInputZ(nextWell.WellHeadPoint, well.WellHeadPoint, well.WellHeadPoint.Z - earthData.earthHeight));
                            }
                            else
                            {
                                Well prevWell = section.Wells[i - 1];
                                earthSurface.Add(GetMiddlePointWithInputZ(well.WellHeadPoint, prevWell.WellHeadPoint, well.WellHeadPoint.Z - earthData.earthHeight));
                            }
                            //Отрисовываем первую точку в первой скважине
                            InterpolateAndAddPoint(earthSurface, new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, well.WellHeadPoint.Z - earthData.earthHeight));
                            //Отрисовываем экстраполяцию в случае если только 1 скважина
                            if (i == section.Wells.Count - 1 && i == 0)
                            {
                                Well prevWell = new Well();
                                earthSurface.Add(ExtrapolatePointWithInputZ(well.WellHeadPoint, prevWell.WellHeadPoint, well.WellHeadPoint.Z - earthData.earthHeight));
                            }

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
                                        earthSurface.Add(GetMiddlePointWithInputZ(curWell.WellHeadPoint, prevWell.WellHeadPoint, curWell.WellHeadPoint.Z - nextEarthData.earthHeight));
                                    }

                                    //Добавляем найденную точку в коллекцию точек поверхности
                                    InterpolateAndAddPoint(earthSurface, 
                                        new Point3d(section.Wells[k].WellHeadPoint.X, section.Wells[k].WellHeadPoint.Y, section.Wells[k].WellHeadPoint.Z - nextEarthData.earthHeight));

                                    //Если порода есть в последней скважине - экстраполируем дополнительную точку.
                                    if (k == section.Wells.Count - 1)
                                    {
                                        Well prevWell = section.Wells[k - 1];
                                        Well curWell = section.Wells[k];
                                        earthSurface.Add(ExtrapolatePointWithInputZ(prevWell.WellHeadPoint, curWell.WellHeadPoint, curWell.WellHeadPoint.Z - nextEarthData.earthHeight));
                                        break;
                                    }
                                }
                                else if (earthSurface.Count != 0)
                                {
                                    isInterrupted = true;

                                    Well prevWell = section.Wells[k - 1];
                                    Well curWell = section.Wells[k];
                                    earthSurface.Add(GetMiddlePointWithInputZ(prevWell.WellHeadPoint, curWell.WellHeadPoint, earthSurface[earthSurface.Count - 1].Z));
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

                    //Экстраполируем последнюю точку земной поверхности
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





        //private static void DrawHardEarthTypes(Section section)
        //{
        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Database bd = doc.Database;

        //    bool isThereNoSolidHardEarth = true;

        //    using (Transaction tr = bd.TransactionManager.StartTransaction())
        //    {
        //        BlockTable bt = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
        //        BlockTableRecord btr = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

        //        Point3dCollection destEarthPoints = new Point3dCollection();
        //        Point3dCollection solidEarthPoints = new Point3dCollection();


        //        for (int i = 0; i < section.Wells.Count; i++)
        //        {
        //            Well well = section.Wells[i];

        //            string destEarthTemp = well.DestHardEarthThickness.Replace(',', '.');
        //            string solidEarthTemp = well.SolidHardEarthThickness.Replace(',', '.');

        //            double solidEarth;
        //            double.TryParse(solidEarthTemp, out solidEarth);
        //            double destEarth;
        //            double.TryParse(destEarthTemp, out destEarth);

        //            //Работа с плотными коренными породами
        //            if (solidEarth != 0d)
        //            {
        //                AddPointWithExtrapolation2d(solidEarthPoints, currentDist,
        //                    basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + solidEarth),
        //                    section, i);
        //            }
        //            else if (solidEarthPoints.Count != 0)
        //            {
        //                if (Settings.Default.AddHardEarth)
        //                {
        //                    AddPointWithExtrapolation2d(solidEarthPoints, currentDist,
        //                        basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth - Settings.Default.AddHardEarthDist - (destEarth == 0d ? 1d : 0d)),
        //                        section, i);
        //                }
        //                else
        //                {
        //                    InterpolateAndAddPoint(solidEarthPoints, new Point3d(
        //                                    solidEarthPoints[solidEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
        //                                    solidEarthPoints[solidEarthPoints.Count - 1].Y, 0));
        //                    AutoInitial.Initialize(tr, btr, new Spline(solidEarthPoints, 5, 0.0));
        //                    solidEarthPoints = new Point3dCollection();
        //                }
        //            }

        //            //Работа с разрушенными коренными породами
        //            if (destEarth != 0d)
        //            {
        //                AddPointWithExtrapolation2d(destEarthPoints, currentDist,
        //                    basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + destEarth + solidEarth),
        //                    section, i);
        //            }
        //            else if (destEarthPoints.Count != 0)
        //            {
        //                if (Settings.Default.AddHardEarth && solidEarth == 0)
        //                {
        //                    AddPointWithExtrapolation2d(destEarthPoints, currentDist,
        //                        basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + solidEarth - Settings.Default.AddHardEarthDist),
        //                        section, i);
        //                }
        //                else
        //                {
        //                    InterpolateAndAddPoint(destEarthPoints, new Point3d(
        //                                        destEarthPoints[destEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
        //                                        destEarthPoints[destEarthPoints.Count - 1].Y, 0));
        //                    Spline destEarthSurf = new Spline(destEarthPoints, 5, 0.0);
        //                    destEarthSurf.LineWeight = LineWeight.LineWeight015;
        //                    AutoInitial.Initialize(tr, btr, destEarthSurf);
        //                    destEarthPoints = new Point3dCollection();
        //                }
        //            }

        //        }

        //        Spline solidEarthSurface = new Spline(solidEarthPoints, 5, 0.0);
        //        Spline destEarthSurface = new Spline(destEarthPoints, 5, 0.0);
        //        destEarthSurface.LineWeight = LineWeight.LineWeight015;
        //        if (destEarthPoints.Count != 0)
        //            AutoInitial.Initialize(tr, btr, destEarthSurface);
        //        if (solidEarthPoints.Count != 0)
        //            AutoInitial.Initialize(tr, btr, solidEarthSurface);

        //        //if (solidEarthPoints.Count == (section.Wells.Count + 2) * (InterpolatedPointsNumber + 1) - InterpolatedPointsNumber)
        //        //{
        //        //    Spline tempSolidSpline = CreateSplineCopyByY(solidEarthSurface, SolidEarthHatchDist);
        //        //    AutoInitial.Initialize(tr, btr, tempSolidSpline);
        //        //    HatchTwoSplines(solidEarthSurface, tempSolidSpline, "ANSI31", 2);
        //        //    tempSolidSpline.Erase();

        //        //    if (solidEarthPoints.Count == destEarthPoints.Count)
        //        //    {
        //        //        HatchTwoSplines(destEarthSurface, solidEarthSurface, "ANSI31", 4);
        //        //    }
        //        //}
        //        //if ((destEarthPoints.Count == (section.Wells.Count + 2) * (InterpolatedPointsNumber + 1) - 1) && isThereNoSolidHardEarth)
        //        //{
        //        //    Spline tempDestSpline = CreateSplineCopyByY(destEarthSurface, SolidEarthHatchDist);
        //        //    AutoInitial.Initialize(tr, btr, tempDestSpline);
        //        //    HatchTwoSplines(destEarthSurface, tempDestSpline, "ANSI31", 4);
        //        //    tempDestSpline.Erase();
        //        //}
        //        tr.Commit();
        //    }
        //}

        private static void AddPointWithExtrapolation3d(Point3dCollection points, Well well, double Z, Section section, int counter)
        {
            if (points.Count == 0)
            {
                if (counter == 0)
                {
                    Well nextWell = section.Wells.Count > 1 ? section.Wells[section.Wells.IndexOf(well) + 1] : new Well();
                    points.Add(ExtrapolatePointWithInputZ(nextWell.WellHeadPoint, well.WellHeadPoint, Z));
                }
                else
                {
                    Well prevWell = section.Wells[section.Wells.IndexOf(well) - 1];
                    points.Add(GetMiddlePointWithInputZ(well.WellHeadPoint, prevWell.WellHeadPoint, Z));
                }
            }

            //Вносим в массив саму точку
            InterpolateAndAddPoint(points, new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, Z));

            //Проверка последней точки, вносим в массим экстраполяцию последней точки
            if (counter == section.Wells.Count - 1)
            {
                Well prevWell = section.Wells.Count > 1 ? section.Wells[section.Wells.IndexOf(well) + 1] : new Well();
                InterpolateAndAddPoint(points, ExtrapolatePointWithInputZ(prevWell.WellHeadPoint, well.WellHeadPoint, Z));
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




        private static void DrawMeshFromSectionPoints(List<Point3dCollection> lines)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;



                using (PolyFaceMesh faceMesh = new PolyFaceMesh())
                {
                    int vertexCount = 0;
                    // Add the new object to the block table record and the transaction
                    AutoInitial.Initialize(tr, btr, faceMesh);

                    foreach (Point3dCollection line in lines)
                        foreach (Point3d point in line)
                        {
                            PolyFaceMeshVertex acPMeshVer = new PolyFaceMeshVertex(point);
                            faceMesh.AppendVertex(acPMeshVer);
                            tr.AddNewlyCreatedDBObject(acPMeshVer, true);
                        }

                    for (int i = 0; i < lines.Count - 1; i++)
                    {
                        Point3dCollection currentLine = lines[i];
                        Point3dCollection nextLine = lines[i + 1];

                        int currentLineFirstVertex = i == 0 ? 1 : vertexCount - currentLine.Count + 1;
                        int nextLineFirstVertex = currentLineFirstVertex + currentLine.Count;

                        vertexCount = nextLineFirstVertex + nextLine.Count - 1;


                        for (int j = 0; j < Math.Max(currentLine.Count, nextLine.Count) - 1; j++)
                        {
                            if (j >= Math.Min(currentLine.Count, nextLine.Count) - 1)
                            {
                                if (currentLine.Count > nextLine.Count)
                                    using (FaceRecord face = new FaceRecord((short)(currentLineFirstVertex + j), (short)(currentLineFirstVertex + j + 1),
                                    (short)(nextLineFirstVertex + nextLine.Count - 1), 0))
                                    {
                                        faceMesh.AppendFaceRecord(face);
                                        tr.AddNewlyCreatedDBObject(face, true);
                                    }
                                else
                                    using (FaceRecord face = new FaceRecord((short)(nextLineFirstVertex + j), (short)(nextLineFirstVertex + j + 1),
                                    (short)(currentLineFirstVertex + currentLine.Count - 1), 0))
                                    {
                                        faceMesh.AppendFaceRecord(face);
                                        tr.AddNewlyCreatedDBObject(face, true);
                                    }
                                continue;
                            }
                            using (FaceRecord face = new FaceRecord((short)(currentLineFirstVertex + j), (short)(currentLineFirstVertex + j + 1),
                                (short)(nextLineFirstVertex + j + 1), 0))
                            {
                                faceMesh.AppendFaceRecord(face);
                                tr.AddNewlyCreatedDBObject(face, true);
                            }
                            using (FaceRecord face = new FaceRecord((short)(currentLineFirstVertex + j),
                                (short)(nextLineFirstVertex + j + 1), (short)(nextLineFirstVertex + j), 0))
                            {
                                faceMesh.AppendFaceRecord(face);
                                tr.AddNewlyCreatedDBObject(face, true);
                            }
                        }
                    }
                }
                tr.Commit();
            }

        }

        public static List<Point3dCollection> GetSurfacePointsByLines(List<Section> sections)
        {
            List<Point3dCollection> linePoints = new List<Point3dCollection>();
            foreach (Section section in sections)
            {
                Point3dCollection sectionSurfacePoints = new Point3dCollection();
                foreach (Well well in section.Wells)
                {
                    sectionSurfacePoints.Add(well.WellHeadPoint);
                }
                if (sectionSurfacePoints.Count > 0) linePoints.Add(sectionSurfacePoints);
            }
            return linePoints;
        }
        public static List<Point3dCollection> GetGoldBlockTopPointsByLines(List<Section> sections)
        {
            List<Point3dCollection> linePoints = new List<Point3dCollection>();
            foreach (Section section in sections)
            {
                Point3dCollection sectionSurfacePoints = new Point3dCollection();
                foreach (Well well in section.Wells)
                {
                    foreach (GoldLayer layer in well.GoldLayers)
                    {
                        if (layer.isAccounted)
                        {
                            sectionSurfacePoints.Add(new Point3d(well.WellHeadPoint.X, well.WellHeadPoint.Y, well.WellHeadPoint.Z - layer.depth));
                            break;
                        }
                    }
                }
                if (sectionSurfacePoints.Count > 0) linePoints.Add(sectionSurfacePoints);
            }
            return linePoints;
        }


        private static Point3d GetMiddlePointWithInputZ(Point p1, Point p2, double z)
        {
            MathVector3d vector = new MathVector3d(p1, p2);
            return new Point3d(p1.X + vector.X / 2, p1.Y + vector.Y / 2, z);
        }
        private static Point3d ExtrapolatePointWithInputZ(Point p1, Point p2, double z, double extrapolationMod = 1.5d)
        {
            MathVector3d vector = new MathVector3d(p1, p2);
            return new Point3d(p1.X + vector.X * extrapolationMod, p1.Y + vector.Y * extrapolationMod, z);
        }
    }
}
