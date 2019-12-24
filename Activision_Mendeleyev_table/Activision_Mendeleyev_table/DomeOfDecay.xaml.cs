using Activision_Mendeleyev_table.Approximation;
using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для DomeOfDecay.xaml
    /// </summary>
    public partial class DomeOfDecay : Window
    {
        List<List<double>> dat = new List<List<double>>();
        BinSystem sys, sys_ap = null;
        DrawingClasses.CollapseGraph graph, graph_ap;
        System.Windows.Forms.PictureBox diag = new System.Windows.Forms.PictureBox();
        bool f = true;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        public DomeOfDecay(string name)
        {
            InitializeComponent();

            string[] elems = Parse(name);

            Composition A = MendeleevTable.Elems.Find(x => x.Name == elems[0]);
            Composition B = MendeleevTable.Elems.Find(x => x.Name == elems[1]);
            Composition X = MendeleevTable.Elems.Find(x => x.Name == elems[2]);
            if (A == null || B == null || X == null)
                MessageBox.Show("Неверно заданы названия элементов входящих в систему! Измените их в меню настроек!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
                sys = new BinSystem(name, A, B, X);
                sys.setData(30, 2, 6, 1, 1);

            DataSettings ds = new DataSettings(sys);
            ds.ShowDialog();
            sys = ds.GetBS();

            host.Child = diag;
            diag.Paint += new System.Windows.Forms.PaintEventHandler(diag_Paint);

            Points.Columns.Add(new DataGridTextColumn()
            {
                Header = "x",
                Binding = new Binding("[0]")
            });
            Points.Columns.Add(new DataGridTextColumn()
            {
                Header = "y",
                Binding = new Binding("[1]")
            });
            Points.ItemsSource = dat;
        }

        public static string[] Parse(string s)
        {
            string[] names = new string[] { "", "", ""};

            s = s.Replace(" ", "");

            if (new Regex(@"[A-Z]{1}[a-z]?\d*[A-Z]{1}[a-z]?\d*[_-₋]{1}[A-Z]{1}[a-z]?\d*[A-Z]{1}[a-z]?\d*").IsMatch(s))
            {
                for (int i = 0; i < 3; i++)
                {
                    Regex myReg = new Regex(@"[A-Z]{1}[a-z]?"); //шаблон элемента
                    Match match = myReg.Match(s);

                    s = s.Replace(match.Value, "");
                    names[i] = match.Value;
                }

                // поставка элемента Х на третье место в массиве
                string z = names[2];
                names[2] = names[1];
                names[1] = z;
            }

            return names;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите закрыть окно? Все несохраненные данные будут удалены!", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidth = e.NewSize.Height + 92;
            Width = e.NewSize.Height + 92;
        }

        private void diag_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            try
            {
                graph = new DrawingClasses.CollapseGraph(g, sys, diag.Width);

                if (f)
                    graph.DrawCollapse();
                else
                {
                    if (sys_ap != null)
                    {
                        graph_ap = new DrawingClasses.CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawDH();
                    }
                    graph.DrawDH(false);
                }

                graph.DrawAxes();
                graph.DrawExperiment();
            }
            catch
            {
                MessageBox.Show("Неверные данные для построения купола! Измените их в таблицах или в меню настроек!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DataSettings ds = new DataSettings(sys);
                ds.ShowDialog();
                sys = ds.GetBS();
            }
        }

        private void Points_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            float p;
            (e.EditingElement as TextBox).Text = (e.EditingElement as TextBox).Text.Replace(',', '.');
            if (dat[e.Row.GetIndex()].Capacity == 0)
            {
                dat[e.Row.GetIndex()].Add(0);
                dat[e.Row.GetIndex()].Add(0);
            }
            if (!float.TryParse((e.EditingElement as TextBox).Text.Replace('.', ','), out p) || p < 0)
            {
                MessageBox.Show("Координаты точки должны быть неотрицательным числом!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                (e.EditingElement as TextBox).Text = "";

                e.Cancel = true;
            }           
            if (e.Column.DisplayIndex == 0)
                dat[e.Row.GetIndex()][0] = p;
            else
                dat[e.Row.GetIndex()][1] = p;
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            if (!f && sys_ap != null)
                if (MessageBox.Show("Использовать при построении купола новые значения параметров?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    sys = sys_ap.Clone();

            f = true;

            setColor();
            setBorders();
            
            diag.Refresh();
        }

        private void Sensitivity_Click(object sender, RoutedEventArgs e)
        {
            f = false;
            sys_ap = sys.Clone();
            Points.Visibility = Visibility.Hidden;
            Build.Visibility = Visibility.Hidden;
            Approxi.Visibility = Visibility.Hidden;
            Save.Visibility = Visibility.Hidden;
            Load.Visibility = Visibility.Hidden;
            R.Visibility = Visibility.Visible;
            dE.Visibility = Visibility.Visible;
            c.Visibility = Visibility.Visible;
            R_label.Visibility = Visibility.Visible;
            dE_label.Visibility = Visibility.Visible;
            c_label.Visibility = Visibility.Visible;
            R_text.Visibility = Visibility.Visible;
            dE_text.Visibility = Visibility.Visible;
            c_text.Visibility = Visibility.Visible;
            Back.Visibility = Visibility.Visible;
            DownR.IsEnabled = true;
            UpdE.IsEnabled = true;
            Downc.IsEnabled = true;
            UpR.IsEnabled = true;
            DowndE.IsEnabled = true;
            Upc.IsEnabled = true;
            IsExpPoints.IsEnabled = false;

            R.Value = Math.Min(sys_ap.R(0), sys_ap.R(1));
            c.Value = sys_ap.getData()[0];
            dE.Value = sys_ap.delEps;

            setColor();
            setBorders();          

            diag.Refresh();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Points",
                DefaultExt = ".txt",
                Filter = "Text files (.txt)|*.txt"
            };


            if (dlg.ShowDialog() == true)
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                    for (int i = 0; i < dat.Count; i++)
                        sw.WriteLine(dat[i][0] + " " + dat[i][1]);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (.txt)|*.txt"
            };

            try
            {
                if (dlg.ShowDialog() == true)
                {
                    dat.Clear();
                    using (FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                    using (StreamReader sr = new StreamReader(fs))
                        while (!sr.EndOfStream)
                        {
                            string s = sr.ReadLine();
                            dat.Add(new List<double> { double.Parse(s.Split()[0]), double.Parse(s.Split()[1]) });                          
                        }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат файла!");
            }
            Points.Items.Refresh();
            Points_RowEditEnding(this, new DataGridRowEditEndingEventArgs(new DataGridRow(), DataGridEditAction.Commit));
        }

        private void DataSettings_Click(object sender, RoutedEventArgs e)
        {
            DataSettings ds = new DataSettings(sys);
            ds.ShowDialog();
            sys = ds.GetBS();

            diag.Refresh();
        }

        private void Points_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            DrawingClasses.CollapseGraph.clearExperiment();
            for (int i = 0; i < dat.Count; i++)
                DrawingClasses.CollapseGraph.addExperimentalPoint(dat[i][0], dat[i][1]);
            diag.Refresh();
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            DrawingClasses.CollapseGraph.ExperimentIsPoints = true;
        }

        private void Approxi_Click(object sender, RoutedEventArgs e)
        {
            f = false;

            setColor();
            setBorders();

            if (sys != null)
                Approximate(new double[] { Math.Min(sys.R(1), sys.R(0)), sys.delEps, sys.getData()[0] });

            diag.Refresh();
        }

        private void setBorders()
        {
            int t = -1;
            if (!int.TryParse(DownT.Text, out t))
                MessageBox.Show("Неправильно установленно нижняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            DrawingClasses.CollapseGraph.DownTemp = t;

            t = -1;
            if (!int.TryParse(UpT.Text, out t))
                MessageBox.Show("Неправильно установленно верхняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            DrawingClasses.CollapseGraph.UpTemp = t;

            if (!f && sys_ap != null)
            {
                double b = 0.01;
                if (!double.TryParse(UpR.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно верхняя граница параметра R!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                R.Maximum = b;

                b = 0.01;
                if (!double.TryParse(DownR.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно нижняя граница параметра R!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                R.Minimum = b;

                b = 0.01;
                if (!double.TryParse(Upc.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно верхняя граница параметра c!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                c.Maximum = b;

                b = 0.01;
                if (!double.TryParse(Downc.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно нижняя граница параметра c!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                c.Minimum = b;

                b = 0.01;
                if (!double.TryParse(UpdE.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно верхняя граница параметра dE!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dE.Maximum = b;

                b = 0.01;
                if (!double.TryParse(DowndE.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленно нижняя граница параметра dE!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dE.Minimum = b;
            }
        }

        private void setColor()
        {
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt64(Experiment.SelectedColor.Value.B * (Math.Pow(256, 0)) +
                Experiment.SelectedColor.Value.G * (Math.Pow(256, 1)) + Experiment.SelectedColor.Value.R * (Math.Pow(256, 2))));
            DrawingClasses.CollapseGraph.ExperimentColor = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);

            bytes = BitConverter.GetBytes(Convert.ToInt64(Theory.SelectedColor.Value.B * (Math.Pow(256, 0)) +
                Theory.SelectedColor.Value.G * (Math.Pow(256, 1)) + Theory.SelectedColor.Value.R * (Math.Pow(256, 2))));
            DrawingClasses.CollapseGraph.Color = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);

            bytes = BitConverter.GetBytes(Convert.ToInt64(Approximation.SelectedColor.Value.B * (Math.Pow(256, 0)) + 
                Approximation.SelectedColor.Value.G * (Math.Pow(256, 1)) + Approximation.SelectedColor.Value.R * (Math.Pow(256, 2))));
            DrawingClasses.CollapseGraph.ApproximationColor = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);
        }

        private void c_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double[] dat = sys_ap.getData();
            sys_ap.setData(c.Value, dat[1], dat[2], dat[3], dat[4]);

            setColor();
            setBorders();

            diag.Refresh();
        }

        private void dE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sys_ap.delEps = dE.Value; 

            setColor();
            setBorders();

            diag.Refresh();
        }

        private void R_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sys_ap.R_const = R.Value;

            setColor();
            setBorders();

            diag.Refresh();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            f = true;
            sys_ap = null;
            Points.Visibility = Visibility.Visible;
            Build.Visibility = Visibility.Visible;
            Approxi.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Visible;
            Load.Visibility = Visibility.Visible;
            R.Visibility = Visibility.Hidden;
            dE.Visibility = Visibility.Hidden;
            c.Visibility = Visibility.Hidden;
            R_label.Visibility = Visibility.Hidden;
            dE_label.Visibility = Visibility.Hidden;
            c_label.Visibility = Visibility.Hidden;
            R_text.Visibility = Visibility.Hidden;
            dE_text.Visibility = Visibility.Hidden;
            c_text.Visibility = Visibility.Hidden;
            Back.Visibility = Visibility.Hidden;
            DownR.IsEnabled = false;
            UpdE.IsEnabled = false;
            Downc.IsEnabled = false;
            UpR.IsEnabled = false;
            DowndE.IsEnabled = false;
            Upc.IsEnabled = false;
            IsExpPoints.IsEnabled = true;

            setColor();
            setBorders();

            diag.Refresh();
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawingClasses.CollapseGraph.ExperimentIsPoints = false;
        }

        private void Approximate(double[] par)
        {
            List<HelperClasses.Point> Dots = new List<HelperClasses.Point>();
            foreach (List<double> point in dat)      
                Dots.Add(new HelperClasses.Point(point[0], point[1]));
            double[] data = sys.getData();

            Func<double, double[], double> Function = new Func<double, double[], double>((double x, double[] PP)
            => 1000 * x * (1 - x) * ((332 * sys.A / PP[0] * PP[1] * PP[1] + PP[2] * data[1] * data[2] * data[3] * data[4] *
            (Math.Abs(sys.r1 - sys.r2) / PP[0] * Math.Abs(sys.r1 - sys.r2) / PP[0]))));

            try
            {
                double[] par_ap = Library.AproxiTab(Dots, Function, par, Criterion.Criterion_CKO);
                MessageBox.Show(String.Format("R_min = {0:f4}; delta E = {1:f4}; c = {2:f4}", par_ap[0], par_ap[1], par_ap[2]));
                sys_ap = sys.Clone();
                sys_ap.R_const = par_ap[0];
                sys_ap.delEps = par_ap[1];
                sys_ap.setData(par_ap[2], data[1], data[2], data[3], data[4]);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Точки не заданы! Аппроксимация невозможна!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
