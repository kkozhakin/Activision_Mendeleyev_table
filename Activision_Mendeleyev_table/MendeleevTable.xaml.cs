using Activision_Mendeleyev_table.HelperClasses;
using static Activision_Mendeleyev_table.HelperClasses.DataGridHelper;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для MendeleevTable.xaml
    /// </summary>
    public partial class MendeleevTable : Window
    {
        /// <summary>
        /// Лист элементов
        /// </summary>
        public static List<Composition> Elems = new List<Composition>();

        /// <summary>
        /// Лист соединений
        /// </summary>
        public static List<Composition> Compos = new List<Composition>();

        /// <summary>
        /// Лист систем соединений
        /// </summary>
        public static List<System.Data.DataTable> BinarySistem = new List<System.Data.DataTable>();

        /// <summary>
        /// Конструктор главного окна
        /// </summary>
        public MendeleevTable()
        {
            InitializeComponent();
            //Привязка горячей клавише F1 к методу OnF1Handler
            new HotKey(Key.F1, KeyModifier.None, OnF1Handler);

            List<Strings> strings = new List<Strings>(10)
            {
                new Strings(),
                new Strings("Li", "Be"),
                new Strings("Na", "Mg"),
                new Strings("K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge", "As", "Se", "Br", "Kr"),
                new Strings("Rb", "Sr", "Y", "Zr", "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd", "In", "Sn", "Sb", "Te", "I", "Xe"),
                new Strings("Cs", "Ba", "*La", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg", "Tl", "Pb", "Bi", "Po", "At", "Rn"),
                new Strings("Fr", "Ra", "**Ac"),
                new Strings(),
                new Strings("*Ce", "Pr", "Nd", "Pm", "Sm", "Eu", "Gd", "Tb", "Dy", "Ho", "Er", "Tm", "Yb", "Lu"),
                new Strings("**", "Th", "Pa", "U", "Np", "Pu", "Am", "Cm", "Bk", "Cf", "Es", "Fm", "Md", "-")
            };

            strings[0].VIIb = "H";
            strings[0].VIIIb = "He";
            strings[1].IIIb = "B";
            strings[1].IVb = "C";
            strings[1].Vb = "N";
            strings[1].VIb = "O";
            strings[1].VIIb = "F";
            strings[1].VIIIb = "Ne";
            strings[2].IIIb = "Al";
            strings[2].IVb = "Si";
            strings[2].Vb = "P";
            strings[2].VIb = "S";
            strings[2].VIIb = "Cl";
            strings[2].VIIIb = "Ar";
            strings[6].VIIIb = "I газ";

            table.Items.Clear();
            table.ItemsSource = strings;

            Deserialize("BinarySistems.xml", ref BinarySistem);
            Deserialize("Compositions.xml", ref Compos);

            if (!Deserialize("Elems.xml", ref Elems))
            {
                Elems.Add(new Composition("H"));
                Elems.Add(new Composition("Li"));
                Elems.Add(new Composition("Be"));
                Elems.Add(new Composition("Na"));
                Elems.Add(new Composition("Mg"));
                Elems.Add(new Composition("K"));
                Elems.Add(new Composition("Ca"));
                Elems.Add(new Composition("Sc"));
                Elems.Add(new Composition("Ti"));
                Elems.Add(new Composition("V"));
                Elems.Add(new Composition("Cr"));
                Elems.Add(new Composition("Mn"));
                Elems.Add(new Composition("Fe"));
                Elems.Add(new Composition("Co"));
                Elems.Add(new Composition("Ni"));
                Elems.Add(new Composition("Cu"));
                Elems.Add(new Composition("Zn"));
                Elems.Add(new Composition("Ga"));
                Elems.Add(new Composition("Ge"));
                Elems.Add(new Composition("As"));
                Elems.Add(new Composition("Se"));
                Elems.Add(new Composition("Br"));
                Elems.Add(new Composition("Kr"));
                Elems.Add(new Composition("Rb"));
                Elems.Add(new Composition("Sr"));
                Elems.Add(new Composition("Y"));
                Elems.Add(new Composition("Zr"));
                Elems.Add(new Composition("Nb"));
                Elems.Add(new Composition("Mo"));
                Elems.Add(new Composition("Tc"));
                Elems.Add(new Composition("Ru"));
                Elems.Add(new Composition("**Ac"));
                Elems.Add(new Composition("Rh"));
                Elems.Add(new Composition("Pd"));
                Elems.Add(new Composition("Ag"));
                Elems.Add(new Composition("Cd"));
                Elems.Add(new Composition("In"));
                Elems.Add(new Composition("Sn"));
                Elems.Add(new Composition("Sb"));
                Elems.Add(new Composition("Te"));
                Elems.Add(new Composition("I"));
                Elems.Add(new Composition("Xe"));
                Elems.Add(new Composition("Cs"));
                Elems.Add(new Composition("Ba"));
                Elems.Add(new Composition("Hf"));
                Elems.Add(new Composition("Ta"));
                Elems.Add(new Composition("W"));
                Elems.Add(new Composition("Re"));
                Elems.Add(new Composition("Os"));
                Elems.Add(new Composition("Ir"));
                Elems.Add(new Composition("Pt"));
                Elems.Add(new Composition("Au"));
                Elems.Add(new Composition("Hg"));
                Elems.Add(new Composition("Tl"));
                Elems.Add(new Composition("Pb"));
                Elems.Add(new Composition("Bi"));
                Elems.Add(new Composition("Po"));
                Elems.Add(new Composition("At"));
                Elems.Add(new Composition("Rn"));
                Elems.Add(new Composition("Fr"));
                Elems.Add(new Composition("Ra"));
                Elems.Add(new Composition("He"));
                Elems.Add(new Composition("B"));
                Elems.Add(new Composition("C"));
                Elems.Add(new Composition("N"));
                Elems.Add(new Composition("O"));
                Elems.Add(new Composition("F"));
                Elems.Add(new Composition("Ne"));
                Elems.Add(new Composition("Al"));
                Elems.Add(new Composition("Si"));
                Elems.Add(new Composition("P"));
                Elems.Add(new Composition("S"));
                Elems.Add(new Composition("Cl"));
                Elems.Add(new Composition("Ar"));
                Elems.Add(new Composition("I газ"));
                Elems.Add(new Composition("*Ce"));
                Elems.Add(new Composition("Pr"));
                Elems.Add(new Composition("Nd"));
                Elems.Add(new Composition("Pm"));
                Elems.Add(new Composition("Sm"));
                Elems.Add(new Composition("Eu"));
                Elems.Add(new Composition("Gd"));
                Elems.Add(new Composition("Tb"));
                Elems.Add(new Composition("Dy"));
                Elems.Add(new Composition("Ho"));
                Elems.Add(new Composition("Er"));
                Elems.Add(new Composition("Tm"));
                Elems.Add(new Composition("Yb"));
                Elems.Add(new Composition("Lu"));
                Elems.Add(new Composition("Th"));
                Elems.Add(new Composition("Pa"));
                Elems.Add(new Composition("U"));
                Elems.Add(new Composition("Np"));
                Elems.Add(new Composition("Pu"));
                Elems.Add(new Composition("Am"));
                Elems.Add(new Composition("Cm"));
                Elems.Add(new Composition("Bk"));
                Elems.Add(new Composition("Cf"));
                Elems.Add(new Composition("Es"));
                Elems.Add(new Composition("Fm"));
                Elems.Add(new Composition("Md"));
                Elems.Add(new Composition("*La"));

            }           
        }

        /// <summary>
        /// Открывает окно работы с системой соединений
        /// </summary>
        private void Compositions_Click(object sender, RoutedEventArgs e)
        {
            new CreateComposSystem(false).ShowDialog();
        }

        /// <summary>
        /// Открывает окно работы с соединением
        /// </summary>
        private void Composition_Click(object sender, RoutedEventArgs e)
        {
            new CreateComposSystem(true).ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 3; i < 12; i++)
                for (int j = 3; j < 7; j++)
                    BrushCell(j, i, Brushes.LightBlue, table);

            for (int i = 1; i <= 2; i++)
                for (int j = 1; j < 7; j++)
                    BrushCell(j, i, Brushes.LightPink, table);
            BrushCell(2, 13, Brushes.LightPink, table);

            for (int i = 0; i <= 6; i++)
                BrushCell(i, 18, Brushes.Orange, table);

            for (int i = 12; i <= 14; i++)
                for (int j = 3; j < 7; j++)
                    BrushCell(j, i, Brushes.AntiqueWhite, table);
            BrushCell(3, 14, Brushes.White, table);

            BrushCell(1, 13, Brushes.LightGreen, table);
            BrushCell(2, 14, Brushes.LightGreen, table);
            BrushCell(3, 15, Brushes.LightGreen, table);
            BrushCell(4, 16, Brushes.LightGreen, table);
            BrushCell(5, 17, Brushes.LightGreen, table);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGridCell cell;
            double height = e.NewSize.Height;
            double width = e.NewSize.Width;
            sep.Width = 850 + (width - 1280);
            for (int i = 0; i <= 19; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    cell = GetCell(j, i, table);
                    cell.Height = 68 + (height - 780) / 10;
                    cell.FontSize = 20 + (height - 780) / 100;
                }
            }
            M.FontSize = 18 + (height - 780) / 100;
            T.FontSize = 18 + (height - 780) / 100;
            B1.FontSize = 18 + (height - 780) / 100;
            B2.FontSize = 18 + (height - 780) / 100;
            R.FontSize = 18 + (height - 780) / 100;
            TR.FontSize = 18 + (height - 780) / 100;
            TranslateTransform transform = new TranslateTransform
            {
                X = -180 - (width - 1280) / 4.5,
                Y = -170 - (height - 780) / 4.5
            };
            M.RenderTransform = transform;
            transform = new TranslateTransform
            {
                X = -180 - (width - 1280) / 4.5,
                Y = 180 + (height - 780) / 4.5
            };
            T.RenderTransform = transform;
            transform = new TranslateTransform
            {
                X = 260 + (width - 1280) / 6,
                Y = 180 + (height - 780) / 4.5
            };
            B1.RenderTransform = transform;
            transform = new TranslateTransform
            {
                X = 500 + (width - 1280) / 2.5,
                Y = 180 + (height - 780) / 4.5
            };
            B2.RenderTransform = transform;
            transform = new TranslateTransform
            {
                X = 490 + (width - 1280) / 2.5,
                Y = 230 + (height - 780) / 3
            };
            R.RenderTransform = transform;
            transform = new TranslateTransform
            {
                X = 490 + (width - 1280) / 2.5,
                Y = 295 + (height - 780) / 2.5
            };
            TR.RenderTransform = transform;
        }

        /// <summary>
        /// Открывает окно работы с элементом
        /// </summary>
        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGridCellTarget = (DataGridCell)sender;
            if (dataGridCellTarget != null)
            {
                TextBlock text = dataGridCellTarget.Content as TextBlock;
                if (text.Text != "" && text.Text != "-" && text.Text != "**")
                    new ElemContent(text.Text).ShowDialog();
            }
        }

        /// <summary>
        /// Открывает окно справки
        /// </summary>
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            new Help().Show();
        }

        /// <summary>
        /// Открывает окно справки
        /// </summary>
        /// <param name="hotKey">горячая клавиша для вызова метода</param>
        private static void OnF1Handler(HotKey hotKey)
        {
            new Help().Show();
        }
    }
}