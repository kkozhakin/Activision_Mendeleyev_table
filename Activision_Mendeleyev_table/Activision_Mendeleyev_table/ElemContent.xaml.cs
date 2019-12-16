using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Activision_Mendeleyev_table.HelperClasses;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для ElemContent.xaml
    /// </summary>
    public partial class ElemContent : Window
    {
        /// <summary>
        /// Таблица данных
        /// </summary>
        private DataTable dat;

        /// <summary>
        /// Обозначение элемента
        /// </summary>
        private string elem;

        /// <summary>
        /// Конструктор, инициализирующий окно таблицы элемента
        /// </summary>
        /// <param name="elem">название элемента</param>
        public ElemContent(string elem)
        {
            InitializeComponent();
            this.Title = "Свойства элемента " + elem;
            this.elem = elem;

            Composition comp = MendeleevTable.Elems.Find(x => x.Name == elem);
            if (comp != null)
            {
                //Заполнение строк
                dat = comp.DataTable;
                for (int i = 0; i < comp.Properties.Count; i++)
                    for (int j = 0; j < dat.Columns.Count; j++)
                        if (dat.Columns[j].ColumnName == comp.Properties[i].First.First)
                            for (int k = 0; k < comp.Properties[i].Second.Count; k++)
                            {
                                if (dat.Rows.Count <= k)
                                    dat.Rows.Add();
                                dat.Rows[k][j] = comp.Properties[i].Second[k];
                            }
            }

            if (dat == null)
                dat = new DataTable() { TableName = elem };
            else
            {
                //Визуализация столбцов
                foreach (DataColumn i in dat.Columns)
                {
                    ElemTable.Columns.Add(new DataGridTextColumn()
                    {
                        Header = (i.Caption == "" || i.Caption == " ") ? i.ColumnName : i.ColumnName + ", " + i.Caption,
                        Binding = new Binding("[" + ElemTable.Columns.Count + "]")
                    });
                }
            }

            ElemTable.ItemsSource = dat.DefaultView;
            if (dat.Columns.Count > 0)
                DelColumn.IsEnabled = true;
            if (dat.Rows.Count > 0)
                DelRow.IsEnabled = true;
        }

        /// <summary>
        /// Добавляет текстовый столбец в таблицу
        /// </summary>
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            DelColumn.IsEnabled = DataGridHelper.AddColumn(ref ElemTable, ref dat);
        }

        /// <summary>
        /// Добавляет строку в таблицу
        /// </summary>
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            dat.Rows.Add();
            if (dat.Rows.Count > 0)
                DelRow.IsEnabled = true;
            CollectionViewSource.GetDefaultView(ElemTable.ItemsSource).Refresh();

        }

        /// <summary>
        /// Удаляет столбец в таблицу
        /// </summary>
        private void DelColumn_Click(object sender, RoutedEventArgs e)
        {
            ElemTable.Columns.RemoveAt(ElemTable.Columns.Count - 1);
            if (dat.Columns.Count <= 1)
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
            CollectionViewSource.GetDefaultView(ElemTable.ItemsSource).Refresh();
        }

        /// <summary>
        /// Сохраняет данные в файл
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ElemTable.IsReadOnly = true;
            EditTable.Visibility = Visibility.Visible;
            AddColumn.Visibility = Visibility.Hidden;
            AddRow.Visibility = Visibility.Hidden;
            DelColumn.Visibility = Visibility.Hidden;
            DelRow.Visibility = Visibility.Hidden;
            Save.Visibility = Visibility.Hidden;

            dat.AcceptChanges();
            List<Pair<Pair<string, string>, List<string>>> prop = new List<Pair<Pair<string, string>, List<string>>>();

            if (prop == null)
                prop = new List<Pair<Pair<string, string>, List<string>>>();

            for (int i = 0; i < dat.Columns.Count; i++)
            {
                prop.Add(new Pair<Pair<string, string>, List<string>>(new Pair<string, string>(dat.Columns[i].ColumnName, dat.Columns[i].Caption), new List<string>()));
                for (int j = 0; j < dat.Rows.Count; j++)
                    prop[i].Second.Add(dat.Rows[j][i].ToString());
            }

            Composition el = MendeleevTable.Elems.Find(x => x.Name == elem);
            if (el != null)
                MendeleevTable.Elems.Find(x => x.Name == elem).Properties = new List<Pair<Pair<string, string>, List<string>>>();
            else
                MendeleevTable.Elems.Add(new Composition() { Name = elem });

            MendeleevTable.Elems.Find(x => x.Name == elem).Properties = prop;

            DataTable d = dat.Copy();
            d.Rows.Clear();
            for (int i = 0; i < MendeleevTable.Elems.Count; i++)
                MendeleevTable.Elems[i].DataTable = d;

            DataGridHelper.Serialize("Elems.xml", ref MendeleevTable.Elems);
        }

        private void ElemContent_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        { 
            string str1 = StringHelper.DoString((e.EditingElement as TextBox).Text);
            str1 = str1.Replace('.', ',');
            dat.Rows[e.Row.GetIndex()][e.Column.DisplayIndex] = str1;
            (e.EditingElement as TextBox).Text = str1;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите закрыть окно? Все несохраненные данные будут удалены!", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
            else
                dat.RejectChanges();
        }

        /// <summary>
        /// Позволяет редактировать таблицу
        /// </summary>
        private void EditTable_Click(object sender, RoutedEventArgs e)
        {
            ElemTable.IsReadOnly = false;
            EditTable.Visibility = Visibility.Hidden;
            AddColumn.Visibility = Visibility.Visible;
            AddRow.Visibility = Visibility.Visible;
            DelColumn.Visibility = Visibility.Visible;
            DelRow.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Visible;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width != 0)
            {
                AddRow.Width += (e.NewSize.Width - e.PreviousSize.Width) / 5;
                AddColumn.Width += (e.NewSize.Width - e.PreviousSize.Width) / 5;
                DelColumn.Width += (e.NewSize.Width - e.PreviousSize.Width) / 5;
                DelRow.Width += (e.NewSize.Width - e.PreviousSize.Width) / 5;
                Save.Width += (e.NewSize.Width - e.PreviousSize.Width) / 5;
                DelColumn.RenderTransform = new TranslateTransform(360 + (e.NewSize.Width - 880) / 2.5, 0);
                AddRow.RenderTransform = new TranslateTransform(190 + (e.NewSize.Width - 880) / 5, 0);
                Save.RenderTransform = new TranslateTransform(700 + (e.NewSize.Width - 880) / 1.25, 0);
                DelRow.RenderTransform = new TranslateTransform(530 + (e.NewSize.Width - 880) / 1.66, 0);
            }
        }
    }
}
