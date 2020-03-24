﻿using System;

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
        const double kN = 1.9844 * 0.001;
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
        string sourceString;
        /// <summary>
        /// Первый химический элемент
        /// </summary>
        Composition elemA;
        /// <summary>
        /// Второй химический элемент
        /// </summary>
        Composition elemB;
        /// <summary>
        /// Общий химический элемент
        /// </summary>
        Composition elemX;

        //Прочие переменные, которые описываются позже
        double c = -1;
        double m = -1;
        double n = -1;
        double z = -1;
        double r_1 = -1;
        double r_2 = -1;
        double r_3 = -1;
        public double R_const = -1;
        double deleps = -1;
        
        /// <summary>
        /// Ионный радиус первого элемента
        /// </summary>
        public double r1
        {
            get
            {
                if (r_1 == -1)
                    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[numA], out r_1);
 
                return r_1;
            }

            set
            {
                r_1 = value;
            }
        }

        /// <summary>
        /// Ионный радиус второго элемента
        /// </summary>
        public double r2
        {
            get
            {
                if (r_2 == -1)
                    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[numB], out r_2);

                return r_2;
            }

            set
            {
                r_2 = value;
            }
        }

        /// <summary>
        /// Ионный радиус общего элемента
        /// </summary>
        public double r3
        {
            get
            {
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);

                return r_3;
            }

            set
            {
                r_3 = value;
            }
        }

        /// <summary>
        /// Межатомное расстояние первого соединения
        /// </summary>
        public double R1
        {
            get
            {
                if (r_1 == -1)
                    double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[numA], out r_1);
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);

                if (r_3 != -1 && r_1 != -1)
                    return r_1 + r_3;
                else
                    return -1;
            }
        }

        /// <summary>
        /// Межатомное расстояние второго соединения
        /// </summary>
        public double R2
        {
            get
            {
                if (r_2 == -1)
                    double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[numB], out r_2);
                if (r_3 == -1)
                    double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);

                if (r_3 != -1 && r_2 != -1)
                    return r_2 + r_3;
                else
                    return -1;
            }
        }

        /// <summary>
        /// Первый химический элемент
        /// </summary>
        public string elementA
        {
            get { return elemA.Name; }
        }

        /// <summary>
        /// Второй химический элемент
        /// </summary>
        public string elementB
        {
            get { return elemB.Name; }
        }

        /// <summary>
        /// Общий химический элемент
        /// </summary>
        public string elementX
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
        public void setData(double c, double m, double n, double z)
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
        public double[] getData() { return new double[] { c, m, n, z, numA, numB, numX }; }

        /// <summary>
        /// Энтропия смешения
        /// </summary>
        public double Ssm(double x1)
        {
            double x2 = 1 - x1;
            double Skon = (-1) * kN * (x1 * Math.Log(x1) + x2 * Math.Log(x2));
            double Skol = 2.7252 * x1 * x2 * (delR / R1) * 0.001;

            return Skon + Skol;
        }

        /// <summary>
        /// Среднее межатомное расстояние
        /// </summary>
        public double R(double x1)
        {
            if (R_const != -1)
                return R_const;

            if (r_2 == -1)
                double.TryParse(elemB.Properties.Find(x => x.First.Second == symbols[0]).Second[numB], out r_2);
            if (r_3 == -1)
                double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[0]).Second[numX], out r_3);
            if (r_1 == -1)
                double.TryParse(elemA.Properties.Find(x => x.First.Second == symbols[0]).Second[numA], out r_1);

            if (r_1 != -1 && r_2 != -1 && r_3 != -1)
                return x1 * r_1 + (1 - x1) * r_2 + r_3;
            else
                return -1;
        }

        /// <summary>
        /// Разность радиусов
        /// </summary>
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
            double x2 = 1 - x1;

            double first = x1 * x2 * (322 * A / R(x1)) * (delEps * delEps);

            if (zX == -1)
                double.TryParse(elemX.Properties.Find(x => x.First.Second == symbols[2]).Second[numX], out zX);

            double second = x1 * x2 * (c * m * n * z * zX *
                ((delR / R(x1)) * (delR / R(x1))));

            return first + second;
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

                return (c * m * n * z * zX * ((delR / Math.Min(R1, R2)) * (delR / Math.Min(R1, R2)))) / (2 * kN);
            }
        }

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
            toClone.setData(c, m, n, z);
            toClone.numA = numA;
            toClone.numB = numB;
            toClone.numX = numX;
            toClone.A = A;

            return toClone;
        }
    }
}
