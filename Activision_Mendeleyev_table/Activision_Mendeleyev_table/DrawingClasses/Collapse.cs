using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Activision_Mendeleyev_table.DrawingClasses
{
    class Collapse
    {
        public Point[] right;
        public Point[] left;

        public Collapse(BinSystem system)
        {
            string r1 = GetRatio(system.delR / system.r1);
            string r2 = GetRatio(system.delR / system.r2);

            System.Windows.Resources.StreamResourceInfo ri = System.Windows.Application.GetResourceStream(new Uri("DrawingClasses/Collapse.xml", UriKind.Relative));
            System.IO.Stream data = ri.Stream;

            XDocument doc = XDocument.Load(data);
            string[] x1values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r1).Element("x1").Value.Split(';');
            string[] x2values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r2).Element("x2").Value.Split(';');

            right = new Point[x1values.Length];
            left = new Point[x2values.Length];

            right[0] = new Point(double.Parse(x1values[0]), 0.20);

            double t = 0.30;
            for (int i = 1; i < x1values.Length; i++)
            {
                right[i] = new Point(double.Parse(x1values[i]), t);
                t += 0.05;
            }

            left[0] = new Point(double.Parse(x2values[0]), 0.20);

            t = 0.30;
            for (int i = 1; i < x2values.Length; i++)
            {
                left[i] = new Point(double.Parse(x2values[i]), t);
                t += 0.05;
            }
        }

        string GetRatio(double ratio)
        {
            ratio = Math.Round(ratio, 3);
            if ((ratio <= 0.05) || ((ratio > 0.05) && (ratio < 0.075))) return "0,05";
            else
                if ((ratio <= 0.1) || ((ratio > 0.1) && (ratio < 0.125))) return "0,10";
            else
                    if ((ratio <= 0.15) || ((ratio > 0.15) && (ratio < 0.175))) return "0,15";
            else
                        if ((ratio <= 0.20) || ((ratio > 0.20) && (ratio < 0.225))) return "0,20";
            else
                            if ((ratio <= 0.25) || ((ratio > 0.25) && (ratio < 0.275))) return "0,25";
            else
                                if ((ratio <= 0.30) || ((ratio > 0.30) && (ratio < 0.325))) return "0,30";
            else throw new Exception("Недопустимое отношение радиусов!");
        }
    }
}
