using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using Point = System.Drawing.Point;

namespace 热爱天使打怪
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
        Thread clickThread = null;
        bool m_Running = true;
        public MainWindow()
        {
            InitializeComponent();
            clickThread = new Thread(calltoClickThread);
           // clickThread.Start();
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Interval = new TimeSpan(0, 0, 1);
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
          //  onFindWindowBTN(sender, null);
        }

        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_Running = false;
            clickThread.Join();
        }

        IntPtr gameWidnow = IntPtr.Zero;
        void findGameWindow()
        {
            gameWidnow = WindowAPIHelper.FindWindow("SlOnline", "重生");
            if (gameWidnow == IntPtr.Zero) this.Title = "can not found";
            else
                this.Title = $"{gameWidnow} --fond";

            tbResult.Text = this.Title;
        }
        Bitmap bitmap1=null, bitmap2;

        Int32 curPos = 0;
       
        void takePhoto()
        {
            var image = WindowAPIHelper.GetShotCutImage(gameWidnow);
            image = comparePicture.grayPicture(image); ;
            var bitmapImage = BitmapConveters.ConvertToBitmapImage(image);
            if (curPos++ % 2 == 0)
            {
                myImage.Source = bitmapImage;
                image.Save("c:\\logs\\111.png");
                bitmap1 = image;
            }
            else
            {
                image.Save("c:\\logs\\222.png");
                myImage2.Source = bitmapImage;
                bitmap2 = image;
            }
        }
        public Bitmap changeColor(Bitmap imgObj, SortedDictionary<int, List<System.Drawing.Rectangle>> vlist)
        {
            System.Drawing.Color color2 = System.Drawing.Color.Red;
            string xText = "";
            Dictionary<int, List<int>> newList = new Dictionary<int, List<int>>();
            int ia = 0;
            List<int> xList = new List<int>();
            int lastX = -1;
            foreach (var mapItem in vlist)
            {
                xText += $"{ia}--{mapItem.Key}-> \n";
                if(lastX==-1)
                {
                    lastX = mapItem.Key;
                    xList.Add(mapItem.Key);
                    continue;
                }
                if (mapItem.Key - lastX > 20)
                {
                    if (xList.Count > 4)
                        newList[mapItem.Key] = xList;
                    xList.Clear();
                }
                xList.Add(mapItem.Key);
                lastX = mapItem.Key;
                ia++;
            }
            xText+= "aa"+ xList.Count().ToString();
            tbResult.Text = xText;
            ia = 0;
            xText = "";
            foreach (var mapItem in newList)
            {
                xText += $"{ia}--{mapItem.Key}-> \n";
                foreach(var item in mapItem.Value)
                {
                    xText += $"{item}  ";
                }
                xText += "\n";
            }
            tbResult2.Text = xText;
            //System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format1bppIndexed;//5+1+a+s+p+x
            //BitmapData bmpData = imgObj.LockBits(new System.Drawing.Rectangle(0, 0, imgObj.Width, imgObj.Height), ImageLockMode.ReadWrite, pf);
            //unsafe
            //{
            //    byte* ptr = (byte*)(bmpData.Scan0);
            //    byte temp = 0;
            //    for (int i = 0; i < bmpData.Height; i++)
            //    {
            //        for (int j = 0; j < bmpData.Width; j++)
            //        {
            //            temp = (byte)(0.229 * ptr[2] + 0.587 * ptr[1] + 0.144 * ptr[0]);
            //            temp = (byte)(temp * 10);
            //            ptr[0] = ptr[1] = ptr[2] = temp;
            //            ptr += 3;
            //        }
            //        ptr += bmpData.Stride - bmpData.Width * 3;
            //    }
            //}
            //imgObj.UnlockBits(bmpData);
            return imgObj;
            foreach (var mapItem in vlist)
            {
                foreach (var v in mapItem.Value)
                {
                    xText += $"{mapItem.Key}-> {v.X} {v.Y} {v.Width} {v.Height}\n";
                    for (int i = v.X; i < v.Right; i++)
                    {
                        if (i < imgObj.Width && v.Y < imgObj.Height)
                            imgObj.SetPixel(i, v.Y, color2);
                        if (i < imgObj.Width && v.Bottom < imgObj.Height)
                            imgObj.SetPixel(i, v.Bottom, color2);
                        
                    }
                    for (int h = v.Y; h < v.Bottom; h++)
                    {
                        if (v.X < imgObj.Width && h < imgObj.Height)
                            imgObj.SetPixel(v.X, h, color2);
                        if (v.Right < imgObj.Width && h < imgObj.Height)
                            imgObj.SetPixel(v.Right, h, color2);
                    }
                }
            }
           
            return imgObj;
        }
        ComparePicture comparePicture = new ComparePicture();
        SortedDictionary<int, List<System.Drawing.Rectangle>> rectangles = null;
        void compareTwoPic()
        {
            if (curPos < 2) return;
            var aw1 = myImage.Source.Width;
            var ah1 = myImage.Source.Height;
            var aw2 = myImage2.Source.Width;
            var ah2 = myImage2.Source.Height;



            rectangles = comparePicture.compareTwo(bitmap1, bitmap2);
            myImage2.Source = BitmapConveters.ConvertToBitmapImage(changeColor(bitmap2, rectangles));
        }
        void findValidRect(List<System.Drawing.Rectangle> _rectangles)
        {

        }
        
        void calltoClickThread()
        {
            int offset = 100;
            int nCount = 0;
            while (m_Running )
            {
                if (bitmap1 != null)
                {
                    int x = bitmap1.Width / 2;
                    int y = bitmap1.Height / 2;
                    for(int i=x- offset; m_Running && i <x+ offset; i+=5)
                    {
                        for (int j = y - offset; m_Running&& j < y + offset; j += 5)
                        {
                            WindowAPIHelper.OnClickRButton(gameWidnow, i, j);
                        }
                    }
                }
                this.tbResult.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.tbResult.Text = $"{nCount}";
                }));
                System.Threading.Thread.Sleep(1000);
                nCount++;
            }
        }
        private IntPtr hWndOriginalParent;

        private void onRBTN(object sender, RoutedEventArgs e)
        {
            WindowAPIHelper.TTTT();
        }

        private void onsetParent(object sender, RoutedEventArgs e)
        {
            if(gameWidnow==null)
                gameWidnow = WindowAPIHelper.FindWindow("notepad", null);
            this.Title = $"{gameWidnow}";
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            hWndOriginalParent  = WindowAPIHelper.SetParent(gameWidnow, windowHandle);

        }

        private void onUnsetParent(object sender, RoutedEventArgs e)
        {
            if(gameWidnow!=null)
            WindowAPIHelper.SetParent(gameWidnow, hWndOriginalParent);
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
