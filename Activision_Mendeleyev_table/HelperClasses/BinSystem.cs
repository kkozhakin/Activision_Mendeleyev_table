using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, представляющий бинарную систему соединений
    /// </summary>
    public class BinSystem
    {
        /// <summary>
        /// Универсальная газовая постоянная
        /// </summary>
        private const double kN = 1.9844 * 0.001;
        /// <summary>
        /// Константа Моделунга
        /// </summary>
        public double A { private set; get; }
        /// <summary>
        /// Формальный заряд общего химического элемента
        /// </summary>
        public double zX { private set; get; }
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
        /// Число структурных единиц
        /// </summary>
        public double m { private set; get; }
        /// <summary>
        /// Координационное число
        /// </summary>
        public double n { private set; get; }
        /// <summary>
        /// Формальный заряд
        /// </summary>
        public double z { private set; get; }
        /// <summary>
        /// Ионный радиус первого элемента
        /// </summary>
        public double r_1 = -1;
        /// <summary>
        /// Ионный радиус второго элемента
        /// </summary>
        public double r_2 = -1;
        /// <summary>
        /// Ионный радиус общего элемента
        /// </summary>
        public double r_3 = -1;
        /// <summary>
        /// Электроотрицательность первого элемента
        /// </summary>
        public double x_1 = -1;
        /// <summary>
        /// Электроотрицательность второго элемента
        /// </summary>
        public double x_2 = -1;
        /// <summary>
        /// Электроотрицательность общего элемента
        /// </summary>
        public double x_3 = -1;

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
        public BinSystem(string source, Composition A, Composition B, Composition X, double n = -1, double a = -1, double m = -1, double z = -1, double zX = -1)
        {
            sourceString = source;
            elemA = A;
            elemB = B;
            elemX = X;
            this.n = n;
            this.m = m;
            this.A = a;
            this.z = z;
            this.zX = zX;
        }

        /// <summary>
        /// Эмпирический параметр
        /// </summary>
        public double c
        {
            get
            {
                return 33.33 * Eps(1) + 8.83;
            }
        }

        /// <summary>
        /// Энтропия смешения
        /// </summary>
        public double Ssm(double x)
        {
            return (-1) * kN * (x * Math.Log(x) + (1 - x) * Math.Log(1 - x)) + 2.7250 * x * (1 - x) * delR / Math.Min(R(1), R(0));
        }

        /// <summary>
        /// Среднее межатомное расстояние
        /// </summary>
        public double R(double x)
        {
            return x * r_1 + (1 - x) * r_2 + r_3;  
        }

        /// <summary>
        /// Разность радиусов
        /// </summary>
        public double delR
        {
            get
            {
                return Math.Abs(r_1 - r_2);
            }
        }

        /// <summary>
        /// Степень ионности
        /// </summary>
        /// <param name="i">Флаг: 1 - элемент A, 2 - элемент B</param>
        public double Eps(int i)
        {
            if (i == 1)
                return 1 - (z / n) * Math.Exp((x_1 - x_3) * (x_1 - x_3) * -0.25);
            else if (i == 2)
                return 1 - (z / n) * Math.Exp((x_2 - x_3) * (x_2 - x_3) * -0.25);
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
                return Math.Abs(Eps(1) - Eps(2));
            }
        }

        /// <summary>
        /// Теплота смешения
        /// </summary>
        public double Hsm(double x)
        {
            return x * (1 - x) * (322 * A / R(x) * (delEps * delEps) + c * m * n * z * zX * (delR / R(x) * delR / R(x)));
        }

        /// <summary>
        /// Свободная энергия Гиббса
        /// </summary>
        /// <param name="T">температура</param>
        public double Gsm(double x, double T, double k = 1)
        {
            return k * Hsm(x) - T * Ssm(x);
        }

        /// <summary>
        /// Критическая температура
        /// </summary>
        public double Tmax
        {
            get
            {
                return c * m * n * z * zX * Math.Pow(delR / Math.Min(R(1), R(0)), 2) / (2 * kN);
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
            BinSystem toClone = new BinSystem(sourceString, new Composition(elemA.Name, elemA.DataTable, elemA.Properties),
                new Composition(elemB.Name, elemB.DataTable, elemB.Properties), new Composition(elemX.Name, elemX.DataTable, elemX.Properties), n, A, m, z, zX)
            {
                r_1 = r_1,
                r_2 = r_2,
                r_3 = r_3,
                x_1 = x_1,
                x_2 = x_2,
                x_3 = x_3,
            };

            return toClone;
        }
    }
}
