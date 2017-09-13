using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Coloriz
{
    public static class Ext
    {        
        public static List<Segment> ToSegmentList(this SortedList<int, List<int>> _list)
        {
            var result = new List<Segment>();

            //foreach(var e in _list)
            //    result.Add(new Segment()

            return result;
        }
    }
    public class Segment
    {
        int Parent { get; set; }
        public double MaxWeight { get; set; }
        private List<int> childs;

        public Segment(int _maxWeight, int _parent, List<int> _childs)
        {
            Parent = _parent;
            childs = _childs;
        }
        public void Connect(Segment _segment)
        {
            childs.AddRange(_segment.childs);
            if (MaxWeight < _segment.MaxWeight)
                MaxWeight = _segment.MaxWeight;
            _segment = null;
        }
    }
    class Picture
    {
       
        private class SetPixels
        {
            public int Parent { get; set; }
            byte[] color;
            List<int> members;
            public SetPixels(int _parent, byte[] _color)
            {
                members = new List<int>();
                Parent = _parent; color = _color;
            }
            public void Add(int _memberInd)
            {
                members.Add(_memberInd);
            }
            public void Del(int _memberInd)
            {
                members.Remove(_memberInd);
            }

        }
        private class DisjointSet
        {
            int length;
            public int[] P { get; set; }
            int[] Rank { get; set; }

            public void rar()
            {
                for (int i = 0; i < P.Length; i++)
                    P[i] = Find(i);
            }

            public DisjointSet(int _length)
            {
                length = _length;
                P = new int[length];
                Rank = new int[length];

                for (int i = 0; i < length; i++)
                    MakeSet(i);
            }
            public void MakeSet(int x)
            {
                P[x] = x;
                Rank[x] = 0;
            }
            public int Find(int x)
            {
                return (x == P[x] ? x : P[x] = Find(P[x]));
            }
            public void Union(int x, int y)
            {
                if ((x = Find(x)) == (y = Find(y)))
                    return;

                if (Rank[x] < Rank[y])
                    P[x] = y;
                else
                    P[y] = x;

                if (Rank[x] == Rank[y])
                    ++Rank[x];
            }
        }
        public class BitmapLocker
        {
            public int bytes { get; private set; }
            public IntPtr ptr { get; private set; }
            public BitmapData pictureData { get; private set; }
            public byte[] rgbValues { get; set; }
            public BitmapLocker(int _bytes, IntPtr _ptr, BitmapData _pictureData, byte[] _rgbValues)
            {
                bytes = _bytes;
                ptr = _ptr;
                pictureData = _pictureData;
                rgbValues = _rgbValues;
            }
        }
        private class Edge
        {
            public double Weight { get; set; }
            public int Vertex1 { get; set; }
            public int Vertex2 { get; set; }
            public Edge(double _w, int _v1, int _v2)
            {
                Weight = _w; Vertex1 = _v1; Vertex2 = _v2;
            }
        }
       

        private byte[] resource;
        private Bitmap resBitmap;
        private int width, height, length;

        private BitmapLocker bLocker;
        private bool isLock;

        public Picture(Bitmap _source)
        {
            resBitmap = _source;
            isLock = false;
            width = resBitmap.Width;
            height = resBitmap.Height;
            length = width * height;
        }
        public static BitmapLocker LockBitmap(Bitmap _source)
        {
            int w = _source.Width, h = _source.Height, l = w * h;
            var pData = _source.LockBits(new Rectangle(0, 0, w, h),
               ImageLockMode.ReadWrite, _source.PixelFormat);
            var pointer = pData.Scan0;
            var len = Math.Abs(pData.Stride) * _source.Height;///////
            var rgb = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(pointer, rgb, 0, len);

            return new BitmapLocker(len, pointer, pData, rgb);
        }
        public static void UnLockBitmap(BitmapLocker _bLocker, Bitmap _source)
        {
            if (_bLocker != null)
            {
                System.Runtime.InteropServices.
                    Marshal.Copy(_bLocker.rgbValues, 0, _bLocker.ptr, _bLocker.bytes);
                _source.UnlockBits(_bLocker.pictureData);
            }
        }
        public static byte[] BitmapToGrayColorMas(Bitmap _source)
        {
            int h = _source.Width, w = _source.Height, len = w * h;
            var bLocker = LockBitmap(_source);
            var rgbMass = bLocker.rgbValues;
            var result = new byte[len];

            for (int i = 0; i < bLocker.bytes; i += 3)
                result[i / 3] = (byte)(rgbMass[i] * 0.3 + rgbMass[i + 1] *
                    0.59 + rgbMass[i + 2] * 0.11);
            UnLockBitmap(bLocker, _source);
            return result;
        }
        public static Bitmap GrayColorMasToBitmap(byte[] _mas, int _width, int _height)
        {
            Bitmap bMap = new Bitmap(_width, _height, PixelFormat.Format24bppRgb);

            var bLocker = LockBitmap(bMap);
            var rgbMass = bLocker.rgbValues;
            for (int i = 0; i < bLocker.bytes; i += 3)
                rgbMass[i] = rgbMass[i + 1] = rgbMass[i + 2] = _mas[i / 3];
            bLocker.rgbValues = rgbMass;
            UnLockBitmap(bLocker, bMap);
            return bMap;
        }

        public Bitmap GetImage()
        {
            return resBitmap;
        }
        public bool Lock()
        {
            if (!isLock)
            {
                bLocker = LockBitmap(resBitmap);
                return isLock = true;
            }
            return false;
        }
        public bool UnLock()
        {
            if (isLock)
            {
                UnLockBitmap(bLocker, resBitmap);
                return isLock = false;
            }
            return true;
        }
        public bool IsLock()
        {
            return isLock;
        }

        public byte[] GetPixelRGB(int x, int y)
        {
            int indexInMas = (x + y * width) * 3;
            return
                new byte[]
                {
                    bLocker.rgbValues[indexInMas++],
                    bLocker.rgbValues[indexInMas++],
                    bLocker.rgbValues[indexInMas]
                };
        }
        private byte GetPixelGray(int indexInMas)
        {
            return resource[indexInMas];
        }

        public void SetPixelRgb(int x, int y, byte[] color)
        {
            int indexInMas = (x + y * width) * 3;
            bLocker.rgbValues[indexInMas++] = color[0];
            bLocker.rgbValues[indexInMas++] = color[1];
            bLocker.rgbValues[indexInMas]   = color[2];
        }
        private void Set(int indexInMas, byte brightness)
        {
            resource[indexInMas] = brightness;
        }

        public void GaussBlur(int radius)
        {
            resource = BitmapToGrayColorMas(resBitmap);
            int count = 2 * radius - 1;

            for (int i = radius; i < width - radius; i++)
                for (int j = 0; j < height; j++)
                {
                    int c = (i + j * width),
                        p = 0;
                    for (int l = -radius + 1; l < radius; l++)
                        p += resource[c + l];
                    resource[c] = (byte)(p / count);
                }
            for (int i = 0; i < width; i++)
                for (int j = radius; j < height - radius; j++)
                {
                    int c = (i + j * width),
                        p = 0;
                    for (int l = -radius + 1; l < radius; l++)
                        p += resource[c + l * width];
                    resource[c] = (byte)(p / count);
                }
            resBitmap = GrayColorMasToBitmap(resource, width, height);
        }
        private double Distance(int ind1, int ind2)
        {
            double w1 = GetPixelGray(ind1),
                   w2 = GetPixelGray(ind2);
            return Math.Sqrt(Math.Pow(w1 - w2, 2) + Math.Pow(ind1 % width - ind2 % width, 2) + Math.Pow(ind1 / width - ind2 / width, 2));
        }
        private DisjointSet MakeDisjointSet(List<Edge> list, double coef)
        {
            DisjointSet set = new DisjointSet(length);

            foreach (var e in list)
                if (set.Find(e.Vertex1) != set.Find(e.Vertex2) && e.Weight < coef)
                    set.Union(e.Vertex1, e.Vertex2);
            return set;
        }
        private List<Edge> CalcWeight()
        {
            var list = new List<Edge>();

            int x, y, i2, width2 = width - 1, height2 = height - 1;

            for (int i = 0; i < length; i++)
            {
                x = i % width; y = i / width;
                if (x > 0)
                    list.Add(new Edge(Distance(i, i2 = i - 1), i, i2));
                if (x < width2)
                    list.Add(new Edge(Distance(i, i2 = i + 1), i, i2));
                if (y > 0)
                    list.Add(new Edge(Distance(i, i2 = i - width), i, i2));
                if (y < height2)
                    list.Add(new Edge(Distance(i, i2 = i + width), i, i2));
            }

            return list.OrderBy(o => o.Weight).ToList();
        }


        class Element
        {
            public int child { get; set; }
            public int parent { get; set; }
            public Element(int a, int b)
            {
                parent = a; child = b;
            }
        }
        public Bitmap Segmentation(double coef)
        {
            var weightList = CalcWeight();
            DisjointSet djSet = MakeDisjointSet(weightList, coef);

            var mas = djSet.P;

            SortedList<int, List<int>> segments = new SortedList<int, List<int>>();

            for (int i = 0; i < length; i++)
            {
                int parent = djSet.Find(mas[i]);
                List<int> kp;
                segments.TryGetValue(parent, out kp);
                if (kp != null)
                    kp.Add(i);
                else
                {
                    var l = new List<int>();
                    l.Add(parent); l.Add(i);
                    segments.Add(parent, l);
                }
            }

            Random rand = new Random();
            byte[] r = new byte[segments.Count()];
            byte[] g = new byte[segments.Count()];
            byte[] b = new byte[segments.Count()];
            rand.NextBytes(r);
            rand.NextBytes(g);
            rand.NextBytes(b);

            Lock();
            int count = 0;
            List<Color> colList = new List<Color>();
            foreach (var s in segments)
            {
                {
                    foreach (var item in s.Value)
                    {
                        SetPixelRgb(item % width, item / width, new byte[] { r[count], g[count], b[count] });
                    }
                    count++;
                }
            }
            UnLock();
            int ac = 1;
            

            return resBitmap;
            //return GrayColorMasToBitmap(resource, width, height);
        }
        private bool Compare(Color c, Color k)
        {
            return c.R == k.R && c.G == k.G && c.B == k.B;
        }

    }
   
}
