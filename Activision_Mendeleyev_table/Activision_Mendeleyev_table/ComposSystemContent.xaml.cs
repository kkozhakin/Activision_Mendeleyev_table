using Activision_Mendeleyev_table.HelperClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для ComposSystemContent.xaml
    /// </summary>
    public partial class ComposSystemContent : Window
    {        
        /// <summary>
        /// Таблица данных
        /// </summary>
        DataTable dat;

        /// <summary>
        /// Обозначение соединения(системы соединений)
        /// </summary>
        string elem;

        /// <summary>
        /// Флаг: true - соединение, false - система
        /// </summary>
        bool f;

        /// <summary>
        /// Конструктор, инициализирующий окно таблицы соединения(системы соединений)
        /// </summary>
        /// <param name="elem">название соединения(системы соединений)</param>
        /// <param name="f">флаг: true - соединение, false - система</param>
        public ComposSystemContent(string elem, bool f)
        {
            InitializeComponent();
            this.f = f;
            this.elem = elem;

            if (f)
            {
                this.Title = "Таблица соединения " + elem;
                DomeOfDecayWindowOpen.Visibility = Visibility.Hidden;
                EditTable.RenderTransform = new TranslateTransform();

                Composition comp = MendeleevTable.Compos.Find(x => x.Name == elem);
                dat = new DataTable() { TableName = elem };
                if (comp != null)
                {
                    //Заполнение столбцов
                    for (int i = 0; i < comp.Properties.Count; i++)
                        dat.Columns.Add( new DataColumn() { ColumnName = comp.Properties[i].First.First, Caption = comp.Properties[i].First.Second });

                    //Заполнение строк
                    for (int i = 0; i < comp.Properties.Count; i++)
                        for (int j = 0; j < dat.Columns.Count; j++)
                            if (dat.Columns[j].ColumnName == comp.Properties[i].First.First)
                                for (int k = 0; k < comp.Properties[i].Second.Count; k++)
                                {
                                    if (dat.Rows.Count <= k)
                                        dat.Rows.Add();
                                    dat.Rows[k][j] = comp.Properties[i].Second[k];
                                }

                    //Визуализация столбцов
                    foreach (DataColumn i in dat.Columns)
                    {
                        bool r =  (i.ColumnName[0] == '=') ? true : false;
                        ComposSystemTable.Columns.Add(new DataGridTextColumn()
                        {
                            Header = (i.Caption == "" || i.Caption == " ") ? i.ColumnName : (i.ColumnName[0] == '=') ? i.Caption + i.ColumnName : i.ColumnName + ", " + i.Caption,
                            Binding = new Binding("[" + ComposSystemTable.Columns.Count + "]"),
                            IsReadOnly = r
                        });
                    }
                }
            }
            else
            {
                this.Title = "Таблица системы " + elem;

                dat = MendeleevTable.BinarySistem.Find(x => x.TableName == elem);

                if (dat == null)
                {
                    dat = new DataTable() { TableName = elem };
                    dat.Columns.Add(new DataColumn("X") { Caption = "x"} );
                    ComposSystemTable.Columns.Add(new DataGridTextColumn()
                    {
                        Header = "X",
                        Binding = new Binding("[0]"),
                        IsReadOnly = false,
                        Width = 100
                    });
                }
                else
                {
                    foreach (DataColumn i in dat.Columns)
                    {
                        bool r = (i.ColumnName[0] == '=') ? true : false;
                        ComposSystemTable.Columns.Add(new DataGridTextColumn()
                        {
                            Header = (i.Caption == "" || i.Caption == " ") ? i.ColumnName : (i.ColumnName[0] == '=') ? i.Caption + i.ColumnName : i.ColumnName + ", " + i.Caption,
                            Binding = new Binding("[" + ComposSystemTable.Columns.Count + "]"),
                            IsReadOnly = r
                        });
                    }
                }
            }
            ComposSystemTable.ItemsSource = dat.DefaultView;

            if (f && dat.Columns.Count > 0 || !f && dat.Columns.Count > 1)
                DelColumn.IsEnabled = true;
            if (dat.Rows.Count > 0)
                DelRow.IsEnabled = true;
        }

        /// <summary>
        /// Добавляет текстовый столбец в таблицу
        /// </summary>
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            DelColumn.IsEnabled = DataGridHelper.AddColumn(ref ComposSystemTable, ref dat, f);
        }       

        /// <summary>
        /// Добавляет строку в таблицу
        /// </summary>
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            dat.Rows.Add();
            if (dat.Rows.Count > 0)
                DelRow.IsEnabled = true;
            if (!f)
                dat.Rows[dat.Rows.Count - 1][0] = 0;
            CollectionViewSource.GetDefaultView(ComposSystemTable.ItemsSource).Refresh();
        }

        /// <summary>
        /// Удаляет столбец в таблицу
        /// </summary>
        private void DelColumn_Click(object sender, RoutedEventArgs e)
        {
            ComposSystemTable.Columns.RemoveAt(ComposSystemTable.Columns.Count - 1);
            if (f && dat.Columns.Count <= 1 || !f && dat.Columns.Count <= 2)
                DelColumn.IsEnabled = false;
            dat.Columns.RemoveAt(dat.Columns.Count - 1);
        }

        /// <summary>
        /// Удаляет строку в таблицу
        /// </summary>
        private void DelRow_Click(object sender, RoutedEventArgs e)
        {
            if (dat.Rows.Count <= 1)
                DelRow.IsEnabled = false;
            dat.Rows.RemoveAt(dat.Rows.Count - 1);
            CollectionViewSource.GetDefaultView(ComposSystemTable.ItemsSource).Refresh();
        }

        /// <summary>
        /// Запускает раcчет формул и сохраняет данные в файл
        /// </summary>
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            ComposSystemTable.IsReadOnly = true;
            EditTable.Visibility = Visibility.Visible;
            AddColumn.Visibility = Visibility.Hidden;
            AddRow.Visibility = Visibility.Hidden;
            DelColumn.Visibility = Visibility.Hidden;
            DelRow.Visibility = Visibility.Hidden;
            Calculate.Visibility = Visibility.Hidden;
            AddFormul.Visibility = Visibility.Hidden;
            if (!f)
                DomeOfDecayWindowOpen.Visibility = Visibility.Visible;

            try
            {
                //Рассчет формул
                for (int i = f?0:1; i < dat.Columns.Count; i++)
                    for (int u = 0; u < dat.Rows.Count; u++)
                        if (dat.Columns[i].ColumnName[0] == '=')
                            dat.Rows[u][i] = String.Format("{0:f4}", MathParser.Parse(dat.Columns[i].ColumnName.Substring(1), ref dat, u));               
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message == "MyException")
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Неверный формат формулы!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            dat.AcceptChanges();

            if (f)
            {
                List<Pair<Pair<string, string>, List<string>>> prop = new List<Pair<Pair<string, string>, List<string>>>();

                if (prop == null)
                    prop = new List<Pair<Pair<string, string>, List<string>>>();

                for (int i = 0; i < dat.Columns.Count; i++)
                {
                    prop.Add(new Pair<Pair<string, string>, List<string>>(new Pair<string, string>(dat.Columns[i].ColumnName, dat.Columns[i].Caption), new List<string>()));
                    for (int j = 0; j < dat.Rows.Count; j++)
                        prop[i].Second.Add(dat.Rows[j][i].ToString());
                }

                Composition el = MendeleevTable.Compos.Find(x => x.Name == elem);
                if (el != null)
                    MendeleevTable.Compos.Find(x => x.Name == elem).Properties = new List<Pair<Pair<string, string>, List<string>>>();
                else
                    MendeleevTable.Compos.Add(new Composition() { Name = elem });

                MendeleevTable.Compos.Find(x => x.Name == elem).Properties = prop;

                DataGridHelper.Serialize("Compositions.xml", ref MendeleevTable.Compos);
            }
            else
            {
                MendeleevTable.BinarySistem.Remove(MendeleevTable.BinarySistem.Find(x => x.TableName == elem));
                MendeleevTable.BinarySistem.Add(dat);

                DataGridHelper.Serialize("BinarySistems.xml", ref MendeleevTable.BinarySistem);
            }
        }

        /// <summary>
        /// Добавляет столбец-формулу в таблицу
        /// </summary>
        private void AddFormul_Click(object sender, RoutedEventArgs e)
        {
            FormulaInput form = new FormulaInput();
            form.ShowDialog();
            try
            {
                if (form.formula != "")
                {
                    if (form.symbol != "" && form.symbol != " ")
                        foreach (DataColumn v in dat.Columns)
                            if (v.Caption == form.symbol)
                                throw new DuplicateNameException("Такая формула уже принадлежит данной таблице!", new Exception("MyException"));

                    DataColumn col = new DataColumn('=' + form.formula) { Caption = form.symbol };
                    dat.Columns.Add(col);
                    ComposSystemTable.Columns.Add(new DataGridTextColumn()
                    {
                        Header = form.symbol + '=' + form.formula,
                        Binding = new Binding("[" + ComposSystemTable.Columns.Count + "]"),
                        IsReadOnly = true
                    });
                    for (int u = 0; u < dat.Rows.Count; u++)
                        dat.Rows[u][dat.Columns.Count - 1] = MathParser.Parse(dat.Columns[dat.Columns.Count - 1].ColumnName.Substring(1), ref dat, u);
                }                          
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message == "MyException")
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Неверный формат формулы или она уже содержится в данной таблице!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (f && dat.Columns.Count > 0 || !f && dat.Columns.Count > 1)
                DelColumn.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите закрыть окно? Все несохраненные данные будут удалены!", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
            else
                dat.RejectChanges();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width != 0)
            {
                AddRow.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                AddColumn.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                DelColumn.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                DelRow.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                AddFormul.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                Calculate.Width += (e.NewSize.Width - e.PreviousSize.Width) / 6;
                AddRow.RenderTransform = new TranslateTransform(360 + (e.NewSize.Width - 1050) / 3, 0);
                AddColumn.RenderTransform = new TranslateTransform(190 + (e.NewSize.Width - 1050) / 6, 0);
                DelRow.RenderTransform = new TranslateTransform(700 + (e.NewSize.Width - 1050) / 1.5, 0);
                DelColumn.RenderTransform = new TranslateTransform(530 + (e.NewSize.Width - 1050) / 2, 0);
                Calculate.RenderTransform = new TranslateTransform(870 + (e.NewSize.Width - 1050) / 1.2, 0);
            }
        }

        private void ComposSystemTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string str1 = (e.EditingElement as TextBox).Text;
            double i = 0;
            str1 = str1.Replace('.', ',');
            if (!double.TryParse(str1, out i))
                str1 = StringHelper.DoString(str1);
            else
                str1 = String.Format("{0:f4}", i);

            if (e.Column.DisplayIndex == 0)
                str1 = i.ToString();

            dat.Rows[e.Row.GetIndex()][e.Column.DisplayIndex] = str1;
            (e.EditingElement as TextBox).Text = str1;
        }

        /// <summary>
        /// Позволяет редактировать таблицу
        /// </summary>
        private void EditTable_Click(object sender, RoutedEventArgs e)
        {
            ComposSystemTable.IsReadOnly = false;
            EditTable.Visibility = Visibility.Hidden;
            AddColumn.Visibility = Visibility.Visible;
            AddRow.Visibility = Visibility.Visible;
            DelColumn.Visibility = Visibility.Visible;
            DelRow.Visibility = Visibility.Visible;
            Calculate.Visibility = Visibility.Visible;
            AddFormul.Visibility = Visibility.Visible;
            if (!f)
                DomeOfDecayWindowOpen.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Открывает окно построения купола распада
        /// </summary>
        private void DomeOfDecayWindowOpen_Click(object sender, RoutedEventArgs e)
        {
            new DomeOfDecay(elem).ShowDialog();
        }
    }
}
