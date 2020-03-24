using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Activision_Mendeleyev_table.DrawingClasses
{
    /// <summary>
    /// Класс, содержащий набор точек фазовой диаграммы и методы их получения
    /// </summary>
    class Collapse
    {
        /// <summary>
        /// Набор точек правого соединений
        /// </summary>
        public Point[] right;

        /// <summary>
        /// Набор точек левого соединений
        /// </summary>
        public Point[] left;

        /// <summary>
        /// Получает точки для фазовой диаграммы
        /// </summary>
        /// <param name="system">система соединений</param>
        public Collapse(BinSystem system)
        {
            string r = GetRatio(system.delR / Math.Min(system.R1, system.R2));

            /*if (filename == "")
            {
                Microsoft.Win32.OpenFileDialog myDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "XML-файл(*.XML)|*.XML",
                    CheckFileExists = true
                };

                if (myDialog.ShowDialog() == true)
                    filename = myDialog.FileName;
                else throw new Exception("Файл с точками купола не выбран! Построение невозможно!", new Exception("MyException"));
            }*/
            System.Windows.Resources.StreamResourceInfo ri = System.Windows.Application.GetResourceStream(new Uri("DrawingClasses/Collapse.xml", UriKind.Relative));
            System.IO.Stream data = ri.Stream;

            XDocument doc = XDocument.Load(data);
            string[] x1values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r).Element("x1").Value.Split(';');
            string[] x2values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r).Element("x2").Value.Split(';');
            string[] y1values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r).Element("y1").Value.Split(';');
            string[] y2values = doc.Root.Elements().First(
                p => p.Attribute("ratio").Value == r).Element("y2").Value.Split(';');

            right = new Point[x1values.Length];
            left = new Point[x2values.Length];

            for (int i = 0; i < x1values.Length; i++)
                right[i] = new Point(double.Parse(x1values[i]), double.Parse(y1values[i]));

            for (int i = 0; i < x2values.Length; i++)
                left[i] = new Point(double.Parse(x2values[i]), double.Parse(y2values[i]));
        }

        /// <summary>
        /// Получает соотношение радиусов
        /// </summary>
        /// <param name="ratio">delR/Rmin</param>
        /// <returns>соотношение радиусов</returns>
        string GetRatio(double ratio)
        {
            ratio = Math.Round(ratio, 3);
            if ((ratio >= 0) && (ratio < 0.025)) return "0,00";
            else if ((ratio <= 0.05) || ((ratio > 0.05) && (ratio < 0.075))) return "0,05";
            else if ((ratio <= 0.1) || ((ratio > 0.1) && (ratio < 0.125))) return "0,10";
            else if ((ratio <= 0.15) || ((ratio > 0.15) && (ratio < 0.175))) return "0,15";
            else if ((ratio <= 0.20) || ((ratio > 0.20) && (ratio < 0.225))) return "0,20";
            else if ((ratio <= 0.25) || ((ratio > 0.25) && (ratio < 0.275))) return "0,25";
            else if ((ratio <= 0.30) || ((ratio > 0.30) && (ratio < 0.325))) return "0,30";
            else throw new Exception("Недопустимое отношение радиусов!", new Exception("MyException"));
        }
    }
}
