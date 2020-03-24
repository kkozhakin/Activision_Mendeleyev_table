using System.Windows;
using Activision_Mendeleyev_table.HelperClasses;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для FormulaInput.xaml
    /// </summary>
    public partial class FormulaInput : Window
    {
        /// <summary>
        /// Строка-формула
        /// </summary>
        public string formula { get; set; }

        /// <summary>
        /// Обозначение формулы
        /// </summary>
        public string symbol { get; set; }

        /// <summary>
        /// Конструктор, инициализирующий окно создания столбца-формулы
        /// </summary>
        public FormulaInput()
        {
            InitializeComponent();
            formula = "";
            symbol = "";
        }

        /// <summary>
        /// Создание столбца-формулы
        /// </summary>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            formula = StringHelper.DoString(NewFormula.Text);
            symbol = StringHelper.DoString(FormulaSymbol.Text);
            Close();
        }

        /// <summary>
        /// Отмена изменений и закрытие окна
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Просмотр списка формул
        /// </summary>
        private void OpenFormuls_Click(object sender, RoutedEventArgs e)
        {
            new FormulaList().Show();
        }
    }
}
