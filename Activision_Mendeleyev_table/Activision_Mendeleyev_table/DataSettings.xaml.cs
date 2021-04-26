using Activision_Mendeleyev_table.HelperClasses;
using System.Windows;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для DataSettings.xaml
    /// </summary>
    public partial class DataSettings : Window
    {
        /// <summary>
        /// Система соединений
        /// </summary>
        private BinSystem sys;

        /// <summary>
        /// Настройка некоторых параметров системы
        /// </summary>
        /// <param name="sys">система соединений</param>
        public DataSettings(BinSystem sys)
        {
            InitializeComponent();
            this.sys = sys;
            if (sys != null)
            {
                elemA.Text = sys.ElementA;
                elemB.Text = sys.ElementB;
                elemX.Text = sys.ElementX;
                z.Text = sys.z.ToString();
                m.Text = sys.m.ToString();
                n.Text = sys.n.ToString();
                _A.Text = sys.A.ToString();
            }
        }

        /// <summary>
        /// Возвращает систему соединений
        /// </summary>
        public BinSystem GetBS() { return sys; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите закрыть окно? Все несохраненные данные будут удалены!", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }

        /// <summary>
        /// Сохраняет параметры в системе и закрывает окно
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {

            Composition A = MendeleevTable.Elems.Find(x => x.Name == elemA.Text);
            Composition B = MendeleevTable.Elems.Find(x => x.Name == elemB.Text);
            Composition X = MendeleevTable.Elems.Find(x => x.Name == elemX.Text);
            if (A == null || B == null || X == null)
                MessageBox.Show("Неверные заданы названия элементов входящих в систему! Измените их в меню настроек!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {                
                _A.Text = _A.Text.Replace('.', ',');
                double q = -1;
                int w = -1;
                if (!double.TryParse(z.Text, out q) || q < 0)
                    MessageBox.Show("Поле z - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!int.TryParse(numA.Text, out w) || w < 0)
                    MessageBox.Show("Поле numA - целое неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!int.TryParse(numB.Text, out w) || w < 0)
                    MessageBox.Show("Поле numB - целое неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!int.TryParse(numX.Text, out w) || w < 0)
                    MessageBox.Show("Поле numX - целое неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(m.Text, out q) || q < 0)
                    MessageBox.Show("Поле m - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
               else if (!double.TryParse(n.Text, out q) || q < 0)
                    MessageBox.Show("Поле n - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(_A.Text, out q) || q < 0)
                    MessageBox.Show("Поле A - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    double.TryParse(X.Properties.Find(x => x.First.Second == FZ.Text).Second[int.Parse(numX.Text)], out q); //try
                    sys = new BinSystem(elemA.Text + elemX.Text + '-' + elemB.Text + elemX.Text, A, B, X, double.Parse(n.Text), double.Parse(_A.Text), double.Parse(m.Text), double.Parse(z.Text), q);
                    double.TryParse(A.Properties.Find(_x => _x.First.Second == x.Text).Second[int.Parse(numA.Text)], out sys.x_1); 
                    double.TryParse(B.Properties.Find(_x => _x.First.Second == x.Text).Second[int.Parse(numB.Text)], out sys.x_2);
                    double.TryParse(X.Properties.Find(_x => _x.First.Second == x.Text).Second[int.Parse(numX.Text)], out sys.x_3);
                    double.TryParse(B.Properties.Find(x => x.First.Second == r.Text).Second[int.Parse(numB.Text)], out sys.r_2);
                    double.TryParse(X.Properties.Find(x => x.First.Second == r.Text).Second[int.Parse(numX.Text)], out sys.r_3);
                    double.TryParse(A.Properties.Find(x => x.First.Second == r.Text).Second[int.Parse(numA.Text)], out sys.r_1);
                    this.Close();
                }
            }
        }
    }
}
