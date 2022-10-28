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
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Color = System.Drawing.Color;

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
        public SortedDictionary<int, List<Rectangle>> compareTwo(Bitmap img1, Bitmap img2)
        {
            return compareTwo(img1, img2, new Size(20, 20));
        }
        #region 灰度处理
        /// <summary>
        /// 将源图像灰度化，并转化为8位灰度图像。
        /// </summary>
        /// <param name="original"> 源图像。 </param>
        /// <returns> 8位灰度图像。 </returns>
        public static Bitmap RgbToGrayScale(Bitmap original)
        {
            if (original != null)
            {
                // 将源图像内存区域锁定
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);

                // 获取图像参数
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度,比实际图片要大
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置的指针
                int scanBytesLength = stride * height;  // 用stride宽度，表示这是内存区域的大小

                // 分别设置两个位置指针，指向源数组和目标数组
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytesLength];  // 为目标数组分配内存
                Marshal.Copy(ptr, rgbValues, 0, scanBytesLength);  // 将图像数据拷贝到rgbValues中
                // 分配灰度数组
                byte[] grayValues = new byte[width * height]; // 不含未用空间。
                // 计算灰度数组

                byte blue, green, red, YUI;



                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {

                        blue = rgbValues[posScan];
                        green = rgbValues[posScan + 1];
                        red = rgbValues[posScan + 2];
                        YUI = (byte)(0.229 * red + 0.587 * green + 0.144 * blue);
                        //grayValues[posDst] = (byte)((blue + green + red) / 3);
                        grayValues[posDst] = YUI;
                        posScan += 3;
                        posDst++;

                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                    posScan += offset;
                }

                // 内存解锁
                Marshal.Copy(rgbValues, 0, ptr, scanBytesLength);
                original.UnlockBits(bmpData);  // 解锁内存区域

                // 构建8位灰度位图
                Bitmap retBitmap = BuiltGrayBitmap(grayValues, width, height);
                return retBitmap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 用灰度数组新建一个8位灰度图像。
        /// </summary>
        /// <param name="rawValues"> 灰度数组(length = width * height)。 </param>
        /// <param name="width"> 图像宽度。 </param>
        /// <param name="height"> 图像高度。 </param>
        /// <returns> 新建的8位灰度位图。 </returns>
        private static Bitmap BuiltGrayBitmap(byte[] rawValues, int width, int height)
        {
            // 新建一个8位灰度位图，并锁定内存区域操作
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // 计算图像参数
            int offset = bmpData.Stride - bmpData.Width;        // 计算每行未用空间字节数
            IntPtr ptr = bmpData.Scan0;                         // 获取首地址
            int scanBytes = bmpData.Stride * bmpData.Height;    // 图像字节数 = 扫描字节数 * 高度
            byte[] grayValues = new byte[scanBytes];            // 为图像数据分配内存

            // 为图像数据赋值
            int posSrc = 0, posScan = 0;                        // rawValues和grayValues的索引
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grayValues[posScan++] = rawValues[posSrc++];
                }
                // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                posScan += offset;
            }

            // 内存解锁
            Marshal.Copy(grayValues, 0, ptr, scanBytes);
            bitmap.UnlockBits(bmpData);  // 解锁内存区域

            // 修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette palette;
            // 获取一个Format8bppIndexed格式图像的Palette对象
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 修改生成位图的索引表
            bitmap.Palette = palette;

            return bitmap;
        }
        #endregion
        #region 二值化
        /*
        1位深度图像 颜色表数组255个元素 只有用前两个 0对应0  1对应255 
        1位深度图像每个像素占一位
        8位深度图像每个像素占一个字节  是1位的8倍
        */
        /// <summary>
        /// 将源灰度图像二值化，并转化为1位二值图像。
        /// </summary>
        /// <param name="bmp"> 源灰度图像。 </param>
        /// <returns> 1位二值图像。 </returns>
        public static Bitmap GTo2Bit(Bitmap bmp)
        {
            if (bmp != null)
            {
                // 将源图像内存区域锁定
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly,
                        PixelFormat.Format8bppIndexed);

                // 获取图像参数
                int leng, offset_1bit = 0;
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度,比实际图片要大
                int offset = stride - width;  // 显示宽度与扫描线宽度的间隙
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置的指针
                int scanBytesLength = stride * height;  // 用stride宽度，表示这是内存区域的大小
                if (width % 32 == 0)
                {
                    leng = width / 8;
                }
                else
                {
                    leng = width / 8 + (4 - (width / 8 % 4));
                    if (width % 8 != 0)
                    {
                        offset_1bit = leng - width / 8;
                    }
                    else
                    {
                        offset_1bit = leng - width / 8;
                    }
                }

                // 分别设置两个位置指针，指向源数组和目标数组
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytesLength];  // 为目标数组分配内存
                Marshal.Copy(ptr, rgbValues, 0, scanBytesLength);  // 将图像数据拷贝到rgbValues中
                // 分配二值数组
                byte[] grayValues = new byte[leng * height]; // 不含未用空间。
                // 计算二值数组
                int x, v, t = 0;
                for (int i = 0; i < height; i++)
                {
                    for (x = 0; x < width; x++)
                    {
                        v = rgbValues[posScan];
                        t = (t << 1) | (v > 100 ? 1 : 0);


                        if (x % 8 == 7)
                        {
                            grayValues[posDst] = (byte)t;
                            posDst++;
                            t = 0;
                        }
                        posScan++;
                    }

                    if ((x %= 8) != 7)
                    {
                        t <<= 8 - x;
                        grayValues[posDst] = (byte)t;
                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                    posScan += offset;
                    posDst += offset_1bit;
                }

                // 内存解锁
                Marshal.Copy(rgbValues, 0, ptr, scanBytesLength);
                bmp.UnlockBits(bmpData);  // 解锁内存区域

                // 构建1位二值位图
                Bitmap retBitmap = twoBit(grayValues, width, height);
                return retBitmap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 用二值数组新建一个1位二值图像。
        /// </summary>
        /// <param name="rawValues"> 二值数组(length = width * height)。 </param>
        /// <param name="width"> 图像宽度。 </param>
        /// <param name="height"> 图像高度。 </param>
        /// <returns> 新建的1位二值位图。 </returns>
        private static Bitmap twoBit(byte[] rawValues, int width, int height)
        {
            // 新建一个1位二值位图，并锁定内存区域操作
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            // 计算图像参数
            int offset = bmpData.Stride - bmpData.Width / 8;        // 计算每行未用空间字节数
            IntPtr ptr = bmpData.Scan0;                         // 获取首地址
            int scanBytes = bmpData.Stride * bmpData.Height;    // 图像字节数 = 扫描字节数 * 高度
            byte[] grayValues = new byte[scanBytes];            // 为图像数据分配内存

            // 为图像数据赋值
            int posScan = 0;                        // rawValues和grayValues的索引
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < bmpData.Width / 8; j++)
                {
                    grayValues[posScan] = rawValues[posScan];
                    posScan++;
                }
                // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                posScan += offset;
            }

            // 内存解锁
            Marshal.Copy(grayValues, 0, ptr, scanBytes);
            bitmap.UnlockBits(bmpData);  // 解锁内存区域

            // 修改生成位图的索引表
            ColorPalette palette;
            // 获取一个Format8bppIndexed格式图像的Palette对象
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 2; i = +254)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 修改生成位图的索引表
            bitmap.Palette = palette;

            return bitmap;
        }
        #endregion

        public Bitmap grayPicture(Bitmap source)
        {
            return source;
            Bitmap bitmapGray8bits= RgbToGrayScale(source); ;
            Bitmap bitmap2bit= GTo2Bit(bitmapGray8bits);
            return bitmap2bit;
            System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;//5+1+a+s+p+x
            BitmapData bmpData = source.LockBits(new System.Drawing.Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, pf);
            unsafe
            {
                byte* ptr = (byte*)(bmpData.Scan0);
                byte temp = 0;
                for (int i=0;i< bmpData.Height;i++)
                {
                    for(int j = 0; j < bmpData.Width; j++)
                    {
                        temp = (byte)(0.229 * ptr[2] + 0.587 * ptr[1] + 0.144 * ptr[0]);
                        temp = (byte)(temp * 10);
                        ptr[0] = ptr[1] = ptr[2] = temp;
                        ptr += 3;
                    }
                    ptr += bmpData.Stride - bmpData.Width * 3;
                }
            }
            source.UnlockBits(bmpData);
            return source;
        }

        public SortedDictionary<int, List<Rectangle>> compareTwo(Bitmap img1, Bitmap img2, Size block)
        {
            SortedDictionary<int, List<Rectangle>> rects = new SortedDictionary<int, List<Rectangle>>();

            System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;//5+1+a+s+p+x
            BitmapData data1 = img1.LockBits(new System.Drawing.Rectangle(0, 0, img1.Width, img1.Height), ImageLockMode.ReadWrite, pf);
            BitmapData data2 = img2.LockBits(new System.Drawing.Rectangle(0, 0, img2.Width, img2.Height), ImageLockMode.ReadWrite, pf);
            unsafe
            {
                int h = 0;
                while (h < img1.Height - 180)
                {
                    byte* p1 = (byte*)data1.Scan0 + h * data1.Stride;
                    byte* p2 = (byte*)data2.Scan0 + h * data2.Stride;
                    int w = 0;
                    while (w < img1.Width)
                    {
                        for (int i = 0; i < block.Width; i++)
                        {
                            int wi = w + i;


                            if (wi >= data1.Width) break;
                            for (int j = 0; j < block.Height; j++)
                            {
                                int hj = h + j;
                                if (hj >= data1.Height) break;

                                if (hj < 200 && wi < 200) continue;
                                ICColor* pc1 = (ICColor*)(p1 + wi * 3 + data1.Stride * j);
                                ICColor* pc2 = (ICColor*)(p2 + wi * 3 + data1.Stride * j);

                                if (pc1->R != pc2->R || pc1->G != pc2->G || pc1->B != pc2->B)
                                {
                                    //当前块有某个象素点颜色值不相同.也就是有差异.

                                    int bw = Math.Min(block.Width, data1.Width - w);
                                    int bh = Math.Min(block.Height, data1.Height - h);

                                    if (!rects.ContainsKey(w))
                                        rects.Add(w, new List<Rectangle>());

                                    rects[w].Add(new Rectangle(w, h, bw, bh));
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

        public int[] GetHisogram(Bitmap img)
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
