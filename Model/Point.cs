using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MathModule.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellCalculations2010.Model
{
    public class Point : ICloneable
    {
        public string Id;
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point()
        {
            Id = string.Empty;
            Name = string.Empty;
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Point(double X, double Y, double Z)
        {
            Id = string.Empty;
            Name = string.Empty;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }      

        public static implicit operator Point3d(Point point)
        {
            return new Point3d(point.X, point.Y, point.Z);
        }
        public static implicit operator PolylineVertex3d(Point point)
        {
            return new PolylineVertex3d(point);
        }
        public static implicit operator MathPoint(Point point)
        {
            return new MathPoint(point.X, point.Y, point.Z);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
