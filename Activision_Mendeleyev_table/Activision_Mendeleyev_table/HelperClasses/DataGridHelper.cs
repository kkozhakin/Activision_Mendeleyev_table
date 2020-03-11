using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Activision_Mendeleyev_table.HelperClasses
{   
    /// <summary>
    ///Вспомогательный статический класс для работы с DataGrid 
    /// </summary>
    public static class DataGridHelper
    {
        /// <summary>
        /// Вспомогательный метод для получения ячейки таблицы DataGrid
        /// </summary>
        /// <typeparam name="T">Visual type</typeparam>
        /// <param name="parent">предок</param>
        /// <returns>потомок</returns>
        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            try
            {
                T child = default(T);
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                        child = GetVisualChild<T>(v);

                    if (child != null)
                        break;
                }
                return child;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения данных из таблицы функция (GetVisualChild<T>)!\n\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

        }

        /// <summary>
        /// Получает строку из DataGrid
        /// </summary>
        /// <param name="index">номер строки</param>
        /// <param name="dg">DataGrid</param>
        /// <returns>Строку DataGrid</returns>
        private static DataGridRow GetRow(int index, DataGrid dg)
        {
            try
            {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
                if (row == null)
                {
                    dg.UpdateLayout();
                    dg.ScrollIntoView(dg.Items[index]);
                    row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);

                }
                return row;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения данных из таблицы функция (GetRow)!\n\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Добавляет текстовый столбец в таблицу
        /// </summary>
        /// <param name="dg">DataGrid</param>
        /// <param name="dat">таблица данных</param>
        /// <param name="f">флаг: true - соединение(элемент), false - система</param>
        /// <returns>Можно ли удалять столбцы?</returns>
        public static bool AddColumn(ref DataGrid dg, ref DataTable dat, bool f = true)
        {
            ColumnAddWindow form = new ColumnAddWindow();
            form.ShowDialog();
            try
            {
                if (form.name != "")
                {
                    if (form.symbol != "" && form.symbol != " ")
                        foreach (DataColumn v in dat.Columns)
                            if (v.Caption == form.symbol)
                                throw new DuplicateNameException();

                    DataColumn col = new DataColumn(form.name) { Caption = form.symbol };
                    dat.Columns.Add(col);
                    dg.Columns.Add(new DataGridTextColumn()
                    {
                        Header = (form.symbol == "" || form.symbol == " ") ? form.name : form.name + ", " + form.symbol,
                        Binding = new Binding("[" + dg.Columns.Count + "]")
                    });
                }
            }
            catch (DuplicateNameException)
            {
                MessageBox.Show("Столбец с данным именем(обозначением) уже принадлежит данной таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (f && dat.Columns.Count > 0 || !f && dat.Columns.Count > 1)
                return true;
            return false;
        }

        /// <summary>
        /// Получает ячейку из DataGrid
        /// </summary>
        /// <param name="row">номер строки</param>
        /// <param name="column">номер столбца</param>
        /// <param name="dg">DataGrid</param>
        /// <returns>Ячейка DataGrid</returns>
        public static DataGridCell GetCell(int row, int column, DataGrid dg)
        {
            try
            {
                DataGridRow rowContainer = GetRow(row, dg);

                if (rowContainer != null)
                {
                    System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);

                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    if (cell == null)
                    {
                        dg.ScrollIntoView(rowContainer, dg.Columns[column]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    }
                    return cell;
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения данных из таблицы функция (GetCell)!\n\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Закрашивает ячейку DataGrid
        /// </summary>
        /// <param name="row">номер строки</param>
        /// <param name="column">номер столбца</param>
        /// <param name="color">цвет для закрашивания</param>
        /// <param name="dg">DataGrid</param>
        public static void BrushCell(int row, int column, Brush color, DataGrid dg)
        {
            DataGridCell cell = GetCell(row, column, dg);
            cell.Background = color;
            cell.BorderBrush = color;
        }

        /// <summary>
        /// Метод сериализации соединений/элементов/систем соединений
        /// </summary>
        /// <typeparam name="T">тип: List<Composition> или List<DataTable></typeparam>
        /// <param name="name">имя файла</param>
        /// <param name="data">лист соединений/элементов/систем соединений</param>
        public static void Serialize<T>(string name, ref System.Collections.Generic.List<T> data)
        {
            using (FileStream fs = new FileStream(name, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                serializer.Serialize(fs, data);
            }
        }

        /// <summary>
        /// Метод десериализации соединений/элементов/систем соединений
        /// </summary>
        /// <typeparam name="T">тип: List<Composition> или List<DataTable></typeparam>
        /// <param name="name">имя файла</param>
        /// <param name="data">лист соединений/элементов/систем соединений</param>
        /// <returns>Удалось ли десериализовать?</returns>
        public static bool Deserialize<T>(string name, ref System.Collections.Generic.List<T> data)
        {
            if (!File.Exists(name))
                return false;

            try
            {
                using (FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(data.GetType());
                    data = (System.Collections.Generic.List<T>)serializer.Deserialize(fs);
                }
                return true;
            }
            catch (FileLoadException)
            {
                return false;
            }
        }
    }
}
