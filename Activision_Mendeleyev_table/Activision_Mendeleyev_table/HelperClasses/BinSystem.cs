using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, представляющий бинарную систему соединений
    /// </summary>
    public class BinSystem
    {
        /// <summary>
        /// Массив обозначений в таблицах данных
        /// </summary>
        public string[] symbols = new string[3] { "R(i)", "х", "ФЗ" };
        /// <summary>
        /// Универсальная газовая постоянная
        /// </summary>
        private const double kN = 1.9844 * 0.001;
        /// <summary>
        /// Константа Моделунга
        /// </summary>
        public double A = -1;
        /// <summary>
        /// Формальный заряд общего химического элемента
        /// </summary>
        public double zX = -1;
        /// <summary>
        /// Номер строки из таблицы данных первого химического элемента
        /// </summary>
        public int numA = 0;
        /// <summary>
        /// Номер строки из таблицы данных второго химического элемента
        /// </summary>
        public int numB = 0;
        /// <summary>
        /// Номер строки из таблицы данных общего химического элемента
        /// </summary>
        public int numX = 0;
        /// <summary>
        /// Обозначение системы соединений
        /// </summary>
        private readonly string sourceString;
        /// <summary>
        /// Первый химический элемент
        /// </summary>
        private Composition elemA;
        /// <summary>
        /// Второй химический элемент
        /// </summary>
        private Composition elemB;
        /// <summary>
        /// Общий химический элемент
        /// </summary>
        private Composition elemX;

        /// <summary>
        /// Эмпирический параметр
        /// </summary>
        private double c = -1;
        /// <summary>
        /// Число структурных единиц
        /// </summary>
        private double m = -1;
        /// <summary>
        /// Координационное число
        /// </summary>
        private double n = -1;
        /// <summary>
        /// Формальный заряд
        /// </summary>
        private double z = -1;
        /// <summary>
        /// Ионный радиус первого элемента
        /// </summary>
        private double r_1 = -1;
        /// <summary>
        /// Ионный радиус второго элемента
        /// </summary>
        private double r_2 = -1;
        /// <summary>
        /// Ионный радиус общего элемента
        /// </summary>
        private double r_3 = -1;
        /// <summary>
        /// Межатомное расстояние
        /// </summary>
        public double R_const = -1;
        /// <summary>
        /// Разность степеней ионности
        /// </summary>
        private double deleps = -1;

        /// <summary>
        /// Возвращает ионный радиус первого элемента
        /// </summary>
        public double r1
        {
            get
            {
                if (r_1 == -1)
                    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[numA], out r_1);

                return r_1;
            }
        }

        /// <summary>
        /// Возвращает ионный радиус второго элемента
        /// </summary>
        public double r2
        {
            get
            {
                if (r_2 == -1)
                    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[numB], out r_2);

                return r_2;
            }
        }

        /// <summary>
        /// Возвращает ионный радиус общего элемента
        /// </summary>
        public double r3
        {
            get
            {
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);

                return r_3;
            }
        }

        /// <summary>
        /// Межатомное расстояние первого соединения
        /// </summary>
        public double R1
        {
            get { return R(0); }
        }

        /// <summary>
        /// Межатомное расстояние второго соединения
        /// </summary>
        public double R2
        {
            get { return R(1); }
        }

        /// <summary>
        /// Обозначение первого химического элемента
        /// </summary>
        public string ElementA
        {
            get { return elemA.Name; }
        }

        /// <summary>
        ///  Обозначение второго химического элемента
        /// </summary>
        public string ElementB
        {
            get { return elemB.Name; }
        }

        /// <summary>
        ///  Обозначение общего химического элемента
        /// </summary>
        public string ElementX
        {
            get { return elemX.Name; }
        }

        /// <summary>
        /// Конструктор системы
        /// </summary>
        /// <param name="source">обозначение системы</param>
        /// <param name="A">первый химичский элемент</param>
        /// <param name="B">второй химичский элемент</param>
        /// <param name="X">общий химичский элемент</param>
        public BinSystem(string source, Composition A, Composition B, Composition X)
        {
            sourceString = source;
            elemA = A;
            elemB = B;
            elemX = X;
        }

        /// <summary>
        /// Позволяет задать параметры системы
        /// </summary>
        /// <param name="c">эмпирический параметр</param>
        /// <param name="m">число структурных единиц</param>
        /// <param name="n">координационное число</param>
        /// <param name="z">формальный заряд</param>
        public void SetData(double c, double m, double n, double z)
        {
            this.c = c;
            this.m = m;
            this.n = n;
            this.z = z;
        }

        /// <summary>
        /// Позволяет получить параметры системы
        /// </summary>
        /// <returns>массив параметров</returns>
        public double[] GetData() { return new double[] { c, m, n, z, numA, numB, numX }; }

        /// <summary>
        /// Энтропия смешения
        /// </summary>
        public double Ssm(double x1)
        {
            double x2 = 1 - x1;
            double Skon = (-1) * kN * (x1 * Math.Log(x1) + x2 * Math.Log(x2));
            double Skol = 2.725 * x1 * x2 * (delR / R1) * 0.001;

            return Skon + Skol;
        }

        /// <summary>
        /// Среднее межатомное расстояние
        /// </summary>
        public double R(double x1)
        {
            //if (R_const != -1)
            //    return R_const;

            //if (r_2 == -1)
            //    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[numB], out r_2);
            //if (r_3 == -1)
            //    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);
            //if (r_1 == -1)
            //    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[numA], out r_1);

            //if (r_1 != -1 && r_2 != -1 && r_3 != -1)
            //    return x1 * r_1 + (1 - x1) * r_2 + r_3;
            //else
            //    return -1;
            //return x1 * 2.774 + (1 - x1) * 2.819;
            return x1 * 2.054 + (1 - x1) * 1.966;
        }

        /// <summary>
        /// Разность радиусов
        /// </summary>
        public double delR
        {
            get
            {
                //if (r1 != -1 && r2 != -1)
                //    return Math.Abs(r_1 - r_2);
                //else
                //    return -1;
                //return 0.045;
                return 0.088;
            }
        }

        /// <summary>
        /// Степень ионности
        /// </summary>
        /// <param name="i">Флаг: 1 - элемент A, 2 - элемент B</param>
        public double Eps(int i)
        {

            Composition temp;
            int num;

            switch (i)
            {
                case 1:
                    temp = elemA;
                    num = numA;
                    break;
                case 2:
                    temp = elemB;
                    num = numB;
                    break;
                default:
                    return 0;
            }

            if (double.TryParse(temp.Properties.Find(x => x.First.Second == symbols[1]).Second[num], out double k)
                && double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[1]).Second[numX], out double j))
                return 1 - (z / n) * Math.Exp((k - j) * (k - j) * -0.25);
            else
                return -1;
        }

        /// <summary>
        /// Разность степеней ионности
        /// </summary>
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

        /// <summary>
        /// Теплота смешения
        /// </summary>
        public double Hsm(double x1)
        {
            if (zX == -1)
                double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[2]).Second[numX], out zX);

            return x1 * (1 - x1) * (322 * A / R(x1) * (delEps * delEps) + c * m * n * z * zX * (delR / R(x1) * delR / R(x1)));
        }

        /// <summary>
        /// Свободная энергия Гиббса
        /// </summary>
        /// <param name="T">температура</param>
        public double Gsm(double x1, double T)
        {
            return Hsm(x1) - T * Ssm(x1);
        }

        /// <summary>
        /// Критическая температура
        /// </summary>
        public double Tmax
        {
            get
            {
                if (zX == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[2]).Second[numX], out zX);

                return c * m * n * z * zX * (delR / Math.Min(R1, R2) * (delR / Math.Min(R1, R2))) / (2 * kN);
            }
        }

        /// <summary>
        /// Возвращает обозначение системы
        /// </summary>
        /// <returns>обозначение системы</returns>
        public override string ToString()
        {
            return sourceString;
        }

        /// <summary>
        /// Создает копию системы
        /// </summary>
        /// <returns>копия данной системы</returns>
        public BinSystem Clone()
        {
            BinSystem toClone = new BinSystem(sourceString, new Composition(elemA.Name, elemA.DataTable, elemA.Properties), new Composition(elemB.Name, elemB.DataTable, elemB.Properties),
                new Composition(elemX.Name, elemX.DataTable, elemX.Properties));
            toClone.SetData(c, m, n, z);
            toClone.numA = numA;
            toClone.numB = numB;
            toClone.numX = numX;
            toClone.A = A;

            return toClone;
        }
    }
}
