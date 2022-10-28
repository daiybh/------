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
using OpenCvSharp;
using OpenCvSharp.Internal.Vectors;

namespace 热爱天使打怪
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
        Thread clickThread = null;
        bool m_Running = true;
        void openCvTest()
        {
            var img1 = Cv2.ImRead("c:\\logs\\a\\111.png");
            var img2 = Cv2.ImRead("c:\\logs\\a\\222.png");
            Mat img_gray1 = new Mat();
            Cv2.CvtColor(img1, img_gray1, ColorConversionCodes.BGR2GRAY);
            Mat img_gray2 = new Mat();
            Cv2.CvtColor(img2, img_gray2, ColorConversionCodes.BGR2GRAY);
            Mat frameDiff = new Mat();
            Cv2.Subtract(img_gray1, img_gray2, frameDiff, new Mat());
            Cv2.ImShow("aa", frameDiff);
            Mat absframeDiff = new Mat();

            absframeDiff = Cv2.Abs(frameDiff);
            absframeDiff.ConvertTo(absframeDiff, MatType.CV_8UC1, 1, 0);
            Cv2.ImShow("absframeDiff", absframeDiff);

            Mat Image_threshold = new Mat();
            Cv2.Threshold(absframeDiff, Image_threshold, 20, 255, ThresholdTypes.Binary);
            Cv2.ImShow("Image_threshold20", Image_threshold);

            Cv2.Threshold(absframeDiff, Image_threshold, 30, 255, ThresholdTypes.Binary);
            Cv2.ImShow("Image_threshold30", Image_threshold);

            Mat morphologyKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5), new OpenCvSharp.Point(-1, -1));
            // Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));

            Mat Image_morp = new Mat();
            Cv2.MorphologyEx(Image_threshold, Image_morp, MorphTypes.Close, morphologyKernel,new OpenCvSharp.Point(-1,-1),2,BorderTypes.Replicate);
            Cv2.ImShow("Image_morp", Image_morp);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            //Cv2.FindContours(Image_morp, contours, hierarchy, RETR_EXTERNAL, CHAIN_APPROX_SIMPLE, Point(0, 0));
            Cv2.FindContours(Image_morp, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));
            Scalar color = new Scalar(0, 0, 255);
            Mat drawing = Mat.Zeros(Image_threshold.Size(), MatType.CV_8UC3);
            for (int i = 0; i < contours.Length; i++)
            {
                OutputArray oa;
                List<Point2f> output = new List<Point2f>();
                Cv2.ApproxPolyDP(InputArray.Create( contours[i]), OutputArray.Create(output), 3, true);

                var boundRect = Cv2.BoundingRect(contours[i]);
                Cv2.Rectangle(drawing, boundRect, color);
            }

            Cv2.ImShow("Image_morp222", drawing);
            Cv2.WaitKey(0);
        }
        public MainWindow()
        {
            openCvTest();
            InitializeComponent();
            clickThread = new Thread(calltoClickThread);
           // clickThread.Start();
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Interval = new TimeSpan(0, 0, 1);
           // updateTimer.Start();

            var img1 = Cv2.ImRead("c:\\logs\\a\\111.png");
            var img2 = Cv2.ImRead("c:\\logs\\a\\222.png");
            Mat img_gray1 = new Mat();
            Cv2.CvtColor(img1, img_gray1, ColorConversionCodes.BGR2GRAY);
            Mat img_gray2 = new Mat();
            Cv2.CvtColor(img2, img_gray2, ColorConversionCodes.BGR2GRAY);

            Mat img_gauss1 = new Mat();
            Cv2.GaussianBlur(img_gray1, img_gauss1, new OpenCvSharp.Size(11, 11), 4, 4);

            Mat img_gauss2 = new Mat();
            Cv2.GaussianBlur(img_gray2, img_gauss2, new OpenCvSharp.Size(11, 11), 4, 4);

            Mat Image_diff = new Mat();
            //帧差
            Cv2.Absdiff(img_gauss1, img_gauss2, Image_diff);
            /*cout << "帧差" << endl;*/
            Cv2.ImShow("Image_diff", Image_diff);
            //二值化CV_THRESH_BINARY

            Mat Image_threshold = new Mat();
            Cv2.Threshold(Image_diff, Image_threshold, 0, 255, ThresholdTypes.Binary);
            Cv2.ImShow("Image_threshold", Image_threshold);

            Mat Image_morp = new Mat();
            //自定义核,进行开、闭运算
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(Image_threshold, Image_morp, MorphTypes.Open, element);
            Cv2.MorphologyEx(Image_morp, Image_morp, MorphTypes.Close, element);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            //Cv2.FindContours(Image_morp, contours, hierarchy, RETR_EXTERNAL, CHAIN_APPROX_SIMPLE, Point(0, 0));
            Cv2.FindContours(Image_morp, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            //画出矩阵和圆形
            Mat drawing = Mat.Zeros(Image_threshold.Size(),MatType.CV_8UC3);
            foreach(var item in contours)
            {
                // Cv2.ContourArea(item);
                var boundRect = Cv2.BoundingRect(item);

                Scalar color = new Scalar(0,0,255);
                Cv2.Rectangle(drawing, boundRect, color);

            }
            Cv2.ImShow("aa", drawing);
            Cv2.WaitKey(0);
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
