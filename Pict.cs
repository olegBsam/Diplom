using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coloriz
{

    class Pict
    {
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
        private Bitmap res;
        private int width, height, length;
        private bool isLock;

        private int bytes;
        private IntPtr ptr;
        private BitmapData pictureData;
        private byte[] rgbValues;

        public Pict(Bitmap bm)
        {
            res = new Bitmap(bm);
            isLock = false;
            width = res.Width;
            height = res.Height;
            length = width * height;
        }
#region Lock's
        /// <summary>
        /// Если до вызова было забл, то вернет false
        /// </summary>
        /// <returns></returns>
        public bool Lock()
        {
            if (!isLock)
            {
                pictureData =
                  res.LockBits(new Rectangle(0, 0, width, height),
                  ImageLockMode.ReadWrite,
                  res.PixelFormat);

                ptr = pictureData.Scan0;
                bytes = res.Width * res.Height * 3; // Math.Abs(pictureData.Stride) * picture.Height;
                rgbValues = new byte[bytes];

                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                return isLock = true;
            }
            return false;
        }
        /// <summary>
        /// Если до вызова было разбл, то вернет true
        /// </summary>
        /// <returns></returns>
        public bool UnLock()
        {
            if (isLock)
            {
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
                res.UnlockBits(pictureData);
                return isLock = false;
            }
            return true;
        }
        public bool IsLock()
        {
            return isLock;
        }
        #endregion
#region Get's
        public byte[] GetPixel(int x, int y)
        {
            int indexInMas = (x + y * width) * 3;
            return 
                new byte[] 
                {
                    rgbValues[indexInMas++],
                    rgbValues[indexInMas++],
                    rgbValues[indexInMas]
                };
        }
        public byte[] GetPixel(int indexInMas)
        {
            return
                new byte[]
                {
                    rgbValues[indexInMas++],
                    rgbValues[indexInMas++],
                    rgbValues[indexInMas]
                };
        }
        public byte GetPixelColor(int indexInMas)
        {
            return rgbValues[indexInMas];
        }
        public Bitmap GetImage()
        {
            return res;
        }
        #endregion
#region Set's
        public void Set(int x, int y, byte[] color)
        {
            int indexInMas = (x + y * width) * 3;
            rgbValues[indexInMas++] = color[0];
            rgbValues[indexInMas++] = color[1];
            rgbValues[indexInMas] = color[2];
        }
        public void Set(int indexInMas, byte[] color)
        {
            rgbValues[indexInMas++] = color[0];
            rgbValues[indexInMas++] = color[1];
            rgbValues[indexInMas] = color[2];
        }
#endregion
        public void GaussBlur(int radius)
        {
            Lock();
            int count = 2 * radius - 1;
            for (int i = radius; i < width - radius; i++)
                for (int j = 0; j < height; j++)
                {
                    int c = (i + j * width) * 3,
                        p = 0;
                    for (int l = -radius + 1; l < radius; l++)
                        p += rgbValues[c + l * 3];
                    byte brightness = (byte)(p / count);
                    rgbValues[c] = rgbValues[c + 1] = rgbValues[c + 2] = brightness;
                }
            for (int i = 0; i < width; i++)
                for (int j = radius; j < height - radius; j++)
                {
                    int c = (i + j * width) * 3,
                        p = 0;
                    for (int l = -radius + 1; l < radius; l++)
                        p += rgbValues[c + l * width * 3];

                    byte brightness = (byte)(p / count);
                    rgbValues[c] = rgbValues[c + 1] = rgbValues[c + 2] = brightness;
                }
            UnLock();
        }

        private double Distance(int ind1, int ind2)
        {
            double w1 = GetPixelColor(ind1),
                   w2 = GetPixelColor(ind2);
            return Math.Sqrt(Math.Pow(w1 - w2, 2) + Math.Pow(ind1 % width - ind2 % width, 2) + Math.Pow(ind1 / width - ind2 / width, 2));
        }
        public Bitmap Segmentation(double coef)
        {
            if (isLock) UnLock();
            Bitmap segments = new Bitmap(res);
            if (!isLock) Lock();

            DisjointSet djSet = MakeDisjointSet(CalcWeight(), coef);

            return segments;
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

    }
}
