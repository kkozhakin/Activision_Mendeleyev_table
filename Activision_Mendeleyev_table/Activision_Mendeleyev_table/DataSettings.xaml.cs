﻿using Activision_Mendeleyev_table.HelperClasses;
using System.Windows;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для DataSettings.xaml
    /// </summary>
    public partial class DataSettings : Window
    {
        private BinSystem sys;

        public DataSettings(BinSystem sys)
        {
            InitializeComponent();
            this.sys = sys;
            if (sys != null)
            {
                elemA.Text = sys.elementA;
                elemB.Text = sys.elementB;
                elemX.Text = sys.elementX;
                double[] par = sys.getData();
                zX.Text = par[4].ToString();
                z.Text = par[3].ToString();
                c.Text = par[0].ToString();
                m.Text = par[1].ToString();
                n.Text = par[2].ToString();
            }
        }

        public BinSystem GetBS() { return sys; }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите закрыть окно? Все несохраненные данные будут удалены!", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }

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
                sys = new BinSystem(elemA.Text + elemX.Text + '-' + elemB.Text + elemX.Text, A, B, X);
                sys.symbols[0] = r.Text;
                sys.symbols[1] = x.Text;
                double q = -1;
                if (!double.TryParse(z.Text, out q) || q < 0)
                    MessageBox.Show("Поле z - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(zX.Text, out q) || q < 0)
                    MessageBox.Show("Поле zX - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(m.Text, out q) || q < 0)
                    MessageBox.Show("Поле m - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(c.Text, out q) || q < 0)
                    MessageBox.Show("Поле c - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (!double.TryParse(n.Text, out q) || q < 0)
                    MessageBox.Show("Поле n - неотрицательное число!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    sys.setData(double.Parse(c.Text), double.Parse(m.Text), double.Parse(n.Text), double.Parse(z.Text), double.Parse(zX.Text));
                    this.Close();
                }
            }
        }
    }
}