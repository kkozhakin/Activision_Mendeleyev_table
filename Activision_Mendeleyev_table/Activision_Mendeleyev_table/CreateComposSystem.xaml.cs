using System.Windows;
using Activision_Mendeleyev_table.HelperClasses;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для CreateComposSystem.xaml
    /// </summary>
    public partial class CreateComposSystem : Window
    {
        /// <summary>
        /// Флаг: true - соединение, false - система
        /// </summary>
        private bool f;

        /// <summary>
        /// Конструктор, инициализирующий окно создания/выбора соединения(системы соединений)
        /// </summary>
        /// <param name="f">флаг: true - соединение, false - система</param>
        public CreateComposSystem(bool f)
        {
            InitializeComponent();

            this.f = f;

            if (f)
            {
                this.Title = "Выберите/введите соединение";
                label.Content = "Введите новое соединения";
                for (int i = 0; i < MendeleevTable.Compos.Count; i++)
                    ComposSystem.Items.Add(MendeleevTable.Compos[i].Name);
            }
            else
            {
                label.Content = "Введите новую систему соединений";
                this.Title = "Выберите/введите систему соединений";
                for (int i = 0; i < MendeleevTable.BinarySistem.Count; i++)
                    ComposSystem.Items.Add(MendeleevTable.BinarySistem[i].TableName);
            }

            if (ComposSystem.Items.Count > 0)
                ComposSystem.SelectedIndex = 0;
        }

        /// <summary>
        /// Открывает таблицу созданного/выбранного соединения(системы соединений)
        /// </summary>
        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ComposSystemContent(StringHelper.DoString(NewComposSystem.Text), f).ShowDialog();
            this.Close();
        }

        /// <summary>
        /// Дублирует соединение(систему соединений) из ComboBox в TextBox
        /// </summary>
        private void Compos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            NewComposSystem.Text = e.AddedItems[0] as string;
        }
    }
}
