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
        /// Критерий max|f-y| оценки отклонения F(x) от точек
        /// </summary>
        /// <param name="tab">лист точек</param>
        /// <param name="F">функция</param>
        /// <param name="par">начальное значение параметров функции</param>
        /// <returns>значение отклонения</returns>
        public static double Criterion_MAX(List<Point> tab, Func<double, double[], double> F, double[] par)
        {
            double max = 0, f, s = 0;    
            foreach (Point mp in tab)
            {
                f = F(mp.X, par);
                s = Math.Abs(f - mp.Y);
                max = max > s ? max : s;
            }

            return max;
        }

        /// <summary>
        /// Критерий (sum|f-y|^2)/N оценки отклонения F(x) от точек
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
                sum += Math.Pow((f - mp.Y), 2);
            }

            return Math.Sqrt(sum / tab.Count);
        }
    }
}
