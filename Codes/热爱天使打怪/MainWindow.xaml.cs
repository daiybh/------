using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 热爱天使打怪
{
    public class WindowFinder
    {
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll",EntryPoint ="FindWindow")]
       public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public static IntPtr FindWindowA(string caption)
        {
            return FindWindow(null, caption);
        }
    }
    public class WindowCaptureHelper
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           
        }
        IntPtr gameWidnow = IntPtr.Zero;
        void findGameWindow()
        {
            gameWidnow = WindowFinder.FindWindowA("重生");
            if (gameWidnow == IntPtr.Zero) this.Title = "can not found";
            else
                this.Title = $"{gameWidnow} --fond";

        }
        Bitmap bitmap1, bitmap2;
        private BitmapImage GetWindowShotCut(IntPtr intPtr)
        {
            var image = WindowCaptureHelper.GetShotCutImage(intPtr);
            var bitmapImage = BitmapConveters.ConvertToBitmapImage(image);
            return bitmapImage;
        }
        Int32 curPos = 0;
        void takePhoto()
        {
            var image = WindowCaptureHelper.GetShotCutImage(gameWidnow);
            var bitmapImage = BitmapConveters.ConvertToBitmapImage(image);
            if (curPos++ % 2 == 0)
            {
                myImage.Source = bitmapImage;
                bitmap1 = image;
            }
            else
            {
                myImage2.Source = bitmapImage;
                bitmap2 = image;
            }
        }
        public Bitmap changeColor(Bitmap imgObj , List<System.Drawing.Rectangle> vlist)
        {
            System.Drawing.Color color2 = System.Drawing.Color.Red;
            foreach (var v in vlist)
            {
                //   x += $"{v.X} {v.Y} {v.Width} {v.Height}\n";
                for(int i=v.X;i<v.Right;i++)
                {
                    for(int h=v.Y;h<v.Bottom;h++)
                        imgObj.SetPixel(i,h, color2);
                }
            }
           
            return imgObj;
        }
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")] private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public enum WMessages : int
        {
            WM_LBUTTONDOWN = 0x201, //Left mousebutton down
            WM_LBUTTONUP = 0x202,   //Left mousebutton up
            WM_LBUTTONDBLCLK = 0x203, //Left mousebutton doubleclick
            WM_RBUTTONDOWN = 0x204, //Right mousebutton down
            WM_RBUTTONUP = 0x205,   //Right mousebutton up
            WM_RBUTTONDBLCLK = 0x206, //Right mousebutton do
        }
        private int MAKELPARAM(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }
        void compareTwoPic()
        {
            if (curPos < 2) return;
            var aw1= myImage.Source.Width;
            var ah1 =myImage.Source.Height;
            var aw2 = myImage2.Source.Width;
            var ah2 = myImage2.Source.Height;

            this.Title = $"{aw1} {ah1} {aw2} {ah2}";
            ComparePicture cp = new ComparePicture();

            var vList = cp.compareTwo(bitmap1, bitmap2);
            
            myImage2.Source = BitmapConveters.ConvertToBitmapImage(changeColor(bitmap2,vList));
            this.Title = vList.Count().ToString();
            for (int i = 0; i < 100; i++)
            {
                foreach (var v in vList)
                {

                    PostMessage(gameWidnow, (int)WMessages.WM_RBUTTONDOWN, 0, MAKELPARAM(v.X, v.Y));

                    PostMessage(gameWidnow, (int)WMessages.WM_RBUTTONUP, 0, MAKELPARAM(v.X, v.Y));

                    System.Threading.Thread.Sleep(100);
                    //mouse_event(0x0002, 0, 0, 0, 0); //模拟鼠标按下操作
                    //mouse_event(0x0004, 0, 0, 0, 0); //模拟鼠标放开操作
                }
                this.Title = i.ToString();
            }
        }
        private void onFindWindowBTN(object sender, RoutedEventArgs e)
        {           
            findGameWindow();
            if (gameWidnow == IntPtr.Zero) return;
            takePhoto();
            compareTwoPic();
            //this.Title = new WindowInteropHelper(this).Handle.ToString();
            //IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            //WindowFinder.SetParent(fwind, windowHandle);
        }
    }
}
