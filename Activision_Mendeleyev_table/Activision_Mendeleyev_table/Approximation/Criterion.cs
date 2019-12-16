using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;

namespace Activision_Mendeleyev_table.Approximation
{
    public static class Criterion
    {
        // Критерий max|f-y| оценки отклонения F(x) от точек:
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
        } // Criterion_MAX

        // Критерий (sum|f-y|^2)/N оценки отклонения F(x) от точек:
        public static double Criterion_CKO(List<Point> tab, Func<double, double[], double> F, double[] par)
        {
            double f, sum = 0;
            foreach (Point mp in tab)
            {
                f = F(mp.X, par);
                sum += Math.Pow((f - mp.Y), 2);
            }

            return Math.Sqrt(sum / tab.Count);
        } // Criterion_CKO
    }
}
