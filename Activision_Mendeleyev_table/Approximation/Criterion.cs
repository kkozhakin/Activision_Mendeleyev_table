using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;

namespace Activision_Mendeleyev_table.Approximation
{
    /// <summary>
    /// Класс, содержащий различные критерии оценки отклонения функции
    /// </summary>
    public static class Criterion
    {
        /// <summary>
        /// Критерий sum((f-y)^2)/N оценки отклонения F(x) от точек
        /// </summary>
        /// <param name="tab">лист точек</param>
        /// <param name="F">функция</param>
        /// <param name="par">начальное значение параметров функции</param>
        /// <returns>значение отклонения</returns>
        public static double Criterion_CKO(List<Point> tab, Func<double, double[], double> F, double[] par)
        {
            double f, sum = 0;
            foreach (Point mp in tab)
            {
                f = F(mp.X, par);
                sum += Math.Pow(f - mp.Y, 2);
            }

            return Math.Sqrt(sum / tab.Count);
        }

        /// <summary>
        /// Штрафная функция
        /// </summary>
        /// <param name="par_lims">допустимая погрешность параметров</param>
        /// <param name="par_0">начальное значение параметров функции</param>
        /// <param name="par_new">текущее значение параметров функции</param>
        /// <returns>значение штрафа</returns>
        public static double PenaltyF(List<double> par_lims, double[] par_0, double[] par_new)
        {
            double sum = 0;
            for (int i = 0; i < par_lims.Count; i++)
                if (Math.Abs(par_0[i] - par_new[i]) > par_lims[i] || par_lims[i] == 0)
                    sum += Math.Abs(par_lims[i] - par_new[i]) * 1000;

            return sum;
        }

        /// <summary>
        /// Вычисление расстояния между одним набором точек и другим
        /// </summary>
        /// <param name="Dots_1">первый набор точек</param>
        /// <param name="Dots_2">второй набор точек</param>
        /// <returns>расстояние</returns>
        public static double Dots_Distance(List<Point> Dots_1, List<Point> Dots_2)
        {
            double min_sum = 0;

            for (int i = 0; i < Dots_1.Count; i++)
            {
                double min = -1;
                for (int j = 0; j < Dots_2.Count; j++)
                    if (min == -1 || Math.Sqrt((Dots_2[j].X - Dots_1[i].X) * (Dots_2[j].X - Dots_1[i].X) + (Dots_2[j].Y - Dots_1[i].Y) * (Dots_2[j].Y - Dots_1[i].Y)) < min)
                        min = Math.Sqrt((Dots_2[j].X - Dots_1[i].X) * (Dots_2[j].X - Dots_1[i].X) + (Dots_2[j].Y - Dots_1[i].Y) * (Dots_2[j].Y - Dots_1[i].Y));

                min_sum += min;
            }

            return min_sum;
        }
    }
}
