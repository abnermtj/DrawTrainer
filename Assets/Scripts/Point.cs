using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FitEllipse
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class PointCollection : List<Point>
    {
    }
}