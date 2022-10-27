using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace 热爱天使打怪
{
    public class WindowAPIHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref System.Drawing.Rectangle rect);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(IntPtr hdc);
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWndChild, int nIndex); 

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWndChild, int nIndex,int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")] private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public enum WMessages : int
        {
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x201, //Left mousebutton down
            WM_LBUTTONUP = 0x202,   //Left mousebutton up
            WM_LBUTTONDBLCLK = 0x203, //Left mousebutton doubleclick
            WM_RBUTTONDOWN = 0x204, //Right mousebutton down
            WM_RBUTTONUP = 0x205,   //Right mousebutton up
            WM_RBUTTONDBLCLK = 0x206, //Right mousebutton do
        }
        private static int MAKELPARAM(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }
        [DllImport("Shcore.dll")]
        private static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

        [DllImport("User32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);
        private enum PROCESS_DPI_AWARENESS
        {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }
        public static void OnClickRButton(IntPtr hWnd, int x, int y)
        {
            int curPos = MAKELPARAM(x, y);
           // bool b1 = PostMessage(hWnd, (int)WMessages.WM_MOUSEMOVE, 0, curPos);
            System.Threading.Thread.Sleep(10);
             PostMessage(hWnd, (int)WMessages.WM_RBUTTONDOWN, 0, curPos);
             PostMessage(hWnd, (int)WMessages.WM_RBUTTONUP, 0, curPos);
           
        }
        private const int MONITOR_DEFAULTTONULL = 0x00000000;
        private const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int MONITORINFOF_PRIMARY = 0x00000001;
        public static void TTTT()
        {
            System.Diagnostics.Process proc = Process.GetProcessById(21464);
            PROCESS_DPI_AWARENESS value;
            int res = GetProcessDpiAwareness(proc.Handle, out value);
            System.Diagnostics.Debug.Print(value.ToString());

            // Get the monitor that the window is currently displayed on
            // (where hWnd is a handle to the window of interest).
          //  IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);

            //// Get the logical width and height of the monitor.
            //MONITORINFOEX miex;
            //miex.cbSize = sizeof(miex);
            //GetMonitorInfo(hMonitor, &miex);
            //int cxLogical = (miex.rcMonitor.right - miex.rcMonitor.left);
            //int cyLogical = (miex.rcMonitor.bottom - miex.rcMonitor.top);

            //// Get the physical width and height of the monitor.
            //DEVMODE dm;
            //dm.dmSize = sizeof(dm);
            //dm.dmDriverExtra = 0;
            //EnumDisplaySettings(miex.szDevice, ENUM_CURRENT_SETTINGS, &dm);
            //int cxPhysical = dm.dmPelsWidth;
            //int cyPhysical = dm.dmPelsHeight;

            //// Calculate the scaling factor.
            //double horzScale = ((double)cxPhysical / (double)cxLogical);
            //double vertScale = ((double)cyPhysical / (double)cyLogical);
            //ASSERT(horzScale == vertScale);
        }

        public static void OnClickLButton(IntPtr hWnd, int x, int y)
        {
            int curPos = MAKELPARAM(x, y);
           // bool b1 = PostMessage(hWnd, (int)WMessages.WM_MOUSEMOVE, 0, curPos);
            System.Threading.Thread.Sleep(10);
            PostMessage(hWnd, (int)WMessages.WM_LBUTTONDOWN, 0, curPos);
            PostMessage(hWnd, (int)WMessages.WM_LBUTTONUP, 0, curPos);
        }
        public static Bitmap GetShotCutImage(IntPtr hWnd)
        {
            var hscrdc = GetWindowDC(hWnd);
            var windowRect = new System.Drawing.Rectangle();
            GetWindowRect(hWnd, ref windowRect);
            int width = Math.Abs(windowRect.Width - windowRect.X);
            int height = Math.Abs(windowRect.Height - windowRect.Y);
            var hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            var hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            var bmp = System.Drawing.Image.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);
            DeleteDC(hmemdc);
            return bmp;
        }
    }
}
