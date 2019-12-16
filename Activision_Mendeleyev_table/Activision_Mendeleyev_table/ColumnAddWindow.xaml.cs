using System.Windows;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для ColumnAddWindow.xaml
    /// </summary>
    public partial class ColumnAddWindow : Window
    {
        /// <summary>
        /// Название столбца
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// Обозначение свойства, значения которого будут находиться в данном столбце
        /// </summary>
        public string symbol { get; private set; }

        /// <summary>
        /// Конструктор, инициализирующий окно создания текстового столбца
        /// </summary>
        public ColumnAddWindow()
        {
            InitializeComponent();
            name = "";
            symbol = "";
        }

        /// <summary>
        /// Создание текстового столбца
        /// </summary>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            name = ColumnName.Text;
            symbol = ColumnSymbol.Text;
            Close();
        }

        /// <summary>
        /// Отмена изменений и закрытие окна
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
