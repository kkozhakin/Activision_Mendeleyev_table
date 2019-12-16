using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    public class BinSystem
    {
        //array of table symbols
        public string[] symbols = new string[2] { "R(i)", "х" };
        const double kN = 1.9844 * 0.001;
        double _constA;  
        bool _useConstA = false;
        double c = -1;
        double m = -1;
        double n = -1;
        double z = -1;
        double zX = -1;
        double r_1 = -1;
        double r_2 = -1;
        double r_3 = -1;
        public double R_const = -1;
        double deleps = -1;

        string sourceString;

        Composition elemA;
        Composition elemB;
        Composition elemX;

        public double r1
        {
            get
            {
                if (r_1 == -1)
                    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_1);
 
                return r_1;
            }

            set
            {
                r_1 = value;
            }
        }

        public double r2
        {
            get
            {
                if (r_2 == -1)
                    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_2);

                return r_2;
            }

            set
            {
                r_2 = value;
            }
        }

        public double r3
        {
            get
            {
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_3);

                return r_3;
            }

            set
            {
                r_3 = value;
            }
        }

        public double R1
        {
            get
            {
                if (r_1 == -1)
                    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_1);
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_3);

                if (r_3 != -1 && r_1 != -1)
                    return r_1 + r_3;
                else
                    return -1;
            }
        }

        public double R2
        {
            get
            {
                if (r_2 == -1)
                    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_2);
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_3);

                if (r_3 != -1 && r_2 != -1)
                    return r_2 + r_3;
                else
                    return -1;
            }
        }

        public string elementA
        {
            get { return elemA.Name; }
        }

        public string elementB
        {
            get { return elemB.Name; }
        }

        public string elementX
        {
            get { return elemX.Name; }
        }

        public BinSystem(string source, Composition A, Composition B, Composition X)
        {
            sourceString = source;
            elemA = A;
            elemB = B;
            elemX = X;
        }

        public void setData(double c, double m, double n, double z, double zX)
        {
            this.c = c;
            this.m = m;
            this.n = n;
            this.z = z;
            this.zX = zX;
        }

        public double[] getData() { return new double[] { c, m, n, z, zX }; }

        public double Ssm(double x1)
        {
            double x2 = 1 - x1;
            double Skon = (-1) * kN * (x1 * Math.Log(x1) + x2 * Math.Log(x2));
            double Skol = 2.7252 * x1 * x2 * (delR / R1) * 0.001;

            return Skon + Skol;
        }

        public double R(double x1)
        {
            if (R_const != -1)
                return R_const;

            if (r_2 == -1)
                double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_2);
            if (r_3 == -1)
                double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_3);
            if (r_1 == -1)
                double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[0], out r_1);

            if (r_1 != -1 && r_2 != -1 && r_3 != -1)
                return x1 * r_1 + (1 - x1) * r_2 + r_3;
            else
                return -1;
        }

        public double A
        {
            get
            {
                if (_useConstA)
                    return _constA;

                double alf = ((-1 + Math.Sqrt(1 + (4 * 1.81) / (n * n))) * (n * n)) / 2;

                return (alf * m * z * zX) / 2;
            }
        }

        public double delR
        {
            get
            {
                if (r1 != -1 && r2 != -1)
                    return Math.Abs(r_1 - r_2);
                else
                    return -1;
            }
        }

        public double Eps(int i)
        {

            Composition temp;

            switch (i)
            {
                case 1: temp = elemA; break;
                case 2: temp = elemB; break;
                default: return 0;
            }
      
            if (double.TryParse(temp.Properties.Find(x => x.First.Second == symbols[1]).Second[0], out double k)
                && double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[1]).Second[0], out double j))
                return 1 - (z / n) * Math.Exp((k - j) * (k - j) * -0.25);
            else
                return -1;
        }

        public double delEps
        {
            get
            {
                if (deleps < 0)
                    deleps = Math.Abs(Eps(1) - Eps(2));

                return deleps;
            }

            set
            {
                deleps = value;
            }
        }

        public double Hsm(double x1)
        {
            double x2 = 1 - x1;

            double first = x1 * x2 * ((322 * A) / R(x1)) * (delEps * delEps);
            double second = x1 * x2 * (c * m * n * z * zX *
                ((delR / R(x1)) * (delR / R(x1))));

            return first + second;
        }

        public double Gsm(double x1, double T)
        {
            return Hsm(x1) - T * Ssm(x1);
        }

        public double Tmax
        {
            get { return (c * m * n * z * zX * ((delR / R1) * (delR / R1))) / (2 * kN); }
        }

        public void setA(bool flag, double value)
        {
            _useConstA = flag;
            _constA = value;
        }

        public bool useA
        {
            get { return _useConstA; }
        }

        public override string ToString()
        {
            return sourceString;
        }

        public BinSystem Clone()
        {
            BinSystem toClone = new BinSystem(sourceString, new Composition(elemA.Name, elemA.DataTable, elemA.Properties), new Composition(elemB.Name, elemB.DataTable, elemB.Properties),
                new Composition(elemX.Name, elemX.DataTable, elemX.Properties));
            toClone.setData(c, m, n, z, zX);

            return toClone;
        }
    }
}
