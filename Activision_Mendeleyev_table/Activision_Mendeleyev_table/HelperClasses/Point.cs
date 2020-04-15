namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, который представляет собой точку с двумя численными координатами
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Конструктор точки
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Свойство координаты Y 
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Свойство координаты X
        /// </summary>
        public double X { get; set; }
    }
}
