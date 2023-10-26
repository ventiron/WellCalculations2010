using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Windows;


namespace TriangulationAutoCAD
{
    class AutoInitial
    {
        // Функции для автоматической инициализации объектов в AutoCAD
        // НЕ ОТРИСОВЫВАЮТ, НЕ ЗАНОСЯТ В БАЗЫ ДАННЫХ, ТОЛЬКО ИНИЦИАЛИЗИРУЮТ

        #region [CreateLine]
        public static Line CreateLine(Point3d firstPoint, Point3d secondPoint, byte color = 0, LineWeight lineWeight = LineWeight.ByLayer)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.ColorIndex = color <= 0 ? line.ColorIndex : color;
            line.LineWeight = lineWeight;

            return line;

        }
        public static Line CreateLine(Point3d firstPoint, Point3d secondPoint, Color color)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.Color = color;

            return line;

        }
        public static Line CreateLine(Point3d firstPoint, Point3d secondPoint, LayerTableRecord layer)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.Layer = layer.Name;

            return line;

        }
        public static Line CreateLine(Matrix3d matrix, Point3d firstPoint, Point3d secondPoint, byte color = 0)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.TransformBy(matrix);
            line.ColorIndex = color <= 0 ? line.ColorIndex : color;
            return line;

        }
        public static Line CreateLine(Matrix3d matrix, Point3d firstPoint, Point3d secondPoint, Color color)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.TransformBy(matrix);
            line.Color = color;

            return line;

        }
        public static Line CreateLine(Matrix3d matrix, Point3d firstPoint, Point3d secondPoint, LayerTableRecord layer)
        {
            Line line = new Line(firstPoint, secondPoint);
            line.SetDatabaseDefaults();
            line.TransformBy(matrix);
            line.Layer = layer.Name;

            return line;

        }
        #endregion

        #region [CreateMtext]
        public static MText CreateMtext(string content, Point3d location, byte color = 0, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.TopLeft)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.ColorIndex = color <= 0 ? text.ColorIndex : color;
            text.Attachment = atPoint;
            text.LineSpaceDistance = lineSpacing == 0d ? text.LineSpaceDistance : lineSpacing;

            return text;
        }
        public static MText CreateMtext(string content, Point3d location, Color color, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.BaseAlign)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.Color = color;
            text.Attachment = atPoint;

            return text;
        }
        public static MText CreateMtext(string content, Point3d location, LayerTableRecord layer, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.BaseAlign)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.Layer = layer.Name;
            text.Attachment = atPoint;


            return text;
        }
        public static MText CreateMtext(Matrix3d matrix, string content, Point3d location, byte color = 0, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.BaseAlign)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.ColorIndex = color <= 0 ? text.ColorIndex : color;
            text.Attachment = atPoint;
            text.TransformBy(matrix);

            return text;
        }
        public static MText CreateMtext(Matrix3d matrix, string content, Point3d location, Color color, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.BaseAlign)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.Color = color;
            text.Attachment = atPoint;
            text.TransformBy(matrix);

            return text;
        }
        public static MText CreateMtext(Matrix3d matrix, string content, Point3d location, LayerTableRecord layer, double textHeight = 0d, double rotation = 0, double lineSpacing = 0d, AttachmentPoint atPoint = AttachmentPoint.BaseAlign)
        {
            MText text = new MText();
            text.SetDatabaseDefaults();
            text.Location = location;
            text.Contents = content;
            text.TextHeight = textHeight == 0d ? text.TextHeight : textHeight;
            text.Rotation = rotation;
            text.Layer = layer.Name;
            text.Attachment = atPoint;
            text.TransformBy(matrix);

            return text;
        }
        #endregion

        #region [CreateDBPoint]
        public static DBPoint CreateDBPoint(Point3d location)
        {
            DBPoint point = new DBPoint(location);
            point.SetDatabaseDefaults();
            return point;
        }
        public static DBPoint CreateDBPoint(Matrix3d matrix, Point3d location)
        {
            DBPoint point = new DBPoint(location);
            point.SetDatabaseDefaults();
            point.TransformBy(matrix);
            return point;
        }
        public static DBPoint CreateDBPoint(double x, double y, double z = 0)
        {
            DBPoint point = new DBPoint(new Point3d(x, y, z));
            point.SetDatabaseDefaults();
            return point;
        }
        public static DBPoint CreateDBPoint(Matrix3d matrix, double x, double y, double z = 0)
        {
            DBPoint point = new DBPoint(new Point3d(x, y, z));
            point.SetDatabaseDefaults();
            point.TransformBy(matrix);
            return point;
        }
        #endregion

        #region [CreateCircle]
        public static Circle CreateCircle(Point3d point)
        {
            Circle circle = new Circle();
            circle.Center = point;
            circle.Radius = 1.0d;
            circle.SetDatabaseDefaults();
            return circle;
        }
        public static Circle CreateCircle(Point3d point, double radius)
        {
            Circle circle = new Circle();
            circle.Center = point;
            circle.Radius = radius;
            circle.SetDatabaseDefaults();
            return circle;
        }
        public static Circle CreateCircle(Point3d point, Vector3d vector, double radius)
        {
            Circle circle = new Circle();
            circle.Center = point;
            circle.Radius = radius;
            circle.Normal = vector;
            circle.SetDatabaseDefaults();
            return circle;
        }
        #endregion

        // Отрисовывает любые доступные для этого объекты
        public static void Initialize(Transaction tr, BlockTableRecord btr, Entity en, bool add = true)
        {
            btr.AppendEntity(en);
            tr.AddNewlyCreatedDBObject(en, add);
        }
    }
}
