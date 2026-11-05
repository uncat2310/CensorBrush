using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            Dark = false;
            try
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (k != null)
                {
                    object v = k.GetValue("AppsUseLightTheme");
                    if (v is int && (int)v == 0) Dark = true;
                    k.Close();
                }
            }
            catch { }
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

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radius * 2;
            if (r.Width <= 0 || r.Height <= 0) return p;
            if (radius <= 0) { p.AddRectangle(r); return p; }
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
            Rectangle b = Box(r, (int)(r.Height * 0.64f));
            using (Pen p = PenFor(c, 1.5f * r.Height / 28f))
            {
                int l = Math.Max(3, b.Width / 3);
                g.DrawLine(p, b.X, b.Y + l, b.X, b.Y); g.DrawLine(p, b.X, b.Y, b.X + l, b.Y);
                g.DrawLine(p, b.Right - l, b.Y, b.Right, b.Y); g.DrawLine(p, b.Right, b.Y, b.Right, b.Y + l);
                g.DrawLine(p, b.Right, b.Bottom - l, b.Right, b.Bottom); g.DrawLine(p, b.Right, b.Bottom, b.Right - l, b.Bottom);
                g.DrawLine(p, b.X + l, b.Bottom, b.X, b.Bottom); g.DrawLine(p, b.X, b.Bottom, b.X, b.Bottom - l);
            }
        }

        public static void Grid3(Graphics g, Rectangle r, Color c)
        {
            Rectangle b = Box(r, (int)(r.Height * 0.66f));
            using (Brush br = new SolidBrush(c))
            using (Pen p = new Pen(c, 1.2f * r.Height / 24f))
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
            Rectangle b = Box(r, (int)(r.Height * 0.57f));
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
            Rectangle b = Box(r, (int)(r.Height * 0.5f));
            using (Pen p = new Pen(c, 1.5f * r.Height / 28f))
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
            float f = r.Height / 24f;
            using (Pen p = PenFor(c, 1.5f * f))
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
            using (Pen p = PenFor(c))
            {
                int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
                int half = Math.Max(3, r.Height / 4);
                g.DrawLine(p, cx - half, cy, cx + half, cy);
                g.DrawLine(p, cx, cy - half, cx, cy + half);
            }
        }

        public static void Minus(Graphics g, Rectangle r, Color c)
        {
            using (Pen p = PenFor(c))
            {
                int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
                int half = Math.Max(3, r.Height / 4);
                g.DrawLine(p, cx - half, cy, cx + half, cy);
            }
        }
    }

    // ================= flat icon button (Win11 command-bar style) =================
    class IconButton : Button
    {
        public string Glyph;
        public Action<Graphics, Rectangle, Color> Vector;
        bool hover, down;

        Font glyphFontCache;
        float glyphFontSizeCache = 0f;

        Font GetGlyphFont(float dcDpi)
        {
            float sz = Math.Max(8f, Height * 0.52f * 96f / Math.Max(72f, dcDpi));
            if (glyphFontCache == null || Math.Abs(sz - glyphFontSizeCache) > 0.5f)
            {
                if (glyphFontCache != null) glyphFontCache.Dispose();
                glyphFontCache = new Font(Program.IconFontName, sz);
                glyphFontSizeCache = sz;
            }
            return glyphFontCache;
        }

        public IconButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            UseVisualStyleBackColor = false;
            TabStop = false;
            Text = "";
            Cursor = Cursors.Default;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fg = Enabled ? ForeColor : Theme.Disabled;
            if (Enabled && (hover || down))
            {
                Rectangle rr = new Rectangle(1, 1, Width - 2, Height - 2);
                using (GraphicsPath path = Theme.RoundedRect(rr, 4))
                using (Brush b = new SolidBrush(down ? Theme.Pressed : Theme.Hover))
                    g.FillPath(b, path);
            }
            if (Vector != null)
            {
                Vector(g, ClientRectangle, fg);
            }
            else if (Glyph != null)
            {
                TextRenderer.DrawText(g, Glyph, GetGlyphFont(g.DpiX), ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }
    }

    // ================= segmented mode button =================
    class ModeButton : Button
    {
        public bool Selected;
        public Action<Graphics, Rectangle, Color> Vector;
        bool hover, down;

        public ModeButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            UseVisualStyleBackColor = false;
            TabStop = false;
            Text = "";
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fg = Theme.Text;
            Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
            if (Selected)
            {
                using (GraphicsPath path = Theme.RoundedRect(rr, 4))
                {
                    using (Brush b = new SolidBrush(Theme.SegSelected)) g.FillPath(b, path);
                    using (Pen p = new Pen(Theme.Border)) g.DrawPath(p, path);
                }
            }
            else if (hover || down)
            {
                using (GraphicsPath path = Theme.RoundedRect(rr, 4))
                using (Brush b = new SolidBrush(down ? Theme.Pressed : Theme.Hover))
                    g.FillPath(b, path);
            }
            int pad = Math.Max(4, (int)(8 * Height / 24f));
            int iconW = (int)(20 * Height / 24f);
            if (Vector != null) Vector(g, new Rectangle(pad, 0, iconW, Height), fg);
            Rectangle tr = new Rectangle(pad + iconW, 0, Width - pad - iconW, Height);
            TextRenderer.DrawText(g, Text, Font, tr, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    // ================= accent button (empty state) =================
    class AccentButton : Button
    {
        bool hover, down;
        public AccentButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            UseVisualStyleBackColor = false;
            TabStop = false;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            Color fill = down ? Theme.Accent : (hover ? Theme.AccentButtonHover : Theme.AccentButton);
            using (GraphicsPath path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 4))
            using (Brush b = new SolidBrush(fill))
                g.FillPath(b, path);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Color.White,
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

        float Inset() { return Height * 0.3f; }

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
            float ins = Inset(); float th = Math.Max(3f, Height * 0.14f);
            RectangleF track = new RectangleF(ins, cy - th / 2f, Width - ins * 2f, th);
            using (GraphicsPath tp = Theme.RoundedRect(Rectangle.Round(track), Math.Max(1, (int)(th / 2f))))
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
            int r = (int)((dragging || hoverThumb) ? Height * 0.30f : Height * 0.26f); if (r < 5) r = 5;
            using (Brush wb = new SolidBrush(Color.White))
            using (Pen bp = new Pen(Color.FromArgb(145, 145, 145), 1))
            {
                g.FillEllipse(wb, tx - r, cy - r, r * 2, r * 2);
                g.DrawEllipse(bp, tx - r, cy - r, r * 2, r * 2);
            }
            if (dragging || hoverThumb)
            {
                using (Brush ab = new SolidBrush(Theme.Accent))
                {
                    float dr = Height * 0.11f; g.FillEllipse(ab, tx - dr, cy - dr, dr * 2f, dr * 2f);
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
            hoverThumb = Math.Abs(e.X - ThumbX()) <= Height * 0.4f && Math.Abs(e.Y - Height / 2) <= Height * 0.45f;
            if (dragging) UpdateFromX(e.X); else Invalidate();
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false; Capture = false; Invalidate();
            base.OnMouseUp(e);
        }
        protected override void OnMouseLeave(EventArgs e) { hoverThumb = false; Invalidate(); base.OnMouseLeave(e); }
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

    static class Program
    {
        public static string IconFontName = "Segoe MDL2 Assets";
        public static string UiFontName = "Microsoft YaHei UI";

        [STAThread]
        static void Main(string[] args)
        {
            // DPI awareness (PerMonitorV2 + WinForms auto-resizing) comes from CensorBrush.exe.config
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            IconFontName = FamilyExists("Segoe Fluent Icons") ? "Segoe Fluent Icons"
                : (FamilyExists("Segoe MDL2 Assets") ? "Segoe MDL2 Assets" : "Segoe UI Symbol");
            UiFontName = FamilyExists("Segoe UI Variable Text") ? "Segoe UI Variable Text"
                : (FamilyExists("Segoe UI") ? "Segoe UI" : "Microsoft YaHei UI");
            MainForm f = new MainForm();
            if (args.Length > 0 && File.Exists(args[0]))
            {
                string p = args[0];
                f.Shown += delegate { f.OpenImage(p); };
            }
            Application.Run(f);
        }

        static bool FamilyExists(string name)
        {
            foreach (FontFamily ff in FontFamily.Families)
                if (ff.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
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
        string filePath;

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
        Label brushVal, blockVal, statusLeft, dimLabel, zoomLabel;
        ModeButton modeMosaicBtn, modeEraseBtn;
        IconButton btnSave, btnSaveAs, btnUndo, btnFit, btnZoomIn, btnZoomOut;
        AccentButton btnOpenBig;
        FlowLayoutPanel statusRight;
        ToolTip tip;
        Timer saveTimer;

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        public MainForm()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "打码笔";
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            Width = 1120;
            Height = 760;
            MinimumSize = new Size(840, 560);
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


            // ---------- command bar ----------
            FlowLayoutPanel header = new FlowLayoutPanel();
            header.Dock = DockStyle.Top;
            header.Height = 44;
            header.WrapContents = false;
            header.Padding = new Padding(8, 8, 4, 0);
            header.BackColor = Theme.Bar;

            IconButton btnOpen = new IconButton();
            btnOpen.Glyph = "\uE838";
            btnOpen.Size = new Size(36, 28);
            btnOpen.Margin = new Padding(0, 0, 2, 0);
            btnOpen.BackColor = Theme.Bar; btnOpen.ForeColor = Theme.Text;
            tip.SetToolTip(btnOpen, "打开图片 (Ctrl+O)");
            btnOpen.Click += delegate { DoOpen(); };

            btnSave = new IconButton();
            btnSave.Glyph = "\uE74E";
            btnSave.Size = new Size(36, 28);
            btnSave.Margin = new Padding(0, 0, 2, 0);
            btnSave.BackColor = Theme.Bar; btnSave.ForeColor = Theme.Text;
            tip.SetToolTip(btnSave, "保存 (Ctrl+S)");
            btnSave.Click += delegate { DoQuickSave(); };

            btnSaveAs = new IconButton();
            btnSaveAs.Glyph = "\uE8C8";
            btnSaveAs.Size = new Size(36, 28);
            btnSaveAs.Margin = new Padding(0, 0, 2, 0);
            btnSaveAs.BackColor = Theme.Bar; btnSaveAs.ForeColor = Theme.Text;
            tip.SetToolTip(btnSaveAs, "另存为");
            btnSaveAs.Click += delegate { DoSaveAs(); };

            btnUndo = new IconButton();
            btnUndo.Glyph = "\uE7A7";
            btnUndo.Size = new Size(36, 28);
            btnUndo.Margin = new Padding(0, 0, 2, 0);
            btnUndo.BackColor = Theme.Bar; btnUndo.ForeColor = Theme.Text;
            tip.SetToolTip(btnUndo, "撤销 (Ctrl+Z)");
            btnUndo.Click += delegate { DoUndo(); };

            btnFit = new IconButton();
            btnFit.Vector = IconArt.Fit;
            btnFit.Size = new Size(36, 28);
            btnFit.Margin = new Padding(0, 0, 2, 0);
            btnFit.BackColor = Theme.Bar; btnFit.ForeColor = Theme.Text;
            tip.SetToolTip(btnFit, "适合窗口 (Ctrl+0)");
            btnFit.Click += delegate { FitView(); canvas.Invalidate(); };

            brushSlider = new ModernSlider();
            brushSlider.Minimum = 10; brushSlider.Maximum = 300; brushSlider.Value = 80;
            brushSlider.Width = 104;
            brushSlider.Margin = new Padding(2, 0, 2, 0);
            brushSlider.BackColor = Theme.Bar;
            tip.SetToolTip(brushSlider, "笔刷大小");
            brushSlider.ValueChanged += delegate
            {
                brushVal.Text = brushSlider.Value.ToString();
                if (hasMouse && currentBmp != null) InvalidateBrushAt(lastMouse);
            };

            blockSlider = new ModernSlider();
            blockSlider.Minimum = 4; blockSlider.Maximum = 40; blockSlider.Value = 12;
            blockSlider.Width = 80;
            blockSlider.Margin = new Padding(2, 0, 2, 0);
            blockSlider.BackColor = Theme.Bar;
            tip.SetToolTip(blockSlider, "马赛克块大小");
            blockSlider.ValueChanged += delegate { blockVal.Text = blockSlider.Value.ToString(); };

            brushVal = MakePill("80");
            blockVal = MakePill("12");

            StaticIcon brushIcon = new StaticIcon();
            brushIcon.Vector = IconArt.BrushDot;
            brushIcon.Size = new Size(20, 28);
            brushIcon.Margin = new Padding(6, 0, 0, 0);
            brushIcon.BackColor = Theme.Bar;
            tip.SetToolTip(brushIcon, "笔刷大小");

            StaticIcon blockIcon = new StaticIcon();
            blockIcon.Vector = IconArt.Grid2;
            blockIcon.Size = new Size(20, 28);
            blockIcon.Margin = new Padding(6, 0, 0, 0);
            blockIcon.BackColor = Theme.Bar;
            tip.SetToolTip(blockIcon, "马赛克块大小");

            // segmented mode switch
            Panel segPanel = new Panel();
            segPanel.Size = new Size(158, 28);
            segPanel.Margin = new Padding(8, 0, 0, 0);
            segPanel.BackColor = Theme.SegBg;

            modeMosaicBtn = new ModeButton();
            modeMosaicBtn.Text = "打码";
            modeMosaicBtn.Vector = IconArt.Grid3;
            modeMosaicBtn.Font = new Font(Program.UiFontName, 9f);
            modeMosaicBtn.Location = new Point(2, 2);
            modeMosaicBtn.Size = new Size(76, 24);
            modeMosaicBtn.BackColor = Theme.SegBg;
            modeMosaicBtn.Selected = true;
            tip.SetToolTip(modeMosaicBtn, "马赛克笔 · 左键涂抹打码");
            modeMosaicBtn.Click += delegate { SetRestoreMode(false); };

            modeEraseBtn = new ModeButton();
            modeEraseBtn.Text = "还原";
            modeEraseBtn.Vector = IconArt.Eraser;
            modeEraseBtn.Font = new Font(Program.UiFontName, 9f);
            modeEraseBtn.Location = new Point(80, 2);
            modeEraseBtn.Size = new Size(76, 24);
            modeEraseBtn.BackColor = Theme.SegBg;
            modeEraseBtn.Selected = false;
            tip.SetToolTip(modeEraseBtn, "还原笔 · 左键擦除打码");
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

            // ---------- status bar ----------
            Panel statusPanel = new Panel();
            statusPanel.Dock = DockStyle.Bottom;
            statusPanel.Height = 26;
            statusPanel.BackColor = Theme.Bar;
            statusPanel.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Theme.Border)) e.Graphics.DrawLine(p, 0, 0, statusPanel.Width, 0);
            };

            statusLeft = new Label();
            statusLeft.Dock = DockStyle.Left;
            statusLeft.AutoSize = true;
            statusLeft.TextAlign = ContentAlignment.MiddleLeft;
            statusLeft.Padding = new Padding(10, 0, 0, 0);
            statusLeft.Font = new Font(Program.UiFontName, 9f);
            statusLeft.ForeColor = Theme.Secondary;
            statusLeft.Text = "";
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
            dimLabel.Margin = new Padding(0, 5, 10, 0);
            dimLabel.Font = new Font(Program.UiFontName, 9f);
            dimLabel.ForeColor = Theme.Secondary;
            dimLabel.Text = "";

            btnZoomOut = new IconButton();
            btnZoomOut.Vector = IconArt.Minus;
            btnZoomOut.Size = new Size(26, 20);
            btnZoomOut.Margin = new Padding(0, 3, 0, 0);
            btnZoomOut.BackColor = Theme.Bar; btnZoomOut.ForeColor = Theme.Text;
            tip.SetToolTip(btnZoomOut, "缩小 (Ctrl+-)");
            btnZoomOut.Click += delegate { ZoomStep(1f / 1.25f); };

            zoomLabel = new Label();
            zoomLabel.Size = new Size(48, 20);
            zoomLabel.Margin = new Padding(0, 3, 0, 0);
            zoomLabel.TextAlign = ContentAlignment.MiddleCenter;
            zoomLabel.Font = new Font(Program.UiFontName, 9f);
            zoomLabel.ForeColor = Theme.Text;
            zoomLabel.Cursor = Cursors.Hand;
            zoomLabel.Text = "";
            tip.SetToolTip(zoomLabel, "适合窗口");
            zoomLabel.Click += delegate { FitView(); canvas.Invalidate(); };

            btnZoomIn = new IconButton();
            btnZoomIn.Vector = IconArt.Plus;
            btnZoomIn.Size = new Size(26, 20);
            btnZoomIn.Margin = new Padding(0, 3, 0, 0);
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
            btnOpenBig.Font = new Font(Program.UiFontName, 9f);
            btnOpenBig.Size = new Size(112, 32);
            btnOpenBig.BackColor = Theme.Canvas;
            btnOpenBig.Click += delegate { DoOpen(); };
            canvas.Controls.Add(btnOpenBig);

            Controls.Add(canvas);
            Controls.Add(statusPanel);
            Controls.Add(header);

            DragEnter += DragEnterHandler;
            DragDrop += DragDropHandler;
            KeyDown += Form_KeyDown;
            KeyUp += Form_KeyUp;

            saveTimer = new Timer();
            saveTimer.Interval = 2600;
            saveTimer.Tick += delegate { saveTimer.Stop(); UpdateStatus(); };

            RefreshCommandStates();
        }

        bool scaledDone = false;


        void EnsureScaled()
        {
            if (scaledDone) return;
            scaledDone = true;
            uint dpi = 96;
            try { dpi = GetDpiForWindow(Handle); } catch { }
            if (dpi <= 96) return;
            float f = dpi / 96f;
            ScaleTree(this, f);
            MinimumSize = new Size((int)(840 * f), (int)(560 * f));
            ClientSize = new Size((int)(1094 * f), (int)(689 * f));
            Rectangle sb = Screen.FromControl(this).Bounds;
            int ow = (int)(1120 * f), oh = (int)(760 * f);
            SetWindowPos(Handle, IntPtr.Zero, (sb.Width - ow) / 2, (sb.Height - oh) / 2, ow, oh, 0x0004);
            LayoutEmptyButton();
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);

        static Padding ScalePadding(Padding pd, float f)
        {
            return new Padding((int)Math.Round(pd.Left * f), (int)Math.Round(pd.Top * f),
                               (int)Math.Round(pd.Right * f), (int)Math.Round(pd.Bottom * f));
        }

        static void ScaleTree(Control c, float f)
        {
            List<Control> kids = new List<Control>();
            foreach (Control ch in c.Controls) kids.Add(ch);
            foreach (Control ch in kids) ScaleTree(ch, f);
            if (c is Form) return;
            c.SetBounds((int)Math.Round(c.Left * f), (int)Math.Round(c.Top * f),
                        (int)Math.Round(c.Width * f), (int)Math.Round(c.Height * f));
            c.Margin = ScalePadding(c.Margin, f);
            c.Padding = ScalePadding(c.Padding, f);
        }

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr h);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnsureScaled();
            if (Theme.Dark)
            {
                try { int v = 1; DwmSetWindowAttribute(Handle, 20, ref v, 4); } catch { }
            }
        }

        Label MakePill(string text)
        {
            Label l = new Label();
            l.Text = text;
            l.Size = new Size(30, 20);
            l.Margin = new Padding(0, 4, 10, 0);
            l.TextAlign = ContentAlignment.MiddleCenter;
            l.Font = new Font(Program.UiFontName, 9f);
            l.ForeColor = Theme.Text;
            l.BackColor = Theme.Pill;
            l.SizeChanged += delegate
            {
                if (l.Width <= 0 || l.Height <= 0) return;
                using (GraphicsPath p = Theme.RoundedRect(new Rectangle(0, 0, l.Width, l.Height), Math.Max(2, l.Height / 2 - 1)))
                    l.Region = new Region(p);
            };
            return l;
        }

        Panel MakeSeparator()
        {
            Panel p = new Panel();
            p.Size = new Size(1, 20);
            p.Margin = new Padding(8, 4, 8, 0);
            p.BackColor = Theme.Border;
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
            statusRight.Visible = has;
            btnOpenBig.Visible = !has;
            LayoutEmptyButton();
        }

        void LayoutEmptyButton()
        {
            if (canvas == null || btnOpenBig == null) return;
            if (canvas.Width <= 0 || canvas.Height <= 0) return;
            int off = (int)(56 * Math.Max(0.6f, canvas.Height / 690f));
            btnOpenBig.Location = new Point((canvas.Width - btnOpenBig.Width) / 2, canvas.Height / 2 + off);
        }

        // ================= file open / save =================

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
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*";
            dlg.Title = "打开图片";
            if (dlg.ShowDialog(this) == DialogResult.OK) OpenImage(dlg.FileName);
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
                if (currentBmp != null) currentBmp.Dispose();
                currentBmp = bmp;
                imgW = bmp.Width; imgH = bmp.Height;

                origPixels = ExtractPixels(bmp);
                curPixels = (byte[])origPixels.Clone();
                undoStack.Clear();
                filePath = path;
                Text = Path.GetFileName(path) + " - 打码笔";
                FitView();
                RefreshCommandStates();
                UpdateStatus();
                canvas.Invalidate();
                canvas.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "打开失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            string dir = Path.GetDirectoryName(filePath);
            string name = Path.GetFileNameWithoutExtension(filePath);
            return Path.Combine(dir, name + "_censor.png");
        }

        void DoQuickSave()
        {
            if (currentBmp == null) return;
            SaveTo(QuickSavePath());
        }

        void DoSaveAs()
        {
            if (currentBmp == null) return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG 图片|*.png|JPEG 图片|*.jpg";
            dlg.FileName = Path.GetFileName(QuickSavePath());
            dlg.InitialDirectory = Path.GetDirectoryName(filePath);
            if (dlg.ShowDialog(this) == DialogResult.OK) SaveTo(dlg.FileName);
        }

        void SaveTo(string path)
        {
            try
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    Bitmap tmp = new Bitmap(imgW, imgH, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(tmp)) { g.DrawImage(currentBmp, 0, 0, imgW, imgH); }
                    ImageCodecInfo codec = GetJpegCodec();
                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
                    tmp.Save(path, codec, ep);
                    tmp.Dispose();
                }
                else
                {
                    currentBmp.Save(path, ImageFormat.Png);
                }
                saveTimer.Stop();
                statusLeft.Text = "✓ 已保存 · " + Path.GetFileName(path);
                saveTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "打码笔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            RefreshCommandStates();
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
                statusLeft.Text = "";
                dimLabel.Text = "";
                zoomLabel.Text = "";
                return;
            }
            statusLeft.Text = Path.GetFileName(filePath);
            dimLabel.Text = imgW + " × " + imgH;
            zoomLabel.Text = (int)(zoom * 100) + "%";
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
            if (currentBmp == null)
            {
                int cx = canvas.Width / 2, cy = canvas.Height / 2;
                float fs = Math.Max(0.6f, canvas.Height / 690f);
                using (Font glyphF = new Font(Program.IconFontName, 54f))
                using (Font titleF = new Font(Program.UiFontName, 12f))
                using (Font subF = new Font(Program.UiFontName, 9f))
                {
                    TextRenderer.DrawText(g, "\uE91B", glyphF,
                        new Rectangle(cx - (int)(60 * fs), cy - (int)(118 * fs), (int)(120 * fs), (int)(90 * fs)),
                        Color.FromArgb(120, Theme.Secondary.R, Theme.Secondary.G, Theme.Secondary.B),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "拖放图片到这里", titleF,
                        new Rectangle(cx - (int)(160 * fs), cy - (int)(22 * fs), (int)(320 * fs), (int)(28 * fs)), Theme.Text,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "或双击任意处打开 · PNG / JPG / BMP", subF,
                        new Rectangle(cx - (int)(160 * fs), cy + (int)(6 * fs), (int)(320 * fs), (int)(22 * fs)), Theme.Secondary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                if (dragOver) DrawDragFrame(g);
                return;
            }
            // soft shadow
            float dw = imgW * zoom, dh = imgH * zoom;
            using (Brush sh1 = new SolidBrush(Color.FromArgb(22, 0, 0, 0)))
                g.FillRectangle(sh1, panX + 1, panY + 4, dw, dh);
            using (Brush sh2 = new SolidBrush(Color.FromArgb(16, 0, 0, 0)))
                g.FillRectangle(sh2, panX, panY + 2, dw, dh);

            g.InterpolationMode = zoom >= 1f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TranslateTransform(panX, panY);
            g.ScaleTransform(zoom, zoom);
            g.DrawImage(currentBmp, 0, 0, imgW, imgH);
            g.ResetTransform();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen border = new Pen(Theme.ImageBorder, 1))
            {
                g.DrawRectangle(border, panX, panY, dw, dh);
            }
            if (hasMouse && !panning && !stroking)
            {
                DrawBrushRing(g, lastMouse);
            }
            else if (hasMouse && stroking)
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
            if (currentBmp != null)
            {
                if (hasMouse) InvalidateBrushAt(lastMouse);
                lastMouse = e.Location; hasMouse = true;
                InvalidateBrushAt(lastMouse);
            }
            else
            {
                lastMouse = e.Location; hasMouse = true;
            }
        }

        void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            stroking = false;
            if (panning)
            {
                panning = false;
                canvas.Cursor = Cursors.Cross;
            }
        }

        void ApplyStrokeTo(Point m)
        {
            float ix, iy;
            if (!TryImgPoint(m, out ix, out iy)) return;
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
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control) return;
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

        // ================= keys =================

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
            if (e.Control && e.KeyCode == Keys.S) { DoQuickSave(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.O) { DoOpen(); e.Handled = true; return; }
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
    }
}
