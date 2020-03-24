namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, который представляет собой точку с двумя численными координатами
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Координаты точки
        /// </summary>
        double x, y;

        /// <summary>
        /// Конструктор точки
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Возвращает значение координаты Y 
        /// </summary>
        public double Y { get { return y; } }
        /// <summary>
        /// Возвращает значение координаты X
        /// </summary>
        public double X { get { return x; } }
    }
}
