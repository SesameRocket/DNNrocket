﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Simplisity;

namespace DNNrocketAPI
{
    public class ImgUtils
    {

        public static void CopyImageForSEO(string sourceFilePath, string destinationfolder, string fileName = "")
        {
            var imageDirectorySEO = destinationfolder;
            if (!Directory.Exists(imageDirectorySEO))
            {
                Directory.CreateDirectory(imageDirectorySEO);
            }

            if (File.Exists(sourceFilePath) && !File.Exists(imageDirectorySEO + "\\XL_" + fileName))
            {
                if (fileName == "") fileName = Path.GetFileName(sourceFilePath);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\XL_" + fileName, 1024);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\L_" + fileName, 800);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\M_" + fileName, 460);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\S_" + fileName, 240);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\T_" + fileName, 160);
                ResizeImage(sourceFilePath, imageDirectorySEO + "\\XS_" + fileName, 75);
            }
        }


        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            string mimeType = "image/x-unknown";

            if (format.Equals(ImageFormat.Gif))
            {
                mimeType = "image/gif";
            }
            else if (format.Equals(ImageFormat.Jpeg))
            {
                mimeType = "image/jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                mimeType = "image/png";
            }
            else if (format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.MemoryBmp))
            {
                mimeType = "image/bmp";
            }
            else if (format.Equals(ImageFormat.Tiff))
            {
                mimeType = "image/tiff";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                mimeType = "image/x-icon";
            }

            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo e = encoders.FirstOrDefault(x => x.MimeType == mimeType);
            return e;
        }

        // Bitmap bytes have to be created via a direct memory copy of the bitmap
        private static byte[] BmpToBytesMemStream(Bitmap bmp)
        {
            // convert to jpeg
            return BmpToBytesMemStream(bmp, ImageFormat.Jpeg);
        }
        private static byte[] BmpToBytesMemStream(Bitmap bmp, ImageFormat imgFormat)
        {
            var ms = new MemoryStream();

            // Save to memory using the Jpeg format
            //var info = ImageCodecInfo.GetImageEncoders();
            var jgpEncoder = GetEncoder(imgFormat);

            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

            bmp.Save(ms, jgpEncoder, encoderParameters);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }

        public static byte[] ImageToByteArray(string imagefilePath)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(imagefilePath);
            byte[] imageByte = ImageToByteArraybyMemoryStream(image);
            return imageByte;
        }
        private static byte[] ImageToByteArraybyMemoryStream(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public static void ByteArrayToImageFilebyMemoryStream(byte[] imageByte, string fileMapPath)
        {
            MemoryStream ms = new MemoryStream(imageByte);
            Image image = Image.FromStream(ms);
            image.Save(fileMapPath);
        }

        public static bool IsImageFile(FileInfo sFileInfo)
        {
            return IsImageFile(sFileInfo.Extension);
        }

        public static bool IsImageFile(string strExtension)
        {
            return strExtension.ToLower() == ".jpg" | strExtension.ToLower() == ".jpeg" | strExtension.ToLower() == ".gif" | strExtension.ToLower() == ".png" | strExtension.ToLower() == ".tiff" | strExtension.ToLower() == ".bmp";
        }

        public static void AddWatermark(string imageFilePath, string waterMarkImagePath, string newImageMapPath)
        {
            //add watermark if needed
            if (!string.IsNullOrEmpty(waterMarkImagePath))
            {
                using (var output = new ImgWaterMark(imageFilePath, waterMarkImagePath, true))
                {
                    output.AddWaterMark();
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    output.Image.Save(newImageMapPath, jpgEncoder, myEncoderParameters);
                }
            }
        }

        public static Bitmap CreateCanvas(int width, int height)
        {
            return CreateCanvas(width, height, "White");
        }

        public static Bitmap CreateCanvas(int width, int height, string backGroundColor)
        {
            var b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(b);
            var colorBrush = new SolidBrush(Color.FromName(backGroundColor));
            g.FillRectangle(colorBrush, 0, 0, width, height);
            g.Dispose();
            colorBrush.Dispose();
            return b;
        }

        public static void AddToCanvas(string imageFilePath, string canvasImagePath)
        {
            //add watermark if needed
            if (!string.IsNullOrEmpty(canvasImagePath))
            {
                var output = new ImgWaterMark(canvasImagePath, imageFilePath, false);
                output.AddWaterMark();
                FileUtils.SaveFile(imageFilePath, BmpToBytesMemStream(output.Image));
            }
        }

        public static string ResizeImageToJpg(string fileNameIn, string fileNameOut, double imgSize)
        {
            try
            {
                return ResizeImage(fileNameIn, GeneralUtils.ReplaceFileExt(fileNameOut, ".jpg"), imgSize);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ResizeImageToPng(string fileNameIn, string fileNameOut, double imgSize)
        {
            try
            {
                return ResizeImage(fileNameIn, GeneralUtils.ReplaceFileExt(fileNameOut, ".png"), imgSize);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ResizeImage(string fileNamePath, string fileNamePathOut, double intMaxWidth)
        {

            try
            {
                // Get source Image
                using (var sourceImage = new Bitmap(fileNamePath))
                {
                    var thumbW = 0;
                    var thumbH = 0;
                    if (sourceImage.Height > sourceImage.Width)
                    {
                        thumbH = Convert.ToInt32(intMaxWidth);
                        thumbW = 0;
                    }
                    else
                    {
                        thumbW = Convert.ToInt32(intMaxWidth);
                        thumbH = 0;
                    }

                    var fName1 = Path.GetFileName(fileNamePathOut);
                    if (fName1 != null)
                    {
                        var fName2 = fName1.Replace(" ", "_");
                        fileNamePathOut = fileNamePathOut.Replace(fName1, fName2);

                        using (var newImage = CreateThumbnail(fileNamePath, Convert.ToInt32(thumbW), Convert.ToInt32(thumbH)))
                        {

                            if ((newImage != null))
                            {
                                ImageCodecInfo useEncoder;
                                var extension = Path.GetExtension(fileNamePathOut);
                                if (extension != null && extension.ToLower() == ".png")
                                {
                                    useEncoder = GetEncoder(ImageFormat.Png);
                                }
                                else
                                {
                                    useEncoder = GetEncoder(ImageFormat.Jpeg);
                                }

                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

                                try
                                {
                                    newImage.Save(fileNamePathOut, useEncoder, encoderParameters);
                                }
                                catch (Exception)
                                {
                                    GC.Collect();
                                    // attempt to clear all file locks and try again
                                    try
                                    {
                                        newImage.Save(fileNamePathOut, useEncoder, encoderParameters);
                                    }
                                    // ReSharper disable EmptyGeneralCatchClause
                                    catch
                                    // ReSharper restore EmptyGeneralCatchClause
                                    {
                                        //abandon save. 
                                        //Assumption is the thumb already is there, but locked. So no need for error.
                                    }
                                }

                                // Clean up
                                newImage.Dispose();
                            }
                        }
                    }

                    // Clean up
                    sourceImage.Dispose();
                }

                return fileNamePathOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static int GetThumbWidth(string thumbSize)
        {
            if (!GeneralUtils.IsNumeric(thumbSize) & !string.IsNullOrEmpty(thumbSize))
            {
                var thumbSplit = thumbSize.Split('x');
                return Convert.ToInt32(thumbSplit[0]);
            }
            if (GeneralUtils.IsNumeric(thumbSize))
            {
                return Convert.ToInt32(thumbSize);
            }
            return 0;
        }

        public static int GetThumbHeight(string thumbSize)
        {
            var thumbH = -1;
            if (!GeneralUtils.IsNumeric(thumbSize) & !string.IsNullOrEmpty(thumbSize))
            {
                string[] thumbSplit = thumbSize.Split('x');
                if (thumbSplit.GetUpperBound(0) >= 1)
                {
                    thumbH = Convert.ToInt32(thumbSplit[1]);
                }
            }
            return Convert.ToInt32(thumbH);
        }

        public static string GetThumbFileName(string imgPathName, int thumbW)
        {
            return GetThumbFileName(imgPathName, thumbW, 0);
        }

        public static string GetThumbFileName(string imgPathName, int thumbW, int thumbH)
        {
            if (thumbH < 0)
            {
                return string.Format("Thumb_{0}{1}", thumbW, Path.GetFileName(imgPathName));
            }
            return string.Format("Thumb_{0}x{1}{2}", thumbW, thumbH, Path.GetFileName(imgPathName));
        }

        public static string GetThumbFilePathName(string imgPathName, int thumbW)
        {
            return GetThumbFilePathName(imgPathName, thumbW, 0);
        }

        public static string GetThumbFilePathName(string imgPathName, int thumbW, int thumbH)
        {
            var fileName = Path.GetFileName(imgPathName);
            if (!string.IsNullOrEmpty(fileName))
            {
                return imgPathName.Replace(fileName, GetThumbFileName(imgPathName, thumbW, thumbH));
            }
            return imgPathName;
        }

        public static void CreateThumbOnDisk(string imgPathName, string thumbSizeCsv)
        {

            if (!string.IsNullOrEmpty(thumbSizeCsv))
            {

                if (!string.IsNullOrEmpty(imgPathName))
                {
                    var thumbSizeList = thumbSizeCsv.Split(',');

                    for (var lp = 0; lp <= thumbSizeList.GetUpperBound(0); lp++)
                    {
                        var thumbW = GetThumbWidth(thumbSizeList[lp]);
                        var thumbH = GetThumbHeight(thumbSizeList[lp]);
                        var filePathOut = GetThumbFilePathName(imgPathName, thumbW, thumbH);

                        using (var newImage = CreateThumbnail(imgPathName, Convert.ToInt32(thumbW), Convert.ToInt32(thumbH)))
                        {

                            if ((newImage != null))
                            {
                                ImageCodecInfo useEncoder;
                                var extension = Path.GetExtension(imgPathName);
                                if (extension != null && extension.ToLower() == ".png")
                                {
                                    useEncoder = GetEncoder(ImageFormat.Png);
                                }
                                else
                                {
                                    useEncoder = GetEncoder(ImageFormat.Jpeg);
                                }

                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

                                try
                                {
                                    newImage.Save(filePathOut, useEncoder, encoderParameters);
                                }
                                catch (Exception)
                                {
                                    GC.Collect();
                                    // attempt to clear all file locks and try again
                                    try
                                    {
                                        newImage.Save(filePathOut, useEncoder, encoderParameters);
                                    }
                                    // ReSharper disable EmptyGeneralCatchClause
                                    catch
                                    // ReSharper restore EmptyGeneralCatchClause
                                    {
                                        //abandon save. 
                                        //Assumption is the thumb already is there, but locked. So no need for error.
                                    }
                                }

                                // Clean up
                                newImage.Dispose();
                            }
                        }
                    }
                }
            }


        }

        public static Bitmap CreateThumbnail(string strFilepath, int intMaxWidth)
        {
            return CreateThumbnail(strFilepath, intMaxWidth, 0);
        }

        public static Bitmap CreateThumbnail(string strFilepath, int intMaxWidth, int intMaxHeight)
        {
            Bitmap newImage = null;

            if (System.IO.File.Exists(strFilepath))
            {
                try
                {
                    using (var sourceImage = new Bitmap(strFilepath))
                    {

                        var intSourceWidth = sourceImage.Width;
                        var intSourceHeight = sourceImage.Height;

                        //check if we dynamically choose Height or Width
                        if (intMaxHeight == -1)
                        {
                            intMaxHeight = 0;
                            if (intSourceWidth < intSourceHeight)
                            {
                                intMaxHeight = intMaxWidth;
                                intMaxWidth = 0;
                            }
                        }

                        if ((intMaxWidth > 0 & intMaxWidth < intSourceWidth) | (intMaxHeight > 0 & intMaxHeight < intSourceHeight))
                        {

                            // Resize image:
                            double aspect = sourceImage.PhysicalDimension.Width / sourceImage.PhysicalDimension.Height;

                            Bitmap cropImage;

                            int newWidth;
                            int newHeight;

                            if (intMaxWidth == 0)
                            {
                                intMaxWidth = Convert.ToInt32(intMaxHeight * aspect);
                            }
                            if (intMaxHeight == 0)
                            {
                                intMaxHeight = Convert.ToInt32(intMaxWidth / aspect);
                            }

                            if (sourceImage.PhysicalDimension.Height < sourceImage.PhysicalDimension.Width)
                            {
                                newWidth = Convert.ToInt32(intMaxHeight * aspect);
                                newHeight = intMaxHeight;
                            }
                            else
                            {
                                newWidth = intMaxWidth;
                                newHeight = Convert.ToInt32(intMaxWidth / aspect);
                            }

                            if (newWidth < intMaxWidth)
                            {
                                newWidth = intMaxWidth;
                                newHeight = Convert.ToInt32(intMaxWidth / aspect);
                            }
                            if (newHeight < intMaxHeight)
                            {
                                newHeight = intMaxHeight;
                                newWidth = Convert.ToInt32(intMaxHeight * aspect);
                            }

                            cropImage = new Bitmap(newWidth, newHeight);
                            var gc = Graphics.FromImage(cropImage);
                            gc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gc.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                            var destinationRec = new Rectangle(0, 0, intMaxWidth, intMaxHeight);
                            newImage = new Bitmap(intMaxWidth, intMaxHeight);
                            var g = Graphics.FromImage(newImage);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(cropImage, destinationRec, Convert.ToInt32((cropImage.Width - intMaxWidth) / 2), Convert.ToInt32((cropImage.Height - intMaxHeight) / 2), intMaxWidth, intMaxHeight, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            var cloneRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                            var format = sourceImage.PixelFormat;
                            newImage = sourceImage.Clone(cloneRect, format);
                            newImage.SetResolution(72, 72);
                        }

                        sourceImage.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    // invalid bitmap file
                    DNNrocketUtils.LogDebug("Invalid BitMap File: " + strFilepath + "  : " + ex.ToString());
                    DNNrocketUtils.LogException(ex);
                }
            }
            return newImage;

        }
    }


    public class ImgWaterMark : IDisposable
    {


        private Bitmap _bmp;
        private readonly Bitmap _wbmp;

        private readonly bool _isPng;

        public ImgWaterMark(string physicalPathToImage, string physicalPathToWatermarkImage) : this(physicalPathToImage, physicalPathToWatermarkImage, false)
        {
        }

        public ImgWaterMark(string physicalPathToImage, string physicalPathToWatermarkImage, bool makeTransparent)
        {
            _bmp = new Bitmap(physicalPathToImage);
            _wbmp = new Bitmap(physicalPathToWatermarkImage);
            if (makeTransparent)
            {
                var extension = System.IO.Path.GetExtension(physicalPathToWatermarkImage);
                if (extension != null && extension.ToLower() == ".png")
                {
                    _isPng = true;
                }
                else
                {
                    _isPng = false;
                }
            }
            else
            {
                //set png so no tranparent is done.
                _isPng = true;
            }
        }

        public void AddWaterMark()
        {
            //get the drawing canvas (graphics object) from the bitmap

            // ReSharper disable PossibleLossOfFraction
            float x = (_bmp.Width - _wbmp.Width) / 2;
            // ReSharper restore PossibleLossOfFraction
            // ReSharper disable PossibleLossOfFraction
            float y = (_bmp.Height - _wbmp.Height) / 2;
            // ReSharper restore PossibleLossOfFraction

            Graphics canvas;

            try
            {
                canvas = Graphics.FromImage(_bmp);
            }
            catch (Exception)
            {
                //You cannot create a Graphics object 
                //from an image with an indexed pixel format.
                //If you want to open this image and draw 
                //on it you need to do the following...
                //size the new bitmap to the source bitmaps dimensions
                var bmpNew = new Bitmap(_bmp.Width, _bmp.Height);
                canvas = Graphics.FromImage(bmpNew);
                //draw the old bitmaps contents to the new bitmap
                //paint the entire region of the old bitmap to the 
                //new bitmap..use the rectangle type to 
                //select area of the source image
                canvas.DrawImage(_bmp, new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), 0, 0, _bmp.Width, _bmp.Height, GraphicsUnit.Pixel);
                _bmp = bmpNew;
            }

            var wb = DrawWatermark(_wbmp);
            canvas.DrawImage(wb, x, y, _wbmp.Width, _wbmp.Height);

            //release image
            wb.Dispose();
            _wbmp.Dispose();
            canvas.Dispose();

        }

        public Bitmap Image
        {
            get { return _bmp; }
        }

        public bool IsPng
        {
            get { return _isPng; }
        }

        private Bitmap DrawWatermark(Bitmap watermarkBm)
        {
            if (!_isPng)
            {
                watermarkBm.MakeTransparent(watermarkBm.GetPixel(0, 0));
            }
            return watermarkBm;
        }

        public void Dispose()
        {
            // dispose
            _bmp.Dispose();
            _wbmp.Dispose();
        }
    }



}
