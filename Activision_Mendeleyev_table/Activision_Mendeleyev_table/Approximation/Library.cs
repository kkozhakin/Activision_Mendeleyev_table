using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;

namespace Activision_Mendeleyev_table.Approximation
{
    /// <summary>
    /// Класс, содержащий методы аппрокисимации функции
    /// </summary>
    public class Library
    {
        /// <summary>
        /// Метод аппроксимации набора точек заданной функцией (Function)
        /// </summary>
        /// <param name="tab">лист точек</param>
        /// <param name="Function">аппроксимирующая функция</param>
        /// <param name="Par">начальное значение параметров функции</param>
        /// <param name="ApproxiAccuracy">метод оценки точности аппроксимации</param>
        /// <returns>новый нобор параметров функции</returns>
        public static double[] AproxiTab(List<Point> tab, Func<double, double[], double> Function, double[] Par, 
            Func<List<Point>, Func<double, double[], double>, double[], double> ApproxiAccuracy)
        {
            // Локальный метод
            double funN(double[] par)
            {
                if (tab.Count == 0)
                    throw new ArgumentNullException("", new Exception("MyException"));
                double result = ApproxiAccuracy(tab, Function, par);
                return result;
            }

            double[] res = GradientMinimization(funN, Par, 1E-8, 1E-11, 10000);

            return res;
        }

        /// <summary>
        /// Вычисление градиента и направляющих вектора перемещения
        /// </summary>
        /// <param name="funN">исследуемая функция</param>
        /// <param name="X0">вектор параметров - иследуемая точка</param>
        /// <param name="del">относительная ??вариация??? каждого параметра</param>
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
        static double[] Quadmin(Func<double[], double> funN,
        double[] X0, double Delta = 1E-5, double Epsilon = 1E-7)
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
                P2[i] = P0[i] + 2.0 * H * S[i];
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
            double deltaF = (F1 - F2);
            return Q2;
        }
    }
}
