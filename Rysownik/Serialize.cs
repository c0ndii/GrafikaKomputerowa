using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Rysownik
{
    [Serializable]
    public class ShapeSerialize
    {
    }

    [Serializable]
    public class LineSerialize : ShapeSerialize
    {
        public double X1 {  get; set; }
        public double Y1 { get; set; }
        public double X2 {  get; set; }
        public double Y2 { get; set; }
    }

    [Serializable]
    public class RectangleSerialize : ShapeSerialize
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    [Serializable]
    public class CircleSerialize : ShapeSerialize
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Diameter { get; set; }
    }

    [Serializable]
    [XmlInclude(typeof(LineSerialize))]
    [XmlInclude(typeof(RectangleSerialize))]
    [XmlInclude(typeof(CircleSerialize))]
    public class CanvasShapes
    {
        public List<ShapeSerialize> Shapes { get; set; } = new();
    }
}
