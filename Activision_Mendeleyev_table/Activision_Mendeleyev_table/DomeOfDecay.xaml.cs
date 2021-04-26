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
using Activision_Mendeleyev_table.DrawingClasses;

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
        private CollapseGraph graph, graph_ap;
        /// <summary>
        /// Флаг: 0 - купол распада, 1 - функция смешения, 2 - свободная энергия Гиббса, 3 - оценка чувствительности
        /// </summary>
        private byte f = 0;
        private readonly System.Data.DataTable data;
        private bool changeValue = true;

        private System.Windows.Forms.PictureBox diag = new System.Windows.Forms.PictureBox();

        private System.Drawing.Image DoD_img;
        private System.Drawing.Image Hsm_img;
        private System.Drawing.Image Gsm_img;
        Graphics g;

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
        public DomeOfDecay(string name, ref System.Data.DataTable data)
        {
            InitializeComponent();

            Title = "Фазовые диаграммы " + name;
            this.data = data;
            string[] elems = Parse(name);
            Composition A = MendeleevTable.Elems.Find(x => x.Name == elems[0]);
            Composition B = MendeleevTable.Elems.Find(x => x.Name == elems[1]);
            Composition X = MendeleevTable.Elems.Find(x => x.Name == elems[2]);
            if (A == null || B == null || X == null)
                MessageBox.Show("Неверно заданы названия элементов входящих в систему! Измените их в меню настроек!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
                //sys = new BinSystem(name, A, B, X);
                sys = new BinSystem(name, A, B, X, 6, 4.8, 3, 4, 2);
                //sys = new BinSystem(name, A, B, X, 6, 1.745, 2, 1, 1);

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

            
            EventHandler sliderMinMax = (sender, e) => changeValue = false;
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MinimumProperty, typeof(Slider)).AddValueChanged(r1, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider)).AddValueChanged(r1, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MinimumProperty, typeof(Slider)).AddValueChanged(r2, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider)).AddValueChanged(r2, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MinimumProperty, typeof(Slider)).AddValueChanged(r3, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider)).AddValueChanged(r3, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MinimumProperty, typeof(Slider)).AddValueChanged(x1, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider)).AddValueChanged(x1, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MinimumProperty, typeof(Slider)).AddValueChanged(x3, sliderMinMax);
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider)).AddValueChanged(x3, sliderMinMax);
            UpT.Text = "-1";
            DownT.Text = "-1";
            Tkp.Text = "-1";

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
                CollapseGraph.ClearExperiment();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidth = e.NewSize.Height + 92;
            Width = e.NewSize.Height + 92;
            Points.Height = e.NewSize.Height / 2;
        }

        /// <summary>
        /// Построение графиков
        /// </summary>
        private void diag_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;           
            
            try
            {
                graph = new CollapseGraph(g, sys, diag.Width);

                if (f == 0)
                {
                    graph.DrawCollapse();
                    if (sys_ap != null)
                    {                    
                        graph_ap = new CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawCollapse(true);
                        Tcr_new_label.Content = "Tкр* = " + String.Format("{0:f4}", sys_ap.Tmax - 273) + "°С";
                    }
                    Tcr_label.Content = "Tкр = " + String.Format("{0:f4}", sys.Tmax - 273) + "°С";                      
                }
                else if (f == 1 || f == 3)
                {
                    if (sys_ap != null)
                    {     
                        graph_ap = new CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawDH();
                    }
                    graph.DrawDH(false);
                }
                else {
                    dG_Temp win = new dG_Temp();
                    win.ShowDialog();
                    if (sys_ap != null)
                    {
                        graph_ap = new CollapseGraph(g, sys_ap, diag.Width);
                        graph_ap.DrawDG(CollapseGraph.ToK(int.Parse(win.TempD.Text)), CollapseGraph.ToK(int.Parse(win.TempU.Text)), int.Parse(win.TempInt.Text));
                    }
                    else
                        graph.DrawDG(CollapseGraph.ToK(int.Parse(win.TempD.Text)), CollapseGraph.ToK(int.Parse(win.TempU.Text)), int.Parse(win.TempInt.Text));
                }

                if (f == 0)
                    graph.DrawAxes(true);
                else
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
            //if (f == 1 && sys_ap != null)
            //    sys = sys_ap.Clone();

            f = 0;

            SetColor();
            SetBorders();

            int t = int.Parse(Tkp.Text);

            if (sys_ap == null)
                sys_ap = sys.Clone();

            if (dat.Count > 0 && t != -1)
                Approximate_T(CollapseGraph.ToK(t));
                //Approximate_T(new double[] { sys.r_1, sys.r_2, sys.r_3, sys.c }, new List<double> { 0.02, 0.02, 0.02, 10 });

            diag.Refresh();
        }

        /// <summary>
        /// Запускает оценку чувствительности
        /// </summary>
        private void Sensitivity_Click(object sender, RoutedEventArgs e)
        {
            f = 3;

            Points.Visibility = Visibility.Hidden;
            Tcr_label.Visibility = Visibility.Hidden;
            Tcr_new_label.Visibility = Visibility.Hidden;
            Build.Visibility = Visibility.Hidden;
            DelRows.Visibility = Visibility.Hidden;
            Save.Visibility = Visibility.Hidden;
            Load.Visibility = Visibility.Hidden;
            x1.Visibility = Visibility.Visible;
            x2.Visibility = Visibility.Visible;
            x3.Visibility = Visibility.Visible;
            x1_label.Visibility = Visibility.Visible;
            x2_label.Visibility = Visibility.Visible;
            x3_label.Visibility = Visibility.Visible;
            x1_text.Visibility = Visibility.Visible;
            x2_text.Visibility = Visibility.Visible;
            x3_text.Visibility = Visibility.Visible;
            r1.Visibility = Visibility.Visible;
            r2.Visibility = Visibility.Visible;
            r3.Visibility = Visibility.Visible;
            r1_label.Visibility = Visibility.Visible;
            r2_label.Visibility = Visibility.Visible;
            r3_label.Visibility = Visibility.Visible;
            r1_text.Visibility = Visibility.Visible;
            r2_text.Visibility = Visibility.Visible;
            r3_text.Visibility = Visibility.Visible;

            Back.Visibility = Visibility.Visible;

            Hsm.IsEnabled = false;
            Gsm.IsEnabled = false;
            IsExpPoints.IsEnabled = false;

            if (sys_ap == null)
                sys_ap = sys.Clone();
            r1.Value = sys_ap.r_1;
            r2.Value = sys_ap.r_2;
            r3.Value = sys_ap.r_3;
            x1.Value = sys_ap.x_1;
            x2.Value = sys_ap.x_2;
            x3.Value = sys_ap.x_3;
            r1.Maximum = sys_ap.r_1 + 0.02;
            r1.Minimum = sys_ap.r_1 <= 0.02 ? 0.001 : sys_ap.r_1 - 0.02;
            r2.Maximum = sys_ap.r_2 + 0.02;
            r2.Minimum = sys_ap.r_2 <= 0.02 ? 0.001 : sys_ap.r_2 - 0.02;
            r3.Maximum = sys_ap.r_3 + 0.02;
            r3.Minimum = sys_ap.r_3 <= 0.02 ? 0.001 : sys_ap.r_3 - 0.02;
            x1.Maximum = sys_ap.x_1 + 0.02;
            x1.Minimum = sys_ap.x_1 <= 0.02 ? 0.001 : sys_ap.x_1 - 0.02;
            x2.Maximum = sys_ap.x_2 + 0.02;
            x2.Minimum = sys_ap.x_2 <= 0.02 ? 0.001 : sys_ap.x_2 - 0.02;
            x3.Maximum = sys_ap.x_3 + 0.02;
            x3.Minimum = sys_ap.x_3 <= 0.02 ? 0.001 : sys_ap.x_3 - 0.02;

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
            CollapseGraph.ClearExperiment();
            for (int i = 0; i < dat.Count; i++)
                CollapseGraph.AddExperimentalPoint(dat[i][0], dat[i][1]);
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
                    CollapseGraph.RemoveSelectedPoint(selectedIndex);
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
            CollapseGraph.ExperimentIsPoints = true;
        }
        /// <summary>
        /// Задает флаг, определяющий формат отображения эксперимента ломанными
        /// </summary>
        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            CollapseGraph.ExperimentIsPoints = false;
        }

        /// <summary>
        /// Запускает аппроксимацию функции смешения
        /// </summary>
        private void Approxi_Click(object sender, RoutedEventArgs e)
        {
            f = 1;

            SetColor();
            SetBorders();

            if (sys != null && dat.Count > 0)
                Approximate_dH(new double[] { sys.r_1, sys.r_2, sys.r_3, sys.x_1, sys.x_2, sys.x_3 },
                    new List<double> { 0.02, 0.02, 0.02, 0.02, 0.02, 0.02 });

            diag.Refresh();
        }

        /// <summary>
        /// Запускает построение графика свободной энергии Гиббса
        /// </summary>
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
        private void SetBorders()
        {
            int t = -1;
            if (DownT.Text != "" && !int.TryParse(DownT.Text, out t))
                MessageBox.Show("Неправильно установленна нижняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            CollapseGraph.DownTemp = t == -1 || f != 0 ? t : CollapseGraph.ToK(t);

            t = -1;
            if (UpT.Text != "" && !int.TryParse(UpT.Text, out t))
                MessageBox.Show("Неправильно установленна верхняя граница температуры!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            CollapseGraph.UpTemp = t == -1 || f != 0 ? t : CollapseGraph.ToK(t);

            t = -1;
            if (Tkp.Text != "" && !int.TryParse(Tkp.Text, out t))
                MessageBox.Show("Неправильно установленна критическая температура!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Задает цвета отображения графиков
        /// </summary>
        private void SetColor()
        {
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt64(Experiment.SelectedColor.Value.B * (Math.Pow(256, 0)) +
                Experiment.SelectedColor.Value.G * (Math.Pow(256, 1)) + Experiment.SelectedColor.Value.R * (Math.Pow(256, 2))));
            CollapseGraph.ExperimentColor = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);

            bytes = BitConverter.GetBytes(Convert.ToInt64(Theory.SelectedColor.Value.B * (Math.Pow(256, 0)) +
                Theory.SelectedColor.Value.G * (Math.Pow(256, 1)) + Theory.SelectedColor.Value.R * (Math.Pow(256, 2))));
            CollapseGraph.Color = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);

            bytes = BitConverter.GetBytes(Convert.ToInt64(Approximation.SelectedColor.Value.B * (Math.Pow(256, 0)) + 
                Approximation.SelectedColor.Value.G * (Math.Pow(256, 1)) + Approximation.SelectedColor.Value.R * (Math.Pow(256, 2))));
            CollapseGraph.ApproximationColor = Color.FromArgb(255, bytes[2], bytes[1], bytes[0]);
        }

        /// <summary>
        /// Возвращает к построению купола распада
        /// </summary>
        private void Back_Click(object sender, RoutedEventArgs e) //Add buttom Accept and Back(sys_ap = null)
        {
            f = 1;
            //sys_ap = null;
            Points.Visibility = Visibility.Visible;
            Tcr_label.Visibility = Visibility.Visible;
            Tcr_new_label.Visibility = Visibility.Visible;
            Build.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Visible;
            Load.Visibility = Visibility.Visible;
            DelRows.Visibility = Visibility.Visible;
            x1.Visibility = Visibility.Hidden;
            x2.Visibility = Visibility.Hidden;
            x3.Visibility = Visibility.Hidden;
            x1_label.Visibility = Visibility.Hidden;
            x2_label.Visibility = Visibility.Hidden;
            x3_label.Visibility = Visibility.Hidden;
            x1_text.Visibility = Visibility.Hidden;
            x2_text.Visibility = Visibility.Hidden;
            x3_text.Visibility = Visibility.Hidden;
            r1.Visibility = Visibility.Hidden;
            r2.Visibility = Visibility.Hidden;
            r3.Visibility = Visibility.Hidden;
            r1_label.Visibility = Visibility.Hidden;
            r2_label.Visibility = Visibility.Hidden;
            r3_label.Visibility = Visibility.Hidden;
            r1_text.Visibility = Visibility.Hidden;
            r2_text.Visibility = Visibility.Hidden;
            r3_text.Visibility = Visibility.Hidden;
            Back.Visibility = Visibility.Hidden;

            Hsm.IsEnabled = true;
            Gsm.IsEnabled = true;
            IsExpPoints.IsEnabled = true;

            SetColor();
            SetBorders();

            diag.Refresh();
        }

        /// <summary>
        /// Формирует отчета
        /// </summary>
        private void CreateReport_Click(object sender, RoutedEventArgs e)
        {
            new Report(sys, sys_ap, data, DoD_img, Hsm_img, Gsm_img).CreateReport();
        }
        
        /// <summary>
        /// Сохраняет текущий график для отчета
        /// </summary>
        private void SaveImg_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point relativePoint = diag.PointToScreen(new System.Drawing.Point(0, 0));
            Bitmap pngImage = new Bitmap(diag.Width, diag.Width);
            Graphics g = Graphics.FromImage(pngImage);
            g.CopyFromScreen(relativePoint.X, relativePoint.Y, 0, 0, new System.Drawing.Size(diag.Width, diag.Width), CopyPixelOperation.SourceCopy);

            if (f == 0)
                DoD_img = Report.ResizeImage(400, pngImage);
            else if (f == 2)
                Gsm_img = Report.ResizeImage(400, pngImage);
            else
                Hsm_img = Report.ResizeImage(400, pngImage);
        }

        private void x1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.x_1 = x1.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void x2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.x_2 = x2.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void x3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.x_3 = x3.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void r3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.r_3 = r3.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void r1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.r_1 = r1.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void r2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (changeValue)
                sys_ap.r_2 = r2.Value;

            changeValue = true;
            diag.Refresh();
        }

        private void Borders_Changed(object sender, RoutedEventArgs e)
        {
            SetBorders();
        }

        /// <summary>
        /// Аппроксимация купола распада
        /// </summary>
        /// <param name="t">критическая температура</param>
        private void Approximate_T(int t)
        {
            System.Windows.Resources.StreamResourceInfo ri = Application.GetResourceStream(new Uri("DrawingClasses/Collapse.xml", UriKind.Relative));
            Stream data = ri.Stream;

            List<Point> ExpDots = new List<Point>();
            List<Point> Dots = new List<Point>();
            double min = -1, r_min = 0;
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
                        r_min = double.Parse(r);
                    }
                }

                double r1 = sys.r_1, r2 = sys.r_2, r3 = sys.r_3, x1 = sys.x_1, x3 = sys.x_3;
                bool f = true;
                for (r1 = sys.r_1 - 0.02; r1 <= sys.r_1 + 0.02; r1 += 0.001)
                    if (f && r1 > 0)
                        for (r2 = sys.r_2 - 0.02; r2 <= sys.r_2 + 0.02; r2 += 0.001)
                            if (f && 2 > 0)
                                for (r3 = sys.r_3 - 0.02; r3 <= sys.r_3 + 0.02; r3 += 0.001)
                                    if (f && r3 > 0)
                                        for (x1 = sys.x_1 - 0.02; x1 <= sys.x_1 + 0.02; x1 += 0.001)
                                            if (f && x1 > 0)
                                                for (x3 = sys.x_3 - 0.02; x3 <= sys.x_3 + 0.02; x3 += 0.001)
                                                    if (f && x3 > 0 &&
                                                        (Math.Abs(r1 - r2) / (r3 + Math.Min(r1, r2))) >= r_min - 0.25 &&
                                                        (Math.Abs(r1 - r2) / (r3 + Math.Min(r1, r2))) < r_min + 0.25)
                                                    {
                                                        double _t = (33.33 * (1 - (sys.z / sys.n) * Math.Exp((x1 - x3) * (x1 - x3) * -0.25)) + 8.83) * 
                                                            sys.m * sys.n * sys.z * sys.zX * 
                                                            Math.Pow(Math.Abs(r1 - r2) / Math.Min(r1 + r3, r2 + r3), 2) / (1.9844 * 0.002);
                                                        if (Math.Abs(t - (int)_t) < 10)
                                                        {
                                                            sys_ap.r_1 = r1;
                                                            sys_ap.r_2 = r2;
                                                            sys_ap.r_3 = r3;
                                                            sys_ap.x_3 = x3;
                                                            sys_ap.x_1 = x1;
                                                            f = false;
                                                        }
                                                    }
            }                                          
        }

        /// <summary>
        /// Аппроксимация функции dH
        /// </summary>
        /// <param name="par">набор изменяемых параметров</param>
        private void Approximate_dH(double[] par, List<double> par_lims)
        {
            List<Point> Dots = new List<Point>();
            foreach (List<double> point in dat)
                Dots.Add(new Point(point[0], point[1]));

            Func<double, double[], double> Function = new Func<double, double[], double>((double x, double[] PP)
            => 1000 * x * (1 - x) * (332 * sys.A / (x * PP[0] + (1 - x) * PP[1] + PP[2]) *
            Math.Pow(sys.z / sys.n * Math.Exp((PP[3] - PP[5]) * (PP[3] - PP[5]) * -0.25) + sys.z / sys.n * Math.Exp((PP[4] - PP[5]) * (PP[4] - PP[5]) * -0.25), 2)
            + sys.c * sys.m * sys.n * sys.z * sys.zX * Math.Pow(Math.Abs(PP[0] - PP[1]) / (x * PP[0] + (1 - x) * PP[1] + PP[2]), 2)));

            double[] par_ap = Library.AproxiTab(Dots, Function, par, par_lims, Criterion.Criterion_CKO, Criterion.PenaltyF); //TODO min of max and cko
            sys_ap = sys.Clone();
            sys_ap.r_1 = par_ap[0];
            sys_ap.r_2 = par_ap[1];
            sys_ap.r_3 = par_ap[2];
            sys_ap.x_1 = par_ap[3];
            sys_ap.x_2 = par_ap[4];
            sys_ap.x_3 = par_ap[5];
            MessageBox.Show(String.Format("r1 = {0:f4}; r2 = {1:f4}; r3 = {2:f4}; x1 = {3:f4}; x2 = {4:f4}; x3 = {5:f4}; c = " + sys_ap.c, par_ap[0], par_ap[1], par_ap[2], par_ap[3], par_ap[4], par_ap[5]));
        }
    }
}
