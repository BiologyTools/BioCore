using AForge;
using AForge.Imaging.Filters;
using BitMiracle.LibTiff.Classic;
using CSScripting;
using loci.common.services;
using loci.formats;
using loci.formats.meta;
using loci.formats.services;
using NetVips;
using Newtonsoft.Json;
using ome.xml.model.primitives;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using Image = System.Drawing.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using Size = System.Drawing.Size;
using IntRange = AForge.IntRange;
using OpenSlideGTK;

namespace BioCore
{
    /* A class declaration. */
    public static class Images
    {
        public static List<BioImage> images = new List<BioImage>();
        /// 
        /// @param ids The id of the image you want to get.
        public static BioImage GetImage(string ids)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i].ID == ids || images[i].file == ids || images[i].filename == Path.GetFileName(ids))
                    return images[i];
            }
            return null;
        }
        /// It adds an image to the list of images
        /// 
        /// @param BioImage A class that contains the image data and other information.
        public static void AddImage(BioImage im, bool newtab)
        {
            if (images.Contains(im)) return;
            im.Filename = GetImageName(im.ID);
            im.ID = im.Filename;
            images.Add(im);
            if (newtab)
                App.tabsView.AddTab(im);
            else
            {
                if (!App.viewer.Images.Contains(im))
                {
                    App.viewer.AddImage(im);
                    App.viewer.Update();
                }
            }
        }
        /// It takes a string as an argument, and returns the number of times that string appears in the
        /// list of images
        /// 
        /// @param s The name of the image
        /// 
        /// @return The number of images that contain the name of the image.
        public static int GetImageCountByName(string s)
        {
            string name = Path.GetFileName(s);
            string ext = Path.GetExtension(s);
            if (name.Split('-').Length > 2)
                name = name.Remove(name.LastIndexOf('-'), name.Length - name.LastIndexOf('-'));
            int i = 0;
            for (int im = 0; im < images.Count; im++)
            {
                if (images[im].ID.Contains(name) && images[im].ID.EndsWith(ext))
                    i++;
            }
            return i;
        }
        /// It takes a string, and returns a string
        /// 
        /// @param s The path to the image.
        /// 
        /// @return The name of the image.
        public static string GetImageName(string s)
        {
            //Here we create a unique ID for an image.
            int i = Images.GetImageCountByName(s);
            if (i == 0)
                return Path.GetFileName(s);
            string name = Path.GetFileNameWithoutExtension(s);
            if (Path.GetFileNameWithoutExtension(name) != name)
                name = Path.GetFileNameWithoutExtension(name);
            string ext = s.Substring(s.IndexOf('.'), s.Length - s.IndexOf('.'));
            if (name.Split('-').Length > 2)
            {
                string sts = name.Remove(name.LastIndexOf('-'), name.Length - name.LastIndexOf('-'));
                return sts + "-" + i + ext;
            }
            else
                return name + "-" + i + ext;
        }
        /// This function removes an image from the table
        /// 
        /// @param BioImage This is the image that you want to remove.
        public static void RemoveImage(BioImage im)
        {
            RemoveImage(im.ID);
        }
        /// It removes an image from the table
        /// 
        /// @param id The id of the image to remove.
        /// 
        /// @return The image is being returned.
        public static void RemoveImage(string id)
        {
            BioImage im = GetImage(id);
            if (im == null)
                return;
            images.Remove(im);
            //im.Dispose();
            im = null;
            Recorder.AddLine("Bio.Table.RemoveImage(" + '"' + id + '"' + ");");
        }
        /// It updates an image from the table
        /// 
        /// @param id The id of the image to update.
        /// @param im The BioImage to update with.
        /// @return The image is being returned.
        public static void UpdateImage(BioImage im)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i].ID == im.ID)
                {
                    images[i] = im;
                    return;
                }
            }
        }
    }
    /* Defining a struct called ZCT. */
    public struct ZCT
    {
        public int Z { get; set; }
        public int C { get; set; }
        public int T { get; set; }
        public ZCT(int z, int c, int t)
        {
            Z = z;
            C = c;
            T = t;
        }
        public static bool operator ==(ZCT c1, ZCT c2)
        {
            if (c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T)
                return true;
            else
                return false;
        }
        public static bool operator !=(ZCT c1, ZCT c2)
        {
            if (c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T)
                return false;
            else
                return true;
        }
        public override string ToString()
        {
            return Z + "," + C + "," + T;
        }
    }
    public struct ZCTXY
    {
        public int Z { get; set; }
        public int C { get; set; }
        public int T { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        /* A constructor for a class called ZCTXY. */
        public ZCTXY(int z, int c, int t, int x, int y)
        {
            Z = z;
            C = c;
            T = t;
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return Z + "," + C + "," + T + "," + X + "," + Y;
        }

        public static bool operator ==(ZCTXY c1, ZCTXY c2)
        {
            if (c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T && c1.X == c2.X && c1.Y == c2.Y)
                return true;
            else
                return false;
        }
        public static bool operator !=(ZCTXY c1, ZCTXY c2)
        {
            if (c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T || c1.X != c2.X || c1.Y != c2.Y)
                return false;
            else
                return true;
        }
    }
    /* A struct that is used to store the resolution of an image. */
    public struct Resolution
    {
        int x;
        int y;
        PixelFormat format;
        double px, py, pz, sx, sy, sz;
        public int SizeX
        {
            get { return x; }
            set { x = value; }
        }
        public int SizeY
        {
            get { return y; }
            set { y = value; }
        }
        public double PhysicalSizeX
        {
            get { return px; }
            set { px = value; }
        }
        public double PhysicalSizeY
        {
            get { return py; }
            set { py = value; }
        }
        public double PhysicalSizeZ
        {
            get { return pz; }
            set { pz = value; }
        }
        public double StageSizeX
        {
            get { return sx; }
            set { sx = value; }
        }
        public double StageSizeY
        {
            get { return sy; }
            set { sy = value; }
        }
        public double StageSizeZ
        {
            get { return sz; }
            set { sz = value; }
        }
        public double VolumeWidth
        {
            get
            {
                return PhysicalSizeX * SizeX;
            }
        }
        public double VolumeHeight
        {
            get
            {
                return PhysicalSizeY * SizeY;
            }
        }
        public PixelFormat PixelFormat
        {
            get { return format; }
            set { format = value; }
        }
        public int RGBChannelsCount
        {
            get
            {
                if (PixelFormat == PixelFormat.Format8bppIndexed || PixelFormat == PixelFormat.Format16bppGrayScale)
                    return 1;
                else if (PixelFormat == PixelFormat.Format24bppRgb || PixelFormat == PixelFormat.Format48bppRgb)
                    return 3;
                else
                    return 4;
            }
        }
        public long SizeInBytes
        {
            get
            {
                if (format == PixelFormat.Format8bppIndexed)
                    return (long)y * (long)x;
                else if (format == PixelFormat.Format16bppGrayScale)
                    return (long)y * (long)x * 2;
                else if (format == PixelFormat.Format24bppRgb)
                    return (long)y * (long)x * 3;
                else if (format == PixelFormat.Format32bppRgb || format == PixelFormat.Format32bppArgb)
                    return (long)y * (long)x * 4;
                else if (format == PixelFormat.Format48bppRgb || format == PixelFormat.Format48bppRgb)
                    return (long)y * (long)x * 6;
                throw new NotSupportedException(format + " is not supported.");
            }
        }
        public Resolution(int w, int h, int omePx, int bitsPerPixel, double physX, double physY, double physZ, double stageX, double stageY, double stageZ)
        {
            x = w;
            y = h;
            sx = stageX;
            sy = stageY;
            sz = stageZ;
            format = BioImage.GetPixelFormat(omePx, bitsPerPixel);
            px = physX;
            py = physY;
            pz = physZ;
        }
        public Resolution(int w, int h, PixelFormat f, double physX, double physY, double physZ, double stageX, double stageY, double stageZ)
        {
            x = w;
            y = h;
            format = f;
            sx = stageX;
            sy = stageY;
            sz = stageZ;
            px = physX;
            py = physY;
            pz = physZ;
        }
        public Resolution Copy()
        {
            return new Resolution(x, y, format, px, py, pz, sx, sy, sz);
        }
        public override string ToString()
        {
            return "(" + x + ", " + y + ") " + format.ToString() + " " + (SizeInBytes / 1000) + " KB";
        }
    }
    public enum RGB
    {
        R,
        G,
        B,
        Gray
    }
    public struct ColorS : IDisposable
    {
        internal byte[] bytes;
        public ColorS(ushort s)
        {
            bytes = new byte[6];
            R = s;
            G = s;
            B = s;
        }
        public ColorS(ushort r, ushort g, ushort b)
        {
            bytes = new byte[6];
            R = r;
            G = g;
            B = b;
        }
        public float Rf
        {
            get { return (float)R / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[4] = bt[1];
                bytes[5] = bt[0];
            }
        }
        public float Gf
        {
            get { return (float)G / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[2] = bt[1];
                bytes[3] = bt[0];
            }
        }
        public float Bf
        {
            get { return (float)B / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[0] = bt[1];
                bytes[1] = bt[0];
            }
        }
        public ushort R
        {
            get { return BitConverter.ToUInt16(bytes, 0); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[0] = bt[0];
                bytes[1] = bt[1];
            }
        }
        public ushort G
        {
            get { return BitConverter.ToUInt16(bytes, 2); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[2] = bt[0];
                bytes[3] = bt[1];
            }
        }
        public ushort B
        {
            get { return BitConverter.ToUInt16(bytes, 4); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[4] = bt[0];
                bytes[5] = bt[1];
            }
        }
        public byte[] Bytes { get { return bytes; } }
        public static ColorS FromColor(System.Drawing.Color col)
        {
            float r = (((float)col.R) / 255) * ushort.MaxValue;
            float g = (((float)col.G) / 255) * ushort.MaxValue;
            float b = (((float)col.B) / 255) * ushort.MaxValue;
            ColorS color = ColorS.FromVector(r, g, b);
            return color;
        }
        public static ColorS FromVector(float x, float y, float z)
        {
            ColorS color = new ColorS();
            color.bytes = new byte[6];
            color.Rf = x;
            color.Gf = y;
            color.Bf = z;
            return color;
        }
        /// It converts the RGB values of the pixel to a byte array that can be written to a bitmap
        /// 
        /// @param PixelFormat The format of the image.
        /// 
        /// @return The byte array of the pixel.
        public byte[] GetBytes(PixelFormat px)
        {
            if (px == PixelFormat.Format8bppIndexed)
            {
                byte[] bt = new byte[1];
                bt[0] = (byte)R;
                return bt;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                return BitConverter.GetBytes(R);
            }
            else
            if (px == PixelFormat.Format24bppRgb)
            {
                byte[] bt = new byte[3];
                bt[0] = (byte)B;
                bt[1] = (byte)G;
                bt[2] = (byte)R;
                return bt;
            }
            else
            if (px == PixelFormat.Format32bppRgb || px == PixelFormat.Format32bppArgb)
            {
                byte[] bt = new byte[4];
                bt[0] = 255;
                bt[1] = (byte)R;
                bt[2] = (byte)G;
                bt[3] = (byte)B;
                return bt;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                return bytes;
            }
            throw new InvalidDataException("Pixel format: " + px.ToString() + " is not supported");

        }
        /// If the bits per pixel is 8, then the color is already in the correct format. If the bits per
        /// pixel is 16, then the color needs to be converted from a 16 bit color to an 8 bit color
        /// 
        /// @param ColorS 
        /// @param bitsPerPixel 8 or 16
        /// 
        /// @return A System.Drawing.Color object.
        public static System.Drawing.Color ToColor(ColorS col, int bitsPerPixel)
        {
            if (bitsPerPixel == 8)
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb((byte)col.R, (byte)col.G, (byte)col.B);
                return c;
            }
            else
            {
                int r = (int)(((float)col.R / 65535) * 255);
                int g = (int)(((float)col.G / 65535) * 255);
                int b = (int)(((float)col.B / 65535) * 255);
                System.Drawing.Color c = System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
                return c;
            }
        }
        public SharpDX.Vector4 ToVector()
        {
            return new SharpDX.Vector4(Rf, Gf, Bf, 1.0f);
        }
        public override string ToString()
        {
            return R + "," + G + "," + B;
        }
        public override bool Equals(object obj)
        {
            ColorS s = (ColorS)obj;
            if (s.R == R && s.G == G && s.B == B)
                return true;
            else
                return false;
        }
        public static ColorS operator /(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf / b.Rf), (ushort)(a.Gf / b.Gf), (ushort)(a.Bf / b.Bf));
        }
        public static ColorS operator *(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf * b.Rf), (ushort)(a.Gf * b.Gf), (ushort)(a.Bf * b.Bf));
        }
        public static ColorS operator +(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf + b.Rf), (ushort)(a.Gf + b.Gf), (ushort)(a.Bf + b.Bf));
        }
        public static ColorS operator -(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf - b.Rf), (ushort)(a.Gf - b.Gf), (ushort)(a.Bf - b.Bf));
        }
        public static ColorS operator /(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf / b, a.Gf / b, a.Bf / b);
        }
        public static ColorS operator *(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf * b, a.Gf * b, a.Bf * b);
        }
        public static ColorS operator +(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf + b, a.Gf + b, a.Bf + b);
        }
        public static ColorS operator -(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf - b, a.Gf - b, a.Bf - b);
        }
        public static bool operator ==(ColorS a, ColorS b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return true;
            else
                return false;
        }
        public static bool operator !=(ColorS a, ColorS b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return false;
            else
                return true;
        }
        public void Dispose()
        {
            bytes = null;
        }
    }
    public struct RectangleD
    {
        private double x;
        private double y;
        private double w;
        private double h;
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double W { get { return w; } set { w = value; } }
        public double H { get { return h; } set { h = value; } }

        /* Creating a new rectangle with the given parameters. */
        public RectangleD(double X, double Y, double W, double H)
        {
            x = X;
            y = Y;
            w = W;
            h = H;
        }
        public System.Drawing.Rectangle ToRectangleInt()
        {
            return new System.Drawing.Rectangle((int)X, (int)Y, (int)W, (int)H);
        }
        /// If any of the four corners of the rectangle are inside the polygon, then the rectangle
        /// intersects with the polygon
        /// 
        /// @param RectangleD The rectangle to check for intersection with.
        /// 
        /// @return A boolean value.
        public bool IntersectsWith(RectangleD p)
        {
            if (IntersectsWith(p.X, p.Y) || IntersectsWith(p.X + p.W, p.Y) || IntersectsWith(p.X, p.Y + p.H) || IntersectsWith(p.X + p.W, p.Y + p.H))
                return true;
            else
                return false;
        }
        public bool IntersectsWith(PointD p)
        {
            return IntersectsWith(p.X, p.Y);
        }
        /// If the point is within the rectangle, return true. Otherwise, return false
        /// 
        /// @param x The x coordinate of the point to check
        /// @param y The y coordinate of the point to test.
        /// 
        /// @return A boolean value.
        public bool IntersectsWith(double x, double y)
        {
            if (X <= x && (X + W) >= x && Y <= y && (Y + H) >= y)
                return true;
            else
                return false;
        }
        public RectangleF ToRectangleF()
        {
            return new RectangleF((float)X, (float)Y, (float)W, (float)H);
        }
        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString() + ", " + W.ToString() + ", " + H.ToString();
        }

    }
    public class ROI
    {
        public enum Type
        {
            Rectangle,
            Point,
            Line,
            Polygon,
            Polyline,
            Freeform,
            Ellipse,
            Label,
            Mask
        }
        /* A property of a class. */
        public PointD Point
        {
            get
            {
                if (Points.Count == 0)
                    return new PointD(0, 0);
                if (type == Type.Line || type == Type.Ellipse || type == Type.Label || type == Type.Freeform)
                    return new PointD(BoundingBox.X, BoundingBox.Y);
                return Points[0];
            }
            set
            {
                if (Points.Count == 0)
                {
                    AddPoint(value);
                }
                else
                    UpdatePoint(value, 0);
                UpdateBoundingBox();
            }
        }
        /* A property of a class. */
        public RectangleD Rect
        {
            get
            {
                if (Points.Count == 0)
                    return new RectangleD(0, 0, 0, 0);
                if (type == Type.Line || type == Type.Polyline || type == Type.Polygon || type == Type.Freeform || type == Type.Label)
                    return BoundingBox;
                if (type == Type.Rectangle || type == Type.Ellipse)
                    return new RectangleD(Points[0].X, Points[0].Y, Points[1].X - Points[0].X, Points[2].Y - Points[0].Y);
                else
                    return new RectangleD(Points[0].X, Points[0].Y, 1, 1);
            }
            set
            {
                if (type == Type.Line || type == Type.Polyline || type == Type.Polygon || type == Type.Freeform)
                {
                    BoundingBox = value;
                }
                else
                if (Points.Count < 4 && (type == Type.Rectangle || type == Type.Ellipse))
                {
                    AddPoint(new PointD(value.X, value.Y));
                    AddPoint(new PointD(value.X + value.W, value.Y));
                    AddPoint(new PointD(value.X, value.Y + value.H));
                    AddPoint(new PointD(value.X + value.W, value.Y + value.H));
                }
                else
                if (type == Type.Rectangle || type == Type.Ellipse)
                {
                    Points[0] = new PointD(value.X, value.Y);
                    Points[1] = new PointD(value.X + value.W, value.Y);
                    Points[2] = new PointD(value.X, value.Y + value.H);
                    Points[3] = new PointD(value.X + value.W, value.Y + value.H);
                }
                UpdateBoundingBox();
            }
        }
        /* A property of a class. */
        public double X
        {
            get
            {
                return Point.X;
            }
            set
            {
                Rect = new RectangleD(value, Y, W, H);
                Recorder.AddLine("App.AddROI(" + BioImage.ROIToString(this) + ");");
            }
        }
        /* A property of a class. */
        public double Y
        {
            get
            {
                return Point.Y;
            }
            set
            {
                Rect = new RectangleD(X, value, W, H);
                Recorder.AddLine("App.AddROI(" + BioImage.ROIToString(this) + ");");
            }
        }
        public double W
        {
            get
            {
                if (type == Type.Point)
                    return strokeWidth;
                else
                    return BoundingBox.W;
            }
            set
            {
                Rect = new RectangleD(X, Y, value, H);
                Recorder.AddLine("App.AddROI(" + BioImage.ROIToString(this) + ");");
            }
        }
        public double H
        {
            get
            {
                if (type == Type.Point)
                    return strokeWidth;
                else
                    return BoundingBox.H;
            }
            set
            {
                Rect = new RectangleD(X, Y, W, value);
                Recorder.AddLine("App.AddROI(" + BioImage.ROIToString(this) + ");");
            }
        }
        public Type type;
        public static float selectBoxSize = 8f;
        private List<PointD> Points = new List<PointD>();
        public List<PointD> PointsD
        {
            get
            {
                return Points;
            }
        }
        private List<RectangleD> selectBoxs = new List<RectangleD>();
        public List<int> selectedPoints = new List<int>();
        public RectangleD BoundingBox;
        public Font font = System.Drawing.SystemFonts.DefaultFont;
        public ZCT coord;
        public System.Drawing.Color strokeColor;
        public System.Drawing.Color fillColor;
        public bool isFilled = false;
        public string id = "";
        public string roiID = "";
        public string roiName = "";
        public int serie = 0;
        private string text = "";
        public string properties;
        public double strokeWidth = 1;
        public int shapeIndex = 0;
        public bool closed = false;
        public bool selected = false;
        public PointD[] PointsImage
        {
            get
            {
                return ImageView.SelectedImage.ToImageSpace(PointsD);
            }
        }
        /// Copy() is a function that copies the values of the ROI object to a new ROI object
        /// 
        /// @return A copy of the ROI object.
        public ROI Copy()
        {
            ROI copy = new ROI();
            copy.id = id;
            copy.roiID = roiID;
            copy.roiName = roiName;
            copy.text = text;
            copy.strokeWidth = strokeWidth;
            copy.strokeColor = strokeColor;
            copy.fillColor = fillColor;
            copy.Points = Points;
            copy.selected = selected;
            copy.shapeIndex = shapeIndex;
            copy.closed = closed;
            copy.font = font;
            copy.selectBoxs = selectBoxs;
            copy.BoundingBox = BoundingBox;
            copy.isFilled = isFilled;
            copy.coord = coord;
            copy.selectedPoints = selectedPoints;

            return copy;
        }
        /// Copy() is a function that copies the ROI object and returns a new ROI object
        /// 
        /// @param ZCT A class that contains the coordinates of the image.
        /// 
        /// @return A copy of the ROI object.
        public ROI Copy(ZCT cord)
        {
            ROI copy = new ROI();
            copy.type = type;
            copy.id = id;
            copy.roiID = roiID;
            copy.roiName = roiName;
            copy.text = text;
            copy.strokeWidth = strokeWidth;
            copy.strokeColor = strokeColor;
            copy.fillColor = fillColor;
            copy.Points.AddRange(Points);
            copy.selected = selected;
            copy.shapeIndex = shapeIndex;
            copy.closed = closed;
            copy.font = font;
            copy.selectBoxs.AddRange(selectBoxs);
            copy.BoundingBox = BoundingBox;
            copy.isFilled = isFilled;
            copy.coord = cord;
            copy.selectedPoints = selectedPoints;
            return copy;
        }
        /* A property of a class. */
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (type == Type.Label)
                {
                    UpdateBoundingBox();
                }
            }
        }
        public Size TextSize
        {
            get
            {
                return TextRenderer.MeasureText(text, font);
            }
        }
        public Mask roiMask { get; set; }
        /// <summary>
        /// Represents a Mask layer.
        /// </summary>
        public class Mask : IDisposable
        {
            public float min = 0;
            float[] mask;
            int width;
            int height;
            public int Width { get { return width; } set { width = value; } }
            public int Height { get { return height; } set { height = value; } }
            public double PhysicalSizeX { get; set; }
            public double PhysicalSizeY { get; set; }
            bool updatePixbuf = true;
            bool updateColored = true;
            BufferInfo pixbuf;
            BufferInfo colored;
            bool selected = false;
            internal bool Selected
            {
                get { return selected; }
                set
                {
                    selected = value;
                    if (selected)
                        UpdateColored(System.Drawing.Color.Blue);
                    else
                        updateColored = true;
                }
            }
            public bool IsSelected(int x, int y)
            {
                int ind = y * width + x;
                if (ind > mask.Length)
                    throw new ArgumentException("Point " + x + "," + y + " is outside the mask.");
                if (mask[ind] > min)
                {
                    return true;
                }
                return false;
            }
            public float GetValue(int x, int y)
            {
                int ind = y * width + x;
                if (ind > mask.Length)
                    throw new ArgumentException("Point " + x + "," + y + " is outside the mask.");
                return mask[ind];
            }
            public void SetValue(int x, int y, float val)
            {
                int ind = y * width + x;
                if (ind > mask.Length)
                    throw new ArgumentException("Point " + x + "," + y + " is outside the mask.");
                mask[ind] = val;
                updatePixbuf = true;
                updateColored = true;
            }
            public Mask(float[] fts, int width, int height, double physX, double physY)
            {
                this.width = width;
                this.height = height;
                PhysicalSizeX = physX;
                PhysicalSizeY = physY;
                mask = fts;
            }
            public Mask(byte[] bts, int width, int height, double physX, double physY)
            {
                this.width = width;
                this.height = height;
                PhysicalSizeX = physX;
                PhysicalSizeY = physY;
                mask = new float[bts.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ind = y * width + x;
                        mask[ind] = (float)BitConverter.ToSingle(bts, ind * 4);
                    }
                }
            }
            public BufferInfo Bitmap
            {
                get
                {
                    if (updatePixbuf)
                    {
                        if (pixbuf != null)
                            pixbuf.Dispose();
                        byte[] pixelData = new byte[width * height * 4];
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int ind = y * width + x;
                                if (mask[ind] > 0)
                                {
                                    pixelData[4 * ind] = (byte)(mask[ind] / 255);// Blue
                                    pixelData[4 * ind + 1] = (byte)(mask[ind] / 255);// Green
                                    pixelData[4 * ind + 2] = (byte)(mask[ind] / 255);// Red
                                    pixelData[4 * ind + 3] = 125;// Alpha
                                }
                                else
                                    pixelData[4 * ind + 3] = 0;
                            }
                        }
                        pixbuf = new BufferInfo("",width,height,PixelFormat.Format32bppArgb,pixelData, new ZCT(), 0);
                        updatePixbuf = false;
                        return pixbuf;
                    }
                    else
                        return pixbuf;
                }
            }
            void UpdateColored(System.Drawing.Color col)
            {
                byte[] pixelData = new byte[width * height * 4];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ind = y * width + x;
                        if (mask[ind] > 0)
                        {
                            pixelData[4 * ind] = col.R;
                            pixelData[4 * ind + 1] = col.G;
                            pixelData[4 * ind + 2] = col.B;
                            pixelData[4 * ind + 3] = 125;
                        }
                        else
                            pixelData[4 * ind + 3] = 0;
                    }
                }
                colored = new BufferInfo(width, height, PixelFormat.Format32bppArgb, pixelData, new ZCT(), "");
                updateColored = false;
            }
            public BufferInfo GetColored(System.Drawing.Color col, bool forceUpdate = false)
            {
                if (updateColored || forceUpdate)
                {
                    UpdateColored(col);
                    return colored;
                }
                else
                    return colored;
            }
            public byte[] GetBytes()
            {
                byte[] pixelData = new byte[width * height * 4];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ind = y * width + x;
                        byte[] bt = BitConverter.GetBytes(mask[ind]);
                        pixelData[4 * ind] = bt[0];
                        pixelData[4 * ind + 1] = bt[1];
                        pixelData[4 * ind + 2] = bt[2];
                        pixelData[4 * ind + 3] = bt[3];
                    }
                }
                return pixelData;
            }
            public void Dispose()
            {
                if (pixbuf != null)
                    pixbuf.Dispose();
                if (colored != null)
                    colored.Dispose();
                mask = null;
            }
        }

        public ROI()
        {
            coord = new ZCT(0, 0, 0);
            strokeColor = System.Drawing.Color.Yellow;
            font = SystemFonts.DefaultFont;
            BoundingBox = new RectangleD(0, 0, 1, 1);
        }
        /// > This function returns a rectangle that is the bounding box of the object, but with a
        /// border of half the scale
        /// 
        /// @param scale the scale of the image
        /// 
        /// @return A rectangle with the following properties:
        public RectangleD GetSelectBound(double scaleX, double scaleY)
        {
            double fx = scaleX / 2;
            double fy = scaleY / 2;
            return new RectangleD(BoundingBox.X - fx, BoundingBox.Y - fy, BoundingBox.W + scaleX, BoundingBox.H + scaleY);
        }
        /// It creates a list of rectangles, each rectangle is a square with a side length of
        /// ROI.selectBoxSize, and the center of the square is the point in the list of points
        /// 
        /// @return A list of RectangleF objects.
        public RectangleD[] GetSelectBoxes()
        {
            selectBoxs.Clear();
            for (int i = 0; i < Points.Count; i++)
            {
                selectBoxs.Add(new RectangleD((Points[i].X), (Points[i].Y), ROI.selectBoxSize, ROI.selectBoxSize));
            }
            return selectBoxs.ToArray();
        }
        /// It returns an array of RectangleF objects that are used to draw the selection boxes around
        /// the points of the polygon
        /// 
        /// @param s the size of the select box
        /// 
        /// @return A list of RectangleF objects.
        public RectangleD[] GetSelectBoxes(double s)
        {
            double f = s / 2;
            selectBoxs.Clear();
            for (int i = 0; i < Points.Count; i++)
            {
                selectBoxs.Add(new RectangleD((float)(Points[i].X - f), (float)(Points[i].Y - f), (float)s, (float)s));
            }
            return selectBoxs.ToArray();
        }
        /// Create a new ROI object, add a point to it, and return it
        /// 
        /// @param ZCT a class that contains the Z, C, and T coordinates of the image.
        /// @param x x coordinate of the point
        /// @param y The y coordinate of the point
        /// 
        /// @return A new ROI object
        public static ROI CreatePoint(ZCT coord, double x, double y)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.AddPoint(new PointD(x, y));
            an.type = Type.Point;
            Recorder.AddLine("ROI.CreatePoint(new ZCT(" + coord.Z + "," + coord.C + "," + coord.T + "), " + x + "," + y + ");");
            return an;
        }
        /// Create a point ROI at the specified ZCT coordinates and x,y coordinates
        /// 
        /// @param s series
        /// @param z the z-slice of the image
        /// @param c channel
        /// @param t timepoint
        /// @param x x coordinate of the point
        /// @param y y-coordinate of the point
        /// 
        /// @return A point ROI
        public static ROI CreatePoint(int s, int z, int c, int t, double x, double y)
        {
            return CreatePoint(new ZCT(z, c, t), x, y);
        }
        /// Create a line ROI with the specified coordinates
        /// 
        /// @param ZCT Z is the Z-axis, C is the color channel, and T is the time frame.
        /// @param PointD A point in the image.
        /// @param PointD A point in the image.
        /// 
        /// @return A new ROI object.
        public static ROI CreateLine(ZCT coord, PointD x1, PointD x2)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Line;
            an.AddPoint(x1);
            an.AddPoint(x2);
            Recorder.AddLine("ROI.CreateLine(new ZCT(" + coord.Z + "," + coord.C + "," + coord.T + "), new PointD(" + x1.X + "," + x1.Y + "), new PointD(" + x2.X + "," + x2.Y + "));");
            return an;
        }
        /// Create a new ROI object with a rectangle shape, and add a line to the recorder
        /// 
        /// @param ZCT The ZCT coordinates of the image you want to create the ROI on.
        /// @param x x coordinate of the top left corner of the rectangle
        /// @param y y-coordinate of the top-left corner of the rectangle
        /// @param w width
        /// @param h height
        /// 
        /// @return A new ROI object.
        public static ROI CreateRectangle(ZCT coord, double x, double y, double w, double h)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Rectangle;
            an.Rect = new RectangleD(x, y, w, h);
            Recorder.AddLine("ROI.CreateRectangle(new ZCT(" + coord.Z + "," + coord.C + "," + coord.T + "), new RectangleD(" + x + "," + y + "," + w + "," + h + ");");
            return an;
        }
        /// Create an ellipse ROI at the specified ZCT coordinate with the specified dimensions
        /// 
        /// @param ZCT The ZCT coordinates of the image you want to create the ROI on.
        /// @param x x-coordinate of the top-left corner of the rectangle
        /// @param y The y-coordinate of the upper-left corner of the rectangle to create.
        /// @param w width
        /// @param h height
        /// 
        /// @return A new ROI object.
        public static ROI CreateEllipse(ZCT coord, double x, double y, double w, double h)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Ellipse;
            an.Rect = new RectangleD(x, y, w, h);
            Recorder.AddLine("ROI.CreateEllipse(new ZCT(" + coord.Z + "," + coord.C + "," + coord.T + "), new RectangleD(" + x + "," + y + "," + w + "," + h + ");");
            return an;
        }
        /// > Create a new ROI object of type Polygon, with the given coordinate system and points
        /// 
        /// @param ZCT The ZCT coordinate of the ROI.
        /// @param pts an array of PointD objects, which are just a pair of doubles (x,y)
        /// 
        /// @return A ROI object
        public static ROI CreatePolygon(ZCT coord, PointD[] pts)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Polygon;
            an.AddPoints(pts);
            an.closed = true;
            return an;
        }
        /// > Create a new ROI object of type Freeform, with the specified ZCT coordinates and points
        /// 
        /// @param ZCT A class that contains the Z, C, and T coordinates of the ROI.
        /// @param pts an array of PointD objects, which are just a pair of doubles (x,y)
        /// 
        /// @return A new ROI object.
        public static ROI CreateFreeform(ZCT coord, PointD[] pts)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Freeform;
            an.AddPoints(pts);
            an.closed = true;
            return an;
        }
        public static ROI CreateMask(ZCT coord, float[] mask, int width, int height, PointD loc, double physicalX, double physicalY)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Mask;
            an.roiMask = new Mask(mask, width, height, physicalX, physicalY);
            an.AddPoint(loc);
            return an;
        }
        public static ROI CreateMask(ZCT coord, byte[] mask, int width, int height, PointD loc, double physicalX, double physicalY)
        {
            ROI an = new ROI();
            an.coord = coord;
            an.type = Type.Mask;
            an.roiMask = new Mask(mask, width, height, physicalX, physicalY);
            an.AddPoint(loc);
            return an;
        }
        /// This function updates the point at the specified index
        /// 
        /// @param PointD A class that contains an X and Y coordinate.
        /// @param i The index of the point to update
        public void UpdatePoint(PointD p, int i)
        {
            if (i < Points.Count)
            {
                Points[i] = p;
            }
            UpdateBoundingBox();
        }
        /// This function returns the point at the specified index
        /// 
        /// @param i The index of the point to get.
        /// 
        /// @return The point at index i in the Points array.
        public PointD GetPoint(int i)
        {
            return Points[i];
        }
        /// It returns an array of PointD objects
        /// 
        /// @return An array of PointD objects.
        public PointD[] GetPoints()
        {
            return Points.ToArray();
        }
        /// It converts a list of points to an array of points
        /// 
        /// @return A PointF array.
        public System.Drawing.PointF[] GetPointsF()
        {
            PointF[] pfs = new PointF[Points.Count];
            for (int i = 0; i < Points.Count; i++)
            {
                pfs[i].X = (float)Points[i].X;
                pfs[i].Y = (float)Points[i].Y;
            }
            return pfs;
        }
        /// This function adds a point to the list of points that make up the polygon
        /// 
        /// @param PointD 
        public void AddPoint(PointD p)
        {
            Points.Add(p);
            UpdateBoundingBox();
        }
        /// > Adds a range of points to the Points collection and updates the bounding box
        /// 
        /// @param p The points to add to the polygon
        public void AddPoints(PointD[] p)
        {
            Points.AddRange(p);
            UpdateBoundingBox();
        }
        /// > Adds a range of float points to the Points collection and updates the bounding box
        /// 
        /// @param p The points to add to the polygon
        public void AddPoints(float[] xp, float[] yp)
        {
            for (int i = 0; i < xp.Length; i++)
            {
                Points.Add(new PointD(xp[i], yp[i]));
            }
            UpdateBoundingBox();
        }
        /// > Adds a range of float points to the Points collection and updates the bounding box
        /// 
        /// @param p The points to add to the polygon
        public void AddPoints(int[] xp, int[] yp)
        {
            for (int i = 0; i < xp.Length; i++)
            {
                Points.Add(new PointD(xp[i], yp[i]));
            }
            UpdateBoundingBox();
        }
        /// It removes points from a list of points based on an array of indexes
        /// 
        /// @param indexs an array of integers that represent the indexes of the points to be removed
        public void RemovePoints(int[] indexs)
        {
            List<PointD> inds = new List<PointD>();
            for (int i = 0; i < Points.Count; i++)
            {
                bool found = false;
                for (int ind = 0; ind < indexs.Length; ind++)
                {
                    if (indexs[ind] == i)
                        found = true;
                }
                if (!found)
                    inds.Add(Points[i]);
            }
            Points = inds;
            UpdateBoundingBox();
        }
        /// This function returns the number of points in the polygon
        /// 
        /// @return The number of points in the list.
        public int GetPointCount()
        {
            return Points.Count;
        }
        /// It takes a string of points and returns an array of PointD objects
        /// 
        /// @param s The string to convert to points.
        /// 
        /// @return A list of points.
        public PointD[] stringToPoints(string s)
        {
            List<PointD> pts = new List<PointD>();
            string[] ints = s.Split(' ');
            for (int i = 0; i < ints.Length; i++)
            {
                string[] sints;
                if (s.Contains("\t"))
                    sints = ints[i].Split('\t');
                else
                    sints = ints[i].Split(',');
                double x = double.Parse(sints[0]);
                double y = double.Parse(sints[1]);
                pts.Add(new PointD(x, y));
            }
            return pts.ToArray();
        }
        /// This function takes a BioImage object and returns a string of the points in the image space
        /// 
        /// @param BioImage The image that the ROI is on
        /// 
        /// @return The points of the polygon in the image space.
        public string PointsToString(BioImage b)
        {
            string pts = "";
            PointD[] ps = b.ToImageSpace(Points);
            for (int j = 0; j < ps.Length; j++)
            {
                if (j == ps.Length - 1)
                    pts += ps[j].X.ToString() + "," + ps[j].Y.ToString();
                else
                    pts += ps[j].X.ToString() + "," + ps[j].Y.ToString() + " ";
            }
            return pts;
        }
        /// It takes the minimum and maximum X and Y values of the points in the shape and uses them to
        /// create a bounding box
        public void UpdateBoundingBox()
        {
            if (type == Type.Label)
            {
                if (text != "")
                {
                    Size s = TextSize;
                    BoundingBox = new RectangleD(Points[0].X, Points[0].Y, s.Width, s.Height);
                }
            }
            else
            {
                PointD min = new PointD(double.MaxValue, double.MaxValue);
                PointD max = new PointD(double.MinValue, double.MinValue);
                foreach (PointD p in Points)
                {
                    if (min.X > p.X)
                        min.X = p.X;
                    if (min.Y > p.Y)
                        min.Y = p.Y;

                    if (max.X < p.X)
                        max.X = p.X;
                    if (max.Y < p.Y)
                        max.Y = p.Y;
                }
                RectangleD r = new RectangleD();
                r.X = min.X;
                r.Y = min.Y;
                r.W = max.X - min.X;
                r.H = max.Y - min.Y;
                if (r.W == 0)
                    r.W = 1;
                if (r.H == 0)
                    r.H = 1;
                BoundingBox = r;
            }
        }
        /// It returns a string that contains the type of the object, the text, the width and height,
        /// and the coordinates of the object
        /// 
        /// @return The type of the object, the text, the width, the height, the point, and the
        /// coordinates.
        public override string ToString()
        {
            return type.ToString() + ", " + Text + " (" + W + ", " + H + "); " + " (" + Point.X + ", " + Point.Y + ") " + coord.ToString();
        }
    }
    public class Channel : IDisposable
    {
        public IntRange[] range;
        public ChannelInfo info;
        public Statistics[] stats;
        private ome.xml.model.enums.ContrastMethod contrastMethod = null;
        private ome.xml.model.enums.IlluminationType illuminationType = null;
        private ome.xml.model.enums.AcquisitionMode acquisitionMode = null;
        private int index;
        [Serializable]
        public struct ChannelInfo
        {
            internal string name;
            internal string ID;
            internal int index;
            internal string fluor;
            internal int samplesPerPixel;
            internal System.Drawing.Color? color;
            internal int emission;
            internal int excitation;
            internal int exposure;
            internal string lightSource;
            internal double lightSourceIntensity;
            internal int lightSourceWavelength;
            internal string contrastMethod;
            internal string illuminationType;
            internal int bitsPerPixel;
            internal int min, max;
            internal string diodeName;
            internal string lightSourceAttenuation;
            internal string acquisitionMode;
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }

            }
            public int Min
            {
                get
                {
                    return min;
                }
                set
                {
                    if (value <= 0)
                    {
                        max = 0;
                        return;
                    }
                    if (value > ushort.MaxValue)
                        min = 0;
                    else
                        min = value;
                }
            }
            public int Max
            {
                get
                {
                    return max;
                }
                set
                {
                    if (value <= 0)
                    {
                        max = 1;
                        return;
                    }
                    if (value > ushort.MaxValue)
                        max = ushort.MaxValue;
                    else
                        max = value;

                }
            }
            public string Fluor
            {
                get { return fluor; }
                set { fluor = value; }
            }
            public int SamplesPerPixel
            {
                get { return samplesPerPixel; }
                set { samplesPerPixel = value; }
            }
            public System.Drawing.Color? Color
            {
                get { return color; }
                set { color = value; }
            }
            public int Emission
            {
                get { return emission; }
                set { emission = value; }
            }
            public int Excitation
            {
                get { return excitation; }
                set { excitation = value; }
            }
            public int Exposure
            {
                get { return exposure; }
                set { exposure = value; }
            }
            public string LightSource
            {
                get { return lightSource; }
                set { lightSource = value; }
            }
            public double LightSourceIntensity
            {
                get { return lightSourceIntensity; }
                set { lightSourceIntensity = value; }
            }
            public int LightSourceWavelength
            {
                get { return lightSourceWavelength; }
                set { lightSourceWavelength = value; }
            }
            public string ContrastMethod
            {
                get
                {
                    if (contrastMethod == null)
                        return ome.xml.model.enums.ContrastMethod.BRIGHTFIELD.ToString();
                    return contrastMethod;
                }
                set
                {
                    contrastMethod = value;
                }
            }
            public string IlluminationType
            {
                get
                {
                    if (illuminationType == null)
                        return ome.xml.model.enums.IlluminationType.OTHER.ToString();
                    return illuminationType;
                }
                set
                {
                    illuminationType = value;
                }
            }
            public string DiodeName
            {
                get { return diodeName; }
                set
                {
                    diodeName = value;
                }
            }
            public string LightSourceAttenuation
            {
                get { return lightSourceAttenuation; }
                set
                {
                    lightSourceAttenuation = value;
                }
            }
            public string AcquisitionMode
            {
                get
                {
                    if (acquisitionMode == null)
                        return ome.xml.model.enums.AcquisitionMode.WIDEFIELD.ToString();
                    return acquisitionMode;
                }
                set
                {
                    acquisitionMode = value;
                }
            }
        }
        /// > The function takes a wavelength in nanometers and returns a color in RGB
        /// 
        /// @param l wavelength in nanometers
        /// 
        /// @return A color.
        public static System.Drawing.Color SpectralColor(double l) // RGB <0,1> <- lambda l <400,700> [nm]
        {
            double t;
            double r = 0;
            double g = 0;
            double b = 0;
            if ((l >= 400.0) && (l < 410.0)) { t = (l - 400.0) / (410.0 - 400.0); r = +(0.33 * t) - (0.20 * t * t); }
            else if ((l >= 410.0) && (l < 475.0)) { t = (l - 410.0) / (475.0 - 410.0); r = 0.14 - (0.13 * t * t); }
            else if ((l >= 545.0) && (l < 595.0)) { t = (l - 545.0) / (595.0 - 545.0); r = +(1.98 * t) - (t * t); }
            else if ((l >= 595.0) && (l < 650.0)) { t = (l - 595.0) / (650.0 - 595.0); r = 0.98 + (0.06 * t) - (0.40 * t * t); }
            else if ((l >= 650.0) && (l < 700.0)) { t = (l - 650.0) / (700.0 - 650.0); r = 0.65 - (0.84 * t) + (0.20 * t * t); }
            if ((l >= 415.0) && (l < 475.0)) { t = (l - 415.0) / (475.0 - 415.0); g = +(0.80 * t * t); }
            else if ((l >= 475.0) && (l < 590.0)) { t = (l - 475.0) / (590.0 - 475.0); g = 0.8 + (0.76 * t) - (0.80 * t * t); }
            else if ((l >= 585.0) && (l < 639.0)) { t = (l - 585.0) / (639.0 - 585.0); g = 0.84 - (0.84 * t); }
            if ((l >= 400.0) && (l < 475.0)) { t = (l - 400.0) / (475.0 - 400.0); b = +(2.20 * t) - (1.50 * t * t); }
            else if ((l >= 475.0) && (l < 560.0)) { t = (l - 475.0) / (560.0 - 475.0); b = 0.7 - (t) + (0.30 * t * t); }
            r *= 255;
            g *= 255;
            b *= 255;
            return System.Drawing.Color.FromArgb(255, (int)r, (int)g, (int)b);
        }
        public string Name
        {
            get { return info.name; }
            set { info.name = value; }
        }
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }
        public IntRange RangeR
        {
            get
            {
                return range[0];
            }
        }
        public IntRange RangeG
        {
            get
            {
                if (range.Length > 1)
                    return range[1];
                else
                    return range[0];
            }
        }
        public IntRange RangeB
        {
            get
            {
                if (range.Length > 1)
                    return range[2];
                else
                    return range[0];
            }
        }
        public string Fluor
        {
            get { return info.fluor; }
            set { info.fluor = value; }
        }
        public int SamplesPerPixel
        {
            get { return info.samplesPerPixel; }
            set
            {
                info.samplesPerPixel = value;
                range = new IntRange[info.samplesPerPixel];
            }
        }
        public System.Drawing.Color? Color
        {
            get { return info.color; }
            set { info.color = value; }
        }
        public System.Drawing.Color EmissionColor
        {
            get { return SpectralColor(Emission); }
        }
        public int Emission
        {
            get { return info.emission; }
            set { info.emission = value; }
        }
        public int Excitation
        {
            get { return info.excitation; }
            set { info.excitation = value; }
        }
        public int Exposure
        {
            get { return info.exposure; }
            set { info.exposure = value; }
        }
        public string LightSource
        {
            get { return info.lightSource; }
            set { info.lightSource = value; }
        }
        public double LightSourceIntensity
        {
            get { return info.lightSourceIntensity; }
            set { info.lightSourceIntensity = value; }
        }
        public int LightSourceWavelength
        {
            get { return info.lightSourceWavelength; }
            set { info.lightSourceWavelength = value; }
        }
        public string LightSourceAttenuation
        {
            get { return info.lightSourceAttenuation; }
            set { info.lightSourceAttenuation = value; }
        }
        public string DiodeName
        {
            get { return info.diodeName; }
            set { info.diodeName = value; }
        }
        public ome.xml.model.enums.ContrastMethod ContrastMethod
        {
            get { return contrastMethod; }
            set
            {
                contrastMethod = value;
            }
        }
        public ome.xml.model.enums.IlluminationType IlluminationType
        {
            get { return illuminationType; }
            set { illuminationType = value; }
        }
        public ome.xml.model.enums.AcquisitionMode AcquisitionMode
        {
            get { return acquisitionMode; }
            set { acquisitionMode = value; }
        }

        public int BitsPerPixel
        {
            get { return info.bitsPerPixel; }
            set { info.bitsPerPixel = value; }
        }
        public Channel(int ind, int bitsPerPixel, int samples)
        {
            info = new ChannelInfo();
            if (bitsPerPixel == 16)
                info.Max = 65535;
            if (bitsPerPixel == 14)
                info.Max = 16383;
            if (bitsPerPixel == 12)
                info.Max = 4096;
            if (bitsPerPixel == 10)
                info.Max = 1024;
            if (bitsPerPixel == 8)
                info.Max = byte.MaxValue;
            info.samplesPerPixel = samples;
            range = new IntRange[info.SamplesPerPixel];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = new IntRange(0, info.Max);
            }
            info.min = 0;
            info.bitsPerPixel = bitsPerPixel;
            info.Name = ind.ToString();
            contrastMethod = ome.xml.model.enums.ContrastMethod.BRIGHTFIELD;
            illuminationType = ome.xml.model.enums.IlluminationType.TRANSMITTED;
            info.Fluor = "None";
            info.LightSource = "Unknown";
            info.diodeName = "LED";
            info.lightSourceAttenuation = "1.0";
            info.LightSource = null;
            Index = ind;
        }
        public Channel Copy()
        {
            Channel c = new Channel(info.index, info.bitsPerPixel, SamplesPerPixel);
            c.Name = Name;
            c.info.ID = info.ID;
            c.range = range;
            c.info.color = info.color;
            c.Fluor = Fluor;
            c.SamplesPerPixel = SamplesPerPixel;
            c.Emission = Emission;
            c.Excitation = Excitation;
            c.Exposure = Exposure;
            c.LightSource = LightSource;
            c.LightSourceIntensity = LightSourceIntensity;
            c.LightSourceWavelength = LightSourceWavelength;
            c.contrastMethod = contrastMethod;
            c.illuminationType = illuminationType;
            c.DiodeName = DiodeName;
            c.LightSourceAttenuation = LightSourceAttenuation;
            return c;
        }
        public override string ToString()
        {
            if (Name == "")
                return index.ToString();
            else
                return index + ", " + Name;
        }
        public void Dispose()
        {
            if (stats != null)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    if (stats[i] != null)
                        stats[i].Dispose();
                }
            }
        }
    }
    public class Plane
    {
        private double exposure;
        private ZCT coordinate;
        private double delta;
        private Point3D location;
        public double Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }
        public ZCT Coordinate
        {
            get { return coordinate; }
            set { coordinate = value; }
        }
        public double Delta
        {
            get { return delta; }
            set { delta = value; }
        }
        public Point3D Location
        {
            get { return location; }
            set { location = value; }
        }
    }
    public class BufferInfo : IDisposable
    {
        public ushort GetValueRGB(int x, int y, int RGBChannel)
        {
            if (bytes == null)
                return 0;
            if (x >= SizeX || x < 0)
                x = 0;
            if (y >= SizeY || y < 0)
                y = 0;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount) + (RGBChannel * 2);
                return BitConverter.ToUInt16(bytes, index2);
            }
            else
            {
                int stride = SizeX;
                int index = ((y * stridex + x) * RGBChannelsCount) + RGBChannel;
                return bytes[index];
            }
        }
        public ColorS GetPixel(int ix, int iy)
        {
            if (isRGB)
                return new ColorS(GetValueRGB(ix, iy, 0), GetValueRGB(ix, iy, 1), GetValueRGB(ix, iy, 2));
            else
            {
                ushort s = GetValueRGB(ix, iy, 0);
                return new ColorS(s, s, s);
            }
        }
        public void SetPixel(int ix, int iy, ColorS col)
        {
            if (isRGB)
            {
                SetColorRGB(ix, iy, col);
            }
            else
                SetValue(ix, iy, col.R);
        }
        /// It takes a pixel coordinate and a value, and sets the pixel to that value
        /// 
        /// @param x The x coordinate of the pixel to set
        /// @param y The y coordinate of the pixel to set
        /// @param value the value to be set
        public void SetValue(int x, int y, ushort value)
        {
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount);
                byte upper = (byte)(value >> 8);
                byte lower = (byte)(value & 0xff);
                bytes[index2] = upper;
                bytes[index2 + 1] = lower;
            }
            else
            {
                int index = (y * stridex + x) * RGBChannelsCount;
                bytes[index] = (byte)value;
            }
        }
        /// The function takes a pixel coordinate (ix, iy) and a color channel (RGBChannel) and a value
        /// (value) and sets the pixel value to the value
        /// 
        /// @param ix x coordinate of the pixel
        /// @param iy The y coordinate of the pixel to set.
        /// @param RGBChannel 0 = Red, 1 = Green, 2 = Blue
        /// @param value the value to set the pixel to
        public void SetValueRGB(int ix, int iy, int RGBChannel, ushort value)
        {
            int x = ix;
            int y = iy;
            //We invert the RGB channel parameter since pixels are in BGR order.
            if (RGBChannel == 2)
                RGBChannel = 0;
            else
            if (RGBChannel == 0)
                RGBChannel = 2;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount) + (RGBChannel * 2);
                byte upper = (byte)(value >> 8);
                byte lower = (byte)(value & 0xff);
                bytes[index2] = upper;
                bytes[index2 + 1] = lower;
            }
            else
            {
                int index = ((y * stridex + x) * RGBChannelsCount) + RGBChannel;
                bytes[index] = (byte)value;
            }
        }
        /// The function takes a pixel coordinate and a color value and sets the pixel to that color
        /// 
        /// @param ix x coordinate of the pixel
        /// @param iy The y coordinate of the pixel to set
        /// @param ColorS a struct that contains a byte array of 6 bytes.
        public void SetColorRGB(int ix, int iy, ColorS value)
        {
            int x = ix;
            int y = iy;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 6);
                bytes[index2] = value.bytes[5];
                bytes[index2 + 1] = value.bytes[4];
                bytes[index2 + 2] = value.bytes[3];
                bytes[index2 + 3] = value.bytes[2];
                bytes[index2 + 4] = value.bytes[1];
                bytes[index2 + 5] = value.bytes[0];
            }
            else
            {
                int index2 = ((y * stridex + x) * RGBChannelsCount);
                bytes[index2 + 2] = (byte)value.R;
                bytes[index2 + 1] = (byte)value.G;
                bytes[index2] = (byte)value.B;
            }
        }
        /// It takes a filepath and an index and returns a string that is the filepath with a slash and
        /// the index appended to it
        /// 
        /// @param filepath The path to the file.
        /// @param index The index of the file in the folder.
        /// 
        /// @return The filepath and the index.
        public static string CreateID(string filepath, int index)
        {
            if (filepath == null)
                return "";
            const char sep = '/';
            filepath = filepath.Replace("\\", "/");
            string s = filepath + sep + 'i' + sep + index;
            return s;
        }
        public string ID;
        public string File
        {
            get { return file; }
            set { file = value; }
        }
        public int HashID
        {
            get
            {
                return ID.GetHashCode();
            }
        }
        public int SizeX, SizeY;
        public int Stride
        {
            get
            {
                int s = 0;
                if (pixelFormat == PixelFormat.Format8bppIndexed)
                    s = SizeX;
                else
                if (pixelFormat == PixelFormat.Format16bppGrayScale)
                    s = SizeX * 2;
                else
                if (pixelFormat == PixelFormat.Format24bppRgb)
                    s = SizeX * 3;
                else
                    if (pixelFormat == PixelFormat.Format32bppRgb || pixelFormat == PixelFormat.Format32bppArgb)
                    s = SizeX * 4;
                else
                    s = SizeX * 3 * 2;
                return s;
            }
        }
        public int PaddedStride
        {
            get
            {
                return GetStridePadded(Stride);
            }
        }
        public byte[] PaddedBuffer
        {
            get
            {
                return GetPaddedBuffer(Bytes, SizeX, SizeY, Stride, PixelFormat);
            }
        }
        public bool LittleEndian
        {
            get
            {
                return littleEndian;
            }
            set
            {
                littleEndian = value;
            }
        }
        public long Length
        {
            get
            {
                return bytes.Length;
            }
        }
        public int RGBChannelsCount
        {
            get
            {
                if (PixelFormat == PixelFormat.Format24bppRgb || PixelFormat == PixelFormat.Format48bppRgb)
                    return 3;
                else
                if (PixelFormat == PixelFormat.Format8bppIndexed || PixelFormat == PixelFormat.Format16bppGrayScale)
                    return 1;
                else
                    return 4;
            }
        }
        public int BitsPerPixel
        {
            get
            {
                if (PixelFormat == PixelFormat.Format16bppGrayScale || PixelFormat == PixelFormat.Format48bppRgb)
                {
                    return 16;
                }
                else
                    return 8;
            }
        }
        public ZCT Coordinate;
        public PixelFormat PixelFormat
        {
            get
            {
                return pixelFormat;
            }
            set
            {
                pixelFormat = value;
            }
        }
        public byte[] Bytes
        {
            get { return bytes; }
            set
            {
                bytes = value;
            }
        }
        public byte[] PaddedBytes
        {
            get
            {
                return GetPaddedBuffer(bytes, SizeX, SizeY, Stride, PixelFormat);
            }
        }
        /* Converting a byte array to an image. */
        public Image Image
        {
            get
            {
                return GetBitmap(SizeX, SizeY, Stride, PixelFormat, Bytes);
            }
            set
            {
                Bitmap bitmap;
                bitmap = (Bitmap)value;
                if (!LittleEndian)
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                PixelFormat = value.PixelFormat;
                SizeX = value.Width;
                SizeY = value.Height;
                bytes = GetBuffer(bitmap, Stride);
                if (LittleEndian)
                    Array.Reverse(bytes);
                bitmap.Dispose();
                bitmap = null;
                GC.Collect();
            }
        }
        /* Converting a bitmap to a byte array. */
        public Image ImageRGB
        {
            get
            {
                return GetBitmapRGB(SizeX, SizeY, PixelFormat, Bytes);
            }
            set
            {
                Bitmap bitmap;
                bitmap = (Bitmap)value;
                if (!LittleEndian)
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                PixelFormat = value.PixelFormat;
                SizeX = value.Width;
                SizeY = value.Height;
                bytes = GetBuffer(bitmap, Stride);
                if (LittleEndian)
                    Array.Reverse(bytes);
                bitmap.Dispose();
                bitmap = null;
                GC.Collect();
            }
        }
        public IntPtr RGBData
        {
            get
            {
                return GetRGBData(SizeX, SizeY, PixelFormat, bytes);
            }
        }
        public Plane Plane
        {
            get { return plane; }
            set { plane = value; }
        }
        public int PixelFormatSize
        {
            get
            {
                if (pixelFormat == PixelFormat.Format8bppIndexed)
                    return 1;
                else if (pixelFormat == PixelFormat.Format16bppGrayScale)
                    return 2;
                else if (pixelFormat == PixelFormat.Format24bppRgb)
                    return 3;
                else if (pixelFormat == PixelFormat.Format32bppRgb || pixelFormat == PixelFormat.Format32bppArgb)
                    return 4;
                else if (pixelFormat == PixelFormat.Format48bppRgb)
                    return 6;
                throw new InvalidDataException("Bio only supports 8, 16, 24, 32, and 48 bit images.");
            }
        }
        private PixelFormat pixelFormat;
        public Statistics[] Stats
        {
            get { return stats; }
            set { stats = value; }
        }
        Statistics[] stats;
        byte[] bytes;
        string file;
        bool littleEndian = BitConverter.IsLittleEndian;
        Plane plane = null;
        /// It takes a bitmap, switches the red and blue channels if necessary, rotates it 180 degrees,
        /// and then gets the bytes from the bitmap
        /// 
        /// @param Bitmap The bitmap to be converted to a byte array
        /// @param switchRGB If true, the red and blue channels are switched.
        public void SetImage(Bitmap bitmap, bool switchRGB)
        {
            if (switchRGB)
                bitmap = BufferInfo.SwitchRedBlue(bitmap);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            PixelFormat = bitmap.PixelFormat;
            SizeX = bitmap.Width;
            SizeY = bitmap.Height;
            bytes = GetBuffer((Bitmap)bitmap, Stride);
        }
        /// If the stride is not divisible by 4, add 2 to it until it is. If the stride is divisible by
        /// 3 but not 2, add 1 to it until it is divisible by 4. If the stride is divisible by 2 but not
        /// 3, add 3 to it until it is divisible by 4
        /// 
        /// @param stride The stride of the image.
        /// 
        /// @return The stride of the image.
        private static int GetStridePadded(int stride)
        {
            if (stride % 4 == 0)
                return stride;
            int newstride = stride + 2;
            if (stride % 3 == 0 && stride % 2 != 0)
            {
                newstride = stride + 1;
                if (newstride % 4 != 0)
                    newstride = stride + 3;
            }
            if (newstride % 4 != 0)
                return stride + 5;
            return newstride;
        }
        /// It takes a byte array, width, height, stride, and pixel format and returns a new byte array
        /// with the stride padded to a multiple of 4
        /// 
        /// @param bts the byte array of the image
        /// @param w width of the image
        /// @param h height of the image
        /// @param stride The stride of the image.
        /// @param PixelFormat Format16bppRgb555
        /// 
        /// @return A byte array.
        private static byte[] GetPaddedBuffer(byte[] bts, int w, int h, int stride, PixelFormat px)
        {
            if (stride == 0)
                return null;
            int newstride = GetStridePadded(stride);
            if (newstride == stride)
                return bts;
            byte[] newbts = new byte[newstride * h];
            if (px == PixelFormat.Format24bppRgb || px == PixelFormat.Format32bppArgb || px == PixelFormat.Format32bppRgb || px == PixelFormat.Format8bppIndexed)
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        int index = (y * stride) + x;
                        int index2 = (y * newstride) + x;
                        newbts[index2] = bts[index];
                    }
                }
            }
            else
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w * 2; ++x)
                    {
                        int index = (y * stride) + x;
                        int index2 = (y * newstride) + x;
                        newbts[index2] = bts[index];
                    }
                }
            }
            return newbts;
        }
        /// It takes a byte array of RGB48 data, converts it to RGB16, and returns a BufferInfo array of
        /// 3 BufferInfo objects, each containing a Bitmap of the RGB16 data
        /// 
        /// @param file the file name
        /// @param w width of the image
        /// @param h height of the image
        /// @param stride the number of bytes per row
        /// @param bts byte array of the image data
        /// @param ZCT Z, C, T coordinates
        /// @param index the index of the image in the file
        /// @param Plane enum with values XY, XZ, YZ
        /// 
        /// @return A BufferInfo array of 3 elements.
        public static BufferInfo[] RGB48To16(string file, int w, int h, int stride, byte[] bts, ZCT coord, int index, Plane plane)
        {
            BufferInfo[] bfs = new BufferInfo[3];
            Bitmap bmpr = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bmpg = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bmpb = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);

            //creating the bitmapdata and lock bits
            System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
            BitmapData bmdr = bmpr.LockBits(rec, ImageLockMode.ReadWrite, bmpr.PixelFormat);
            BitmapData bmdg = bmpg.LockBits(rec, ImageLockMode.ReadWrite, bmpg.PixelFormat);
            BitmapData bmdb = bmpb.LockBits(rec, ImageLockMode.ReadWrite, bmpb.PixelFormat);
            unsafe
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* rowr = (byte*)bmdr.Scan0 + (y * bmdr.Stride);
                    byte* rowg = (byte*)bmdg.Scan0 + (y * bmdg.Stride);
                    byte* rowb = (byte*)bmdb.Scan0 + (y * bmdb.Stride);
                    int rowRGB = y * stride;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        rowr[index16 + 1] = bts[rowRGB + indexRGB];
                        rowr[index16] = bts[rowRGB + indexRGB + 1];
                        //G
                        rowg[index16 + 1] = bts[rowRGB + indexRGB + 2];
                        rowg[index16] = bts[rowRGB + indexRGB + 3];
                        //B
                        rowb[index16 + 1] = bts[rowRGB + indexRGB + 4];
                        rowb[index16] = bts[rowRGB + indexRGB + 5];

                    }
                }
            }
            bmpr.UnlockBits(bmdr);
            bmpg.UnlockBits(bmdg);
            bmpb.UnlockBits(bmdb);
            bfs[0] = new BufferInfo(file, bmpr, new ZCT(coord.Z, 0, coord.T), index, plane);
            bfs[0].RotateFlip(RotateFlipType.Rotate180FlipNone);
            bfs[1] = new BufferInfo(file, bmpg, new ZCT(coord.Z, 0, coord.T), index + 1, plane);
            bfs[1].RotateFlip(RotateFlipType.Rotate180FlipNone);
            bfs[2] = new BufferInfo(file, bmpb, new ZCT(coord.Z, 0, coord.T), index + 2, plane);
            bfs[2].RotateFlip(RotateFlipType.Rotate180FlipNone);
            return bfs;
        }
        /// It takes a list of BufferInfo objects, each of which contains a byte array of 16-bit pixel
        /// data, and returns a single BufferInfo object containing a byte array of 48-bit pixel data
        /// 
        /// @param bfs an array of BufferInfo objects.
        /// 
        /// @return A BufferInfo object.
        public static BufferInfo RGB16To48(BufferInfo[] bfs)
        {
            //If this is a 2 channel image we fill the last channel with black.
            if (bfs[2] == null)
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 2 * 3);
                    int row16 = y * (bfs[0].SizeX * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bt[rowRGB + indexRGB] = 0;
                        bt[rowRGB + indexRGB + 1] = 0;
                        //G
                        bt[rowRGB + indexRGB + 2] = bfs[1].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 3] = bfs[1].Bytes[row16 + index16 + 1];
                        //B
                        bt[rowRGB + indexRGB + 4] = bfs[0].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 5] = bfs[0].Bytes[row16 + index16 + 1];
                    }
                }
                BufferInfo bf = new BufferInfo(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, bt, bfs[0].Coordinate, 0, bfs[0].Plane);
                return bf;
            }
            else
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 2 * 3);
                    int row16 = y * (bfs[0].SizeX * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bt[rowRGB + indexRGB] = bfs[2].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 1] = bfs[2].Bytes[row16 + index16 + 1];
                        //G
                        bt[rowRGB + indexRGB + 2] = bfs[1].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 3] = bfs[1].Bytes[row16 + index16 + 1];
                        //B
                        bt[rowRGB + indexRGB + 4] = bfs[0].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 5] = bfs[0].Bytes[row16 + index16 + 1];
                    }
                }
                BufferInfo bf = new BufferInfo(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, bt, bfs[0].Coordinate, 0, bfs[0].Plane);
                return bf;
            }
        }
        /// It takes a 16 bit image and converts it to a 48 bit image by copying the red value to the
        /// green and blue values
        /// 
        /// @param BufferInfo This is a class that contains the following properties:
        /// 
        /// @return A BufferInfo object.
        public static BufferInfo RGB16To48(BufferInfo bfs)
        {
            byte[] bt = new byte[bfs.SizeY * (bfs.SizeX * 2 * 3)];
            //iterating through all the pixels in y direction
            for (int y = 0; y < bfs.SizeY; y++)
            {
                //getting the pixels of current row
                int rowRGB = y * (bfs.SizeX * 2 * 3);
                int row16 = y * (bfs.SizeX * 2);
                //iterating through all the pixels in x direction
                for (int x = 0; x < bfs.SizeX; x++)
                {
                    int indexRGB = x * 6;
                    int index16 = x * 2;
                    //R
                    bt[rowRGB + indexRGB] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 1] = bfs.Bytes[row16 + index16 + 1];
                    //G
                    bt[rowRGB + indexRGB + 2] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 3] = bfs.Bytes[row16 + index16 + 1];
                    //B
                    bt[rowRGB + indexRGB + 4] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 5] = bfs.Bytes[row16 + index16 + 1];
                }
            }
            BufferInfo bf = new BufferInfo(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format48bppRgb, bt, bfs.Coordinate, 0, bfs.Plane);
            return bf;
        }
        /// It takes a buffer of RGB data, and returns a bitmap
        /// 
        /// @param bfs BufferInfo[] - this is an array of BufferInfo objects.  Each BufferInfo object
        /// represents a single image buffer.  In this case, we're only using one image buffer, but you
        /// can use multiple image buffers to create a single image.  For example, you can use a red
        /// image
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A Bitmap object.
        public static Bitmap GetRGBBitmap(BufferInfo[] bfs, IntRange rr, IntRange rg, IntRange rb)
        {
            int stride;
            if (bfs[0].BitsPerPixel > 8)
                stride = bfs[0].SizeX * 6;
            else
                stride = bfs[0].SizeX * 3;
            int w = bfs[0].SizeX;
            int h = bfs[0].SizeY;
            byte[] bt = new byte[h * w * 3];
            if (bfs[0].BitsPerPixel == 8)
            {
                BufferInfo bf = BufferInfo.RGB8To24(bfs);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    int rowRGB = y * stride;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 3;
                        float ri = ((float)bf.Bytes[rowRGB + indexRGB] - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)bf.Bytes[rowRGB + indexRGB + 1] - rg.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rg.Max;
                        float bi = ((float)bf.Bytes[rowRGB + indexRGB + 2] - rb.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rb.Max;
                        bt[rowRGB + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[rowRGB + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[rowRGB + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
                bf.Dispose();
            }
            else
            {
                BufferInfo bf = BufferInfo.RGB16To48(bfs);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * w * 6;
                    int row = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int indexRGBA = x * 3;
                        float ri = ((float)BitConverter.ToUInt16(bf.Bytes, rowRGB + indexRGB) - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)BitConverter.ToUInt16(bf.Bytes, rowRGB + indexRGB + 2) - rg.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rg.Max;
                        float bi = ((float)BitConverter.ToUInt16(bf.Bytes, rowRGB + indexRGB + 4) - rb.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rb.Max;
                        bt[row + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[row + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[row + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
                bf.Dispose();
            }
            return GetBitmap(w, h, w * 3, PixelFormat.Format24bppRgb, bt);
        }
        /// It takes a buffer, a color, and a range, and returns a bitmap with the color applied to the
        /// buffer
        /// 
        /// @param BufferInfo This is a class that contains the following properties:
        /// @param IntRange This is a struct that contains a min and max value.
        /// @param col The color to use for the emission.
        /// 
        /// @return A bitmap.
        public static Bitmap GetEmissionBitmap(BufferInfo bfs, IntRange rr, System.Drawing.Color col)
        {
            int stride;
            if (bfs.BitsPerPixel > 8)
                stride = bfs.SizeX * 3 * 2;
            else
                stride = bfs.SizeX * 3;
            float r = (col.R / 255f);
            float g = (col.G / 255f);
            float b = (col.B / 255f);

            int w = bfs.SizeX;
            int h = bfs.SizeY;
            byte[] bts = new byte[h * stride];

            if (bfs.BitsPerPixel > 8)
            {
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (w * 2 * 3);
                    int row16 = y * (w * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;

                        float rf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * r;
                        float gf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * g;
                        float bf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * b;
                        ushort rs = (ushort)(rf * ushort.MaxValue);
                        ushort gs = (ushort)(gf * ushort.MaxValue);
                        ushort bs = (ushort)(bf * ushort.MaxValue);
                        byte[] rbb = BitConverter.GetBytes(rs);
                        byte[] gbb = BitConverter.GetBytes(gs);
                        byte[] bbb = BitConverter.GetBytes(bs);
                        //R
                        bts[rowRGB + indexRGB] = rbb[0];
                        bts[rowRGB + indexRGB + 1] = rbb[1];
                        //G
                        bts[rowRGB + indexRGB + 2] = gbb[0];
                        bts[rowRGB + indexRGB + 3] = gbb[1];
                        //B
                        bts[rowRGB + indexRGB + 4] = bbb[0];
                        bts[rowRGB + indexRGB + 5] = bbb[1];
                    }
                }
            }
            else
            {
                for (int y = 0; y < bfs.SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs.SizeX * 3);
                    int row8 = y * (bfs.SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs.SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;

                        float rf = (bfs.Bytes[row8 + index8] / 255f) * r;
                        float gf = (bfs.Bytes[row8 + index8] / 255f) * g;
                        float bf = (bfs.Bytes[row8 + index8] / 255f) * b;
                        byte rs = (byte)(rf * byte.MaxValue);
                        byte gs = (byte)(gf * byte.MaxValue);
                        byte bs = (byte)(bf * byte.MaxValue);
                        //R
                        bts[rowRGB + indexRGB] = rs;
                        //G
                        bts[rowRGB + indexRGB + 1] = gs;
                        //B
                        bts[rowRGB + indexRGB + 2] = bs;
                    }
                }
            }
            byte[] bt = new byte[h * (w * 3)];
            if (bfs.BitsPerPixel == 8)
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 3;
                        float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)bts[rowRGB + indexRGB + 1] - rr.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rr.Max;
                        float bi = ((float)bts[rowRGB + indexRGB + 2] - rr.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rr.Max;
                        bt[rowRGB + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[rowRGB + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[rowRGB + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
            }
            else
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * w * 3 * 2;
                    int row = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int indexRGBA = x * 3;
                        float ri = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) - rr.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rr.Max;
                        float bi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) - rr.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rr.Max;
                        bt[row + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[row + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[row + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
            }
            bts = null;
            Bitmap bmp;
            if (bfs.BitsPerPixel > 8)
                return GetBitmap(w, h, w * 3, PixelFormat.Format24bppRgb, bt);
            else
                return GetBitmap(w, h, w * 3 * 2, PixelFormat.Format24bppRgb, bt);
        }
        /// It takes a list of buffer info objects and a list of channel objects and returns a bitmap of
        /// the emission data
        /// 
        /// @param bfs an array of BufferInfo objects, each of which contains a buffer of data (a
        /// float[]) and the size of the buffer (SizeX and SizeY).
        /// @param chans an array of Channel objects, which are defined as:
        /// 
        /// @return A bitmap of the emission image.
        public static Bitmap GetEmissionBitmap(BufferInfo[] bfs, Channel[] chans)
        {
            Bitmap bm = new Bitmap(bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb);
            Merge m = new Merge(bm);
            for (int i = 0; i < chans.Length; i++)
            {
                m.OverlayImage = bm;
                Bitmap b = GetEmissionBitmap(bfs[i], chans[i].range[0], chans[i].EmissionColor);
                bm = m.Apply(b);
            }
            return bm;
        }
        /// It takes an array of BufferInfo objects, each of which contains a byte array of 8-bit pixel
        /// data, and returns a single BufferInfo object containing a byte array of 24-bit pixel data
        /// 
        /// @param bfs an array of BufferInfo objects.
        /// 
        /// @return A BufferInfo object.
        public static BufferInfo RGB8To24(BufferInfo[] bfs)
        {
            //If this is a 2 channel image we fill the last channel with black.
            if (bfs[2] == null)
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 3);
                    int row8 = y * (bfs[0].SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        //R
                        bt[rowRGB + indexRGB] = 0;
                        //G
                        bt[rowRGB + indexRGB + 1] = bfs[1].Bytes[row8 + index8];
                        //B
                        bt[rowRGB + indexRGB + 2] = bfs[0].Bytes[row8 + index8];
                    }
                }
                return new BufferInfo(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bt, bfs[0].Coordinate, 0);
            }
            else
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 3);
                    int row8 = y * (bfs[0].SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        //R
                        bt[rowRGB + indexRGB] = bfs[2].Bytes[row8 + index8];
                        //G
                        bt[rowRGB + indexRGB + 1] = bfs[1].Bytes[row8 + index8];
                        //B
                        bt[rowRGB + indexRGB + 2] = bfs[0].Bytes[row8 + index8];
                    }
                }
                return new BufferInfo(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bt, bfs[0].Coordinate, 0);
            }
        }
        /// It takes a buffer of bytes, and converts it from 8-bit grayscale to 24-bit RGB
        /// 
        /// @param BufferInfo This is a class that contains the following properties:
        /// 
        /// @return A BufferInfo object.
        public static BufferInfo RGB8To24(BufferInfo bfs)
        {
            byte[] bt = new byte[bfs.SizeY * (bfs.SizeX * 3)];
            //iterating through all the pixels in y direction
            for (int y = 0; y < bfs.SizeY; y++)
            {
                //getting the pixels of current row
                int rowRGB = y * (bfs.SizeX * 3);
                int row8 = y * (bfs.SizeX);
                //iterating through all the pixels in x direction
                for (int x = 0; x < bfs.SizeX; x++)
                {
                    int indexRGB = x * 3;
                    int index8 = x;
                    //R
                    bt[rowRGB + indexRGB] = bfs.Bytes[row8 + index8];
                    //G
                    bt[rowRGB + indexRGB + 1] = bfs.Bytes[row8 + index8];
                    //B
                    bt[rowRGB + indexRGB + 2] = bfs.Bytes[row8 + index8];
                }
            }
            BufferInfo bf = new BufferInfo(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format24bppRgb, bt, bfs.Coordinate, 0);
            return bf;
        }
        /// It takes a 24-bit RGB image and returns an array of 8-bit images, one for each channel
        /// 
        /// @param Bitmap The image to be converted
        /// 
        /// @return A Bitmap array.
        public static Bitmap[] RGB24To8(Bitmap info)
        {
            Bitmap[] bfs = new Bitmap[3];
            ExtractChannel cr = new ExtractChannel((short)0);
            ExtractChannel cg = new ExtractChannel((short)1);
            ExtractChannel cb = new ExtractChannel((short)2);
            bfs[0] = cr.Apply(info);
            bfs[1] = cg.Apply(info);
            bfs[2] = cb.Apply(info);
            cr = null;
            cg = null;
            cb = null;
            return bfs;
        }
        /// If the stride is not a multiple of 4, then pad the buffer with zeros and create a new bitmap
        /// with the padded buffer
        /// 
        /// @param w width of the image
        /// @param h height of the image
        /// @param stride The stride of the image.
        /// @param PixelFormat Format24bppRgb
        /// @param bts the byte array of the image
        /// 
        /// @return A Bitmap object.
        public static unsafe Bitmap GetBitmap(int w, int h, int stride, PixelFormat px, byte[] bts)
        {
            fixed (byte* numPtr1 = bts)
            {
                if (stride % 4 == 0)
                {
                    return new Bitmap(w, h, stride, px, new IntPtr((void*)numPtr1));
                }
                int newstride = GetStridePadded(stride);
                byte[] newbts = GetPaddedBuffer(bts, w, h, stride, px);
                fixed (byte* numPtr2 = newbts)
                {
                    return new Bitmap(w, h, newstride, px, new IntPtr((void*)numPtr2));
                }
            }
        }
        /// It takes a byte array of RGB or RGBA data and converts it to a Bitmap
        /// 
        /// @param w width of the image
        /// @param h height of the image
        /// @param PixelFormat The pixel format of the image.
        /// @param bts the byte array of the image
        /// 
        /// @return A Bitmap object.
        public static unsafe Bitmap GetBitmapRGB(int w, int h, PixelFormat px, byte[] bts)
        {
            if (px == PixelFormat.Format32bppArgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 4;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 4;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = bts[rowRGB + indexRGB + 3];//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else if (px == PixelFormat.Format24bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = byte.MaxValue;//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 6;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            int b = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            int g = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) / 255);
                            int r = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            byte b = bts[rowRGB + indexRGB];
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 2;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            ushort b = (ushort)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }

            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        /// It takes a byte array of RGB data and returns a pointer to a 32 bit ARGB image
        /// 
        /// @param w width of the image
        /// @param h height of the image
        /// @param PixelFormat The pixel format of the image.
        /// @param bts the byte array of the image
        /// 
        /// @return A pointer to the first byte of the image data.
        public static unsafe IntPtr GetRGBData(int w, int h, PixelFormat px, byte[] bts)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = byte.MaxValue;//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 6;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            int b = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            int g = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) / 255);
                            int r = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            byte b = bts[rowRGB + indexRGB];
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 2;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            ushort b = (ushort)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        /// It takes a byte array of RGB data and returns a pointer to a 32 bit ARGB image
        /// 
        /// @param w width of the image
        /// @param h height of the image
        /// @param PixelFormat The pixel format of the image.
        /// @param bts the byte array of the image
        /// 
        /// @return A pointer to the first byte of the image data.
        public static unsafe byte[] GetRGBBuffer(int w, int h, PixelFormat px, byte[] bts)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                BufferInfo bmp = new BufferInfo(w, h, PixelFormat.Format32bppArgb);
                // The destination array will be larger due to the added alpha channel.
                int destLength = (bts.Length / 3) * 4; // For every 3 bytes in the source, there will be 4 bytes in the destination.
                byte[] dest = new byte[destLength];

                for (int i = 0, j = 0; i < bts.Length; i += 3, j += 4)
                {
                    // Copy RGB components directly.
                    dest[j] = bts[i];     // Red
                    dest[j + 1] = bts[i + 1]; // Green
                    dest[j + 2] = bts[i + 2]; // Blue
                    dest[j + 3] = 255;       // Alpha (fully opaque)
                }
                return bmp.Bytes;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                // Initialize the destination array.
                int pixelCount = bts.Length / 6; // 6 bytes per pixel in source, 4 bytes per pixel in destination.
                byte[] dest = new byte[pixelCount * 4];

                for (int i = 0, j = 0; i < bts.Length; i += 6, j += 4)
                {
                    // Use BitConverter to convert two bytes to a single ushort, then downsample by shifting.
                    ushort red = BitConverter.ToUInt16(bts, i);
                    ushort green = BitConverter.ToUInt16(bts, i + 2);
                    ushort blue = BitConverter.ToUInt16(bts, i + 4);

                    // Downsample to 8 bits by shifting right 8 bits.
                    // This effectively takes the higher 8 bits of each 16-bit color value.
                    dest[j] = (byte)(red >> 8);       // Red
                    dest[j + 1] = (byte)(green >> 8); // Green
                    dest[j + 2] = (byte)(blue >> 8);  // Blue
                    dest[j + 3] = 255;                // Alpha (fully opaque)
                }
                return dest;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                int pixelCount = bts.Length; // 1 byte per pixel in source, 4 bytes per pixel in destination.
                byte[] dest = new byte[pixelCount * 4];

                for (int i = 0, j = 0; i < bts.Length; i++, j += 4)
                {
                    byte gray = bts[i];

                    // Set RGB channels to the grayscale value and Alpha to 255 (fully opaque).
                    dest[j] = gray;     // Red
                    dest[j + 1] = gray; // Green
                    dest[j + 2] = gray; // Blue
                    dest[j + 3] = 255;  // Alpha
                }

                return dest;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                int pixelCount = bts.Length / 2; // 2 bytes per pixel in source, 4 bytes per pixel in destination.
                byte[] dest = new byte[pixelCount * 4];

                for (int i = 0, j = 0; i < bts.Length; i += 2, j += 4)
                {
                    // Convert two bytes to a single ushort to get the 16-bit grayscale value.
                    ushort gray = BitConverter.ToUInt16(bts, i);

                    // Downsample to 8 bits by taking the higher byte.
                    // This is a simple method and may not preserve the full dynamic range.
                    byte gray8bit = (byte)(gray >> 8);

                    // Set RGB channels to the grayscale value and Alpha to 255 (fully opaque).
                    dest[j] = gray8bit;     // Red
                    dest[j + 1] = gray8bit; // Green
                    dest[j + 2] = gray8bit; // Blue
                    dest[j + 3] = 255;      // Alpha
                }

                return dest;
            }
            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        /// It takes a byte array of image data, and returns a bitmap
        /// 
        /// @param w width of the image
        /// @param h height of the image
        /// @param stride the number of bytes per row of the image.
        /// @param PixelFormat The pixel format of the image.
        /// @param bts the byte array of the image
        /// @param IntRange This is a struct that contains a min and max value.
        /// @param IntRange This is a struct that contains a min and max value.
        /// @param IntRange This is a struct that contains a min and max value.
        /// 
        /// @return A Bitmap object.
        public static Bitmap GetFiltered(int w, int h, int stride, PixelFormat px, byte[] bts, IntRange rr, IntRange rg, IntRange rb)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 3;
                            int indexRGBA = x * 3;
                            row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                            row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                            row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                            float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)bts[rowRGB + indexRGB + 1] - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)bts[rowRGB + indexRGB + 2] - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format32bppArgb)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppRgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 4;
                            int indexRGBA = x * 4;
                            row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                            row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                            row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                            float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)bts[rowRGB + indexRGB + 1] - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)bts[rowRGB + indexRGB + 2] - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            float ri = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }

                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            float ri = (float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min;
                            if (ri < 0)
                                ri = 0;
                            ri = ri / rr.Max;
                            int b = (int)(ri * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the bitmapdata and lock bits
                System.Drawing.Rectangle rec = new System.Drawing.Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            float ri = (float)bts[rowRGB + indexRGB] - rr.Min;
                            if (ri < 0)
                                ri = 0;
                            ri = ri / rr.Max;
                            int b = (int)(ri * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }

                bmp.UnlockBits(bmd);
                return bmp;
            }
            throw new InvalidDataException("Bio supports only 8, 16 24, 32, 48 bit images.");
        }
        /// It takes a range of red, green, and blue values and returns a bitmap that contains only the
        /// pixels that fall within the range
        /// 
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A bitmap.
        public Bitmap GetFiltered(IntRange rr, IntRange rg, IntRange rb)
        {
            return BufferInfo.GetFiltered(SizeX, SizeY, Stride, PixelFormat, Bytes, rr, rg, rb);
        }
        /// It takes a rectangle and crops the image to that rectangle
        /// 
        /// @param Rectangle The rectangle to crop to.
        public void Crop(Rectangle r)
        {
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                if (RGBChannelsCount == 1)
                {
                    byte[] bts = null;
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    bytes = bts;
                }
                else
                {
                    byte[] bts = null;
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    bytes = bts;
                }
            }
            else
            {
                Image = ((Bitmap)Image).Clone(r, PixelFormat);
            }
            SizeX = r.Width;
            SizeY = r.Height;
        }
        /// It creates a new Bitmap object from the original image, but only using the bytes that are
        /// within the rectangle
        /// 
        /// @param Rectangle The rectangle to crop the image to.
        /// 
        /// @return A Bitmap object.
        public Bitmap GetCropBitmap(Rectangle r)
        {
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                byte[] bts = null;
                if (RGBChannelsCount == 1)
                {
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x) * RGBChannelsCount;
                            int indexold = (((y + r.Y) * strideold + (x + (r.X * bytesPer))) * RGBChannelsCount);// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    return new Bitmap(r.Width, r.Height, stridenew, PixelFormat.Format16bppGrayScale, Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0));
                }
                else
                {
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    //bytes = bts;
                    return new Bitmap(r.Width, r.Height, stridenew, PixelFormat.Format48bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0));
                }
            }
            else
            {
                return ((Bitmap)Image).Clone(r, PixelFormat);
            }

        }
        /// It takes a rectangle and returns a BufferInfo object that contains the cropped image
        /// 
        /// @param Rectangle The rectangle to crop the image to.
        /// 
        /// @return A BufferInfo object.
        public BufferInfo GetCropBuffer(Rectangle r)
        {
            BufferInfo inf = null;
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                byte[] bts = null;
                if (RGBChannelsCount == 1)
                {
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x) * RGBChannelsCount;
                            int indexold = (((y + r.Y) * strideold + (x + (r.X * bytesPer))) * RGBChannelsCount);// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    BufferInfo bf = new BufferInfo(r.Width, r.Height, PixelFormat.Format16bppGrayScale, bts, Coordinate, ID);
                    return bf;
                }
                else
                {
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    BufferInfo bf = new BufferInfo(r.Width, r.Height, PixelFormat.Format48bppRgb, bts, Coordinate, ID);
                    return bf;
                }
            }
            else
            {
                Bitmap bmp = ((Bitmap)Image).Clone(r, PixelFormat);
                return new BufferInfo(ID, bmp, Coordinate, 0);
            }
        }
        /* Creating a new BufferInfo object. */
        public BufferInfo(int w, int h, PixelFormat px)
        {
            ID = CreateID("", 0);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = new ZCT();
            Bytes = new byte[Stride * h];
        }
        /* Creating a new BufferInfo object. */
        public BufferInfo(string file, int w, int h, PixelFormat px, byte[] bts, ZCT coord, int index)
        {
            ID = CreateID(file, index);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            Bytes = bts;
            if (isRGB)
                SwitchRedBlue();
        }
        /* Creating a new BufferInfo object. */
        public BufferInfo(string file, int w, int h, PixelFormat px, byte[] bts, ZCT coord, int index, Plane plane)
        {
            ID = CreateID(file, index);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            Bytes = bts;
            if (isRGB)
                SwitchRedBlue();
            Plane = plane;
        }
        /* Creating a new BufferInfo object. */
        public BufferInfo(string file, int w, int h, PixelFormat px, byte[] byts, ZCT coord, int index, bool littleEndian = true, bool interleaved = true)
        {
            ID = CreateID(file, index);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            Bytes = byts;
            if (!interleaved)
            {
                byte[] bts = new byte[Length];
                int strplane = 0;
                if (BitsPerPixel > 8)
                    strplane = w * 2;
                else
                    strplane = w;
                if (RGBChannelsCount == 1)
                {
                    for (int y = 0; y < h; y++)
                    {
                        int x = 0;
                        int str1 = Stride * y;
                        int str2 = strplane * y;
                        for (int st = 0; st < strplane; st++)
                        {
                            bts[str1 + x] = bytes[str2 + st];
                            x++;
                        }
                    }
                }
                else
                {
                    if (BitsPerPixel > 8)
                    {
                        int ind = strplane * h;
                        int ind2 = strplane * h * 2;
                        for (int y = 0; y < h; y++)
                        {
                            int i = 0;
                            int x = 0;
                            int str1 = Stride * y;
                            int str2 = strplane * y;
                            for (int st = 0; st < strplane; st += 2)
                            {
                                bts[str1 + x + 0] = bytes[str2 + st];
                                bts[str1 + x + 1] = bytes[str2 + st + 1];
                                bts[str1 + x + 2] = bytes[ind + str2 + st];
                                bts[str1 + x + 3] = bytes[ind + str2 + st + 1];
                                bts[str1 + x + 4] = bytes[ind2 + str2 + st];
                                bts[str1 + x + 5] = bytes[ind2 + str2 + st + 1];
                                if (i == 1)
                                {
                                    i = 0; x += 6;
                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        int ind = strplane * h;
                        int indb = ind * 2;
                        for (int y = 0; y < h; y++)
                        {
                            int x = 0;
                            int str1 = Stride * y;
                            int str2 = strplane * y;
                            for (int st = 0; st < strplane; st++)
                            {
                                bts[str1 + x + 2] = bytes[str2 + st];
                                bts[str1 + x + 1] = bytes[ind + str2 + st];
                                bts[str1 + x] = bytes[indb + str2 + st];
                                x += 3;
                            }
                        }
                    }
                }
                bytes = bts;
            }
            if (!littleEndian)
            {
                Array.Reverse(Bytes);
                RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            if (isRGB)
                SwitchRedBlue();
        }
        /* Creating a new BufferInfo object. */
        public BufferInfo(string file, int w, int h, PixelFormat px, byte[] byts, ZCT coord, int index, Plane plane, bool littleEndian = true, bool interleaved = true)
        {
            ID = CreateID(file, index);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            Bytes = byts;
            if (!interleaved)
            {
                byte[] bts = new byte[Length];
                int strplane = 0;
                if (BitsPerPixel > 8)
                    strplane = w * 2;
                else
                    strplane = w;
                if (RGBChannelsCount == 1)
                {
                    for (int y = 0; y < h; y++)
                    {
                        int x = 0;
                        int str1 = Stride * y;
                        int str2 = strplane * y;
                        for (int st = 0; st < strplane; st++)
                        {
                            bts[str1 + x] = bytes[str2 + st];
                            x++;
                        }
                    }
                }
                else
                {
                    if (BitsPerPixel > 8)
                    {
                        int ind = strplane * h;
                        int ind2 = strplane * h * 2;
                        for (int y = 0; y < h; y++)
                        {
                            int i = 0;
                            int x = 0;
                            int str1 = Stride * y;
                            int str2 = strplane * y;
                            for (int st = 0; st < strplane; st += 2)
                            {
                                bts[str1 + x + 0] = bytes[str2 + st];
                                bts[str1 + x + 1] = bytes[str2 + st + 1];
                                bts[str1 + x + 2] = bytes[ind + str2 + st];
                                bts[str1 + x + 3] = bytes[ind + str2 + st + 1];
                                bts[str1 + x + 4] = bytes[ind2 + str2 + st];
                                bts[str1 + x + 5] = bytes[ind2 + str2 + st + 1];
                                if (i == 1)
                                {
                                    i = 0; x += 6;
                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        int ind = strplane * h;
                        int indb = ind * 2;
                        for (int y = 0; y < h; y++)
                        {
                            int x = 0;
                            int str1 = Stride * y;
                            int str2 = strplane * y;
                            for (int st = 0; st < strplane; st++)
                            {
                                bts[str1 + x + 2] = bytes[str2 + st];
                                bts[str1 + x + 1] = bytes[ind + str2 + st];
                                bts[str1 + x] = bytes[indb + str2 + st];
                                x += 3;
                            }
                        }
                    }
                }
                bytes = bts;
            }
            if (!littleEndian)
            {
                Array.Reverse(Bytes);
                RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            if (isRGB)
                SwitchRedBlue();
            Plane = plane;
        }
        public BufferInfo(string file, Image im, ZCT coord, int index)
        {
            ID = CreateID(file, index);
            SizeX = im.Width;
            SizeY = im.Height;
            pixelFormat = im.PixelFormat;
            Coordinate = coord;
            if (isRGB)
                SwitchRedBlue();
            Image = im;
        }
        public BufferInfo(string file, Image im, ZCT coord, int index, Plane pl)
        {
            ID = CreateID(file, index);
            SizeX = im.Width;
            SizeY = im.Height;
            pixelFormat = im.PixelFormat;
            Coordinate = coord;
            if (isRGB)
                SwitchRedBlue();
            Image = im;
            Plane = pl;
        }
        public BufferInfo(int w, int h, PixelFormat px, byte[] bts, ZCT coord, string id)
        {
            ID = id;
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            if (isRGB)
                SwitchRedBlue();
            Bytes = bts;
        }
        /// It takes a Bitmap image, extracts the red and blue channels, replaces the red channel with
        /// the blue channel and the blue channel with the red channel, and returns the modified image
        /// 
        /// @param Bitmap The image to be processed
        /// 
        /// @return The image is being returned.
        public static Bitmap SwitchRedBlue(Bitmap image)
        {
            ExtractChannel cr = new ExtractChannel(AForge.Imaging.RGB.R);
            ExtractChannel cb = new ExtractChannel(AForge.Imaging.RGB.B);
            // apply the filter
            Bitmap rImage = cr.Apply(image);
            Bitmap bImage = cb.Apply(image);

            ReplaceChannel replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, bImage);
            replaceRFilter.ApplyInPlace(image);

            ReplaceChannel replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, rImage);
            replaceBFilter.ApplyInPlace(image);
            rImage.Dispose();
            bImage.Dispose();
            return image;
        }
        /// It switches the red and blue channels of the image
        /// 
        /// @return a byte array.
        public void SwitchRedBlue()
        {
            if (PixelFormat == PixelFormat.Format8bppIndexed || PixelFormat == PixelFormat.Format16bppGrayScale || Bytes == null)
                return;
            //BufferInfo bf = new BufferInfo(SizeX, SizeY,PixelFormat, bytes, Coordinate, ID);
            if (PixelFormat == PixelFormat.Format24bppRgb)
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < Stride; x += 3)
                    {
                        int i = y * Stride + x;
                        byte bb = bytes[i + 2];
                        bytes[i + 2] = bytes[i];
                        bytes[i] = bb;
                    }
                }
            else
            if (PixelFormat == PixelFormat.Format32bppArgb || PixelFormat == PixelFormat.Format32bppRgb)
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < Stride; x += 4)
                    {
                        int i = y * Stride + x;
                        byte bb = bytes[i + 2];
                        bytes[i + 2] = bytes[i];
                        bytes[i] = bb;
                    }
                }
            else
            if (PixelFormat == PixelFormat.Format48bppRgb)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (Stride);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < Stride; x += 6)
                    {
                        int indexRGB = x;
                        byte b1 = bytes[rowRGB + indexRGB];
                        byte b2 = bytes[rowRGB + indexRGB + 1];
                        //B
                        bytes[rowRGB + indexRGB] = bytes[rowRGB + indexRGB + 4];
                        bytes[rowRGB + indexRGB + 1] = bytes[rowRGB + indexRGB + 5];
                        //R
                        bytes[rowRGB + indexRGB + 4] = b1;
                        bytes[rowRGB + indexRGB + 5] = b2;
                    }
                }
            }
        }
        /// It takes a buffer, copies it, flips it, rotates it, and then copies the bytes from the
        /// bitmap to a byte array
        /// 
        /// @param littleEndian true if the image is in little endian format, false if it's in big
        /// endian format.
        /// 
        /// @return A byte array of the image data.
        public byte[] GetSaveBytes(bool littleEndian)
        {
            BufferInfo bf = this.Copy();
            if (!littleEndian)
            {
                bf.SwitchRedBlue();
                bf.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            Bitmap bitmap = (Bitmap)bf.Image;
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, SizeX, SizeY), ImageLockMode.ReadWrite, PixelFormat);
            IntPtr ptr = data.Scan0;
            int length = this.bytes.Length;
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            if (!littleEndian)
                Array.Reverse(bytes);
            bitmap.UnlockBits(data);
            bitmap.Dispose();
            return bytes;
        }
        /// It takes a bitmap, locks it, copies the bytes into a byte array, reverses the byte array, and
        /// then unlocks the bitmap
        /// 
        /// @param Bitmap The bitmap to convert to a byte array.
        /// @param stride The stride is the width of a single row of pixels (a scan line), rounded up to
        /// a four-byte boundary. If the stride is positive, the bitmap is top-down. If the stride is
        /// negative, the bitmap is bottom-up. (In Windows GDI, the
        /// 
        /// @return A byte array of the image data.
        public static byte[] GetBuffer(Bitmap bmp, int stride)
        {
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            IntPtr ptr = data.Scan0;
            int length = data.Stride * bmp.Height;
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            Array.Reverse(bytes);
            bmp.UnlockBits(data);

            return bytes;
        }
        /// "If the image is 16bpp, convert it to 8bpp, then draw it to a new 24bpp image."
        /// 
        /// The reason for this is that the AForge library doesn't support 16bpp images
        /// 
        /// @param Bitmap The bitmap to convert
        /// 
        /// @return A Bitmap object.
        public static Bitmap To24Bit(Bitmap b)
        {
            Bitmap bm = new Bitmap(b.Width, b.Height, PixelFormat.Format24bppRgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
            if (b.PixelFormat == PixelFormat.Format16bppGrayScale || b.PixelFormat == PixelFormat.Format48bppRgb)
            {
                g.DrawImage(AForge.Imaging.Image.Convert16bppTo8bpp(b), 0, 0);
            }
            else
            {
                g.DrawImage(b, 0, 0);
            }
            g.Dispose();
            return bm;
        }
        /// If the image is 16 bit grayscale, convert it to 8 bit grayscale, then convert it to 32 bit
        /// ARGB
        /// 
        /// @param Bitmap The bitmap to convert
        /// 
        /// @return A Bitmap
        public static Bitmap To32Bit(Bitmap b)
        {
            Bitmap bm = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
            if (b.PixelFormat == PixelFormat.Format16bppGrayScale || b.PixelFormat == PixelFormat.Format48bppRgb)
            {
                bm = AForge.Imaging.Image.Convert16bppTo8bpp(b);
            }
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
            g.DrawImage(b, 0, 0);
            return bm;
        }
        /// It creates a new bitmap with the same dimensions as the original, but with a 32 bit pixel
        /// format, then it draws the original image onto the new bitmap
        public void RGBTo32Bit()
        {
            Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Format32bppArgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
            g.DrawImage((Bitmap)Image, 0, 0);
            Image = bm;
        }
        /// It takes an image, extracts the two channels you want to switch, replaces the channels in
        /// the original image, and returns the original image with the channels switched
        /// 
        /// @param Bitmap The image to be processed
        /// @param c1 The channel to be replaced
        /// @param c2 The channel to be replaced
        /// 
        /// @return The image is being returned.
        public static Bitmap SwitchChannels(Bitmap image, int c1, int c2)
        {
            ExtractChannel cr = new ExtractChannel((short)c1);
            ExtractChannel cb = new ExtractChannel((short)c2);
            // apply the filter
            Bitmap rImage = cr.Apply(image);
            Bitmap bImage = cb.Apply(image);
            ReplaceChannel replaceRFilter = new ReplaceChannel((short)c1, bImage);
            replaceRFilter.ApplyInPlace(image);
            ReplaceChannel replaceBFilter = new ReplaceChannel((short)c2, rImage);
            replaceBFilter.ApplyInPlace(image);
            rImage.Dispose();
            bImage.Dispose();
            return image;
        }
        public byte[] RGBBytes => GetRGBBuffer(SizeX, SizeY, PixelFormat, Bytes);
        /// It creates a new byte array, copies the bytes from the original array into the new array, and
        /// then creates a new BufferInfo object with the new byte array
        /// 
        /// @return A new BufferInfo object.
        public BufferInfo Copy()
        {
            byte[] bt = new byte[Bytes.Length];
            for (int i = 0; i < bt.Length; i++)
            {
                bt[i] = bytes[i];
            }
            BufferInfo bf = new BufferInfo(SizeX, SizeY, PixelFormat, bt, Coordinate, ID);
            bf.plane = Plane;
            return bf;
        }
        /// It creates a new BufferInfo object, copies the values of the current object into the new
        /// object, and returns the new object
        /// 
        /// @return A new BufferInfo object.
        public BufferInfo CopyInfo()
        {
            BufferInfo bf = new BufferInfo(SizeX, SizeY, PixelFormat, null, Coordinate, ID);
            bf.bytes = new byte[bf.Stride * bf.SizeY];
            bf.plane = Plane;
            return bf;
        }
        /// Convert the 16 bit image to 8 bit, rotate it 180 degrees, and set the image to the new 8 bit
        /// image
        public void To8Bit()
        {
            Bitmap bm = AForge.Imaging.Image.Convert16bppTo8bpp((Bitmap)Image);
            bm.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Image = bm;
        }
        /// Convert the image to 16 bit
        public void To16Bit()
        {
            Bitmap bm = AForge.Imaging.Image.Convert8bppTo16bpp((Bitmap)Image);
            Image = bm;
        }

        /// It converts the image to RGB format
        /// 
        /// @return a byte array.
        public void ToRGB()
        {
            int stride;
            if (BitsPerPixel > 8)
                stride = SizeX * 3 * 2;
            else
                stride = SizeX * 3;

            int w = SizeX;
            int h = SizeY;
            byte[] bts = null;
            if (PixelFormat == PixelFormat.Format48bppRgb)
            {
                return;
            }
            else
            if (PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                bts = new byte[h * SizeX * 3 * 2];
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (w * 2 * 3);
                    int row16 = y * (w * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bts[rowRGB + indexRGB] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 1] = Bytes[row16 + index16 + 1];
                        //G
                        bts[rowRGB + indexRGB + 2] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 3] = Bytes[row16 + index16 + 1];
                        //B
                        bts[rowRGB + indexRGB + 4] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 5] = Bytes[row16 + index16 + 1];
                    }
                }
                Bytes = bts;
                PixelFormat = PixelFormat.Format48bppRgb;
            }
            else
            if (PixelFormat == PixelFormat.Format24bppRgb)
            {
                return;
            }
            else
            if (PixelFormat == PixelFormat.Format8bppIndexed)
            {
                bts = new byte[h * SizeX * 3];
                for (int y = 0; y < SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (SizeX * 3);
                    int row8 = y * (SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        bts[rowRGB + indexRGB] = Bytes[row8 + index8];
                        bts[rowRGB + indexRGB + 1] = Bytes[row8 + index8];
                        bts[rowRGB + indexRGB + 2] = Bytes[row8 + index8];
                    }
                }
                Bytes = bts;
                PixelFormat = PixelFormat.Format24bppRgb;
            }
        }
        /// It takes a bitmap, clones it, rotates it, and then sets the image to the rotated bitmap
        /// 
        /// @param RotateFlipType 
        public void RotateFlip(RotateFlipType rot)
        {
            Bitmap fl = (Bitmap)Image.Clone();
            fl.RotateFlip(rot);
            Image = fl;
            fl.Dispose();
        }
        /* Checking if the image is RGB or not. */
        public bool isRGB
        {
            get
            {
                if (pixelFormat == PixelFormat.Format8bppIndexed || pixelFormat == PixelFormat.Format16bppGrayScale)
                    return false;
                else
                    return true;
            }
        }
        /// It returns the ID of the object.
        /// 
        /// @return The ID of the object.
        public override string ToString()
        {
            return ID;
        }
        /// It's a function that disposes of the object's memory
        public void Dispose()
        {
            bytes = null;
            if (stats != null)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    if (stats[i] != null)
                        stats[i].Dispose();
                }
            }
            ID = null;
            file = null;
            GC.Collect();
        }

        public static BufferInfo operator /(BufferInfo a, BufferInfo b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static BufferInfo operator *(BufferInfo a, BufferInfo b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static BufferInfo operator +(BufferInfo a, BufferInfo b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static BufferInfo operator -(BufferInfo a, BufferInfo b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b.GetPixel(x, y));
                }
            }
            return bf;
        }

        public static BufferInfo operator /(BufferInfo a, float b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b);
                }
            }
            return bf;
        }
        public static BufferInfo operator *(BufferInfo a, float b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b);
                }
            }
            return bf;
        }
        public static BufferInfo operator +(BufferInfo a, float b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b);
                }
            }
            return bf;
        }
        public static BufferInfo operator -(BufferInfo a, float b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b);
                }
            }
            return bf;
        }

        public static BufferInfo operator /(BufferInfo a, ColorS b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b);
                }
            }
            return bf;
        }
        public static BufferInfo operator *(BufferInfo a, ColorS b)
        {
            BufferInfo bf = a.Copy();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b);
                }
            }
            return bf;
        }
        public static BufferInfo operator +(BufferInfo a, ColorS b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b);
                }
            }
            return bf;
        }
        public static BufferInfo operator -(BufferInfo a, ColorS b)
        {
            BufferInfo bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b);
                }
            }
            return bf;
        }
    }

    /* It's a class that holds a string, an IFilter, and a Type */
    public class Filt
    {
        /* Defining an enum. */
        public enum Type
        {
            Base,
            Base2,
            InPlace,
            InPlace2,
            InPlacePartial,
            Resize,
            Rotate,
            Transformation,
            Copy
        }
        public string name;
        public IFilter filt;
        public Type type;
        public Filt(string s, IFilter f, Type t)
        {
            name = s;
            filt = f;
            type = t;
        }
    }
    public static class Filters
    {
        /// It returns the filter object that has the name that was passed in
        /// 
        /// @param name The name of the filter.
        /// 
        /// @return The value of the key "name" in the dictionary "filters"
        public static Filt GetFilter(string name)
        {
            return filters[name];
        }
        public static Dictionary<string, Filt> filters = new Dictionary<string, Filt>();
        /// It takes an image, applies a filter to it, and returns the filtered image
        /// 
        /// @param id the id of the image to apply the filter to
        /// @param name The name of the filter to apply.
        /// @param inPlace If true, the image will be modified in place. If false, a new image will be
        /// created.
        /// 
        /// @return The image that was filtered.
        public static BioImage Base(string id, string name, bool inPlace)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseFilter fi = (BaseFilter)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false); ;
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.Base(" + '"' + id +
                    '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// This function takes two images, applies a filter to the first image, and then applies the
        /// second image as an overlay to the first image
        /// 
        /// @param id the id of the image to be filtered
        /// @param id2 the image to be filtered
        /// @param name The name of the filter.
        /// @param inPlace true if you want to apply the filter to the image in place, false if you want
        /// to create a new image with the filter applied.
        /// 
        /// @return The image that was filtered.
        public static BioImage Base2(string id, string id2, string name, bool inPlace)
        {
            BioImage c2 = Images.GetImage(id);
            BioImage img = Images.GetImage(id2);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseFilter2 fi = (BaseFilter2)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    fi.OverlayImage = (Bitmap)c2.Buffers[i].Image;
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.Base2(" + '"' + id + '"' + "," +
                   '"' + id2 + '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
                return img;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// It takes an image, applies a filter to it, and returns the image
        /// 
        /// @param id the id of the image to apply the filter to
        /// @param name The name of the filter to apply.
        /// @param inPlace If true, the image will be modified in place. If false, a copy of the image
        /// will be made and the copy will be modified.
        /// 
        /// @return The image that was filtered.
        public static BioImage InPlace(string id, string name, bool inPlace)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseInPlaceFilter fi = (BaseInPlaceFilter)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.InPlace(" + '"' + id +
                    '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
                return img;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// This function takes two images, applies a filter to the first image, and returns the first
        /// image
        /// 
        /// @param id the id of the image to be filtered
        /// @param id2 the id of the image to be filtered
        /// @param name The name of the filter to apply.
        /// @param inPlace true if you want to modify the original image, false if you want to create a
        /// new image
        /// 
        /// @return The image that was filtered.
        public static BioImage InPlace2(string id, string id2, string name, bool inPlace)
        {
            BioImage c2 = Images.GetImage(id);
            BioImage img = Images.GetImage(id2);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseInPlaceFilter2 fi = (BaseInPlaceFilter2)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    fi.OverlayImage = (Bitmap)c2.Buffers[i].Image;
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.InPlace2(" + '"' + id + '"' + "," +
                   '"' + id2 + '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
                return img;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// It takes an image, applies a filter to it, and returns the image
        /// 
        /// @param id the id of the image to apply the filter to
        /// @param name The name of the filter to apply.
        /// @param inPlace If true, the original image is modified. If false, a copy of the original
        /// image is modified.
        /// 
        /// @return The image that was filtered.
        public static BioImage InPlacePartial(string id, string name, bool inPlace)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseInPlacePartialFilter fi = (BaseInPlacePartialFilter)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.InPlacePartial(" + '"' + id +
                    '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
                return img;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// This function takes an image, resizes it, and returns the resized image
        /// 
        /// @param id the id of the image to be resized
        /// @param name The name of the filter to use.
        /// @param inPlace whether to apply the filter to the original image or to a copy of it
        /// @param w width
        /// @param h height
        /// 
        /// @return The image that was resized.
        public static BioImage Resize(string id, string name, bool inPlace, int w, int h)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseResizeFilter fi = (BaseResizeFilter)f.filt;
                fi.NewHeight = h;
                fi.NewWidth = w;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.Resize(" + '"' + id +
                    '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + "," + w + "," + h + ");");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// It takes an image, rotates it, and returns the rotated image
        /// 
        /// @param id the id of the image to be filtered
        /// @param name The name of the filter.
        /// @param inPlace whether to apply the filter to the original image or to a copy of it
        /// @param angle the angle to rotate the image
        /// @param a alpha
        /// @param r red
        /// @param g green
        /// @param b blue
        /// 
        /// @return The image that was rotated.
        public static BioImage Rotate(string id, string name, bool inPlace, float angle, int a, int r, int g, int b)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(Images.GetImage(id));
            try
            {
                Filt f = filters[name];
                BaseRotateFilter fi = (BaseRotateFilter)f.filt;
                fi.Angle = angle;
                fi.FillColor = System.Drawing.Color.FromArgb(a, r, g, b);
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img,true);
                }
                Recorder.AddLine("Bio.Filters.Rotate(" + '"' + id +
                    '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + "," + angle.ToString() + "," +
                    a + "," + r + "," + g + "," + b + ");");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;

        }
        /// This function takes an image, applies a filter to it, and returns the filtered image
        /// 
        /// @param id the id of the image to be transformed
        /// @param name The name of the filter
        /// @param inPlace true if you want to apply the filter to the original image, false if you want
        /// to create a new image with the filter applied.
        /// @param angle The angle of rotation in degrees.
        /// 
        /// @return The image that was transformed.
        public static BioImage Transformation(string id, string name, bool inPlace, float angle)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseTransformationFilter fi = (BaseTransformationFilter)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].SetImage(fi.Apply((Bitmap)img.Buffers[i].Image), false);
                }
                if (!inPlace)
                {
                    Images.AddImage(img,true);
                }
                Recorder.AddLine("Bio.Filters.Transformation(" + '"' + id +
                        '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + "," + angle + ");");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// It takes an image, applies a filter to it, and returns the filtered image
        /// 
        /// @param id the id of the image to be filtered
        /// @param name The name of the filter to apply.
        /// @param inPlace If true, the original image will be modified. If false, a copy of the image
        /// will be made and the copy will be modified.
        /// 
        /// @return The image that was copied.
        public static BioImage Copy(string id, string name, bool inPlace)
        {
            BioImage img = Images.GetImage(id);
            if (!inPlace)
                img = BioImage.Copy(img);
            try
            {
                Filt f = filters[name];
                BaseUsingCopyPartialFilter fi = (BaseUsingCopyPartialFilter)f.filt;
                for (int i = 0; i < img.Buffers.Count; i++)
                {
                    img.Buffers[i].Image = fi.Apply((Bitmap)img.Buffers[i].Image);
                }
                if (!inPlace)
                {
                    Images.AddImage(img, true);
                }
                Recorder.AddLine("Bio.Filters.Copy(" + '"' + id +
                        '"' + "," + '"' + name + '"' + "," + inPlace.ToString().ToLower() + ");");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Filter Error");
            }
            return img;
        }
        /// It takes an image, crops it, and returns the cropped image
        /// 
        /// @param id the id of the image to crop
        /// @param x x coordinate of the top left corner of the rectangle
        /// @param y y-coordinate of the top-left corner of the rectangle
        /// @param w width of the image
        /// @param h height
        /// 
        /// @return The cropped image.
        public static BioImage Crop(string id, double x, double y, double w, double h)
        {
            BioImage c = Images.GetImage(id);
            RectangleF r = c.ToImageSpace(new RectangleD(x, y, w, h));
            Rectangle rec = new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
            BioImage img = BioImage.Copy(c, false);
            for (int i = 0; i < img.Buffers.Count; i++)
            {
                img.Buffers[i].Crop(rec);
            }
            Images.AddImage(img,false);
            Recorder.AddLine("Bio.Filters.Crop(" + '"' + id + '"' + "," + x + "," + y + "," + w + "," + h + ");");
            App.tabsView.AddTab(img);
            return img;
        }
        /// > This function takes a string and a rectangle and returns a BioImage
        /// 
        /// @param id the id of the image to crop
        /// @param RectangleD 
        /// 
        /// @return A BioImage object
        public static BioImage Crop(string id, RectangleD r)
        {
            return Crop(id, r.X, r.Y, r.W, r.H);
        }
        /// It creates a dictionary of filters, where the key is the name of the filter and the value is
        /// the filter itself
        public static void Init()
        {
            //Base Filters
            Filt f = new Filt("AdaptiveSmoothing", new AdaptiveSmoothing(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("BayerFilter", new BayerFilter(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("BayerFilterOptimized", new BayerFilterOptimized(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("BayerDithering", new BayerDithering(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("ConnectedComponentsLabeling", new ConnectedComponentsLabeling(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("ExtractChannel", new ExtractChannel(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("ExtractNormalizedRGBChannel", new ExtractNormalizedRGBChannel(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("Grayscale", new Grayscale(0.2125, 0.7154, 0.0721), Filt.Type.Base);
            filters.Add(f.name, f);
            //f = new Filt("TexturedFilter", new TexturedFilter());
            //filters.Add(f.name, f);
            f = new Filt("WaterWave", new WaterWave(), Filt.Type.Base);
            filters.Add(f.name, f);
            f = new Filt("YCbCrExtractChannel", new YCbCrExtractChannel(), Filt.Type.Base);
            filters.Add(f.name, f);

            //BaseFilter2
            f = new Filt("ThresholdedDifference", new ThresholdedDifference(), Filt.Type.Base2);
            filters.Add(f.name, f);
            f = new Filt("ThresholdedEuclideanDifference", new ThresholdedDifference(), Filt.Type.Base2);
            filters.Add(f.name, f);


            //BaseInPlaceFilter
            f = new Filt("BackwardQuadrilateralTransformation", new BackwardQuadrilateralTransformation(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("BlobsFiltering", new BlobsFiltering(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("BottomHat", new BottomHat(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("BradleyLocalThresholding", new BradleyLocalThresholding(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("CanvasCrop", new CanvasCrop(Rectangle.Empty), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("CanvasFill", new CanvasFill(Rectangle.Empty), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("CanvasMove", new CanvasMove(new IntPoint()), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("FillHoles", new FillHoles(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("FlatFieldCorrection", new FlatFieldCorrection(), Filt.Type.InPlace);
            filters.Add(f.name, f);
            f = new Filt("TopHat", new TopHat(), Filt.Type.InPlace);
            filters.Add(f.name, f);

            //BaseInPlaceFilter2
            f = new Filt("Add", new Add(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("Difference", new Difference(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("Intersect", new Intersect(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("Merge", new Merge(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("Morph", new Morph(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("MoveTowards", new MoveTowards(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("StereoAnaglyph", new StereoAnaglyph(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            f = new Filt("Subtract", new Subtract(), Filt.Type.InPlace2);
            filters.Add(f.name, f);
            //f = new Filt("Add", new TexturedMerge(), Filt.Type.InPlace2);
            //filters.Add(f.name, f);

            //BaseInPlacePartialFilter
            f = new Filt("AdditiveNoise", new AdditiveNoise(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);

            //f = new Filt("ApplyMask", new ApplyMask(), Filt.Type.InPlacePartial2);
            //filters.Add(f.name, f);
            f = new Filt("BrightnessCorrection", new BrightnessCorrection(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ChannelFiltering", new ChannelFiltering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ColorFiltering", new ColorFiltering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ColorRemapping", new ColorRemapping(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ContrastCorrection", new ContrastCorrection(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ContrastStretch", new ContrastStretch(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            //f = new Filt("ErrorDiffusionDithering", new ErrorDiffusionDithering(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);
            f = new Filt("EuclideanColorFiltering", new EuclideanColorFiltering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("GammaCorrection", new GammaCorrection(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("HistogramEqualization", new HistogramEqualization(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("HorizontalRunLengthSmoothing", new HorizontalRunLengthSmoothing(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("HSLFiltering", new HSLFiltering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("HueModifier", new HueModifier(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("Invert", new Invert(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("LevelsLinear", new LevelsLinear(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("LevelsLinear16bpp", new LevelsLinear16bpp(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            //f = new Filt("MaskedFilter", new MaskedFilter(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);
            //f = new Filt("Mirror", new Mirror(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);
            f = new Filt("OrderedDithering", new OrderedDithering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("OtsuThreshold", new OtsuThreshold(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("Pixellate", new Pixellate(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("PointedColorFloodFill", new PointedColorFloodFill(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("PointedMeanFloodFill", new PointedMeanFloodFill(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("ReplaceChannel", new Invert(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("RotateChannels", new LevelsLinear(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("SaltAndPepperNoise", new LevelsLinear16bpp(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("SaturationCorrection", new SaturationCorrection(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("Sepia", new Sepia(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("SimplePosterization", new SimplePosterization(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("SISThreshold", new SISThreshold(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            //f = new Filt("Texturer", new Texturer(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);
            //f = new Filt("Threshold", new Threshold(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);
            f = new Filt("ThresholdWithCarry", new ThresholdWithCarry(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("VerticalRunLengthSmoothing", new VerticalRunLengthSmoothing(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("YCbCrFiltering", new YCbCrFiltering(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            f = new Filt("YCbCrLinear", new YCbCrLinear(), Filt.Type.InPlacePartial);
            filters.Add(f.name, f);
            //f = new Filt("YCbCrReplaceChannel", new YCbCrReplaceChannel(), Filt.Type.InPlacePartial);
            //filters.Add(f.name, f);

            //BaseResizeFilter
            f = new Filt("ResizeBicubic", new ResizeBicubic(0, 0), Filt.Type.Resize);
            filters.Add(f.name, f);
            f = new Filt("ResizeBilinear", new ResizeBilinear(0, 0), Filt.Type.Resize);
            filters.Add(f.name, f);
            f = new Filt("ResizeNearestNeighbor", new ResizeNearestNeighbor(0, 0), Filt.Type.Resize);
            filters.Add(f.name, f);
            //BaseRotateFilter
            f = new Filt("RotateBicubic", new RotateBicubic(0), Filt.Type.Rotate);
            filters.Add(f.name, f);
            f = new Filt("RotateBilinear", new RotateBilinear(0), Filt.Type.Rotate);
            filters.Add(f.name, f);
            f = new Filt("RotateNearestNeighbor", new RotateNearestNeighbor(0), Filt.Type.Rotate);
            filters.Add(f.name, f);

            //Transformation
            f = new Filt("Crop", new AForge.Imaging.Filters.Crop(Rectangle.Empty), Filt.Type.Transformation);
            filters.Add(f.name, f);

            f = new Filt("QuadrilateralTransformation", new QuadrilateralTransformation(), Filt.Type.Transformation);
            filters.Add(f.name, f);
            //f = new Filt("QuadrilateralTransformationBilinear", new QuadrilateralTransformationBilinear(), Filt.Type.Transformation);
            //filters.Add(f.name, f);
            //f = new Filt("QuadrilateralTransformationNearestNeighbor", new QuadrilateralTransformationNearestNeighbor(), Filt.Type.Transformation);
            //filters.Add(f.name, f);
            f = new Filt("Shrink", new Shrink(), Filt.Type.Transformation);
            filters.Add(f.name, f);
            f = new Filt("SimpleQuadrilateralTransformation", new SimpleQuadrilateralTransformation(), Filt.Type.Transformation);
            filters.Add(f.name, f);
            f = new Filt("TransformFromPolar", new TransformFromPolar(), Filt.Type.Transformation);
            filters.Add(f.name, f);
            f = new Filt("TransformToPolar", new TransformToPolar(), Filt.Type.Transformation);
            filters.Add(f.name, f);

            //BaseUsingCopyPartialFilter 
            f = new Filt("BinaryDilatation3x3", new BinaryDilatation3x3(), Filt.Type.Copy);
            filters.Add(f.name, f);
            f = new Filt("BilateralSmoothing ", new BilateralSmoothing(), Filt.Type.Copy);
            filters.Add(f.name, f);
            f = new Filt("BinaryErosion3x3 ", new BinaryErosion3x3(), Filt.Type.Copy);
            filters.Add(f.name, f);

        }
    }
    /* It calculates the statistics of an image */
    public class Statistics
    {
        private int[] values = null;
        public int[] Values
        {
            get { return values; }
            set { values = value; }
        }
        private int bitsPerPixel;
        private int min = ushort.MaxValue;
        private int max = ushort.MinValue;
        private float stackMin = ushort.MaxValue;
        private float stackMax = ushort.MinValue;
        private float stackMean = 0;
        private float stackMedian = 0;
        private float mean = 0;
        private float median = 0;
        private float meansum = 0;
        private float[] stackValues = new float[ushort.MaxValue];
        private int count = 0;
        public int Min
        {
            get { return min; }
        }
        public int Max
        {
            get { return max; }
        }
        public double Mean
        {
            get { return mean; }
        }
        public int BitsPerPixel
        {
            get { return bitsPerPixel; }
        }
        public float Median
        {
            get
            {
                return median;
            }
        }
        public float StackMedian
        {
            get
            {
                return stackMedian;
            }
        }
        public float StackMean
        {
            get
            {
                return stackMean;
            }
        }
        public float StackMax
        {
            get
            {
                return stackMax;
            }
        }
        public float StackMin
        {
            get
            {
                return stackMin;
            }
        }
        public float[] StackValues
        {
            get { return stackValues; }
        }
        public Statistics(bool bit16)
        {
            values = new int[ushort.MaxValue + 1];
            if (bit16)
            {
                bitsPerPixel = 16;
            }
            else
            {
                bitsPerPixel = 8;
            }
        }
        /// It takes a byte array, width, height, number of channels, bits per pixel, and stride, and
        /// returns an array of Statistics objects
        /// 
        /// @param bts the byte array of the image
        /// @param w width of the image
        /// @param h height of the image
        /// @param rGBChannels The number of channels in the image.
        /// @param BitsPerPixel 8 or 16
        /// @param stride The number of bytes per row.
        /// 
        /// @return An array of Statistics objects.
        public static Statistics[] FromBytes(byte[] bts, int w, int h, int rGBChannels, int BitsPerPixel, int stride)
        {
            Statistics[] sts = new Statistics[rGBChannels];
            bool bit16 = false;
            if (BitsPerPixel > 8)
                bit16 = true;
            for (int i = 0; i < rGBChannels; i++)
            {
                sts[i] = new Statistics(bit16);
                sts[i].max = ushort.MinValue;
                sts[i].min = ushort.MaxValue;
                sts[i].bitsPerPixel = BitsPerPixel;
            }

            float sumr = 0;
            float sumg = 0;
            float sumb = 0;
            float suma = 0;
            if (BitsPerPixel > 8)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < stride; x += 2 * rGBChannels)
                    {
                        if (rGBChannels == 3)
                        {
                            ushort b = BitConverter.ToUInt16(bts, (y * stride + (x)));
                            ushort g = BitConverter.ToUInt16(bts, (y * stride + (x + 2)));
                            ushort r = BitConverter.ToUInt16(bts, (y * stride + (x + 4)));
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                            if (sts[1].max < g)
                                sts[1].max = g;
                            if (sts[1].min > g)
                                sts[1].min = g;
                            sts[1].values[g]++;
                            sumg += g;
                            if (sts[2].max < b)
                                sts[2].max = b;
                            if (sts[2].min > b)
                                sts[2].min = b;
                            sts[2].values[b]++;
                            sumb += b;
                        }
                        else
                        {
                            ushort r = BitConverter.ToUInt16(bts, (y * stride + (x)));
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (rGBChannels > 1)
                        {
                            byte b = bts[y * stride + x];
                            byte g = bts[y * stride + (x + 1)];
                            byte r = bts[y * stride + (x + 2)];
                            byte a = 0;
                            if (rGBChannels == 4)
                            {
                                a = bts[y * stride + x];
                                b = bts[y * stride + (x + 1)];
                                g = bts[y * stride + (x + 2)];
                                r = bts[y * stride + (x + 3)];

                                if (sts[0].max < a)
                                    sts[0].max = a;
                                if (sts[0].min > a)
                                    sts[0].min = a;
                                sts[0].values[a]++;
                                suma += a;
                                if (sts[1].max < b)
                                    sts[1].max = b;
                                if (sts[1].min > b)
                                    sts[1].min = b;
                                sts[1].values[b]++;
                                sumb += b;
                                if (sts[2].max < g)
                                    sts[2].max = g;
                                if (sts[2].min > g)
                                    sts[2].min = g;
                                sts[2].values[g]++;
                                sumg += g;
                                if (sts[3].max < r)
                                    sts[3].max = r;
                                if (sts[3].min > r)
                                    sts[3].min = r;
                                sts[3].values[r]++;
                                sumr += r;
                            }
                            else
                            {
                                if (sts[0].max < r)
                                    sts[0].max = r;
                                if (sts[0].min > r)
                                    sts[0].min = r;
                                sts[0].values[r]++;
                                sumr += r;
                                if (sts[1].max < g)
                                    sts[1].max = g;
                                if (sts[1].min > g)
                                    sts[1].min = g;
                                sts[1].values[g]++;
                                sumg += g;
                                if (sts[2].max < b)
                                    sts[2].max = b;
                                if (sts[2].min > b)
                                    sts[2].min = b;
                                sts[2].values[b]++;
                                sumb += b;
                            }
                        }
                        else
                        {
                            byte r = bts[y * stride + x];
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                        }
                    }
                }
            }

            sts[0].mean = sumr / (float)(w * h);
            if (rGBChannels > 1)
            {
                sts[1].mean = sumg / (float)(w * h);
                sts[2].mean = sumb / (float)(w * h);
                if (rGBChannels == 4)
                    sts[3].mean = suma / (float)(w * h);
            }

            for (int i = 0; i < sts[0].values.Length; i++)
            {
                if (sts[0].median < sts[0].values[i])
                    sts[0].median = sts[0].values[i];
            }
            if (rGBChannels > 1)
            {
                for (int i = 0; i < sts[1].values.Length; i++)
                {
                    if (sts[1].median < sts[1].values[i])
                        sts[1].median = sts[1].values[i];
                    if (sts[2].median < sts[2].values[i])
                        sts[2].median = sts[2].values[i];
                    if (rGBChannels == 4)
                    {
                        if (sts[3].median < sts[3].values[i])
                            sts[3].median = sts[3].values[i];
                    }
                }
            }
            return sts;
        }
        /// It takes a byte array, and returns an array of Statistics objects
        /// 
        /// @param BufferInfo 
        /// 
        /// @return An array of Statistics objects.
        public static Statistics[] FromBytes(BufferInfo bf)
        {
            return FromBytes(bf.Bytes, bf.SizeX, bf.SizeY, bf.RGBChannelsCount, bf.BitsPerPixel, bf.Stride);
        }
        public static BioImage b = null;
        public static Dictionary<string, BufferInfo> list = new Dictionary<string, BufferInfo>();
        /// It takes the data from the byte array and converts it into a string
        public static void FromBytes()
        {
            string name = Thread.CurrentThread.Name;
            list[name].Stats = FromBytes(list[name]);
            list.Remove(name);
        }
        /// It creates a new thread, adds the thread to a dictionary, and starts the thread
        /// 
        /// @param BufferInfo 
        public static void CalcStatistics(BufferInfo bf)
        {
            bf.Stats = null;
            Thread th = new Thread(FromBytes);
            th.Name = bf.ID;
            list.Add(th.Name.ToString(), bf);
            th.Start();
        }
        /// This function clears the list of numbers and operators
        public static void ClearCalcBuffer()
        {
            list.Clear();
        }
        /// It takes a Statistics object and adds it to the current Statistics object
        /// 
        /// @param Statistics 
        public void AddStatistics(Statistics s)
        {
            if (stackValues == null)
            {
                if (bitsPerPixel > 8)
                    stackValues = new float[ushort.MaxValue + 1];
                else
                    stackValues = new float[byte.MaxValue + 1];
            }
            if (stackMax < s.max)
                stackMax = s.max;
            if (stackMin > s.min)
                stackMin = s.min;
            meansum += s.mean;
            for (int i = 0; i < stackValues.Length; i++)
            {
                stackValues[i] += s.values[i];
            }
            values = s.values;
            count++;
        }
        /// This function takes the sum of all the values in the stack and divides it by the number of
        /// values in the stack. 
        /// 
        /// The stackMean is the average of all the values in the stack. 
        /// 
        /// The stackMedian is the highest value in the stack. 
        /// 
        /// The stackValues is the array of values in the stack. 
        /// 
        /// The count is the number of values in the stack. 
        /// 
        /// The meansum is the sum of all the values in the stack.
        public void MeanHistogram()
        {
            for (int i = 0; i < stackValues.Length; i++)
            {
                stackValues[i] /= (float)count;
            }
            stackMean = (float)meansum / (float)count;

            for (int i = 0; i < stackValues.Length; i++)
            {
                if (stackMedian < stackValues[i])
                    stackMedian = (float)stackValues[i];
            }

        }

        /// It takes an array of floats, and returns an array of points, where each point is the average
        /// of the values in the original array
        /// 
        /// @param bin The number of pixels to skip when sampling the image.
        /// 
        /// @return An array of points.
        public PointF[] GetPoints(int bin)
        {
            PointF[] pts = new PointF[stackValues.Length];
            for (int x = 0; x < stackValues.Length; x += bin)
            {
                pts[x].X = x;
                pts[x].Y = stackValues[x];
            }
            return pts;
        }
        /// The Dispose() function is used to free up memory that is no longer needed
        public void Dispose()
        {
            stackValues = null;
            values = null;
        }
        /// > This function is used to free up the memory used by the histogram
        public void DisposeHistogram()
        {
            stackValues = null;
            values = null;
        }
    }
    /* The class is used to store the physical and stage sizes of an image */
    public class ImageInfo
    {
        bool HasPhysicalXY;
        bool HasPhysicalXYZ = false;
        private double physicalSizeX = -1;
        private double physicalSizeY = -1;
        private double physicalSizeZ = -1;
        public double PhysicalSizeX
        {
            get { return physicalSizeX; }
            set
            {
                physicalSizeX = value;
                HasPhysicalXY = true;
            }
        }
        public double PhysicalSizeY
        {
            get { return physicalSizeY; }
            set
            {
                physicalSizeY = value;
                HasPhysicalXY = true;
            }
        }
        public double PhysicalSizeZ
        {
            get { return physicalSizeZ; }
            set
            {
                physicalSizeZ = value;
                HasPhysicalXYZ = true;
            }
        }

        bool HasStageXY = false;
        bool HasStageXYZ = false;
        public double stageSizeX = -1;
        public double stageSizeY = -1;
        public double stageSizeZ = -1;
        public double StageSizeX
        {
            get { return stageSizeX; }
            set
            {
                stageSizeX = value;
                HasStageXY = true;
            }
        }
        public double StageSizeY
        {
            get { return stageSizeY; }
            set
            {
                stageSizeY = value;
                HasStageXY = true;
            }
        }
        public double StageSizeZ
        {
            get { return stageSizeZ; }
            set
            {
                stageSizeZ = value;
                HasStageXYZ = true;
            }
        }

        private int series = 0;
        public int Series
        {
            get { return series; }
            set { series = value; }
        }

        public ImageInfo Copy()
        {
            ImageInfo inf = new ImageInfo();
            inf.PhysicalSizeX = PhysicalSizeX;
            inf.PhysicalSizeY = PhysicalSizeY;
            inf.PhysicalSizeZ = PhysicalSizeZ;
            inf.StageSizeX = StageSizeX;
            inf.StageSizeY = StageSizeY;
            inf.StageSizeZ = StageSizeZ;
            inf.HasPhysicalXY = HasPhysicalXY;
            inf.HasPhysicalXYZ = HasPhysicalXYZ;
            inf.StageSizeX = StageSizeX;
            inf.StageSizeY = StageSizeY;
            inf.StageSizeZ = StageSizeZ;
            inf.HasStageXY = HasStageXY;
            inf.HasStageXYZ = HasStageXYZ;
            return inf;
        }

    }
    public class BioImage : IDisposable
    {
        public class WellPlate
        {
            public class Well
            {
                public string ID { get; set; }
                public int Index { get; set; }
                public int Column { get; set; }
                public int Row { get; set; }
                public System.Drawing.Color Color { get; set; }
                public List<Sample> Samples = new List<Sample>();
                public class Sample
                {
                    public string ID { get; set; }
                    public int Index { get; set; }
                    public PointD Position { get; set; }
                    public Timestamp Time { get; set; }
                }
            }
            public List<Well> Wells = new List<Well>();
        
            public PointD Origin;
            public string ID;
            public string Name;
            public WellPlate(BioImage b)
            {
                ID = b.meta.getPlateID(b.series);
                Name = b.meta.getPlateName(b.series);
                double x, y;
                if (b.meta.getPlateWellOriginX(b.series) != null)
                    x = b.meta.getPlateWellOriginX(b.series).value().doubleValue();
                else
                    x = 0;
                if (b.meta.getPlateWellOriginY(b.series) != null)
                    y = b.meta.getPlateWellOriginY(b.series).value().doubleValue();
                else
                    y = 0;
                Origin = new PointD(x, y);
                int ws = b.meta.getWellCount(b.series);
                for (int i = 0; i < ws; i++)
                {
                    Well w = new Well();
                    w.ID = b.meta.getWellID(b.series, i);
                    w.Column = b.meta.getWellColumn(b.series, i).getNumberValue().intValue();
                    w.Row = b.meta.getWellRow(b.series, i).getNumberValue().intValue();
                    int wsc = b.meta.getWellSampleCount(b.series, i);
                    for (int s = 0; s < wsc; s++)
                    {
                        Well.Sample sa = new Well.Sample();
                        sa.Time = b.meta.getWellSampleTimepoint(b.series, i, s);
                        sa.Index = b.meta.getWellSampleIndex(b.series, i, s).getNumberValue().intValue();
                        double sx, sy;
                        if (b.meta.getWellSamplePositionX(b.series, i, s) != null)
                            sx = b.meta.getWellSamplePositionX(b.series, i, s).value().doubleValue();
                        else sx = 0;
                        if (b.meta.getWellSamplePositionY(b.series, i, s) != null)
                            sy = b.meta.getWellSamplePositionY(b.series, i, s).value().doubleValue();
                        else sy = 0;
                        sa.Position = new PointD(sx, sy);
                        sa.ID = b.meta.getWellSampleID(b.series, i, s);
                        w.Samples.Add(sa);
                    }
                    ome.xml.model.primitives.Color c = b.meta.getWellColor(b.series, i);
                    if(c!=null)
                    w.Color = System.Drawing.Color.FromArgb(c.getAlpha(),c.getRed(),c.getGreen(),c.getBlue());
                    Wells.Add(w);
                }

            }
        }
        public int[,,] Coords;
        private ZCT coordinate;
        /* A property. */
        public ZCT Coordinate
        {
            get
            {
                return coordinate;
            }
            set
            {
                coordinate = value;
            }
        }
        public enum ImageType
        {
            pyramidal,
            stack,
            well
        }
        private string id;
        private ImageType type = ImageType.stack;
        public WellPlate Plate = null;
        public ImageType Type
        {
            get { return type; }
            set 
            { 
                type = value;
            }
        }
        public List<Channel> Channels = new List<Channel>();
        public List<Resolution> Resolutions = new List<Resolution>();
        public List<BufferInfo> Buffers = new List<BufferInfo>();
        public VolumeD Volume {  get; set; }
        public List<ROI> Annotations = new List<ROI>();
        public OpenSlideImage openSlideImage;
        public OpenSlideBase openSlideBase;
        public SlideBase slideBase;
        public SlideImage slideImage;
        public string filename = "";
        public string script = "";
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }
        public int[] rgbChannels = new int[3];
        public int RGBChannelCount
        {
            get
            {
                return Buffers[0].RGBChannelsCount;
            }
        }
        public int? MacroResolution { get; set; }
        public int? LabelResolution { get; set; }
        void SetLabelMacroResolutions()
        {
            int i = 0;
            Size s = new Size(int.MaxValue, int.MaxValue);
            bool noMacro = true;
            PixelFormat pf = Resolutions[0].PixelFormat;
            foreach (Resolution res in Resolutions)
            {
                if (res.SizeX < s.Width && res.SizeY < s.Height)
                {
                    //If the pixel format changes it means this is the macro resolution.
                    if (pf != res.PixelFormat)
                    {
                        noMacro = false;
                        break;
                    }
                    pf = res.PixelFormat;
                    s = new Size(res.SizeX, res.SizeY);
                }
                else
                {
                    //If this level is bigger than the previous this is the macro resolution.
                    noMacro = false;
                    break;
                }
                i++;
            }
            if (!noMacro)
            {
                MacroResolution = i;
                LabelResolution = i + 1;
            }
        }
        public int bitsPerPixel;
        public int imagesPerSeries = 0;
        public int seriesCount = 1;
        public double frameInterval = 0;
        public bool littleEndian = false;
        public bool isGroup = false;
        public long loadTimeMS = 0;
        public long loadTimeTicks = 0;
        public bool selected = false;
        private double resolution = 1;
        public double Resolution 
        { 
            get { return resolution; }
            set 
            {
                if(value<0)
                    return;
                if (!isPyramidal)
                    return;
                if(MacroResolution.HasValue)
                {
                    if (value < GetUnitPerPixel(Level))
                        resolution = value;
                }
                else
                resolution = value; 
            } 
        }
        public Statistics Statistics
        {
            get
            {
                return statistics;
            }
            set
            {
                statistics = value;
            }
        }
        private int sizeZ, sizeC, sizeT;
        private Statistics statistics;
        int tileX, tileY;
        private int level = 0;
        ImageInfo imageInfo = new ImageInfo();
        public static BioImage Copy(BioImage b, bool rois)
        {
            BioImage bi = new BioImage(b.ID);
            if (rois)
                foreach (ROI an in b.Annotations)
                {
                    bi.Annotations.Add(an);
                }
            foreach (BufferInfo bf in b.Buffers)
            {
                bi.Buffers.Add(bf.Copy());
            }
            foreach (Channel c in b.Channels)
            {
                bi.Channels.Add(c);
            }
            bi.Coords = b.Coords;
            bi.sizeZ = b.sizeZ;
            bi.sizeC = b.sizeC;
            bi.sizeT = b.sizeT;
            bi.series = b.series;
            bi.seriesCount = b.seriesCount;
            bi.frameInterval = b.frameInterval;
            bi.littleEndian = b.littleEndian;
            bi.isGroup = b.isGroup;
            bi.imageInfo = b.imageInfo.Copy();
            bi.bitsPerPixel = b.bitsPerPixel;
            bi.file = b.file;
            bi.filename = b.filename;
            bi.Resolutions = b.Resolutions;
            bi.statistics = b.statistics;
            return bi;
        }
        public static BioImage Copy(BioImage b)
        {
            return Copy(b, true);
        }
        public BioImage Copy(bool rois)
        {
            return BioImage.Copy(this, rois);
        }
        public BioImage Copy()
        {
            return BioImage.Copy(this, true);
        }
        public static BioImage CopyInfo(BioImage b, bool copyAnnotations, bool copyChannels)
        {
            BioImage bi = new BioImage(b.ID);
            if (copyAnnotations)
                foreach (ROI an in b.Annotations)
                {
                    bi.Annotations.Add(an.Copy());
                }
            if (copyChannels)
                foreach (Channel c in b.Channels)
                {
                    bi.Channels.Add(c.Copy());
                }

            bi.Coords = b.Coords;
            bi.sizeZ = b.sizeZ;
            bi.sizeC = b.sizeC;
            bi.sizeT = b.sizeT;
            bi.series = b.series;
            bi.seriesCount = b.seriesCount;
            bi.frameInterval = b.frameInterval;
            bi.littleEndian = b.littleEndian;
            bi.isGroup = b.isGroup;
            bi.imageInfo = b.imageInfo.Copy();
            bi.bitsPerPixel = b.bitsPerPixel;
            bi.Resolutions = b.Resolutions;
            bi.Coordinate = b.Coordinate;
            bi.file = b.file;
            bi.Filename = b.Filename;
            bi.ID = Images.GetImageName(b.file);
            bi.statistics = b.statistics;
            return bi;
        }
        public string ID
        {
            get { return id; }
            set { id = value; }
        }
        public int ImageCount
        {
            get
            {
                return Buffers.Count;
            }
        }
        public double PhysicalSizeX
        {
            get { return Resolutions[Level].PhysicalSizeX; }
        }
        public double PhysicalSizeY
        {
            get { return Resolutions[Level].PhysicalSizeY; }
        }
        public double PhysicalSizeZ
        {
            get { return Resolutions[Level].PhysicalSizeZ; }
        }
        public int TileX
        {
            get { return tileX; }
        }
        public int TileY
        {
            get { return tileY; }
        }
        public double StageSizeX
        {
            get
            {
                return Resolutions[Level].StageSizeX;
            }
            set { imageInfo.StageSizeX = value; }
        }
        public double StageSizeY
        {
            get { return Resolutions[Level].StageSizeY; }
            set { imageInfo.StageSizeY = value; }
        }
        public double StageSizeZ
        {
            get { return Resolutions[Level].StageSizeZ; }
            set { imageInfo.StageSizeZ = value; }
        }
        Size s = new Size(1920, 1080);
        public Size PyramidalSize { get { return s; } set { s = value; } }
        PointD pyramidalOrigin = new PointD(0, 0);
        public PointD PyramidalOrigin
        {
            get { return pyramidalOrigin; }
            set
            {
                pyramidalOrigin = value;
            }
        }
        public int Level
        {
            get
            {
                if (openSlideBase != null)
                    return OpenSlideGTK.TileUtil.GetLevel(openSlideBase.Schema.Resolutions, Resolution);
                else
                    if (slideBase != null)
                    return LevelFromResolution(Resolution);
                return level;
            }
            set
            {
                if (Type == ImageType.well)
                {
                    level = value;
                    reader.setId(file);
                    reader.setSeries(value);
                    // read the image data bytes
                    int pages = reader.getImageCount();
                    bool inter = reader.isInterleaved();
                    PixelFormat pf = Buffers[0].PixelFormat;
                    int w = Buffers[0].SizeX; int h = Buffers[0].SizeY;
                    Buffers.Clear();
                    for (int p = 0; p < pages; p++)
                    {
                        BufferInfo bf;
                        byte[] bytes = reader.openBytes(p);
                        bf = new BufferInfo(file, w, h, pf, bytes, new ZCT(), p, null, littleEndian, inter);
                        Buffers.Add(bf);
                    }
                }
            }
        }
        /// <summary>
        /// Get the downsampling factor of a given level.
        /// </summary>
        /// <param name="level">The desired level.</param>
        /// <return>
        /// The downsampling factor for this level.
        /// </return> 
        /// <exception cref="OpenSlideException"/>
        public double GetLevelDownsample(int level)
        {
            int originalWidth = Resolutions[0].SizeX; // Width of the original level
            int nextLevelWidth = Resolutions[level].SizeX; // Width of the next level (downsampled)
            return (double)originalWidth / (double)nextLevelWidth;
        }
        /// <summary>
        /// Returns the level of a given resolution.
        /// </summary>
        /// <param name="Resolution"></param>
        /// <returns></returns>
        public int LevelFromResolution(double Resolution)
        {
            double[] ds = new double[Resolutions.Count];
            for (int i = 0; i < Resolutions.Count; i++)
            {
                ds[i] = Resolutions[0].PhysicalSizeX * GetLevelDownsample(i);
            }
            for (int i = 0; i < ds.Length; i++)
            {
                if (ds[i] > Resolution)
                    return i;
            }
            return 0;
        }
        /// <summary>
        /// Get Unit Per Pixel for pyramidal images.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public double GetUnitPerPixel(int level)
        {
            return Resolutions[0].PhysicalSizeX * GetLevelDownsample(level);
        }
        /// <summary>
        /// Updates the Buffers based on current pyramidal origin and resolution.
        /// </summary>
        public async void UpdateBuffersPyramidal()
        {
            for (int i = 0; i < Buffers.Count; i++)
            {
                Buffers[i].Dispose();
            }
            Buffers.Clear();
            for (int i = 0; i < imagesPerSeries; i++)
            {
                if (openSlideImage != null)
                {
                    byte[] bts = openSlideBase.GetSlice(new OpenSlideGTK.SliceInfo(PyramidalOrigin.X, PyramidalOrigin.Y, PyramidalSize.Width, PyramidalSize.Height, resolution));
                    BufferInfo bf = new BufferInfo((int)Math.Round(OpenSlideBase.destExtent.Width), (int)Math.Round(OpenSlideBase.destExtent.Height), PixelFormat.Format24bppRgb, bts, new ZCT(), "");
                    bf.Stats = Statistics.FromBytes(bf);
                    Buffers.Add(bf);
                }
                else
                {
                start:
                    byte[] bts = await slideBase.GetSlice(new SliceInfo(PyramidalOrigin.X, PyramidalOrigin.Y, PyramidalSize.Width, PyramidalSize.Height, resolution));
                    if (bts == null)
                    {
                        if (PyramidalOrigin.X == 0 && PyramidalOrigin.Y == 0)
                        {
                            Resolution = 1;
                        }
                        pyramidalOrigin = new PointD(0, 0);
                        goto start;
                    }
                    BufferInfo bf = new BufferInfo((int)Math.Round(SlideBase.destExtent.Width), (int)Math.Round(SlideBase.destExtent.Height), PixelFormat.Format24bppRgb, bts, new ZCT(), "");
                    bf.Stats = Statistics.FromBytes(bf);
                    Buffers.Add(bf);
                }
            }
            BioImage.AutoThreshold(this, false);
            if (bitsPerPixel > 8)
                StackThreshold(true);
            else
                StackThreshold(false);
        }
        public int series
        {
            get
            {
                return imageInfo.Series;
            }
            set
            {
                imageInfo.Series = value;
            }
        }
        public List<NetVips.Image> vipPages = new List<NetVips.Image>();
        static bool initialized = false;
        public Channel RChannel
        {
            get
            {
                if (Channels[0].range.Length == 1)
                    return Channels[rgbChannels[0]];
                else
                    return Channels[0];
            }
        }
        public Channel GChannel
        {
            get
            {
                if (Channels[0].range.Length == 1)
                    return Channels[rgbChannels[1]];
                else
                    return Channels[0];
            }
        }
        public Channel BChannel
        {
            get
            {
                if (Channels[0].range.Length == 1)
                {
                    if (Channels.Count < 3)
                        return GChannel;
                    else
                        return Channels[rgbChannels[2]];
                }
                else
                    return Channels[0];
            }
        }
        public static bool Planes = false;
        /* It's a class that holds the information that is stored in the ImageJ description file */
        public class ImageJDesc
        {
            public string ImageJ;
            public int images = 0;
            public int channels = 0;
            public int slices = 0;
            public int frames = 0;
            public bool hyperstack;
            public string mode;
            public string unit;
            public double finterval = 0;
            public double spacing = 0;
            public bool loop;
            public double min = 0;
            public double max = 0;
            public int count;
            public bool bit8color = false;

            public ImageJDesc FromImage(BioImage b)
            {
                ImageJ = "";
                images = b.ImageCount;
                channels = b.SizeC;
                slices = b.SizeZ;
                frames = b.SizeT;
                hyperstack = true;
                mode = "grayscale";
                unit = "micron";
                finterval = b.frameInterval;
                spacing = b.PhysicalSizeZ;
                loop = false;
                /*
                double dmax = double.MinValue;
                double dmin = double.MaxValue;
                foreach (Channel c in b.Channels)
                {
                    if(dmax < c.Max)
                        dmax = c.Max;
                    if(dmin > c.Min)
                        dmin = c.Min;
                }
                min = dmin;
                max = dmax;
                */
                min = b.Channels[0].RangeR.Min;
                max = b.Channels[0].RangeR.Max;
                return this;
            }
            public string GetString()
            {
                string s = "";
                s += "ImageJ=" + ImageJ + "\n";
                s += "images=" + images + "\n";
                s += "channels=" + channels.ToString() + "\n";
                s += "slices=" + slices.ToString() + "\n";
                s += "frames=" + frames.ToString() + "\n";
                s += "hyperstack=" + hyperstack.ToString() + "\n";
                s += "mode=" + mode.ToString() + "\n";
                s += "unit=" + unit.ToString() + "\n";
                s += "finterval=" + finterval.ToString() + "\n";
                s += "spacing=" + spacing.ToString() + "\n";
                s += "loop=" + loop.ToString() + "\n";
                s += "min=" + min.ToString() + "\n";
                s += "max=" + max.ToString() + "\n";
                return s;
            }
            public void SetString(string desc)
            {
                string[] lines = desc.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                int maxlen = 20;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i < maxlen)
                    {
                        string[] sp = lines[i].Split('=');
                        if (sp[0] == "ImageJ")
                            ImageJ = sp[1];
                        if (sp[0] == "images")
                            images = int.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "channels")
                            channels = int.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "slices")
                            slices = int.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "frames")
                            frames = int.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "hyperstack")
                            hyperstack = bool.Parse(sp[1]);
                        if (sp[0] == "mode")
                            mode = sp[1];
                        if (sp[0] == "unit")
                            unit = sp[1];
                        if (sp[0] == "finterval")
                            finterval = double.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "spacing")
                            spacing = double.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "loop")
                            loop = bool.Parse(sp[1]);
                        if (sp[0] == "min")
                            min = double.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "max")
                            max = double.Parse(sp[1], CultureInfo.InvariantCulture);
                        if (sp[0] == "8bitcolor")
                            bit8color = bool.Parse(sp[1]);
                    }
                    else
                        return;
                }

            }
        }
        public int SizeX
        {
            get
            {
                if (Buffers.Count > 0)
                    return Buffers[0].SizeX;
                else return 0;
            }
        }
        public int SizeY
        {
            get
            {
                if (Buffers.Count > 0)
                    return Buffers[0].SizeY;
                else return 0;
            }
        }
        public int SizeZ
        {
            get { return sizeZ; }
        }
        public int SizeC
        {
            get { return sizeC; }
        }
        public int SizeT
        {
            get { return sizeT; }
        }
        public IntRange RRange
        {
            get
            {
                return RChannel.RangeR;
            }
        }
        public IntRange GRange
        {
            get
            {
                return GChannel.RangeG;
            }
        }
        public IntRange BRange
        {
            get
            {
                return BChannel.RangeB;
            }
        }
        public BufferInfo SelectedBuffer
        {
            get
            {
                return Buffers[Coords[Coordinate.Z, Coordinate.C, Coordinate.T]];
            }
        }
        public Stopwatch watch = new Stopwatch();
        public bool isRGB
        {
            get
            {
                if (RGBChannelCount == 3 || RGBChannelCount == 4)
                    return true;
                else
                    return false;
            }
        }
        public bool isTime
        {
            get
            {
                if (SizeT > 1)
                    return true;
                else
                    return false;
            }
        }
        public bool isSeries
        {
            get
            {
                if (seriesCount > 1)
                    return true;
                else
                    return false;
            }
        }
        public bool isPyramidal
        {
            get
            {
                if (Type == ImageType.pyramidal)
                    return true;
                else
                    return false;
            }
        }
        public string file;
        public static bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        /// It converts a 16-bit image to an 8-bit image
        /// 
        /// @return A list of BufferInfo objects.
        public void To8Bit()
        {
            if (Buffers[0].RGBChannelsCount == 4)
                To24Bit();
            PixelFormat px = Buffers[0].PixelFormat;
            if (px == PixelFormat.Format8bppIndexed)
                return;
            if (px == PixelFormat.Format48bppRgb)
            {
                To24Bit();
                List<BufferInfo> bfs = new List<BufferInfo>();
                int index = 0;
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap[] bs = BufferInfo.RGB24To8((Bitmap)Buffers[i].Image);
                    BufferInfo br = new BufferInfo(ID, bs[2], new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), index, Buffers[i].Plane);
                    BufferInfo bg = new BufferInfo(ID, bs[1], new ZCT(Buffers[i].Coordinate.Z, 1, Buffers[i].Coordinate.T), index + 1, Buffers[i].Plane);
                    BufferInfo bb = new BufferInfo(ID, bs[0], new ZCT(Buffers[i].Coordinate.Z, 2, Buffers[i].Coordinate.T), index + 2, Buffers[i].Plane);
                    for (int b = 0; b < 3; b++)
                    {
                        bs[b].Dispose();
                    }
                    bs = null;
                    GC.Collect();
                    Statistics.CalcStatistics(br);
                    Statistics.CalcStatistics(bg);
                    Statistics.CalcStatistics(bb);
                    bfs.Add(br);
                    bfs.Add(bg);
                    bfs.Add(bb);
                    index += 3;
                }
                Buffers = bfs;
                UpdateCoords(SizeZ, 3, SizeT);
            }
            else if (px == PixelFormat.Format24bppRgb)
            {
                List<BufferInfo> bfs = new List<BufferInfo>();
                int index = 0;
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap[] bs = BufferInfo.RGB24To8((Bitmap)Buffers[i].Image);
                    BufferInfo br = new BufferInfo(ID, bs[2], new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), index, Buffers[i].Plane);
                    BufferInfo bg = new BufferInfo(ID, bs[1], new ZCT(Buffers[i].Coordinate.Z, 1, Buffers[i].Coordinate.T), index + 1, Buffers[i].Plane);
                    BufferInfo bb = new BufferInfo(ID, bs[0], new ZCT(Buffers[i].Coordinate.Z, 2, Buffers[i].Coordinate.T), index + 2, Buffers[i].Plane);
                    for (int b = 0; b < 3; b++)
                    {
                        bs[b].Dispose();
                        bs[b] = null;
                    }
                    bs = null;
                    GC.Collect();
                    Statistics.CalcStatistics(br);
                    Statistics.CalcStatistics(bg);
                    Statistics.CalcStatistics(bb);
                    bfs.Add(br);
                    bfs.Add(bg);
                    bfs.Add(bb);
                    index += 3;
                }
                Buffers = bfs;
                UpdateCoords(SizeZ, 3, SizeT);
            }
            else
            {
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap b = AForge.Imaging.Image.Convert16bppTo8bpp((Bitmap)Buffers[i].Image);
                    Buffers[i].Image = b;
                    b.Dispose();
                    b = null;
                    GC.Collect();
                    Statistics.CalcStatistics(Buffers[i]);
                }
                for (int c = 0; c < Channels.Count; c++)
                {
                    Channels[c].BitsPerPixel = 8;
                    for (int i = 0; i < Channels[c].range.Length; i++)
                    {
                        Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)ushort.MaxValue) * byte.MaxValue);
                        Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)ushort.MaxValue) * byte.MaxValue);
                    }
                }

            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(this, false);
            bitsPerPixel = 8;
            Recorder.AddLine("Bio.Table.GetImage(" + '"' + ID + '"' + ")" + "." + "To8Bit();");
        }
        /// It converts the image to 16 bit, and then calculates the threshold for the image
        /// 
        /// @return A list of BufferInfo objects.
        public void To16Bit()
        {
            if (Buffers[0].RGBChannelsCount == 4)
                To24Bit();
            if (Buffers[0].PixelFormat == PixelFormat.Format16bppGrayScale)
                return;
            bitsPerPixel = 16;
            if (Buffers[0].PixelFormat == PixelFormat.Format48bppRgb)
            {
                List<BufferInfo> bfs = new List<BufferInfo>();
                int index = 0;
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Array.Reverse(Buffers[i].Bytes);
                    BufferInfo[] bs = BufferInfo.RGB48To16(ID, SizeX, SizeY, Buffers[i].Stride, Buffers[i].Bytes, Buffers[i].Coordinate, index, Buffers[i].Plane);
                    Statistics.CalcStatistics(bs[0]);
                    Statistics.CalcStatistics(bs[1]);
                    Statistics.CalcStatistics(bs[2]);
                    bfs.AddRange(bs);
                    index += 3;
                }
                Buffers = bfs;
                UpdateCoords(SizeZ, SizeC * 3, SizeT);
                if (Channels[0].SamplesPerPixel == 3)
                {
                    Channel c = Channels[0].Copy();
                    c.SamplesPerPixel = 1;
                    c.range = new IntRange[1];
                    Channels.Clear();
                    Channels.Add(c);
                    Channels.Add(c.Copy());
                    Channels.Add(c.Copy());
                    Channels[1].Index = 1;
                    Channels[2].Index = 2;
                }
            }
            else if (Buffers[0].PixelFormat == PixelFormat.Format8bppIndexed)
            {
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap b = AForge.Imaging.Image.Convert8bppTo16bpp((Bitmap)Buffers[i].Image);
                    Buffers[i].Image = b;
                    b.Dispose();
                    b = null;
                    GC.Collect();
                    Statistics.CalcStatistics(Buffers[i]);
                }
                for (int c = 0; c < Channels.Count; c++)
                {
                    for (int i = 0; i < Channels[c].range.Length; i++)
                    {
                        Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)byte.MaxValue) * ushort.MaxValue);
                        Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)byte.MaxValue) * ushort.MaxValue);
                    }
                    Channels[c].BitsPerPixel = 16;
                }
            }
            else if (Buffers[0].PixelFormat == PixelFormat.Format24bppRgb)
            {
                List<BufferInfo> bfs = new List<BufferInfo>();
                int index = 0;
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap[] bs = BufferInfo.RGB24To8((Bitmap)Buffers[i].Image);
                    BufferInfo br = new BufferInfo(ID, bs[2], new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), index, Buffers[i].Plane);
                    BufferInfo bg = new BufferInfo(ID, bs[1], new ZCT(Buffers[i].Coordinate.Z, 1, Buffers[i].Coordinate.T), index + 1, Buffers[i].Plane);
                    BufferInfo bb = new BufferInfo(ID, bs[0], new ZCT(Buffers[i].Coordinate.Z, 2, Buffers[i].Coordinate.T), index + 2, Buffers[i].Plane);
                    for (int b = 0; b < 3; b++)
                    {
                        bs[b].Dispose();
                        bs[b] = null;
                    }
                    bs = null;
                    GC.Collect();
                    br.To16Bit();
                    bg.To16Bit();
                    bb.To16Bit();
                    Statistics.CalcStatistics(br);
                    Statistics.CalcStatistics(bg);
                    Statistics.CalcStatistics(bb);
                    bfs.Add(br);
                    bfs.Add(bg);
                    bfs.Add(bb);
                    index += 3;
                }
                Buffers = bfs;
                UpdateCoords(SizeZ, 3, SizeT);
                for (int c = 0; c < Channels.Count; c++)
                {
                    for (int i = 0; i < Channels[c].range.Length; i++)
                    {
                        Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)byte.MaxValue) * ushort.MaxValue);
                        Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)byte.MaxValue) * ushort.MaxValue);
                    }
                    Channels[c].BitsPerPixel = 16;
                }
            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(this, false);
            StackThreshold(true);
            Recorder.AddLine("Bio.Table.GetImage(" + '"' + ID + '"' + ")" + "." + "To16Bit();");
        }
        /// Converts a 16 bit image to a 24 bit image
        /// 
        /// @return A Bitmap
        public void To24Bit()
        {
            if (Buffers[0].PixelFormat == PixelFormat.Format24bppRgb)
                return;
            bitsPerPixel = 8;
            if (Buffers[0].PixelFormat == PixelFormat.Format32bppArgb || Buffers[0].PixelFormat == PixelFormat.Format32bppRgb)
            {
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap b = BufferInfo.To24Bit((Bitmap)Buffers[i].Image);
                    Buffers[i].Image = b;
                    b.Dispose();
                    b = null;
                    GC.Collect();
                    Statistics.CalcStatistics(Buffers[i]);
                }
                if (Channels.Count == 4)
                {
                    Channels.RemoveAt(0);
                }
                else
                {
                    Channels[0].SamplesPerPixel = 3;
                }
            }
            else
            if (Buffers[0].PixelFormat == PixelFormat.Format48bppRgb)
            {
                //We run 8bit so we get 24 bit rgb.
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap b = AForge.Imaging.Image.Convert16bppTo8bpp((Bitmap)Buffers[i].Image);
                    Buffers[i].Image = b;
                    Statistics.CalcStatistics(Buffers[i]);
                }
                if (Channels[0].SamplesPerPixel == 3)
                {
                    Channel c = Channels[0].Copy();
                    c.SamplesPerPixel = 1;
                    c.range = new IntRange[1];
                    Channels.Clear();
                    Channels.Add(c);
                    Channels.Add(c.Copy());
                    Channels.Add(c.Copy());
                    Channels[1].Index = 1;
                    Channels[2].Index = 2;
                }
                for (int c = 0; c < Channels.Count; c++)
                {
                    for (int i = 0; i < Channels[c].range.Length; i++)
                    {
                        Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)ushort.MaxValue) * byte.MaxValue);
                        Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)ushort.MaxValue) * byte.MaxValue);
                    }
                    Channels[c].BitsPerPixel = 8;
                }
            }
            else
            if (Buffers[0].PixelFormat == PixelFormat.Format16bppGrayScale || Buffers[0].PixelFormat == PixelFormat.Format8bppIndexed)
            {
                if (Buffers[0].PixelFormat == PixelFormat.Format16bppGrayScale)
                {
                    for (int i = 0; i < Buffers.Count; i++)
                    {
                        Bitmap b = AForge.Imaging.Image.Convert16bppTo8bpp((Bitmap)Buffers[i].Image);
                        Buffers[i].Image = b;
                        b.Dispose();
                        b = null;
                        GC.Collect();
                    }
                    for (int c = 0; c < Channels.Count; c++)
                    {
                        for (int i = 0; i < Channels[c].range.Length; i++)
                        {
                            Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)ushort.MaxValue) * byte.MaxValue);
                            Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)ushort.MaxValue) * byte.MaxValue);
                        }
                        Channels[c].BitsPerPixel = 8;
                    }
                }
                List<BufferInfo> bfs = new List<BufferInfo>();
                if (Buffers.Count % 3 != 0 && Buffers.Count % 2 != 0)
                    for (int i = 0; i < Buffers.Count; i++)
                    {
                        BufferInfo bs = new BufferInfo(ID, SizeX, SizeY, Buffers[i].PixelFormat, Buffers[i].Bytes, new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), i, Buffers[i].Plane);
                        BufferInfo bbs = BufferInfo.RGB16To48(bs);
                        Statistics.CalcStatistics(bbs);
                        bs.Dispose();
                        bs = null;
                        bfs.Add(bbs);
                    }
                else
                    for (int i = 0; i < Buffers.Count; i += Channels.Count)
                    {
                        BufferInfo[] bs = new BufferInfo[3];
                        bs[0] = new BufferInfo(ID, SizeX, SizeY, Buffers[i].PixelFormat, Buffers[i].Bytes, new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), i, Buffers[i].Plane);
                        bs[1] = new BufferInfo(ID, SizeX, SizeY, Buffers[i + 1].PixelFormat, Buffers[i + 1].Bytes, new ZCT(Buffers[i + 1].Coordinate.Z, 0, Buffers[i + 1].Coordinate.T), i + 1, Buffers[i + 1].Plane);
                        if (Channels.Count > 2)
                            bs[2] = new BufferInfo(ID, SizeX, SizeY, Buffers[i + 2].PixelFormat, Buffers[i + 2].Bytes, new ZCT(Buffers[i + 2].Coordinate.Z, 0, Buffers[i + 2].Coordinate.T), i + 2, Buffers[i + 2].Plane);
                        BufferInfo bbs = BufferInfo.RGB8To24(bs);
                        for (int b = 0; b < 3; b++)
                        {
                            if (bs[b] != null)
                                bs[b].Dispose();
                            bs[b] = null;
                        }
                        Statistics.CalcStatistics(bbs);
                        bfs.Add(bbs);
                    }
                Buffers = bfs;
                UpdateCoords(SizeZ, 1, SizeT);
            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(this, false);
            //StackThreshold(false);
            Recorder.AddLine("Bio.Table.GetImage(" + '"' + ID + '"' + ")" + "." + "To24Bit();");
        }
        /// It converts the image to 32 bit format
        /// 
        /// @return A Bitmap object
        public void To32Bit()
        {
            if (Buffers[0].PixelFormat == PixelFormat.Format32bppArgb)
                return;
            if (Buffers[0].PixelFormat != PixelFormat.Format24bppRgb)
            {
                To24Bit();
            }
            for (int i = 0; i < Buffers.Count; i++)
            {
                Bitmap b = BufferInfo.To32Bit((Bitmap)Buffers[i].Image);
                Buffers[i].Image = b;
                Statistics.CalcStatistics(Buffers[i]);
                b.Dispose();
                b = null;
            }
            GC.Collect();
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(50);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(this, false);
            Recorder.AddLine("Bio.Table.GetImage(" + '"' + ID + '"' + ")" + "." + "To32Bit();");
        }
        /// It converts the image to 48 bit RGB
        /// 
        /// @return A list of BufferInfo objects.
        public void To48Bit()
        {
            if (Buffers[0].RGBChannelsCount == 4)
                To24Bit();
            if (Buffers[0].PixelFormat == PixelFormat.Format48bppRgb)
                return;
            if (Buffers[0].PixelFormat == PixelFormat.Format8bppIndexed || Buffers[0].PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                if (Buffers[0].PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    for (int i = 0; i < Buffers.Count; i++)
                    {
                        Bitmap b = AForge.Imaging.Image.Convert8bppTo16bpp((Bitmap)Buffers[i].Image);
                        Buffers[i].Image = b;
                        b.Dispose();
                        b = null;
                    }
                }
                GC.Collect();
                List<BufferInfo> bfs = new List<BufferInfo>();
                if (Buffers.Count % 3 != 0 && Buffers.Count % 2 != 0)
                    for (int i = 0; i < Buffers.Count; i++)
                    {
                        BufferInfo bs = new BufferInfo(ID, SizeX, SizeY, Buffers[i].PixelFormat, Buffers[i].Bytes, new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), i, Buffers[i].Plane);
                        BufferInfo bbs = BufferInfo.RGB16To48(bs);
                        Statistics.CalcStatistics(bbs);
                        bs.Dispose();
                        bs = null;
                        bfs.Add(bbs);
                    }
                else
                    for (int i = 0; i < Buffers.Count; i += Channels.Count)
                    {
                        BufferInfo[] bs = new BufferInfo[3];
                        if (Channels.Count > 2)
                        {
                            bs[2] = new BufferInfo(ID, SizeX, SizeY, Buffers[i].PixelFormat, Buffers[i].Bytes, new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), i, Buffers[i].Plane);
                            bs[1] = new BufferInfo(ID, SizeX, SizeY, Buffers[i + 1].PixelFormat, Buffers[i + 1].Bytes, new ZCT(Buffers[i + 1].Coordinate.Z, 0, Buffers[i + 1].Coordinate.T), i + 1, Buffers[i + 1].Plane);
                            bs[0] = new BufferInfo(ID, SizeX, SizeY, Buffers[i + 2].PixelFormat, Buffers[i + 2].Bytes, new ZCT(Buffers[i + 2].Coordinate.Z, 0, Buffers[i + 2].Coordinate.T), i + 2, Buffers[i + 2].Plane);
                        }
                        else
                        {
                            bs[0] = new BufferInfo(ID, SizeX, SizeY, Buffers[i].PixelFormat, Buffers[i].Bytes, new ZCT(Buffers[i].Coordinate.Z, 0, Buffers[i].Coordinate.T), i, Buffers[i].Plane);
                            bs[1] = new BufferInfo(ID, SizeX, SizeY, Buffers[i + 1].PixelFormat, Buffers[i + 1].Bytes, new ZCT(Buffers[i + 1].Coordinate.Z, 0, Buffers[i + 1].Coordinate.T), i + 1, Buffers[i + 1].Plane);
                        }
                        BufferInfo bbs = BufferInfo.RGB16To48(bs);
                        for (int b = 0; b < 3; b++)
                        {
                            if (bs[b] != null)
                                bs[b].Dispose();
                            bs[b] = null;
                        }
                        Statistics.CalcStatistics(bbs);
                        bfs.Add(bbs);
                    }
                GC.Collect();
                Buffers = bfs;
                int index = 0;
                UpdateCoords(SizeZ, 1, SizeT);

            }
            else
            if (Buffers[0].PixelFormat == PixelFormat.Format24bppRgb)
            {
                for (int i = 0; i < Buffers.Count; i++)
                {
                    Bitmap b = AForge.Imaging.Image.Convert8bppTo16bpp((Bitmap)Buffers[i].Image);
                    Buffers[i].Image = b;
                    b.Dispose();
                    Statistics.CalcStatistics(Buffers[i]);
                }
                for (int c = 0; c < Channels.Count; c++)
                {
                    for (int i = 0; i < Channels[c].range.Length; i++)
                    {
                        Channels[c].range[i].Min = (int)(((float)Channels[c].range[i].Min / (float)byte.MaxValue) * ushort.MaxValue);
                        Channels[c].range[i].Max = (int)(((float)Channels[c].range[i].Max / (float)byte.MaxValue) * ushort.MaxValue);
                    }
                    Channels[c].BitsPerPixel = 16;
                }
            }
            else
            {
                int index = 0;
                List<BufferInfo> buffers = new List<BufferInfo>();
                for (int i = 0; i < Buffers.Count; i += 3)
                {
                    BufferInfo[] bf = new BufferInfo[3];
                    bf[0] = Buffers[i];
                    bf[1] = Buffers[i + 1];
                    bf[2] = Buffers[i + 2];
                    BufferInfo inf = BufferInfo.RGB16To48(bf);
                    buffers.Add(inf);
                    Statistics.CalcStatistics(inf);
                    for (int b = 0; b < 3; b++)
                    {
                        bf[b].Dispose();
                    }
                    index++;
                }
                Buffers = buffers;
                UpdateCoords(SizeZ, 1, SizeT);
            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(50);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            bitsPerPixel = 16;
            AutoThreshold(this, false);
            Recorder.AddLine("Bio.Table.GetImage(" + '"' + ID + '"' + ")" + "." + "To48Bit();");
        }
        /// It rotates and flips the image
        /// 
        /// @param RotateFlipType 
        public void RotateFlip(RotateFlipType rot)
        {
            for (int i = 0; i < Buffers.Count; i++)
            {
                Buffers[i].RotateFlip(rot);
            }
        }
        /// Bake(int rmin, int rmax, int gmin, int gmax, int bmin, int bmax)
        /// 
        /// @param rmin The minimum value of the red channel.
        /// @param rmax The maximum value of the red channel.
        /// @param gmin The minimum value of the green channel.
        /// @param gmax The maximum value of the green channel.
        /// @param bmin The minimum value of the blue channel.
        /// @param bmax The maximum value of the blue channel.
        public void Bake(int rmin, int rmax, int gmin, int gmax, int bmin, int bmax)
        {
            Bake(new IntRange(rmin, rmax), new IntRange(gmin, gmax), new IntRange(bmin, bmax));
        }
        /// It takes a range of values for each channel, and creates a new image with the filtered
        /// values
        /// 
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        public void Bake(IntRange rf, IntRange gf, IntRange bf)
        {
            BioImage bm = new BioImage(Images.GetImageName(ID));
            bm = CopyInfo(this, true, true);
            for (int i = 0; i < Buffers.Count; i++)
            {
                ZCT co = Buffers[i].Coordinate;
                Bitmap b = GetFiltered(i, rf, gf, bf);
                BufferInfo inf = new BufferInfo(bm.ID, b, co, i);
                Statistics.CalcStatistics(inf);
                bm.Coords[co.Z, co.C, co.T] = i;
                bm.Buffers.Add(inf);
            }
            foreach (Channel item in bm.Channels)
            {
                for (int i = 0; i < item.range.Length; i++)
                {
                    item.range[i].Min = 0;
                    if (bm.bitsPerPixel > 8)
                        item.range[i].Max = ushort.MaxValue;
                    else
                        item.range[i].Max = 255;
                }
            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(50);
            } while (Buffers[Buffers.Count - 1].Stats == null);
            AutoThreshold(bm, false);
            Statistics.ClearCalcBuffer();
            Images.AddImage(bm, false);
            App.tabsView.AddTab(bm);
            Recorder.AddLine("ImageView.SelectedImage.Bake(" + rf.Min + "," + rf.Max + "," + gf.Min + "," + gf.Max + "," + bf.Min + "," + bf.Max + ");");
        }

        static bool vips = false;
        public static void OpenVips(BioImage b, int pagecount)
        {
            try
            {
                for (int i = 0; i < pagecount; i++)
                {
                    b.vipPages.Add(NetVips.Image.Tiffload(b.file, i));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public static void OpenVip(BioImage b, int pagenumber, int x, int y, int w, int h)
        {
            try
            {
                NetVips.Image im = NetVips.Image.Tiffload(b.file, pagenumber);
                b.vipPages.Add(im);
                BufferInfo bf = GetTile(b, new ZCT(0, 0, 0), pagenumber, x, y, w, h);
                b.Buffers.Add(bf);
                Statistics.CalcStatistics(bf);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public void UpdateCoords()
        {
            int z = 0;
            int c = 0;
            int t = 0;
            Coords = new int[SizeZ, SizeC, SizeT];
            for (int im = 0; im < Buffers.Count; im++)
            {
                ZCT co = new ZCT(z, c, t);
                Coords[co.Z, co.C, co.T] = im;
                Buffers[im].Coordinate = co;
                if (c < SizeC - 1)
                    c++;
                else
                {
                    c = 0;
                    if (z < SizeZ - 1)
                        z++;
                    else
                    {
                        z = 0;
                        if (t < SizeT - 1)
                            t++;
                        else
                            t = 0;
                    }
                }
            }
        }
        /// It takes the number of Z, C, and T planes in the image and then assigns each image buffer a
        /// coordinate in the ZCT space
        /// 
        /// @param sz number of z-slices
        /// @param sc number of channels
        /// @param st number of time points
        public void UpdateCoords(int sz, int sc, int st)
        {
            int z = 0;
            int c = 0;
            int t = 0;
            sizeZ = sz;
            sizeC = sc;
            sizeT = st;
            Coords = new int[sz, sc, st];
            for (int im = 0; im < Buffers.Count; im++)
            {
                ZCT co = new ZCT(z, c, t);
                Coords[co.Z, co.C, co.T] = im;
                Buffers[im].Coordinate = co;
                if (c < SizeC - 1)
                    c++;
                else
                {
                    c = 0;
                    if (z < SizeZ - 1)
                        z++;
                    else
                    {
                        z = 0;
                        if (t < SizeT - 1)
                            t++;
                        else
                            t = 0;
                    }
                }
            }
        }
        /// It takes a list of images and assigns them to a 3D array of coordinates
        /// 
        /// @param sz size of the Z dimension
        /// @param sc number of channels
        /// @param st number of time points
        /// @param order XYCZT or XYZCT
        public void UpdateCoords(int sz, int sc, int st, string order)
        {
            int z = 0;
            int c = 0;
            int t = 0;
            sizeZ = sz;
            sizeC = sc;
            sizeT = st;
            Coords = new int[sz, sc, st];
            if (order == "XYCZT")
            {
                for (int im = 0; im < Buffers.Count; im++)
                {
                    ZCT co = new ZCT(z, c, t);
                    Coords[co.Z, co.C, co.T] = im;
                    Buffers[im].Coordinate = co;
                    if (c < SizeC - 1)
                        c++;
                    else
                    {
                        c = 0;
                        if (z < SizeZ - 1)
                            z++;
                        else
                        {
                            z = 0;
                            if (t < SizeT - 1)
                                t++;
                            else
                                t = 0;
                        }
                    }
                }
            }
            else if (order == "XYZCT")
            {
                for (int im = 0; im < Buffers.Count; im++)
                {
                    ZCT co = new ZCT(z, c, t);
                    Coords[co.Z, co.C, co.T] = im;
                    Buffers[im].Coordinate = co;
                    if (z < SizeZ - 1)
                        z++;
                    else
                    {
                        z = 0;
                        if (c < SizeC - 1)
                            c++;
                        else
                        {
                            c = 0;
                            if (t < SizeT - 1)
                                t++;
                            else
                                t = 0;
                        }
                    }
                }
            }
        }

        /// Convert a physical distance to an image distance
        /// 
        /// @param d the distance in millimeters
        /// 
        /// @return The value of d divided by the physicalSizeX.
        public double ToImageSizeX(double d)
        {
            if (isPyramidal)
                return d;
            else
                return d / Resolutions[Level].PhysicalSizeX;
        }
        /// Convert a physical distance to an image distance
        /// 
        /// @param d the distance in millimeters
        /// 
        /// @return The image size in the Y direction.
        public double ToImageSizeY(double d)
        {
            if (isPyramidal)
                return d;
            else
                return d / Resolutions[Level].PhysicalSizeY;
        }
        /// > Convert a stage coordinate to an image coordinate
        /// 
        /// @param x the x coordinate of the point in the image
        /// 
        /// @return The return value is a double.
        public double ToImageSpaceX(double x)
        {
            if (isPyramidal)
                return x;
            else
                return (float)((x - StageSizeX) / Resolutions[Level].PhysicalSizeX);
        }
        /// > Convert a Y coordinate from stage space to image space
        /// 
        /// @param y the y coordinate of the point in the image
        /// 
        /// @return The return value is the y-coordinate of the image.
        public double ToImageSpaceY(double y)
        {
            if (isPyramidal)
                return y;
            else
                return (float)((y - StageSizeY) / Resolutions[Level].PhysicalSizeY);
        }
        /// > The function takes a point in the stage coordinate system and returns a point in the image
        /// coordinate system
        /// 
        /// @param PointD a class that contains two double values, X and Y.
        /// 
        /// @return A PointD object.
        public PointD ToImageSpace(PointD p)
        {
            return new PointD((float)ToImageSpaceX(p.X), (float)ToImageSpaceY(p.Y));
        }

        /// It takes a list of points in physical space and returns a list of points in image space
        /// 
        /// @param p List of points in stage space
        /// 
        /// @return A PointD array.
        public PointD[] ToImageSpace(List<PointD> p)
        {
            PointD[] ps = new PointD[p.Count];
            for (int i = 0; i < p.Count; i++)
            {
                ps[i] = ToImageSpace(p[i]);
            }
            return ps;
        }
        /// > The function takes a list of points in the stage coordinate system and returns a list of
        /// points in the image coordinate system
        /// 
        /// @param p the points to be converted
        /// 
        /// @return A PointF array.
        public PointF[] ToImageSpace(PointF[] p)
        {
            PointF[] ps = new PointF[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                PointF pp = new PointF();
                ps[i] = ToImageSpace(new PointD(p[i].X, p[i].Y)).ToPointF();
            }
            return ps;
        }
        /// > Convert a rectangle in physical space to a rectangle in image space
        /// 
        /// @param RectangleD 
        /// 
        /// @return A RectangleF object.
        public System.Drawing.RectangleF ToImageSpace(RectangleD p)
        {
            System.Drawing.RectangleF r = new RectangleF();
            System.Drawing.Point pp = new System.Drawing.Point();
            r.X = (int)((p.X - StageSizeX) / Resolutions[Level].PhysicalSizeX);
            r.Y = (int)((p.Y - StageSizeY) / Resolutions[Level].PhysicalSizeY);
            r.Width = (int)(p.W / Resolutions[Level].PhysicalSizeX);
            r.Height = (int)(p.H / Resolutions[Level].PhysicalSizeY);
            return r;
        }
        /// > The function takes a point in the volume space and returns a point in the stage space
        /// 
        /// @param PointD A struct that contains an X and Y value.
        /// 
        /// @return A PointD object.
        public PointD ToStageSpace(PointD p)
        {
            PointD pp = new PointD();
            pp.X = ((p.X * Resolutions[Level].PhysicalSizeX) + Volume.Location.X);
            pp.Y = ((p.Y * Resolutions[Level].PhysicalSizeY) + Volume.Location.Y);
            return pp;
        }
        /// > The function takes a point in the volume space and converts it to a point in the stage
        /// space
        /// 
        /// @param PointD A custom class that holds an X and Y coordinate.
        /// @param physicalSizeX The width of the stage in mm
        /// @param physicalSizeY The height of the stage in mm
        /// @param volumeX The X coordinate of the top left corner of the volume in stage space.
        /// @param volumeY The Y position of the top left corner of the volume in stage space.
        /// 
        /// @return A PointD object.
        public static PointD ToStageSpace(PointD p, double physicalSizeX, double physicalSizeY, double volumeX, double volumeY)
        {
            PointD pp = new PointD();
            pp.X = ((p.X * physicalSizeX) + volumeX);
            pp.Y = ((p.Y * physicalSizeY) + volumeY);
            return pp;
        }
        /// > Convert a rectangle from the coordinate space of the image to the coordinate space of the
        /// stage
        /// 
        /// @param RectangleD A rectangle with double precision coordinates.
        /// 
        /// @return A RectangleD object.
        public RectangleD ToStageSpace(RectangleD p)
        {
            RectangleD r = new RectangleD();
            r.X = ((p.X * PhysicalSizeX) + Volume.Location.X);
            r.Y = ((p.Y * PhysicalSizeY) + Volume.Location.Y);
            r.W = (p.W * Resolutions[Level].PhysicalSizeX);
            r.H = (p.H * Resolutions[Level].PhysicalSizeY);
            return r;
        }
        /// > This function takes a rectangle in the coordinate space of the image and converts it to the
        /// coordinate space of the stage
        /// 
        /// @param RectangleD A rectangle with double precision.
        /// @param physicalSizeX The width of the stage in pixels
        /// @param physicalSizeY The height of the stage in pixels
        /// @param volumeX The X position of the volume in stage space.
        /// @param volumeY The Y position of the top of the volume in stage space.
        /// 
        /// @return A RectangleD object.
        public static RectangleD ToStageSpace(RectangleD p, double physicalSizeX, double physicalSizeY, double volumeX, double volumeY)
        {
            RectangleD r = new RectangleD();
            r.X = ((p.X * physicalSizeX) + volumeX);
            r.Y = ((p.Y * physicalSizeY) + volumeY);
            r.W = (p.W * physicalSizeX);
            r.H = (p.H * physicalSizeY);
            return r;
        }
        /// > It takes a list of points in the coordinate system of the image and returns a list of
        /// points in the coordinate system of the stage
        /// 
        /// @param p The array of points to convert
        /// 
        /// @return A PointD[] array.
        public PointD[] ToStageSpace(PointD[] p)
        {
            PointD[] ps = new PointD[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                PointD pp = new PointD();
                pp.X = ((p[i].X * PhysicalSizeX) + Volume.Location.X);
                pp.Y = ((p[i].Y * PhysicalSizeY) + Volume.Location.Y);
                ps[i] = pp;
            }
            return ps;
        }
        /// It takes a list of points, and converts them from a coordinate system where the origin is in
        /// the center of the image, to a coordinate system where the origin is in the top left corner
        /// of the image
        /// 
        /// @param p the array of points to convert
        /// @param physicalSizeX The width of the image in microns
        /// @param physicalSizeY The height of the image in microns
        /// @param volumeX The X position of the volume in stage space.
        /// @param volumeY The Y position of the top left corner of the volume in stage space.
        /// 
        /// @return A PointD array.
        public static PointD[] ToStageSpace(PointD[] p, double physicalSizeX, double physicalSizeY, double volumeX, double volumeY)
        {
            PointD[] ps = new PointD[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                PointD pp = new PointD();
                pp.X = ((p[i].X * physicalSizeX) + volumeX);
                pp.Y = ((p[i].Y * physicalSizeY) + volumeY);
                ps[i] = pp;
            }
            return ps;
        }
        /// Convert a point in the image space to a point in the stage space
        /// 
        /// @param PointD A point in the image space
        /// @param resolution the resolution of the image (0, 1, 2, 3, 4)
        /// 
        /// @return A PointD object.
        public PointD ToStageSpace(PointD p, int resolution)
        {
            PointD pp = new PointD();
            pp.X = ((p.X * Resolutions[resolution].PhysicalSizeX) + Volume.Location.X);
            pp.Y = ((p.Y * Resolutions[resolution].PhysicalSizeY) + Volume.Location.Y);
            return pp;
        }
        public BioImage(string file)
        {
            id = file;
            filename = Images.GetImageName(id);
            Coordinate = new ZCT();
            rgbChannels[0] = 0;
            rgbChannels[1] = 0;
            rgbChannels[2] = 0;
        }
        /// It takes a BioImage object, and returns a new BioImage object that is a subset of the
        /// original
        /// 
        /// @param BioImage the image to be processed
        /// @param ser series number
        /// @param zs starting z-plane
        /// @param ze end of z-stack
        /// @param cs channel start
        /// @param ce channel end
        /// @param ts time start
        /// @param te time end
        /// 
        /// @return A new BioImage object.
        public static BioImage Substack(BioImage orig, int ser, int zs, int ze, int cs, int ce, int ts, int te)
        {
            BioImage b = CopyInfo(orig, false, false);
            b.ID = Images.GetImageName(orig.ID);
            int i = 0;
            b.Coords = new int[ze - zs, ce - cs, te - ts];
            b.sizeZ = ze - zs;
            b.sizeC = ce - cs;
            b.sizeT = te - ts;
            for (int ti = 0; ti < b.SizeT; ti++)
            {
                for (int zi = 0; zi < b.SizeZ; zi++)
                {
                    for (int ci = 0; ci < b.SizeC; ci++)
                    {
                        int ind = orig.Coords[zs + zi, cs + ci, ts + ti];
                        BufferInfo bf = new BufferInfo(Images.GetImageName(orig.id), orig.SizeX, orig.SizeY, orig.Buffers[0].PixelFormat, orig.Buffers[ind].Bytes, new ZCT(zi, ci, ti), i);
                        Statistics.CalcStatistics(bf);
                        b.Buffers.Add(bf);
                        b.Coords[zi, ci, ti] = i;
                        i++;
                    }
                }
            }
            for (int ci = cs; ci < ce; ci++)
            {
                b.Channels.Add(orig.Channels[ci]);
            }
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (b.Buffers[b.Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(b, false);
            if (b.bitsPerPixel > 8)
                b.StackThreshold(true);
            else
                b.StackThreshold(false);
            Images.AddImage(b, false);
            Recorder.AddLine("Bio.BioImage.Substack(" + '"' + orig.Filename + '"' + "," + ser + "," + zs + "," + ze + "," + cs + "," + ce + "," + ts + "," + te + ");");
            return b;
        }
        /// This function takes two images and merges them together
        /// 
        /// @param BioImage The image to be merged
        /// @param BioImage The image to be merged
        /// 
        /// @return A new BioImage object.
        public static BioImage MergeChannels(BioImage b2, BioImage b)
        {
            BioImage res = new BioImage(b2.ID);
            res.ID = Images.GetImageName(b2.ID);
            res.series = b2.series;
            res.sizeZ = b2.SizeZ;
            int cOrig = b2.SizeC;
            res.sizeC = b2.SizeC + b.SizeC;
            res.sizeT = b2.SizeT;
            res.bitsPerPixel = b2.bitsPerPixel;
            res.imageInfo = b2.imageInfo;
            res.littleEndian = b2.littleEndian;
            res.seriesCount = b2.seriesCount;
            res.imagesPerSeries = res.ImageCount / res.seriesCount;
            res.Coords = new int[res.SizeZ, res.SizeC, res.SizeT];

            int i = 0;
            int cc = 0;
            for (int ti = 0; ti < res.SizeT; ti++)
            {
                for (int zi = 0; zi < res.SizeZ; zi++)
                {
                    for (int ci = 0; ci < res.SizeC; ci++)
                    {
                        ZCT co = new ZCT(zi, ci, ti);
                        if (ci < cOrig)
                        {
                            //If this channel is part of the image b1 we add planes from it.
                            BufferInfo copy = new BufferInfo(b2.id, b2.SizeX, b2.SizeY, b2.Buffers[0].PixelFormat, b2.Buffers[i].Bytes, co, i);
                            if (b2.littleEndian)
                                copy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            res.Coords[zi, ci, ti] = i;
                            res.Buffers.Add(b2.Buffers[i]);
                            res.Buffers.Add(copy);
                            //Lets copy the ROI's from the original image.
                            List<ROI> anns = b2.GetAnnotations(zi, ci, ti);
                            if (anns.Count > 0)
                                res.Annotations.AddRange(anns);
                        }
                        else
                        {
                            //This plane is not part of b1 so we add the planes from b2 channels.
                            BufferInfo copy = new BufferInfo(b.id, b.SizeX, b.SizeY, b.Buffers[0].PixelFormat, b.Buffers[i].Bytes, co, i);
                            if (b2.littleEndian)
                                copy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            res.Coords[zi, ci, ti] = i;
                            res.Buffers.Add(b.Buffers[i]);
                            res.Buffers.Add(copy);

                            //Lets copy the ROI's from the original image.
                            List<ROI> anns = b.GetAnnotations(zi, cc, ti);
                            if (anns.Count > 0)
                                res.Annotations.AddRange(anns);
                        }
                        i++;
                    }
                }
            }
            for (int ci = 0; ci < res.SizeC; ci++)
            {
                if (ci < cOrig)
                {
                    res.Channels.Add(b2.Channels[ci].Copy());
                }
                else
                {
                    res.Channels.Add(b.Channels[cc].Copy());
                    res.Channels[cOrig + cc].Index = ci;
                    cc++;
                }
            }
            Images.AddImage(res, false);
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (res.Buffers[res.Buffers.Count - 1].Stats == null);
            AutoThreshold(res, false);
            if (res.bitsPerPixel > 8)
                res.StackThreshold(true);
            else
                res.StackThreshold(false);
            Recorder.AddLine("Bio.BioImage.MergeChannels(" + '"' + b.ID + '"' + "," + '"' + b2.ID + '"' + ");");
            return res;
        }
        /// MergeChannels(b, b2) takes two images, b and b2, and merges the channels of b2 into b
        /// 
        /// @param bname The name of the first image
        /// @param b2name The name of the image to be merged with the first image.
        /// 
        /// @return A BioImage object.
        public static BioImage MergeChannels(string bname, string b2name)
        {
            BioImage b = Images.GetImage(bname);
            BioImage b2 = Images.GetImage(b2name);
            return MergeChannels(b, b2);
        }
        /// It takes a 3D image and merges the Z-stack into a single 2D image
        /// 
        /// @param BioImage The image to be merged
        /// 
        /// @return A new BioImage object.
        public static BioImage MergeZ(BioImage b)
        {
            BioImage bi = BioImage.CopyInfo(b, true, true);
            int ind = 0;
            for (int c = 0; c < b.SizeC; c++)
            {
                for (int t = 0; t < b.sizeT; t++)
                {
                    Merge m = new Merge((Bitmap)b.Buffers[b.Coords[0, c, t]].Image);
                    Bitmap bm = new Bitmap(b.SizeX, b.SizeY, b.Buffers[0].PixelFormat);
                    for (int i = 1; i < b.sizeZ; i++)
                    {
                        m.OverlayImage = bm;
                        bm = m.Apply((Bitmap)b.Buffers[b.Coords[i, c, t]].Image);
                    }
                    BufferInfo bf = new BufferInfo(b.file, bm, new ZCT(0, c, t), ind);
                    bi.Buffers.Add(bf);
                    Statistics.CalcStatistics(bf);
                    ind++;
                }
            }
            Images.AddImage(bi, false);
            bi.UpdateCoords(1, b.SizeC, b.SizeT);
            bi.Coordinate = new ZCT(0, 0, 0);
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (bi.Buffers[bi.Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(bi, false);
            if (bi.bitsPerPixel > 8)
                bi.StackThreshold(true);
            else
                bi.StackThreshold(false);
            Recorder.AddLine("Bio.BioImage.MergeZ(" + '"' + b.ID + '"' + ");");
            return bi;
        }
        /// It takes a 3D image and merges the time dimension into a single image. 
        /// 
        /// @param BioImage The image to be processed
        /// 
        /// @return A new BioImage object.
        public static BioImage MergeT(BioImage b)
        {
            BioImage bi = BioImage.CopyInfo(b, true, true);
            int ind = 0;
            for (int c = 0; c < b.SizeC; c++)
            {
                for (int z = 0; z < b.sizeZ; z++)
                {
                    Merge m = new Merge((Bitmap)b.Buffers[b.Coords[z, c, 0]].Image);
                    Bitmap bm = new Bitmap(b.SizeX, b.SizeY, b.Buffers[0].PixelFormat);
                    for (int i = 1; i < b.sizeT; i++)
                    {
                        m.OverlayImage = bm;
                        bm = m.Apply((Bitmap)b.Buffers[b.Coords[z, c, i]].Image);
                    }
                    BufferInfo bf = new BufferInfo(b.file, bm, new ZCT(z, c, 0), ind);
                    bi.Buffers.Add(bf);
                    Statistics.CalcStatistics(bf);
                    ind++;
                }
            }
            Images.AddImage(bi, false);
            bi.UpdateCoords(1, b.SizeC, b.SizeT);
            bi.Coordinate = new ZCT(0, 0, 0);
            //We wait for threshold image statistics calculation
            do
            {
                Thread.Sleep(100);
            } while (bi.Buffers[bi.Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(bi, false);
            if (bi.bitsPerPixel > 8)
                bi.StackThreshold(true);
            else
                bi.StackThreshold(false);
            Recorder.AddLine("Bio.BioImage.MergeT(" + '"' + b.ID + '"' + ");");
            return bi;
        }
        /// It takes a single image and splits it into three separate images, one for each color channel
        /// 
        /// @return An array of BioImages.
        public BioImage[] SplitChannels()
        {
            BioImage[] bms;
            if (isRGB)
            {
                bms = new BioImage[3];
                BioImage ri = new BioImage(Path.GetFileNameWithoutExtension(ID) + "-1" + Path.GetExtension(ID));
                BioImage gi = new BioImage(Path.GetFileNameWithoutExtension(ID) + "-2" + Path.GetExtension(ID));
                BioImage bi = new BioImage(Path.GetFileNameWithoutExtension(ID) + "-3" + Path.GetExtension(ID));

                ri.sizeC = 1;
                gi.sizeC = 1;
                bi.sizeC = 1;
                ri.sizeZ = SizeZ;
                gi.sizeZ = SizeZ;
                bi.sizeZ = SizeZ;
                ri.sizeT = SizeT;
                gi.sizeT = SizeT;
                bi.sizeT = SizeT;

                ri.Coords = new int[SizeZ, 1, SizeT];
                gi.Coords = new int[SizeZ, 1, SizeT];
                bi.Coords = new int[SizeZ, 1, SizeT];
                int ind = 0;
                for (int i = 0; i < ImageCount; i++)
                {
                    if (Buffers[i].PixelFormat == PixelFormat.Format48bppRgb)
                    {
                        //For 48bit images we need to use our own function as AForge won't give us a proper image.
                        BufferInfo[] bfs = BufferInfo.RGB48To16(ID, SizeX, SizeY, Buffers[i].Stride, Buffers[i].Bytes, Buffers[i].Coordinate, ind, Buffers[i].Plane);
                        ind += 3;
                        ri.Buffers.Add(bfs[0]);
                        gi.Buffers.Add(bfs[1]);
                        bi.Buffers.Add(bfs[2]);
                        Statistics.CalcStatistics(bfs[0]);
                        Statistics.CalcStatistics(bfs[1]);
                        Statistics.CalcStatistics(bfs[2]);
                        ri.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;
                        gi.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;
                        bi.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;
                    }
                    else
                    {

                        Bitmap rImage = extractR.Apply((Bitmap)Buffers[i].Image);
                        BufferInfo rbf = new BufferInfo(ri.ID, rImage, Buffers[i].Coordinate, ind++);
                        Statistics.CalcStatistics(rbf);
                        ri.Buffers.Add(rbf);
                        ri.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;

                        Bitmap gImage = extractG.Apply((Bitmap)Buffers[i].Image);
                        BufferInfo gbf = new BufferInfo(gi.ID, gImage, Buffers[i].Coordinate, ind++);
                        Statistics.CalcStatistics(gbf);
                        gi.Buffers.Add(gbf);
                        gi.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;

                        Bitmap bImage = extractB.Apply((Bitmap)Buffers[i].Image);
                        //Clipboard.SetImage(bImage);
                        BufferInfo bbf = new BufferInfo(bi.ID, bImage, Buffers[i].Coordinate, ind++);
                        Statistics.CalcStatistics(bbf);
                        bi.Buffers.Add(bbf);
                        bi.Coords[Buffers[i].Coordinate.Z, Buffers[i].Coordinate.C, Buffers[i].Coordinate.T] = i;

                    }
                }
                //We wait for threshold image statistics calculation
                do
                {
                    Thread.Sleep(100);
                } while (bi.Buffers[bi.Buffers.Count - 1].Stats == null);

                ri.Channels.Add(Channels[0].Copy());
                gi.Channels.Add(Channels[0].Copy());
                bi.Channels.Add(Channels[0].Copy());
                AutoThreshold(ri, false);
                AutoThreshold(gi, false);
                AutoThreshold(bi, false);
                Images.AddImage(ri, true);
                Images.AddImage(gi, true);
                Images.AddImage(bi, true);
                Statistics.ClearCalcBuffer();
                bms[0] = ri;
                bms[1] = gi;
                bms[2] = bi;
            }
            else
            {
                bms = new BioImage[SizeC];
                for (int c = 0; c < SizeC; c++)
                {
                    BioImage b = BioImage.Substack(this, 0, 0, SizeZ, c, c + 1, 0, SizeT);
                    bms[c] = b;
                }
            }

            Statistics.ClearCalcBuffer();
            Recorder.AddLine("Bio.BioImage.SplitChannels(" + '"' + Filename + '"' + ");");
            return bms;
        }
        /// > SplitChannels splits a BioImage into its constituent channels
        /// 
        /// @param BioImage The image to split
        /// 
        /// @return An array of BioImages
        public static BioImage[] SplitChannels(BioImage bb)
        {
            return bb.SplitChannels();
        }
        /// This function takes an image and splits it into its individual channels
        /// 
        /// @param name The name of the image to split.
        /// 
        /// @return An array of BioImage objects.
        public static BioImage[] SplitChannels(string name)
        {
            return SplitChannels(Images.GetImage(name));
        }

        public static LevelsLinear filter8 = new LevelsLinear();
        public static LevelsLinear16bpp filter16 = new LevelsLinear16bpp();
        private static ExtractChannel extractR = new ExtractChannel(AForge.Imaging.RGB.R);
        private static ExtractChannel extractG = new ExtractChannel(AForge.Imaging.RGB.G);
        private static ExtractChannel extractB = new ExtractChannel(AForge.Imaging.RGB.B);

        /// It returns an image from a buffer based on the z, c, and t coordinates
        /// 
        /// @param z the z-stack index
        /// @param c channel
        /// @param t time
        /// 
        /// @return The image at the specified coordinates.
        public Image GetImageByCoord(int z, int c, int t)
        {
            return Buffers[Coords[z, c, t]].ImageRGB;
        }
        /// > Get the bitmap from the buffer at the given coordinates
        /// 
        /// @param z the z-stack index
        /// @param c channel
        /// @param t time
        /// 
        /// @return A Bitmap object.
        public Bitmap GetBitmap(int z, int c, int t)
        {
            return (Bitmap)Buffers[Coords[z, c, t]].Image;
        }
        /// Get index of pixel in the buffer.
        /// @param ix x coordinate of the pixel
        /// @param iy The y coordinate of the pixel to get the index of.
        /// 
        /// @return The index of the pixel in the array.
        public int GetIndex(int ix, int iy)
        {
            if (ix > SizeX || iy > SizeY || ix < 0 || iy < 0)
                return 0;
            int stridex = SizeX;
            int x = ix;
            int y = iy;
            if (bitsPerPixel > 8)
            {
                return (y * stridex + x) * 2;
            }
            else
            {
                return (y * stridex + x);
            }
        }
        /// Get index of pixel in the buffer.
        /// @param ix x coordinate of the pixel
        /// @param iy The y coordinate of the pixel
        /// @param index 0 = Red, 1 = Green, 2 = Blue
        /// 
        /// @return The index of the pixel in the buffer.
        public int GetIndexRGB(int ix, int iy, int index)
        {
            int stridex = SizeX;
            //For 16bit (2*8bit) images we multiply buffer index by 2
            int x = ix;
            int y = iy;
            if (bitsPerPixel > 8)
            {
                return (y * stridex + x) * 2 * index;
            }
            else
            {
                return (y * stridex + x) * index;
            }
        }
        /// > This function returns the value of a pixel at a given coordinate
        /// 
        /// @param ZCTXY a struct that contains the X, Y, Z, C, and T coordinates of the pixel.
        /// 
        /// @return The value of the pixel at the given coordinate.
        public ushort GetValue(ZCTXY coord)
        {
            if (coord.X < 0 || coord.Y < 0 || coord.X > SizeX || coord.Y > SizeY)
            {
                return 0;
            }
            if (isRGB)
            {
                if (coord.C == 0)
                    return GetValueRGB(coord, 0);
                else if (coord.C == 1)
                    return GetValueRGB(coord, 1);
                else if (coord.C == 2)
                    return GetValueRGB(coord, 2);
            }
            else
                return GetValueRGB(coord, 0);
            return 0;
        }
        /// > Get the value of the pixel at the given coordinates in the given buffer
        /// 
        /// @param ZCTXY a struct that contains the Z, C, T, X, and Y coordinates of the pixel.
        /// @param index 0, 1, 2
        /// 
        /// @return A ushort value.
        public ushort GetValueRGB(ZCTXY coord, int index)
        {
            int ind = Coords[coord.Z, coord.C, coord.T];
            ColorS c = Buffers[ind].GetPixel(coord.X, coord.Y);
            if (index == 0)
                return c.R;
            else
            if (index == 1)
                return c.G;
            else
            if (index == 2)
                return c.B;
            throw new IndexOutOfRangeException();
        }
        /// > Get the value of the pixel at the given coordinates
        /// 
        /// @param ZCT Z is the Z-plane, C is the channel, T is the timepoint
        /// @param x x coordinate of the pixel
        /// @param y The y coordinate of the pixel
        /// 
        /// @return The value of the pixel at the given coordinates.
        public ushort GetValue(ZCT coord, int x, int y)
        {
            return GetValueRGB(new ZCTXY(coord.Z, coord.C, coord.T, x, y), 0);
        }
        /// > This function returns the value of the pixel at the specified ZCTXY coordinates
        /// 
        /// @param z The Z-plane of the image.
        /// @param c channel
        /// @param t time
        /// @param x x coordinate of the pixel
        /// @param y the y coordinate of the pixel
        /// 
        /// @return The value of the pixel at the given coordinates.
        public ushort GetValue(int z, int c, int t, int x, int y)
        {
            return GetValue(new ZCTXY(z, c, t, x, y));
        }
        /// > Get the value of a pixel at a given coordinate
        /// 
        /// @param ZCT The ZCT coordinate of the image.
        /// @param x x coordinate of the pixel
        /// @param y the y coordinate of the pixel
        /// @param RGBindex 0 = Red, 1 = Green, 2 = Blue
        /// 
        /// @return The value of the pixel at the given coordinates.
        public ushort GetValueRGB(ZCT coord, int x, int y, int RGBindex)
        {
            ZCTXY c = new ZCTXY(coord.Z, coord.C, coord.T, x, y);
            if (isRGB)
            {
                return GetValueRGB(c, RGBindex);
            }
            else
                return GetValue(coord, x, y);
        }
        /// This function returns the value of the pixel at the specified Z, C, T, X, Y, and RGBindex
        /// 
        /// @param z The Z-plane index
        /// @param c channel
        /// @param t time index
        /// @param x x coordinate of the pixel
        /// @param y The y coordinate of the pixel
        /// @param RGBindex 0 = Red, 1 = Green, 2 = Blue
        /// 
        /// @return The value of the pixel at the given coordinates.
        public ushort GetValueRGB(int z, int c, int t, int x, int y, int RGBindex)
        {
            return GetValueRGB(new ZCT(z, c, t), x, y, RGBindex);
        }
        /// It takes a coordinate and a value, and sets the value at that coordinate
        /// 
        /// @param ZCTXY a struct that contains the Z, C, T, X, and Y coordinates of the pixel.
        /// @param value The value to set the pixel to.
        public void SetValue(ZCTXY coord, ushort value)
        {
            int i = Coords[coord.Z, coord.C, coord.T];
            Buffers[i].SetValue(coord.X, coord.Y, value);
        }
        /// It sets the value of a pixel in a buffer
        /// 
        /// @param x The x coordinate of the pixel to set.
        /// @param y The y coordinate of the pixel to set.
        /// @param ind The index of the buffer to set the value in.
        /// @param value The value to set the pixel to.
        public void SetValue(int x, int y, int ind, ushort value)
        {
            Buffers[ind].SetValue(x, y, value);
        }
        /// This function sets the value of a pixel at a given x,y coordinate in a given image plane
        /// 
        /// @param x x coordinate of the pixel
        /// @param y The y coordinate of the pixel to set.
        /// @param ZCT a struct that contains the Z, C, and T coordinates of the pixel
        /// @param value the value to set
        public void SetValue(int x, int y, ZCT coord, ushort value)
        {
            SetValue(x, y, Coords[coord.Z, coord.C, coord.T], value);
        }
        /// It takes a coordinate and a value and sets the value at that coordinate in the buffer
        /// 
        /// @param ZCTXY a struct that contains the Z, C, T, X, and Y coordinates of the pixel
        /// @param RGBindex 0 = Red, 1 = Green, 2 = Blue
        /// @param value the value to be set
        public void SetValueRGB(ZCTXY coord, int RGBindex, ushort value)
        {
            int ind = Coords[coord.Z, coord.C, coord.T];
            Buffers[ind].SetValueRGB(coord.X, coord.Y, RGBindex, value);
        }
        /// > This function returns a Bitmap object from the image data stored in the OME-TIFF file
        /// 
        /// @param ZCT Z = Z-stack, C = channel, T = timepoint
        /// 
        /// @return A Bitmap object.
        public Bitmap GetBitmap(ZCT coord)
        {
            return (Bitmap)GetImageByCoord(coord.Z, coord.C, coord.T);
        }
        /// > Get the image at the specified ZCT coordinate, and return a filtered version of it
        /// 
        /// @param ZCT Z is the Z-stack, C is the channel, T is the timepoint
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A bitmap.
        public Bitmap GetFiltered(ZCT coord, IntRange r, IntRange g, IntRange b)
        {
            int index = Coords[coord.Z, coord.C, coord.T];
            return GetFiltered(index, r, g, b);
        }
        /// It takes a range of RGB values and returns a filtered image
        /// 
        /// @param ind the index of the buffer to get the filtered image from
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A Bitmap
        public Bitmap GetFiltered(int ind, IntRange r, IntRange g, IntRange b)
        {
            if (Buffers[ind].BitsPerPixel > 8)
            {
                BioImage.filter16.InRed = r;
                BioImage.filter16.InGreen = g;
                BioImage.filter16.InBlue = b;
                return BioImage.filter16.Apply((Bitmap)Buffers[ind].Image);
            }
            else
            {
                // set ranges
                BioImage.filter8.InRed = r;
                BioImage.filter8.InGreen = g;
                BioImage.filter8.InBlue = b;
                //We give the filter an RGB image (ImageRGB) instead of Image as with some padded 8-bit images
                //AForge will return an invalid managed Bitmap which causes a crash due to faulty image properties.
                return BioImage.filter8.Apply((Bitmap)Buffers[ind].ImageRGB);
            }
        }
        /// It takes an image, and returns a single channel of that image
        /// 
        /// @param ind the index of the buffer
        /// @param RGB enum with R, G, B
        /// 
        /// @return A Bitmap
        public Bitmap GetChannelImage(int ind, RGB rGB)
        {
            BufferInfo bf = Buffers[ind];
            if (bf.isRGB)
            {
                if (rGB == RGB.R)
                    return extractR.Apply((Bitmap)Buffers[ind].Image);
                else
                if (rGB == RGB.G)
                    return extractG.Apply((Bitmap)Buffers[ind].Image);
                else
                    return extractB.Apply((Bitmap)Buffers[ind].Image);
            }
            else
                throw new InvalidOperationException();
        }
        /// It takes a ZCT coordinate and returns a bitmap of the emission image at that coordinate
        /// 
        /// @param ZCT Z, C, T coordinates
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A bitmap.
        public Bitmap GetEmission(ZCT coord, IntRange rf, IntRange gf, IntRange bf)
        {
            if (RGBChannelCount == 1)
            {
                BufferInfo[] bs = new BufferInfo[Channels.Count];
                for (int c = 0; c < Channels.Count; c++)
                {
                    int index = Coords[coord.Z, c, coord.T];
                    bs[c] = Buffers[index];
                }
                return BufferInfo.GetEmissionBitmap(bs, Channels.ToArray());
            }
            else
            {
                int index = Coords[coord.Z, coord.C, coord.T];
                return (Bitmap)Buffers[index].Image;
            }
        }
        /// > Get the RGB bitmap for the specified ZCT coordinate
        /// 
        /// @param ZCT a 3-tuple of integers (z, c, t)
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A bitmap.
        public Bitmap GetRGBBitmap(ZCT coord, IntRange rf, IntRange gf, IntRange bf)
        {
            int index = Coords[coord.Z, coord.C, coord.T];
            if (Buffers[0].RGBChannelsCount == 1)
            {
                if (Channels.Count >= 3)
                {
                    BufferInfo[] bs = new BufferInfo[3];
                    bs[0] = Buffers[index + RChannel.Index];
                    bs[1] = Buffers[index + GChannel.Index];
                    bs[2] = Buffers[index + BChannel.Index];
                    return BufferInfo.GetRGBBitmap(bs, rf, gf, bf);
                }
                else
                {
                    BufferInfo[] bs = new BufferInfo[3];
                    bs[0] = Buffers[index + RChannel.Index];
                    bs[1] = Buffers[index + RChannel.Index + 1];
                    bs[2] = Buffers[index + RChannel.Index + 2];
                    return BufferInfo.GetRGBBitmap(bs, rf, gf, bf);
                }
            }
            else
                return (Bitmap)Buffers[index].Image;
        }

        public static Stopwatch swatch = new Stopwatch();
        /// > GetAnnotations() returns a list of ROI objects that are associated with the ZCT coordinate
        /// passed in as a parameter
        /// 
        /// @param ZCT a 3D coordinate (Z, C, T)
        /// 
        /// @return A list of ROI objects.
        public List<ROI> GetAnnotations(ZCT coord)
        {
            List<ROI> annotations = new List<ROI>();
            foreach (ROI an in Annotations)
            {
                if (an == null)
                    continue;
                if (an.coord == coord)
                    annotations.Add(an);
            }
            return annotations;
        }
        /// This function returns a list of ROI objects that have the same Z, C, and T coordinates as
        /// the input parameters
        /// 
        /// @param Z The Z-plane of the image
        /// @param C Channel
        /// @param T Time
        /// 
        /// @return A list of ROI objects.
        public List<ROI> GetAnnotations(int Z, int C, int T)
        {
            List<ROI> annotations = new List<ROI>();
            foreach (ROI an in Annotations)
            {
                if (an.coord.Z == Z && an.coord.Z == Z && an.coord.C == C && an.coord.T == T)
                    annotations.Add(an);
            }
            return annotations;
        }

        public bool Loading = false;
        /// We initialize OME on a seperate thread so the user doesn't have to wait for initialization to
        /// view images.
        public static void Initialize()
        {
            //We initialize OME on a seperate thread so the user doesn't have to wait for initialization to
            //view images. 
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(InitOME));
            t.Start();
        }
        /// > Initialize the OME-XML library
        private static void InitOME()
        {
            factory = new ServiceFactory();
            service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            reader = new ImageReader();
            writer = new ImageWriter();
            initialized = true;
        }
        /// This function takes a string array of file names and a string ID and saves the files to the
        /// database
        /// 
        /// @param file The file path of the file to be saved.
        /// @param ID The ID of the series you want to save.
        public static void SaveFile(string file, string ID)
        {
            string[] sts = new string[1];
            sts[0] = file;
            SaveSeries(sts, ID);
        }
        /// It takes a list of image files, and saves them as a multi-page TIFF file
        /// 
        /// @param files An array of file paths to the images to be saved.
        /// @param ID The file name of the output file.
        public static void SaveSeries(string[] files, string ID)
        {
            string desc = "";
            int stride = 0;
            ImageJDesc j = new ImageJDesc();
            BioImage bi = Images.GetImage(files[0]);
            j.FromImage(bi);
            desc = j.GetString();
            for (int fi = 0; fi < files.Length; fi++)
            {
                string file = files[fi];

                BioImage b = Images.GetImage(file);
                string fn = Path.GetFileNameWithoutExtension(ID);
                string dir = Path.GetDirectoryName(ID);
                stride = b.Buffers[0].Stride;

                //Save ROIs to CSV file.
                if (b.Annotations.Count > 0)
                {
                    string f = dir + "//" + fn + ".csv";
                    ExportROIsCSV(f, b.Annotations);
                }

                //Embed ROI's to image description.
                for (int i = 0; i < b.Annotations.Count; i++)
                {
                    desc += "-ROI:" + b.series + ":" + ROIToString(b.Annotations[i]) + NewLine;
                }
                foreach (Channel c in b.Channels)
                {
                    string cj = JsonConvert.SerializeObject(c.info, Formatting.None);
                    desc += "-Channel:" + fi + ":" + cj + NewLine;
                }
                string json = JsonConvert.SerializeObject(b.imageInfo, Formatting.None);
                desc += "-ImageInfo:" + fi + ":" + json + NewLine;
            }

            Tiff image = Tiff.Open(ID, "w");
            for (int fi = 0; fi < files.Length; fi++)
            {
                int im = 0;
                string file = files[fi];
                Progress pr = new Progress(file, "Saving");
                pr.Show();
                Application.DoEvents();
                BioImage b = Images.GetImage(file);
                int sizec = 1;
                if (!b.isRGB)
                {
                    sizec = b.SizeC;
                }
                byte[] buffer;
                for (int c = 0; c < sizec; c++)
                {
                    for (int z = 0; z < b.SizeZ; z++)
                    {
                        for (int t = 0; t < b.SizeT; t++)
                        {
                            image.SetDirectory((short)(im + (b.Buffers.Count * fi)));
                            image.SetField(TiffTag.IMAGEWIDTH, b.SizeX);
                            image.SetField(TiffTag.IMAGEDESCRIPTION, desc);
                            image.SetField(TiffTag.IMAGELENGTH, b.SizeY);
                            image.SetField(TiffTag.BITSPERSAMPLE, b.bitsPerPixel);
                            image.SetField(TiffTag.SAMPLESPERPIXEL, b.RGBChannelCount);
                            image.SetField(TiffTag.ROWSPERSTRIP, b.SizeY);
                            image.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                            image.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                            image.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                            image.SetField(TiffTag.ROWSPERSTRIP, image.DefaultStripSize(0));
                            if (b.PhysicalSizeX != -1 && b.PhysicalSizeY != -1)
                            {
                                image.SetField(TiffTag.XRESOLUTION, (b.PhysicalSizeX * b.SizeX) / ((b.PhysicalSizeX * b.SizeX) * b.PhysicalSizeX));
                                image.SetField(TiffTag.YRESOLUTION, (b.PhysicalSizeY * b.SizeY) / ((b.PhysicalSizeY * b.SizeY) * b.PhysicalSizeY));
                                image.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.NONE);
                            }
                            else
                            {
                                image.SetField(TiffTag.XRESOLUTION, 100.0);
                                image.SetField(TiffTag.YRESOLUTION, 100.0);
                                image.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                            }
                            // specify that it's a page within the multipage file
                            image.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                            // specify the page number
                            buffer = b.Buffers[im].GetSaveBytes(true);
                            image.SetField(TiffTag.PAGENUMBER, im + (b.Buffers.Count * fi), b.Buffers.Count * files.Length);
                            for (int i = 0, offset = 0; i < b.SizeY; i++)
                            {
                                image.WriteScanline(buffer, offset, i, 0);
                                offset += stride;
                            }
                            image.WriteDirectory();
                            pr.UpdateProgressF((float)im / (float)b.ImageCount);
                            Application.DoEvents();
                            im++;
                        }
                    }
                }
                pr.Close();
            }
            image.Dispose();

        }
        /// It opens a tiff file, reads the number of pages, reads the number of channels, and then
        /// reads each page into a BioImage object
        /// 
        /// @param file the path to the file
        /// 
        /// @return An array of BioImage objects.
        public static BioImage[] OpenSeries(string file)
        {
            Tiff image = Tiff.Open(file, "r");
            int pages = image.NumberOfDirectories();
            FieldValue[] f = image.GetField(TiffTag.IMAGEDESCRIPTION);
            int sp = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
            ImageJDesc imDesc = new ImageJDesc();
            int count = 1;
            if (f != null)
            {
                string desc = f[0].ToString();
                if (desc.StartsWith("ImageJ"))
                {
                    imDesc.SetString(desc);
                    if (imDesc.channels != 0)
                        count = imDesc.channels;
                }
            }
            int scount = (pages * sp) / count;
            BioImage[] bs = new BioImage[pages];
            image.Close();
            for (int i = 0; i < pages; i++)
            {
                bs[i] = OpenFile(file, i, true);
            }
            return bs;
        }
        /// This function opens a file and returns a BioImage object
        /// 
        /// @param file The path to the file you want to open.
        /// 
        /// @return A BioImage object.
        public static BioImage OpenFile(string file)
        {
            return OpenFile(file, 0, true);
        }
        static bool IsTiffTiled(string imagePath)
        {
            if (imagePath.EndsWith(".tif") || imagePath.EndsWith(".tiff"))
            {
                using (Tiff tiff = Tiff.Open(imagePath, "r"))
                {
                    if (tiff == null)
                    {
                        throw new Exception("Failed to open TIFF image.");
                    }
                    bool t = tiff.IsTiled();
                    tiff.Close();
                    return t;
                }
            }
            else return false;
        }
        static void InitDirectoryResolution(BioImage b, Tiff image, ImageJDesc jdesc = null)
        {
            Resolution res = new Resolution();
            FieldValue[] rs = image.GetField(TiffTag.RESOLUTIONUNIT);
            string unit = "NONE";
            if (rs != null)
                unit = rs[0].ToString();
            if (jdesc != null)
                res.PhysicalSizeZ = jdesc.finterval;
            else
                res.PhysicalSizeZ = 2.54 / 96;
            if (unit == "CENTIMETER")
            {
                if (image.GetField(TiffTag.XRESOLUTION) != null)
                {
                    double x = image.GetField(TiffTag.XRESOLUTION)[0].ToDouble();
                    res.PhysicalSizeX = (1000 / x);
                }
                if (image.GetField(TiffTag.YRESOLUTION) != null)
                {
                    double y = image.GetField(TiffTag.YRESOLUTION)[0].ToDouble();
                    res.PhysicalSizeY = (1000 / y);
                }
            }
            else
            if (unit == "INCH")
            {
                if (image.GetField(TiffTag.XRESOLUTION) != null)
                {
                    double x = image.GetField(TiffTag.XRESOLUTION)[0].ToDouble();
                    res.PhysicalSizeX = (2.54 / x) / 2.54;
                }
                if (image.GetField(TiffTag.YRESOLUTION) != null)
                {
                    double y = image.GetField(TiffTag.YRESOLUTION)[0].ToDouble();
                    res.PhysicalSizeY = (2.54 / y) / 2.54;
                }
            }
            else
            if (unit == "NONE")
            {
                if (jdesc != null)
                {
                    if (jdesc.unit == "micron")
                    {
                        if (image.GetField(TiffTag.XRESOLUTION) != null)
                        {
                            double x = image.GetField(TiffTag.XRESOLUTION)[0].ToDouble();
                            res.PhysicalSizeX = (2.54 / x) / 2.54;
                        }
                        if (image.GetField(TiffTag.YRESOLUTION) != null)
                        {
                            double y = image.GetField(TiffTag.YRESOLUTION)[0].ToDouble();
                            res.PhysicalSizeY = (2.54 / y) / 2.54;
                        }
                    }
                }
                else
                {
                    res.PhysicalSizeX = 2.54 / 96;
                    res.PhysicalSizeY = 2.54 / 96;
                    res.PhysicalSizeZ = 2.54 / 96;
                }
            }
            res.SizeX = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            res.SizeY = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            int bitsPerPixel = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            int RGBChannelCount = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
            res.PixelFormat = GetPixelFormat(RGBChannelCount, bitsPerPixel);
            b.Resolutions.Add(res);
        }
        /// It opens a file, reads the metadata, reads the image data, and then calculates the image
        /// statistics
        /// 
        /// @param file The file to open
        /// @param series The series number of the image to open.
        /// 
        /// @return A BioImage object
        public static BioImage OpenFile(string file, int series, bool newTab)
        {
            return OpenFile(file, series, newTab, true, false, 0, 0, 1920, 1080);
        }
        /// It opens a file, reads the metadata, reads the image data, and then calculates the image
        /// statistics
        /// 
        /// @param file The file to open
        /// @param series The series number of the image to open.
        /// 
        /// @return A BioImage object
        public static BioImage OpenFile(string file, int series, bool newTab, bool addToImages, bool tile, int x, int y, int w, int h)
        {
            if (isOME(file))
            {
                return OpenOME(file);
            }
            vips = VipsSupport(file);
            Stopwatch st = new Stopwatch();
            st.Start();
            Progress pr = new Progress(file, "Opening");
            pr.Show();
            Application.DoEvents();
            BioImage b = new BioImage(file);
            bool tiled = IsTiffTiled(file);
            if (tiled)
            {
                b.Type = ImageType.pyramidal;
            }
            b.series = series;
            b.file = file;
            string fn = Path.GetFileNameWithoutExtension(file);
            string dir = Path.GetDirectoryName(file);
            if (File.Exists(fn + ".csv"))
            {
                string f = dir + "//" + fn + ".csv";
                b.Annotations = BioImage.ImportROIsCSV(f);
            }
            if (file.EndsWith("tif") || file.EndsWith("tiff") || file.EndsWith("TIF") || file.EndsWith("TIFF"))
            {
                Tiff image = Tiff.Open(file, "r");
                for (int d = 0; d < image.NumberOfDirectories(); d++)
                {
                    image.SetDirectory((short)d);
                    int SizeX = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    int SizeY = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    b.bitsPerPixel = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                    b.littleEndian = image.IsBigEndian();
                    int RGBChannelCount = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
                    string desc = "";

                    FieldValue[] f = image.GetField(TiffTag.IMAGEDESCRIPTION);
                    ImageJDesc imDesc = new ImageJDesc();
                    b.sizeC = 1;
                    b.sizeT = 1;
                    b.sizeZ = 1;
                    if (f != null)
                    {
                        desc = f[0].ToString();
                        if (desc.StartsWith("ImageJ"))
                        {
                            imDesc.SetString(desc);
                            if (imDesc.channels != 0)
                                b.sizeC = imDesc.channels;
                            else
                                b.sizeC = 1;
                            if (imDesc.slices != 0)
                                b.sizeZ = imDesc.slices;
                            else
                                b.sizeZ = 1;
                            if (imDesc.frames != 0)
                                b.sizeT = imDesc.frames;
                            else
                                b.sizeT = 1;
                            if (imDesc.finterval != 0)
                                b.frameInterval = imDesc.finterval;
                            else
                                b.frameInterval = 1;
                            if (imDesc.spacing != 0)
                                b.imageInfo.PhysicalSizeZ = imDesc.spacing;
                            else
                                b.imageInfo.PhysicalSizeZ = 1;
                        }
                    }
                    int stride = 0;
                    PixelFormat PixelFormat;
                    if (RGBChannelCount == 1)
                    {
                        if (b.bitsPerPixel > 8)
                        {
                            PixelFormat = PixelFormat.Format16bppGrayScale;
                            stride = SizeX * 2;
                        }
                        else
                        {
                            PixelFormat = PixelFormat.Format8bppIndexed;
                            stride = SizeX;
                        }
                    }
                    else
                    if (RGBChannelCount == 3)
                    {
                        b.sizeC = 1;
                        if (b.bitsPerPixel > 8)
                        {
                            PixelFormat = PixelFormat.Format48bppRgb;
                            stride = SizeX * 2 * 3;
                        }
                        else
                        {
                            PixelFormat = PixelFormat.Format24bppRgb;
                            stride = SizeX * 3;
                        }
                    }
                    else
                    {
                        PixelFormat = PixelFormat.Format32bppArgb;
                        stride = SizeX * 4;
                    }

                    string[] sts = desc.Split('\n');
                    int index = 0;
                    for (int i = 0; i < sts.Length; i++)
                    {
                        if (sts[i].StartsWith("-Channel"))
                        {
                            string val = sts[i].Substring(9);
                            val = val.Substring(0, val.IndexOf(':'));
                            int serie = int.Parse(val);
                            if (serie == series && sts[i].Length > 7)
                            {
                                string cht = sts[i].Substring(sts[i].IndexOf('{'), sts[i].Length - sts[i].IndexOf('{'));
                                Channel.ChannelInfo info = JsonConvert.DeserializeObject<Channel.ChannelInfo>(cht);
                                Channel ch = new Channel(index, b.bitsPerPixel, info.samplesPerPixel);
                                ch.info = info;
                                b.Channels.Add(ch);
                                index++;
                            }
                        }
                        else
                        if (sts[i].StartsWith("-ROI"))
                        {
                            string val = sts[i].Substring(5);
                            val = val.Substring(0, val.IndexOf(':'));
                            int serie = int.Parse(val);
                            if (serie == series && sts[i].Length > 7)
                            {
                                string s = sts[i].Substring(sts[i].IndexOf("ROI:") + 4, sts[i].Length - (sts[i].IndexOf("ROI:") + 4));
                                string ro = s.Substring(s.IndexOf(":") + 1, s.Length - (s.IndexOf(':') + 1));
                                ROI roi = StringToROI(ro);
                                b.Annotations.Add(roi);
                            }
                        }
                        else
                        if (sts[i].StartsWith("-ImageInfo"))
                        {
                            string val = sts[i].Substring(11);
                            val = val.Substring(0, val.IndexOf(':'));
                            int serie = int.Parse(val);
                            if (serie == series && sts[i].Length > 10)
                            {
                                string cht = sts[i].Substring(sts[i].IndexOf('{'), sts[i].Length - sts[i].IndexOf('{'));
                                b.imageInfo = JsonConvert.DeserializeObject<ImageInfo>(cht);
                            }

                        }
                    }

                    InitDirectoryResolution(b, image, imDesc);

                    b.Coords = new int[b.SizeZ, b.SizeC, b.SizeT];

                    //If this is a tiff file not made by Bio we init channels based on RGBChannels.
                    if (b.Channels.Count == 0)
                        b.Channels.Add(new Channel(0, b.bitsPerPixel, RGBChannelCount));

                    //Lets check to see the channels are correctly defined in this file
                    for (int ch = 0; ch < b.Channels.Count; ch++)
                    {
                        if (b.Channels[ch].SamplesPerPixel != RGBChannelCount)
                        {
                            b.Channels[ch].SamplesPerPixel = RGBChannelCount;
                        }
                    }

                    int z = 0;
                    int c = 0;
                    int t = 0;
                    b.Buffers = new List<BufferInfo>();

                    if (vips && tiled)
                    {
                        OpenVip(b, d, x, y, w, h);
                    }
                    else
                    {
                        int pages = image.NumberOfDirectories() / b.seriesCount;
                        //int stride = image.ScanlineSize();
                        int str = image.ScanlineSize();
                        //If calculated stride and image scanline size is not the same it means the image is written in planes
                        bool planes = false;
                        if (stride != str)
                            planes = true;
                        if (planes)
                        {
                            BufferInfo[] bfs = new BufferInfo[3];
                            for (int pl = 0; pl < 3; pl++)
                            {
                                byte[] bytes = new byte[str * SizeY];
                                for (int im = 0, offset = 0; im < SizeY; im++)
                                {
                                    image.ReadScanline(bytes, offset, im, (short)pl);
                                    offset += str;
                                }
                                if (b.bitsPerPixel > 8)
                                    bfs[pl] = new BufferInfo(file, SizeX, SizeY, PixelFormat.Format16bppGrayScale, bytes, new ZCT(0, 0, 0), d, b.littleEndian, true);
                                else
                                    bfs[pl] = new BufferInfo(file, SizeX, SizeY, PixelFormat.Format8bppIndexed, bytes, new ZCT(0, 0, 0), d, b.littleEndian, true);
                            }
                            BufferInfo bf;
                            if (b.bitsPerPixel > 8)
                                bf = BufferInfo.RGB16To48(bfs);
                            else
                                bf = BufferInfo.RGB8To24(bfs);
                            bf.SwitchRedBlue();
                            Statistics.CalcStatistics(bf);
                            b.Buffers.Add(bf);
                        }
                        else
                        {
                            byte[] bytes = new byte[stride * SizeY];
                            for (int im = 0, offset = 0; im < SizeY; im++)
                            {
                                image.ReadScanline(bytes, offset, im, 0);
                                offset += stride;
                            }
                            BufferInfo inf = new BufferInfo(file, SizeX, SizeY, PixelFormat, bytes, new ZCT(0, 0, 0), d, b.littleEndian, true);
                            if (inf.PixelFormat == PixelFormat.Format48bppRgb)
                                inf.SwitchRedBlue();
                            b.Buffers.Add(inf);
                            Statistics.CalcStatistics(inf);
                        }
                        pr.UpdateProgressF((float)((double)d / (double)(series + 1) * pages));
                        Application.DoEvents();
                    }

                }
                image.Close();
                b.UpdateCoords();
            }
            else
            {
                b.bitsPerPixel = 8;
                b.littleEndian = BitConverter.IsLittleEndian;
                b.sizeZ = 1;
                b.sizeC = 1;
                b.sizeT = 1;
                BufferInfo inf = null;
                //We use a try block incase this is an OME file
                try
                {
                    Image im = Image.FromFile(file);
                    b.imageInfo.PhysicalSizeZ = 1;
                    b.Resolutions.Add(new Resolution(im.Width, im.Height, im.PixelFormat, 2.54 / im.HorizontalResolution, 2.54 / im.VerticalResolution, 1, 0, 0, 0));
                    inf = new BufferInfo(file, Image.FromFile(file), new ZCT(0, 0, 0), 0);
                    Statistics.CalcStatistics(inf);
                }
                catch (Exception)
                {
                    b.Dispose();
                    return OpenOME(file);
                }
                b.Buffers.Add(inf);
                Channel ch = new Channel(0, 8, b.RGBChannelCount);
                b.Channels.Add(ch);
                b.Coords = new int[b.SizeZ, b.SizeC, b.sizeT];
                b.imageInfo.Series = 0;

            }
            //We wait for histogram image statistics calculation
            do
            {
            } while (b.Buffers[b.Buffers.Count - 1].Stats == null);

            Statistics.ClearCalcBuffer();
            AutoThreshold(b, false);
            if (b.bitsPerPixel > 8)
                b.StackThreshold(true);
            else
                b.StackThreshold(false);
            Recorder.AddLine("Bio.BioImage.Open(" + '"' + file + '"' + ");");
            Images.AddImage(b,newTab);
            pr.Close();
            pr.Dispose();
            st.Stop();
            return b;
        }
        /// > The function checks if the image is a tiff image and if it is, it checks if the image is a
        /// series of images
        /// 
        /// @param file the path to the file
        /// 
        /// @return a boolean value.
        public static bool isTiffSeries(string file)
        {
            Tiff image = Tiff.Open(file, "r");
            string desc = "";
            FieldValue[] f = image.GetField(TiffTag.IMAGEDESCRIPTION);
            image.Close();
            string[] sts = desc.Split('\n');
            int index = 0;
            for (int i = 0; i < sts.Length; i++)
            {
                if (sts[i].StartsWith("-ImageInfo"))
                {
                    string val = sts[i].Substring(11);
                    val = val.Substring(0, val.IndexOf(':'));
                    int serie = int.Parse(val);
                    if (sts[i].Length > 10)
                    {
                        string cht = sts[i].Substring(sts[i].IndexOf('{'), sts[i].Length - sts[i].IndexOf('{'));
                        ImageInfo info = JsonConvert.DeserializeObject<ImageInfo>(cht);
                        if (info.Series > 1)
                            return true;
                        else
                            return false;
                    }
                }
            }
            return false;
        }
        /// If the file is a TIFF, check the image description for the OME XML. If it's not a TIFF,
        /// assume it's OME
        /// 
        /// @param file the file path of the image
        /// 
        /// @return A boolean value.
        public static bool isOME(string file)
        {
            if (file.EndsWith(".ome.tif") || file.EndsWith(".OME.TIF"))
                return true;
            if (file.EndsWith(".tif") || file.EndsWith(".TIF") || file.EndsWith("tiff") || file.EndsWith("TIFF"))
            {
                Tiff image = Tiff.Open(file, "r");
                string desc = "";
                FieldValue[] f = image.GetField(TiffTag.IMAGEDESCRIPTION);
                image.Close();
                if (desc.Contains("<OME"))
                    return true;
                else
                    return false;
            }
            if (!(file.EndsWith("png") || file.EndsWith("PNG") || file.EndsWith("jpg") || file.EndsWith("JPG") ||
                file.EndsWith("jpeg") || file.EndsWith("JPEG") || file.EndsWith("bmp") || file.EndsWith("BMP")))
            {
                return true;
            }
            else return false;
        }
        /// > If the file is an OME file and has more than one series, then it is a series
        /// 
        /// @param file the file path to the image
        /// 
        /// @return A boolean value.
        public static bool isOMESeries(string file)
        {
            if (!isOME(file))
                return false;
            ImageReader reader = new ImageReader();
            var meta = (IMetadata)((OMEXMLService)new ServiceFactory().getInstance(typeof(OMEXMLService))).createOMEXMLMetadata();
            reader.setMetadataStore((MetadataStore)meta);
            file = file.Replace("\\", "/");
            reader.setId(file);
            bool ser = false;
            if (reader.getSeriesCount() > 1)
                ser = true;
            reader.close();
            reader = null;
            return ser;
        }
        /// This function takes a string array of IDs and a file name and saves the OME-TIFF file
        /// 
        /// @param file the file to save to
        /// @param ID The ID of the image to be saved
        public static void SaveOME(string file, string ID)
        {
            string[] sts = new string[1];
            sts[0] = ID;
            SaveOMESeries(sts, file, Properties.Settings.Default.Planes);
        }
        /// It saves a series of images to a file, and adds OME-XML metadata to the file
        /// 
        /// @param files an array of file paths to the images to be saved
        /// @param f the file name to save to
        /// @param planes if true, the planes will be saved as well.
        public static void SaveOMESeries(string[] files, string f, bool planes)
        {
            if (File.Exists(f))
                File.Delete(f);
            loci.formats.meta.IMetadata omexml = service.createOMEXMLMetadata();
            for (int fi = 0; fi < files.Length; fi++)
            {
                int serie = fi;
                string file = files[fi];
                BioImage b = Images.GetImage(file);
                // create OME-XML metadata store

                omexml.setImageID("Image:" + serie, serie);
                omexml.setPixelsID("Pixels:" + serie, serie);
                omexml.setPixelsInterleaved(java.lang.Boolean.TRUE, serie);
                omexml.setPixelsDimensionOrder(ome.xml.model.enums.DimensionOrder.XYCZT, serie);
                if (b.bitsPerPixel > 8)
                    omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT16, serie);
                else
                    omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT8, serie);
                omexml.setPixelsSizeX(new PositiveInteger(java.lang.Integer.valueOf(b.SizeX)), serie);
                omexml.setPixelsSizeY(new PositiveInteger(java.lang.Integer.valueOf(b.SizeY)), serie);
                omexml.setPixelsSizeZ(new PositiveInteger(java.lang.Integer.valueOf(b.SizeZ)), serie);
                int samples = 1;
                if (b.isRGB)
                    samples = 3;
                omexml.setPixelsSizeC(new PositiveInteger(java.lang.Integer.valueOf(b.SizeC)), serie);
                omexml.setPixelsSizeT(new PositiveInteger(java.lang.Integer.valueOf(b.SizeT)), serie);
                if (BitConverter.IsLittleEndian)
                    omexml.setPixelsBigEndian(java.lang.Boolean.FALSE, serie);
                else
                    omexml.setPixelsBigEndian(java.lang.Boolean.TRUE, serie);
                ome.units.quantity.Length p1 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeX), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeX(p1, serie);
                ome.units.quantity.Length p2 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeY), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeY(p2, serie);
                ome.units.quantity.Length p3 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeZ), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeZ(p3, serie);
                ome.units.quantity.Length s1 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.X), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelX(s1, serie);
                ome.units.quantity.Length s2 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.Y), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelY(s2, serie);
                ome.units.quantity.Length s3 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.Z), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelZ(s3, serie);
                omexml.setStageLabelName("StageLabel:" + serie, serie);

                for (int channel = 0; channel < b.Channels.Count; channel++)
                {
                    Channel c = b.Channels[channel];
                    for (int r = 0; r < c.range.Length; r++)
                    {
                        omexml.setChannelID("Channel:" + channel + ":" + serie, serie, channel + r);
                        omexml.setChannelSamplesPerPixel(new PositiveInteger(java.lang.Integer.valueOf(1)), serie, channel + r);
                        if (c.LightSourceWavelength != 0)
                        {
                            omexml.setChannelLightSourceSettingsID("LightSourceSettings:" + channel, serie, channel + r);
                            ome.units.quantity.Length lw = new ome.units.quantity.Length(java.lang.Double.valueOf(c.LightSourceWavelength), ome.units.UNITS.NANOMETER);
                            omexml.setChannelLightSourceSettingsWavelength(lw, serie, channel + r);
                            omexml.setChannelLightSourceSettingsAttenuation(PercentFraction.valueOf(c.LightSourceAttenuation), serie, channel + r);
                        }
                        omexml.setChannelName(c.Name, serie, channel + r);
                        if (c.Color != null)
                        {
                            ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(c.Color.Value.R, c.Color.Value.G, c.Color.Value.B, c.Color.Value.A);
                            omexml.setChannelColor(col, serie, channel + r);
                        }
                        if (c.Emission != 0)
                        {
                            ome.units.quantity.Length em = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Emission), ome.units.UNITS.NANOMETER);
                            omexml.setChannelEmissionWavelength(em, serie, channel + r);
                            ome.units.quantity.Length ex = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Excitation), ome.units.UNITS.NANOMETER);
                            omexml.setChannelExcitationWavelength(ex, serie, channel + r);
                        }
                        omexml.setChannelContrastMethod(c.ContrastMethod, serie, channel + r);
                        omexml.setChannelFluor(c.Fluor, serie, channel + r);
                        omexml.setChannelIlluminationType(c.IlluminationType, serie, channel + r);

                        if (c.LightSourceIntensity != 0)
                        {
                            ome.units.quantity.Power pw = new ome.units.quantity.Power(java.lang.Double.valueOf(c.LightSourceIntensity), ome.units.UNITS.VOLT);
                            omexml.setLightEmittingDiodePower(pw, serie, channel + r);
                            omexml.setLightEmittingDiodeID(c.DiodeName, serie, channel + r);
                        }
                        if (c.AcquisitionMode != null)
                            omexml.setChannelAcquisitionMode(c.AcquisitionMode, serie, channel + r);
                    }
                }

                int i = 0;
                foreach (ROI an in b.Annotations)
                {
                    if (an.roiID == "")
                        omexml.setROIID("ROI:" + i.ToString() + ":" + serie, i);
                    else
                        omexml.setROIID(an.roiID, i);
                    omexml.setROIName(an.roiName, i);
                    if (an.type == ROI.Type.Point)
                    {
                        if (an.id != "")
                            omexml.setPointID(an.id, i, serie);
                        else
                            omexml.setPointID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setPointX(java.lang.Double.valueOf(b.ToImageSpaceX(an.X)), i, serie);
                        omexml.setPointY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Y)), i, serie);
                        omexml.setPointTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setPointTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setPointTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setPointText(an.Text, i, serie);
                        else
                            omexml.setPointText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setPointFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setPointStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setPointStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setPointFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Polygon || an.type == ROI.Type.Freeform)
                    {
                        if (an.id != "")
                            omexml.setPolygonID(an.id, i, serie);
                        else
                            omexml.setPolygonID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setPolygonPoints(an.PointsToString(b), i, serie);
                        omexml.setPolygonTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setPolygonTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setPolygonTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setPolygonText(an.Text, i, serie);
                        else
                            omexml.setPolygonText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setPolygonFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setPolygonStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setPolygonStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setPolygonFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Rectangle)
                    {
                        if (an.id != "")
                            omexml.setRectangleID(an.id, i, serie);
                        else
                            omexml.setRectangleID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setRectangleWidth(java.lang.Double.valueOf(b.ToImageSizeX(an.W)), i, serie);
                        omexml.setRectangleHeight(java.lang.Double.valueOf(b.ToImageSizeY(an.H)), i, serie);
                        omexml.setRectangleX(java.lang.Double.valueOf(b.ToImageSpaceX(an.Rect.X)), i, serie);
                        omexml.setRectangleY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Rect.Y)), i, serie);
                        omexml.setRectangleTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setRectangleTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setRectangleTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        omexml.setRectangleText(i.ToString(), i, serie);
                        if (an.Text != "")
                            omexml.setRectangleText(an.Text, i, serie);
                        else
                            omexml.setRectangleText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setRectangleFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setRectangleStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setRectangleStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setRectangleFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Line)
                    {
                        if (an.id != "")
                            omexml.setLineID(an.id, i, serie);
                        else
                            omexml.setLineID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setLineX1(java.lang.Double.valueOf(b.ToImageSpaceX(an.GetPoint(0).X)), i, serie);
                        omexml.setLineY1(java.lang.Double.valueOf(b.ToImageSpaceY(an.GetPoint(0).Y)), i, serie);
                        omexml.setLineX2(java.lang.Double.valueOf(b.ToImageSpaceX(an.GetPoint(1).X)), i, serie);
                        omexml.setLineY2(java.lang.Double.valueOf(b.ToImageSpaceY(an.GetPoint(1).Y)), i, serie);
                        omexml.setLineTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setLineTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setLineTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setLineText(an.Text, i, serie);
                        else
                            omexml.setLineText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setLineFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setLineStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setLineStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setLineFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Ellipse)
                    {

                        if (an.id != "")
                            omexml.setEllipseID(an.id, i, serie);
                        else
                            omexml.setEllipseID("Shape:" + i + ":" + serie, i, serie);
                        //We need to change System.Drawing.Rectangle to ellipse radius;
                        double w = (double)an.W / 2;
                        double h = (double)an.H / 2;
                        omexml.setEllipseRadiusX(java.lang.Double.valueOf(b.ToImageSizeX(w)), i, serie);
                        omexml.setEllipseRadiusY(java.lang.Double.valueOf(b.ToImageSizeY(h)), i, serie);

                        double x = an.Point.X + w;
                        double y = an.Point.Y + h;
                        omexml.setEllipseX(java.lang.Double.valueOf(b.ToImageSpaceX(x)), i, serie);
                        omexml.setEllipseY(java.lang.Double.valueOf(b.ToImageSpaceX(y)), i, serie);
                        omexml.setEllipseTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setEllipseTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setEllipseTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setEllipseText(an.Text, i, serie);
                        else
                            omexml.setEllipseText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setEllipseFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setEllipseStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setEllipseStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setEllipseFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Label)
                    {
                        if (an.id != "")
                            omexml.setLabelID(an.id, i, serie);
                        else
                            omexml.setLabelID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setLabelX(java.lang.Double.valueOf(b.ToImageSpaceX(an.Rect.X)), i, serie);
                        omexml.setLabelY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Rect.Y)), i, serie);
                        omexml.setLabelTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setLabelTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setLabelTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        omexml.setLabelText(i.ToString(), i, serie);
                        if (an.Text != "")
                            omexml.setLabelText(an.Text, i, serie);
                        else
                            omexml.setLabelText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setLabelFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setLabelStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setLabelStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setLabelFillColor(colf, i, serie);
                    }
                    i++;
                }
                if (b.Buffers[0].Plane != null && planes)
                    for (int bu = 0; bu < b.Buffers.Count; bu++)
                    {
                        //Correct order of parameters.
                        if (b.Buffers[bu].Plane.Delta != 0)
                        {
                            ome.units.quantity.Time t = new ome.units.quantity.Time(java.lang.Double.valueOf(b.Buffers[bu].Plane.Delta), ome.units.UNITS.MILLISECOND);
                            omexml.setPlaneDeltaT(t, serie, bu);
                        }
                        if (b.Buffers[bu].Plane.Exposure != 0)
                        {
                            ome.units.quantity.Time et = new ome.units.quantity.Time(java.lang.Double.valueOf(b.Buffers[bu].Plane.Exposure), ome.units.UNITS.MILLISECOND);
                            omexml.setPlaneExposureTime(et, serie, bu);
                        }
                        ome.units.quantity.Length lx = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Buffers[bu].Plane.Location.X), ome.units.UNITS.MICROMETER);
                        ome.units.quantity.Length ly = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Buffers[bu].Plane.Location.Y), ome.units.UNITS.MICROMETER);
                        ome.units.quantity.Length lz = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Buffers[bu].Plane.Location.Z), ome.units.UNITS.MICROMETER);
                        omexml.setPlanePositionX(lx, serie, bu);
                        omexml.setPlanePositionY(ly, serie, bu);
                        omexml.setPlanePositionZ(lz, serie, bu);
                        omexml.setPlaneTheC(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.C)), serie, bu);
                        omexml.setPlaneTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.Z)), serie, bu);
                        omexml.setPlaneTheT(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.T)), serie, bu);

                        omexml.setTiffDataPlaneCount(new NonNegativeInteger(java.lang.Integer.valueOf(1)), serie, bu);
                        omexml.setTiffDataIFD(new NonNegativeInteger(java.lang.Integer.valueOf(bu)), serie, bu);
                        omexml.setTiffDataFirstC(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.C)), serie, bu);
                        omexml.setTiffDataFirstZ(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.Z)), serie, bu);
                        omexml.setTiffDataFirstT(new NonNegativeInteger(java.lang.Integer.valueOf(b.Buffers[bu].Plane.Coordinate.T)), serie, bu);

                    }

            }
            writer.setMetadataRetrieve(omexml);
            f = f.Replace("\\", "/");
            writer.setId(f);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                Progress pr = new Progress(file, "Saving");
                pr.Show();
                BioImage b = Images.GetImage(files[i]);
                writer.setSeries(i);
                for (int bu = 0; bu < b.Buffers.Count; bu++)
                {
                    writer.saveBytes(bu, b.Buffers[bu].GetSaveBytes(BitConverter.IsLittleEndian));
                    pr.UpdateProgressF((float)bu / b.Buffers.Count);
                    Application.DoEvents();
                }
                pr.Close();
                pr.Dispose();
            }
            bool stop = false;
            do
            {
                try
                {
                    writer.close();
                    stop = true;
                }
                catch (Exception e)
                {
                    Scripting.LogLine(e.Message);
                }

            } while (!stop);
        }
        public static void SaveOMESeries(BioImage[] bms, string f, bool planes)
        {
            List<string> sts = new List<string>();
            foreach (BioImage b in bms)
            {
                sts.Add(b.ID);
            }
            SaveOMESeries(sts.ToArray(), f, planes);
        }
        public static void SaveOMEStiched(BioImage[] bms, string file, string compression)
        {
            if (File.Exists(file))
                File.Delete(file);
            loci.formats.meta.IMetadata omexml = service.createOMEXMLMetadata();
            //We need to go through the images and find the ones belonging to each resolution.
            //As well we need to determine the dimensions of the tiles.
            Dictionary<double, List<BioImage>> bis = new Dictionary<double, List<BioImage>>();
            Dictionary<double, Point3D> min = new Dictionary<double, Point3D>();
            Dictionary<double, Point3D> max = new Dictionary<double, Point3D>();
            for (int i = 0; i < bms.Length; i++)
            {
                Resolution r = bms[i].Resolutions[bms[i].Level];
                if (bis.ContainsKey(r.PhysicalSizeX))
                {
                    bis[r.PhysicalSizeX].Add(bms[i]);
                    if (bms[i].StageSizeX < min[r.PhysicalSizeX].X || bms[i].StageSizeY < min[r.PhysicalSizeX].Y)
                    {
                        min[r.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                    if (bms[i].StageSizeX > max[r.PhysicalSizeX].X || bms[i].StageSizeY > max[r.PhysicalSizeX].Y)
                    {
                        max[r.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                }
                else
                {
                    bis.Add(r.PhysicalSizeX, new List<BioImage>());
                    min.Add(r.PhysicalSizeX, new Point3D(bms[i].StageSizeX, bms[i].StageSizeY, bms[i].StageSizeZ));
                    max.Add(r.PhysicalSizeX, new Point3D(bms[i].StageSizeX, bms[i].StageSizeY, bms[i].StageSizeZ));
                    bis[r.PhysicalSizeX].Add(bms[i]);
                }
            }
            int s = 0;
            foreach (double px in bis.Keys)
            {
                Resolution rr = bis[px][0].Resolutions[bis[px][0].Level];
                int xi = 1 + (int)Math.Ceiling((max[px].X - min[px].X) / rr.VolumeWidth);
                int yi = 1 + (int)Math.Ceiling((max[px].Y - min[px].Y) / rr.VolumeHeight);
                BioImage b = bis[px][0];
                int serie = s;
                // create OME-XML metadata store.
                omexml.setImageID("Image:" + serie, serie);
                omexml.setPixelsID("Pixels:" + serie, serie);
                omexml.setPixelsInterleaved(java.lang.Boolean.TRUE, serie);
                omexml.setPixelsDimensionOrder(ome.xml.model.enums.DimensionOrder.XYCZT, serie);
                if (b.bitsPerPixel > 8)
                    omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT16, serie);
                else
                    omexml.setPixelsType(ome.xml.model.enums.PixelType.UINT8, serie);
                omexml.setPixelsSizeX(new PositiveInteger(java.lang.Integer.valueOf(b.SizeX * xi)), serie);
                omexml.setPixelsSizeY(new PositiveInteger(java.lang.Integer.valueOf(b.SizeY * yi)), serie);
                omexml.setPixelsSizeZ(new PositiveInteger(java.lang.Integer.valueOf(b.SizeZ)), serie);
                int samples = 1;
                if (b.isRGB)
                    samples = 3;
                omexml.setPixelsSizeC(new PositiveInteger(java.lang.Integer.valueOf(b.SizeC)), serie);
                omexml.setPixelsSizeT(new PositiveInteger(java.lang.Integer.valueOf(b.SizeT)), serie);
                if (BitConverter.IsLittleEndian)
                    omexml.setPixelsBigEndian(java.lang.Boolean.FALSE, serie);
                else
                    omexml.setPixelsBigEndian(java.lang.Boolean.TRUE, serie);
                ome.units.quantity.Length p1 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeX), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeX(p1, serie);
                ome.units.quantity.Length p2 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeY), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeY(p2, serie);
                ome.units.quantity.Length p3 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.PhysicalSizeZ), ome.units.UNITS.MICROMETER);
                omexml.setPixelsPhysicalSizeZ(p3, serie);
                ome.units.quantity.Length s1 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.X), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelX(s1, serie);
                ome.units.quantity.Length s2 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.Y), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelY(s2, serie);
                ome.units.quantity.Length s3 = new ome.units.quantity.Length(java.lang.Double.valueOf(b.Volume.Location.Z), ome.units.UNITS.MICROMETER);
                omexml.setStageLabelZ(s3, serie);
                omexml.setStageLabelName("StageLabel:" + serie, serie);

                for (int channel = 0; channel < b.Channels.Count; channel++)
                {
                    Channel c = b.Channels[channel];
                    for (int r = 0; r < c.range.Length; r++)
                    {
                        omexml.setChannelID("Channel:" + channel + ":" + serie, serie, channel + r);
                        omexml.setChannelSamplesPerPixel(new PositiveInteger(java.lang.Integer.valueOf(1)), serie, channel + r);
                        if (c.LightSourceWavelength != 0)
                        {
                            omexml.setChannelLightSourceSettingsID("LightSourceSettings:" + channel, serie, channel + r);
                            ome.units.quantity.Length lw = new ome.units.quantity.Length(java.lang.Double.valueOf(c.LightSourceWavelength), ome.units.UNITS.NANOMETER);
                            omexml.setChannelLightSourceSettingsWavelength(lw, serie, channel + r);
                            omexml.setChannelLightSourceSettingsAttenuation(PercentFraction.valueOf(c.LightSourceAttenuation), serie, channel + r);
                        }
                        omexml.setChannelName(c.Name, serie, channel + r);
                        if (c.Color != null)
                        {
                            ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(c.Color.Value.R, c.Color.Value.G, c.Color.Value.B, c.Color.Value.A);
                            omexml.setChannelColor(col, serie, channel + r);
                        }
                        if (c.Emission != 0)
                        {
                            ome.units.quantity.Length em = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Emission), ome.units.UNITS.NANOMETER);
                            omexml.setChannelEmissionWavelength(em, serie, channel + r);
                            ome.units.quantity.Length ex = new ome.units.quantity.Length(java.lang.Double.valueOf(c.Excitation), ome.units.UNITS.NANOMETER);
                            omexml.setChannelExcitationWavelength(ex, serie, channel + r);
                        }
                        omexml.setChannelContrastMethod(c.ContrastMethod, serie, channel + r);
                        omexml.setChannelFluor(c.Fluor, serie, channel + r);
                        omexml.setChannelIlluminationType(c.IlluminationType, serie, channel + r);

                        if (c.LightSourceIntensity != 0)
                        {
                            ome.units.quantity.Power pw = new ome.units.quantity.Power(java.lang.Double.valueOf(c.LightSourceIntensity), ome.units.UNITS.VOLT);
                            omexml.setLightEmittingDiodePower(pw, serie, channel + r);
                            omexml.setLightEmittingDiodeID(c.DiodeName, serie, channel + r);
                        }
                        if (c.AcquisitionMode != null)
                            omexml.setChannelAcquisitionMode(c.AcquisitionMode, serie, channel + r);
                    }
                }

                int i = 0;
                foreach (ROI an in b.Annotations)
                {
                    if (an.roiID == "")
                        omexml.setROIID("ROI:" + i.ToString() + ":" + serie, i);
                    else
                        omexml.setROIID(an.roiID, i);
                    omexml.setROIName(an.roiName, i);
                    if (an.type == ROI.Type.Point)
                    {
                        if (an.id != "")
                            omexml.setPointID(an.id, i, serie);
                        else
                            omexml.setPointID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setPointX(java.lang.Double.valueOf(b.ToImageSpaceX(an.X)), i, serie);
                        omexml.setPointY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Y)), i, serie);
                        omexml.setPointTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setPointTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setPointTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setPointText(an.Text, i, serie);
                        else
                            omexml.setPointText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setPointFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setPointStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setPointStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setPointFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Polygon || an.type == ROI.Type.Freeform)
                    {
                        if (an.id != "")
                            omexml.setPolygonID(an.id, i, serie);
                        else
                            omexml.setPolygonID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setPolygonPoints(an.PointsToString(b), i, serie);
                        omexml.setPolygonTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setPolygonTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setPolygonTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setPolygonText(an.Text, i, serie);
                        else
                            omexml.setPolygonText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setPolygonFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setPolygonStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setPolygonStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setPolygonFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Rectangle)
                    {
                        if (an.id != "")
                            omexml.setRectangleID(an.id, i, serie);
                        else
                            omexml.setRectangleID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setRectangleWidth(java.lang.Double.valueOf(b.ToImageSizeX(an.W)), i, serie);
                        omexml.setRectangleHeight(java.lang.Double.valueOf(b.ToImageSizeY(an.H)), i, serie);
                        omexml.setRectangleX(java.lang.Double.valueOf(b.ToImageSpaceX(an.Rect.X)), i, serie);
                        omexml.setRectangleY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Rect.Y)), i, serie);
                        omexml.setRectangleTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setRectangleTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setRectangleTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        omexml.setRectangleText(i.ToString(), i, serie);
                        if (an.Text != "")
                            omexml.setRectangleText(an.Text, i, serie);
                        else
                            omexml.setRectangleText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setRectangleFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setRectangleStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setRectangleStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setRectangleFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Line)
                    {
                        if (an.id != "")
                            omexml.setLineID(an.id, i, serie);
                        else
                            omexml.setLineID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setLineX1(java.lang.Double.valueOf(b.ToImageSpaceX(an.GetPoint(0).X)), i, serie);
                        omexml.setLineY1(java.lang.Double.valueOf(b.ToImageSpaceY(an.GetPoint(0).Y)), i, serie);
                        omexml.setLineX2(java.lang.Double.valueOf(b.ToImageSpaceX(an.GetPoint(1).X)), i, serie);
                        omexml.setLineY2(java.lang.Double.valueOf(b.ToImageSpaceY(an.GetPoint(1).Y)), i, serie);
                        omexml.setLineTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setLineTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setLineTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setLineText(an.Text, i, serie);
                        else
                            omexml.setLineText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setLineFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setLineStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setLineStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setLineFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Ellipse)
                    {

                        if (an.id != "")
                            omexml.setEllipseID(an.id, i, serie);
                        else
                            omexml.setEllipseID("Shape:" + i + ":" + serie, i, serie);
                        //We need to change System.Drawing.Rectangle to ellipse radius;
                        double w = (double)an.W / 2;
                        double h = (double)an.H / 2;
                        omexml.setEllipseRadiusX(java.lang.Double.valueOf(b.ToImageSizeX(w)), i, serie);
                        omexml.setEllipseRadiusY(java.lang.Double.valueOf(b.ToImageSizeY(h)), i, serie);

                        double x = an.Point.X + w;
                        double y = an.Point.Y + h;
                        omexml.setEllipseX(java.lang.Double.valueOf(b.ToImageSpaceX(x)), i, serie);
                        omexml.setEllipseY(java.lang.Double.valueOf(b.ToImageSpaceX(y)), i, serie);
                        omexml.setEllipseTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setEllipseTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setEllipseTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        if (an.Text != "")
                            omexml.setEllipseText(an.Text, i, serie);
                        else
                            omexml.setEllipseText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setEllipseFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setEllipseStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setEllipseStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setEllipseFillColor(colf, i, serie);
                    }
                    else
                    if (an.type == ROI.Type.Label)
                    {
                        if (an.id != "")
                            omexml.setLabelID(an.id, i, serie);
                        else
                            omexml.setLabelID("Shape:" + i + ":" + serie, i, serie);
                        omexml.setLabelX(java.lang.Double.valueOf(b.ToImageSpaceX(an.Rect.X)), i, serie);
                        omexml.setLabelY(java.lang.Double.valueOf(b.ToImageSpaceY(an.Rect.Y)), i, serie);
                        omexml.setLabelTheZ(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.Z)), i, serie);
                        omexml.setLabelTheC(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.C)), i, serie);
                        omexml.setLabelTheT(new NonNegativeInteger(java.lang.Integer.valueOf(an.coord.T)), i, serie);
                        omexml.setLabelText(i.ToString(), i, serie);
                        if (an.Text != "")
                            omexml.setLabelText(an.Text, i, serie);
                        else
                            omexml.setLabelText(i.ToString(), i, serie);
                        ome.units.quantity.Length fl = new ome.units.quantity.Length(java.lang.Double.valueOf(an.font.Size), ome.units.UNITS.PIXEL);
                        omexml.setLabelFontSize(fl, i, serie);
                        ome.xml.model.primitives.Color col = new ome.xml.model.primitives.Color(an.strokeColor.R, an.strokeColor.G, an.strokeColor.B, an.strokeColor.A);
                        omexml.setLabelStrokeColor(col, i, serie);
                        ome.units.quantity.Length sw = new ome.units.quantity.Length(java.lang.Double.valueOf(an.strokeWidth), ome.units.UNITS.PIXEL);
                        omexml.setLabelStrokeWidth(sw, i, serie);
                        ome.xml.model.primitives.Color colf = new ome.xml.model.primitives.Color(an.fillColor.R, an.fillColor.G, an.fillColor.B, an.fillColor.A);
                        omexml.setLabelFillColor(colf, i, serie);
                    }
                    i++;
                }
                s++;
            }
            writer.setMetadataRetrieve(omexml);
            file = file.Replace("\\", "/");
            writer.setId(file);
            writer.setCompression(compression);
            int xx; int yy;
            Progress pr = new Progress(file, "Saving");
            pr.Show();
            s = 0;
            foreach (double px in bis.Keys)
            {
                writer.setSeries(s);
                PointD p = new PointD(max[px].X - min[px].X, max[px].Y - min[px].Y);
                for (int i = 0; i < bis[px].Count; i++)
                {
                    BioImage b = bis[px][i];
                    writer.setTileSizeX(b.SizeX);
                    writer.setTileSizeY(b.SizeY);
                    Resolution r = bis[px][i].Resolutions[bis[px][i].Level];
                    double dx = Math.Ceiling((bis[px][i].StageSizeX - min[px].X) / r.VolumeWidth);
                    double dy = Math.Ceiling((bis[px][i].StageSizeY - min[px].Y) / r.VolumeHeight);
                    for (int bu = 0; bu < b.Buffers.Count; bu++)
                    {
                        byte[] bt = b.Buffers[bu].GetSaveBytes(BitConverter.IsLittleEndian);
                        writer.saveBytes(bu, bt, (int)dx * b.SizeX, (int)dy * b.SizeY, b.SizeX, b.SizeY);
                        Application.DoEvents();
                    }
                    pr.UpdateProgressF((float)i / bis[px].Count);

                }
            }
            pr.Close();
            pr.Dispose();
            bool stop = false;
            do
            {
                try
                {
                    writer.close();
                    stop = true;
                }
                catch (Exception e)
                {
                    Scripting.LogLine(e.Message);
                }
            } while (!stop);
        }

        /// <summary>
        /// The function "GetBands" returns the number of color bands for a given pixel format in the
        /// AForge library.
        /// </summary>
        /// <param name="PixelFormat">The PixelFormat parameter is an enumeration that represents the
        /// format of a pixel in an image. It specifies the number of bits per pixel and the color space
        /// used by the pixel. The PixelFormat enumeration is defined in the AForge.Imaging
        /// namespace.</param>
        /// <returns>
        /// The method is returning the number of color bands for a given pixel format.
        /// </returns>
        private static int GetBands(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format8bppIndexed: return 1;
                case PixelFormat.Format16bppGrayScale: return 1;
                case PixelFormat.Format24bppRgb: return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
                case PixelFormat.Format48bppRgb:
                    return 3;
                default:
                    throw new NotSupportedException($"Unsupported pixel format: {format}");
            }
        }
        /// <summary>
        /// The function `SaveOMEPyramidal` saves a collection of BioImages as a pyramidal OME-TIFF
        /// file.
        /// </summary>
        /// <param name="bms">An array of BioImage objects representing the images to be saved.</param>
        /// <param name="file">The `file` parameter is a string that represents the file path where the
        /// OME Pyramidal TIFF file will be saved.</param>
        /// <param name="compression">The `compression` parameter is of type
        /// `Enums.ForeignTiffCompression` and it specifies the compression method to be used when
        /// saving the TIFF file.</param>
        public static void SaveOMEPyramidal(BioImage[] bms, string file, Enums.ForeignTiffCompression compression, int compressionLevel)
        {
            if (File.Exists(file))
                File.Delete(file);
            //We need to go through the images and find the ones belonging to each resolution.
            //As well we need to determine the dimensions of the tiles.
            Dictionary<double, List<BioImage>> bis = new Dictionary<double, List<BioImage>>();
            Dictionary<double, Point3D> min = new Dictionary<double, Point3D>();
            Dictionary<double, Point3D> max = new Dictionary<double, Point3D>();
            for (int i = 0; i < bms.Length; i++)
            {
                Resolution res = bms[i].Resolutions[bms[i].Level];
                if (bis.ContainsKey(res.PhysicalSizeX))
                {
                    bis[res.PhysicalSizeX].Add(bms[i]);
                    if (bms[i].StageSizeX < min[res.PhysicalSizeX].X || bms[i].StageSizeY < min[res.PhysicalSizeX].Y)
                    {
                        min[res.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                    if (bms[i].StageSizeX > max[res.PhysicalSizeX].X || bms[i].StageSizeY > max[res.PhysicalSizeX].Y)
                    {
                        max[res.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                }
                else
                {
                    bis.Add(res.PhysicalSizeX, new List<BioImage>());
                    min.Add(res.PhysicalSizeX, new Point3D(double.MaxValue, double.MaxValue, double.MaxValue));
                    max.Add(res.PhysicalSizeX, new Point3D(double.MinValue, double.MinValue, double.MinValue));
                    if (bms[i].StageSizeX < min[res.PhysicalSizeX].X || bms[i].StageSizeY < min[res.PhysicalSizeX].Y)
                    {
                        min[res.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                    if (bms[i].StageSizeX > max[res.PhysicalSizeX].X || bms[i].StageSizeY > max[res.PhysicalSizeX].Y)
                    {
                        max[res.PhysicalSizeX] = bms[i].Volume.Location;
                    }
                    bis[res.PhysicalSizeX].Add(bms[i]);
                }
            }
            int s = 0;
            //We determine the sizes of each resolution.
            Dictionary<double, Size> ss = new Dictionary<double, Size>();
            int minx = int.MaxValue;
            int miny = int.MaxValue;
            double last = 0;
            foreach (double px in bis.Keys)
            {
                int xs = (1 + (int)Math.Ceiling((max[px].X - min[px].X) / bis[px][0].Resolutions[0].VolumeWidth)) * bis[px][0].SizeX;
                int ys = (1 + (int)Math.Ceiling((max[px].Y - min[px].Y) / bis[px][0].Resolutions[0].VolumeHeight)) * bis[px][0].SizeY;
                if (minx > xs)
                    minx = xs;
                if (miny > ys)
                    miny = ys;
                ss.Add(px, new Size(xs, ys));
                last = px;
            }
            s = 0;
            string met = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" +
                "<OME xmlns=\"http://www.openmicroscopy.org/Schemas/OME/2016-06\" " +
                "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xsi:schemaLocation=\"http://www.openmicroscopy.org/Schemas/OME/2016-06 http://www.openmicroscopy.org/Schemas/OME/2016-06/ome.xsd\">";
            NetVips.Image img = null;
            int ib = 0;

            foreach (double px in bis.Keys)
            {
                int c = bis[px][s].SizeC;
                if (bis[px][s].Buffers[0].isRGB)
                    c = 3;
                string endian = (bis[px][s].Buffers[0].LittleEndian).ToString().ToLower(CultureInfo.InvariantCulture);
                met +=
                "<Image ID=\"Image:" + ib + "\">" +
                "<Pixels BigEndian=\"" + endian + "\" DimensionOrder= \"XYCZT\" ID= \"Pixels:0\" Interleaved=\"true\" " +
                "PhysicalSizeX=\"" + bis[px][s].PhysicalSizeX + "\" PhysicalSizeXUnit=\"µm\" PhysicalSizeY=\"" + bis[px][s].PhysicalSizeY + "\" PhysicalSizeYUnit=\"µm\" SignificantBits=\"" + bis[px][s].bitsPerPixel + "\" " +
                "SizeC = \"" + c + "\" SizeT = \"" + bis[px][s].SizeT + "\" SizeX =\"" + ss[px].Width +
                "\" SizeY= \"" + ss[px].Height + "\" SizeZ=\"" + bis[px][s].SizeZ;
                if (bis[px][s].bitsPerPixel > 8) met += "\" Type= \"uint16\">";
                else met += "\" Type= \"uint8\">";
                int i = 0;
                foreach (Channel ch in bis[px][s].Channels)
                {
                    met += "<Channel ID=\"Channel:" + ib + ":" + i + "\" SamplesPerPixel=\"1\"></Channel>";
                    i++;
                }
                met += "</Pixels></Image>";
                ib++;
            }
            met += "</OME>";
            foreach (double px in bis.Keys)
            {
                PixelFormat pf = bis[px][0].Buffers[0].PixelFormat;
                Bitmap level = new Bitmap(ss[px].Width, ss[px].Height, pf);
                int bands = GetBands(pf);
                BitmapData d = level.LockBits(new Rectangle(0, 0, level.Width, level.Height),ImageLockMode.ReadWrite,pf);
                if (bis[px][0].bitsPerPixel > 8)
                    img = NetVips.Image.NewFromMemory(d.Scan0, (ulong)(d.Stride * d.Height), level.Width, level.Height, bands, Enums.BandFormat.Ushort);
                else
                    img = NetVips.Image.NewFromMemory(d.Scan0, (ulong)(d.Stride * d.Height), level.Width, level.Height, bands, Enums.BandFormat.Uchar);
                int i = 0;
                foreach (BioImage b in bis[px])
                {
                    Size si = ss[px];
                    double xs = (-(min[px].X - bis[px][i].Volume.Location.X) / bis[px][i].Resolutions[0].VolumeWidth) * bis[px][i].SizeX;
                    double ys = (-(min[px].Y - bis[px][i].Volume.Location.Y) / bis[px][i].Resolutions[0].VolumeHeight) * bis[px][i].SizeY;
                    NetVips.Image tile;
                    Bitmap bm = (Bitmap)bis[px][i].Buffers[0].Image;
                    BitmapData bd = bm.LockBits(new Rectangle(0, 0, level.Width, level.Height), ImageLockMode.ReadWrite, pf);
                    if (b.bitsPerPixel > 8)
                        tile = NetVips.Image.NewFromMemory(bd.Scan0, (ulong)bis[px][i].Buffers[0].Length, bis[px][i].Buffers[0].SizeX, bis[px][i].Buffers[0].SizeY, bands, Enums.BandFormat.Ushort);
                    else
                        tile = NetVips.Image.NewFromMemory(bd.Scan0, (ulong)bis[px][i].Buffers[0].Length, bis[px][i].Buffers[0].SizeX, bis[px][i].Buffers[0].SizeY, bands, Enums.BandFormat.Uchar);
                    img = img.Insert(tile, (int)xs, (int)ys);
                    bm.UnlockBits(bd);
                    i++;
                };
                level.UnlockBits(d);
                using var mutated = img.Mutate(mutable =>
                {
                    // Set the ImageDescription tag
                    mutable.Set(GValue.GStrType, "image-description", met);
                    mutable.Set(GValue.GIntType, "page-height", ss[last].Height);
                });
                if (bis[px][0].bitsPerPixel > 8)
                    mutated.Tiffsave(file, compression, 1, Enums.ForeignTiffPredictor.None, true, ss[px].Width, ss[px].Height, true, false, 16,
                    Enums.ForeignTiffResunit.Cm, 1000 * bis[px][0].PhysicalSizeX, 1000 * bis[px][0].PhysicalSizeY, true, null, Enums.RegionShrink.Nearest,
                    compressionLevel, true, Enums.ForeignDzDepth.One, true, false, null, null, ss[px].Height);
                else
                    mutated.Tiffsave(file, compression, 1, Enums.ForeignTiffPredictor.None, true, ss[px].Width, ss[px].Height, true, false, 8,
                    Enums.ForeignTiffResunit.Cm, 1000 * bis[px][0].PhysicalSizeX, 1000 * bis[px][0].PhysicalSizeY, true, null, Enums.RegionShrink.Nearest,
                    compressionLevel, true, Enums.ForeignDzDepth.One, true, false, null, null, ss[px].Height);
                s++;
            }

        }

        /// > This function opens an OME file and returns a BioImage object
        /// 
        /// @param file the path to the file you want to open
        /// 
        /// @return A list of BioImages.
        public static BioImage OpenOME(string file)
        {
            return OpenOMESeries(file, true, true)[0];
        }
        /// > OpenOME(string file, int serie)
        /// 
        /// The first parameter is a string, the second is an integer
        /// 
        /// @param file the path to the file
        /// @param serie the image series to open
        /// 
        /// @return A BioImage object.
        public static BioImage OpenOME(string file, int serie, bool newTab, bool addToImages)
        {
            Recorder.AddLine("Bio.BioImage.OpenOME(\"" + file + "\"," + serie + ");");
            return OpenOME(file, serie, newTab, addToImages, false, 0, 0, 0, 0);
        }
        /// It takes a list of files, and creates a new BioImage object with the first file in the list,
        /// then adds the buffers from the rest of the files to the new BioImage object
        /// 
        /// @param files an array of file paths
        /// @param sizeZ number of images in the stack
        /// @param sizeC number of channels
        /// @param sizeT number of time points
        /// 
        /// @return A BioImage object.
        public static BioImage FilesToStack(string[] files, int sizeZ, int sizeC, int sizeT)
        {
            BioImage b = new BioImage(files[0]);
            for (int i = 0; i < files.Length; i++)
            {
                BioImage bb = OpenFile(files[i]);
                b.Buffers.AddRange(bb.Buffers);
            }
            b.UpdateCoords(sizeZ, sizeC, sizeT);
            Images.AddImage(b,true);
            return b;
        }
        /// It takes a folder of images and creates a stack from them
        /// 
        /// @param path the path to the folder containing the images
        /// 
        /// @return A BioImage object.
        public static BioImage FolderToStack(string path)
        {
            string[] files = Directory.GetFiles(path);
            BioImage b = new BioImage(files[0]);
            int z = 0;
            int c = 0;
            int t = 0;
            BioImage bb = null;
            for (int i = 0; i < files.Length; i++)
            {
                string[] st = files[i].Split('_');
                if (st.Length > 3)
                {
                    z = int.Parse(st[1].Replace("Z", ""));
                    c = int.Parse(st[2].Replace("C", ""));
                    t = int.Parse(st[3].Replace("T", ""));
                }
                bb = OpenFile(files[i]);
                b.Buffers.AddRange(bb.Buffers);
            }
            if (z == 0)
            {
                ImagesToStack im = new ImagesToStack();
                if (im.ShowDialog() != DialogResult.OK)
                    return null;
                b.UpdateCoords(im.SizeZ, im.SizeC, im.SizeT);
            }
            else
                b.UpdateCoords(z + 1, c + 1, t + 1);
            Images.AddImage(b,true);
            Recorder.AddLine("BioImage.FolderToStack(\"" + path + "\");");
            return b;
        }
        public static BioImage OpenOME(string file, int serie, bool tab, bool addToImages, bool tile, int tilex, int tiley, int tileSizeX, int tileSizeY, bool useOpenSlide = true)
        {
            if (file == null || file == "")
                throw new InvalidDataException("File is empty or null");
            //We wait incase OME has not initialized.
            do
            {
                Thread.Sleep(10);
            } while (!initialized);
            reader = new ImageReader();
            Console.WriteLine("OpenOME " + file);
            if (tileSizeX == 0)
                tileSizeX = 1920;
            if (tileSizeY == 0)
                tileSizeY = 1080;
            Progress pr = new Progress(file, "Opening OME");
            pr.Show();
            Application.DoEvents();
            BioImage b = new BioImage(file);
            b.imRead = reader;
            b.Type = ImageType.stack;
            b.Loading = true;
            if (b.meta == null)
                b.meta = service.createOMEXMLMetadata();
            reader.close();
            reader.setMetadataStore(b.meta);
            reader.setId(file);
            pr.Status = "Reading Metadata";
            //status = "Reading OME Metadata.";
            reader.setSeries(serie);
            int RGBChannelCount = reader.getRGBChannelCount();
            //OME reader.getBitsPerPixel(); sometimes returns incorrect bits per pixel, like when opening ImageJ images.
            //So we check the pixel type from xml metadata and if it fails we use the readers value.
            PixelFormat PixelFormat;
            try
            {
                PixelFormat = GetPixelFormat(RGBChannelCount, b.meta.getPixelsType(serie));
            }
            catch (Exception)
            {
                PixelFormat = GetPixelFormat(RGBChannelCount, reader.getBitsPerPixel());
            }

            b.id = file;
            b.file = file;
            int SizeX, SizeY;
            SizeX = reader.getSizeX();
            SizeY = reader.getSizeY();
            int SizeZ = reader.getSizeZ();
            b.sizeC = reader.getSizeC();
            b.sizeZ = reader.getSizeZ();
            b.sizeT = reader.getSizeT();
            b.littleEndian = reader.isLittleEndian();
            b.seriesCount = reader.getSeriesCount();
            b.imagesPerSeries = reader.getImageCount();
            b.bitsPerPixel = reader.getBitsPerPixel();
            b.series = serie;
            string order = reader.getDimensionOrder();

            //Lets get the channels and initialize them
            int i = 0;
            pr.Status = "Reading Channels";
            int sumSamples = 0;
            while (true)
            {
                Channel ch = new Channel(i, b.bitsPerPixel, 1);
                bool def = false;
                try
                {
                    if (b.meta.getChannelSamplesPerPixel(serie, i) != null)
                    {
                        int s = b.meta.getChannelSamplesPerPixel(serie, i).getNumberValue().intValue();
                        ch.SamplesPerPixel = s;
                        sumSamples += s;
                        def = true;
                    }
                    if (b.meta.getChannelName(serie, i) != null)
                        ch.Name = b.meta.getChannelName(serie, i);
                    if (b.meta.getChannelAcquisitionMode(serie, i) != null)
                        ch.AcquisitionMode = b.meta.getChannelAcquisitionMode(serie, i);
                    if (b.meta.getChannelID(serie, i) != null)
                        ch.info.ID = b.meta.getChannelID(serie, i);
                    if (b.meta.getChannelFluor(serie, i) != null)
                        ch.Fluor = b.meta.getChannelFluor(serie, i);
                    if (b.meta.getChannelColor(serie, i) != null)
                    {
                        ome.xml.model.primitives.Color cc = b.meta.getChannelColor(serie, i);
                        ch.Color = System.Drawing.Color.FromArgb(cc.getRed(), cc.getGreen(), cc.getBlue());
                    }
                    if (b.meta.getChannelIlluminationType(serie, i) != null)
                        ch.IlluminationType = b.meta.getChannelIlluminationType(serie, i);
                    if (b.meta.getChannelContrastMethod(serie, i) != null)
                        ch.ContrastMethod = b.meta.getChannelContrastMethod(serie, i);
                    if (b.meta.getChannelEmissionWavelength(serie, i) != null)
                        ch.Emission = b.meta.getChannelEmissionWavelength(serie, i).value().intValue();
                    if (b.meta.getChannelExcitationWavelength(serie, i) != null)
                        ch.Excitation = b.meta.getChannelExcitationWavelength(serie, i).value().intValue();
                    if (b.meta.getLightEmittingDiodePower(serie, i) != null)
                        ch.LightSourceIntensity = b.meta.getLightEmittingDiodePower(serie, i).value().doubleValue();
                    if (b.meta.getLightEmittingDiodeID(serie, i) != null)
                        ch.DiodeName = b.meta.getLightEmittingDiodeID(serie, i);
                    if (b.meta.getChannelLightSourceSettingsAttenuation(serie, i) != null)
                        ch.LightSourceAttenuation = b.meta.getChannelLightSourceSettingsAttenuation(serie, i).toString();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //If this channel is not defined we have loaded all the channels in the file.
                if (!def)
                    break;
                else
                    b.Channels.Add(ch);
                if (i == 0)
                {
                    b.rgbChannels[0] = 0;
                }
                else
                if (i == 1)
                {
                    b.rgbChannels[1] = 1;
                }
                else
                if (i == 2)
                {
                    b.rgbChannels[2] = 2;
                }
                i++;
            }
            //If the file doens't have channels we initialize them.
            if (b.Channels.Count == 0)
            {
                b.Channels.Add(new Channel(0, b.bitsPerPixel, RGBChannelCount));
            }

            //Bioformats gives a size of 3 for C when saved in ImageJ as RGB. We need to correct for this as C should be 1 for RGB.
            if (RGBChannelCount >= 3)
            {
                b.sizeC = sumSamples / b.Channels[0].SamplesPerPixel;
            }
            b.Coords = new int[b.SizeZ, b.SizeC, b.SizeT];

            int resc = reader.getResolutionCount();
            try
            {
                int wells = b.meta.getWellCount(0);
                if (wells > 0)
                {
                    b.Type = ImageType.well;
                    b.Plate = new WellPlate(b);
                    tile = false;
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            List<Resolution> rss = new List<Resolution>();
            for (int s = 0; s < b.seriesCount; s++)
            {
                reader.setSeries(s);
                for (int r = 0; r < reader.getResolutionCount(); r++)
                {
                    Resolution res = new Resolution();
                    try
                    {
                        int rgbc = reader.getRGBChannelCount();
                        int bps = reader.getBitsPerPixel();
                        PixelFormat px;
                        try
                        {
                            px = GetPixelFormat(rgbc, b.meta.getPixelsType(r));
                        }
                        catch (Exception)
                        {
                            px = GetPixelFormat(rgbc, bps);
                        }
                        res.PixelFormat = px;
                        res.SizeX = reader.getSizeX();
                        res.SizeY = reader.getSizeY();
                        if (b.meta.getPixelsPhysicalSizeX(r) != null)
                        {
                            res.PhysicalSizeX = b.meta.getPixelsPhysicalSizeX(r).value().doubleValue();
                        }
                        else
                            res.PhysicalSizeX = (96 / 2.54) / 1000;
                        if (b.meta.getPixelsPhysicalSizeY(r) != null)
                        {
                            res.PhysicalSizeY = b.meta.getPixelsPhysicalSizeY(r).value().doubleValue();
                        }
                        else
                            res.PhysicalSizeY = (96 / 2.54) / 1000;

                        if (b.meta.getStageLabelX(r) != null)
                            res.StageSizeX = b.meta.getStageLabelX(r).value().doubleValue();
                        if (b.meta.getStageLabelY(r) != null)
                            res.StageSizeY = b.meta.getStageLabelY(r).value().doubleValue();
                        if (b.meta.getStageLabelZ(r) != null)
                            res.StageSizeZ = b.meta.getStageLabelZ(r).value().doubleValue();
                        else
                            res.StageSizeZ = 1;
                        if (b.meta.getPixelsPhysicalSizeZ(r) != null)
                        {
                            res.PhysicalSizeZ = b.meta.getPixelsPhysicalSizeZ(r).value().doubleValue();
                        }
                        else
                        {
                            res.PhysicalSizeZ = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("No Stage Coordinates. PhysicalSize:(" + res.PhysicalSizeX + "," + res.PhysicalSizeY + "," + res.PhysicalSizeZ + ")");
                    }
                    rss.Add(res);
                }
            }
            reader.setSeries(serie);
            int pyramidCount = 0;
            int pyramidResolutions = 0;
            List<Tuple<int, int>> prs = new List<Tuple<int, int>>();
            //We need to determine if this image is pyramidal or not.
            //We do this by seeing if the resolutions are downsampled or not.
            if (rss.Count > 1 && b.Type != ImageType.well)
            {
                if (rss[0].SizeX > rss[1].SizeX)
                {
                    b.Type = ImageType.pyramidal;
                    tile = true;
                    //We need to determine number of pyramids in this image and which belong to the series we are opening.
                    int? sr = null;
                    for (int r = 0; r < rss.Count - 1; r++)
                    {
                        if (rss[r].SizeX > rss[r + 1].SizeX && rss[r].PixelFormat == rss[r + 1].PixelFormat)
                        {
                            if (sr == null)
                            {
                                sr = r;
                                prs.Add(new Tuple<int, int>(r, 0));
                            }
                        }
                        else
                        {
                            if (rss[prs[prs.Count - 1].Item1].PixelFormat == rss[r].PixelFormat)
                                prs[prs.Count - 1] = new Tuple<int, int>(prs[prs.Count - 1].Item1, r);
                            sr = null;
                        }
                    }
                    pyramidCount = prs.Count;
                    for (int p = 0; p < prs.Count; p++)
                    {
                        pyramidResolutions += (prs[p].Item2 - prs[p].Item1) + 1;
                    }

                    if (prs[serie].Item2 == 0)
                    {
                        prs[serie] = new Tuple<int, int>(prs[serie].Item1, rss.Count);
                    }
                    for (int r = prs[serie].Item1; r < prs[serie].Item2; r++)
                    {
                        b.Resolutions.Add(rss[r]);
                    }
                }
                else
                {
                    b.Resolutions.AddRange(rss);
                }
            }
            else
                b.Resolutions.AddRange(rss);

            //If we have 2 resolutions that were not added they are the label & macro resolutions so we add them to the image.
            if (rss.Count - pyramidResolutions == 2)
            {
                b.Resolutions.Add(rss[rss.Count - 2]);
                b.Resolutions.Add(rss[rss.Count - 1]);
            }

            b.Volume = new VolumeD(new Point3D(b.StageSizeX, b.StageSizeY, b.StageSizeZ), new Point3D(b.PhysicalSizeX * SizeX, b.PhysicalSizeY * SizeY, b.PhysicalSizeZ * SizeZ));
            pr.Status = "Reading ROIs";
            int rc = b.meta.getROICount();
            for (int im = 0; im < rc; im++)
            {
                string roiID = b.meta.getROIID(im);
                string roiName = b.meta.getROIName(im);
                ZCT co = new ZCT(0, 0, 0);
                int scount = 1;
                try
                {
                    scount = b.meta.getShapeCount(im);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
                for (int sc = 0; sc < scount; sc++)
                {
                    string type = b.meta.getShapeType(im, sc);
                    ROI an = new ROI();
                    an.roiID = roiID;
                    an.roiName = roiName;
                    an.shapeIndex = sc;
                    if (type == "Point")
                    {
                        an.type = ROI.Type.Point;
                        an.id = b.meta.getPointID(im, sc);
                        double dx = b.meta.getPointX(im, sc).doubleValue();
                        double dy = b.meta.getPointY(im, sc).doubleValue();
                        an.AddPoint(b.ToStageSpace(new PointD(dx, dy)));
                        an.coord = new ZCT();
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getPointTheZ(im, sc);
                        if (nz != null)
                            an.coord.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getPointTheC(im, sc);
                        if (nc != null)
                            an.coord.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getPointTheT(im, sc);
                        if (nt != null)
                            an.coord.T = nt.getNumberValue().intValue();
                        an.Text = b.meta.getPointText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getPointStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getPointStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getPointStrokeColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Line")
                    {
                        an.type = ROI.Type.Line;
                        an.id = b.meta.getLineID(im, sc);
                        double px1 = b.meta.getLineX1(im, sc).doubleValue();
                        double py1 = b.meta.getLineY1(im, sc).doubleValue();
                        double px2 = b.meta.getLineX2(im, sc).doubleValue();
                        double py2 = b.meta.getLineY2(im, sc).doubleValue();
                        an.AddPoint(b.ToStageSpace(new PointD(px1, py1)));
                        an.AddPoint(b.ToStageSpace(new PointD(px2, py2)));
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getLineTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getLineTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getLineTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = b.meta.getLineText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getLineStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getLineStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getLineFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Rectangle")
                    {
                        an.type = ROI.Type.Rectangle;
                        an.id = b.meta.getRectangleID(im, sc);
                        double px = b.meta.getRectangleX(im, sc).doubleValue();
                        double py = b.meta.getRectangleY(im, sc).doubleValue();
                        double pw = b.meta.getRectangleWidth(im, sc).doubleValue();
                        double ph = b.meta.getRectangleHeight(im, sc).doubleValue();
                        an.Rect = b.ToStageSpace(new RectangleD(px, py, pw, ph));
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getRectangleTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getRectangleTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getRectangleTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;

                        an.Text = b.meta.getRectangleText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getRectangleStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getRectangleStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getRectangleFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        ome.xml.model.enums.FillRule fr = b.meta.getRectangleFillRule(im, sc);
                    }
                    else
                    if (type == "Ellipse")
                    {
                        an.type = ROI.Type.Ellipse;
                        an.id = b.meta.getEllipseID(im, sc);
                        double px = b.meta.getEllipseX(im, sc).doubleValue();
                        double py = b.meta.getEllipseY(im, sc).doubleValue();
                        double ew = b.meta.getEllipseRadiusX(im, sc).doubleValue();
                        double eh = b.meta.getEllipseRadiusY(im, sc).doubleValue();
                        //We convert the ellipse radius to Rectangle
                        double w = ew * 2;
                        double h = eh * 2;
                        double x = px - ew;
                        double y = py - eh;
                        an.Rect = b.ToStageSpace(new RectangleD(x, y, w, h));
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getEllipseTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getEllipseTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getEllipseTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = b.meta.getEllipseText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getEllipseStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getEllipseStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getEllipseFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polygon")
                    {
                        an.type = ROI.Type.Polygon;
                        an.id = b.meta.getPolygonID(im, sc);
                        an.closed = true;
                        string pxs = b.meta.getPolygonPoints(im, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        pts = b.ToStageSpace(pts);
                        if (pts.Length > 100)
                        {
                            an.type = ROI.Type.Freeform;
                        }
                        an.AddPoints(pts);
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getPolygonTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getPolygonTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getPolygonTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = b.meta.getPolygonText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getPolygonStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getPolygonStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getPolygonFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polyline")
                    {
                        an.type = ROI.Type.Polyline;
                        an.id = b.meta.getPolylineID(im, sc);
                        string pxs = b.meta.getPolylinePoints(im, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        for (int pi = 0; pi < pts.Length; pi++)
                        {
                            pts[pi] = b.ToStageSpace(pts[pi]);
                        }
                        an.AddPoints(an.stringToPoints(pxs));
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getPolylineTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getPolylineTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getPolylineTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = b.meta.getPolylineText(im, sc);
                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getPolylineStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getPolylineStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getPolylineFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Label")
                    {
                        an.type = ROI.Type.Label;
                        an.id = b.meta.getLabelID(im, sc);

                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getLabelTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getLabelTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getLabelTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;

                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getLabelStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getLabelStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getLabelFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        PointD p = new PointD(b.meta.getLabelX(im, sc).doubleValue(), b.meta.getLabelY(im, sc).doubleValue());
                        an.AddPoint(b.ToStageSpace(p));
                        an.Text = b.meta.getLabelText(im, sc);
                    }
                    else
                    if (type == "Mask")
                    {
                        byte[] bts = b.meta.getMaskBinData(im, sc);
                        bool end = b.meta.getMaskBinDataBigEndian(im, sc).booleanValue();
                        an = ROI.CreateMask(co, bts, b.Resolutions[0].SizeX, b.Resolutions[0].SizeY, new PointD(b.StageSizeX, b.StageSizeY), b.PhysicalSizeX, b.PhysicalSizeY);
                        an.Text = b.meta.getMaskText(im, sc);
                        an.id = b.meta.getMaskID(im, sc);
                        ome.xml.model.primitives.NonNegativeInteger nz = b.meta.getMaskTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = b.meta.getMaskTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = b.meta.getMaskTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;

                        int fs = 8;
                        ome.units.quantity.Length fl = b.meta.getPointFontSize(im, sc);
                        if (fl != null)
                            fs = fl.value().intValue();
                        ome.xml.model.enums.FontFamily ff = b.meta.getPointFontFamily(im, sc);
                        if (ff != null)
                            an.font = new Font(new FontFamily(ff.name()), fs);
                        ome.xml.model.primitives.Color col = b.meta.getMaskStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = b.meta.getMaskStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = b.meta.getMaskFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    b.Annotations.Add(an);
                }
            }

            List<string> serFiles = new List<string>();
            serFiles.AddRange(reader.getSeriesUsedFiles());

            b.Buffers = new List<BufferInfo>();
            if (b.Type == ImageType.pyramidal)
                try
                {
                    string st = OpenSlideImage.DetectVendor(file);
                    if (st != null && !file.EndsWith("ome.tif") && useOpenSlide)
                    {
                        b.openSlideImage = OpenSlideImage.Open(file);
                        b.openSlideBase = (OpenSlideBase)OpenSlideGTK.SlideSourceBase.Create(file);
                    }
                    else
                    {
                        b.slideBase = new SlideBase(b,SlideImage.Open(b));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                    b.slideBase = new SlideBase(b,SlideImage.Open(b));
                }

            // read the image data bytes
            int pages = reader.getImageCount();
            bool inter = reader.isInterleaved();
            int z = 0;
            int c = 0;
            int t = 0;
            pr.Status = "Reading Image Data";
            if (!tile)
            {
                try
                {
                    for (int p = 0; p < pages; p++)
                    {
                        BufferInfo bf;
                        pr.UpdateProgressF((float)p / (float)pages);
                        byte[] bytes = reader.openBytes(p);
                        bf = new BufferInfo(file, SizeX, SizeY, PixelFormat, bytes, new ZCT(z, c, t), p, null, b.littleEndian, inter);
                        b.Buffers.Add(bf);
                    }
                }
                catch (Exception)
                {
                    //If we failed to read an entire plane it is likely too large so we open as pyramidal.
                    b.Type = ImageType.pyramidal;
                    try
                    {
                        string st = OpenSlideImage.DetectVendor(file);
                        if (st != null && !file.EndsWith("ome.tif") && useOpenSlide)
                        {
                            b.openSlideImage = OpenSlideImage.Open(file);
                            b.openSlideBase = (OpenSlideBase)OpenSlideGTK.SlideSourceBase.Create(file);
                        }
                        else
                        {
                            b.slideBase = new SlideBase(b, SlideImage.Open(b));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                        b.slideBase = new SlideBase(b, SlideImage.Open(b));
                    }
                    b.imRead = reader;
                    for (int p = 0; p < pages; p++)
                    {
                        b.Buffers.Add(GetTile(b, p, serie, tilex, tiley, tileSizeX, tileSizeY));
                        Statistics.CalcStatistics(b.Buffers.Last());
                    }
                }
                
            }
            else
            {
                b.imRead = reader;
                for (int p = 0; p < pages; p++)
                {
                    b.Buffers.Add(GetTile(b, p, serie, tilex, tiley, tileSizeX, tileSizeY));
                    Statistics.CalcStatistics(b.Buffers.Last());
                }
            }
            int pls;
            try
            {
                pls = b.meta.getPlaneCount(serie);
            }
            catch (Exception)
            {
                pls = 0;
            }
            pr.Status = "Reading Plane Data";
            if (pls == b.Buffers.Count)
                for (int bi = 0; bi < b.Buffers.Count; bi++)
                {
                    Plane pl = new Plane();
                    pl.Coordinate = new ZCT();
                    double px = 0; double py = 0; double pz = 0;
                    if (b.meta.getPlanePositionX(serie, bi) != null)
                        px = b.meta.getPlanePositionX(serie, bi).value().doubleValue();
                    if (b.meta.getPlanePositionY(serie, bi) != null)
                        py = b.meta.getPlanePositionY(serie, bi).value().doubleValue();
                    if (b.meta.getPlanePositionZ(serie, bi) != null)
                        pz = b.meta.getPlanePositionZ(serie, bi).value().doubleValue();
                    pl.Location = new Point3D(px, py, pz);
                    int cc = 0; int zc = 0; int tc = 0;
                    if (b.meta.getPlaneTheC(serie, bi) != null)
                        cc = b.meta.getPlaneTheC(serie, bi).getNumberValue().intValue();
                    if (b.meta.getPlaneTheZ(serie, bi) != null)
                        zc = b.meta.getPlaneTheZ(serie, bi).getNumberValue().intValue();
                    if (b.meta.getPlaneTheT(serie, bi) != null)
                        tc = b.meta.getPlaneTheT(serie, bi).getNumberValue().intValue();
                    pl.Coordinate = new ZCT(zc, cc, tc);
                    if (b.meta.getPlaneDeltaT(serie, bi) != null)
                        pl.Delta = b.meta.getPlaneDeltaT(serie, bi).value().doubleValue();
                    if (b.meta.getPlaneExposureTime(serie, bi) != null)
                        pl.Exposure = b.meta.getPlaneExposureTime(serie, bi).value().doubleValue();
                    b.Buffers[bi].Plane = pl;
                }

            b.UpdateCoords(b.SizeZ, b.SizeC, b.SizeT, order);
            //We wait for histogram image statistics calculation
            do
            {
                Thread.Sleep(50);
            } while (b.Buffers[b.Buffers.Count - 1].Stats == null);
            Statistics.ClearCalcBuffer();
            AutoThreshold(b, true);
            if (b.bitsPerPixel > 8)
                b.StackThreshold(true);
            else
                b.StackThreshold(false);
            
            //We use a try block to close the reader as sometimes this will cause an error.
            bool stop = false;
            do
            {
                try
                {
                    reader.close();
                    stop = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            } while (!stop);
            b.Loading = false;
            b.SetLabelMacroResolutions();
            Console.WriteLine("Opening complete " + file);
            pr.Close();
            if (addToImages)
                Images.AddImage(b, tab);
            return b;
        }
        public ImageReader imRead = new ImageReader();
        byte[] bts;
        int ssx, ssy;
        public static int FindFocus(BioImage im, int Channel, int Time)
        {

            long mf = 0;
            int fr = 0;
            List<double> dt = new List<double>();
            ZCT c = new ZCT(0, 0, 0);
            for (int i = 0; i < im.SizeZ; i++)
            {
                long f = CalculateFocusQuality(im.Buffers[im.Coords[i, Channel, Time]]);
                dt.Add(f);
                if (f > mf)
                {
                    mf = f;
                    fr = im.Coords[i, Channel, Time];
                }
            }
            Plot pl = new Plot(dt.ToArray(), "Focus");
            return fr;
        }
        static long CalculateFocusQuality(BufferInfo b)
        {
            if (b.RGBChannelsCount == 1)
            {
                long sum = 0;
                long sumOfSquaresR = 0;
                for (int y = 0; y < b.SizeY; y++)
                    for (int x = 0; x < b.SizeX; x++)
                    {
                        ColorS pixel = b.GetPixel(x, y);
                        sum += pixel.R;
                        sumOfSquaresR += pixel.R * pixel.R;
                    }
                return sumOfSquaresR * b.SizeX * b.SizeY - sum * sum;
            }
            else
            {
                long sum = 0;
                long sumOfSquares = 0;
                for (int y = 0; y < b.SizeY; y++)
                    for (int x = 0; x < b.SizeX; x++)
                    {
                        ColorS pixel = b.GetPixel(x, y);
                        int p = (pixel.R + pixel.G + pixel.B) / 3;
                        sum += p;
                        sumOfSquares += p * p;
                    }
                return sumOfSquares * b.SizeX * b.SizeY - sum * sum;
            }
        }
        public static BufferInfo ExtractRegionFromTiledTiff(BioImage b, int x, int y, int width, int height, int res)
        {
            try
            {

                NetVips.Image subImage = b.vipPages[res].Crop(x, y, width, height);
                if (b.vipPages[res].Format == NetVips.Enums.BandFormat.Uchar)
                {
                    BufferInfo bm;
                    byte[] imageData = subImage.WriteToMemory();
                    if (b.Resolutions[res].RGBChannelsCount == 3)
                        bm = new BufferInfo(b.file, subImage.Width, subImage.Height, PixelFormat.Format24bppRgb, imageData, new ZCT(), res);
                    else
                        bm = new BufferInfo(b.file, subImage.Width, subImage.Height, PixelFormat.Format8bppIndexed, imageData, new ZCT(), res);
                    return bm;

                }
                else if (b.vipPages[res].Format == NetVips.Enums.BandFormat.Ushort)
                {
                    BufferInfo bm;
                    byte[] imageData = subImage.WriteToMemory();
                    if (b.Resolutions[res].RGBChannelsCount == 3)
                        bm = new BufferInfo(b.file, subImage.Width, subImage.Height, PixelFormat.Format48bppRgb, imageData, new ZCT(), res);
                    else
                        bm = new BufferInfo(b.file, subImage.Width, subImage.Height, PixelFormat.Format16bppGrayScale, imageData, new ZCT(), res);
                    return bm;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
            return null;
        }
        /// It reads a tile from a file, and returns a bitmap
        /// 
        /// @param BioImage This is a class that contains the image file name, the image reader, and the
        /// coordinates of the image.
        /// @param ZCT Z, C, T
        /// @param serie the series number (0-based).
        /// @param tilex the x coordinate of the tile.
        /// @param tiley the y coordinate of the tile.
        /// @param tileSizeX the width of the tile.
        /// @param tileSizeY the height of the tile.
        /// @param OpenSlide whether the tile should be opened with OpenSlide.
        /// @return A Bitmap object.
        public static BufferInfo GetTile(BioImage b, ZCT coord, int serie, int tilex, int tiley, int tileSizeX, int tileSizeY, bool OpenSlide = true)
        {
            if ((b.file.EndsWith("ome.tif") && vips) || (b.file.EndsWith(".tif") && vips))
            {
                //We can get a tile faster with libvips rather than bioformats.
                //and incase we are on mac we can't use bioformats due to IKVM not supporting mac.
                return ExtractRegionFromTiledTiff(b, tilex, tiley, tileSizeX, tileSizeY, serie);
            }
            //We check if we can open this with OpenSlide as this is faster than Bioformats with IKVM.
            if (b.openSlideImage != null && OpenSlide && !b.file.EndsWith("ome.tif"))
            {
                return new BufferInfo(tileSizeX, tileSizeY, PixelFormat.Format32bppArgb, b.openSlideImage.ReadRegion(serie, tilex, tiley, tileSizeX, tileSizeY), coord, "");
            }
            if (b.imRead == null)
                b.imRead = new ImageReader();
            string s = b.imRead.getCurrentFile();
            b.file = b.file.Replace("\\", "/");
            if (s == null)
                b.imRead.setId(b.file);
            if (b.imRead.getSeries() != serie)
                b.imRead.setSeries(serie);
            int SizeX = b.imRead.getSizeX();
            int SizeY = b.imRead.getSizeY();
            int p = b.Coords[coord.Z, coord.C, coord.T];
            bool littleEndian = b.imRead.isLittleEndian();
            PixelFormat px = b.Resolutions[serie].PixelFormat;
            if (tilex < 0)
                tilex = 0;
            if (tiley < 0)
                tiley = 0;
            if (tilex >= SizeX)
                tilex = SizeX - 1;
            if (tiley >= SizeY)
                tiley = SizeY - 1;
            int sx = tileSizeX;
            if (tilex + tileSizeX > SizeX)
                sx -= (tilex + tileSizeX) - (SizeX);
            int sy = tileSizeY;
            if (tiley + tileSizeY > SizeY)
                sy -= (tiley + tileSizeY) - (SizeY);
            if (sx <= 0)
                return null;
            if (sy <= 0)
                return null;
            try
            {
                byte[] bytesr = b.imRead.openBytes(b.Coords[coord.Z, coord.C, coord.T], tilex, tiley, sx, sy);
                bool interleaved = b.imRead.isInterleaved();
                BufferInfo bm = new BufferInfo(b.file, sx, sy, px, bytesr, coord, p, null, littleEndian, interleaved);
                return bm;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }
        public static BufferInfo GetTile(BioImage b, int index, int serie, int tilex, int tiley, int tileSizeX, int tileSizeY, bool OpenSlide = true)
        {
            if ((b.file.EndsWith("ome.tif") && vips) || (b.file.EndsWith(".tif") && vips))
            {
                //We can get a tile faster with libvips rather than bioformats.
                //and incase we are on mac we can't use bioformats due to IKVM not supporting mac.
                return ExtractRegionFromTiledTiff(b, tilex, tiley, tileSizeX, tileSizeY, serie);
            }
            //We check if we can open this with OpenSlide as this is faster than Bioformats with IKVM.
            if (b.openSlideImage != null && OpenSlide && !b.file.EndsWith("ome.tif"))
            {
                return new BufferInfo(tileSizeX, tileSizeY, PixelFormat.Format32bppArgb, b.openSlideImage.ReadRegion(serie, tilex, tiley, tileSizeX, tileSizeY), new ZCT(), "");
            }
            if (b.imRead == null)
                b.imRead = new ImageReader();
            string s = b.imRead.getCurrentFile();
            b.file = b.file.Replace("\\", "/");
            if (s == null)
                b.imRead.setId(b.file);
            if (b.imRead.getSeries() != serie)
                b.imRead.setSeries(serie);
            int SizeX = b.imRead.getSizeX();
            int SizeY = b.imRead.getSizeY();
            bool littleEndian = b.imRead.isLittleEndian();
            PixelFormat px = b.Resolutions[serie].PixelFormat;
            if (tilex < 0)
                tilex = 0;
            if (tiley < 0)
                tiley = 0;
            if (tilex >= SizeX)
                tilex = SizeX - 1;
            if (tiley >= SizeY)
                tiley = SizeY - 1;
            int sx = tileSizeX;
            if (tilex + tileSizeX > SizeX)
                sx -= (tilex + tileSizeX) - (SizeX);
            int sy = tileSizeY;
            if (tiley + tileSizeY > SizeY)
                sy -= (tiley + tileSizeY) - (SizeY);
            if (sx <= 0)
                return null;
            if (sy <= 0)
                return null;
            byte[] bytesr = b.imRead.openBytes(index, tilex, tiley, sx, sy);
            bool interleaved = b.imRead.isInterleaved();
            BufferInfo bm = new BufferInfo(b.file, sx, sy, px, bytesr, new ZCT(), index, null, littleEndian, interleaved);
            return bm;
        }
        public Bitmap GetTileRGB(ZCT coord, int serie, int tilex, int tiley, int tileSizeX, int tileSizeY)
        {
            return (Bitmap)GetTile(this, coord, serie, tilex, tiley, tileSizeX, tileSizeY).ImageRGB;
        }
        static NetVips.Image netim;
        public static bool VipsSupport(string file)
        {
            NetVips.Image im;
            try
            {
                im = NetVips.Image.Tiffload(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            im.Close();
            im.Dispose();
            return true;
        }
        /// This function sets the minimum and maximum values of the image to the minimum and maximum
        /// values of the stack
        /// 
        /// @param bit16 true = 16 bit, false = 8 bit
        public void StackThreshold(bool bit16)
        {
            if (bit16)
            {
                for (int ch = 0; ch < Channels.Count; ch++)
                {
                    for (int i = 0; i < Channels[ch].range.Length; i++)
                    {
                        Channels[ch].range[i].Min = (int)Channels[ch].stats[i].StackMin;
                        Channels[ch].range[i].Max = (int)Channels[ch].stats[i].StackMax;
                    }
                    Channels[ch].BitsPerPixel = 16;
                }
                bitsPerPixel = 16;
            }
            else
            {
                for (int ch = 0; ch < Channels.Count; ch++)
                {
                    for (int i = 0; i < Channels[ch].range.Length; i++)
                    {
                        Channels[ch].range[i].Min = (int)Channels[ch].stats[i].StackMin;
                        Channels[ch].range[i].Max = (int)Channels[ch].stats[i].StackMax;
                    }
                    Channels[ch].BitsPerPixel = 8;
                }
                bitsPerPixel = 8;
            }
        }
        /// > If the number is less than or equal to 255, then it's 8 bits. If it's less than or equal
        /// to 512, then it's 9 bits. If it's less than or equal to 1023, then it's 10 bits. If it's
        /// less than or equal to 2047, then it's 11 bits. If it's less than or equal to 4095, then it's
        /// 12 bits. If it's less than or equal to 8191, then it's 13 bits. If it's less than or equal
        /// to 16383, then it's 14 bits. If it's less than or equal to 32767, then it's 15 bits. If it's
        /// less than or equal to 65535, then it's 16 bits
        /// 
        /// @param bt The number of bits per pixel.
        /// 
        /// @return The number of bits per pixel.
        public static int GetBitsPerPixel(int bt)
        {
            if (bt <= 255)
                return 8;
            if (bt <= 512)
                return 9;
            else if (bt <= 1023)
                return 10;
            else if (bt <= 2047)
                return 11;
            else if (bt <= 4095)
                return 12;
            else if (bt <= 8191)
                return 13;
            else if (bt <= 16383)
                return 14;
            else if (bt <= 32767)
                return 15;
            else
                return 16;
        }
        /// It returns the maximum value of a bit.
        /// 
        /// @param bt bit depth
        /// 
        /// @return The maximum value of a bit.
        public static int GetBitMaxValue(int bt)
        {
            if (bt == 8)
                return 255;
            if (bt == 9)
                return 512;
            else if (bt == 10)
                return 1023;
            else if (bt == 11)
                return 2047;
            else if (bt == 12)
                return 4095;
            else if (bt == 13)
                return 8191;
            else if (bt == 14)
                return 16383;
            else if (bt == 15)
                return 32767;
            else
                return 65535;
        }
        /// If the bits per pixel is 8, then the pixel format is either 8bppIndexed, 24bppRgb, or
        /// 32bppArgb. If the bits per pixel is 16, then the pixel format is either 16bppGrayScale or
        /// 48bppRgb
        /// 
        /// @param rgbChannelCount The number of channels in the image. For example, a grayscale image
        /// has 1 channel, a color image has 3 channels (red, green, blue).
        /// @param bitsPerPixel 8 or 16
        /// 
        /// @return The PixelFormat of the image.
        public static PixelFormat GetPixelFormat(int rgbChannelCount, int bitsPerPixel)
        {
            if (bitsPerPixel == 8)
            {
                if (rgbChannelCount == 1)
                    return PixelFormat.Format8bppIndexed;
                else if (rgbChannelCount == 3)
                    return PixelFormat.Format24bppRgb;
                else if (rgbChannelCount == 4)
                    return PixelFormat.Format32bppArgb;
            }
            else
            {
                if (rgbChannelCount == 1)
                    return PixelFormat.Format16bppGrayScale;
                if (rgbChannelCount == 3)
                    return PixelFormat.Format48bppRgb;
            }
            throw new NotSupportedException("Not supported pixel format.");
        }
        public static PixelFormat GetPixelFormat(int rgbChannelCount, ome.xml.model.enums.PixelType px)
        {
            if (rgbChannelCount == 1)
            {
                if (px == ome.xml.model.enums.PixelType.INT8 || px == ome.xml.model.enums.PixelType.UINT8)
                    return PixelFormat.Format8bppIndexed;
                else if (px == ome.xml.model.enums.PixelType.INT16 || px == ome.xml.model.enums.PixelType.UINT16)
                    return PixelFormat.Format16bppGrayScale;
            }
            else
            {
                if (px == ome.xml.model.enums.PixelType.INT8 || px == ome.xml.model.enums.PixelType.UINT8)
                    return PixelFormat.Format24bppRgb;
                else if (px == ome.xml.model.enums.PixelType.INT16 || px == ome.xml.model.enums.PixelType.UINT16)
                    return PixelFormat.Format48bppRgb;
            }
            throw new NotSupportedException("Not supported pixel format.");
        }
        /// It opens an OME-TIFF file, and returns an array of BioImage objects, one for each series in
        /// the file
        /// 
        /// @param file the path to the file
        /// 
        /// @return An array of BioImage objects.
        public static BioImage[] OpenOMESeries(string file, bool newTab, bool addToImages)
        {
            reader = new ImageReader();
            var meta = service.createOMEXMLMetadata();
            reader.setMetadataStore((MetadataStore)meta);
            file = file.Replace("\\", "/");
            reader.setId(file);
            bool tile = false;
            if (reader.getOptimalTileWidth() != reader.getSizeX())
                tile = true;
            int count = reader.getSeriesCount();
            BioImage[] bs = null;
            if (tile)
            {
                bs = new BioImage[1];
                bs[0] = OpenOME(file, 0, newTab, addToImages, true, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                return bs;
            }
            else
                bs = new BioImage[count];
            reader.close();
            for (int i = 0; i < count; i++)
            {
                bs[i] = OpenOME(file, i,newTab,addToImages);
                if (bs[i] == null)
                    return null;
            }
            return bs;
        }
        /// It opens a file in a new thread.
        /// 
        /// @param file The file to open
        public static async Task OpenAsync(string file, bool OME, bool newtab, bool images, int series)
        {
            openfile = file;
            omes = OME;
            tab = newtab;
            add = images;
            serie = series;
            await Task.Run(OpenThread);
        }
        /// It opens a file asynchronously
        /// 
        /// @param files The file(s) to open.
        public static async Task OpenAsync(string[] files, bool OME, bool tab, bool images)
        {
            foreach (string file in files)
            {
                await OpenAsync(file, OME, tab, images, 0);
            }
        }

        /// It opens a TIFF file and returns a BioImage object
        /// 
        /// @param file the file path
        /// @param series the series number of the image to open
        /// 
        /// @return A BioImage object.
        public static BioImage OpenFile(string file, int series, bool tab, bool addToImages)
        {
            return OpenFile(file, series, tab, addToImages, false, 0,0,0,0);
        }

        static string openfile;
        static bool omes, tab, add;
        static int serie;
        static void OpenThread()
        {
            OpenFile(openfile, serie, tab, add);
        }
        static string savefile, saveid;
        static bool some;
        static int sserie;
        static void SaveThread()
        {
            if (omes)
                SaveOME(savefile, saveid);
            else
                SaveFile(savefile, saveid);
        }
        /// <summary>
        /// The SaveAsync function saves data to a file asynchronously.
        /// </summary>
        /// <param name="file">The file parameter is a string that represents the file path or name
        /// where the data will be saved.</param>
        /// <param name="id">The "id" parameter is a string that represents an identifier for the save
        /// operation. It could be used to uniquely identify the saved data or to specify a specific
        /// location or format for the saved file.</param>
        /// <param name="serie">The "serie" parameter is an integer that represents a series or sequence
        /// number. It is used as a parameter in the SaveAsync method.</param>
        /// <param name="ome">The "ome" parameter is a boolean value that determines whether or not to
        /// perform a specific action in the saving process.</param>
        public static async Task SaveAsync(string file, string id, int serie, bool ome)
        {
            savefile = file;
            saveid = id;
            some = ome;
            await Task.Run(SaveThread);
        }

        static List<string> sts = new List<string>();
        static void SaveSeriesThread()
        {
            if (omes)
            {
                BioImage[] bms = new BioImage[sts.Count];
                int i = 0;
                foreach (string st in sts)
                {
                    bms[i] = Images.GetImage(st);
                }
                SaveOMESeries(bms, savefile, Planes);
            }
            else
                SaveSeries(sts.ToArray(), savefile);
        }
        /// <summary>
        /// The function `SaveSeriesAsync` saves a series of `BioImage` objects to a file asynchronously.
        /// </summary>
        /// <param name="imgs">imgs is an array of BioImage objects.</param>
        /// <param name="file">The "file" parameter is a string that represents the file path where the
        /// series of BioImages will be saved.</param>
        /// <param name="ome">The "ome" parameter is a boolean flag that indicates whether the images
        /// should be saved in OME-TIFF format or not. If "ome" is set to true, the images will be saved
        /// in OME-TIFF format. If "ome" is set to false, the images will be</param>
        public static async Task SaveSeriesAsync(BioImage[] imgs, string file, bool ome)
        {
            sts.Clear();
            foreach (BioImage item in imgs)
            {
                sts.Add(item.ID);
            }
            savefile = file;
            some = ome;
            await Task.Run(SaveSeriesThread);
        }
        static Enums.ForeignTiffCompression comp;
        static int compLev = 0;
        static BioImage[] bms;
        static void SavePyramidalThread()
        {
            SaveOMEPyramidal(bms, savefile, comp, compLev);
        }
        /// <summary>
        /// The function `SavePyramidalAsync` saves an array of `BioImage` objects as a pyramidal TIFF
        /// file asynchronously.
        /// </summary>
        /// <param name="imgs">imgs is an array of BioImage objects.</param>
        /// <param name="file">The "file" parameter is a string that represents the file path where the
        /// pyramidal image will be saved.</param>
        /// <param name="com">The parameter "com" is of type Enums.ForeignTiffCompression, which is an
        /// enumeration representing different compression options for the TIFF file.</param>
        /// <param name="compLevel">The `compLevel` parameter is an integer that represents the
        /// compression level for the TIFF file. It is used to specify the level of compression to be
        /// applied to the image data when saving the pyramidal image. The higher the compression level,
        /// the smaller the file size but potentially lower image quality.</param>
        public static async Task SavePyramidalAsync(BioImage[] imgs, string file, Enums.ForeignTiffCompression com, int compLevel)
        {
            bms = imgs;
            savefile = file;
            comp = com;
            compLev = compLevel;
            await Task.Run(SavePyramidalThread);
        }
        private static List<string> openOMEfile = new List<string>();
        /// It opens the OME file.
        private static void OpenOME()
        {
            foreach (string f in openOMEfile)
            {
                OpenOME(f, 0, true, true, false, 0,0,0,0);
            }
            openOMEfile.Clear();
        }
        private static string saveOMEfile;
        private static string saveOMEID;
        /// It opens a file
        /// 
        /// @param file The file to open.
        public static void Open(string file)
        {
            OpenFile(file);
        }
        /// It opens a file
        /// 
        /// @param files The files to open.
        public static void Open(string[] files)
        {
            foreach (string file in files)
            {
                Open(file);
            }
        }
        /// It takes a file and an ID and saves the file to the ID
        /// 
        /// @param file The file to save the data to.
        /// @param ID The ID of the user
        public static void Save(string file, string ID)
        {
            SaveFile(file, ID);
        }
        /// It takes the file name and the ID of the OME and saves it to the file
        private static void SaveOME()
        {
            SaveOME(saveOMEfile, saveOMEID);
        }

        /// It takes a list of files, opens them, and stacks them into a single BioImage object
        /// 
        /// @param files an array of file paths
        /// 
        /// @return A BioImage object.
        public static BioImage ImagesToStack(string[] files)
        {
            BioImage[] bs = new BioImage[files.Length];
            int z = 0;
            int c = 0;
            int t = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string str = Path.GetFileNameWithoutExtension(files[i]);
                str = str.Replace(".ome", "");
                string[] st = str.Split('_');
                if (st.Length > 3)
                {
                    z = int.Parse(st[1].Replace("Z", ""));
                    c = int.Parse(st[2].Replace("C", ""));
                    t = int.Parse(st[3].Replace("T", ""));
                }
                if (i == 0)
                    bs[0] = OpenOME(files[i]);
                else
                {
                    bs[i] = OpenFile(files[i], 0, true);
                }
            }
            BioImage b = BioImage.CopyInfo(bs[0], true, true);
            for (int i = 0; i < files.Length; i++)
            {
                for (int bc = 0; bc < bs[i].Buffers.Count; bc++)
                {
                    b.Buffers.Add(bs[i].Buffers[bc]);
                }
            }
            b.UpdateCoords(z + 1, c + 1, t + 1);
            return b;
        }
        /// The function takes a BioImage object, opens the file, and returns a new BioImage object
        /// 
        /// @param BioImage This is the class that contains the image data.
        public static void Update(BioImage b)
        {
            b = OpenFile(b.file);
        }
        /// It updates the current object.
        public void Update()
        {
            Update(this);
        }

        private static List<Resolution> GetResolutions()
        {
            List<Resolution> rss = new List<Resolution>();
            int seriesCount = reader.getSeriesCount();
            for (int s = 0; s < seriesCount; s++)
            {
                reader.setSeries(s);
                IMetadata meta = (IMetadata)reader.getMetadataStore();
                for (int r = 0; r < reader.getResolutionCount(); r++)
                {
                    Resolution res = new Resolution();
                    try
                    {
                        int rgbc = reader.getRGBChannelCount();
                        int bps = reader.getBitsPerPixel();
                        PixelFormat px;
                        try
                        {
                            px = GetPixelFormat(rgbc, meta.getPixelsType(r));
                        }
                        catch (Exception)
                        {
                            px = GetPixelFormat(rgbc, bps);
                        }
                        res.PixelFormat = px;
                        res.SizeX = reader.getSizeX();
                        res.SizeY = reader.getSizeY();
                        if (meta.getPixelsPhysicalSizeX(r) != null)
                        {
                            res.PhysicalSizeX = meta.getPixelsPhysicalSizeX(r).value().doubleValue();
                        }
                        else
                            res.PhysicalSizeX = (96 / 2.54) / 1000;
                        if (meta.getPixelsPhysicalSizeY(r) != null)
                        {
                            res.PhysicalSizeY = meta.getPixelsPhysicalSizeY(r).value().doubleValue();
                        }
                        else
                            res.PhysicalSizeY = (96 / 2.54) / 1000;

                        if (meta.getStageLabelX(r) != null)
                            res.StageSizeX = meta.getStageLabelX(r).value().doubleValue();
                        if (meta.getStageLabelY(r) != null)
                            res.StageSizeY = meta.getStageLabelY(r).value().doubleValue();
                        if (meta.getStageLabelZ(r) != null)
                            res.StageSizeZ = meta.getStageLabelZ(r).value().doubleValue();
                        else
                            res.StageSizeZ = 1;
                        if (meta.getPixelsPhysicalSizeZ(r) != null)
                        {
                            res.PhysicalSizeZ = meta.getPixelsPhysicalSizeZ(r).value().doubleValue();
                        }
                        else
                        {
                            res.PhysicalSizeZ = 1;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("No Stage Coordinates. PhysicalSize:(" + res.PhysicalSizeX + "," + res.PhysicalSizeY + "," + res.PhysicalSizeZ + ")");
                    }
                    rss.Add(res);
                }
            }
            return rss;
        }
        private static int GetPyramidCount()
        {
            List<Resolution> rss = GetResolutions();
            //We need to determine if this image is pyramidal or not.
            //We do this by seeing if the resolutions are downsampled or not.
            //We also need to determine number of pyramids in this image and which belong to the series we are opening.
            List<Tuple<int, int>> prs = new List<Tuple<int, int>>();
            int? sr = null;
            for (int r = 0; r < rss.Count - 1; r++)
            {
                if (rss[r].SizeX > rss[r + 1].SizeX && rss[r].PixelFormat == rss[r + 1].PixelFormat)
                {
                    if (sr == null)
                    {
                        sr = r;
                        prs.Add(new Tuple<int, int>(r, 0));
                    }
                }
                else
                {
                    if (rss[prs[prs.Count - 1].Item1].PixelFormat == rss[r].PixelFormat)
                        prs[prs.Count - 1] = new Tuple<int, int>(prs[prs.Count - 1].Item1, r);
                    sr = null;
                }
            }
            return prs.Count;
        }
        public static int GetSeriesCount(string file)
        {
            reader.setId(file);
            int i = reader.getSeriesCount();
            int prs = GetPyramidCount();
            reader.close();
            if (prs > 0)
                return prs;
            else
                return i;
        }

        private static Stopwatch st = new Stopwatch();
        private static ServiceFactory factory;
        private static OMEXMLService service;
        private static ImageReader reader;
        private static ImageWriter writer;
        private loci.formats.meta.IMetadata meta;

        //We use UNIX type line endings since they are supported by ImageJ & BioImage.
        public const char NewLine = '\n';
        public const string columns = "ROIID,ROINAME,TYPE,ID,SHAPEINDEX,TEXT,S,C,Z,T,X,Y,W,H,POINTS,STROKECOLOR,STROKECOLORW,FILLCOLOR,FONTSIZE\n";

        /// > Open the file, get the image description field, and return it as a string
        /// 
        /// @param file the path to the file
        /// 
        /// @return The image description of the tiff file.
        public static string OpenXML(string file)
        {
            if (!file.EndsWith(".tif"))
                return null;
            Tiff image = Tiff.Open(file, "r");
            FieldValue[] f = image.GetField(TiffTag.IMAGEDESCRIPTION);
            return f[0].ToString();
        }

        /// This function takes a file path to an OME-TIFF file and returns a list of ROI objects
        /// 
        /// @param file the path to the OME-TIFF file
        /// @param series the series number of the image you want to open
        /// 
        /// @return A list of ROI objects.
        public static List<ROI> OpenOMEROIs(string file, int series)
        {
            List<ROI> Annotations = new List<ROI>();
            // create OME-XML metadata store
            ServiceFactory factory = new ServiceFactory();
            OMEXMLService service = (OMEXMLService)factory.getInstance(typeof(OMEXMLService));
            loci.formats.ome.OMEXMLMetadata meta = service.createOMEXMLMetadata();
            // create format reader
            ImageReader imageReader = new ImageReader();
            imageReader.setMetadataStore(meta);
            // initialize file
            file = file.Replace("\\", "/");
            imageReader.setId(file);
            int imageCount = imageReader.getImageCount();
            int seriesCount = imageReader.getSeriesCount();
            double physicalSizeX = 0;
            double physicalSizeY = 0;
            double physicalSizeZ = 0;
            double stageSizeX = 0;
            double stageSizeY = 0;
            double stageSizeZ = 0;
            int SizeX = imageReader.getSizeX();
            int SizeY = imageReader.getSizeY();
            int SizeZ = imageReader.getSizeY();
            try
            {
                bool hasPhysical = false;
                if (meta.getPixelsPhysicalSizeX(series) != null)
                {
                    physicalSizeX = meta.getPixelsPhysicalSizeX(series).value().doubleValue();
                    hasPhysical = true;
                }
                if (meta.getPixelsPhysicalSizeY(series) != null)
                {
                    physicalSizeY = meta.getPixelsPhysicalSizeY(series).value().doubleValue();
                }
                if (meta.getPixelsPhysicalSizeZ(series) != null)
                {
                    physicalSizeZ = meta.getPixelsPhysicalSizeZ(series).value().doubleValue();
                }
                else
                {
                    physicalSizeZ = 1;
                }
                if (meta.getStageLabelX(series) != null)
                    stageSizeX = meta.getStageLabelX(series).value().doubleValue();
                if (meta.getStageLabelY(series) != null)
                    stageSizeY = meta.getStageLabelY(series).value().doubleValue();
                if (meta.getStageLabelZ(series) != null)
                    stageSizeZ = meta.getStageLabelZ(series).value().doubleValue();
                else
                    stageSizeZ = 1;
            }
            catch (Exception e)
            {
                stageSizeX = 0;
                stageSizeY = 0;
                stageSizeZ = 1;
            }
            VolumeD volume = new VolumeD(new Point3D(stageSizeX, stageSizeY, stageSizeZ), new Point3D(physicalSizeX * SizeX, physicalSizeY * SizeY, physicalSizeZ * SizeZ));
            int rc = meta.getROICount();
            for (int im = 0; im < rc; im++)
            {
                string roiID = meta.getROIID(im);
                string roiName = meta.getROIName(im);
                ZCT co = new ZCT(0, 0, 0);
                int scount = 1;
                try
                {
                    scount = meta.getShapeCount(im);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
                for (int sc = 0; sc < scount; sc++)
                {
                    string type = meta.getShapeType(im, sc);
                    ROI an = new ROI();
                    an.roiID = roiID;
                    an.roiName = roiName;
                    an.shapeIndex = sc;
                    if (type == "Point")
                    {
                        an.type = ROI.Type.Point;
                        an.id = meta.getPointID(im, sc);
                        double dx = meta.getPointX(im, sc).doubleValue();
                        double dy = meta.getPointY(im, sc).doubleValue();
                        an.AddPoint(ToStageSpace(new PointD(dx, dy), physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y));
                        an.coord = new ZCT();
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getPointTheZ(im, sc);
                        if (nz != null)
                            an.coord.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getPointTheC(im, sc);
                        if (nc != null)
                            an.coord.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getPointTheT(im, sc);
                        if (nt != null)
                            an.coord.T = nt.getNumberValue().intValue();
                        an.Text = meta.getPointText(im, sc);
                        ome.units.quantity.Length fl = meta.getPointFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPointStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPointStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPointStrokeColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Line")
                    {
                        an.type = ROI.Type.Line;
                        an.id = meta.getLineID(im, sc);
                        double px1 = meta.getLineX1(im, sc).doubleValue();
                        double py1 = meta.getLineY1(im, sc).doubleValue();
                        double px2 = meta.getLineX2(im, sc).doubleValue();
                        double py2 = meta.getLineY2(im, sc).doubleValue();
                        an.AddPoint(ToStageSpace(new PointD(px1, py1), physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y));
                        an.AddPoint(ToStageSpace(new PointD(px2, py2), physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y));
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getLineTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getLineTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getLineTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = meta.getLineText(im, sc);
                        ome.units.quantity.Length fl = meta.getLineFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLineStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLineStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLineFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Rectangle")
                    {
                        an.type = ROI.Type.Rectangle;
                        an.id = meta.getRectangleID(im, sc);
                        double px = meta.getRectangleX(im, sc).doubleValue();
                        double py = meta.getRectangleY(im, sc).doubleValue();
                        double pw = meta.getRectangleWidth(im, sc).doubleValue();
                        double ph = meta.getRectangleHeight(im, sc).doubleValue();
                        an.Rect = ToStageSpace(new RectangleD(px, py, pw, ph), physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y);
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getRectangleTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getRectangleTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getRectangleTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;

                        an.Text = meta.getRectangleText(im, sc);
                        ome.units.quantity.Length fl = meta.getRectangleFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getRectangleStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getRectangleStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getRectangleFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        ome.xml.model.enums.FillRule fr = meta.getRectangleFillRule(im, sc);
                    }
                    else
                    if (type == "Ellipse")
                    {
                        an.type = ROI.Type.Ellipse;
                        an.id = meta.getEllipseID(im, sc);
                        double px = meta.getEllipseX(im, sc).doubleValue();
                        double py = meta.getEllipseY(im, sc).doubleValue();
                        double ew = meta.getEllipseRadiusX(im, sc).doubleValue();
                        double eh = meta.getEllipseRadiusY(im, sc).doubleValue();
                        //We convert the ellipse radius to System.Drawing.Rectangle
                        double w = ew * 2;
                        double h = eh * 2;
                        double x = px - ew;
                        double y = py - eh;
                        an.Rect = ToStageSpace(new RectangleD(px, py, w, h), physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y);
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getEllipseTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getEllipseTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getEllipseTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = meta.getEllipseText(im, sc);
                        ome.units.quantity.Length fl = meta.getEllipseFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getEllipseStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getEllipseStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getEllipseFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polygon")
                    {
                        an.type = ROI.Type.Polygon;
                        an.id = meta.getPolygonID(im, sc);
                        an.closed = true;
                        string pxs = meta.getPolygonPoints(im, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        pts = ToStageSpace(pts, physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y);
                        if (pts.Length > 100)
                        {
                            an.type = ROI.Type.Freeform;
                        }
                        an.AddPoints(pts);
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolygonTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolygonTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolygonTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = meta.getPolygonText(im, sc);
                        ome.units.quantity.Length fl = meta.getPolygonFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolygonStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolygonStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolygonFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Polyline")
                    {
                        an.type = ROI.Type.Polyline;
                        an.id = meta.getPolylineID(im, sc);
                        string pxs = meta.getPolylinePoints(im, sc);
                        PointD[] pts = an.stringToPoints(pxs);
                        for (int pi = 0; pi < pts.Length; pi++)
                        {
                            pts[pi] = ToStageSpace(pts[pi], physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y);
                        }
                        an.AddPoints(an.stringToPoints(pxs));
                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getPolylineTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getPolylineTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getPolylineTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;
                        an.Text = meta.getPolylineText(im, sc);
                        ome.units.quantity.Length fl = meta.getPolylineFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getPolylineStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getPolylineStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getPolylineFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                    }
                    else
                    if (type == "Label")
                    {
                        an.type = ROI.Type.Label;
                        an.id = meta.getLabelID(im, sc);

                        ome.xml.model.primitives.NonNegativeInteger nz = meta.getLabelTheZ(im, sc);
                        if (nz != null)
                            co.Z = nz.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nc = meta.getLabelTheC(im, sc);
                        if (nc != null)
                            co.C = nc.getNumberValue().intValue();
                        ome.xml.model.primitives.NonNegativeInteger nt = meta.getLabelTheT(im, sc);
                        if (nt != null)
                            co.T = nt.getNumberValue().intValue();
                        an.coord = co;

                        ome.units.quantity.Length fl = meta.getLabelFontSize(im, sc);
                        if (fl != null)
                            an.font = new Font(SystemFonts.DefaultFont.FontFamily, (float)fl.value().doubleValue(), FontStyle.Regular);
                        ome.xml.model.primitives.Color col = meta.getLabelStrokeColor(im, sc);
                        if (col != null)
                            an.strokeColor = System.Drawing.Color.FromArgb(col.getAlpha(), col.getRed(), col.getGreen(), col.getBlue());
                        ome.units.quantity.Length fw = meta.getLabelStrokeWidth(im, sc);
                        if (fw != null)
                            an.strokeWidth = (float)fw.value().floatValue();
                        ome.xml.model.primitives.Color colf = meta.getLabelFillColor(im, sc);
                        if (colf != null)
                            an.fillColor = System.Drawing.Color.FromArgb(colf.getAlpha(), colf.getRed(), colf.getGreen(), colf.getBlue());
                        PointD p = new PointD(meta.getLabelX(im, sc).doubleValue(), meta.getLabelY(im, sc).doubleValue());
                        an.AddPoint(ToStageSpace(p, physicalSizeX, physicalSizeY, volume.Location.X, volume.Location.Y));
                        an.Text = meta.getLabelText(im, sc);
                    }
                }
            }

            imageReader.close();
            return Annotations;
        }
        /// It takes a list of ROI objects and returns a string of all the ROI objects in the list
        /// 
        /// @param Annotations List of ROI objects
        /// 
        /// @return A string of the ROI's in the list.
        public static string ROIsToString(List<ROI> Annotations)
        {
            string s = "";
            for (int i = 0; i < Annotations.Count; i++)
            {
                s += ROIToString(Annotations[i]);
            }
            return s;
        }
        /// This function takes an ROI object and converts it to a string that can be written to a file
        /// 
        /// @param ROI The ROI object
        /// 
        /// @return A string
        public static string ROIToString(ROI an)
        {
            PointD[] points = an.GetPoints();
            string pts = "";
            for (int j = 0; j < points.Length; j++)
            {
                if (j == points.Length - 1)
                    pts += points[j].X.ToString() + "," + points[j].Y.ToString();
                else
                    pts += points[j].X.ToString() + "," + points[j].Y.ToString() + " ";
            }
            char sep = (char)34;
            string sColor = sep.ToString() + an.strokeColor.A.ToString() + ',' + an.strokeColor.R.ToString() + ',' + an.strokeColor.G.ToString() + ',' + an.strokeColor.B.ToString() + sep.ToString();
            string bColor = sep.ToString() + an.fillColor.A.ToString() + ',' + an.fillColor.R.ToString() + ',' + an.fillColor.G.ToString() + ',' + an.fillColor.B.ToString() + sep.ToString();

            string line = an.roiID + ',' + an.roiName + ',' + an.type.ToString() + ',' + an.id + ',' + an.shapeIndex.ToString() + ',' +
                an.Text + ',' + an.serie + ',' + an.coord.Z.ToString() + ',' + an.coord.C.ToString() + ',' + an.coord.T.ToString() + ',' + an.X.ToString() + ',' + an.Y.ToString() + ',' +
                an.W.ToString() + ',' + an.H.ToString() + ',' + sep.ToString() + pts + sep.ToString() + ',' + sColor + ',' + an.strokeWidth.ToString() + ',' + bColor + ',' + an.font.Size.ToString() + ',' + NewLine;
            return line;
        }
        /// It takes a string of comma separated values and returns an ROI object
        /// 
        /// @param sts the string that contains the ROI data
        /// 
        /// @return A ROI object.
        public static ROI StringToROI(string sts)
        {
            //Works with either comma or tab separated values.
            if (sts.StartsWith("<?xml") || sts.StartsWith("{"))
                return null;
            ROI an = new ROI();
            string val = "";
            bool inSep = false;
            int col = 0;
            double x = 0;
            double y = 0;
            double w = 0;
            double h = 0;
            string line = sts;
            bool points = false;
            char sep = '"';
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == sep)
                {
                    if (!inSep)
                    {
                        inSep = true;
                    }
                    else
                        inSep = false;
                    continue;
                }

                if ((c == ',' || c == '\t') && (!inSep || points))
                {
                    //ROIID,ROINAME,TYPE,ID,SHAPEINDEX,TEXT,C,Z,T,X,Y,W,H,POINTS,STROKECOLOR,STROKECOLORW,FILLCOLOR,FONTSIZE
                    if (col == 0)
                    {
                        //ROIID
                        an.roiID = val;
                    }
                    else
                    if (col == 1)
                    {
                        //ROINAME
                        an.roiName = val;
                    }
                    else
                    if (col == 2)
                    {
                        //TYPE
                        an.type = (ROI.Type)Enum.Parse(typeof(ROI.Type), val);
                    }
                    else
                    if (col == 3)
                    {
                        //ID
                        an.id = val;
                    }
                    else
                    if (col == 4)
                    {
                        //SHAPEINDEX/
                        an.shapeIndex = int.Parse(val);
                    }
                    else
                    if (col == 5)
                    {
                        //TEXT/
                        an.Text = val;
                    }
                    else
                    if (col == 6)
                    {
                        an.serie = int.Parse(val);
                    }
                    else
                    if (col == 7)
                    {
                        an.coord.Z = int.Parse(val);
                    }
                    else
                    if (col == 8)
                    {
                        an.coord.C = int.Parse(val);
                    }
                    else
                    if (col == 9)
                    {
                        an.coord.T = int.Parse(val);
                    }
                    else
                    if (col == 10)
                    {
                        x = double.Parse(val);
                    }
                    else
                    if (col == 11)
                    {
                        y = double.Parse(val);
                    }
                    else
                    if (col == 12)
                    {
                        w = double.Parse(val);
                    }
                    else
                    if (col == 13)
                    {
                        h = double.Parse(val);
                    }
                    else
                    if (col == 14)
                    {
                        //POINTS
                        if (c == ',')
                        {
                            inSep = true;
                            points = true;
                            val += c;
                            continue;
                        }
                        else
                        {
                            an.AddPoints(an.stringToPoints(val));
                            points = false;
                            an.Rect = new RectangleD(x, y, w, h);
                        }
                    }
                    else
                    if (col == 15)
                    {
                        //STROKECOLOR
                        string[] st = val.Split(',');
                        an.strokeColor = System.Drawing.Color.FromArgb(int.Parse(st[0]), int.Parse(st[1]), int.Parse(st[2]), int.Parse(st[3]));
                    }
                    else
                    if (col == 16)
                    {
                        //STROKECOLORW
                        an.strokeWidth = double.Parse(val);
                    }
                    else
                    if (col == 17)
                    {
                        //FILLCOLOR
                        string[] st = val.Split(',');
                        an.fillColor = System.Drawing.Color.FromArgb(int.Parse(st[0]), int.Parse(st[1]), int.Parse(st[2]), int.Parse(st[3]));
                    }
                    else
                    if (col == 18)
                    {
                        //FONTSIZE
                        double s = double.Parse(val);
                        an.font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, (float)s, System.Drawing.FontStyle.Regular);
                    }
                    col++;
                    val = "";
                }
                else
                    val += c;
            }

            return an;
        }
        /// This function takes a list of ROIs and converts them to a string, then writes that string to
        /// a file
        /// 
        /// @param filename the name of the file to be saved
        /// @param Annotations List of ROI objects
        public static void ExportROIsCSV(string filename, List<ROI> Annotations)
        {
            string con = columns;
            con += ROIsToString(Annotations);
            File.WriteAllText(filename, con);
        }
        /// It reads the CSV file and converts each line into a ROI object
        /// 
        /// @param filename The path to the CSV file.
        /// 
        /// @return A list of ROI objects.
        public static List<ROI> ImportROIsCSV(string filename)
        {
            List<ROI> list = new List<ROI>();
            if (!File.Exists(filename))
                return list;
            string[] sts = File.ReadAllLines(filename);
            //We start reading from line 1.
            for (int i = 1; i < sts.Length; i++)
            {
                list.Add(StringToROI(sts[i]));
            }
            return list;
        }
        /// ExportROIFolder(path, filename)
        /// 
        /// This function takes a folder path and a filename as input and exports all the ROIs in the
        /// folder as CSV files
        /// 
        /// @param path the path to the folder containing the OMERO ROI files
        /// @param filename the name of the file you want to export
        public static void ExportROIFolder(string path, string filename)
        {
            string[] fs = Directory.GetFiles(path);
            int i = 0;
            foreach (string f in fs)
            {
                List<ROI> annotations = OpenOMEROIs(f, 0);
                string ff = Path.GetFileNameWithoutExtension(f);
                ExportROIsCSV(path + "//" + ff + "-" + i.ToString() + ".csv", annotations);
                i++;
            }
        }

        private static BioImage bstats = null;
        private static bool update = false;
        /// It takes a BioImage object, and for each channel, it calculates the mean histogram of the
        /// channel, and then it calculates the mean histogram of the entire image
        /// 
        /// @param BioImage This is the image object that contains the image data.
        /// @param updateImageStats if true, the image stats will be updated.
        public static void AutoThreshold(BioImage b, bool updateImageStats)
        {
            bstats = b;
            Statistics statistics = null;
            if (b.bitsPerPixel > 8)
                statistics = new Statistics(true);
            else
                statistics = new Statistics(false);
            for (int i = 0; i < b.Buffers.Count; i++)
            {
                if (b.Buffers[i].Stats == null || updateImageStats)
                    b.Buffers[i].Stats = Statistics.FromBytes(b.Buffers[i]);
                if (b.Buffers[i].RGBChannelsCount == 1)
                    statistics.AddStatistics(b.Buffers[i].Stats[0]);
                else
                {
                    for (int r = 0; r < b.Buffers[i].RGBChannelsCount; r++)
                    {
                        statistics.AddStatistics(b.Buffers[i].Stats[r]);
                    }
                }
            }

            for (int c = 0; c < b.Channels.Count; c++)
            {
                Statistics[] sts;
                if (b.RGBChannelCount == 1)
                {
                    sts = new Statistics[1];
                    if (b.bitsPerPixel > 8)
                    {
                        sts[0] = new Statistics(true);
                    }
                    else
                        sts[0] = new Statistics(false);
                }
                else
                {
                    sts = new Statistics[b.Buffers[0].RGBChannelsCount];
                    if (b.RGBChannelCount == 3)
                    {
                        sts[0] = new Statistics(true);
                        sts[1] = new Statistics(true);
                        sts[2] = new Statistics(true);
                    }
                    else
                    {
                        sts[0] = new Statistics(false);
                        sts[1] = new Statistics(false);
                        sts[2] = new Statistics(false);
                        sts[3] = new Statistics(false);
                    }
                }

                for (int z = 0; z < b.SizeZ; z++)
                {
                    for (int t = 0; t < b.SizeT; t++)
                    {
                        int ind = 0;
                        if (b.RGBChannelCount > 1)
                            ind = b.Coords[z, 0, t];
                        else
                            ind = b.Coords[z, c, t];
                        if (b.Buffers[ind].RGBChannelsCount == 1)
                            sts[0].AddStatistics(b.Buffers[ind].Stats[0]);
                        else
                        {
                            sts[0].AddStatistics(b.Buffers[ind].Stats[0]);
                            sts[1].AddStatistics(b.Buffers[ind].Stats[1]);
                            sts[2].AddStatistics(b.Buffers[ind].Stats[2]);
                            if (b.Buffers[ind].RGBChannelsCount == 4)
                                sts[3].AddStatistics(b.Buffers[ind].Stats[3]);
                        }
                    }
                }
                if (b.RGBChannelCount == 1)
                    sts[0].MeanHistogram();
                else
                {
                    sts[0].MeanHistogram();
                    sts[1].MeanHistogram();
                    sts[2].MeanHistogram();
                    if (b.Buffers[0].RGBChannelsCount == 4)
                        sts[3].MeanHistogram();
                }
                b.Channels[c].stats = sts;
            }
            statistics.MeanHistogram();
            b.statistics = statistics;

        }
        /// It takes the current image, and the current image's statistics, and updates the image's
        /// statistics
        public static void AutoThreshold()
        {
            AutoThreshold(bstats, update);
        }
        /// "This function creates a new thread and starts it, and the thread calls the AutoThreshold
        /// function."
        /// 
        /// The AutoThreshold function is the function that actually does the work.
        /// 
        /// @param BioImage This is a class that contains the image data and some other information.
        public static void AutoThresholdThread(BioImage b)
        {
            bstats = b;
            Thread th = new Thread(AutoThreshold);
            th.Start();
        }
        /// It disposes of all the buffers and channels in the image, removes the image from the Images
        /// list, and then calls the garbage collector
        public void Dispose()
        {
            for (int i = 0; i < Buffers.Count; i++)
            {
                Buffers[i].Dispose();
            }
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i].Dispose();
            }
            Images.RemoveImage(this);
            GC.Collect();
        }
        /// This function returns a string that contains the filename of the object, and the location of
        /// the object
        /// 
        /// @return The filename, and the location of the volume.
        public override string ToString()
        {
            return Filename.ToString() + ", (" + Volume.Location.X + ", " + Volume.Location.Y + ", " + Volume.Location.Z + ")";
        }

        /// This function divides each pixel in the image by a constant value
        /// 
        /// @param BioImage a class that contains a list of buffers (which are 2D arrays of floats)
        /// @param b the value to divide by
        /// 
        /// @return The image itself.
        public static BioImage operator /(BioImage a, float b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] / b;
            }
            return a;
        }
        /// This function multiplies each pixel in the image by a constant value
        /// 
        /// @param BioImage a class that contains a list of buffers (which are 2D arrays of floats)
        /// @param b the image to be multiplied
        /// 
        /// @return The image itself.
        public static BioImage operator *(BioImage a, float b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] * b;
            }
            return a;
        }
        /// This function adds a constant value to each pixel in the image
        /// 
        /// @param BioImage a class that contains a list of buffers (float[])
        /// @param b the value to add to the image
        /// 
        /// @return The image itself.
        public static BioImage operator +(BioImage a, float b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] + b;
            }
            return a;
        }
        /// This function subtracts a float from each pixel in the image
        /// 
        /// @param BioImage a class that contains a list of buffers (which are 2D arrays of floats)
        /// @param b the value to subtract from the image
        /// 
        /// @return The image is being returned.
        public static BioImage operator -(BioImage a, float b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] - b;
            }
            return a;
        }

        /// This function divides the color of each pixel in the image by the color of the second image
        /// 
        /// @param BioImage a class that contains a list of ColorS objects.
        /// @param ColorS a struct that contains a byte for each color channel (R, G, B, A)
        /// 
        /// @return A BioImage object.
        public static BioImage operator /(BioImage a, ColorS b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] / b;
            }
            return a;
        }
        /// It takes a BioImage and a ColorS and returns a BioImage
        /// 
        /// @param BioImage a class that contains a list of ColorS objects.
        /// @param ColorS a struct that contains a byte for each color channel (R, G, B, A)
        /// 
        /// @return A BioImage object.
        public static BioImage operator *(BioImage a, ColorS b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] * b;
            }
            return a;
        }
        /// It takes a BioImage object and a ColorS object and adds the ColorS object to each of the
        /// buffers in the BioImage object
        /// 
        /// @param BioImage a class that contains a list of ColorS objects.
        /// @param ColorS a struct that contains a byte for each color channel (R, G, B, A)
        /// 
        /// @return A BioImage object
        public static BioImage operator +(BioImage a, ColorS b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] + b;
            }
            return a;
        }
        /// The function subtracts a color from each pixel in the image
        /// 
        /// @param BioImage a class that contains a list of ColorS objects.
        /// @param ColorS a struct that contains a byte for each color channel (R, G, B, A)
        /// 
        /// @return The image is being returned.
        public static BioImage operator -(BioImage a, ColorS b)
        {
            for (int i = 0; i < a.Buffers.Count; i++)
            {
                a.Buffers[i] = a.Buffers[i] - b;
            }
            return a;
        }
    }
}