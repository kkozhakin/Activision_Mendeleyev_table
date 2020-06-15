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
using System.Linq;
using Point = Activision_Mendeleyev_table.HelperClasses.Point;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для DomeOfDecay.xaml
    /// </summary>
    public partial class DomeOfDecay : Window
    {
        /// <summary>
        /// Лист точек, представленных в DataGrid
        /// </summary>
        private List<List<double>> dat = new List<List<double>>();
        /// <summary>
        /// Системы соединений(базовая и аппроксимированная)
        /// </summary>
        private BinSystem sys, sys_ap = null;
        /// <summary>
        /// Графики(купол распада/функция смешения и аппроксимированная функция смешения)
        /// </summary>
        private DrawingClasses.CollapseGraph graph, graph_ap;
        /// <summary>
        /// Флаг: 0 - купол распада, 1 - функция смешения, 2 - свободная энергия Гиббса, 3 - оценка чувствительности
        /// </summary>
        private byte f = 0;
        private string ratio = "";

        private System.Windows.Forms.PictureBox diag = new System.Windows.Forms.PictureBox();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        /// <summary>
        /// Инициализация элемента host
        /// </summary>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        /// <summary>
        /// Первоначальные настройки и построение купола распада
        /// </summary>
        /// <param name="name">обозначение системы</param>
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

            SetBorders();
            SetColor();
        }

        /// <summary>
        /// Получение химических элементов из обозначения системы
        /// </summary>
        /// <param name="s">обозначение системы</param>
        /// <returns>массив обозначений химических элементов</returns>
        public static string[] Parse(string s)
        {
            string[] names = new string[] { "", "", ""};

            s = s.Replace(" ", "");

            if (new Regex(@"[A-Z]{1}[a-z]?[₀₁₂₃₄₅₆₇₈₉]*[A-Z]{1}[a-z]?[₀₁₂₃₄₅₆₇₈₉]*[-]{1}[A-Z]{1}[a-z]?[₀₁₂₃₄₅₆₇₈₉]*[A-Z]{1}[a-z]?[₀₁₂₃₄₅₆₇₈₉]*").IsMatch(s))
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
            else
                DrawingClasses.CollapseGraph.ClearExperiment();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidth = e.NewSize.Height + 92;
            Width = e.NewSize.Height + 92;
        }

        /// <summary>
        /// Построение графиков
        /// </summary>
        private void diag_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;           

            try
            {
                graph = new DrawingClasses.CollapseGraph(g, sys, diag.Width);

                if (f == 0)
                {
                    graph.DrawCollapse();
                    if (ratio != "")
                    {                    
                        graph_ap = new DrawingClasses.CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawCollapse(true, ratio);
                    }
                    MessageBox.Show(sys.Tmax.ToString());
                    if (sys_ap != null)
                        MessageBox.Show(sys_ap.Tmax.ToString());
                }
                else if (f == 1 || f == 3)
                {
                    if (sys_ap != null)
                    {
                        //MessageBox.Show(sys_ap.Tmax.ToString());
                        graph_ap = new DrawingClasses.CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawDH();
                    }
                    //else
                    // MessageBox.Show(sys.Tmax.ToString());
                    //graph.DrawDH(false);
                }
                else {
                    dG_Temp win = new dG_Temp();
                    win.ShowDialog();
                    if (sys_ap != null)
                    {
                        graph_ap = new DrawingClasses.CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawDG(int.Parse(win.TempD.Text), int.Parse(win.TempU.Text), int.Parse(win.TempInt.Text));
                    }
                    else
                        graph.DrawDG(int.Parse(win.TempD.Text), int.Parse(win.TempU.Text), int.Parse(win.TempInt.Text));
                }

                graph.DrawAxes();
                graph.DrawExperiment();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message == "MyException")
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Неверные данные для построения купола! Измените их в таблицах или в меню настроек!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Points_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            (e.EditingElement as TextBox).Text = (e.EditingElement as TextBox).Text.Replace(',', '.');
            if (dat[e.Row.GetIndex()].Capacity == 0)
            {
                dat[e.Row.GetIndex()].Add(0);
                dat[e.Row.GetIndex()].Add(0);
            }
            if (!float.TryParse((e.EditingElement as TextBox).Text.Replace('.', ','), out float p) || p < 0)
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

        /// <summary>
        /// Построение купола распада
        /// </summary>
        private void Build_Click(object sender, RoutedEventArgs e)
        {
            if (f == 1 && sys_ap != null) // f == 3?
                sys = sys_ap.Clone();

            f = 0;

            SetColor();
            SetBorders();

            int t = int.Parse(Tkp.Text);

            //if (dat.Count > 0 && t != -1)
              //  ratio = Approximate_T(t);

            if (sys_ap == null)
                sys_ap = sys.Clone();

            double sys_rat = double.Parse(DrawingClasses.Collapse.GetRatio(sys.delR / sys.R_const == -1 ? Math.Min(sys.R1, sys.R2) : sys.R_const));
            ratio = sys_rat.ToString();

            if (t != -1)
            {
                
                if (ratio != "")
                {
                    if (sys_rat > double.Parse(ratio))
                        sys_ap.R_const = sys.delR / (sys_rat - 0.0251);
                    else if (sys_rat < double.Parse(ratio))
                        sys_ap.R_const = sys.delR / (sys_rat + 0.0251);

                    //MessageBox.Show(sys_ap.R_const.ToString());
                    double[] dat = sys_ap.GetData();
                    sys_ap.SetData(t / (dat[1] * dat[2] * dat[3] * sys_ap.zX * (sys.delR / Math.Min(sys_ap.R(0), sys_ap.R(1))) * (sys.delR / Math.Min(sys_ap.R(0), sys_ap.R(1)))) * 2 * 1.9844 * 0.001, dat[1], dat[2], dat[3]);
                    MessageBox.Show((t / (dat[1] * dat[2] * dat[3] * sys_ap.zX * (sys.delR / Math.Min(sys_ap.R(0), sys_ap.R(1))) * (sys.delR / Math.Min(sys_ap.R(0), sys_ap.R(1)))) * 2 * 1.9844 * 0.001).ToString());
                }
            }
            diag.Refresh();
        }

        /// <summary>
        /// Запускает оценку чувствительности
        /// </summary>
        private void Sensitivity_Click(object sender, RoutedEventArgs e)
        {
            f = 3;
            sys_ap = sys.Clone();
            Points.Visibility = Visibility.Hidden;
            Build.Visibility = Visibility.Hidden;
            DelRows.Visibility = Visibility.Hidden;
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
            Hsm.IsEnabled = false;
            Gsm.IsEnabled = false;
            IsExpPoints.IsEnabled = false;

            R.Value = Math.Min(sys_ap.R(0), sys_ap.R(1));
            c.Value = sys_ap.GetData()[0];
            dE.Value = sys_ap.delEps;

            SetColor();
            SetBorders();          

            diag.Refresh();
        }

        /// <summary>
        /// Сохранение экспериментальных точек в файл
        /// </summary>
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

        /// <summary>
        /// Загрузка экспериментальных точек из файла
        /// </summary>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (.txt)|*.txt",
                CheckFileExists = true
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
            Points.Items.Refresh();//to do Refresh не разрешено во время выполнения операции AddNew или EditItem.
            Points_RowEditEnding(this, new DataGridRowEditEndingEventArgs(new DataGridRow(), DataGridEditAction.Commit));
        }

        /// <summary>
        /// Открывает окно настройки параметров
        /// </summary>
        private void DataSettings_Click(object sender, RoutedEventArgs e)
        {
            DataSettings ds = new DataSettings(sys);
            ds.ShowDialog();
            sys = ds.GetBS();

            diag.Refresh();
        }

        private void Points_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            DrawingClasses.CollapseGraph.ClearExperiment();
            for (int i = 0; i < dat.Count; i++)
                DrawingClasses.CollapseGraph.AddExperimentalPoint(dat[i][0], dat[i][1]);
            diag.Refresh();
        }

        /// <summary>
        /// Удаляет выделенные строки из таблицы
        /// </summary>
        private void DeleteSelectedRows(object sender, RoutedEventArgs e)
        {
            try
            {
                while (Points.SelectedItems.Count > 0)
                {
                    int selectedIndex = Points.SelectedIndex;
                    DrawingClasses.CollapseGraph.RemoveSelectedPoint(selectedIndex);
                    (Points.ItemsSource as List<List<double>>).RemoveAt(selectedIndex);
                    Points.Items.Refresh();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно удалить этот элемент!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);      
            }
            diag.Refresh();
        }

        /// <summary>
        /// Задает флаг, определяющий формат отображения эксперимента точками
        /// </summary>
        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            DrawingClasses.CollapseGraph.ExperimentIsPoints = true;
        }
        /// <summary>
        /// Задает флаг, определяющий формат отображения эксперимента ломанными
        /// </summary>
        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            DrawingClasses.CollapseGraph.ExperimentIsPoints = false;
        }

        /// <summary>
        /// Запускает аппроксимацию функции смешения
        /// </summary>
        private void Approxi_Click(object sender, RoutedEventArgs e)
        {
            f = 1;

            SetColor();
            SetBorders();

            if (sys != null)
                Approximate_dH(new double[] { Math.Min(sys.R1, sys.R2), sys.delEps, sys.GetData()[0] });

            diag.Refresh();
        }

        private void Gsm_Click(object sender, RoutedEventArgs e)
        {
            f = 2;

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        /// <summary>
        /// Задает границы параметров
        /// </summary>
        private void SetBorders() // to do throw
        {
            int t = -1;
            if (!int.TryParse(DownT.Text, out t))
                MessageBox.Show("Неправильно установленна нижняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            DrawingClasses.CollapseGraph.DownTemp = t;

            t = -1;
            if (!int.TryParse(UpT.Text, out t))
                MessageBox.Show("Неправильно установленна верхняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            DrawingClasses.CollapseGraph.UpTemp = t;

            t = -1;
            if (!int.TryParse(Tkp.Text, out t))
                MessageBox.Show("Неправильно установленна критическая температура!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (f == 3 && sys_ap != null)
            {
                if (!double.TryParse(UpR.Text.Replace('.', ','), out double b) && b <= 0)
                    MessageBox.Show("Неправильно установленна верхняя граница параметра R!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                R.Maximum = b;

                b = 0.01;
                if (!double.TryParse(DownR.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленна нижняя граница параметра R!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                R.Minimum = b;

                b = 0.01;
                if (!double.TryParse(Upc.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленна верхняя граница параметра c!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                c.Maximum = b;

                b = 0.01;
                if (!double.TryParse(Downc.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленна нижняя граница параметра c!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                c.Minimum = b;

                b = 0.01;
                if (!double.TryParse(UpdE.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленна верхняя граница параметра dE!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dE.Maximum = b;

                b = 0.01;
                if (!double.TryParse(DowndE.Text.Replace('.', ','), out b) && b <= 0)
                    MessageBox.Show("Неправильно установленна нижняя граница параметра dE!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dE.Minimum = b;
            }
        }

        /// <summary>
        /// Задает цвета отображения графиков
        /// </summary>
        private void SetColor()
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
            double[] dat = sys_ap.GetData();
            sys_ap.SetData(c.Value, dat[1], dat[2], dat[3]);

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        private void dE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sys_ap.delEps = dE.Value; 

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        private void R_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sys_ap.R_const = R.Value;

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        /// <summary>
        /// Возвращает к построению купола распада
        /// </summary>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            f = 1;
            //sys_ap = null;
            Points.Visibility = Visibility.Visible;
            Build.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Visible;
            Load.Visibility = Visibility.Visible;
            DelRows.Visibility = Visibility.Visible;
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
            Hsm.IsEnabled = true;
            Gsm.IsEnabled = true;
            IsExpPoints.IsEnabled = true;

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        /// <summary>
        /// Аппроксимация функции dH
        /// </summary>
        /// <param name="par">набор изменяемых параметров</param>
        private void Approximate_dH(double[] par)
        {
            List<Point> Dots = new List<Point>();
            foreach (List<double> point in dat)      
                Dots.Add(new Point(point[0], point[1]));
            double[] data = sys.GetData();

            Func<double, double[], double> Function = new Func<double, double[], double>((double x, double[] PP)
            => 1000 * x * (1 - x) * ((332 * sys.A / PP[0] * PP[1] * PP[1] + PP[2] * data[1] * data[2] * data[3] * sys.zX *
            (sys.delR / PP[0] * sys.delR) / PP[0])));

            try
            {
                double[] par_ap = Library.AproxiTab(Dots, Function, par, Criterion.Criterion_CKO); //TODO min of max and sko
                MessageBox.Show(String.Format("R_min = {0:f4}; delta E = {1:f4}; c = {2:f4}", par_ap[0], par_ap[1], par_ap[2]));
                sys_ap = sys.Clone();
                sys_ap.R_const = par_ap[0];
                sys_ap.delEps = par_ap[1];
                sys_ap.SetData(par_ap[2], data[1], data[2], data[3]);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Точки не заданы! Аппроксимация невозможна!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Аппроксимация купола распада
        /// </summary>
        /// <param name="t">критическая температура</param>
        private string Approximate_T(int t)
        {
            System.Windows.Resources.StreamResourceInfo ri = Application.GetResourceStream(new Uri("DrawingClasses/Collapse.xml", UriKind.Relative));
            Stream data = ri.Stream;

            List<Point> ExpDots = new List<Point>();
            List<Point> Dots = new List<Point>();
            double min = -1;
            string r_min = "";
            foreach (List<double> point in dat)
                ExpDots.Add(new Point(point[0], point[1]));
           
            if (ExpDots.Count > 0)
            {
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(data);
                foreach (string r in new string[7] { "0,00", "0,05", "0,10", "0,15", "0,20", "0,25", "0,30" })
                {
                    string[] x1values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("x1").Value.Split(';');
                    string[] x2values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("x2").Value.Split(';');
                    string[] y1values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("y1").Value.Split(';');
                    string[] y2values = doc.Root.Elements().First(p => p.Attribute("ratio").Value == r).Element("y2").Value.Split(';');

                    Dots.Clear();
                    for (int i = 0; i < x1values.Length; i++)
                        Dots.Add(new Point(double.Parse(x1values[i]), double.Parse(y1values[i]) * t));

                    for (int i = 0; i < x2values.Length; i++)
                        Dots.Add(new Point(double.Parse(x2values[i]), double.Parse(y2values[i]) * t));

                    double s = Library.ApproxiDots(ExpDots, Dots);

                    if (min == -1 || s < min)
                    {
                        min = s;
                        r_min = r;
                    }
                }
            }

            return r_min;
        }
    }
}
