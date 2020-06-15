using System.Windows;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для dG_Temp.xaml
    /// </summary>
    public partial class dG_Temp : Window
    {
        public dG_Temp()
        {
            InitializeComponent();
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!int.TryParse(TempD.Text, out int t) || !int.TryParse(TempU.Text, out t) || !int.TryParse(TempInt.Text, out t))
            {
                MessageBox.Show("Границы температуры - целые числа!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }
    }
}
