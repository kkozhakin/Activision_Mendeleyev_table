using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = System.Drawing.Point;

namespace Activision_Mendeleyev_table.DrawingClasses
{
    /// <summary>
    /// Класс, отрисовывающий фазовую диаграмму
    /// </summary>
    class CollapseGraph
    {
        /// <summary>
        /// Цвет теоритического соотношения
        /// </summary>
        static Pen pen = Pens.Black;
        /// <summary>
        /// Цвет экспериментального соотношения
        /// </summary>
        static Pen penExp = Pens.Red;
        /// <summary>
        /// Цвет аппроксримированного соотношения
        /// </summary>
        static Pen penApp = Pens.Green;
        /// <summary>
        /// Точки эксперимента
        /// </summary>
        static List<PointF> experiment = new List<PointF>();
        /// <summary>
        /// Если true показывает эксперимент точками, иначе - ломаными
        /// </summary>
        static bool experimetnIsPoints = true;

        /// <summary>
        /// Верхняя граница температуры(графика по Y)
        /// </summary>
        static int upT = -1;
        /// <summary>
        /// Нижняя граница температуры(графика по Y)
        /// </summary>
        static int downT = -1;

        /// <summary>
        /// Порверхность для рисования
        /// </summary>
        Graphics g;
        /// <summary>
        /// Система соединений
        /// </summary>
        BinSystem system;
        /// <summary>
        /// Ширина поля для диаграммы
        /// </summary>
        int width;

        /// <summary>
        /// Точки правого соединения(теория)
        /// </summary>
        Point[] right;
        /// <summary>
        /// Точки левого соединения(теория)
        /// </summary>
        Point[] left;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="g">поверхность для рисования</param>
        /// <param name="system">система соединений</param>
        /// <param name="width">ширина поля для диаграммы</param>
        public CollapseGraph(Graphics g, BinSystem system, int width)
        {
            this.width = width - 30;
            this.g = g;
            this.system = system;
        }

        /// <summary>
        /// Свойство цвета теоритического соотношения
        /// </summary>
        static public Color Color
        {
            get { return pen.Color; }
            set { pen = new Pen(value); }
        }

        /// <summary>
        /// Свойство цвета экспериментального соотношения
        /// </summary>
        static public Color ExperimentColor
        {
            get { return penExp.Color; }
            set { penExp = new Pen(value); }
        }

        /// <summary>
        /// Свойство цвета аппроксимированного соотношения
        /// </summary>
        static public Color ApproximationColor
        {
            get { return penApp.Color; }
            set { penApp = new Pen(value); }
        }

        /// <summary>
        /// Рисует купол распада
        /// </summary>
        public void DrawCollapse()
        {
            Collapse collapse = new Collapse(system);
            upT = upT == -1 ? (int)system.Tmax : upT;
            downT = downT == -1 ? (int)(0.20 * system.Tmax) : downT;

            right = new Point[collapse.right.Length];
            for (int i = 0; i < right.Length; i++)
            {
                int x = 30 + (int)(width * (1 - collapse.right[i].X));
                int y = width - (int)(width * ((collapse.right[i].Y * system.Tmax - downT) / (upT - downT)));
                y = y > width ? width : y;

                right[i] = new Point(x, y);
            }

            left = new Point[collapse.left.Length];
            for (int i = 0; i < left.Length; i++)
            {
                int x = 30 + (int)(width * collapse.left[i].X);
                int y = width - (int)(width * ((collapse.left[i].Y * system.Tmax - downT) / (upT - downT)));
                y = y > width ? width : y;

                left[i] = new Point(x, y);
            }


            g.DrawLines(pen, right);
            g.DrawLines(pen, left);
        }

        /// <summary>
        /// Рисует термодинамическую функцию смешения
        /// </summary>
        /// <param name="f">флаг: true - аппроксимация, false - теория</param>
        public void DrawDH(bool f = true)
        {
            downT = downT == -1 ? 0 : downT;
            upT = upT == -1 ? (int)(system.Hsm(0.5) * 1000) + 50 : upT;

            Point[] dh = new Point[21];
            for (double i = 0; i < 1; i += 0.05)
                dh[(int)Math.Round(i * 20)] = new Point(30 + (int)(i * width), width - (int)(((system.Hsm(i) * 1000 - downT) / (upT - downT)) * width));

            dh[20] = new Point(30 + width, width);
            if (f)
                g.DrawLines(penApp, dh);
            else
                g.DrawLines(pen, dh);
        }

        /// <summary>
        /// Рисует оси координат
        /// </summary>
        public void DrawAxes()
        {
            g.DrawString(string.Format("{0:f0}", upT), new Font("X", 8), Brushes.Black, new Point(0, 0));
            g.DrawString(string.Format("{0:f0}", downT), new Font("X", 8), Brushes.Black, new Point(0, width));

            g.DrawString(system.elementA + system.elementX, new Font("X", 14), Brushes.Black, new Point(30, width + 20));
            g.DrawString(system.elementB + system.elementX, new Font("X", 14), Brushes.Black, new Point(width - 20, width + 20));

            g.DrawLine(Pens.Black, 30, 0, 30, width + 30);
            g.DrawLine(Pens.Black, 0, width, width + 30, width);

            for (double x = 0; x <= 1; x += 0.1)
            {
                g.DrawString(1 - x < 0.1 ? "0" : x <= 0.5 ? x.ToString() : (1 - x).ToString(), new Font("X", 8), Brushes.Black, 
                    20 + (float)(width * x), width + 6);
                g.DrawLine(Pens.Black, 30 + (int)(width * x), width - 5, 30 + (int)(width * x), width + 5);
            }
        }

        /// <summary>
        /// Рисует эксперимент
        /// </summary>
        public void DrawExperiment()
        {
            List<Point> left = new List<Point>();
            List<Point> right = new List<Point>();

            foreach (var item in experiment)
            {

                int x = 30 + (int)(item.X * width);
                int y = width - (int)(((item.Y - downT) / (upT - downT)) * width);

                y = y > width ? width : y;

                if (item.X <= 0.5)
                    left.Add(new Point(x, y));
                else
                    right.Add(new Point(x, y));
            }

            if (!experimetnIsPoints)
            {
                Point[] arrLeft = left.ToArray();
                Point[] arrRight = right.ToArray();

                Array.Sort(arrLeft, (x, y) => (x.X.CompareTo(y.X)));
                Array.Sort(arrRight, (x, y) => (x.X.CompareTo(y.X)));

                if (left.Count > 1)
                    g.DrawLines(penExp, arrLeft);

                if (right.Count > 1)
                    g.DrawLines(penExp, arrRight);
            }
            else
            {
                foreach (var item in left)
                {
                    g.FillEllipse(penExp.Brush, item.X - 2, item.Y - 2, 4, 4);
                }
                foreach (var item in right)
                {
                    g.FillEllipse(penExp.Brush, item.X - 2, item.Y - 2, 4, 4);
                }
            }
        }

        /// <summary>
        /// Свойство, определяющее вид отрисовки эксперимента
        /// </summary>
        static public bool ExperimentIsPoints
        {
            get { return experimetnIsPoints; }
            set { experimetnIsPoints = value; }
        }

        /// <summary>
        /// Свойство верхней границы температуры(графика по Y)
        /// </summary>
        static public int UpTemp
        {
            get { return upT; }
            set { upT = value; }
        }

        /// <summary>
        /// Свойство нижней границы температуры(графика по Y)
        /// </summary>
        static public int DownTemp
        {
            get { return downT; }
            set { downT = value; }
        }

        /// <summary>
        /// Добавляет точку в экспиремент 
        /// </summary>
        /// <param name="x1">координата X</param>
        /// <param name="t">координата Y</param>
        public static void addExperimentalPoint(double x1, double t)
        {
            experiment.Add(new PointF((float)x1, (float)t));
        }

        /// <summary>
        /// Удаляет последнюю точку из эксперимента
        /// </summary>
        public static void removeLastPoint()
        {
            if (experiment.Count > 0)
                experiment.RemoveAt(experiment.Count - 1);
        }

        /// <summary>
        /// Удаляет все точки из эксперимента
        /// </summary>
        public static void clearExperiment()
        {
            experiment.Clear();
        }
    }
}
