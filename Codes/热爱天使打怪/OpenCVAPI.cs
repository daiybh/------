using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 热爱天使打怪
{
    internal class OpenCVAPI
    {
        public Bitmap openBitmap(Bitmap bitmap1,Bitmap bitmap2)
        {
            var image1= OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap1);
            var image2 = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap2);
            var img3 = compareTwoPic(image1, image2);

            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img3);
        }

        public void openCvTest()
        {
            var img1 = Cv2.ImRead("c:\\logs\\a\\111.png");
            var img2 = Cv2.ImRead("c:\\logs\\a\\222.png");
           var img3= compareTwoPic(img1,img2);
            Cv2.ImShow("openCvTest ", img3);
              Cv2.WaitKey(0);
        }
        private Mat compareTwoPic(Mat img1,Mat img2) { 
            Mat img_gray1 = new Mat();
            Cv2.CvtColor(img1, img_gray1, ColorConversionCodes.BGR2GRAY);
            Mat img_gray2 = new Mat();
            Cv2.CvtColor(img2, img_gray2, ColorConversionCodes.BGR2GRAY);
            Mat frameDiff = new Mat();
            Cv2.Subtract(img_gray1, img_gray2, frameDiff, new Mat());
            //Cv2.ImShow("aa", frameDiff);
            Mat absframeDiff = new Mat();

            absframeDiff = Cv2.Abs(frameDiff);
            absframeDiff.ConvertTo(absframeDiff, MatType.CV_8UC1, 1, 0);
           // Cv2.ImShow("absframeDiff", absframeDiff);

            Mat Image_threshold = new Mat();
            Cv2.Threshold(absframeDiff, Image_threshold, 20, 255, ThresholdTypes.Binary);
           // Cv2.ImShow("Image_threshold20", Image_threshold);

            Cv2.Threshold(absframeDiff, Image_threshold, 30, 255, ThresholdTypes.Binary);
           // Cv2.ImShow("Image_threshold30", Image_threshold);

            Mat morphologyKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5), new OpenCvSharp.Point(-1, -1));
            // Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));

            Mat Image_morp = new Mat();
            Cv2.MorphologyEx(Image_threshold, Image_morp, MorphTypes.Close, morphologyKernel, new OpenCvSharp.Point(-1, -1), 2, BorderTypes.Replicate);
          //  Cv2.ImShow("Image_morp", Image_morp);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            //Cv2.FindContours(Image_morp, contours, hierarchy, RETR_EXTERNAL, CHAIN_APPROX_SIMPLE, Point(0, 0));
            Cv2.FindContours(Image_morp, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));
            Scalar color = new Scalar(0, 0, 255);
            Mat drawing = Mat.Zeros(Image_threshold.Size(), MatType.CV_8UC3);
            rects.Clear();
            for (int i = 0; i < contours.Length; i++)
            {
                OutputArray oa;
                List<Point2f> output = new List<Point2f>();
                Cv2.ApproxPolyDP(InputArray.Create(contours[i]), OutputArray.Create(output), 3, true);

                var boundRect = Cv2.BoundingRect(contours[i]);
                if (boundRect.Width < 50 || boundRect.Height < 50) continue;
                Cv2.Rectangle(img1, boundRect, color);
                rects.Add(boundRect);
            }
           
            return img1;
        }
        public List<Rect> getList() { return rects; }
        List<Rect> rects = new List<Rect>();
        public void  testOPencv2()
        {
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
            Mat drawing = Mat.Zeros(Image_threshold.Size(), MatType.CV_8UC3);
            foreach (var item in contours)
            {
                // Cv2.ContourArea(item);
                var boundRect = Cv2.BoundingRect(item);

                Scalar color = new Scalar(0, 0, 255);
                Cv2.Rectangle(drawing, boundRect, color);

            }
            Cv2.ImShow("aa", drawing);
            Cv2.WaitKey(0);
        }
    }
}
