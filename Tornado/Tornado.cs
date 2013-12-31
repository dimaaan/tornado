using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;

namespace DimaSoft
{
    /// <summary>
    /// Summary description for Tornado.
    /// </summary>
    public class Tornado : System.Windows.Forms.Control
    {
        private Bitmap src;

        private struct AxisPoint
        {
            public double x, y, z;
            public double wx, wy, wz;
            public double r;
        }

        private struct Particle
        {
            public double x, y, z,
                            vx, vy, vz;
            public int fulcrum;
            public int xscr, yscr, xscr1, yscr1;
        }

        // --------------------------------------------------------------

        private const int AXIS_LEN = 512;
        private const int NUM_PARTICLES = 1024;

        // --------------------------------------------------------------
        private int x = 0;
        double gamma = 1.9;
        private AxisPoint[] Axis = new AxisPoint[AXIS_LEN + 1];
        private Particle[] tornado = new Particle[NUM_PARTICLES];
        private System.Windows.Forms.Timer tmrRedraw;
        private System.ComponentModel.IContainer components;
        private int color = 255;
        Color[] pal = new Color[256];

        // ===============================================================

        public Tornado()
        {
            int i, m;
            Random rand = new Random();

            InitializeComponent();

            SetStyle(ControlStyles.UserPaint
                | ControlStyles.DoubleBuffer
                | ControlStyles.AllPaintingInWmPaint, true);

            MakeAxis(0);
            InitParticles();

            for (i = 0; i < NUM_PARTICLES; i++)
            {
                tornado[i].xscr = rand.Next() % 320/*base.Width*/;
                tornado[i].yscr = rand.Next() % 200/*base.Height*/;
            }
            MakeAxis(6400);
            for (i = 0; i < NUM_PARTICLES; i++)
            {
                m = tornado[i].fulcrum;
                tornado[i].xscr1 = tornado[i].xscr;
                tornado[i].yscr1 = tornado[i].yscr;
                tornado[i].xscr = (int)((Axis[m].x + tornado[i].x) * 180 + 160)/*65536*/;
                tornado[i].yscr = (int)(175 - (Axis[m].y + tornado[i].y) * 150)/*65536*/;
            }

            src = new Bitmap(320, 240);
            Color c;
            int r, g, b;
            for (i = 0; i < 256; i++)
            {
                r = gamcor(Convert.ToDouble(i) / 3 / 255);
                g = gamcor(Convert.ToDouble((i + 1) / 3 / 255));
                b = gamcor(Convert.ToDouble((i + 2) / 3 / 255));
                c = Color.FromArgb(r, g, b);
                //src.Palette.Entries[i] = c;
                pal[i] = c;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrRedraw = new System.Windows.Forms.Timer(this.components);
            // 
            // tmrRedraw
            // 
            this.tmrRedraw.Enabled = true;
            this.tmrRedraw.Interval = 50;
            this.tmrRedraw.Tick += new System.EventHandler(this.tmrRedraw_Tick);

        }
        #endregion

        // ===============================================================

        private int gamcor(double v)
        {
            if (v < 0.001)
                return 0;
            else if (v > 1)
                return 63;
            else
                return (int)(63 * Math.Exp(Math.Log(v) / gamma));
        }

        private static void MidPoint(double y0, double v0,
            double y1, double v1, out double y, out double v)
        {
            y = (y0 + y1) / 2 + (v0 - v1) / 8;
            v = ((y1 - y0) * 6 - v0 - v1) / 4;
        }

        // ---------------------------------------------------------------

        private void MakeAxis1(int n1, int n2)
        {
            if (!(n1 + 1 < n2)) return;

            int n = (n1 + n2) / 2;
            Axis[n1].wx /= 2;
            Axis[n2].wx /= 2;
            //    Axis[n].wx/=2;
            Axis[n1].wy /= 2;
            Axis[n2].wy /= 2;
            //    Axis[n].wy/=2;
            Axis[n1].wz /= 2;
            Axis[n2].wz /= 2;
            //    Axis[n].wz/=2;
            MidPoint(Axis[n1].x, Axis[n1].wx, Axis[n2].x, Axis[n2].wx,
                out Axis[n].x, out Axis[n].wx);
            MidPoint(Axis[n1].y, Axis[n1].wy, Axis[n2].y, Axis[n2].wy,
                out Axis[n].y, out Axis[n].wy);
            MidPoint(Axis[n1].z, Axis[n1].wz, Axis[n2].z, Axis[n2].wz,
                out Axis[n].z, out Axis[n].wz);
            MakeAxis1(n1, n); // до средней точки
            MakeAxis1(n, n2); // после средней точки
        }

        // ---------------------------------------------------------------

        private void MakeAxis(double t)
        {
            int n;

            Axis[0].x = Math.Sin(t / 13) / 2;
            Axis[0].y = Math.Sin(t / 7) / 8;
            Axis[0].z = Math.Sin(t / 4) / 8;

            Axis[0].wx = 0;
            Axis[0].wy = 4;
            Axis[0].wz = 0;

            Axis[AXIS_LEN].x = Math.Sin(t / 5.5) / 2;
            Axis[AXIS_LEN].y = 1;
            Axis[AXIS_LEN].z = Math.Cos(t / 7) / 2;

            Axis[AXIS_LEN].wx = Axis[AXIS_LEN].x - Axis[0].x + Math.Cos(t / 5) / 2;
            Axis[AXIS_LEN].wy = 1;
            Axis[AXIS_LEN].wz = Math.Cos(t / 5) / 2;

            MakeAxis1(0, AXIS_LEN);

            for (n = 0; n <= AXIS_LEN; n++)
            {
                Axis[n].r = 0.3 * Math.Sqrt((double)n / AXIS_LEN) + 0.03;
            }
        }

        // ---------------------------------------------------------------

        private void InitParticles()
        {
            int n;
            Random rand = new Random();

            for (n = 0; n < NUM_PARTICLES; n++)
            {
                int m;
                tornado[n].fulcrum = rand.Next() % AXIS_LEN;
                m = tornado[n].fulcrum;
                tornado[n].x = Math.Sin(rand.Next()) * Axis[m].r;
                tornado[n].y = Math.Sin(rand.Next()) * Axis[m].r;
                tornado[n].z = Math.Sin(rand.Next()) * Axis[m].r;
                tornado[n].vx = 0;
                tornado[n].vy = 0;
                tornado[n].vz = 0;
            };
        }

        // ---------------------------------------------------------------

        private void NetxStep()
        {
            int n, m;
            double r, SqrtR;
            Random rand = new Random();

            for (n = 0; n < NUM_PARTICLES; n++)
            {
                m = tornado[n].fulcrum;
                tornado[n].vx = tornado[n].y * Axis[m].wz - tornado[n].z * Axis[m].wy;
                tornado[n].vy = tornado[n].z * Axis[m].wx - tornado[n].x * Axis[m].wz;
                tornado[n].vz = tornado[n].x * Axis[m].wy - tornado[n].y * Axis[m].wx;
                r = tornado[n].x * tornado[n].x +
                    tornado[n].y * tornado[n].y +
                    tornado[n].z * tornado[n].z;
                r = Math.Sqrt(r) + 0.01;
                SqrtR = Math.Sqrt(r) * 16;
                tornado[n].x += tornado[n].vx / r;
                tornado[n].y += tornado[n].vy / r;
                tornado[n].z += tornado[n].vz / r;
                if (r > Axis[m].r)
                {
                    tornado[n].x *= Axis[m].r / r;
                    tornado[n].y *= Axis[m].r / r;
                    tornado[n].z *= Axis[m].r / r;
                }
                tornado[n].x += (rand.Next() % 100) * 0.0001 - 0.005;
                tornado[n].y += (rand.Next() % 100) * 0.0001 - 0.005;
                tornado[n].z += (rand.Next() % 100) * 0.0001 - 0.005;
            }
        }

        // ---------------------------------------------------------------

        private void MovePixels()
        {
            for (int i = 0; i < NUM_PARTICLES; i++)
            {
                tornado[i].fulcrum++;
                if (tornado[i].fulcrum >= AXIS_LEN) tornado[i].fulcrum = 0;
            }
        }

        private void AddPixel(int x, int y, int c)
        {
            src.SetPixel(x, y, pal[c]);
        }

        private void PutPixel(int x, int y)
        {
            uint dx = (uint)x & 0xFFFF;
            uint dy = (uint)y & 0xFFFF;

            if (x < 0 || y < 0 || x > 318 || y > 198)
                return;

            AddPixel(x, y, (int)(((color * (65536 - dy)) >> 16) * (65536 - dx)) >> 16);
            AddPixel(x + 1, y, (int)(((color * (65536 - dy)) >> 16) * dx) >> 16);
            AddPixel(x, y + 1, (int)(((color * dy) >> 16) * (65536 - dx)) >> 16);
            AddPixel(x + 1, y + 1, (int)(((color * dy) >> 16) * dx) >> 16);
        }

        private void Line(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int l = (dx * dx + dy * dy) >> 16;

            if (l == 0)
                l = 1;

            dx /= l;
            dy /= l;

            int x = x1 + dx / 2;
            int y = y1 + dy / 2;
            for (int i = 0; i < l; i++)
            {
                PutPixel(x, y);
                x += dx;
                y += dy;
            }
        }

        // ===============================================================

        protected override void OnPaint(PaintEventArgs pe)
        {
            int m;

            base.OnPaint(pe);
            MakeAxis(x * 0.01);

            for (int i = 0; i < NUM_PARTICLES; i++)
            {
                m = tornado[i].fulcrum;
                tornado[i].xscr1 = tornado[i].xscr;
                tornado[i].yscr1 = tornado[i].yscr;
                tornado[i].xscr = (int)((Axis[m].x + tornado[i].x) * 180 + 160);
                tornado[i].yscr = (int)(175 - (Axis[m].y + tornado[i].y) * 150);
                if (m > 1)
                    color = (int)-(tornado[i].z / Axis[m].r) * 256 + 128;
                if (m < AXIS_LEN / 16)
                    color = color * m / (AXIS_LEN / 16);
                if (color < 0)
                    color = 0;
                if (color > 300)
                    color = 300;

                Line(tornado[i].xscr, tornado[i].yscr, tornado[i].xscr1, tornado[i].yscr1);
                pe.Graphics.DrawImageUnscaled(src, 0, 0);
                src = new Bitmap(320, 240);
                x++;
                NetxStep();
                MovePixels();
            }

        }

        private void tmrRedraw_Tick(object sender, System.EventArgs e)
        {
            Invalidate();
        }
    }
}
