using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = System.Drawing.Point;

namespace Activision_Mendeleyev_table.DrawingClasses
{
    class Diagram
    {
        Collapse collapse;
        Energy energy;
        BinSystem system;
        int a;
        Graphics g;

        List<Point> diagram = new List<Point>();

        public Diagram(Graphics g, BinSystem system, int wight)
        {
            this.system = system.Clone();
            collapse = new Collapse(this.system);
            energy = new Energy(this.system);
            this.a = wight;
            this.g = g;

            double x1, x2;
            int T;

            energy.CountDiagram(out x1, out x2, out T);

            if (x1 == -1)
            {
                throw new Exception("Энергия одного знака!");
            }

            int x, y;

            for (int i = 0; i < collapse.left.Length; i++)
            {
                if (collapse.left[i].X > x1 || (collapse.left[i].Y * system.Tmax > T))
                {
                    x = 30 + (int)(a * (x1));
                    y = a - (int)(a * ((T - 0.20 * system.Tmax) / (0.8 * system.Tmax)));

                    diagram.Add(new Point(x, y));

                    break;
                }

                x = 30 + (int)(a * (collapse.left[i].X));
                y = a - (int)(a * ((collapse.left[i].Y - 0.20) / 0.8));


                diagram.Add(new Point(x, y));
            }

            bool flag = true;

            for (int i = collapse.right.Length - 1; i >= 0; i--)
            {
                if ((1 - collapse.right[i].X) < (1 - x2) || (collapse.right[i].Y * system.Tmax > T))
                {
                    if (flag)
                    {
                        x = 30 + (int)(a * (1 - x2));
                        y = a - (int)(a * ((T - 0.20 * system.Tmax) / (0.8 * system.Tmax)));

                        diagram.Add(new Point(x, y));

                        flag = false;
                    }

                    continue;
                }

                x = 30 + (int)(a * (1 - collapse.right[i].X));
                y = a - (int)(a * ((collapse.right[i].Y - 0.20) / 0.8));

                diagram.Add(new Point(x, y));
            }
        }

        public void DrawDiagram()
        {
            if (diagram.Count > 1)
                g.DrawLines(Pens.Black, diagram.ToArray());
        }

        public void DrawAxes()
        {
            g.DrawString(string.Format("{0:f0}", (int)(system.Tmax)), new Font("X", 8), Brushes.Black, new Point(0, 0));
            g.DrawString(string.Format("{0:f0}", (int)(system.Tmax * 0.2)), new Font("X", 8), Brushes.Black, new Point(0, a));

            string left = system.ToString().Split('-')[0];
            string right = system.ToString().Split('-')[1];

            g.DrawString(left, new Font("X", 14), Brushes.Black, new Point(30, a + 20));
            g.DrawString(right, new Font("X", 14), Brushes.Black, new Point(a - 30, a + 20));

            g.DrawLine(Pens.Black, 30, 0, 30, a + 30);
            g.DrawLine(Pens.Black, 0, a, a + 30, a);

            for (double x = 0.1; x <= 0.5; x += 0.10)
            {
                g.DrawString(x.ToString(), new Font("X", 8), Brushes.Black, 20 + (float)(a * x), (float)a + 6);
                g.DrawString(x.ToString(), new Font("X", 8), Brushes.Black, 20 + (float)(a * (1 - x)), (float)a + 6);

                g.DrawLine(Pens.Black, 30 + (int)(a * x), a - 5, 30 + (int)(a * x), a + 5);
                g.DrawLine(Pens.Black, 30 + (int)(a * (1 - x)), a - 5, 30 + (int)(a * (1 - x)), a + 5);
            }
        }
    }
}
