using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CensorBrushApp
{
    // ================= theme (follows Windows light/dark, like Notepad) =================
    static class Theme
    {
        public static bool Dark;
        public static Color Bar, Canvas, Text, Secondary, Border, Hover, Pressed;
        public static Color Accent, AccentButton, AccentButtonHover, Track, SegBg, SegSelected, Pill, Disabled, ImageBorder;

        static Theme()
        {
            Apply(DetectSystemDark());
        }

        public static bool DetectSystemDark()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (k != null)
                    {
                        object v = k.GetValue("AppsUseLightTheme");
                        if (v is int && (int)v == 0) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static void Apply(bool dark)
        {
            Dark = dark;
            if (!Dark)
            {
                Bar = Color.FromArgb(249, 249, 249);
                Canvas = Color.FromArgb(231, 231, 231);
                Text = Color.FromArgb(27, 27, 27);
                Secondary = Color.FromArgb(93, 93, 93);
                Border = Color.FromArgb(224, 224, 224);
                Hover = Color.FromArgb(234, 234, 234);
                Pressed = Color.FromArgb(222, 222, 222);
                Accent = Color.FromArgb(0, 103, 192);
                AccentButton = Color.FromArgb(0, 103, 192);
                AccentButtonHover = Color.FromArgb(25, 117, 197);
                Track = Color.FromArgb(191, 191, 191);
                SegBg = Color.FromArgb(236, 236, 236);
                SegSelected = Color.White;
                Pill = Color.FromArgb(238, 238, 238);
                Disabled = Color.FromArgb(172, 172, 172);
                ImageBorder = Color.FromArgb(185, 185, 185);
            }
            else
            {
                Bar = Color.FromArgb(32, 32, 32);
                Canvas = Color.FromArgb(24, 24, 24);
                Text = Color.FromArgb(255, 255, 255);
                Secondary = Color.FromArgb(163, 163, 163);
                Border = Color.FromArgb(51, 51, 51);
                Hover = Color.FromArgb(45, 45, 45);
                Pressed = Color.FromArgb(56, 56, 56);
                Accent = Color.FromArgb(76, 194, 255);
                AccentButton = Color.FromArgb(0, 103, 192);
                AccentButtonHover = Color.FromArgb(25, 117, 197);
                Track = Color.FromArgb(77, 77, 79);
                SegBg = Color.FromArgb(43, 43, 43);
                SegSelected = Color.FromArgb(58, 58, 58);
                Pill = Color.FromArgb(43, 43, 43);
                Disabled = Color.FromArgb(96, 96, 96);
                ImageBorder = Color.FromArgb(68, 68, 68);
            }
        }

        public static Color Blend(Color fg, Color bg, float fgAmount)
        {
            float a = Math.Max(0f, Math.Min(1f, fgAmount));
            int r = (int)Math.Round(bg.R + (fg.R - bg.R) * a);
            int g = (int)Math.Round(bg.G + (fg.G - bg.G) * a);
            int b = (int)Math.Round(bg.B + (fg.B - bg.B) * a);
            return Color.FromArgb(Math.Max(0, Math.Min(255, r)), Math.Max(0, Math.Min(255, g)), Math.Max(0, Math.Min(255, b)));
        }

        // Guaranteed mathematically sound rounded rectangle with clamped corner radius
        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            if (r.Width <= 0 || r.Height <= 0) return p;
            if (radius <= 0) { p.AddRectangle(r); return p; }
            int maxR = Math.Min(r.Width, r.Height) / 2;
            if (radius > maxR) radius = maxR;
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ================= vector icon art (crisp at any DPI) =================
    static class IconArt
    {
        static Pen PenFor(Color c) { return PenFor(c, 1.5f); }
        static Pen PenFor(Color c, float w) { Pen p = new Pen(c, w); p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round; return p; }
        static Rectangle Box(Rectangle r, int s) { return new Rectangle(r.X + (r.Width - s) / 2, r.Y + (r.Height - s) / 2, s, s); }

        public static void Fit(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.58f));
            using (Pen p = PenFor(c, Math.Max(1.2f, 1.5f * r.Height / 32f)))
            {
                int l = Math.Max(3, b.Width / 3);
                g.DrawLine(p, b.X, b.Y + l, b.X, b.Y); g.DrawLine(p, b.X, b.Y, b.X + l, b.Y);
                g.DrawLine(p, b.Right - l, b.Y, b.Right, b.Y); g.DrawLine(p, b.Right, b.Y, b.Right, b.Y + l);
                g.DrawLine(p, b.Right, b.Bottom - l, b.Right, b.Bottom); g.DrawLine(p, b.Right, b.Bottom, b.Right - l, b.Bottom);
                g.DrawLine(p, b.X + l, b.Bottom, b.X, b.Bottom); g.DrawLine(p, b.X, b.Bottom, b.X, b.Bottom - l);
            }
        }

        public static void ThemeAuto(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.58f));
            using (Pen p = PenFor(c, Math.Max(1.2f, 1.5f * r.Height / 32f)))
            using (Brush br = new SolidBrush(c))
            {
                g.DrawEllipse(p, b);
                g.FillPie(br, b.X + 1, b.Y + 1, b.Width - 2, b.Height - 2, 90, 180);
            }
        }

        public static void Grid3(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.62f));
            using (Brush br = new SolidBrush(c))
            using (Pen p = new Pen(c, Math.Max(1f, 1.2f * r.Height / 26f)))
            {
                int cellSz = Math.Max(2, b.Width / 4);
                int step = cellSz + Math.Max(1, b.Width / 8);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        Rectangle cell = new Rectangle(b.X + i * step, b.Y + j * step, cellSz, cellSz);
                        if (i == 1 && j == 1) g.DrawRectangle(p, cell.X, cell.Y, cell.Width, cell.Height);
                        else g.FillRectangle(br, cell);
                    }
            }
        }

        public static void Grid2(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.55f));
            using (Brush br = new SolidBrush(c))
            {
                int gap = Math.Max(2, b.Width / 8);
                int cellSz = (b.Width - gap) / 2;
                g.FillRectangle(br, b.X, b.Y, cellSz, cellSz);
                g.FillRectangle(br, b.X + cellSz + gap, b.Y, cellSz, cellSz);
                g.FillRectangle(br, b.X, b.Y + cellSz + gap, cellSz, cellSz);
                g.FillRectangle(br, b.X + cellSz + gap, b.Y + cellSz + gap, cellSz, cellSz);
            }
        }

        public static void BrushDot(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.48f));
            using (Pen p = new Pen(c, Math.Max(1.2f, 1.5f * r.Height / 32f)))
            using (Brush br = new SolidBrush(c))
            {
                g.DrawEllipse(p, b);
                int d = Math.Max(3, b.Width / 3);
                g.FillEllipse(br, r.X + r.Width / 2 - d / 2, r.Y + r.Height / 2 - d / 2, d, d);
            }
        }

        public static void Eraser(Graphics g, Rectangle r, Color c)
        {
            GraphicsState st = g.Save();
            g.TranslateTransform(r.X + r.Width / 2f, r.Y + r.Height / 2f);
            g.RotateTransform(-30f);
            float f = r.Height / 26f;
            using (Pen p = PenFor(c, Math.Max(1.2f, 1.5f * f)))
            {
                int hw = Math.Max(4, (int)(8 * f)), hh = Math.Max(3, (int)(5 * f));
                using (GraphicsPath path = Theme.RoundedRect(new Rectangle(-hw, -hh, hw * 2, hh * 2), Math.Max(1, (int)(2 * f))))
                    g.DrawPath(p, path);
                g.DrawLine(p, -1.5f * f, -hh, -1.5f * f, hh);
            }
            g.Restore(st);
        }

        public static void Plus(Graphics g, Rectangle r, Color c)
        {
            float w = Math.Max(1.2f, 1.5f * r.Height / 22f);
            using (Pen p = PenFor(c, w))
            {
                int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
                int half = Math.Max(3, r.Height / 4);
                g.DrawLine(p, cx - half, cy, cx + half, cy);
                g.DrawLine(p, cx, cy - half, cx, cy + half);
            }
        }

        public static void Minus(Graphics g, Rectangle r, Color c)
        {
            float w = Math.Max(1.2f, 1.5f * r.Height / 22f);
            using (Pen p = PenFor(c, w))
            {
                int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
                int half = Math.Max(3, r.Height / 4);
                g.DrawLine(p, cx - half, cy, cx + half, cy);
            }
        }

        public static void PhotoEmpty(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = PenFor(c, Math.Max(2f, r.Height * 0.04f)))
            {
                int rad = Math.Max(3, (int)(r.Height * 0.12f));
                using (GraphicsPath frame = Theme.RoundedRect(r, rad))
                    g.DrawPath(p, frame);

                int dotR = Math.Max(3, (int)(r.Height * 0.08f));
                int dotX = r.X + (int)(r.Width * 0.68f);
                int dotY = r.Y + (int)(r.Height * 0.32f);
                g.DrawEllipse(p, dotX - dotR, dotY - dotR, dotR * 2, dotR * 2);

                int mx0 = r.X + (int)(r.Width * 0.18f);
                int my0 = r.Bottom - (int)(r.Height * 0.22f);
                int mx1 = r.X + (int)(r.Width * 0.45f);
                int my1 = r.Y + (int)(r.Height * 0.46f);
                int mx2 = r.Right - (int)(r.Width * 0.18f);
                int my2 = my0;

                g.DrawLine(p, mx0, my0, mx1, my1);
                g.DrawLine(p, mx1, my1, mx2, my2);
            }
        }
    }

    // ================= font helper (safely replaces control fonts without handle leaks) =================
    static class FontHelper
    {
        public static void SetFont(Control c, Font newFont)
        {
            if (c == null) return;
            Font old = c.Font;
            c.Font = newFont;
            if (old != null && old != SystemFonts.DefaultFont && old != newFont)
            {
                old.Dispose();
            }
        }
    }

    // ================= flat icon button (Win11 command-bar style) =================
    class IconButton : Control
    {
        public string Glyph;
        public Action<Graphics, Rectangle, Color> Vector;
        bool hover, down;

        Font glyphFontCache;
        float glyphFontSizeCache = 0f;

        Font GetGlyphFont()
        {
            float px = Math.Max(10f, (float)Math.Round(Height * 0.52f));
            if (glyphFontCache == null || Math.Abs(px - glyphFontSizeCache) > 0.5f)
            {
                if (glyphFontCache != null) glyphFontCache.Dispose();
                glyphFontCache = new Font(Program.IconFontName, px, FontStyle.Regular, GraphicsUnit.Pixel);
                glyphFontSizeCache = px;
            }
            return glyphFontCache;
        }

        public IconButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Text = "";
            Cursor = Cursors.Default;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fg = Enabled ? ForeColor : Theme.Disabled;
            if (Enabled && (hover || down))
            {
                Rectangle rr = new Rectangle(1, 1, Width - 2, Height - 2);
                int rad = Math.Max(2, (int)Math.Round(4 * Height / 32f));
                using (GraphicsPath path = Theme.RoundedRect(rr, rad))
                using (Brush b = new SolidBrush(down ? Theme.Pressed : Theme.Hover))
                    g.FillPath(b, path);
            }
            if (Vector != null)
            {
                Vector(g, ClientRectangle, fg);
            }
            else if (!string.IsNullOrEmpty(Glyph))
            {
                TextRenderer.DrawText(g, Glyph, GetGlyphFont(), ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && glyphFontCache != null)
            {
                glyphFontCache.Dispose();
                glyphFontCache = null;
            }
            base.Dispose(disposing);
        }
    }

    // ================= segmented mode button =================
    class ModeButton : Control
    {
        public bool Selected;
        public Action<Graphics, Rectangle, Color> Vector;
        bool hover, down;

        public ModeButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Text = "";
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fg = Theme.Text;
            Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
            int rad = Math.Max(2, (int)Math.Round(4 * Height / 26f));
            if (Selected)
            {
                using (GraphicsPath path = Theme.RoundedRect(rr, rad))
                {
                    using (Brush b = new SolidBrush(Theme.SegSelected)) g.FillPath(b, path);
                    using (Pen p = new Pen(Theme.Border)) g.DrawPath(p, path);
                }
            }
            else if (hover || down)
            {
                using (GraphicsPath path = Theme.RoundedRect(rr, rad))
                using (Brush b = new SolidBrush(down ? Theme.Pressed : Theme.Hover))
                    g.FillPath(b, path);
            }
            int pad = Math.Max(4, (int)(8 * Height / 26f));
            int iconW = (int)(20 * Height / 26f);
            if (Vector != null) Vector(g, new Rectangle(pad, 0, iconW, Height), fg);
            Rectangle tr = new Rectangle(pad + iconW, 0, Width - pad - iconW, Height);
            TextRenderer.DrawText(g, Text, Font, tr, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    // ================= accent button (empty state) =================
    class AccentButton : Control
    {
        bool hover, down;
        public AccentButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Cursor = Cursors.Hand;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fill = down ? Theme.Accent : (hover ? Theme.AccentButtonHover : Theme.AccentButton);
            int rad = Math.Max(2, (int)Math.Round(4 * Height / 32f));
            using (GraphicsPath path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), rad))
            using (Brush b = new SolidBrush(fill))
                g.FillPath(b, path);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    // ================= pill badge label (slider values) =================
    class PillLabel : Control
    {
        public PillLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (Brush bg = new SolidBrush(Parent != null ? Parent.BackColor : Theme.Bar))
                g.FillRectangle(bg, ClientRectangle);

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Theme.RoundedRect(r, Height / 2))
            using (Brush b = new SolidBrush(Theme.Pill))
                g.FillPath(b, path);

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    // ================= modern thin slider (Win11 style) =================
    class ModernSlider : Control
    {
        int minV = 0, maxV = 100, val = 0;
        bool dragging, hoverThumb;
        public event EventHandler ValueChanged;

        public ModernSlider()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            TabStop = false;
            Height = 28;
            Cursor = Cursors.Default;
        }

        public int Minimum { get { return minV; } set { minV = value; if (val < minV) val = minV; Invalidate(); } }
        public int Maximum { get { return maxV; } set { maxV = value; if (val > maxV) val = maxV; Invalidate(); } }
        public int Value
        {
            get { return val; }
            set
            {
                int nv = Math.Max(minV, Math.Min(maxV, value));
                if (nv != val) { val = nv; if (ValueChanged != null) ValueChanged(this, EventArgs.Empty); }
                Invalidate();
            }
        }

        float Inset() { return Height * 0.32f; }

        float ThumbX()
        {
            float range = Math.Max(1, maxV - minV);
            float ins = Inset();
            return ins + (val - minV) / range * (Width - ins * 2f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            float cy = Height / 2f;
            float tx = ThumbX();
            float ins = Inset(); float th = Math.Max(3f, Height * 0.15f);
            RectangleF track = new RectangleF(ins, cy - th / 2f, Width - ins * 2f, th);
            using (GraphicsPath tp = Theme.RoundedRect(Rectangle.Round(track), (int)Math.Ceiling(th / 2f)))
            {
                using (Brush tb = new SolidBrush(Theme.Track)) g.FillPath(tb, tp);
                if (tx > ins)
                {
                    GraphicsState st = g.Save();
                    g.SetClip(new RectangleF(ins, 0, tx - ins, Height));
                    using (Brush ab = new SolidBrush(Theme.Accent)) g.FillPath(ab, tp);
                    g.Restore(st);
                }
            }
            int r = (int)((dragging || hoverThumb) ? Height * 0.28f : Height * 0.24f); if (r < 5) r = 5;
            using (Brush wb = new SolidBrush(Color.White))
            using (Pen bp = new Pen(Theme.Dark ? Color.FromArgb(120, 120, 120) : Color.FromArgb(160, 160, 160), 1))
            {
                g.FillEllipse(wb, tx - r, cy - r, r * 2, r * 2);
                g.DrawEllipse(bp, tx - r, cy - r, r * 2, r * 2);
            }
            if (dragging || hoverThumb)
            {
                using (Brush ab = new SolidBrush(Theme.Accent))
                {
                    float dr = Height * 0.11f; if (dr < 3f) dr = 3f;
                    g.FillEllipse(ab, tx - dr, cy - dr, dr * 2f, dr * 2f);
                }
            }
        }

        void UpdateFromX(int x)
        {
            float ins2 = Inset(); float ratio = (x - ins2) / Math.Max(1f, Width - ins2 * 2f);
            if (ratio < 0) ratio = 0; if (ratio > 1) ratio = 1;
            Value = minV + (int)Math.Round(ratio * (maxV - minV));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { dragging = true; Capture = true; UpdateFromX(e.X); Invalidate(); }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool prevHover = hoverThumb;
            hoverThumb = Math.Abs(e.X - ThumbX()) <= Height * 0.4f && Math.Abs(e.Y - Height / 2) <= Height * 0.45f;
            Cursor = (hoverThumb || dragging) ? Cursors.Hand : Cursors.Default;
            if (dragging) UpdateFromX(e.X);
            else if (prevHover != hoverThumb) Invalidate();
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false; Capture = false; Invalidate();
            base.OnMouseUp(e);
        }
        protected override void OnMouseLeave(EventArgs e) { hoverThumb = false; Cursor = Cursors.Default; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int step = Math.Max(1, (maxV - minV) / 20);
            Value = val + (e.Delta > 0 ? step : -step);
            base.OnMouseWheel(e);
        }
    }

    // ================= static small icon =================
    class StaticIcon : Control
    {
        public Action<Graphics, Rectangle, Color> Vector;
        public StaticIcon()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            TabStop = false;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            if (Vector != null) Vector(g, ClientRectangle, Theme.Secondary);
        }
    }

    // ================= app settings (theme mode), stored under %AppData%\CensorBrush =================
    static class AppSettings
    {
        public static int ThemeMode = 0; // 0 = follow system, 1 = light, 2 = dark

        static string SettingsPath
        {
            get
            {
                string d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CensorBrush");
                try { Directory.CreateDirectory(d); } catch { }
                return Path.Combine(d, "settings.txt");
            }
        }

        public static void Load()
        {
            try
            {
                string t = File.ReadAllText(SettingsPath);
                if (t.Contains("theme=light")) ThemeMode = 1;
                else if (t.Contains("theme=dark")) ThemeMode = 2;
                else ThemeMode = 0;
            }
            catch { }
        }

        public static void Save()
        {
            try { File.WriteAllText(SettingsPath, "theme=" + (ThemeMode == 1 ? "light" : ThemeMode == 2 ? "dark" : "auto")); }
            catch { }
        }
    }

    static class Program
    {
        public static string IconFontName = "Segoe Fluent Icons";
        public static string UiFontName = "Microsoft YaHei UI";

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(int awareness);

        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            // PerMonitorV2 DPI awareness initialization (Win10 1703+)
            // -4 = DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2
            try
            {
                if (!SetProcessDpiAwarenessContext((IntPtr)(-4)))
                {
                    if (SetProcessDpiAwareness(2) != 0) // PROCESS_PER_MONITOR_DPI_AWARE
                    {
                        SetProcessDPIAware();
                    }
                }
            }
            catch
            {
                try { SetProcessDPIAware(); } catch { }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSettings.Load();
            Theme.Apply(AppSettings.ThemeMode == 2 || (AppSettings.ThemeMode == 0 && Theme.DetectSystemDark()));

            IconFontName = FamilyExists("Segoe Fluent Icons") ? "Segoe Fluent Icons"
                : (FamilyExists("Segoe MDL2 Assets") ? "Segoe MDL2 Assets" : "Segoe UI Symbol");

            // Native Windows typography: Windows Notepad on zh-CN uses Microsoft YaHei UI (ClearType hinted)
            string sysFont = SystemFonts.MessageBoxFont != null ? SystemFonts.MessageBoxFont.Name : "Microsoft YaHei UI";
            if (FamilyExists("Microsoft YaHei UI")) UiFontName = "Microsoft YaHei UI";
            else if (FamilyExists(sysFont)) UiFontName = sysFont;
            else if (FamilyExists("Segoe UI")) UiFontName = "Segoe UI";
            else UiFontName = FontFamily.GenericSansSerif.Name;

            MainForm f = new MainForm();
            if (args.Length > 0 && File.Exists(args[0]))
            {
                string p = args[0];
                f.Shown += delegate { f.OpenImage(p); };
            }
            Application.Run(f);
        }

        public static bool FamilyExists(string name)
        {
            try
            {
                foreach (FontFamily ff in FontFamily.Families)
                    if (ff.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            catch { }
            return false;
        }
    }

    class CanvasPanel : Panel
    {
        public CanvasPanel()
        {
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }

    public class MainForm : Form
    {
        // ---- image state (full resolution) ----
        byte[] origPixels;
        byte[] curPixels;
        int imgW, imgH;
        Bitmap currentBmp;
        string filePath = "";
        bool isDirty = false;

        // ---- view state ----
        float zoom = 1f;
        float panX = 0f, panY = 0f;
        Point lastMouse;
        bool hasMouse = false;
        bool dragOver = false;

        // ---- stroke state ----
        bool stroking = false;
        bool strokeRestore = false;
        bool restoreMode = false;
        float lastImgX, lastImgY;
        bool hasLastImg = false;

        // ---- pan state ----
        bool panning = false;
        bool spaceDown = false;
        Point panStartMouse;
        float panStartX, panStartY;

        List<byte[]> undoStack = new List<byte[]>();

        // ---- controls ----
        CanvasPanel canvas;
        ModernSlider brushSlider, blockSlider;
        PillLabel brushVal, blockVal;
        Label statusLeft, dimLabel, zoomLabel;
        ModeButton modeMosaicBtn, modeEraseBtn;
        IconButton btnOpen, btnSave, btnSaveAs, btnUndo, btnFit, btnZoomIn, btnZoomOut, themeBtn;
        Panel headerBar, statusPanel, themeWrap, segPanel;
        StaticIcon brushIcon, blockIcon;
        FlowLayoutPanel header, statusRight;
        AccentButton btnOpenBig;
        List<Control> separators = new List<Control>();
        ToolTip tip;
        Timer saveTimer;
        uint currentDpi = 96;
        float scaleFactor = 1f;

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        public MainForm()
        {
            AutoScaleMode = AutoScaleMode.None;
            UpdateTitle();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            Width = 1080;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font(Program.UiFontName, 9f);
            BackColor = Theme.Bar;
            ForeColor = Theme.Text;
            KeyPreview = true;
            AllowDrop = true;
            DoubleBuffered = true;

            tip = new ToolTip();
            tip.AutoPopDelay = 4000;
            tip.InitialDelay = 600;
            tip.ReshowDelay = 200;

            // ---------- command bar (Win11 fluent style) ----------
            headerBar = new Panel();
            headerBar.Dock = DockStyle.Top;
            headerBar.Height = 40;
            headerBar.BackColor = Theme.Bar;
            headerBar.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Theme.Border))
                    e.Graphics.DrawLine(p, 0, headerBar.Height - 1, headerBar.Width, headerBar.Height - 1);
            };

            header = new FlowLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.WrapContents = false;
            header.Padding = new Padding(6, 4, 4, 0);
            header.BackColor = Theme.Bar;

            themeWrap = new Panel();
            themeWrap.Dock = DockStyle.Right;
            themeWrap.Width = 44;
            themeWrap.Padding = new Padding(4);
            themeWrap.BackColor = Theme.Bar;

            themeBtn = new IconButton();
            themeBtn.Dock = DockStyle.Fill;
            themeBtn.BackColor = Theme.Bar; themeBtn.ForeColor = Theme.Text;
            themeBtn.Click += delegate { CycleTheme(); };
            themeWrap.Controls.Add(themeBtn);
            headerBar.Controls.Add(header);
            headerBar.Controls.Add(themeWrap);
            UpdateThemeButton();

            btnOpen = new IconButton();
            btnOpen.Glyph = "\uE838";
            btnOpen.Size = new Size(32, 32);
            btnOpen.Margin = new Padding(0, 0, 2, 0);
            btnOpen.BackColor = Theme.Bar; btnOpen.ForeColor = Theme.Text;
            tip.SetToolTip(btnOpen, "打开图片 (Ctrl+O)");
            btnOpen.Click += delegate { DoOpen(); };

            btnSave = new IconButton();
            btnSave.Glyph = "\uE74E";
            btnSave.Size = new Size(32, 32);
            btnSave.Margin = new Padding(0, 0, 2, 0);
            btnSave.BackColor = Theme.Bar; btnSave.ForeColor = Theme.Text;
            tip.SetToolTip(btnSave, "保存 (Ctrl+S)");
            btnSave.Click += delegate { DoQuickSave(); };

            btnSaveAs = new IconButton();
            btnSaveAs.Glyph = "\uE8C8";
            btnSaveAs.Size = new Size(32, 32);
            btnSaveAs.Margin = new Padding(0, 0, 2, 0);
            btnSaveAs.BackColor = Theme.Bar; btnSaveAs.ForeColor = Theme.Text;
            tip.SetToolTip(btnSaveAs, "另存为 (Ctrl+Shift+S) / 复制 (Ctrl+C)");
            btnSaveAs.Click += delegate { DoSaveAs(); };

            btnUndo = new IconButton();
            btnUndo.Glyph = "\uE7A7";
            btnUndo.Size = new Size(32, 32);
            btnUndo.Margin = new Padding(0, 0, 2, 0);
            btnUndo.BackColor = Theme.Bar; btnUndo.ForeColor = Theme.Text;
            tip.SetToolTip(btnUndo, "撤销 (Ctrl+Z)");
            btnUndo.Click += delegate { DoUndo(); };

            btnFit = new IconButton();
            btnFit.Vector = IconArt.Fit;
            btnFit.Size = new Size(32, 32);
            btnFit.Margin = new Padding(0, 0, 2, 0);
            btnFit.BackColor = Theme.Bar; btnFit.ForeColor = Theme.Text;
            tip.SetToolTip(btnFit, "适合窗口 (Ctrl+0)");
            btnFit.Click += delegate { FitView(); canvas.Invalidate(); };

            brushSlider = new ModernSlider();
            brushSlider.Minimum = 10; brushSlider.Maximum = 300; brushSlider.Value = 80;
            brushSlider.Width = 86;
            brushSlider.Margin = new Padding(2, 0, 2, 0);
            brushSlider.BackColor = Theme.Bar;
            tip.SetToolTip(brushSlider, "笔刷大小 (10 - 300)");
            brushSlider.ValueChanged += delegate
            {
                brushVal.Text = brushSlider.Value.ToString();
                if (hasMouse && currentBmp != null) InvalidateBrushAt(lastMouse);
            };

            blockSlider = new ModernSlider();
            blockSlider.Minimum = 4; blockSlider.Maximum = 40; blockSlider.Value = 12;
            blockSlider.Width = 72;
            blockSlider.Margin = new Padding(2, 0, 2, 0);
            blockSlider.BackColor = Theme.Bar;
            tip.SetToolTip(blockSlider, "马赛克块大小 (4 - 40)");
            blockSlider.ValueChanged += delegate { blockVal.Text = blockSlider.Value.ToString(); };

            brushVal = MakePill("80");
            blockVal = MakePill("12");

            brushIcon = new StaticIcon();
            brushIcon.Vector = IconArt.BrushDot;
            brushIcon.Size = new Size(18, 32);
            brushIcon.Margin = new Padding(4, 0, 0, 0);
            brushIcon.BackColor = Theme.Bar;
            tip.SetToolTip(brushIcon, "笔刷大小");

            blockIcon = new StaticIcon();
            blockIcon.Vector = IconArt.Grid2;
            blockIcon.Size = new Size(18, 32);
            blockIcon.Margin = new Padding(4, 0, 0, 0);
            blockIcon.BackColor = Theme.Bar;
            tip.SetToolTip(blockIcon, "马赛克块大小");

            // segmented mode switch
            segPanel = new Panel();
            segPanel.Size = new Size(146, 30);
            segPanel.Margin = new Padding(6, 0, 0, 0);
            segPanel.BackColor = Theme.SegBg;

            modeMosaicBtn = new ModeButton();
            modeMosaicBtn.Text = "打码";
            modeMosaicBtn.Vector = IconArt.Grid3;
            modeMosaicBtn.Font = new Font(Program.UiFontName, 9f);
            modeMosaicBtn.Location = new Point(2, 2);
            modeMosaicBtn.Size = new Size(70, 26);
            modeMosaicBtn.BackColor = Theme.SegBg;
            modeMosaicBtn.Selected = true;
            tip.SetToolTip(modeMosaicBtn, "马赛克笔 · 左键涂抹打码");
            modeMosaicBtn.Click += delegate { SetRestoreMode(false); };

            modeEraseBtn = new ModeButton();
            modeEraseBtn.Text = "还原";
            modeEraseBtn.Vector = IconArt.Eraser;
            modeEraseBtn.Font = new Font(Program.UiFontName, 9f);
            modeEraseBtn.Location = new Point(74, 2);
            modeEraseBtn.Size = new Size(70, 26);
            modeEraseBtn.BackColor = Theme.SegBg;
            modeEraseBtn.Selected = false;
            tip.SetToolTip(modeEraseBtn, "还原笔 · 左键擦除打码 (也可直接按住鼠标右键涂抹)");
            modeEraseBtn.Click += delegate { SetRestoreMode(true); };

            segPanel.Controls.Add(modeMosaicBtn);
            segPanel.Controls.Add(modeEraseBtn);

            header.Controls.Add(btnOpen);
            header.Controls.Add(btnSave);
            header.Controls.Add(btnSaveAs);
            header.Controls.Add(MakeSeparator());
            header.Controls.Add(btnUndo);
            header.Controls.Add(btnFit);
            header.Controls.Add(MakeSeparator());
            header.Controls.Add(brushIcon);
            header.Controls.Add(brushSlider);
            header.Controls.Add(brushVal);
            header.Controls.Add(blockIcon);
            header.Controls.Add(blockSlider);
            header.Controls.Add(blockVal);
            header.Controls.Add(segPanel);

            // ---------- status bar (Notepad specification: vertically centered & crisp) ----------
            statusPanel = new Panel();
            statusPanel.Dock = DockStyle.Bottom;
            statusPanel.Height = 30;
            statusPanel.BackColor = Theme.Bar;
            statusPanel.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Theme.Border)) e.Graphics.DrawLine(p, 0, 0, statusPanel.Width, 0);
            };

            statusLeft = new Label();
            statusLeft.Dock = DockStyle.Left;
            statusLeft.AutoSize = false;
            statusLeft.Height = 30;
            statusLeft.Width = 500;
            statusLeft.TextAlign = ContentAlignment.MiddleLeft;
            statusLeft.Padding = new Padding(12, 0, 0, 0);
            statusLeft.Font = new Font(Program.UiFontName, 9f);
            statusLeft.ForeColor = Theme.Secondary;
            statusLeft.BackColor = Theme.Bar;
            statusLeft.Text = "就绪";
            statusPanel.Controls.Add(statusLeft);

            statusRight = new FlowLayoutPanel();
            statusRight.Dock = DockStyle.Right;
            statusRight.AutoSize = true;
            statusRight.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            statusRight.WrapContents = false;
            statusRight.BackColor = Theme.Bar;
            statusRight.Padding = new Padding(0, 0, 6, 0);

            dimLabel = new Label();
            dimLabel.AutoSize = true;
            dimLabel.Margin = new Padding(0, 4, 10, 0);
            dimLabel.Font = new Font(Program.UiFontName, 9f);
            dimLabel.ForeColor = Theme.Secondary;
            dimLabel.BackColor = Theme.Bar;
            dimLabel.Text = "";

            btnZoomOut = new IconButton();
            btnZoomOut.Vector = IconArt.Minus;
            btnZoomOut.Size = new Size(22, 22);
            btnZoomOut.Margin = new Padding(0, 3, 0, 0);
            btnZoomOut.BackColor = Theme.Bar; btnZoomOut.ForeColor = Theme.Text;
            tip.SetToolTip(btnZoomOut, "缩小 (Ctrl+-)");
            btnZoomOut.Click += delegate { ZoomStep(1f / 1.25f); };

            zoomLabel = new Label();
            zoomLabel.Size = new Size(48, 22);
            zoomLabel.Margin = new Padding(0, 3, 0, 0);
            zoomLabel.TextAlign = ContentAlignment.MiddleCenter;
            zoomLabel.Font = new Font(Program.UiFontName, 9f);
            zoomLabel.ForeColor = Theme.Text;
            zoomLabel.BackColor = Theme.Bar;
            zoomLabel.Cursor = Cursors.Hand;
            zoomLabel.Text = "";
            tip.SetToolTip(zoomLabel, "适合窗口 (Ctrl+0)");
            zoomLabel.Click += delegate { FitView(); canvas.Invalidate(); };

            btnZoomIn = new IconButton();
            btnZoomIn.Vector = IconArt.Plus;
            btnZoomIn.Size = new Size(22, 22);
            btnZoomIn.Margin = new Padding(0, 3, 8, 0);
            btnZoomIn.BackColor = Theme.Bar; btnZoomIn.ForeColor = Theme.Text;
            tip.SetToolTip(btnZoomIn, "放大 (Ctrl++)");
            btnZoomIn.Click += delegate { ZoomStep(1.25f); };

            statusRight.Controls.Add(dimLabel);
            statusRight.Controls.Add(btnZoomOut);
            statusRight.Controls.Add(zoomLabel);
            statusRight.Controls.Add(btnZoomIn);
            statusPanel.Controls.Add(statusRight);

            // ---------- canvas ----------
            canvas = new CanvasPanel();
            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Theme.Canvas;
            canvas.Cursor = Cursors.Cross;
            canvas.AllowDrop = true;
            canvas.Paint += Canvas_Paint;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.MouseEnter += delegate { canvas.Focus(); };
            canvas.MouseLeave += delegate { if (hasMouse) InvalidateBrushAt(lastMouse); hasMouse = false; };
            canvas.MouseWheel += Canvas_MouseWheel;
            canvas.DragEnter += DragEnterHandler;
            canvas.DragLeave += delegate { dragOver = false; canvas.Invalidate(); };
            canvas.DragDrop += DragDropHandler;
            canvas.DoubleClick += delegate { if (currentBmp == null) DoOpen(); else { FitView(); canvas.Invalidate(); } };
            canvas.Resize += delegate { LayoutEmptyButton(); };

            btnOpenBig = new AccentButton();
            btnOpenBig.Text = "打开图片";
            btnOpenBig.Font = new Font(Program.UiFontName, 9.5f);
            btnOpenBig.Size = new Size(112, 32);
            btnOpenBig.BackColor = Theme.Canvas;
            btnOpenBig.Click += delegate { DoOpen(); };
            canvas.Controls.Add(btnOpenBig);

            Controls.Add(canvas);
            Controls.Add(statusPanel);
            Controls.Add(headerBar);

            DragEnter += DragEnterHandler;
            DragDrop += DragDropHandler;
            KeyDown += Form_KeyDown;
            KeyUp += Form_KeyUp;

            saveTimer = new Timer();
            saveTimer.Interval = 2600;
            saveTimer.Tick += delegate { saveTimer.Stop(); UpdateStatus(); };

            RefreshCommandStates();
            UpdateStatus();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr h);

        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        const int WM_SETREDRAW = 0x000B;
        const int WM_DPICHANGED = 0x02E0;
        const int WM_SETTINGCHANGE = 0x001A;

        void ApplyDpiScale(uint targetDpi, bool isDpiChange)
        {
            if (targetDpi < 96) targetDpi = 96;
            currentDpi = targetDpi;
            scaleFactor = targetDpi / 96f;

            // Responsive DPI scaling aligned with Windows 11 Fluent specifications
            int barH = (int)Math.Round(40 * scaleFactor);
            headerBar.Height = barH;
            header.Padding = new Padding((int)(6 * scaleFactor), (int)(4 * scaleFactor), (int)(4 * scaleFactor), 0);
            themeWrap.Width = (int)Math.Round(42 * scaleFactor);
            themeWrap.Padding = new Padding((int)(4 * scaleFactor));

            int btnSz = (int)Math.Round(32 * scaleFactor);
            Size btnSize = new Size(btnSz, btnSz);
            Padding btnMargin = new Padding(0, 0, (int)Math.Max(1, 2 * scaleFactor), 0);

            btnOpen.Size = btnSize; btnOpen.Margin = btnMargin;
            btnSave.Size = btnSize; btnSave.Margin = btnMargin;
            btnSaveAs.Size = btnSize; btnSaveAs.Margin = btnMargin;
            btnUndo.Size = btnSize; btnUndo.Margin = btnMargin;
            btnFit.Size = btnSize; btnFit.Margin = btnMargin;

            foreach (Control sep in separators)
            {
                sep.Size = new Size(1, (int)Math.Round(20 * scaleFactor));
                sep.Margin = new Padding((int)(6 * scaleFactor), (int)(6 * scaleFactor), (int)(6 * scaleFactor), 0);
            }

            int iconSz = (int)Math.Round(18 * scaleFactor);
            brushIcon.Size = new Size(iconSz, btnSz);
            brushIcon.Margin = new Padding((int)(4 * scaleFactor), 0, 0, 0);

            brushSlider.Size = new Size((int)Math.Round(86 * scaleFactor), btnSz);
            brushSlider.Margin = new Padding((int)(2 * scaleFactor), 0, (int)(2 * scaleFactor), 0);

            Size pillSize = new Size((int)Math.Round(42 * scaleFactor), (int)Math.Round(24 * scaleFactor));
            Padding pillMargin = new Padding(0, (int)(4 * scaleFactor), (int)(6 * scaleFactor), 0);

            brushVal.Size = pillSize; brushVal.Margin = pillMargin;
            FontHelper.SetFont(brushVal, new Font(Program.UiFontName, 9f));

            blockIcon.Size = new Size(iconSz, btnSz);
            blockIcon.Margin = new Padding((int)(4 * scaleFactor), 0, 0, 0);

            blockSlider.Size = new Size((int)Math.Round(72 * scaleFactor), btnSz);
            blockSlider.Margin = new Padding((int)(2 * scaleFactor), 0, (int)(2 * scaleFactor), 0);

            blockVal.Size = pillSize; blockVal.Margin = pillMargin;
            FontHelper.SetFont(blockVal, new Font(Program.UiFontName, 9f));

            int segW = (int)Math.Round(146 * scaleFactor);
            segPanel.Size = new Size(segW, btnSz);
            segPanel.Margin = new Padding((int)(6 * scaleFactor), 0, 0, 0);

            int segGap = Math.Max(2, (int)Math.Round(2 * scaleFactor));
            int segBtnW = (segW - segGap * 3) / 2;
            int segBtnH = btnSz - segGap * 2;

            modeMosaicBtn.SetBounds(segGap, segGap, segBtnW, segBtnH);
            FontHelper.SetFont(modeMosaicBtn, new Font(Program.UiFontName, 9f));

            modeEraseBtn.SetBounds(segGap * 2 + segBtnW, segGap, segBtnW, segBtnH);
            FontHelper.SetFont(modeEraseBtn, new Font(Program.UiFontName, 9f));

            // Status bar (Notepad spec: 30px scaled, vertically centered)
            int statusH = (int)Math.Round(30 * scaleFactor);
            statusPanel.Height = statusH;

            statusLeft.Height = statusH;
            statusLeft.Width = (int)Math.Round(500 * scaleFactor);
            FontHelper.SetFont(statusLeft, new Font(Program.UiFontName, 9f));
            statusLeft.Padding = new Padding((int)(12 * scaleFactor), 0, 0, 0);

            FontHelper.SetFont(dimLabel, new Font(Program.UiFontName, 9f));
            FontHelper.SetFont(zoomLabel, new Font(Program.UiFontName, 9f));

            int zoomBtnSz = (int)Math.Round(22 * scaleFactor);
            btnZoomOut.Size = new Size(zoomBtnSz, zoomBtnSz);
            btnZoomIn.Size = new Size(zoomBtnSz, zoomBtnSz);
            zoomLabel.Size = new Size((int)Math.Round(48 * scaleFactor), zoomBtnSz);

            int statusTop = Math.Max(0, (statusH - zoomBtnSz) / 2);
            dimLabel.Margin = new Padding(0, statusTop + (int)(1 * scaleFactor), (int)(10 * scaleFactor), 0);
            btnZoomOut.Margin = new Padding(0, statusTop, 0, 0);
            zoomLabel.Margin = new Padding(0, statusTop, 0, 0);
            btnZoomIn.Margin = new Padding(0, statusTop, (int)(8 * scaleFactor), 0);

            // Form boundaries & window sizing (preventing toolbar overflow)
            MinimumSize = new Size((int)Math.Round(680 * scaleFactor), (int)Math.Round(450 * scaleFactor));

            if (!isDpiChange)
            {
                Rectangle wa = Screen.FromControl(this).WorkingArea;
                int defW = (int)Math.Round(1080 * scaleFactor);
                int defH = (int)Math.Round(720 * scaleFactor);
                int w = Math.Min(defW, (int)(wa.Width * 0.95f));
                int h = Math.Min(defH, (int)(wa.Height * 0.92f));
                this.Size = new Size(w, h);
                this.Location = new Point(wa.Left + (wa.Width - w) / 2, wa.Top + (wa.Height - h) / 2);
            }

            LayoutEmptyButton();
            Invalidate(true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            uint dpi = 96;
            try { dpi = GetDpiForWindow(Handle); } catch { }
            if (dpi == 0) dpi = 96;
            ApplyDpiScale(dpi, false);
            ApplyTitleBarTheme();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DPICHANGED)
            {
                uint newDpi = (uint)(m.WParam.ToInt32() & 0xFFFF);
                RECT r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                // Maintain proportional image pan coordinates across monitor DPI changes
                if (currentDpi > 0 && newDpi != currentDpi)
                {
                    float ratio = (float)newDpi / currentDpi;
                    panX *= ratio;
                    panY *= ratio;
                }

                ApplyDpiScale(newDpi, true);
                SetWindowPos(Handle, IntPtr.Zero, r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top, 0x0004 | 0x0010 | 0x0020);
                m.Result = IntPtr.Zero;
                return;
            }
            if (m.Msg == WM_SETTINGCHANGE)
            {
                if (AppSettings.ThemeMode == 0)
                {
                    bool dark = Theme.DetectSystemDark();
                    if (dark != Theme.Dark)
                    {
                        ApplyThemeVisuals(dark);
                        UpdateThemeButton();
                    }
                }
            }
            base.WndProc(ref m);
        }

        void ApplyTitleBarTheme()
        {
            try
            {
                int v = Theme.Dark ? 1 : 0;
                DwmSetWindowAttribute(Handle, 20, ref v, 4); // DWMWA_USE_IMMERSIVE_DARK_MODE (Win10 20H1+)
                DwmSetWindowAttribute(Handle, 19, ref v, 4); // DWMWA_USE_IMMERSIVE_DARK_MODE (Win10 1903/1909)
                int capColor = (Theme.Bar.R) | (Theme.Bar.G << 8) | (Theme.Bar.B << 16);
                DwmSetWindowAttribute(Handle, 35, ref capColor, 4); // DWMWA_CAPTION_COLOR (Win11)
                int textColor = (Theme.Text.R) | (Theme.Text.G << 8) | (Theme.Text.B << 16);
                DwmSetWindowAttribute(Handle, 36, ref textColor, 4); // DWMWA_TEXT_COLOR (Win11)
                int borderColor = (Theme.Border.R) | (Theme.Border.G << 8) | (Theme.Border.B << 16);
                DwmSetWindowAttribute(Handle, 34, ref borderColor, 4); // DWMWA_BORDER_COLOR (Win11)
                SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0027);
            }
            catch { }
        }

        void UpdateThemeButton()
        {
            if (themeBtn == null) return;
            if (AppSettings.ThemeMode == 1)
            {
                themeBtn.Vector = null; themeBtn.Glyph = "\uE706";
                tip.SetToolTip(themeBtn, "主题：浅色（点击切换）");
            }
            else if (AppSettings.ThemeMode == 2)
            {
                themeBtn.Vector = null; themeBtn.Glyph = "\uE708";
                tip.SetToolTip(themeBtn, "主题：深色（点击切换）");
            }
            else
            {
                themeBtn.Glyph = null; themeBtn.Vector = IconArt.ThemeAuto;
                tip.SetToolTip(themeBtn, "主题：跟随系统（点击切换）");
            }
            themeBtn.Invalidate();
        }

        void CycleTheme()
        {
            AppSettings.ThemeMode = (AppSettings.ThemeMode + 1) % 3;
            AppSettings.Save();
            bool dark = AppSettings.ThemeMode == 2 || (AppSettings.ThemeMode == 0 && Theme.DetectSystemDark());
            if (dark != Theme.Dark) ApplyThemeVisuals(dark);
            UpdateThemeButton();
        }

        void ApplyThemeVisuals(bool dark)
        {
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
            try
            {
                Theme.Apply(dark);

                BackColor = Theme.Bar;
                ForeColor = Theme.Text;

                if (headerBar != null) headerBar.BackColor = Theme.Bar;
                if (header != null) header.BackColor = Theme.Bar;
                if (themeWrap != null) themeWrap.BackColor = Theme.Bar;
                if (statusPanel != null) statusPanel.BackColor = Theme.Bar;
                if (statusRight != null) statusRight.BackColor = Theme.Bar;

                if (canvas != null) canvas.BackColor = Theme.Canvas;
                if (btnOpenBig != null) btnOpenBig.BackColor = Theme.Canvas;

                if (btnOpen != null) { btnOpen.BackColor = Theme.Bar; btnOpen.ForeColor = Theme.Text; }
                if (btnSave != null) { btnSave.BackColor = Theme.Bar; btnSave.ForeColor = Theme.Text; }
                if (btnSaveAs != null) { btnSaveAs.BackColor = Theme.Bar; btnSaveAs.ForeColor = Theme.Text; }
                if (btnUndo != null) { btnUndo.BackColor = Theme.Bar; btnUndo.ForeColor = Theme.Text; }
                if (btnFit != null) { btnFit.BackColor = Theme.Bar; btnFit.ForeColor = Theme.Text; }
                if (themeBtn != null) { themeBtn.BackColor = Theme.Bar; themeBtn.ForeColor = Theme.Text; }

                if (brushIcon != null) brushIcon.BackColor = Theme.Bar;
                if (blockIcon != null) blockIcon.BackColor = Theme.Bar;
                if (brushSlider != null) brushSlider.BackColor = Theme.Bar;
                if (blockSlider != null) blockSlider.BackColor = Theme.Bar;

                if (brushVal != null) { brushVal.ForeColor = Theme.Text; brushVal.Invalidate(); }
                if (blockVal != null) { blockVal.ForeColor = Theme.Text; blockVal.Invalidate(); }

                if (segPanel != null) segPanel.BackColor = Theme.SegBg;
                if (modeMosaicBtn != null) modeMosaicBtn.BackColor = Theme.SegBg;
                if (modeEraseBtn != null) modeEraseBtn.BackColor = Theme.SegBg;

                foreach (Control sep in separators)
                {
                    if (sep != null) sep.BackColor = Theme.Border;
                }

                if (headerBar != null) headerBar.Invalidate();
                if (statusPanel != null) statusPanel.Invalidate();
                if (statusLeft != null) { statusLeft.BackColor = Theme.Bar; statusLeft.ForeColor = Theme.Secondary; }
                if (dimLabel != null) { dimLabel.BackColor = Theme.Bar; dimLabel.ForeColor = Theme.Secondary; }
                if (zoomLabel != null) { zoomLabel.BackColor = Theme.Bar; zoomLabel.ForeColor = Theme.Text; }
                if (btnZoomOut != null) { btnZoomOut.BackColor = Theme.Bar; btnZoomOut.ForeColor = Theme.Text; }
                if (btnZoomIn != null) { btnZoomIn.BackColor = Theme.Bar; btnZoomIn.ForeColor = Theme.Text; }

                ApplyTitleBarTheme();
            }
            finally
            {
                SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, 0x0185); // RDW_INVALIDATE | RDW_ALLCHILDREN | RDW_ERASE | RDW_UPDATENOW
            }
        }

        PillLabel MakePill(string text)
        {
            PillLabel p = new PillLabel();
            p.Text = text;
            p.Size = new Size(42, 24);
            p.ForeColor = Theme.Text;
            p.Font = new Font(Program.UiFontName, 9f);
            return p;
        }

        Control MakeSeparator()
        {
            Panel p = new Panel();
            p.Size = new Size(1, 20);
            p.BackColor = Theme.Border;
            p.Margin = new Padding(6, 6, 6, 0);
            separators.Add(p);
            return p;
        }

        void SetRestoreMode(bool restore)
        {
            restoreMode = restore;
            modeMosaicBtn.Selected = !restore;
            modeEraseBtn.Selected = restore;
            modeMosaicBtn.Invalidate();
            modeEraseBtn.Invalidate();
        }

        void RefreshCommandStates()
        {
            bool has = currentBmp != null;
            btnSave.Enabled = has;
            btnSaveAs.Enabled = has;
            btnUndo.Enabled = has && undoStack.Count > 0;
            btnFit.Enabled = has;
            btnZoomIn.Enabled = has;
            btnZoomOut.Enabled = has;
            btnOpenBig.Visible = !has;
            LayoutEmptyButton();
        }

        void UpdateTitle()
        {
            if (currentBmp == null)
            {
                Text = "打码笔";
            }
            else
            {
                string name = string.IsNullOrEmpty(filePath) ? "未命名图片" : Path.GetFileName(filePath);
                Text = (isDirty ? "*" : "") + name + " - 打码笔";
            }
        }

        void LayoutEmptyButton()
        {
            if (canvas == null || btnOpenBig == null) return;
            if (canvas.Width <= 0 || canvas.Height <= 0) return;
            int bw = (int)Math.Round(112 * scaleFactor);
            int bh = (int)Math.Round(32 * scaleFactor);
            btnOpenBig.Size = new Size(bw, bh);
            FontHelper.SetFont(btnOpenBig, new Font(Program.UiFontName, 9.5f));
            int off = (int)Math.Round(48 * scaleFactor);
            btnOpenBig.Location = new Point((canvas.Width - bw) / 2, canvas.Height / 2 + off);
        }

        // ================= file open / save / clipboard =================

        void DragEnterHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                if (!dragOver) { dragOver = true; canvas.Invalidate(); }
            }
        }

        void DragDropHandler(object sender, DragEventArgs e)
        {
            dragOver = false;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0) OpenImage(files[0]);
            else canvas.Invalidate();
        }

        void DoOpen()
        {
            if (isDirty && currentBmp != null)
            {
                DialogResult r = MessageBox.Show(this, "当前图片有未保存的更改，是否在打开前保存？", "打码笔",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Yes) { if (!DoQuickSave()) return; }
                else if (r == DialogResult.Cancel) return;
            }

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*";
                dlg.Title = "打开图片";
                if (dlg.ShowDialog(this) == DialogResult.OK) OpenImage(dlg.FileName);
            }
        }

        public void OpenImage(string path)
        {
            try
            {
                Bitmap bmp;
                using (Image src = Image.FromFile(path))
                {
                    bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(src, 0, 0, src.Width, src.Height);
                    }
                }
                SetBitmap(bmp, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "打开失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void SetBitmap(Bitmap bmp, string path)
        {
            if (currentBmp != null) currentBmp.Dispose();
            currentBmp = bmp;
            imgW = bmp.Width; imgH = bmp.Height;

            origPixels = ExtractPixels(bmp);
            curPixels = (byte[])origPixels.Clone();
            undoStack.Clear();
            filePath = path;
            isDirty = false;
            UpdateTitle();
            FitView();
            RefreshCommandStates();
            UpdateStatus();
            canvas.Invalidate();
            canvas.Focus();
        }

        static byte[] ExtractPixels(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            byte[] data = new byte[w * h * 4];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(IntPtr.Add(bd.Scan0, y * bd.Stride), data, y * w * 4, w * 4);
                }
            }
            finally { bmp.UnlockBits(bd); }
            return data;
        }

        void WriteBack(Rectangle rc)
        {
            if (currentBmp == null) return;
            rc.Intersect(new Rectangle(0, 0, imgW, imgH));
            if (rc.Width <= 0 || rc.Height <= 0) return;
            BitmapData bd = currentBmp.LockBits(rc, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < rc.Height; y++)
                {
                    Marshal.Copy(curPixels, (rc.Y + y) * imgW * 4 + rc.X * 4,
                        IntPtr.Add(bd.Scan0, y * bd.Stride), rc.Width * 4);
                }
            }
            finally { currentBmp.UnlockBits(bd); }
        }

        string QuickSavePath()
        {
            if (string.IsNullOrEmpty(filePath)) return "";
            string dir = Path.GetDirectoryName(filePath);
            string name = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);
            if (name.EndsWith("_censor", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(dir, name + ext);
            return Path.Combine(dir, name + "_censor" + ext);
        }

        bool DoQuickSave()
        {
            if (currentBmp == null) return false;
            if (string.IsNullOrEmpty(filePath)) return DoSaveAs();
            string target = QuickSavePath();
            SaveTo(target);
            return true;
        }

        bool DoSaveAs()
        {
            if (currentBmp == null) return false;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG 图片|*.png|JPEG 图片|*.jpg;*.jpeg|BMP 图片|*.bmp";
                string defName = string.IsNullOrEmpty(filePath) ? "未命名_打码.png" : Path.GetFileNameWithoutExtension(filePath) + "_censor" + Path.GetExtension(filePath);
                dlg.FileName = defName;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SaveTo(dlg.FileName);
                    filePath = dlg.FileName;
                    UpdateTitle();
                    UpdateStatus();
                    return true;
                }
            }
            return false;
        }

        void SaveTo(string path)
        {
            try
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    using (Bitmap tmp = new Bitmap(imgW, imgH, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics g = Graphics.FromImage(tmp)) { g.DrawImage(currentBmp, 0, 0, imgW, imgH); }
                        ImageCodecInfo codec = GetJpegCodec();
                        using (EncoderParameters ep = new EncoderParameters(1))
                        {
                            ep.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
                            tmp.Save(path, codec, ep);
                        }
                    }
                }
                else
                {
                    currentBmp.Save(path, ImageFormat.Png);
                }
                isDirty = false;
                UpdateTitle();
                saveTimer.Stop();
                statusLeft.Text = "✓ 已保存 · " + Path.GetFileName(path);
                saveTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void DoCopy()
        {
            if (currentBmp == null) return;
            try
            {
                Clipboard.SetImage(currentBmp);
                saveTimer.Stop();
                statusLeft.Text = "✓ 已复制图片到剪贴板";
                saveTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "复制到剪贴板失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void DoPaste()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    if (isDirty && currentBmp != null)
                    {
                        DialogResult res = MessageBox.Show(this, "当前图片有未保存的更改，是否覆盖？", "打码笔",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (res != DialogResult.Yes) return;
                    }
                    Image img = Clipboard.GetImage();
                    if (img != null)
                    {
                        Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(img, 0, 0, img.Width, img.Height);
                        }
                        img.Dispose();
                        SetBitmap(bmp, "");
                        isDirty = true;
                        UpdateTitle();
                        saveTimer.Stop();
                        statusLeft.Text = "✓ 已从剪贴板粘贴图片";
                        saveTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "粘贴图片失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static ImageCodecInfo GetJpegCodec()
        {
            foreach (ImageCodecInfo c in ImageCodecInfo.GetImageEncoders())
                if (c.FormatID == ImageFormat.Jpeg.Guid) return c;
            return null;
        }

        // ================= undo =================

        const long UndoBudgetBytes = 512L * 1024 * 1024;

        void PushUndo()
        {
            undoStack.Add((byte[])curPixels.Clone());
            long total = 0;
            foreach (byte[] b in undoStack) total += b.Length;
            while (undoStack.Count > 1 && (undoStack.Count > 20 || total > UndoBudgetBytes))
            {
                total -= undoStack[0].Length;
                undoStack.RemoveAt(0);
            }
            RefreshCommandStates();
        }

        void DoUndo()
        {
            if (undoStack.Count == 0 || currentBmp == null) return;
            int last = undoStack.Count - 1;
            curPixels = undoStack[last];
            undoStack.RemoveAt(last);
            WriteBack(new Rectangle(0, 0, imgW, imgH));
            if (undoStack.Count == 0) isDirty = false;
            UpdateTitle();
            RefreshCommandStates();
            UpdateStatus();
            canvas.Invalidate();
        }

        // ================= view =================

        void FitView()
        {
            if (currentBmp == null || canvas.Width <= 0 || canvas.Height <= 0) return;
            float zx = (canvas.Width - 32f) / imgW;
            float zy = (canvas.Height - 32f) / imgH;
            zoom = Math.Max(0.01f, Math.Min(zx, zy));
            panX = (canvas.Width - imgW * zoom) / 2f;
            panY = (canvas.Height - imgH * zoom) / 2f;
            UpdateStatus();
        }

        void ZoomStep(float factor)
        {
            if (currentBmp == null) return;
            float cx = canvas.Width / 2f, cy = canvas.Height / 2f;
            float ix = (cx - panX) / zoom, iy = (cy - panY) / zoom;
            float nz = Math.Max(0.05f, Math.Min(40f, zoom * factor));
            if (nz == zoom) return;
            zoom = nz;
            panX = cx - ix * zoom;
            panY = cy - iy * zoom;
            UpdateStatus();
            canvas.Invalidate();
        }

        void UpdateStatus()
        {
            if (currentBmp == null)
            {
                statusLeft.Text = "就绪";
                dimLabel.Text = "";
                zoomLabel.Text = "";
                statusRight.Visible = false;
                return;
            }
            statusRight.Visible = true;
            string name = string.IsNullOrEmpty(filePath) ? "未命名图片" : Path.GetFileName(filePath);
            statusLeft.Text = name + (isDirty ? " *" : "");
            dimLabel.Text = imgW + " × " + imgH + " 像素";
            zoomLabel.Text = (int)Math.Round(zoom * 100) + "%";
        }

        void InvalidateBrushAt(Point p)
        {
            float r = brushSlider.Value / 2f * zoom + 6f;
            Rectangle rc = new Rectangle((int)(p.X - r), (int)(p.Y - r), (int)(r * 2) + 2, (int)(r * 2) + 2);
            canvas.Invalidate(rc);
        }

        Rectangle ImageRectToScreen(Rectangle rc)
        {
            int x = (int)Math.Floor(panX + rc.X * zoom);
            int y = (int)Math.Floor(panY + rc.Y * zoom);
            int w = (int)Math.Ceiling(rc.Width * zoom);
            int h = (int)Math.Ceiling(rc.Height * zoom);
            return new Rectangle(x, y, w, h);
        }

        void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Canvas); // Clear background to provide pristine backdrop for ClearType and eliminate redraw artifacts
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (currentBmp == null)
            {
                int cx = canvas.Width / 2, cy = canvas.Height / 2;
                using (Font glyphF = new Font(Program.IconFontName, 54f * scaleFactor, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Font titleF = new Font(Program.UiFontName, 13f, FontStyle.Regular, GraphicsUnit.Point))
                using (Font subF = new Font(Program.UiFontName, 9.5f, FontStyle.Regular, GraphicsUnit.Point))
                {
                    int iconW = (int)Math.Round(120 * scaleFactor);
                    int iconH = (int)Math.Round(80 * scaleFactor);
                    Color iconFg = Theme.Blend(Theme.Secondary, Theme.Canvas, 0.45f);

                    if (Program.FamilyExists("Segoe Fluent Icons") || Program.FamilyExists("Segoe MDL2 Assets"))
                    {
                        TextRenderer.DrawText(g, "\uE91B", glyphF,
                            new Rectangle(cx - iconW / 2, cy - (int)Math.Round(100 * scaleFactor), iconW, iconH),
                            iconFg,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                    else
                    {
                        Rectangle photoRect = new Rectangle(cx - (int)(32 * scaleFactor), cy - (int)(90 * scaleFactor), (int)(64 * scaleFactor), (int)(48 * scaleFactor));
                        IconArt.PhotoEmpty(g, photoRect, iconFg);
                    }

                    TextRenderer.DrawText(g, "拖放图片到这里", titleF,
                        new Rectangle(cx - (int)Math.Round(200 * scaleFactor), cy - (int)Math.Round(16 * scaleFactor), (int)Math.Round(400 * scaleFactor), (int)Math.Round(28 * scaleFactor)),
                        Theme.Text,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "或双击任意处打开 · PNG / JPG / BMP", subF,
                        new Rectangle(cx - (int)Math.Round(200 * scaleFactor), cy + (int)Math.Round(14 * scaleFactor), (int)Math.Round(400 * scaleFactor), (int)Math.Round(22 * scaleFactor)),
                        Theme.Secondary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                if (dragOver) DrawDragFrame(g);
                return;
            }

            // Pixel-align integer destination coordinates for razor-sharp rendering
            int ix = (int)Math.Round(panX);
            int iy = (int)Math.Round(panY);
            int idw = (int)Math.Round(imgW * zoom);
            int idh = (int)Math.Round(imgH * zoom);

            // soft shadow
            using (Brush sh1 = new SolidBrush(Color.FromArgb(22, 0, 0, 0)))
                g.FillRectangle(sh1, ix + 1, iy + 4, idw, idh);
            using (Brush sh2 = new SolidBrush(Color.FromArgb(16, 0, 0, 0)))
                g.FillRectangle(sh2, ix, iy + 2, idw, idh);

            g.InterpolationMode = zoom >= 1f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(currentBmp, ix, iy, idw, idh);
            g.PixelOffsetMode = PixelOffsetMode.Default;

            g.SmoothingMode = SmoothingMode.None;
            using (Pen border = new Pen(Theme.ImageBorder, 1))
            {
                g.DrawRectangle(border, ix, iy, idw, idh);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (hasMouse && !panning)
            {
                DrawBrushRing(g, lastMouse);
            }
            if (dragOver) DrawDragFrame(g);
        }

        void DrawBrushRing(Graphics g, Point m)
        {
            float r = brushSlider.Value / 2f * zoom;
            if (r < 2f) r = 2f;
            using (Pen halo = new Pen(Color.FromArgb(210, 255, 255, 255), 3f))
            using (Pen ring = new Pen(Theme.Accent, 1.5f))
            {
                g.DrawEllipse(halo, m.X - r, m.Y - r, r * 2, r * 2);
                g.DrawEllipse(ring, m.X - r, m.Y - r, r * 2, r * 2);
            }
        }

        void DrawDragFrame(Graphics g)
        {
            using (Pen p = new Pen(Theme.Accent, 2f))
            {
                p.DashStyle = DashStyle.Dash;
                g.DrawRectangle(p, 8, 8, canvas.Width - 17, canvas.Height - 17);
            }
        }

        // ================= mouse / strokes =================

        bool TryImgPoint(Point m, out float ix, out float iy)
        {
            ix = (m.X - panX) / zoom;
            iy = (m.Y - panY) / zoom;
            return currentBmp != null;
        }

        void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            canvas.Focus();
            if (currentBmp == null) return;
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && spaceDown))
            {
                panning = true;
                panStartMouse = e.Location;
                panStartX = panX; panStartY = panY;
                canvas.Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                strokeRestore = (e.Button == MouseButtons.Right) || restoreMode;
                PushUndo();
                stroking = true;
                hasLastImg = false;
                lastMouse = e.Location; hasMouse = true;
                ApplyStrokeTo(e.Location);
                isDirty = true;
                UpdateTitle();
                UpdateStatus();
            }
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (panning)
            {
                lastMouse = e.Location; hasMouse = true;
                panX = panStartX + (e.Location.X - panStartMouse.X);
                panY = panStartY + (e.Location.Y - panStartMouse.Y);
                canvas.Invalidate();
                return;
            }
            if (stroking)
            {
                lastMouse = e.Location; hasMouse = true;
                ApplyStrokeTo(e.Location);
                return;
            }
            // idle: repaint only the brush ring's old + new spots (keeps motion fluid)
            Point old = lastMouse;
            lastMouse = e.Location;
            hasMouse = true;
            if (currentBmp != null)
            {
                InvalidateBrushAt(old);
                InvalidateBrushAt(lastMouse);
            }
        }

        void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (panning && (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left))
            {
                panning = false;
                canvas.Cursor = spaceDown ? Cursors.SizeAll : Cursors.Cross;
            }
            if (stroking && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                stroking = false;
                hasLastImg = false;
                canvas.Invalidate();
            }
        }

        void ApplyStrokeTo(Point pt)
        {
            float ix, iy;
            if (!TryImgPoint(pt, out ix, out iy)) return;
            Rectangle dirty = Rectangle.Empty;
            if (!hasLastImg)
            {
                Stamp(ix, iy, ref dirty);
                lastImgX = ix; lastImgY = iy;
                hasLastImg = true;
            }
            else
            {
                float dx = ix - lastImgX, dy = iy - lastImgY;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                float step = Math.Max(1f, brushSlider.Value * 0.15f);
                int n = Math.Max(1, (int)Math.Ceiling(dist / step));
                for (int i = 1; i <= n; i++)
                {
                    Stamp(lastImgX + dx * i / n, lastImgY + dy * i / n, ref dirty);
                }
                lastImgX = ix; lastImgY = iy;
            }
            if (!dirty.IsEmpty)
            {
                WriteBack(dirty);
                Rectangle sr = ImageRectToScreen(dirty);
                int pad = (int)(brushSlider.Value / 2f * zoom) + 6;
                sr.Inflate(pad, pad);
                canvas.Invalidate(sr);
            }
        }

        void Stamp(float cx, float cy, ref Rectangle dirty)
        {
            if (strokeRestore) StampRestore(cx, cy, ref dirty);
            else StampMosaic(cx, cy, ref dirty);
        }

        void StampMosaic(float cx, float cy, ref Rectangle dirty)
        {
            int B = blockSlider.Value;
            float r = brushSlider.Value / 2f;
            int bx0 = (int)Math.Floor((cx - r) / B);
            int bx1 = (int)Math.Floor((cx + r) / B);
            int by0 = (int)Math.Floor((cy - r) / B);
            int by1 = (int)Math.Floor((cy + r) / B);
            for (int by = by0; by <= by1; by++)
            {
                for (int bx = bx0; bx <= bx1; bx++)
                {
                    int cellX0 = bx * B, cellY0 = by * B;
                    int x0 = Math.Max(cellX0, 0), y0 = Math.Max(cellY0, 0);
                    int x1 = Math.Min(cellX0 + B, imgW), y1 = Math.Min(cellY0 + B, imgH);
                    if (x0 >= x1 || y0 >= y1) continue;
                    float nx = Math.Max(x0, Math.Min(cx, x1));
                    float ny = Math.Max(y0, Math.Min(cy, y1));
                    float ddx = nx - cx, ddy = ny - cy;
                    if (ddx * ddx + ddy * ddy > r * r) continue;
                    long sb = 0, sg = 0, sr = 0, sa = 0;
                    int cnt = 0;
                    for (int y = y0; y < y1; y++)
                    {
                        int row = y * imgW * 4;
                        for (int x = x0; x < x1; x++)
                        {
                            int i = row + x * 4;
                            sb += origPixels[i];
                            sg += origPixels[i + 1];
                            sr += origPixels[i + 2];
                            sa += origPixels[i + 3];
                            cnt++;
                        }
                    }
                    byte ab = (byte)(sb / cnt), ag = (byte)(sg / cnt),
                         ar = (byte)(sr / cnt), aa = (byte)(sa / cnt);
                    for (int y = y0; y < y1; y++)
                    {
                        int row = y * imgW * 4;
                        for (int x = x0; x < x1; x++)
                        {
                            int i = row + x * 4;
                            curPixels[i] = ab;
                            curPixels[i + 1] = ag;
                            curPixels[i + 2] = ar;
                            curPixels[i + 3] = aa;
                        }
                    }
                    dirty = Rectangle.Union(dirty, new Rectangle(x0, y0, x1 - x0, y1 - y0));
                }
            }
        }

        void StampRestore(float cx, float cy, ref Rectangle dirty)
        {
            float r = brushSlider.Value / 2f;
            int x0 = Math.Max(0, (int)Math.Floor(cx - r));
            int y0 = Math.Max(0, (int)Math.Floor(cy - r));
            int x1 = Math.Min(imgW - 1, (int)Math.Ceiling(cx + r));
            int y1 = Math.Min(imgH - 1, (int)Math.Ceiling(cy + r));
            for (int y = y0; y <= y1; y++)
            {
                int row = y * imgW * 4;
                for (int x = x0; x <= x1; x++)
                {
                    float ddx = (x + 0.5f) - cx, ddy = (y + 0.5f) - cy;
                    if (ddx * ddx + ddy * ddy <= r * r)
                    {
                        int i = row + x * 4;
                        curPixels[i] = origPixels[i];
                        curPixels[i + 1] = origPixels[i + 1];
                        curPixels[i + 2] = origPixels[i + 2];
                        curPixels[i + 3] = origPixels[i + 3];
                    }
                }
            }
            dirty = Rectangle.Union(dirty, new Rectangle(x0, y0, x1 - x0 + 1, y1 - y0 + 1));
        }

        void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentBmp == null) return;
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // Zoom centered at cursor
                float ix, iy;
                if (!TryImgPoint(e.Location, out ix, out iy)) return;
                float factor = e.Delta > 0 ? 1.15f : (1f / 1.15f);
                float nz = Math.Max(0.05f, Math.Min(40f, zoom * factor));
                if (nz == zoom) return;
                zoom = nz;
                panX = e.Location.X - ix * zoom;
                panY = e.Location.Y - iy * zoom;
                UpdateStatus();
                canvas.Invalidate();
            }
            else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                // Shift + Wheel = horizontal pan
                panX += (e.Delta > 0 ? 40f : -40f) * scaleFactor;
                canvas.Invalidate();
            }
            else
            {
                // Normal Wheel = vertical pan
                panY += (e.Delta > 0 ? 40f : -40f) * scaleFactor;
                canvas.Invalidate();
            }
        }

        // ================= keys & close =================

        void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                spaceDown = true;
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (!panning) canvas.Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Z) { DoUndo(); e.Handled = true; return; }
            if (e.Control && e.Shift && e.KeyCode == Keys.S) { DoSaveAs(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.S) { DoQuickSave(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.O) { DoOpen(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.C) { DoCopy(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.V) { DoPaste(); e.Handled = true; return; }
            if (e.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)) { ZoomStep(1.25f); e.Handled = true; return; }
            if (e.Control && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)) { ZoomStep(1f / 1.25f); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.D0) { FitView(); canvas.Invalidate(); e.Handled = true; return; }
        }

        void Form_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                spaceDown = false;
                if (!panning) canvas.Cursor = Cursors.Cross;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isDirty && currentBmp != null)
            {
                string name = string.IsNullOrEmpty(filePath) ? "当前图片" : Path.GetFileName(filePath);
                DialogResult res = MessageBox.Show(this, "是否将更改保存到 " + name + "？", "打码笔",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    if (string.IsNullOrEmpty(filePath))
                    {
                        if (!DoSaveAs()) { e.Cancel = true; return; }
                    }
                    else
                    {
                        DoQuickSave();
                    }
                }
                else if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }
    }
}
