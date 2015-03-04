using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace FS.Utils.Common
{
    /// <summary>
    ///     Thumbnail ��ժҪ˵����
    /// </summary>
    public abstract class Thumbnail
    {
        /// <summary>
        ///     ˮӡλ��
        /// </summary>
        public enum MarkPlace
        {
            /// <summary>
            ///     ���Ͻ�
            /// </summary>
            [Display(Name = "���Ͻ�")]
            LeftTop,

            /// <summary>
            ///     ���Ͻ�
            /// </summary>
            [Display(Name = "���Ͻ�")]
            RightTop,

            /// <summary>
            ///     ���½�
            /// </summary>
            [Display(Name = "���½�")]
            LeftBottom,

            /// <summary>
            ///     ���½�
            /// </summary>
            [Display(Name = "���½�")]
            RightBottom,

            /// <summary>
            ///     ͼƬ����
            /// </summary>
            [Display(Name = "ͼƬ����")]
            Center
        }

        /// <summary>
        ///     ͼƬ�ߴ�����
        /// </summary>
        public enum ThumbnailType
        {
            /// <summary>
            ///     ָ����,�߰�����
            /// </summary>
            W,

            /// <summary>
            ///     ָ���ߣ���������
            /// </summary>
            H,

            /// <summary>
            ///     ȡ��Сֵ
            /// </summary>
            Min,

            /// <summary>
            ///     ȡ���ֵ
            /// </summary>
            Max,

            /// <summary>
            ///     �����ֵ�����ٲü���������
            /// </summary>
            Cut
        }

        /// <summary>
        ///     ��������ͼ(�޲ü�)
        /// </summary>
        /// <param name="oldImagePath">ԭʼͼƬ</param>
        /// <param name="newImagePath">��ͼƬ</param>
        /// <param name="width">����</param>
        /// <param name="height">�߶�</param>
        /// <param name="level">ͼƬ����: 1 - 100</param>
        /// <param name="mode">ͼƬ�ߴ����� </param>
        public static void MakeThumbnail(string oldImagePath, string newImagePath, int width, int height, ThumbnailType mode, int level)
        {
            oldImagePath = Files.GetMapPath(oldImagePath);
            newImagePath = Files.GetMapPath(newImagePath);

            using (var oldimage = Image.FromFile(oldImagePath))
            {
                var towidth = width;
                var toheight = height;

                switch (mode)
                {
                    case ThumbnailType.W: toheight = oldimage.Height * width / oldimage.Width; break;
                    case ThumbnailType.H: towidth = oldimage.Width * height / oldimage.Height; break;
                    case ThumbnailType.Min: toheight = oldimage.Height * width / oldimage.Width;
                        if (toheight > height)
                        {
                            towidth = oldimage.Width * height / oldimage.Height;
                            toheight = height;
                        }
                        break;
                    case ThumbnailType.Max: //���ȸ߶�ȡ���ֵ
                        toheight = oldimage.Height * width / oldimage.Width;
                        if (toheight < height)
                        {
                            towidth = oldimage.Width * height / oldimage.Height;
                            toheight = height;
                        }
                        break;
                }
                using (var bm = new Bitmap(towidth, toheight))
                {
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.High;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(Color.White);
                        g.DrawImage(oldimage, new Rectangle(0, 0, towidth, toheight), new Rectangle(0, 0, oldimage.Width, oldimage.Height), GraphicsUnit.Pixel);
                        g.Dispose();
                    }
                    setQuality(level);
                    bm.Save(newImagePath, ici, ep);
                    bm.Dispose();
                }
                oldimage.Dispose();
            }
            if (mode == ThumbnailType.Cut) { Cut(newImagePath, 0, 0, width, height); }
        }

        /// <summary>
        ///     �ü�ͼƬ
        /// </summary>
        /// <param name="pic">ͼƬ·��</param>
        /// <param name="left">��߾�</param>
        /// <param name="top">�ϱ߾�</param>
        /// <param name="width">�ü������</param>
        /// <param name="height">�ü���߶�</param>
        public static void Cut(string pic, int left, int top, int width, int height)
        {
            pic = Files.GetMapPath(pic);

            using (Image bm = new Bitmap(width, height))
            {
                string tmpName;
                using (var image = Image.FromFile(pic))
                {
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.High;
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        g.Clear(Color.White);
                        //��ָ��λ�ò��Ұ�ָ����С����ԭͼƬ��ָ������ 
                        g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(left, top, width, height), GraphicsUnit.Pixel);
                        tmpName = pic + ".jpg";
                        setQuality(100);
                        bm.Save(tmpName, ici, ep);
                    }
                }
                File.Delete(pic);
                File.Move(tmpName, pic);
            }
        }

        /// <summary>
        ///     ͼƬˮӡ
        /// </summary>
        /// <param name="pic">ԭʼͼƬ</param>
        /// <param name="markImage">ˮӡ</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        public static void Mark(string pic, Image markImage, int x, int y)
        {
            pic = Files.GetMapPath(pic);

            string tmpName;
            using (var image = Image.FromFile(pic))
            {
                if (markImage.Width > image.Width || markImage.Height > image.Height)
                {
                    markImage.Dispose();
                    return;
                }
                using (var bitmap = new Bitmap(image.Width, image.Height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(image, 0, 0, image.Width, image.Height);
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.DrawImage(markImage, new Rectangle(x, y, markImage.Width, markImage.Height), 0, 0,
                                    markImage.Width, markImage.Height, GraphicsUnit.Pixel);
                        markImage.Dispose();
                    }
                    tmpName = pic + ".tmp";
                    setQuality(100);
                    bitmap.Save(tmpName, ici, ep);
                }
            }
            File.Delete(pic);
            File.Move(tmpName, pic);
        }

        /// <summary>
        ///     ˮӡ
        /// </summary>
        /// <param name="pic">ԭʼͼƬ</param>
        /// <param name="markImage">ˮӡ</param>
        /// <param name="place">ˮӡλ��</param>
        public static void Mark(string pic, Image markImage, MarkPlace place)
        {
            pic = Files.GetMapPath(pic);

            int y;
            var x = y = 0;
            using (var image = Image.FromFile(pic))
            {
                switch (place)
                {
                    case MarkPlace.RightBottom:
                        x = image.Width - markImage.Width - 2;
                        y = image.Height - markImage.Height - 2;
                        break;
                    case MarkPlace.LeftBottom:
                        x = 12;
                        y = image.Height - markImage.Height - 12;
                        break;
                    case MarkPlace.LeftTop:
                        x = y = 12;
                        break;
                    case MarkPlace.RightTop:
                        x = image.Width - markImage.Width - 12;
                        y = 12;
                        break;
                    case MarkPlace.Center:
                        x = (image.Width - markImage.Width) / 2;
                        y = (image.Height - markImage.Height) / 2;
                        break;
                }
            }
            Mark(pic, markImage, x, y);
            markImage.Dispose();
        }

        /// <summary>
        ///     ͼƬˮӡ
        /// </summary>
        /// <param name="pic">ԭʼͼƬ</param>
        /// <param name="markImagePath">ˮӡ·��</param>
        /// <param name="place">ˮӡλ��</param>
        public static void Mark(string pic, string markImagePath, MarkPlace place)
        {
            using (var mark = Image.FromFile(markImagePath))
            {
                Mark(pic, mark, place);
            }
        }

        /// <summary>
        ///     ����ˮӡ
        /// </summary>
        /// <param name="pic">ԭʼͼƬ</param>
        /// <param name="text">ˮӡ����</param>
        /// <param name="place">ˮӡλ��</param>
        /// <param name="fontfamily">��������</param>
        /// <param name="fontsize">���ִ�С</param>
        /// <param name="fontcolor">������ɫ</param>
        public static void Mark(string pic, string text, MarkPlace place, string fontfamily, int? fontsize, Brush fontcolor)
        {
            using (Image txt = ToPicture(text, fontfamily, fontsize, fontcolor))
            {
                Mark(pic, txt, place);
            }
        }

        /// <summary>
        ///     ת���ı�ΪBitmap
        /// </summary>
        /// <param name="text">�ı�</param>
        /// <param name="fontfamily">����</param>
        /// <param name="fontsize">�ֺ�</param>
        /// <param name="fontcolor">������ɫ</param>
        /// <returns></returns>
        public static Bitmap ToPicture(string text, string fontfamily, int? fontsize, Brush fontcolor)
        {
            if (fontcolor == null)
                fontcolor = Brushes.White;
            if (fontfamily == null)
                fontfamily = "����";
            if (fontsize == null)
                fontsize = 20;
            var font = new Font(fontfamily, (float)fontsize);
            var bmp = new Bitmap(10, 10);
            var ImageSize = Size.Empty;
            // ����ͼƬ��С
            using (var g = Graphics.FromImage(bmp))
            {
                var size = g.MeasureString(text, font, 10000);
                ImageSize.Width = (int)size.Width + 5;
                ImageSize.Height = (int)size.Height + 5;
            }
            // ����ͼƬ
            bmp = new Bitmap(ImageSize.Width, ImageSize.Height);
            // �����ı�
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                using (var f = new StringFormat())
                {
                    f.Alignment = StringAlignment.Center;
                    f.LineAlignment = StringAlignment.Center;
                    //f.FormatFlags = StringFormatFlags.NoWrap;
                    g.DrawString(text, font, fontcolor, new RectangleF(2, 2, ImageSize.Width, ImageSize.Height), f);
                }
            }
            return bmp;
        }

        /// <summary>
        ///     ͼƬ����
        /// </summary>
        /// <param name="width">��������</param>
        /// <param name="height">�����߶�</param>
        /// <param name="piclist">ͼƬ����</param>
        /// <param name="textlist">�ı�����</param>
        /// <param name="path">ͼƬ����·��</param>
        public static void AppendPicture(int width, int height, List<Picture> piclist, List<Text> textlist, string path)
        {
            path = Files.GetMapPath(path);

            using (Image frm = new Bitmap(width, height))
            {
                using (var g = Graphics.FromImage(frm))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.Clear(Color.White);
                    Image image;
                    foreach (var p in piclist)
                    {
                        using (image = Image.FromFile(p.Path))
                        {
                            g.DrawImage(image, new Rectangle(p.Left, p.Top, width, height),
                                        new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                        }
                    }
                    if (textlist.Count > 0)
                    {
                        foreach (var p in textlist)
                        {
                            using (image = ToPicture(p.Txt, p.FontFamily, p.FontSize, p.FontColor))
                            {
                                g.DrawImage(image, new Rectangle(p.Left, p.Top, width, height),
                                            new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                            }
                        }
                    }
                    setQuality(100);
                    frm.Save(path, ici, ep);
                }
            }
        }

        #region ����ͼƬ����

        /// <summary>
        ///     δ����
        /// </summary>
        public static ImageCodecInfo ici;

        /// <summary>
        ///     δ����
        /// </summary>
        public static EncoderParameters ep;

        /// <summary>
        ///     ���������ȼ�
        /// </summary>
        /// <param name="level">�����ȼ�</param>
        public static void setQuality(int level)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            ici = null;
            foreach (var codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }
            ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, level);
        }

        #endregion
    }

    /// <summary>
    ///     ͼƬʵ��
    /// </summary>
    public class Picture
    {
        /// <summary>
        ///     ���캯��
        /// </summary>
        /// <param name="left">�����</param>
        /// <param name="top">��������</param>
        /// <param name="path">·��</param>
        public Picture(int left, int top, string path)
        {
            Left = left;
            Top = top;
            Path = path;
        }

        /// <summary>
        ///     ���캯��
        /// </summary>
        /// <param name="place"></param>
        /// <param name="path">·��</param>
        public Picture(Thumbnail.MarkPlace place, string path)
        {
            switch (place)
            {
                case Thumbnail.MarkPlace.LeftTop:
                    Left = 12;
                    Top = 60;
                    break;
                default:
                    Left = 300;
                    Top = 12;
                    break;
            }
            Path = path;
        }

        /// <summary>
        ///     �����
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        ///     ·��
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    ///     �ı�ʵ��
    /// </summary>
    public class Text
    {
        /// <summary>
        ///     ���캯��
        /// </summary>
        /// <param name="left">�����</param>
        /// <param name="top">��������</param>
        /// <param name="txt">����</param>
        /// <param name="fontfamily">��������</param>
        /// <param name="fontsize">���ִ�С</param>
        /// <param name="fontcolor">������ɫ</param>
        public Text(int left, int top, string txt, string fontfamily, int? fontsize, Brush fontcolor)
        {
            Left = left;
            Top = top;
            Txt = txt;
            FontFamily = fontfamily ?? " ����";
            FontSize = fontsize ?? 12;
            FontColor = fontcolor ?? Brushes.White;
        }

        /// <summary>
        ///     �����
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        ///     �ļ�
        /// </summary>
        public string Txt { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        ///     �����С
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        ///     ������ɫ
        /// </summary>
        public Brush FontColor { get; set; }
    }
}