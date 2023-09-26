using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Private.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TriangulationAutoCAD;
using WellCalculations2010.Model;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Section = WellCalculations2010.Model.Section;

namespace WellCalculations2010.AutoCAD
{
    public class AutoCAD_Commands : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        private static WellCalculations_MainWindow _MainWindow;

        private static double vertScale;
        private static double horScale;
        private static double vertScaleStep;

        private static double minHeight;
        private static double maxHeight;

        private static readonly double textHeight = 2.5;
        private static readonly double tableRowMult = 2;
        private static readonly double TextSpacing = textHeight * tableRowMult;

        private static double distFromScale = 20;


        [CommandMethod("РРР")]
        public void DrawSection_Command()
        {
            if(_MainWindow == null || !_MainWindow.IsLoaded)
                _MainWindow = new WellCalculations_MainWindow();
            if (_MainWindow.IsLoaded && _MainWindow.WindowState == System.Windows.WindowState.Minimized)
                _MainWindow.WindowState = System.Windows.WindowState.Normal;
            else
                _MainWindow.Show();
        }


        public static void DrawSection(Section section)
        {
            if (section.Wells.Count == 0)
            {
                return;
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            vertScale = GetScaleFromText(section.VerticalScale);
            horScale = GetScaleFromText(section.HorizontalScale);
            vertScaleStep = vertScale * 10.0;
            distFromScale = 20 + 5 / horScale;

            
            

            CalculateMinMaxHeight(section);


            PromptPointOptions ppo = new PromptPointOptions("Выберите точку");
            PromptPointResult ppr = doc.Editor.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
            {
                return;
            }
            Point3d basePoint = ppr.Value;


            //Блочим документ потому что так надо
            using (doc.LockDocument())
            {
                FormatSectionStrings(section);

                DrawScaleRuler(basePoint);
                DrawTable(basePoint, section);

                DrawWells(basePoint, section);
                DrawGoldContents(basePoint, section);
                DrawEarthTypes(basePoint, section);
                DrawHardEarthTypes(basePoint, section);
            }

        }

        /// <summary>
        /// Отрисовывает скважины и земную поверхность
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка<</param>
        private static void DrawWells(Point3d basePoint, Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                //Основной цикл отрисовки
                double currentDist = 0;
                Point3dCollection surface = new Point3dCollection();
                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    // Первая точка сплайна поверхности
                    if (i == 0)
                    {
                        surface.Add(new Point3d(basePoint.X + distFromScale / 2, basePoint.Y + GetHeightDifference(well.WellHeight), 0));
                    }

                    //Отрисовываем скважину
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeight), 0),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth), 0)));
                    //Отрисовываем нижнюю риску скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale - 2, basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth), 0),
                        new Point3d(basePoint.X + currentDist + distFromScale + 2, basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth), 0)));
                    //Отрисовываем название и высоту скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(ApplyAutoCADFont($"{well.WellName}\n\\O{well.WellHeight.ToString("0.0").Replace('.', ',')}"),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeight) + 10, 0),
                        0, 0, textHeight, 0, AttachmentPoint.MiddleCenter));
                    //Отрисовываем глубину скважины
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(ApplyAutoCADFont(well.WellDepth.ToString("0.0").Replace('.', ',')),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth) - 10, 0),
                        0, 0, textHeight, 0, AttachmentPoint.TopCenter));


                    //Отрисовываем сплайн поверхности
                    surface.Add(new Point3d(basePoint.X + distFromScale + currentDist, basePoint.Y + GetHeightDifference(well.WellHeight), 0));
                    if (i == section.Wells.Count - 1)
                        surface.Add(new Point3d(basePoint.X + distFromScale * 1.5 + currentDist, basePoint.Y + GetHeightDifference(well.WellHeight), 0));
                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }
                AutoInitial.Initialize(tr, btr, new Spline(surface, 5, 0.0));

                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает содержания по скважинам
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawGoldContents(Point3d basePoint, Section section)
        {
            double goldContentTextHeight = 0.2 / vertScale;
            double goldContentDepthTextHeight = 0.15 / vertScale;

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
                        bottomHeight = basePoint.Y + GetHeightDifference(well.WellHeight - well.GoldDatas[j].goldHeight);
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                            new Point3d(basePoint.X + currentDist + distFromScale, bottomHeight, 0),
                            new Point3d(basePoint.X + currentDist + distFromScale + 1, bottomHeight, 0)));
                        //Отрисовываем надпись высоты нижней риски содержания
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                            ApplyAutoCADFont($"{well.GoldDatas[j].goldHeight.ToString("0.0").Replace('.',',')}"),
                            new Point3d(basePoint.X + currentDist + distFromScale - 3, bottomHeight, 0),
                            0, 0, goldContentDepthTextHeight, 0, AttachmentPoint.MiddleRight));

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
                                ApplyAutoCADFont($"{(well.GoldDatas[j].goldHeight - 0.5d).ToString("0.0").Replace('.', ',')}"),
                                new Point3d(basePoint.X + currentDist + distFromScale - 3, topHeight, 0),
                                0, 0, goldContentDepthTextHeight, 0, AttachmentPoint.MiddleRight));
                        }

                        //Отрисовываем само содержание
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(ApplyAutoCADFont($"{well.GoldDatas[j].goldContent}"),
                            new Point3d(basePoint.X + currentDist + distFromScale + 3, bottomHeight + (topHeight - bottomHeight) / 2,
                            0), 0, 0, goldContentTextHeight, 0, AttachmentPoint.MiddleLeft));

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
        private static void DrawEarthTypes(Point3d basePoint, Section section)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database bd = doc.Database;

            using (Transaction tr = bd.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bd.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                List<String> earthDatas = new List<String>();
                double currentDist = 0;

                for (int i = 0; i < section.Wells.Count; i++)
                {
                    Well well = section.Wells[i];

                    //Отрисовываем породы
                    foreach (EarthData earthData in well.EarthDatas)
                    {
                        if (!earthDatas.Contains(earthData.earthType))
                        {
                            earthDatas.Add(earthData.earthType);
                            double distForEarth = currentDist;
                            Point3dCollection earthSurface = new Point3dCollection();
                            //Отрисовываем экстраполяцию в начало если скважина - первая в буровой линии
                            if (i == 0)
                                earthSurface.Add(new Point3d(
                                    basePoint.X + distFromScale / 2 + distForEarth, basePoint.Y + GetHeightDifference(well.WellHeight - earthData.earthHeight), 0));
                            else
                                earthSurface.Add(new Point3d(
                                    basePoint.X + distFromScale + distForEarth - section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                    basePoint.Y + GetHeightDifference(well.WellHeight - earthData.earthHeight), 0));
                            //Отрисовываем первую точку в первой скважине
                            earthSurface.Add(new Point3d(
                                    basePoint.X + distFromScale + distForEarth, basePoint.Y + GetHeightDifference(well.WellHeight - earthData.earthHeight), 0));

                            bool isInterrupted = false;
                            distForEarth += well.DistanceToNextWell / horScale;
                            for (int k = i + 1; k < section.Wells.Count; k++)
                            {
                                int index = -1;
                                foreach(EarthData data in section.Wells[k].EarthDatas)
                                {
                                    if (data.earthType == earthData.earthType) index = section.Wells[k].EarthDatas.IndexOf(data);
                                }
                                if (index != -1)
                                {
                                    EarthData nextEarthData = section.Wells[k].EarthDatas[index];

                                    //При прерывании если находим в последующих скважинах нужную породу сначала добавляем точку посередине 2 скважин
                                    if (earthSurface.Count == 0)
                                        earthSurface.Add(new Point3d(
                                            basePoint.X + distFromScale + distForEarth - section.Wells[k - 1].DistanceToNextWell / horScale / 2,
                                            basePoint.Y + GetHeightDifference(section.Wells[k].WellHeight - nextEarthData.earthHeight), 0));
                                    //Добавляем найденную точку в коллекцию точек поверхности
                                    earthSurface.Add(new Point3d(
                                        basePoint.X + distFromScale + distForEarth,
                                        basePoint.Y + GetHeightDifference(section.Wells[k].WellHeight - nextEarthData.earthHeight), 0));

                                    if (k == section.Wells.Count - 1)
                                        earthSurface.Add(new Point3d(
                                            basePoint.X + distFromScale * 1.5 + distForEarth,
                                            basePoint.Y + GetHeightDifference(section.Wells[k].WellHeight - nextEarthData.earthHeight), 0));
                                }
                                else if (earthSurface.Count != 0)
                                {
                                    isInterrupted = true;

                                    earthSurface.Add(new Point3d(
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
                            AutoInitial.Initialize(tr, btr, new Spline(earthSurface, 5, 0.0));
                        }
                    }
                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }
                
                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает линии коренных пород по табличным данным
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawHardEarthTypes(Point3d basePoint, Section section)
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

                    double solidEarth = 0.0;
                    double destEarth = 0.0;

                    if (double.TryParse(solidEarthTemp, out solidEarth))
                    {
                        isThereNoSolidHardEarth = false;
                        if (solidEarthPoints.Count == 0)
                        {
                            if (i == 0)
                            {
                                solidEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale / 2 + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + solidEarth), 0));
                            }
                            else
                                solidEarthPoints.Add(new Point3d(
                                    basePoint.X + distFromScale + currentDist - section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                    basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + solidEarth), 0));
                        }
                        solidEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + solidEarth), 0));
                        if (i == section.Wells.Count - 1)
                        {
                            solidEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale * 1.5 + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + solidEarth), 0));
                        }
                    }
                    else if (solidEarthPoints.Count != 0)
                    {
                        solidEarthPoints.Add(new Point3d(
                                            solidEarthPoints[solidEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                            solidEarthPoints[solidEarthPoints.Count - 1].Y, 0));
                        AutoInitial.Initialize(tr, btr, new Spline(solidEarthPoints, 5, 0.0));
                        solidEarthPoints = new Point3dCollection();
                    }


                    if (double.TryParse(destEarthTemp, out destEarth))
                    {
                        if (destEarthPoints.Count == 0)
                        {
                            if (i == 0)
                            {
                                destEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale / 2 + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + destEarth + solidEarth), 0));
                            }
                            else
                                destEarthPoints.Add(new Point3d(
                                    basePoint.X + distFromScale + currentDist - section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                    basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + destEarth + solidEarth), 0));
                        }
                        destEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + destEarth + solidEarth), 0));
                        if (i == section.Wells.Count - 1)
                        {
                            destEarthPoints.Add(new Point3d(
                                        basePoint.X + distFromScale * 1.5 + currentDist,
                                        basePoint.Y + GetHeightDifference(well.WellHeight - well.WellDepth + destEarth + solidEarth), 0));
                        }
                    }
                    else if (destEarthPoints.Count != 0)
                    {
                        destEarthPoints.Add(new Point3d(
                                            destEarthPoints[destEarthPoints.Count - 1].X + section.Wells[i - 1].DistanceToNextWell / horScale / 2,
                                            destEarthPoints[destEarthPoints.Count - 1].Y, 0));
                        AutoInitial.Initialize(tr, btr, new Spline(destEarthPoints, 5, 0.0));
                        destEarthPoints = new Point3dCollection();
                    }


                    //Увеличиваем расстояние до скважины
                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }
                Spline solidEarthSurface = new Spline(solidEarthPoints, 5, 0.0);
                Spline destEarthSurface = new Spline(destEarthPoints, 5, 0.0);
                AutoInitial.Initialize(tr, btr, destEarthSurface);
                AutoInitial.Initialize(tr, btr, solidEarthSurface);

                if (solidEarthPoints.Count == section.Wells.Count + 2)
                {
                   
                    Point3dCollection SolidEarthSecondPoints = new Point3dCollection();
                    foreach (Point3d point in solidEarthPoints)
                    {
                        SolidEarthSecondPoints.Add(new Point3d(point.X, point.Y - 40, point.Z));
                    }
                    Spline solidEarthSecondSurface = new Spline(SolidEarthSecondPoints, 5, 0.0);
                    Line VerticalAtStart = AutoInitial.CreateLine(solidEarthPoints[0], SolidEarthSecondPoints[0]);
                    Line VerticalAtEnd = AutoInitial.CreateLine(solidEarthPoints[solidEarthPoints.Count - 1], SolidEarthSecondPoints[SolidEarthSecondPoints.Count - 1]);

                    AutoInitial.Initialize(tr, btr, solidEarthSecondSurface);
                    AutoInitial.Initialize(tr, btr, VerticalAtStart);
                    AutoInitial.Initialize(tr, btr, VerticalAtEnd);

                    //MessageBox.Show("I'm Here");
                    ObjectIdCollection ObjIds = new ObjectIdCollection()
                    {
                    solidEarthSurface.Id,
                    solidEarthSecondSurface.Id,
                    VerticalAtStart.Id,
                    VerticalAtEnd.Id
                    };

                    Hatch oHatch = new Hatch();
                    oHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                    AutoInitial.Initialize(tr, btr, oHatch);

                    oHatch.Associative = false;
                    oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                    oHatch.EvaluateHatch(true);

                    oHatch.PatternScale = 4;
                    oHatch.SetHatchPattern(oHatch.PatternType, oHatch.PatternName);

                    VerticalAtStart.Erase();
                    VerticalAtEnd.Erase();
                    solidEarthSecondSurface.Erase();

                    if (solidEarthPoints.Count == destEarthPoints.Count)
                    {
                        VerticalAtStart = AutoInitial.CreateLine(solidEarthPoints[0], destEarthPoints[0]);
                        VerticalAtEnd = AutoInitial.CreateLine(solidEarthPoints[solidEarthPoints.Count - 1], destEarthPoints[destEarthPoints.Count - 1]);

                        AutoInitial.Initialize(tr, btr, VerticalAtStart);
                        AutoInitial.Initialize(tr, btr, VerticalAtEnd);

                        ObjIds = new ObjectIdCollection()
                        {
                            solidEarthSurface.Id,
                            destEarthSurface.Id,
                            VerticalAtStart.Id,
                            VerticalAtEnd.Id
                        };

                        oHatch = new Hatch();
                        oHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                        AutoInitial.Initialize(tr, btr, oHatch);

                        oHatch.Associative = false;
                        oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                        oHatch.EvaluateHatch(true);

                        oHatch.PatternScale = 2;
                        oHatch.SetHatchPattern(oHatch.PatternType, oHatch.PatternName);

                        

                        VerticalAtStart.Erase();
                        VerticalAtEnd.Erase();
                    }
                }
                if ((destEarthPoints.Count == section.Wells.Count + 2) && isThereNoSolidHardEarth)
                {
                    Point3dCollection destEarthSecondPoints = new Point3dCollection();
                    foreach (Point3d point in destEarthPoints)
                    {
                        destEarthSecondPoints.Add(new Point3d(point.X, point.Y - 40, point.Z));
                    }
                    Spline destEarthSecondSurface = new Spline(destEarthSecondPoints, 5, 0.0);
                    Line VerticalAtStart = AutoInitial.CreateLine(destEarthPoints[0], destEarthSecondPoints[0]);
                    Line VerticalAtEnd = AutoInitial.CreateLine(destEarthPoints[destEarthPoints.Count - 1], destEarthSecondPoints[destEarthSecondPoints.Count - 1]);

                    AutoInitial.Initialize(tr, btr, destEarthSecondSurface);
                    AutoInitial.Initialize(tr, btr, VerticalAtStart);
                    AutoInitial.Initialize(tr, btr, VerticalAtEnd);

                    //MessageBox.Show("I'm Here");
                    ObjectIdCollection ObjIds = new ObjectIdCollection()
                    {
                    destEarthSurface.Id,
                    destEarthSecondSurface.Id,
                    VerticalAtStart.Id,
                    VerticalAtEnd.Id
                    };

                    Hatch oHatch = new Hatch();
                    oHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                    AutoInitial.Initialize(tr, btr, oHatch);

                    oHatch.Associative = false;
                    oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                    oHatch.EvaluateHatch(true);

                    oHatch.PatternScale = 4;
                    oHatch.SetHatchPattern(oHatch.PatternType, oHatch.PatternName);

                    VerticalAtStart.Erase();
                    VerticalAtEnd.Erase();
                    destEarthSecondSurface.Erase();
                }
                    doc.Editor.UpdateScreen();
                tr.Commit();
            }
        }

        /// <summary>
        /// Отрисовывает таблицу под разрезом, а также заполняет её данными скважин
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        /// <param name="section">Разрез, по скважинам которого будет происходить отрисовка</param>
        private static void DrawTable(Point3d basePoint, Section section)
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
                            ApplyAutoCADFont($"{well.WellName}"),
                            new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 0.20, 0),
                            0, 0, textHeight, 0, AttachmentPoint.TopCenter));
                    //Риска расстояния между скважинами
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist, 0),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 2, 0)));
                    //Надпись расстояния между скважинами
                    if (i != section.Wells.Count - 1)
                    {
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                        ApplyAutoCADFont(well.DistanceToNextWell.ToString("0.0").Replace('.', ',')),
                        new Point3d(basePoint.X + currentDist + distFromScale + well.DistanceToNextWell / 2 / horScale,
                        basePoint.Y - distFromTable - tableRowDist * 1.20, 0),
                        0, 0, textHeight, 0, AttachmentPoint.TopCenter));
                    }
                    //Прочие табличные данные
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                        ApplyAutoCADFont($"{well.WellDepth.ToString("0.0").Replace('.', ',')}\n" +
                        $"{well.SoftEarthThickness}\n" +
                        $"{well.DestHardEarthThickness}\n" +
                        $"{well.SolidHardEarthThickness}\n" +
                        $"{well.TurfThickness}\n" +
                        $"{well.GoldLayerThickness}\n" +
                        $"{well.GoldLayerContentSlip}\n" +
                        $"{well.VerticalGoldContent}\n"),
                        new Point3d(basePoint.X + currentDist + distFromScale, basePoint.Y - distFromTable - tableRowDist * 2.20, 0),
                        0, 0, textHeight, 0, TextSpacing, AttachmentPoint.TopCenter));

                    if (i != section.Wells.Count - 1)
                        currentDist += well.DistanceToNextWell / horScale;
                }

                //Отрисовываем границы таблицы
                for (int i = 0; i < 11; i++)
                {
                    AutoInitial.Initialize(tr, btr, AutoInitial.CreateLine(
                        new Point3d(basePoint.X - xTableOffset, basePoint.Y - distFromTable - tableRowDist * i, 0),
                        new Point3d(basePoint.X + currentDist + distFromScale * 2,
                        basePoint.Y - distFromTable - tableRowDist * i, 0)));
                }

                //Отрисовываем надписи (легенду) таблицы
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                    ApplyAutoCADFont("Номер разреза\n" +
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
                    0, 0, textHeight, 0, TextSpacing, AttachmentPoint.TopLeft));
                //Отрисовываем единицы измерения легенды
                AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(
                    ApplyAutoCADFont("м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "м\n" +
                    "г\\м{\\S2;}\n" +
                    "г\\м{\\S3;}\n"),
                    new Point3d(basePoint.X + 1, basePoint.Y - distFromTable - tableRowDist * 0.20, 0),
                    0, 0, textHeight, 0, TextSpacing,  AttachmentPoint.TopCenter));


                tr.Commit();
            }
        }

        /// <summary>
        /// Рисует шкалу вертикального масштаба относительно базовой точки
        /// </summary>
        /// <param name="basePoint">Точка, используемая как начало отсчета координат</param>
        private static void DrawScaleRuler(Point3d basePoint)
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
                    poly.AddVertexAt(0, new Point2d(basePoint.X, basePoint.Y + 10 * i), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(1, new Point2d(basePoint.X, basePoint.Y + 10 * (i + 1)), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(2, new Point2d(basePoint.X + 2, basePoint.Y + 10 * (i + 1)), 0.0, -1.0, -1.0);
                    poly.AddVertexAt(3, new Point2d(basePoint.X + 2, basePoint.Y + 10 * i), 0.0, -1.0, -1.0);
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
                        AutoInitial.Initialize(tr, btr, AutoInitial.CreateMtext(ApplyAutoCADFont($"{minHeight + vertScaleStep * i}"),
                            new Point3d(basePoint.X - 15, basePoint.Y + 10 * i, 0), 0, 0, textHeight, 0, AttachmentPoint.MiddleLeft));
                    }
                }
                tr.Commit();
            }
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
        private static string ApplyAutoCADFont(string text)
        {

            string fontChange = "{\\fTimes new Roman|b0|i0|c204|p18;";
            return fontChange + text + "}";
        }

        private static void CalculateMinMaxHeight(Section section)
        {
            minHeight = section.Wells[0].WellHeight - section.Wells[0].WellDepth;
            maxHeight = section.Wells[0].WellHeight;

            //Рассчитываем минимальную и максимальную высоту буровой линии
            foreach (Well well in section.Wells)
            {
                if (minHeight > well.WellHeight - well.WellDepth) minHeight = well.WellHeight - well.WellDepth;
                if (maxHeight < well.WellHeight) maxHeight = well.WellHeight;
            }

            minHeight = (minHeight % vertScaleStep) == 0 ? minHeight : minHeight - (minHeight % vertScaleStep);
            maxHeight = (vertScaleStep - (maxHeight % vertScaleStep))
                == 0 ? maxHeight : maxHeight + (vertScaleStep - (maxHeight % vertScaleStep));

            minHeight -= vertScaleStep * 3;
            maxHeight += vertScaleStep * 3;
        }

        private static void FormatSectionStrings(Section section)
        {
            foreach (Well well in section.Wells)
            {
                well.SoftEarthThickness = well.SoftEarthThickness.Trim().Replace(',', '.');
                well.DestHardEarthThickness = well.DestHardEarthThickness.Trim().Replace(',', '.');
                well.SolidHardEarthThickness = well.SolidHardEarthThickness.Trim().Replace(',', '.');
                well.TurfThickness = well.TurfThickness.Trim().Replace(',', '.');
                well.GoldLayerThickness = well.GoldLayerThickness.Trim().Replace(',', '.');
                well.GoldLayerContentSlip = well.GoldLayerContentSlip.Trim().Replace('.', ',');
                well.VerticalGoldContent = well.VerticalGoldContent.Trim().Replace(',', '.');

                if (double.TryParse(well.SoftEarthThickness,out double softEarthThickness))
                {
                    well.SoftEarthThickness = softEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.DestHardEarthThickness, out double destHardEarthThickness))
                {
                    well.DestHardEarthThickness = destHardEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.SolidHardEarthThickness, out double solidHardEarthThickness))
                {
                    well.SolidHardEarthThickness = solidHardEarthThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.TurfThickness, out double turfThickness))
                {
                    well.TurfThickness = turfThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.GoldLayerThickness, out double goldLayerThickness))
                {
                    well.GoldLayerThickness = goldLayerThickness.ToString("0.0").Replace('.', ',');
                }
                if (double.TryParse(well.GoldLayerContentSlip, out double goldLayerContentSlip))
                {
                    well.GoldLayerContentSlip = goldLayerContentSlip.ToString("0.000").Replace('.', ',');
                }
                if (double.TryParse(well.VerticalGoldContent, out double verticalGoldContent))
                {
                    well.VerticalGoldContent = verticalGoldContent.ToString("0.0").Replace('.', ',');
                }

                foreach(GoldData goldData in well.GoldDatas)
                {
                    if(double.TryParse(goldData.goldContent, out double goldContent))
                    {
                        goldData.goldContent = goldContent.ToString("0.000").Replace('.', ',');
                    }
                }
            }
        }


        public void Initialize()
        {
            
        }

        public void Terminate()
        {
            
        }
    }
}
