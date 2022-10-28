# 导入必要的包
from skimage.metrics  import structural_similarity
import argparse
import imutils
import cv2
import numpy as np



imageA = cv2.imread("c:\\logs\\a\\111.png")
imageB = cv2.imread("c:\\logs\\a\\222.png")

cv2.imshow("img",imageA)
grayA = cv2.cvtColor(imageA,cv2.COLOR_BGR2GRAY)

grayB = cv2.cvtColor(imageB,cv2.COLOR_BGR2GRAY)

(score, diff) = structural_similarity(grayA, grayB, full=True)
diff = (diff*255).astype("uint8")
print("SSIM:{}".format(score))

#　阈值分割差分图像，然后查找轮廓以获得两个输入图片的不同区域
thresh = cv2.threshold(diff, 0, 255,
                      cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
cnts = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, 
                       cv2.CHAIN_APPROX_SIMPLE)
cnts = imutils.grab_contours(cnts)
# 遍历轮廓
for c in cnts:
  # 计算轮廓的边界框，然后在两张输入图片中代表图片不同点的区域绘制边界框
  (x, y, w, h) = cv2.boundingRect(c)
  cv2.rectangle(imageA, (x, y), (x + w, y + h), (0, 0, 255), 2)
  cv2.rectangle(imageB, (x, y), (x + w, y + h), (0, 0, 255), 2)
 
 
# 显示输出图片
cv2.imshow("Original", imageA)
cv2.imshow("Modified", imageB)
cv2.imshow("Diff", diff)
cv2.imshow("Thresh", thresh)
cv2.waitKey(0)