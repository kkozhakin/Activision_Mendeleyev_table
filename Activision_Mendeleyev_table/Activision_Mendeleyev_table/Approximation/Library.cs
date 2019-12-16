using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;

namespace Activision_Mendeleyev_table.Approximation
{
    public class Library
    {
        // Метод аппроксимации набора точек заданной функцией (Function):
        public static double[] AproxiTab(List<Point> tab, // набор точек
        Func<double, double[], double> Function,// аппроксимирующая функция
        double[] Par, // начальное значение параметров функции
        // метод оценки точности аппроксимации:
        Func<List<Point>, Func<double, double[], double>, double[], double> ApproxiAccuracy)
        {
            // Локальный метод: ////////////////////////////////
            double funN(double[] par)
            {
                if (tab.Count == 0)
                    throw new ArgumentNullException();
                double result = ApproxiAccuracy(tab, Function, par);
                return result;
            }
            double[] res = GradientMinimization(funN, Par, 1E-8, 1E-11, 10000);

            return res;
        } // AproxiTab()

        // // ///////////////////////////////////////////////////////
        // Вычисление градиента и направляющих вектора перемещения
        // Возвращает вектор перемещения вдоль градиента
        public static double[] Gradient(Func<double[], double> funN,
        double[] X0, // вектор параметров - иследуемая точка
        double del = 0.001) // Относительная ??вариация??? каждого параметра
        //double del=0.00001) // Относительная ??вариация??? каждого параметра
        //double del = 0.01) // Относительная ??вариация??? каждого параметра
        {
            // Console.WriteLine(" Gradient");///////TEST///////////////
            int NC = X0.Length; // Количество параметров
            double[] G = new double[NC]; // Координаты градиента - частные производные
            double[] dx = new double[NC]; // Относительные вариации параметров
            for (int j = 0; j < NC; j++) // dx[] всегда +
                if (X0[j] == 0)
                    dx[j] = del;
                else
                    dx[j] = Math.Abs(X0[j] * del);

            double[] V = new double[NC]; // текущий вектор параметров
            X0.CopyTo(V, 0);
            double Fma, Fmi, dFi; // значения функционала в двух точках
            for (int j = 0; j < NC; j++)
            { // Изменяем параметры
                V[j] = X0[j] + dx[j];
                Fma = funN(V);
                V[j] = X0[j] - dx[j];
                Fmi = funN(V);
                dFi = Fma - Fmi;
                G[j] = dFi / (2 * dx[j]); // Оценка производной
                V[j] = X0[j]; // Восстановить исходный вектор параметров
            }

            // unit vector along gradient:
            double[] S = new double[NC];    
            double Dlina = 0; // Размер (длина) градиента
            for (int j = 0; j < NC; j++)
                Dlina += Math.Pow(G[j], 2);

            Dlina = Math.Sqrt(Dlina);
            for (int j = 0; j < NC; j++)
                S[j] = -G[j] / Dlina;

            return S;
        } // Gradient(

        /// ///////////////////////////////////

        // Метод QUADMIN - возвращает параметры (точку) минимума вдоль градиента
        static double[] Quadmin(Func<double[], double> funN,
        double[] X0, // Начальный вектор параметров
        double Delta = 1E-5, double Epsilon = 1E-7)
        {
            // Console.WriteLine(" Quadmin 1");///////TEST////////////////////
            int NC = X0.Length; // number of coordinates
            double Y0 = funN(X0);// Критерий в начальной точке
            double[] P0 = new double[NC];
            X0.CopyTo(P0, 0);
            double[] P1 = new double[NC];
            double[] P2 = new double[NC];      
            double[] S = new double[NC]; // Направляющие векторы градиента
            double H = 1.0; // Относительная величина шага вдоль градиента
            double Err = 1.0;
            int Jmax = 20; //////////////////
            double H0, H1, H2, Hmin, E0, E1, E2, Y1, Y2, D, Ymin;
            int i;
            int Cond = 0; // КАК ИСПОЛЬЗОВАТЬ ЗНАЧЕНИЕ вне метода???...............
            int J = 0;
            ////////////////////////////////////
        
            S = Gradient(funN, X0); // Направляющие векторы градиента
            // Console.WriteLine(" Quadmin 2");///////TEST////////////////////
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
                //if ( Y0 <= Y1 ) /* Make H smaller ERROR!!!*/
                if (Y0 < Y1) /* Make H smaller CORRECT!!! Исправил 20.07.2018 */
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
            } // конец while
            // Console.WriteLine("H={0}",H);/////////////////////
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

            Ymin = funN(Pmin); //

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
            //Console.WriteLine("J={0}",J);////////////////////////////////////
            return Pmin;
        } // Quadmin(

        /////////////////////////////////////////////
        /* **************************************** */
        //.. Метод наискорейшего спуска (метод градиентной минимизации)
        // Возвращает оптимальный вектор параметров (не функционал):
        public static double[] GradientMinimization(Func<double[], double> funN,
        double[] X0, // Начальный вектор параметров
        double Delta = 1E-8, // Tolerance for interval width
        double Epsilon = 1E-11, // Tolerance for |f(b) - f(a)|
        int Max = 100) // Maximum number of iterations
        {
            int NC = X0.Length; // number of coordinates for the parameter point        
            double[] Q1 = new double[NC];
            double[] Q2 = new double[NC];
            double F1, F2;
            int iter = 0;
            double deltaX = 0;
            X0.CopyTo(Q2, 0);

            do
            {
                iter++;
                //if(iter > 6) { ///////TEST////////////////////
                //Console.WriteLine("Контрольная печать до Quadmin, iter={0}", iter ); }////////////
                Q2.CopyTo(Q1, 0);
                F1 = funN(Q1);
                Q2 = Quadmin(funN, Q1);
                //if(iter > 6) { ///////TEST////////////////////
                //Console.WriteLine("Контрольная печать ПОСЛЕ Quadmin, iter={0}", iter ); }
                F2 = funN(Q2);
                // нашли точку минимума Q2 при смещении от Q1 вдоль антиградиента
                // необходимо оценить невязки: и повторить итерации
                deltaX = 0; // максимальное относительное изменение параметра
                for (int k = 0; k < NC; k++)
                {
                    double ZN = Math.Abs(Q2[k]);
                    if (ZN > 0.0)
                    {
                        double DR = Math.Abs((Q2[k] - Q1[k]) / ZN);
                        deltaX = DR > deltaX ? DR : deltaX;
                    }
                } // for (int k
                ////Console.WriteLine("iter={0} F2 = {1}", iter, F2);/////////TEST////////////
            }

            while (iter < Max & Math.Abs(F1 - F2) > Epsilon & deltaX > Delta);
            if (iter >= Max) ;//////////////////////////////////////
            // {
            //string Error = "\n* * * Количество итераций превысило Max = {0} * * *\n";
            //Console.WriteLine(Error, Max); }
            //else Console.WriteLine("Количество выполненных итераций: " + iter);
            //////////////////////////////////////////////////////////////////////////////////////////
            double deltaF = (F1 - F2);
            //MessageBox.Show("Количество выполненных итераций: " + iter + "\ndeltaX = " +
            // deltaX.ToString("E4") + "\ndeltaF =" + deltaF.ToString("E5"),
            // "Градиентный метод" ); // ОТЛАДКА****************
            ///////////////////////////////////////
            return Q2;
        } // GradientMinimization(
    } // class Library
}
