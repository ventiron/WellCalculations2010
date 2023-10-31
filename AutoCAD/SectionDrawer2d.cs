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

using Section = WellCalculations2010.Model.Section;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Point = WellCalculations2010.Model.Point;
using DocumentFormat.OpenXml.Bibliography;
using WellCalculations2010.Properties;

namespace WellCalculations2010.AutoCAD
{
    internal class SectionDrawer2d
    {
        private static Point3d basePoint = new Point3d();

        private static double vertScale;
        private static double horScale;
        private static double vertScaleStep;

        private static double minHeight;
        private static double maxHeight;

        private static int InterpolatedPointsNumber = Properties.Settings.Default.InterpolatedPointsNumber;
        private static double VertScaleWidth = Properties.Settings.Default.VertScaleWidth;
        private static double VertScaleHeight = Properties.Settings.Default.VertScaleHeight;
        private static double VertScaleTextDist = Properties.Settings.Default.VertScaleTextDist;
        private static double VertScaleTextFontSize = Properties.Settings.Default.VertScaleFontSize;

        private static double textHeight = 2.5;

        private static double tableTextHeight = 2.5;
        private static double contentsTextHeight = 0.2;
        private static double contentsDepthTextHeight = 0.15;

        private static double tableRowMult = 2;
        private static double TextSpacing = textHeight * tableRowMult;

        private static double distFromScale = Properties.Settings.Default.DistFromScale;

        private static double SolidEarthHatchDist = Properties.Settings.Default.SolidEarthHatchDist;


        public static void DrawSection(Section section)
        {
            if (section.Wells.Count == 0)
            {
                return;
            }
            UpdateSettings();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;


            vertScale = GetScaleFromText(section.VerticalScale);
            horScale = GetScaleFromText(section.HorizontalScale);
            vertScaleStep = vertScale * VertScaleHeight;


            CalculateMinMaxHeight(section);


            PromptPointOptions ppo = new PromptPointOptions("Выберите точку");
            PromptPointResult ppr = doc.Editor.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
            {
                return;
            }
            basePoint = ppr.Value;
            try
            {
                //Блочим документ потому что так надо
                using (doc.LockDocument())
                {
                    AutoCADTextFormatter.FormatSectionStrings(section);
                    if (!Properties.Settings.Default.IsSplitByDistance)
                    {
                        if(Settings.Default.DrawScaleRuller)
                            DrawScaleRuler();
                        if (Settings.Default.DrawTable)
                            DrawTable(section);
                        if (Settings.Default.DrawWells)
                            DrawWells(section);
                        if (Settings.Default.DrawContents)
                            DrawGoldContents(section);
                        if (Settings.Default.DrawEarthSurfaces)
                            DrawEarthTypes(section);
                        if (Settings.Default.DrawHardEarthSurfaces)
                            DrawHardEarthTypes(section);
                    }
                    else
                    {
                        double length = 0;
                        double currentDist = 0;
                        double tableLength = distFromScale * 1.5 + 55;
                        double distModifier = !Properties.Settings.Default.IsTableConsidered ? tableLength : distFromScale;

                        double distBetveenSections = 100;
                        Section splitSection = new Section();
                        for (int i = 0; i < section.Wells.Count; i++)
                        {
                            splitSection.Wells.Add(section.Wells[i]);
                            length += section.Wells[i].DistanceToNextWell / horScale;
                            if ((length + distModifier > Properties.Settings.Default.SplitDistance) || i == section.Wells.Count - 1)
                            {
                                basePoint = new Point3d(basePoint.X + currentDist, basePoint.Y, basePoint.Z);

                                currentDist += length + tableLength + distBetveenSections;

                                if (!Properties.Settings.Default.IsFullVertScaleRullerReq)
                                    CalculateMinMaxHeight(splitSection);

                                DrawScaleRuler();
                                DrawTable(splitSection);
                                DrawWells(splitSection);
                                DrawGoldContents(splitSection);
                                DrawEarthTypes(splitSection);
                                DrawHardEarthTypes(splitSection);

                                Well lastWell = splitSection.Wells[splitSection.Wells.Count - 1];
                                splitSection = new Section();
                                length = 0;

                                if (lastWell.DistanceToNextWell + distModifier <= Properties.Settings.Default.SplitDistance)
                                {
                                    splitSection.Wells.Add(lastWell);
                                    length += lastWell.DistanceToNextWell / horScale;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Отрисовывает скважины и земную поверхность
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка<</param>
        private static void DrawWells(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;



                //Основной цикл отрисовки
                double currentDist = 0;

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    // Первая точка сплайна поверхности


                    //Отрисовываем скважину
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z), 0),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth), 0), 0, LineWeight.LineWeight015));
                    //Отрисовываем нижнюю риску скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale - 1, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth), 0),
                        new Point3d(basePoint.X + currentDist + distFromScale + 1, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth), 0)));
                    //Отрисовываем название и высоту скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(AutoCADTextFormatter.ApplyAutoCADFont($"{well.WellName}\n\\O{well.WellHeadPoint.Z.ToString("0.0").Replace('.', ',')}"),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z) + 10, 0),
                        textHeight: textHeight, atPoint: AttachmentPoint.MiddleCenter));
                    //Отрисовываем глубину скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(AutoCADTextFormatter.ApplyAutoCADFont(well.WellDepth.ToString("0.0").Replace('.', ',')),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth) - 5, 0),
                        textHeight: textHeight, atPoint: AttachmentPoint.TopCenter));



                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }




                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает содержания по скважинам
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawGoldContents(Section section)
        {
            double goldContentTextHeight = 0.15 / vertScale;
            double goldContentDepthTextHeight = 0.1 / vertScale;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

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
                    for (int j = 0; j < well.GoldDatas.Count; j++)
                    {
                        //Отрисовываем нижнюю риску содержания
                        bottomHeight = basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.GoldDatas[j].goldHeight);
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                            new Point3d(basePoint.X + currentDist + distFromScale, bottomHeight, 0),
                            new Point3d(basePoint.X + currentDist + distFromScale + 1, bottomHeight, 0)));
                        //Отрисовываем надпись высоты нижней риски содержания
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                            AutoCADTextFormatter.ApplyAutoCADFont($"{well.GoldDatas[j].goldHeight.ToString("0.0").Replace('.', ',')}"),
                            new Point3d(basePoint.X + currentDist + distFromScale - 1, bottomHeight, 0),
                            textHeight: goldContentDepthTextHeight, atPoint: AttachmentPoint.MiddleRight));

                        //Отрисовываем верхнюю риску содержания при нужде
                        if ((topHeight - bottomHeight) * vertScale > 0.5)
                        {
                            topHeight = bottomHeight + 0.5 / vertScale;
                            //Отрисовываем верхнюю риску содержания
                            AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                                new Point3d(basePoint.X + currentDist + distFromScale, topHeight, 0),
                                new Point3d(basePoint.X + currentDist + distFromScale + 1, topHeight, 0)));
                            //Отрисовываем надпись высоты верхней риски содержания
                            AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                                AutoCADTextFormatter.ApplyAutoCADFont($"{(well.GoldDatas[j].goldHeight - 0.5d).ToString("0.0").Replace('.', ',')}"),
                                new Point3d(basePoint.X + currentDist + distFromScale - 1, topHeight, 0),
                                textHeight: goldContentDepthTextHeight, atPoint: AttachmentPoint.MiddleRight));
                        }

                        //Отрисовываем само содержание
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(AutoCADTextFormatter.ApplyAutoCADFont($"{well.GoldDatas[j].goldContent}"),
                            new Point3d(basePoint.X + currentDist + distFromScale + 3, bottomHeight + (topHeight - bottomHeight) / 2,
                            0), textHeight: goldContentTextHeight, atPoint: AttachmentPoint.MiddleLeft));

                        topHeight = bottomHeight;
                    }

                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает линии пород в разрезе.
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
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
                double currentDist = 0;

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    if (i == 0)
                    {
                        surfacePoints.Add(new Point3d(basePoint.X + distFromScale / 2, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z), 0));
                    }

                    //Отрисовываем породы
                    foreach (EarthData earthData in well.EarthDatas)
                    {
                        if (!earthDatas.Contains(earthData.earthType))
                        {
                            earthDatas.Add(earthData.earthType);
                            double distForEarth = currentDist;
                            Point3dCollection earthSurface = new Point3dCollection();

                            AddPointWithExtrapolation2d(earthSurface, distForEarth, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - earthData.earthHeight), section, i);

                            bool isInterrupted = false;
                            distForEarth += well.DistanceToNextWell / horScale;
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

                                    AddPointWithExtrapolation2d(earthSurface, distForEarth, basePoint.Y + GetHeightDifference(section.Wells[k].WellHeadPoint.Z - nextEarthData.earthHeight), section, k);

                                    if (k == section.Wells.Count - 1)
                                        break;
                                }
                                else if (earthSurface.Count != 0)
                                {
                                    isInterrupted = true;

                                    InterpolateAndAddPoint(earthSurface, new Point3d(
                                        earthSurface[earthSurface.Count - 1].X + section.Wells[k - 1].DistanceToNextWell / horScale / 2,
                                        earthSurface[earthSurface.Count - 1].Y, 0));
                                }

                                distForEarth += section.Wells[k].DistanceToNextWell / horScale;
                                if (isInterrupted && earthSurface.Count != 0)
                                {
                                    AutoInitial.Initialize(tr, btr, new Spline(earthSurface, 5, 0.0));
                                    earthSurface = new Point3dCollection();
                                    isInterrupted = false;
                                }
                            }
                            if (earthSurface.Count != 0)
                                AutoInitial.Initialize(tr, btr, new Spline(earthSurface, 5, 0.0));
                        }
                    }

                    //Отрисовываем сплайн поверхности
                    InterpolateAndAddPoint(surfacePoints, new Point3d(basePoint.X + distFromScale + currentDist, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z), 0));
                    if (i == section.Wells.Count - 1)
                        InterpolateAndAddPoint(surfacePoints, new Point3d(basePoint.X + distFromScale * 1.5 + currentDist, basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z), 0));

                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }

                Spline surface = new Spline(surfacePoints, 5, 0.0);
                surface.ColorIndex = 42;
                surface.LineWeight = LineWeight.LineWeight015;
                AutoInitial.Initialize(tr, btr, surface);

                Spline surfaceTemp = CreateSplineCopyByY(surface, 2);
                AutoInitial.Initialize(tr, btr, surfaceTemp);
                HatchTwoSplines(surface, surfaceTemp, "EARTH", 0.4, 40);
                surfaceTemp.Erase();

                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает линии коренных пород по табличным данным
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawHardEarthTypes(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database bd = doc.Database;

            bool isThereNoSolidHardEarth = true;

            using (Transaction tr = bd.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Point3dCollection destEarthPoints = new Point3dCollection();
                Point3dCollection solidEarthPoints = new Point3dCollection();

                double currentDist = 0;

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    string destEarthTemp = well.DestHardEarthThickness.Replace(',', '.');
                    string solidEarthTemp = well.SolidHardEarthThickness.Replace(',', '.');

                    double solidEarth;
                    double.TryParse(solidEarthTemp, out solidEarth);
                    double destEarth;
                    double.TryParse(destEarthTemp, out destEarth);

                    //Работа с плотными коренными породами
                    if (solidEarth != 0d)
                    {
                        AddPointWithExtrapolation2d(solidEarthPoints, currentDist,
                            basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + solidEarth),
                            section, i);
                    }
                    else if (solidEarthPoints.Count != 0 || Settings.Default.AddHardEarth)
                    {
                        if (Settings.Default.AddHardEarth)
                        {
                            AddPointWithExtrapolation2d(solidEarthPoints, currentDist,
                                basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth - Settings.Default.AddHardEarthDist - (destEarth == 0d ? 1d : 0d)),
                                section, i);
                        }
                        else
                        {
                            InterpolateAndAddPoint(solidEarthPoints, new Point3d(
                                            solidEarthPoints[solidEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                            solidEarthPoints[solidEarthPoints.Count - 1].Y, 0));
                            AutoInitial.Initialize(tr, btr, new Spline(solidEarthPoints, 5, 0.0));
                            solidEarthPoints = new Point3dCollection();
                        }
                    }

                    //Работа с разрушенными коренными породами
                    if (destEarth != 0d)
                    {
                        AddPointWithExtrapolation2d(destEarthPoints, currentDist,
                            basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + destEarth + solidEarth),
                            section, i);
                    }
                    else if (destEarthPoints.Count != 0 || Settings.Default.AddHardEarth)
                    {
                        if (Settings.Default.AddHardEarth && solidEarth == 0)
                        {
                            AddPointWithExtrapolation2d(destEarthPoints, currentDist,
                                basePoint.Y + GetHeightDifference(well.WellHeadPoint.Z - well.WellDepth + solidEarth - Settings.Default.AddHardEarthDist),
                                section, i);
                        }
                        else
                        {
                            InterpolateAndAddPoint(destEarthPoints, new Point3d(
                                                destEarthPoints[destEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                                destEarthPoints[destEarthPoints.Count - 1].Y, 0));
                            Spline destEarthSurf = new Spline(destEarthPoints, 5, 0.0);
                            destEarthSurf.LineWeight = LineWeight.LineWeight015;
                            AutoInitial.Initialize(tr, btr, destEarthSurf);
                            destEarthPoints = new Point3dCollection();
                        }
                    }


                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }

                Spline solidEarthSurface = new Spline(solidEarthPoints, 5, 0.0);
                Spline destEarthSurface = new Spline(destEarthPoints, 5, 0.0);
                destEarthSurface.LineWeight = LineWeight.LineWeight015;
                if (destEarthPoints.Count != 0)
                    AutoInitial.Initialize(tr, btr, destEarthSurface);
                if (solidEarthPoints.Count != 0)
                    AutoInitial.Initialize(tr, btr, solidEarthSurface);

                if (solidEarthPoints.Count == (section.Wells.Count + 2) * (InterpolatedPointsNumber + 1) - InterpolatedPointsNumber)
                {
                    Spline tempSolidSpline = CreateSplineCopyByY(solidEarthSurface, SolidEarthHatchDist);
                    AutoInitial.Initialize(tr, btr, tempSolidSpline);
                    HatchTwoSplines(solidEarthSurface, tempSolidSpline, "ANSI31", 2);
                    tempSolidSpline.Erase();

                    if (solidEarthPoints.Count == destEarthPoints.Count)
                    {
                        HatchTwoSplines(destEarthSurface, solidEarthSurface, "ANSI31", 4);
                    }
                }
                if ((destEarthPoints.Count == (section.Wells.Count + 2) * (InterpolatedPointsNumber + 1) - 1) && isThereNoSolidHardEarth)
                {
                    Spline tempDestSpline = CreateSplineCopyByY(destEarthSurface, SolidEarthHatchDist);
                    AutoInitial.Initialize(tr, btr, tempDestSpline);
                    HatchTwoSplines(destEarthSurface, tempDestSpline, "ANSI31", 4);
                    tempDestSpline.Erase();
                }
                tr.Commit();
            }
        }

        private static void AddPointWithExtrapolation2d(Point3dCollection points, double dist, double Y, Section section, int counter)
        {
            if (points.Count == 0)
            {
                if (counter == 0)
                {
                    points.Add(new Point3d(basePoint.X + distFromScale / 2 + dist, Y, 0));
                }
                else
                    points.Add(new Point3d(
                        basePoint.X + distFromScale + dist - section.Wells[counter - 1].DistanceToNextWell / horScale / 2,
                        Y, 0));
            }

            //Вносим в массив саму точку
            InterpolateAndAddPoint(points, new Point3d(basePoint.X + distFromScale + dist, Y, 0));

            //Проверка последней точки, вносим в массим экстраполяцию последней точки
            if (counter == section.Wells.Count - 1)
            {
                InterpolateAndAddPoint(points, new Point3d(basePoint.X + distFromScale * 1.5 + dist, Y, 0));
            }
        }

        /// <summary>
        /// Отрисовывает таблицу под разрезом, а также заполняет её данными скважин
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawTable(Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database bd = doc.Database;

            using (Transaction tr = bd.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;


                double currentDist = 0;
                double tableRowDist = textHeight * tableRowMult;
                double xTableOffset = 55;
                double distFromTable = 30;

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    //Название скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                            AutoCADTextFormatter.ApplyAutoCADFont($"{well.WellName}"),
                            new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 0.20, 0),
                            textHeight: textHeight, atPoint: AttachmentPoint.TopCenter));
                    //Риска расстояния между скважинами
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist, 0),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 2, 0)));
                    //Надпись расстояния между скважинами
                    if (i != section.Wells.Count - 1)
                    {
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                        AutoCADTextFormatter.ApplyAutoCADFont(well.DistanceToNextWell.ToString("0.0").Replace('.', ',')),
                        new Point3d(basePoint.X + currentDist + distFromScale + well.DistanceToNextWell / 2 / horScale,
                        basePoint.Y - distFromTable - tableRowDist * 1.20, 0),
                       textHeight: textHeight, atPoint: AttachmentPoint.TopCenter));
                    }
                    //Прочие табличные данные
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                        AutoCADTextFormatter.ApplyAutoCADFont($"{well.WellDepth.ToString("0.0").Replace('.', ',')}\n" +
                        $"{well.SoftEarthThickness}\n" +
                        $"{well.DestHardEarthThickness}\n" +
                        $"{well.SolidHardEarthThickness}\n" +
                        $"{GetGoldLayersString(well)}"),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 2.20, 0),
                        textHeight: textHeight, lineSpacing: TextSpacing, atPoint:  AttachmentPoint.TopCenter));

                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }

                //Отрисовываем границы таблицы
                for (int i = 0; i < 11; i++)
                {
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X - xTableOffset, basePoint.Y - distFromTable - tableRowDist * i, 0),
                        new Point3d(basePoint.X + currentDist + distFromScale * 1.5,
                        basePoint.Y - distFromTable - tableRowDist * i, 0)));
                }

                //Отрисовываем надписи (легенду) таблицы
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                    AutoCADTextFormatter.ApplyAutoCADFont("Номер скважины\n" +
                    "Расстояние между скважинами\n" +
                    "Глубина скважин\n" +
                    "Пройдено по наносам\n" +
                    "Пройдено по РКП\n" +
                    "Пройдено по ПКП\n" +
                    "Мощность торфов\n" +
                    "Мощность песков\n" +
                    "Содержание на пласт\n" +
                    "Вертикальный запас"),
                    new Point3d(basePoint.X - xTableOffset * 0.95, basePoint.Y - distFromTable - tableRowDist * 0.20, 0),
                    textHeight: textHeight, lineSpacing: TextSpacing, atPoint: AttachmentPoint.TopLeft));
                //Отрисовываем единицы измерения легенды
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                    AutoCADTextFormatter.ApplyAutoCADFont("м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "г\\м²\n" +
                    "г\\м³\n"),
                    new Point3d(basePoint.X + 1, basePoint.Y - distFromTable - tableRowDist * 0.20, 0),
                    textHeight: textHeight, lineSpacing: TextSpacing, atPoint: AttachmentPoint.TopCenter));


                tr.Commit();
            }
        }

        private static string GetGoldLayersString(Well well)
        {
            int padding = 9;

            StringBuilder depth = new StringBuilder();
            StringBuilder thickness = new StringBuilder();
            StringBuilder content = new StringBuilder();
            StringBuilder verticalContent = new StringBuilder();
            foreach (GoldLayer layer in well.GoldLayers)
            {
                depth.Append($"{layer.depth}".PadRight(padding));
                thickness.Append($"{layer.thickness}".PadRight(padding));
                content.Append($"{(layer.goldContent.Trim().Equals(string.Empty) ? "-" : layer.goldContent)}".PadRight(padding));

                double parsedContent;
                bool parsed = double.TryParse(layer.goldContent, out parsedContent);
                verticalContent.Append($"{(parsed ? (layer.thickness * parsedContent).ToString("0,000") : "-")}".PadRight(padding));
            }

            return 
                $"{depth}\n" +
                $"{thickness}\n" +
                $"{content}\n" +
                $"{verticalContent}";
        }

        /// <summary>
        /// Рисует шкалу вертикального масштаба относительно базовой точки
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        private static void DrawScaleRuler()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                for (int i = 0; i < (maxHeight - minHeight) / vertScaleStep; i++)
                {
                    //Полилиния - один сегмент шкалы вертикального масштаба
                    Polyline poly = new Polyline();
                    poly.AddVertexAt(0, new Point2d(basePoint.X, basePoint.Y + VertScaleHeight * i), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(1, new Point2d(basePoint.X, basePoint.Y + VertScaleHeight * (i + 1)), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(2, new Point2d(basePoint.X + VertScaleWidth, basePoint.Y + VertScaleHeight * (i + 1)), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(3, new Point2d(basePoint.X + VertScaleWidth, basePoint.Y + VertScaleHeight * i), 0.0, -1.0, -1.0);
                    poly.Closed = true;

                    AutoInitial.Initialize(tr, btr, poly);

                    //Каждый четный элемент должен быть заштрихован
                    if (i % 2 == 0)
                    {
                        ObjectIdCollection ObjIds = new ObjectIdCollection
                        {
                            poly.Id
                        };

                        Hatch oHatch = new Hatch();
                        oHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");

                        AutoInitial.Initialize(tr, btr, oHatch);

                        oHatch.Associative = true;
                        oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                        oHatch.EvaluateHatch(true);
                    }
                    //У каждого пятого по шагу элемента должна быть подпись
                    if (((minHeight + vertScaleStep * i) % (vertScaleStep * 5.0)) == 0.0)
                    {
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(AutoCADTextFormatter.ApplyAutoCADFont($"{minHeight + vertScaleStep * i}"),
                            new Point3d(basePoint.X - VertScaleTextDist, basePoint.Y + VertScaleHeight * i, 0),
                             textHeight: VertScaleTextFontSize, atPoint: AttachmentPoint.MiddleRight));
                    }
                }
                tr.Commit();
            }
        }





        private static void HatchTwoSplines(Spline firstSpline, Spline secondSpline, string hatchType, double hatchScale = 1, double rotationAngle = 0)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Point3dCollection firstSplinePoints = new Point3dCollection();
                Point3dCollection secondSplinePoints = new Point3dCollection();
                for (int i = 0; i < firstSpline.NumFitPoints; i++)
                {
                    firstSplinePoints.Add(firstSpline.GetFitPointAt(i));
                }
                for (int i = 0; i < secondSpline.NumFitPoints; i++)
                {
                    secondSplinePoints.Add(secondSpline.GetFitPointAt(i));
                }

                Line VerticalAtStart = AutoInitial.CreateLine(firstSplinePoints[0], secondSplinePoints[0]);
                Line VerticalAtEnd = AutoInitial.CreateLine(firstSplinePoints[firstSplinePoints.Count - 1], secondSplinePoints[secondSplinePoints.Count - 1]);

                AutoInitial.Initialize(tr, btr, VerticalAtStart);
                AutoInitial.Initialize(tr, btr, VerticalAtEnd);

                ObjectIdCollection ObjIds = new ObjectIdCollection()
                {
                    firstSpline.Id,
                    secondSpline.Id,
                    VerticalAtStart.Id,
                    VerticalAtEnd.Id
                };

                Hatch oHatch = new Hatch();
                oHatch.SetHatchPattern(HatchPatternType.PreDefined, hatchType);
                AutoInitial.Initialize(tr, btr, oHatch);

                oHatch.Associative = false;
                oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                oHatch.EvaluateHatch(true);

                oHatch.PatternScale = hatchScale;
                oHatch.PatternAngle = rotationAngle * Math.PI / 180.0;
                oHatch.SetHatchPattern(oHatch.PatternType, oHatch.PatternName);


                VerticalAtStart.Erase();
                VerticalAtEnd.Erase();

                tr.Commit();
            }
        }

        private static Spline CreateSplineCopyByY(Spline source, double distance)
        {
            Point3dCollection firstSplinePoints = new Point3dCollection();
            Point3dCollection secondSplinePoints = new Point3dCollection();
            for (int i = 0; i < source.NumFitPoints; i++)
            {
                firstSplinePoints.Add(source.GetFitPointAt(i));
            }
            foreach (Point3d point in firstSplinePoints)
            {
                secondSplinePoints.Add(new Point3d(point.X, point.Y - distance, point.Z));
            }
            return new Spline(secondSplinePoints, 5, 0.0);
        }


        //Рассчетные и служебные методы

        /// <summary>
        /// Рассчитывает разницу высот между минимальной отметкой разреза и передаваемой отметкой в масштабе
        /// </summary>
        /// <param name="height">Высота точки в метрах</param>
        /// <returns></returns>
        private static double GetHeightDifference(double height)
        {
            return (height - minHeight) / vertScale;
        }

        /// <summary>
        /// Рассчитывает множитель масштаба исходя из единиц измерения чертежа (миллиметры).
        /// </summary>
        /// <param name="text">Масштаб в формате "N:N"</param>
        /// <returns></returns>
        private static double GetScaleFromText(string text)
        {
            double scale;
            double.TryParse(text.Split(':')[1], out scale);
            return scale * 0.001;
        }

        /// <summary>
        /// Форматирует строку для применения шрифта (Times New Roman)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        

        private static void CalculateMinMaxHeight(Section section)
        {
            minHeight = section.Wells[0].WellHeadPoint.Z - section.Wells[0].WellDepth;
            maxHeight = section.Wells[0].WellHeadPoint.Z;

            //Рассчитываем минимальную и максимальную высоту буровой линии
            foreach (Well well in section.Wells)
            {
                if (minHeight > well.WellHeadPoint.Z - well.WellDepth) minHeight = well.WellHeadPoint.Z - well.WellDepth;
                if (maxHeight < well.WellHeadPoint.Z) maxHeight = well.WellHeadPoint.Z;
            }

            minHeight = (minHeight % vertScaleStep) == 0 ? minHeight : minHeight - (minHeight % vertScaleStep);
            maxHeight = (vertScaleStep - (maxHeight % vertScaleStep))
                == 0 ? maxHeight : maxHeight + (vertScaleStep - (maxHeight % vertScaleStep));

            minHeight -= vertScaleStep * 3;
            maxHeight += vertScaleStep * 3;
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
            List<Point3d> InterpolatedPoints = InterpolateBetweenPoints(points[points.Count - 1], point, InterpolatedPointsNumber);

            foreach (Point3d Interpoint in InterpolatedPoints)
                points.Add(Interpoint);

            points.Add(point);
        }

        private static void UpdateSettings()
        {
            distFromScale = Properties.Settings.Default.DistFromScale;
            VertScaleWidth = Properties.Settings.Default.VertScaleWidth;
            InterpolatedPointsNumber = Properties.Settings.Default.InterpolatedPointsNumber;
            SolidEarthHatchDist = Properties.Settings.Default.SolidEarthHatchDist;
            VertScaleHeight = Properties.Settings.Default.VertScaleHeight;
            VertScaleTextDist = Properties.Settings.Default.VertScaleTextDist;
            VertScaleTextFontSize = Properties.Settings.Default.VertScaleFontSize;
        }
    }
}
