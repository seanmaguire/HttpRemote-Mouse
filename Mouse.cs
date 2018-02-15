using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;

namespace HttpRemote
{
    public static class Mouse
    {
        #region System constants
        const uint MOUSEEVENTF_MOVE = 0x0001; /* mouse move */
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        const uint MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */
        const uint MOUSEEVENTF_RIGHTUP = 0x0010; /* right button up */
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020; /* middle button down */
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040; /* middle button up */
        const uint MOUSEEVENTF_XDOWN = 0x0080; /* x button down */
        const uint MOUSEEVENTF_XUP = 0x0100; /* x button down */
        const uint MOUSEEVENTF_WHEEL = 0x0800; /* wheel button rolled */
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000; /* absolute move */
        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;
        #endregion

        #region Base routines
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        static extern int GetSystemMetrics(int index);

        private static void DoEvent(uint eventType, int globalX, int globalY)
        {
            globalX = globalX > 0 ? globalX : 0;
            globalY = globalY > 0 ? globalY : 0;
            uint x = Convert.ToUInt32((float)globalX * 65536 / GetSystemMetrics(SM_CXSCREEN));
            uint y = Convert.ToUInt32((float)globalY * 65536 / GetSystemMetrics(SM_CYSCREEN));
            mouse_event(MOUSEEVENTF_ABSOLUTE | eventType, x, y, 0, 0);
            System.Threading.Thread.Sleep(Delay);
            Application.DoEvents();
        }
        #endregion

        public static int Delay = 0;

        /// <summary>
        /// Press down left mouse button
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        public static void LeftDown(int globalX, int globalY)
        {
            DoEvent(MOUSEEVENTF_MOVE, globalX, globalY);
            DoEvent(MOUSEEVENTF_LEFTDOWN, globalX, globalY);
        }

        /// <summary>
        /// Release left mouse button
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        public static void LeftUp(int globalX, int globalY)
        {
            DoEvent(MOUSEEVENTF_MOVE, globalX, globalY);
            DoEvent(MOUSEEVENTF_LEFTUP, globalX, globalY);
        }

        /// <summary>
        /// Press down right mouse button
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        public static void RightDown(int globalX, int globalY)
        {
            DoEvent(MOUSEEVENTF_MOVE, globalX, globalY);
            DoEvent(MOUSEEVENTF_RIGHTDOWN, globalX, globalY);
        }

        /// <summary>
        /// Release right mouse button
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        public static void RightUp(int globalX, int globalY)
        {
            DoEvent(MOUSEEVENTF_MOVE, globalX, globalY);
            DoEvent(MOUSEEVENTF_RIGHTUP, globalX, globalY);
        }

        /// <summary>
        /// Move Mouse with emulated speed
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        /// <param name="speed">Speed in pixels per move</param>
        public static void MoveTo(int globalX, int globalY, int speed)
        {
            if (speed != 0)
            {
                int cursorX = Cursor.Position.X;
                int cursorY = Cursor.Position.Y;
                int dX = globalX - cursorX;
                int dY = globalY - cursorY;
                int steps = Math.Max(Math.Abs(dX / speed), Math.Abs(dY / speed));
                if (steps != 0)
                {
                    int stepX = dX / steps;
                    int stepY = dY / steps;
                    for (int i = 0; i < steps; i++)
                    {
                        cursorX += stepX;
                        cursorY += stepY;
                        DoEvent(MOUSEEVENTF_MOVE, cursorX, cursorY);
                    }
                }
            }
            DoEvent(MOUSEEVENTF_MOVE, globalX, globalY);
        }

        /// <summary>
        /// Move mouse
        /// </summary>
        /// <param name="globalX">Global X coordinate</param>
        /// <param name="globalY">Global Y coordinate</param>
        public static void MoveTo(int globalX, int globalY)
        {
            MoveTo(globalX, globalY, 0);
        }

        public static void DragAndDrop(int startX, int startY, int finishX, int finishY)
        {
            LeftDown(startX, startY);
            MoveTo(finishX, finishY, 10);
            LeftUp(finishX, finishY);
        }
    }
}
