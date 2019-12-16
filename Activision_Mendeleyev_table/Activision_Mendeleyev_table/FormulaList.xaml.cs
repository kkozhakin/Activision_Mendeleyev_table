using System.Data;
using System.Windows;

namespace Activision_Mendeleyev_table
{
    /// <summary>
    /// Логика взаимодействия для FormulaList.xaml
    /// </summary>
    public partial class FormulaList : Window
    {
        public FormulaList()
        {
            InitializeComponent();
            DataTable dat = new DataTable();
            dat.Columns.Add("Обозначение");
            dat.Columns.Add("Название/формула");
            foreach (DataColumn i in dat.Columns)
            {
                formuls.Columns.Add(new System.Windows.Controls.DataGridTextColumn()
                {
                    Header = i.ColumnName,
                    Binding = new System.Windows.Data.Binding("[" + formuls.Columns.Count + "]")
                });
            }
            for (int i = 0; i < MendeleevTable.Elems[0].DataTable.Columns.Count; i++)
                dat.Rows.Add(MendeleevTable.Elems[0].DataTable.Columns[i].Caption != null? MendeleevTable.Elems[0].DataTable.Columns[i].Caption:"", MendeleevTable.Elems[0].DataTable.Columns[i].ColumnName);
            for (int i = 0; i < MendeleevTable.Compos.Count; i++)
                for (int j = 0; j < MendeleevTable.Compos[i].Properties.Count; j++)
                    dat.Rows.Add(MendeleevTable.Compos[i].Properties[j].First.First != null ? MendeleevTable.Compos[i].Properties[j].First.Second : "", (MendeleevTable.Compos[i].Properties[j].First.First[0] != '=')?MendeleevTable.Compos[i].Properties[j].First.First: MendeleevTable.Compos[i].Properties[j].First.First.Substring(1));
            for (int i = 0; i < MendeleevTable.BinarySistem.Count; i++)
                for (int j = 0; j < MendeleevTable.BinarySistem[i].Columns.Count; j++)
                    dat.Rows.Add(MendeleevTable.BinarySistem[i].Columns[j].Caption != null ? MendeleevTable.BinarySistem[i].Columns[j].Caption : "", (MendeleevTable.BinarySistem[i].Columns[j].ColumnName[0] != '=')?MendeleevTable.BinarySistem[i].Columns[j].ColumnName: MendeleevTable.BinarySistem[i].Columns[j].ColumnName.Substring(1));

            formuls.ItemsSource = dat.DefaultView;
        }
    }
}
