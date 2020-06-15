using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Activision_Mendeleyev_table.HelperClasses
{
    /// <summary>
    /// Класс для привязки клавишь к действиям
    /// </summary>
    public class HotKey : IDisposable
    {
        /// <summary>
        /// Словарь id и горячих клавиш для обратного вызова
        /// </summary>
        private static Dictionary<int, HotKey> _dictHotKeyToCallBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Слово для генерации сообщений
        /// </summary>
        public const int WmHotKey = 0x0312;

        /// <summary>
        /// Флаг для управления методоми Dispose() и Dispose(bool)
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Клавиша
        /// </summary>
        public Key Key { get; private set; }

        /// <summary>
        /// Модификаторы
        /// </summary>
        public KeyModifier KeyModifiers { get; private set; }

        /// <summary>
        /// Метод, который вызывается по нажатию сочетания клавиш
        /// </summary>
        public Action<HotKey> Action { get; private set; }

        /// <summary>
        /// Уникальный идентификатор сочетания клавиш
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Создает объект типа HotKey и регистрирует переданное сочетание клавиш(по умолчанию)
        /// </summary>
        /// <param name="k">клавиша</param>
        /// <param name="keyModifiers">модификаторы</param>
        /// <param name="action">метод</param>
        /// <param name="register">Нужно ли регистрировать?</param>
        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            if (register)
                Register();
        }

        /// <summary>
        /// Регистрирует сочетание клавиш
        /// </summary>
        /// <returns>Зарегистрирован или нет?</returns>
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (uint)KeyModifiers, (uint)virtualKeyCode);

            if (_dictHotKeyToCallBackProc == null)
            {
                _dictHotKeyToCallBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            if (!_dictHotKeyToCallBackProc.ContainsKey(Id))
                _dictHotKeyToCallBackProc.Add(Id, this);

            Debug.Print(result.ToString() + ", " + Id + ", " + virtualKeyCode);
            return result;
        }

        /// <summary>
        /// Утилизирует управляемые ресурсы
        /// </summary>
        public void Unregister()
        {
            HotKey hotKey;
            if (_dictHotKeyToCallBackProc.TryGetValue(Id, out hotKey))          
                UnregisterHotKey(IntPtr.Zero, Id);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled && msg.message == WmHotKey)
            {
                HotKey hotKey;
                if (_dictHotKeyToCallBackProc.TryGetValue((int)msg.wParam, out hotKey))
                {
                    if (hotKey.Action != null)
                        hotKey.Action.Invoke(hotKey);
                    handled = true;
                }
            }
        }

        /// <summary>
        /// Метод IDisposable, вызывает наш virtual Dispose(bool)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Утилизирует ресурсы
        /// </summary>
        /// <param name="disposing">Можно ли утилизировать управляемые ресурсы?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                    Unregister();

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Модификаторы
    /// </summary>
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
