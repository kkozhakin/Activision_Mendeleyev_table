namespace Activision_Mendeleyev_table.HelperClasses
{
    class Energy
    {
        BinSystem system;
        double[,] energy = new double[19, 16];

        public Energy(BinSystem system)
        {
            this.system = system;

            double x1 = 0.05;
            double T = 0.20 * system.Tmax;

            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    energy[i, j] = system.Gsm(x1, T);

                    T += 0.05 * system.Tmax;
                }

                T = 0.20 * system.Tmax;
                x1 += 0.05;
            }
        }

        public void CountDiagram(out double x1, out double x2, out int T)
        {

            double min = double.MaxValue;

            x1 = 0.05;
            double t = 0.20;

            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (energy[i, j] < min)
                    {
                        min = energy[i, j];
                        x1 = 0.05 + i * 0.05;
                        t = 0.20 + 0.05 * j;
                    }
                }
            }

            if (min > 0)
            {
                x1 = -1;
                x2 = -1;
                T = 0;

                return;
            }

            x2 = -1;
            int indexOfRow = (int)((x1 - 0.05) / 0.05);
            int indexOfColumn = (int)((t - 0.20) / 0.05);
            min = double.MaxValue;

            for (int k = 0; k < 19; k++)
            {
                if (k != indexOfRow && energy[k, indexOfColumn] < min)
                {
                    x2 = 0.05 + k * 0.05;
                }
            }

            T = (int)(t * system.Tmax);
            x2 = 1 - x2;
        }

        public double this[int i, int j]
        {
            get { return energy[i, j]; }
        }
    }
}
