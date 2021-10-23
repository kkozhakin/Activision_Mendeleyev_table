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
        /// Карандаш теоритического соотношения
        /// </summary>
        private static Pen pen = Pens.Black;
        /// <summary>
        /// Карандаш экспериментального соотношения
        /// </summary>
        private static Pen penExp = Pens.Red;
        /// <summary>
        /// Карандаш аппроксримированного соотношения
        /// </summary>
        private static Pen penApp = Pens.Green;
        /// <summary>
        /// Точки эксперимента
        /// </summary>
        private static List<PointF> experiment = new List<PointF>();
        /// <summary>
        /// Порверхность для рисования
        /// </summary>
        private Graphics g;
        /// <summary>
        /// Система соединений
        /// </summary>
        private BinSystem system;
        /// <summary>
        /// Ширина поля для диаграммы
        /// </summary>
        private readonly int width;
        /// <summary>
        /// Точки правого соединения(теория)
        /// </summary>
        private Point[] right;
        /// <summary>
        /// Точки левого соединения(теория)
        /// </summary>
        private Point[] left;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="g">поверхность для рисования</param>
        /// <param name="system">система соединений</param>
        /// <param name="width">ширина поля для диаграммы</param>
        public CollapseGraph(Graphics g, BinSystem system, int width)
        {
            this.width = width - 80;
            this.g = g;
            this.system = system;
        }

        /// <summary>
        /// Свойство цвета теоритического соотношения
        /// </summary>
        public static Color Color
        {
            get { return pen.Color; }
            set { pen = new Pen(value, 3); }
        }

        /// <summary>
        /// Свойство цвета экспериментального соотношения
        /// </summary>
        public static Color ExperimentColor
        {
            get { return penExp.Color; }
            set { penExp = new Pen(value, 3); }
        }

        /// <summary>
        /// Свойство цвета аппроксимированного соотношения
        /// </summary>
        public static Color ApproximationColor
        {
            get { return penApp.Color; }
            set { penApp = new Pen(value, 3); }
        }

        /// <summary>
        /// Рисует купол распада
        /// </summary>
        /// <param name="f">флаг: true - аппроксимация, false - теория</param>
        public void DrawCollapse(bool f = false)
        {
            Collapse collapse = new Collapse(system);
            UpTemp = UpTemp == -1 ? (int)system.Tmax + 10 : UpTemp;
            DownTemp = DownTemp == -1 ? (int)(0.20 * system.Tmax) : DownTemp;

            right = new Point[collapse.right.Length];
            for (int i = 0; i < right.Length; i++)
            {
                int x = 80 + (int)(width * (1 - collapse.right[i].X));
                int y = width - 40 - (int)(width * ((collapse.right[i].Y * system.Tmax - DownTemp) / (UpTemp - DownTemp)));
                y = y > width ? width : y;

                right[i] = new Point(x, y);
            }

            left = new Point[collapse.left.Length];
            for (int i = 0; i < left.Length; i++)
            {
                int x = 80 + (int)(width * collapse.left[i].X);
                int y = width - 40 - (int)(width * ((collapse.left[i].Y * system.Tmax - DownTemp) / (UpTemp - DownTemp)));
                y = y > width ? width : y;

                left[i] = new Point(x, y);
            }

            g.DrawString("T, °С", new Font("X", 14), Brushes.Black, new Point(80, 0));
            if (f)
            {
                g.DrawLines(penApp, right);
                g.DrawLines(penApp, left);
            }
            else
            {
                g.DrawLines(pen, right);
                g.DrawLines(pen, left);
            }         
        }

        /// <summary>
        /// Рисует термодинамическую функцию смешения
        /// </summary>
        /// <param name="f">флаг: true - аппроксимация, false - теория</param>
        public void DrawDH(bool f = true)
        {
            DownTemp = DownTemp == -1 ? 0 : DownTemp;
            UpTemp = UpTemp == -1 ? (int)(system.Hsm(0.5) * 1000) + 100 : UpTemp;

            Point[] dh = new Point[21];
            for (double i = 0; i < 1; i += 0.05)
                dh[(int)Math.Round(i * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((system.Hsm(i) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
            dh[20] = new Point(80 + width, width - 40 - (int)((system.Hsm(0.999999) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
            g.DrawString("ΔHcм, ккал/моль", new Font("X", 14), Brushes.Black, new Point(80, 0));

            if (f)
                g.DrawLines(penApp, dh);
            else
                g.DrawLines(pen, dh);

        }

        /// <summary>
        /// Рисует график свободной энергии Гиббса
        /// </summary>
        /// <param name="tempD">нижняя граница температуры</param>
        /// <param name="tempU">верхняя граница температуры</param>
        /// <param name="tempInt">шаг температуры</param>
        public void DrawDG(int tempD, int tempU, int tempInt, double k1 = 1, double k2 = 1)
        {
            DownTemp = DownTemp == -1 ? 0 : DownTemp;
            UpTemp = UpTemp == -1 ? (int)(system.Hsm(0.5) * 1000) + 100 : UpTemp;

            Point[] dh = new Point[21];
            for (double i = 0; i < 1; i += 0.05)
                dh[(int)Math.Round(i * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((system.Hsm(i) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
            dh[20] = new Point(80 + width, width - 40 - (int)((system.Hsm(0.999999) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
            g.DrawLines(penApp, dh);

            Point[] dg = new Point[11];
            Point[] ds = new Point[21];

            for (int t = tempD; t < tempU; t += tempInt)
            {
                for (double i = 0.00001; i < 0.5; i += 0.05)
                {
                    ds[(int)Math.Round(i * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((-t * system.Ssm(i) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                    dg[(int)Math.Round(i * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((system.Gsm(i, t, k1) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                }
                dg[10] = new Point(80 + (int)(0.5 * width), width - 40 - (int)((system.Gsm(0.5, t, k1) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                g.DrawLines(pen, dg);
                dg = new Point[11];
                for (double i = 0.5; i < 1; i += 0.05)
                {
                    ds[(int)Math.Round(i * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((-t * system.Ssm(i) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                    dg[(int)Math.Round((i - 0.5) * 20)] = new Point(80 + (int)(i * width), width - 40 - (int)((system.Gsm(i, t, k2) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                }
                dg[10] = new Point(80 + width, width - 40 - (int)((system.Gsm(0.99999, t, k2) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                ds[20] = new Point(80 + width, width - 40 - (int)((-t * system.Ssm(0.99999) * 1000 - DownTemp) / (UpTemp - DownTemp) * width));
                g.DrawLines(pen, dg);
                g.DrawLines(penExp, ds);
            }

            g.DrawString("ΔGcм, ккал/моль", new Font("X", 14), Brushes.Black, new Point(80, 0)); 
        }

        /// <summary>
        /// Рисует оси координат
        /// </summary>
        public void DrawAxes(bool f = false)
        {
            g.DrawString(system.ElementA + system.ElementX, new Font("X", 14), Brushes.Black, new Point(80, width + 30));
            g.DrawString(system.ElementB + system.ElementX, new Font("X", 14), Brushes.Black, new Point(width - 100, width + 30));

            g.DrawLine(Pens.Black, 80, 0, 80, width + 30);
            g.DrawLine(Pens.Black, 30, width - 40, width + 80, width - 40);

            for (double x = 0; x <= 1; x += 0.1)
            {
                g.DrawString(x.ToString(), new Font("X", 12), Brushes.Black, 80 + (float)(width * x), width - 30);
                g.DrawLine(Pens.Black, 80 + (int)(width * x), width - 45, 80 + (int)(width * x), width - 35);
            }
            g.DrawString("1", new Font("X", 12), Brushes.Black, width + 65, width - 30);

            double c = Math.Round((UpTemp - DownTemp) / 100.0);
            if (c == 0)
                c = 1;
            if (c < (UpTemp - DownTemp) / 100)
                c = (c + 1) * 10;
            else
                c *= 10;

            if (f)
                for (double x = DownTemp; x <= UpTemp; x += c)
                {
                    g.DrawString(ToC(x).ToString(), new Font("X", 12), Brushes.Black, 0, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width));
                    g.DrawLine(Pens.Black, 75, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width), 85, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width));
                }
            else
                for (double x = DownTemp; x <= UpTemp; x += c)
                {
                    g.DrawString(x.ToString(), new Font("X", 12), Brushes.Black, 0, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width));
                    g.DrawLine(Pens.Black, 75, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width), 85, width - 40 - (int)((x - DownTemp) / (UpTemp - DownTemp) * width));
                }
        }

        /// <summary>
        /// Рисует эксперимент
        /// </summary>
        /// <param name="f">флаг: true - купол распада, false - теплота смешения</param>
        public void DrawExperiment(bool f = true)
        {
            List<Point> left = new List<Point>();
            List<Point> right = new List<Point>();

            foreach (var item in experiment)
            {
                int x = 80 + (int)(item.X * width);
                int y;
                if (f)
                    y = width - 40 - (int)((ToK(item.Y) - DownTemp) / (UpTemp - DownTemp) * width);
                else
                    y = width - 40 - (int)((item.Y - DownTemp) / (UpTemp - DownTemp) * width);
                y = y > width ? width : y;

                if (item.X <= 0.5)
                    left.Add(new Point(x, y));
                else
                    right.Add(new Point(x, y));
            }

            foreach (var item in left)
            {
                g.FillEllipse(penExp.Brush, item.X - 8, item.Y - 8, 16, 16);
            }
            foreach (var item in right)
            {
                g.FillEllipse(penExp.Brush, item.X - 8, item.Y - 8, 16, 16);
            }
        }

        /// <summary>
        /// Свойство верхней границы температуры(графика по Y)
        /// </summary>
        public static int UpTemp { get; set; } = -1;

        /// <summary>
        /// Свойство нижней границы температуры(графика по Y)
        /// </summary>
        public static int DownTemp { get; set; } = -1;

        /// <summary>
        /// Добавляет точку в эксперимент 
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="t">координата Y</param>
        public static void AddExperimentalPoint(double x, double t)
        {
            experiment.Add(new PointF((float)x, (float)t));
        }

        /// <summary>
        /// Удаляет выбранную точку из эксперимента
        /// </summary>
        public static void RemoveSelectedPoint(int i)
        {
            if (experiment.Count > i)
                experiment.RemoveAt(i);
        }

        /// <summary>
        /// Удаляет все точки из эксперимента
        /// </summary>
        public static void ClearExperiment()
        {
            experiment.Clear();
        }

        /// <summary>
        /// Перевод температуры из градусов Кельвина в Цельсия
        /// </summary>
        /// <param name="t">температура в градусах Кельвина</param>
        /// <returns>температура в градусах Цельсия</returns>
        public static double ToC(double t) { return t - 273; }

        /// <summary>
        /// Перевод температуры из градусов Цельсия в Кельвина
        /// </summary>
        /// <param name="t">температура в градусах Цельсия</param>
        /// <returns>температура в градусах Кельвина</returns>
        public static double ToK(double t) { return t + 273; }
    }
}
