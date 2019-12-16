using System;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, который предоставляет возможность хранить два разнородных объекта как единое целое
    /// </summary>
    /// <typeparam name="T">тип первого аргумента</typeparam>
    /// <typeparam name="U">тип второго аргумента</typeparam>
    [Serializable]
    public class Pair<T, U>
    {
        /// <summary>
        /// Первый аргумент
        /// </summary>
        public T First { get; set; }

        /// <summary>
        /// Второй аргумент
        /// </summary>
        public U Second { get; set; }

        /// <summary>
        /// Создает объект типа Pair
        /// </summary>
        public Pair() { }

        /// <summary>
        /// Создает объект типа Pair и инициализирует значение  аргументов
        /// </summary>
        /// <param name="first">первый аргумент</param>
        /// <param name="second">второй аргумент</param>
        public Pair(T first, U second)
        {
            First = first;
            Second = second;
        }        
    };
}
