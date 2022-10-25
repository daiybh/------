using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Collections;

namespace 热爱天使打怪
{
    class ComparePicture
    {
        private float GetAbs(int firstNum, int secondNum)
        {

            float abs = Math.Abs((float)firstNum - (float)secondNum);

            float result = Math.Max(firstNum, secondNum);

            if (result == 0)

                result = 1;

            return abs / result;

        }
        public float GetResult(int[] firstNum, int[] scondNum)
        {

            if (firstNum.Length != scondNum.Length)
            {

                return 0;

            }

            else
            {

                float result = 0;

                int j = firstNum.Length;

                for (int i = 0; i < j; i++)
                {

                    result += 1 - GetAbs(firstNum[i], scondNum[i]);

                    Console.WriteLine(i + "----" + result);

                }

                return result / j;

            }

        }
        [StructLayout(LayoutKind.Explicit)]
        private struct ICColor
        {
            [FieldOffset(0)]
            public byte B;
            [FieldOffset(1)]
            public byte G;
            [FieldOffset(2)]
            public byte R;
        }
        public List<Rectangle> compareTwo(Bitmap img1, Bitmap img2)
        {
            return compareTwo(img1, img2, new Size(20, 20));
        }
        public List<Rectangle> compareTwo(Bitmap img1 ,Bitmap img2, Size block)
        {
            List<Rectangle> rects = new List<Rectangle>();
            
            System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;//5+1+a+s+p+x
            BitmapData data1 = img1.LockBits(new System.Drawing.Rectangle(0, 0, img1.Width, img1.Height), ImageLockMode.ReadWrite, pf);
            BitmapData data2 = img2.LockBits(new System.Drawing.Rectangle(0, 0, img2.Width, img2.Height), ImageLockMode.ReadWrite, pf);
            unsafe
            {               
                int h = 0;
                while(h<img1.Height-180)
                {
                    byte* p1 = (byte*)data1.Scan0 + h * data1.Stride;
                    byte* p2 = (byte*)data2.Scan0 + h * data2.Stride;
                    int w = 0;
                    while(w<img1.Width)
                    {
                        for(int i=0;i<block.Width;i++)
                        {
                            int wi = w + i;
                            if (wi >= data1.Width) break;
                            for(int j=0;j<block.Height;j++)
                            {
                                int hj = h + j;
                                if (hj >= data1.Height) break;

                                ICColor* pc1 = (ICColor*)(p1 + wi * 3 + data1.Stride * j);
                                ICColor* pc2 = (ICColor*)(p2 + wi * 3 + data1.Stride * j);

                                if (pc1->R != pc2->R || pc1->G != pc2->G || pc1->B != pc2->B)
                                {
                                    //当前块有某个象素点颜色值不相同.也就是有差异.

                                    int bw = Math.Min(block.Width, data1.Width - w);
                                    int bh = Math.Min(block.Height, data1.Height - h);
                                    rects.Add(new Rectangle(w, h, bw, bh));
                                    goto E;
                                }
                            }
                        }
E:
                        w += block.Width;
                    }
                    h += block.Height;
                }

            }
            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            return rects;
        }

        public  int[] GetHisogram(Bitmap img)
        {

            BitmapData data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int[] histogram = new int[256];

            unsafe
            {

                byte* ptr = (byte*)data.Scan0;

                int remain = data.Stride - data.Width * 3;

                for (int i = 0; i < histogram.Length; i++)

                    histogram[i] = 0;

                for (int i = 0; i < data.Height; i++)
                {

                    for (int j = 0; j < data.Width; j++)
                    {

                        int mean = ptr[0] + ptr[1] + ptr[2];

                        mean /= 3;

                        histogram[mean]++;

                        ptr += 3;

                    }

                    ptr += remain;

                }

            }

            img.UnlockBits(data);

            return histogram;

        }
    }
}
