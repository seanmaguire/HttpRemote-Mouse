using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HttpRemote
{
    public static class WinAPI
    {
        // get windows
        [DllImport("User32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int index);
        [DllImport("User32.dll")]
        private static extern IntPtr SendDlgItemMessage(IntPtr hWnd, int IDDlgItem, int uMsg, int nMaxCount, StringBuilder lpString);
        [DllImport("User32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);


        // enum windows
        private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsProc x, int y);
        [DllImport("user32")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, int lParam);
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        private static List<IntPtr> _results = new List<IntPtr>();


        // get window text / size
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // mouse
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public struct POINT
        {
            public int X;
            public int Y;

            /*public static implicit operator POINT(POINT point)
            {
                return new POINT(point.X, point.Y);
            }*/
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private static string GetText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // get richedit text
        private const int GWL_ID = -12;
        private const int WM_GETTEXT = 0x000D;

        public static void clickThing(string name, int n, int v)
        {
            foreach (Process procesInfo in Process.GetProcesses())
            {
                if (!procesInfo.ProcessName.Equals("FL")) continue;

                //Console.WriteLine("process {0} {1}", procesInfo.ProcessName, procesInfo.Id);
                foreach (ProcessThread threadInfo in procesInfo.Threads)
                {
                    //Console.WriteLine("\tthread {0:x}", threadInfo.Id);
                    IntPtr[] windows = GetWindowHandlesForThread(threadInfo.Id);
                    if (windows != null && windows.Length > 0)
                    {
                        foreach (IntPtr hWnd in windows)
                        {
                            //Console.Write("\twindow {0} text:{1} caption:{2} rect: ",
                            //    hWnd.ToInt32(), GetText(hWnd), GetEditText(hWnd));

                            RECT rect;
                            GetWindowRect(hWnd, out rect);
                            //Console.WriteLine("(" + rect.Top + "," + rect.Left + "," + rect.Bottom + "," + rect.Right + ")");

                            if (GetText(hWnd).Contains(name))
                            {
                                if (rect.Left > 0)
                                {
                                    POINT lpPoint;
                                    GetCursorPos(out lpPoint);

                                    int dist = (n-1) * 166;

                                    // first pedal
                                    LeftMouseClick(rect.Left + 87 + dist, rect.Top + 351);

                                    SetCursorPos(lpPoint.X, lpPoint.Y);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            /*
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            System.Console.WriteLine("\n\n\n" + lpPoint.X + " " + lpPoint.Y + "\n\n\n");

            RECT rect;
            IntPtr hWnd = new IntPtr(133578);
            GetWindowRect(hWnd, out rect);
            System.Console.WriteLine("\n" + rect.Left + " " + rect.Top + "\n" + rect.Right + " " + rect.Bottom + "\n");
            */
        }

        private static IntPtr[] GetWindowHandlesForThread(int threadHandle)
        {
            _results.Clear();
            EnumWindows(WindowEnum, threadHandle);
            return _results.ToArray();
        }

        private static int WindowEnum(IntPtr hWnd, int lParam)
        {
            int processID = 0;
            int threadID = GetWindowThreadProcessId(hWnd, out processID);
            if (threadID == lParam)
            {
                _results.Add(hWnd);
                EnumChildWindows(hWnd, WindowEnum, threadID);
            }
            return 1;
        }

        private static StringBuilder GetEditText(IntPtr hWnd)
        {
            Int32 dwID = GetWindowLong(hWnd, GWL_ID);
            IntPtr hWndParent = GetParent(hWnd);
            StringBuilder title = new StringBuilder(128);
            SendDlgItemMessage(hWndParent, dwID, WM_GETTEXT, 128, title);
            return title;
        }
    }
}
