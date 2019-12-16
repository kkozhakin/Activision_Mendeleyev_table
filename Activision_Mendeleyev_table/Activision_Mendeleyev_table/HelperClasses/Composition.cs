using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, представляющий собой соединение(элемент)
    /// </summary>
    [Serializable]
    public class Composition
    {
        /// <summary>
        /// Название соединения(элемента)
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Лист свойств и соответствующих им значений
        /// </summary>
        [XmlArray("Properties"), XmlArrayItem("Property")]
        public List<Pair<Pair<string, string>, List<string>>> Properties { get; set; }

        /// <summary>
        /// Таблица свойств элементов
        /// </summary>
        [XmlElement("Table")]
        public DataTable DataTable { get; set; }

        /// <summary>
        /// Создает объект типа Composition и инициализирует лист свойств
        /// </summary>
        public Composition() { Properties = new List<Pair<Pair<string, string>, List<string>>>(); }

        /// <summary>
        /// Создает объект типа Composition и инициализирует все свойства класса
        /// </summary>
        /// <param name="name">название соединения(элемента)</param>
        /// <param name="data">таблица свойств элементов</param>
        /// <param name="prop">лист свойств и соответсвующих им значений</param>
        public Composition(string name, DataTable data = null, List<Pair<Pair<string, string>, List<string>>> prop = null)
        {
            Properties = prop;
            DataTable = data;
            Name = name;
            if (data == null)
                DataTable = new DataTable() { TableName = name};
            if (prop == null)
                Properties = new List<Pair<Pair<string, string>, List<string>>>();
        }
    }
}
