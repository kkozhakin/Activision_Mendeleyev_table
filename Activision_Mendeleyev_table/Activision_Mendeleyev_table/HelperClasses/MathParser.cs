using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс, вычисляющей значение формулы по введенной строке по определенным правилам
    /// </summary>
    public static class MathParser
    {
        /// <summary>
        /// Флаг: true - соединений, false - система
        /// </summary>
        public static bool f = true;

        /// <summary>
        /// Парсинг строки для вычисления значения формулы, если данная строка является формулой
        /// </summary>
        /// <param name="str">содержание ячайки таблицы</param>
        /// <param name="dat">таблица, в которой производим вычисление</param>
        /// <param name="u">номер строки</param>
        /// <returns>результат парсинга</returns>
        public static double Parse(string str, ref DataTable dat, int u)
        {
            str = str.Replace(',', '.');
            string left = "", right = "", inner = "";
            int i = 0, j = 0, k = 0;

            // Парсинг функций
            string[] func = { "sin", "cos", "tan", "ctan", "abs", "exp", "ln", "min", "max", "pow" };
            for (i = 0; i < func.Length; i++)
            {
                k = str.IndexOf(func[i]);
                if (k >= 0)
                {
                    left = str.Substring(0, k);
                    k += func[i].Length;
                    j = 0;
                    bool f = false;
                    right = "";
                    inner = "";
                    while (k < str.Length)
                    {
                        if (!f && str[k] == '(' && k < str.Length)
                        {
                            j++;
                            if (j == 1)
                            {
                                left += right;
                                right = "";
                                k++;
                            }
                        }

                        right += str[k];

                        if (!f && k < str.Length - 1 && str[k + 1] == ')')
                        {
                            j--;
                            if (j == 0)
                            {
                                inner += right;
                                right = "";
                                f = true;
                                k++;
                            }
                        }

                        k++;
                    }

                    switch (i)
                    {
                        case 0:
                            return Parse(left + Math.Sin(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 1:
                            return Parse(left + Math.Cos(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 2:
                            return Parse(left + Math.Tan(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 3:
                            return Parse(left + 1.0 / Math.Tan(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 4:
                            return Parse(left + Math.Abs(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 5:
                            return Parse(left + Math.Exp(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 6:
                            return Parse(left + Math.Log(Parse(inner, ref dat, u)) + right, ref dat, u);

                        case 7:
                            int a = 0, b = 0, v = 0, g = 0;
                            string inleft = "";
                            while (inner[g] != ';' || a != 0 || b != 0 || v != 0)
                            {
                                inleft += inner[g];
                                if (inner[g] == '(')
                                    a++;
                                else if (inner[g] == ')')
                                    a--;
                                else if (inner[g] == '[')
                                    b++;
                                else if (inner[g] == ']')
                                    b--;
                                else if (inner[g] == '{')
                                    v++;
                                else if (inner[g] == '}')
                                    v--;
                                g++;
                            }
                            
                            string inright = inner.Substring(g + 1);
                            return Parse(left + Math.Min(Parse(inleft, ref dat, u), Parse(inright, ref dat, u)) + right, ref dat, u);

                        case 8:
                            a = 0; b = 0; v = 0; g = 0;
                            inleft = "";
                            while (inner[g] != ';' || a != 0 || b != 0 || v != 0)
                            {
                                inleft += inner[g];
                                if (inner[g] == '(')
                                    a++;
                                else if (inner[g] == ')')
                                    a--;
                                else if (inner[g] == '[')
                                    b++;
                                else if (inner[g] == ']')
                                    b--;
                                else if (inner[g] == '{')
                                    v++;
                                else if (inner[g] == '}')
                                    v--;
                                g++;
                            }

                             inright = inner.Substring(g + 1);
                            return Parse(left + Math.Max(Parse(inleft, ref dat, u), Parse(inright, ref dat, u)) + right, ref dat, u);

                        case 9:
                            a = 0; b = 0; v = 0; g = 0;
                            inleft = "";
                            while (inner[g] != ';' || a != 0 || b != 0 || v != 0)
                            {
                                inleft += inner[g];
                                if (inner[g] == '(')
                                    a++;
                                else if (inner[g] == ')')
                                    a--;
                                else if (inner[g] == '[')
                                    b++;
                                else if (inner[g] == ']')
                                    b--;
                                else if (inner[g] == '{')
                                    v++;
                                else if (inner[g] == '}')
                                    v--;
                                g++;
                            }

                            inright = inner.Substring(g + 1);
                            return Parse(left + Math.Pow(Parse(inleft, ref dat, u), Parse(inright, ref dat, u)) + right, ref dat, u);
                    }
                }
            }

            //Парсинг символа x
            Match matchFuncx = Regex.Match(str, @"(x)");
            if (matchFuncx.Groups.Count > 1)
            {
                left = str.Substring(0, matchFuncx.Index);
                right = str.Substring(matchFuncx.Index + matchFuncx.Length);
                return Parse(left + double.Parse(dat.Rows[u]["X"].ToString()) + right, ref dat, u);
            }

            //Парсинг бесконечности
            Match matchinf = Regex.Match(str, @"(∞)");
            if (matchinf.Groups.Count > 1)
            {
                left = str.Substring(0, matchinf.Index);
                right = str.Substring(matchinf.Index + matchinf.Length);
                return Parse(left + "10000000000000000000" + right, ref dat, u);
            }

            string str1 = "";
            int r = -1, c = -1;
            //Парсинг конструкций для получения значений из таблиц
            try
            {
                Match matchElem = Regex.Match(str, @"([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉]*){([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉ ]*){([\[\]\d\.\+\-\*\/%\^_\(\) ]*)}([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉ ]*)}|([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉]*){([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉ ]*)}");
                if (matchElem.Groups.Count > 1)
                {
                    inner = StringHelper.DoString(str.Substring(matchElem.Index, matchElem.Length)).Trim(' ');
                    left = str.Substring(0, matchElem.Index);
                    right = str.Substring(matchElem.Index + matchElem.Length);
                    LinkedList<string> strs = new LinkedList<string>();
                    string symbol = "";
                    k = 0;
                    while (k < inner.Length)
                    {
                        if (inner[k] == '{')
                        {
                            k++;
                            while (k < inner.Length && inner[k] != '}' && inner[k] != '{')
                            {
                                str1 += inner[k];
                                k++;
                            }
                            strs.AddLast(str1);
                            str1 = "";
                        }
                        else if (inner[k] == '}')
                        {
                            k++;
                            while (k < inner.Length && inner[k] != '}' && inner[k] != '{')
                            {
                                str1 += inner[k];
                                k++;
                            }
                            strs.AddFirst(str1);
                            str1 = "";
                        }
                        else
                        {
                            symbol += inner[k];
                            k++;
                        }
                    }
                    strs.AddLast(symbol);
                    strs.RemoveFirst();
                    return Parse(left + Find(strs, u).ToString() + right, ref dat, u);
                }

                matchElem = Regex.Match(str, @"([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉]*){([\d ]*;[\d ]*)}");
                if (matchElem.Groups.Count > 1)
                {
                    inner = StringHelper.DoString(str.Substring(matchElem.Index, matchElem.Length)).Trim(' ');
                    left = str.Substring(0, matchElem.Index);
                    right = str.Substring(matchElem.Index + matchElem.Length);
                    str1 = "";
                    k = 0;
                    Composition e = null;
                    DataTable d = null;

                    while (k < inner.Length)
                    {
                        if (inner[k] == '{')
                        {
                            k++;
                            e = MendeleevTable.Elems.Find(x => x.Name == str1);
                            if (e == null)
                                e = MendeleevTable.Compos.Find(x => x.Name == str1);

                            if (e == null)
                                d = MendeleevTable.BinarySistem.Find(x => x.TableName == str1);

                            if (d == null & e == null)
                                throw new Exception("Отсутствует соединение, элемент или система" + str1 + ".", new Exception("MyException"));
                            str1 = "";
                        }
                        else if (inner[k] == ';')
                        {
                            k++;
                            int.TryParse(str1, out r);
                            str1 = "";
                        }
                        else if (inner[k] == '}')
                        {
                            k++;
                            int.TryParse(str1, out c);
                            str1 = "";
                        }
                        else
                        {
                            str1 += inner[k];
                            k++;
                        }
                    }
                    if (e != null)
                        return Parse(left + e.Properties[c].Second[r] + right, ref dat, u);
                    return Parse(left + d.Rows[r][c] + right, ref dat, u);
                }

                matchElem = Regex.Match(str, @"([\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉]*){([\d ]*;[\w\[\]\.\+\-\*\/%\^_\(\)₀₁₂₃₄₅₆₇₈₉ ]*)}");
                if (matchElem.Groups.Count > 1)
                {
                    inner = StringHelper.DoString(str.Substring(matchElem.Index, matchElem.Length)).Trim(' ');
                    left = str.Substring(0, matchElem.Index);
                    right = str.Substring(matchElem.Index + matchElem.Length);
                    str1 = "";
                    k = 0;
                    Composition e = null;
                    DataTable d = null;

                    while (k < inner.Length)
                    {
                        if (inner[k] == '{')
                        {
                            k++;
                            e = MendeleevTable.Elems.Find(x => x.Name == str1);
                            if (e == null)
                                e = MendeleevTable.Compos.Find(x => x.Name == str1);

                            if (e == null)
                                d = MendeleevTable.BinarySistem.Find(x => x.TableName == str1);

                            if (d == null & e == null)
                                throw new Exception("Отсутствует соединение, элемент или система" + str1 + ".", new Exception("MyException"));
                            str1 = "";
                        }
                        else if (inner[k] == ';')
                        {
                            k++;
                            int.TryParse(str1, out r);
                            str1 = "";
                        }
                        else if (inner[k] == '}')
                        {
                            k++;
                            if (e != null)
                            {
                                for (c = 0; c < e.Properties.Count; c++)
                                    if (e.Properties[c].First.Second == str1)
                                        break;
                            }
                            else
                                for (c = 0; c < d.Columns.Count; c++)
                                    if (d.Columns[c].Caption == str1)
                                        break;
                            str1 = "";
                        }
                        else
                        {
                            str1 += inner[k];
                            k++;
                        }
                    }
                    if (e != null)
                        return Parse(left + e.Properties[c].Second[r] + right, ref dat, u);
                    return Parse(left + d.Rows[r][c] + right, ref dat, u);
                }              
            }
            catch (FormatException)
            {
                throw new FormatException(string.Format("Неверная входная строка '{0}'", str), new Exception("MyException"));
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new FormatException(string.Format("Отсутствует строка {0} или стобец {1} у данного элемента/соединения.", r, c), new Exception("MyException"));
            }
            catch (NullReferenceException)
            {
                throw new FormatException("Отсутствует содержание ячейки, к которой вы обратились.", new Exception("MyException"));
            }

            // Парсинг скобок
            Match brackets = Regex.Match(str, @"\(([\w\d\[\]\.\+\-\*\/%\^_\(\) ]*)\)");
            if (brackets.Groups.Count > 1)
            {
                i = 0;
                j = 0;
                left = "";
                right = "";
                inner = "";
                while (i < str.Length)
                {
                    if (str[i] == '(' && i < str.Length)
                    {
                        j++;
                        if (j == 1)
                        {
                            left += right;
                            right = "";
                            i++;
                        }
                    }

                    if (i < str.Length && str[i] == ')')
                    {
                        j--;
                        if (j == 0)
                        {
                            inner += right;
                            right = str.Substring(i + 1);
                            return Parse(left + Parse(inner, ref dat, u) + right, ref dat, u);
                        }
                    }

                    right += str[i];
                    i++;
                }
                if (inner != "")
                    return Parse(left + Parse(inner, ref dat, u) + right, ref dat, u);
            }
            
            // Парсинг действий
            Match matchMulOp = Regex.Match(str, string.Format(@"({0})\s?({1})\s?({0})\s?", RegexNum, RegexMulOp));
            Match matchAddOp = Regex.Match(str, string.Format(@"({0})\s?({1})\s?({0})\s?", RegexNum, RegexAddOp));
            var match = (matchMulOp.Groups.Count > 1) ? matchMulOp : (matchAddOp.Groups.Count > 1) ? matchAddOp : null;
            if (match != null)
            {
                left = str.Substring(0, match.Index);
                right = str.Substring(match.Index + match.Length);
                return Parse(left + ParseAct(match).ToString(CultureInfo.InvariantCulture) + right, ref dat, u);
            }

            // Парсинг числа
            str1 = "";
            for (i = 0; i < str.Length; i++)
                if (str[i] != ' ')
                    if (str[i] == '.')
                        str1 += ',';
                    else
                        str1 += str[i];
            if (double.TryParse(str1, out double num))
                return num;
            else
                throw new FormatException(string.Format("Неверная входная строка '{0}'", str), new Exception("MyException"));
        }

        /// <summary>
        /// Форматная строка, соответствующая числу
        /// </summary>
        private const string RegexNum = @"[-]?\d+\.?\d*";
        /// <summary>
        /// Форматная строка, соответствующая операциям умножения и деления
        /// </summary>
        private const string RegexMulOp = @"[\*\/%]";
        /// <summary>
        /// Форматная строка, соответствующая операциям сложения и вычитания
        /// </summary>
        private const string RegexAddOp = @"[\+\-]";

        /// <summary>
        /// Выполнение математических операций
        /// </summary>
        /// <param name="match">результаты вычисления регулярного выражения, определяющего мат. операции</param>
        /// <returns>результат вычисления</returns>
        private static double ParseAct(Match match)
        {
            double a = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            double b = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            switch (match.Groups[2].Value)
            {
                case "+":
                    return a + b;

                case "-":
                    return a - b;

                case "*":
                    return a * b;

                case "/":
                    return a / b;

                case "%":
                    return a % b;

                default:
                    throw new FormatException(string.Format("Неверная входная строка '{0}'", match.Value), new Exception("MyException"));
            }
        }

        /// <summary>
        /// Поиск значения свойства в таблицах
        /// </summary>
        /// <param name="strs">лист, включающий название соединения(элемента), обозначение свойства и, если необходимо, доп. свойства и его значение</param>
        /// <returns>значение свойсва</returns>
        private static double Find(LinkedList<string> strs, int u)
        {
            LinkedListNode<string> str = strs.First;
            DataTable d = MendeleevTable.BinarySistem.Find(x => x.TableName == str.Value);
            Composition e = MendeleevTable.Elems.Find(x => x.Name == str.Value);

            if (e == null)
                e = MendeleevTable.Compos.Find(x => x.Name == str.Value);

            if (d == null & e == null)
                throw new Exception("Отсутствует соединение или элемент" + str.Value + ".", new Exception("MyException"));

            str = str.Next;
            int r = -1;
            if (e != null)
            {
                List<Pair<Pair<string, string>, List<string>>> data = null;
                data = e.Properties;

                Pair<Pair<string, string>, List<string>> v = data.Find(x => x.First.Second == str.Value);

                if (v == null)
                    throw new Exception("Отсутствует свойство " + str.Value + " у данного соединения или элемента.", new Exception("MyException"));

                str = str.Next;

                if (str == null)
                    if (f)
                        return double.Parse(v.Second[v.Second.Count > u ? u : 0]);
                    else
                        return double.Parse(v.Second[0]);

                for (int i = 0; i < v.Second.Count; i++)
                    if (v.Second[i] == str.Value)
                        r = i;

                if (r == -1)
                    throw new Exception("Отсутствует значение параметра " + str.Previous.Value + " равного " + str.Value + " у данного соединения или элемента.", new Exception("MyException"));

                v = data.Find(x => x.First.Second == strs.Last.Value);

                if (v == null)
                    throw new Exception("Отсутствует параметр " + strs.Last.Value + " у данного соединения или элемента.", new Exception("MyException"));

                return double.Parse(v.Second[r]);
            }
            int c = -1;

            for (int i = 0; i < d.Columns.Count; i++)
                if (d.Columns[i].Caption == str.Value)
                    c = i;

            if (c == -1)
                throw new Exception("Отсутствует свойство " + str.Value + " у данной системы соединений.", new Exception("MyException"));

            str = str.Next;

            if (str == null)
                return double.Parse(d.Rows[u][c].ToString());

            for (int i = 0; i < d.Rows.Count; i++)
                if (d.Rows[i][c].ToString().Replace(',', '.') == str.Value)
                    r = i;

            if (r == -1)
                throw new Exception("Отсутствует значение параметра " + str.Previous.Value + " равного " + str.Value + " у данной системы соединений.", new Exception("MyException"));

            str = str.Next;
            c = -1;
            for (int i = 0; i < d.Columns.Count; i++)
                if (d.Columns[i].Caption == str.Value)
                    c = i;

            if (c == -1)
                throw new Exception("Отсутствует параметр " + strs.Last.Value + " у данной системы соединений.", new Exception("MyException"));

            return double.Parse(d.Rows[r][c].ToString());
        }
    }
}
