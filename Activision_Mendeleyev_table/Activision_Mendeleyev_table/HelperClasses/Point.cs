namespace Activision_Mendeleyev_table.HelperClasses
{
    public class Point
    {
        double x, y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double Y { get { return y; } }
        public double X { get { return x; } }
    }
}
