namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Вспомогательный класс для заполнения Таблицы Менделеева
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// Задает один ряд элементов по группам и подгруппам
        /// </summary>
        public Strings(string ia = null, string iia = null, string iiia = null, string iva = null, string va = null, string via = null, string viia = null,
            string viiia_1 = null, string viiia_2 = null, string viiia_3 = null, string ib = null, string iib = null, string iiib = null, string ivb = null,
            string vb = null, string vib = null, string viib = null, string viiib = null)
        {
            Ia = ia;
            IIa = iia;
            IIIa = iiia;
            IVa = iva;
            Va = va;
            VIa = via;
            VIIa = viia;
            VIIIa_1 = viiia_1;
            VIIIa_2 = viiia_2;
            VIIIa_3 = viiia_3;
            Ib = ib;
            IIb = iib;
            IIIb = iiib;
            IVb = ivb;
            Vb = vb;
            VIb = vib;
            VIIb = viib;
            VIIIb = viiib;
        }

        public string Ia { get; set; }
        public string IIa { get; set; }
        public string IIIa { get; set; }
        public string IVa { get; set; }
        public string Va { get; set; }
        public string VIa { get; set; }
        public string VIIa { get; set; }
        public string VIIIa_1 { get; set; }
        public string VIIIa_2 { get; set; }
        public string VIIIa_3 { get; set; }
        public string Ib { get; set; }
        public string IIb { get; set; }
        public string IIIb { get; set; }
        public string IVb { get; set; }
        public string Vb { get; set; }
        public string VIb { get; set; }
        public string VIIb { get; set; }
        public string VIIIb { get; set; }
    }

    /// <summary>
    /// Класс для преобразования строк по определенным правилам
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Преобразует определённый символ в соответствующий подстрочный
        /// </summary>
        /// <param name="c">символ</param>
        /// <returns>подстрочный символ</returns>
        private static char Substring(char c)
        {
            switch (c)
            {
                case '0':
                    return '₀';
                case '1':
                    return '₁';
                case '2':
                    return '₂';
                case '3':
                    return '₃';
                case '4':
                    return '₄';
                case '5':
                    return '₅';
                case '6':
                    return '₆';
                case '7':
                    return '₇';
                case '8':
                    return '₈';
                case '9':
                    return '₉';
                case '+':
                    return '₊';
                case '-':
                    return '₋';
                case '=':
                    return '₌';
                case '(':
                    return '₍';
                case ')':
                    return '₎';
                case '.':
                case ',':
                    return '.';
                default:
                    return c;
            }
        }

        /// <summary>
        /// Преобразует определённый символ в соответствующий надстрочный
        /// </summary>
        /// <param name="c">символ</param>
        /// <returns>надстрочный символ</returns>
        private static char Superstring(char c)
        {
            switch (c)
            {
                case '0':
                    return '⁰';
                case '1':
                    return '¹';
                case '2':
                    return '²';
                case '3':
                    return '³';
                case '4':
                    return '⁴';
                case '5':
                    return '⁵';
                case '6':
                    return '⁶';
                case '7':
                    return '⁷';
                case '8':
                    return '⁸';
                case '9':
                    return '⁹';
                case '+':
                    return '⁺';
                case '-':
                    return '⁻';
                case '=':
                    return '⁼';
                case '(':
                    return '⁽';
                case ')':
                    return '⁾';
                case '.':
                case ',':
                    return '·';//'ʼ';
                default:
                    return c;
            }
        }

        /// <summary>
        /// Преобразует некоторые символы строки в над(под)строчные по заданным правилам
        /// </summary>
        /// <param name="str">исходная строка</param>
        /// <returns>преобразованная строка</returns>
        public static string DoString(string str)
        {
            int i = 0;
            string str1 = "";
            while (i < str.Length)
            {
                // Преобразует символы после комбинации _{ и до } в подстрочные
               if (i < str.Length && str[i] == '_')
                {
                    i++;
                    if (str[i] != '{')
                        str1 += Substring(str[i]);
                    else
                    {
                        i++;
                        while (i < str.Length && str[i] != '}')
                        {
                            str1 += Substring(str[i]);
                            i++;
                        }
                    }
                    i++;
                }
                // Преобразует символы после комбинации ^{ и до } в надстрочные
                if (i < str.Length && str[i] == '^')
                {
                    i++;
                    if (str[i] != '{')
                        str1 += Superstring(str[i]);
                    else
                    {
                        i++;
                        while (i < str.Length && str[i] != '}')
                        {
                            str1 += Superstring(str[i]);
                            i++;
                        }
                    }
                    i++;
                }
                if (i < str.Length && str[i] != '_')
                {
                    // Преобразует символы после буквенных и подстрочных символов в подстрочные
                    if (i > 0 && "0123456789+-".Contains(str[i].ToString()) && "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎)QWERTYUIOPASDFGHJKLZXCVVBNMqwertyuiopasdfghjklzxcvbnm".Contains(str1[i - 1].ToString()))                       
                        str1 += Substring(str[i]);
                    else
                        if (i > 1 && '.' == str[i - 1] && "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎()QWERTYUIOPASDFGHJKLZXCVVBNMqwertyuiopasdfghjklzxcvbnm".Contains(str1[i - 2].ToString()))
                        str1 += Substring(str[i]);
                    else
                        str1 += str[i];
                    i++;
                }
            }
            return str1;
        }
    }
}
