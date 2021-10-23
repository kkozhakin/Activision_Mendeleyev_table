using System;
using System.Linq;
using Point = Activision_Mendeleyev_table.HelperClasses.Point;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Activision_Mendeleyev_table.DrawingClasses;

namespace Activision_Mendeleyev_table.Approximation
{
    /// <summary>
    /// Класс, содержащий методы аппроксимации функции
    /// </summary>
    public class Library
    {
        /// <summary>
        /// Метод аппроксимации набора точек заданной функцией
        /// </summary>
        /// <param name="tab">лист точек</param>
        /// <param name="Function">аппроксимирующая функция</param>
        /// <param name="Par">начальное значение параметров функции</param>
        /// <param name="ApproxiAccuracy">метод оценки точности аппроксимации</param>
        /// <param name="Par_lims">погрешности параметров</param>
        /// <param name="Penalty">штрафная функция</param>
        /// <returns>новый нобор параметров функции</returns>
        public static double[] AproxiTab(List<Point> tab, Func<double, double[], double> Function, double[] Par,
            Func<List<Point>, Func<double, double[], double>, double[], double> ApproxiAccuracy, List<double> Par_lims = null, Func<List<double>, double[], double[], double> Penalty = null)
        {

            double funN(double[] par)
            {
                if (tab.Count == 0)
                    throw new ArgumentNullException("", new Exception("MyException"));

                if (Penalty != null)
                    return ApproxiAccuracy(tab, Function, par) + Penalty(Par_lims, Par, par);
                else
                    return ApproxiAccuracy(tab, Function, par);
            }

            double[] a = GradientMinimization(funN, Par, 1E-8, 1E-11, 10000);
            return a;
        }

        /// <summary>
        /// Вычисление градиента и направляющих вектора перемещения
        /// </summary>
        /// <param name="funN">исследуемая функция</param>
        /// <param name="X0">вектор параметров - иследуемая точка</param>
        /// <param name="del">относительная вариация каждого параметра</param>
        /// <returns>вектор перемещения вдоль градиента</returns>
        public static double[] Gradient(Func<double[], double> funN, double[] X0, double del = 0.001)
        //double del=0.00001)
        //double del = 0.01)
        {
            int NC = X0.Length;
            double[] G = new double[NC];
            double[] dx = new double[NC];
            for (int j = 0; j < NC; j++)
                if (X0[j] == 0)
                    dx[j] = del;
                else
                    dx[j] = Math.Abs(X0[j] * del);

            double[] V = new double[NC];
            X0.CopyTo(V, 0);
            double Fma, Fmi, dFi;
            for (int j = 0; j < NC; j++)
            {
                V[j] = X0[j] + dx[j];
                Fma = funN(V);
                V[j] = X0[j] - dx[j];
                Fmi = funN(V);
                dFi = Fma - Fmi;
                G[j] = dFi / (2 * dx[j]);
                V[j] = X0[j];
            }

            // unit vector along gradient:
            double[] S = new double[NC];
            double len = 0;
            for (int j = 0; j < NC; j++)
                len += Math.Pow(G[j], 2);

            len = Math.Sqrt(len);
            for (int j = 0; j < NC; j++)
                S[j] = -G[j] / len;

            return S;
        }

        /// <summary>
        /// Вычисление минимума вдоль градиента
        /// </summary>
        /// <param name="funN">исследуемая функция</param>
        /// <param name="X0">начальный вектор параметров</param>
        /// <param name="Delta">допустимое отклонение для ширины интервала</param>
        /// <param name="Epsilon">допустимое отклонение для |f(b) - f(a)|</param>
        /// <returns>параметры (точка) минимума вдоль градиента</returns>
        private static double[] Quadmin(Func<double[], double> funN, double[] X0, double Delta = 1E-5, double Epsilon = 1E-7)
        {
            int NC = X0.Length;
            double Y0 = funN(X0);
            double[] P0 = new double[NC];
            X0.CopyTo(P0, 0);
            double[] P1 = new double[NC];
            double[] P2 = new double[NC];
            double[] S = new double[NC];
            double H = 1.0;
            double Err = 1.0;
            int Jmax = 20;
            double H0, H1, H2, Hmin, E0, E1, E2, Y1, Y2, D, Ymin;
            int i;
            int Cond = 0;
            int J = 0;

            S = Gradient(funN, X0);
            for (i = 0; i < NC; i++)
            {
                P1[i] = P0[i] + H * S[i];
                P2[i] = P0[i] + 2 * H * S[i];
            }

            Y1 = funN(P1);
            Y2 = funN(P2);
            double[] Pmin = new double[NC];
            while ((J < Jmax) & (Cond == 0))
            {
                if (Y0 < Y1) /* Make H smaller */
                {
                    Y2 = Y1;
                    H = H / 2.0;
                    for (i = 0; i < NC; i++)
                    {
                        P2[i] = P1[i];
                        P1[i] = P0[i] + H * S[i];
                    }
                    Y1 = funN(P1);
                }
                else
                {
                    if (Y2 < Y1) /* Make H larger */
                    {
                        Y1 = Y2;
                        H = 2.0 * H;
                        for (i = 0; i < NC; i++)
                        {
                            P1[i] = P2[i];
                            P2[i] = P0[i] + 2.0 * H * S[i];
                        }
                        Y2 = funN(P2);
                    }
                    else Cond = -1;
                }
            }

            if (H < Delta) Cond = 1;
            D = 4.0 * Y1 - 2.0 * Y0 - 2.0 * Y2;
            /* Quadratic interpolation to find Hmin */
            if (D < 0)
                Hmin = H * (4.0 * Y1 - 3.0 * Y0 - Y2) / D;
            else /* check division by zero */
            {
                Cond = 4;
                Hmin = H / 3.0;
            }

            for (i = 0; i < NC; i++)
                Pmin[i] = P0[i] + Hmin * S[i];

            Ymin = funN(Pmin);

            /* Convergence test for the points */
            H0 = Math.Abs(Hmin);
            H1 = Math.Abs(Hmin - H);
            H2 = Math.Abs(Hmin - 2.0 * H);
            if (H0 < H) H = H0;
            if (H1 < H) H = H1;
            if (H2 < H) H = H2;
            if (H < Delta) Cond = 1;

            /* Convergence test for the function values */
            E0 = Math.Abs(Y0 - Ymin);
            E1 = Math.Abs(Y1 - Ymin);
            E2 = Math.Abs(Y2 - Ymin);
            if (E0 < Err)
                Err = E0;
            else if (E1 < Err)
                Err = E1;
            else if (E2 < Err)
                Err = E2;
            else if ((E0 == 0) && (E1 == 0) && (E2 == 0))
                Err = 0;

            if (Err < Epsilon)
                Cond = 2;
            if ((Cond == 2) && (H < Delta))
                Cond = 3;

            J++;
            return Pmin;
        }

        /// <summary>
        /// Метод наискорейшего спуска (метод градиентной минимизации)
        /// </summary>
        /// <param name="funN">исследуемая функция</param>
        /// <param name="X0">начальный вектор параметров</param>
        /// <param name="Delta">допустимое отклонение для ширины интервала</param>
        /// <param name="Epsilon">допустимое отклонение для |f(b) - f(a)|</param>
        /// <param name="Max">максимальное число итераций</param>
        /// <returns>оптимальный вектор параметров</returns>
        public static double[] GradientMinimization(Func<double[], double> funN, double[] X0, double Delta = 1E-8, double Epsilon = 1E-11, int Max = 100)
        {
            int NC = X0.Length;
            double[] Q1 = new double[NC];
            double[] Q2 = new double[NC];
            double F1, F2;
            int iter = 0;
            double deltaX = 0;
            X0.CopyTo(Q2, 0);

            do
            {
                iter++;
                Q2.CopyTo(Q1, 0);
                F1 = funN(Q1);
                Q2 = Quadmin(funN, Q1);
                F2 = funN(Q2);
                deltaX = 0;
                for (int k = 0; k < NC; k++)
                {
                    double ZN = Math.Abs(Q2[k]);
                    if (ZN > 0.0)
                    {
                        double DR = Math.Abs((Q2[k] - Q1[k]) / ZN);
                        deltaX = DR > deltaX ? DR : deltaX;
                    }
                }
            }
            while (iter < Max & Math.Abs(F1 - F2) > Epsilon & deltaX > Delta);

            return Q2;
        }

        /// <summary>
        /// Метод аппроксимации набора точек аналитическими выражениями, описывающими купол распада
        /// </summary>
        /// <param name="sys">данные системы</param>
        /// <param name="dat">набор точек</param>
        /// <param name="t">критическая температура</param>
        /// <returns>оптимальный набор параметров</returns>
        public static double[] DomeApproxi(List<List<double>> dat, int t, HelperClasses.BinSystem sys)
        {
            System.Windows.Resources.StreamResourceInfo ri = Application.GetResourceStream(new Uri("DrawingClasses/Collapse.xml", UriKind.Relative));
            Stream data = ri.Stream;
            System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(data);

            List<Point> ExpDots = new List<Point>();
            List<Point> Dots = new List<Point>();
            foreach (List<double> point in dat)
                ExpDots.Add(new Point(point[0], point[1]));

            double r1 = sys.r_1, r2 = sys.r_2, r3 = sys.r_3, x1 = sys.x_1, x3 = sys.x_3;
            string r = "", r_old = "1";
            double min = 1000000, min_t = 1000000;
            double[] par_min = new double[] { sys.r_1, sys.r_2, sys.r_3, sys.x_1, sys.x_3 };
            for (r1 = sys.r_1 - 0.02; r1 <= sys.r_1 + 0.02; r1 += 0.002)
                if (r1 > 0)
                    for (r2 = sys.r_2 - 0.02; r2 <= sys.r_2 + 0.02; r2 += 0.002)
                        if (r2 > 0)
                            for (r3 = sys.r_3 - 0.02; r3 <= sys.r_3 + 0.02; r3 += 0.002)
                                if (r3 > 0)
                                    for (x1 = sys.x_1 - 0.02; x1 <= sys.x_1 + 0.02; x1 += 0.002)
                                        if (x1 > 0)
                                            for (x3 = sys.x_3 - 0.02; x3 <= sys.x_3 + 0.02; x3 += 0.002)
                                                if (x3 > 0)
                                                {
                                                    if (r != r_old)
                                                    {
                                                        r = Collapse.GetRatio(Math.Abs(r1 - r2) / Math.Min(r1 + r3, r2 + r3));
                                                        string[] x1values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("x1").Value.Split(';');
                                                        string[] x2values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("x2").Value.Split(';');
                                                        string[] y1values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("y1").Value.Split(';');
                                                        string[] y2values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("y2").Value.Split(';');

                                                        Dots.Clear();
                                                        for (int i = 0; i < x1values.Length; i++)
                                                            Dots.Add(new Point(double.Parse(x1values[i]), double.Parse(y1values[i]) * t));

                                                        for (int i = 0; i < x2values.Length; i++)
                                                            Dots.Add(new Point(double.Parse(x2values[i]), double.Parse(y2values[i]) * t));

                                                        r_old = r;
                                                    }
                                                    double _t = (33.33 * (1 - (sys.z / sys.n) * Math.Exp((x1 - x3) * (x1 - x3) * -0.25)) + 8.83) *
                                                        sys.m * sys.n * sys.z * sys.zX *
                                                        Math.Pow(Math.Abs(r1 - r2) / Math.Min(r1 + r3, r2 + r3), 2) / (1.9844 * 0.002);
                                                    if (Math.Abs(t - _t) < min_t)
                                                    {
                                                        min_t = Math.Abs(t - _t);
                                                        if (min == -1 || Criterion.Dots_Distance(ExpDots, Dots) <= min)
                                                        {
                                                            min = Criterion.Dots_Distance(ExpDots, Dots);
                                                            par_min[0] = r1;
                                                            par_min[1] = r2;
                                                            par_min[2] = r3;
                                                            par_min[3] = x1;
                                                            par_min[4] = x3;
                                                        }
                                                    }
                                                }
            return par_min;
        }
    }
}
