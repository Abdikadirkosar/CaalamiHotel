using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class SplashForm : Form
    {
        private Timer animTimer;
        private int   progressValue = 0;
        private float fadeAlpha     = 0f;
        private int   dotCount      = 0;
        private int   dotTick       = 0;
        private Image hotelLogo     = null;

        private static readonly string[] LoadingMessages = new[]
        {
            "Hubinta xiriirka database-ka",
            "Kicinaya adeegga SQL Server LocalDB",
            "Diiwaan-gelinta xogta Soomaalida",
            "Habeynta qolalka iyo martida",
            "Soo bandhigaya nidaamka maamulka"
        };
        private string currentMsg = LoadingMessages[0];

        // ── Colors ─────────────────────────────────────────────────────────
        private static readonly Color Navy      = Color.FromArgb(8,  20, 50);
        private static readonly Color NavyMid   = Color.FromArgb(15, 35, 75);
        private static readonly Color Gold      = Color.FromArgb(212, 160, 23);
        private static readonly Color GoldLight = Color.FromArgb(255, 210, 80);
        private static readonly Color TextSoft  = Color.FromArgb(190, 210, 245);

        public SplashForm()
        {
            this.Size            = new Size(720, 400);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = Navy;
            this.DoubleBuffered  = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint  |
                          ControlStyles.UserPaint, true);

            // Try to load the real hotel logo
            LoadHotelLogo();

            animTimer          = new Timer();
            animTimer.Interval = 16; // ~60 fps
            animTimer.Tick    += AnimTimer_Tick;
            this.Load         += (s, e) => animTimer.Start();
            this.Paint        += SplashForm_Paint;
        }

        // ── Load logo from resource ────────────────────────────────────────
        private void LoadHotelLogo()
        {
            try
            {
                hotelLogo = Properties.Resources.hotel_logo;
            }
            catch { /* logo optional — will draw fallback crest */ }
        }

        // ── Animation tick ─────────────────────────────────────────────────
        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            if (fadeAlpha < 1f) fadeAlpha = Math.Min(1f, fadeAlpha + 0.05f);
            if (progressValue < 100) progressValue++;

            dotTick++;
            if (dotTick % 20 == 0) dotCount = (dotCount + 1) % 4;

            if      (progressValue < 20) currentMsg = LoadingMessages[0];
            else if (progressValue < 40) currentMsg = LoadingMessages[1];
            else if (progressValue < 60) currentMsg = LoadingMessages[2];
            else if (progressValue < 80) currentMsg = LoadingMessages[3];
            else                         currentMsg = LoadingMessages[4];

            this.Invalidate();

            if (progressValue >= 100)
            {
                animTimer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // ── Custom paint ───────────────────────────────────────────────────
        protected override void OnPaintBackground(PaintEventArgs e) { /* suppress */ }

        private void SplashForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;

            int W = this.Width, H = this.Height;

            // ── 1. Background gradient ────────────────────────────────────
            using (var bg = new LinearGradientBrush(new Rectangle(0, 0, W, H),
                       Navy, NavyMid, LinearGradientMode.BackwardDiagonal))
                g.FillRectangle(bg, 0, 0, W, H);

            // ── 2. Top gold accent bar ────────────────────────────────────
            using (var bar = new LinearGradientBrush(new Rectangle(0, 0, W, 5),
                       GoldLight, Gold, LinearGradientMode.Horizontal))
                g.FillRectangle(bar, 0, 0, W, 5);

            // ── 3. Bottom accent bar ──────────────────────────────────────
            using (var bar = new LinearGradientBrush(new Rectangle(0, H-3, W, 3),
                       Gold, GoldLight, LinearGradientMode.Horizontal))
                g.FillRectangle(bar, 0, H - 3, W, 3);

            // ── 4. Logo area (left panel) ─────────────────────────────────
            int logoX = 30, logoY = 30, logoW = 200, logoH = 200;

            if (hotelLogo != null)
            {
                // Draw logo image with rounded white card behind it
                using (GraphicsPath card = RoundRect(logoX - 5, logoY - 5, logoW + 10, logoH + 10, 12))
                using (Brush bg2 = new SolidBrush(Color.White))
                    g.FillPath(bg2, card);

                // Draw logo image scaled
                g.DrawImage(hotelLogo, new Rectangle(logoX, logoY, logoW, logoH));

                // Gold border on card
                using (GraphicsPath card = RoundRect(logoX - 5, logoY - 5, logoW + 10, logoH + 10, 12))
                using (Pen p = new Pen(Gold, 2))
                    g.DrawPath(p, card);
            }
            else
            {
                // Fallback: draw hexagonal crest
                DrawCrest(g, logoX + logoW/2, logoY + logoH/2);
            }

            // ── 5. Hotel Name ─────────────────────────────────────────────
            int textX = 255;
            using (Font f = new Font("Segoe UI", 26, FontStyle.Bold))
            using (var lb = new LinearGradientBrush(
                       new RectangleF(textX, 40, 440, 55), Gold, GoldLight,
                       LinearGradientMode.Horizontal))
                g.DrawString("Caalami Hotel", f, lb, textX, 42);

            // Hargaisa subtitle under the name
            using (Font f = new Font("Segoe UI", 15, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.FromArgb(200, 220, 255)))
                g.DrawString("Hargaisa, Somaliland", f, b, textX + 2, 96);

            // ── 6. Gold stars ─────────────────────────────────────────────
            using (Font f = new Font("Segoe UI", 13))
            using (Brush b = new SolidBrush(Gold))
                g.DrawString("★ ★ ★ ★ ★", f, b, textX + 2, 130);

            // ── 7. Divider ────────────────────────────────────────────────
            using (Pen p = new Pen(Color.FromArgb(55, 212, 160, 23), 1))
                g.DrawLine(p, textX, 162, W - 40, 162);

            // ── 8. Description ────────────────────────────────────────────
            using (Font f = new Font("Segoe UI", 9.5f, FontStyle.Italic))
            using (Brush b = new SolidBrush(TextSoft))
                g.DrawString("Nidaamka Maamulka Hoteelka — Hotel Management System v2.0", f, b, textX, 172);

            using (Font f = new Font("Segoe UI", 9))
            using (Brush b = new SolidBrush(Color.FromArgb(130, 170, 210)))
                g.DrawString("Xog amaanka, kireynta qolalka & maamulka martida", f, b, textX, 194);

            // ── 9. Thin horizontal divider ────────────────────────────────
            using (Pen p = new Pen(Color.FromArgb(35, 212, 160, 23), 1))
                g.DrawLine(p, 30, 248, W - 30, 248);

            // ── 10. Status message with animated dots ─────────────────────
            string dots = new string('.', dotCount);
            using (Font f = new Font("Segoe UI", 9.5f))
            using (Brush b = new SolidBrush(TextSoft))
                g.DrawString($"⏳  {currentMsg}{dots}", f, b, 32, 260);

            // ── 11. Progress bar track ────────────────────────────────────
            int barX = 30, barY = 292, barW = W - 60, barH = 12;
            using (GraphicsPath track = RoundRect(barX, barY, barW, barH, 6))
            using (Brush tb = new SolidBrush(Color.FromArgb(25, 255, 255, 255)))
                g.FillPath(tb, track);

            // Progress fill
            int filled = (int)(barW * progressValue / 100.0);
            if (filled > 6)
            {
                using (GraphicsPath fill = RoundRect(barX, barY, filled, barH, 6))
                using (var lb = new LinearGradientBrush(
                           new Rectangle(barX, barY, Math.Max(1, filled), barH), Gold, GoldLight,
                           LinearGradientMode.Horizontal))
                {
                    g.FillPath(lb, fill);
                    using (Brush shine = new LinearGradientBrush(
                               new Rectangle(barX, barY, Math.Max(1, filled), barH/2),
                               Color.FromArgb(70, 255, 255, 255), Color.Transparent,
                               LinearGradientMode.Vertical))
                        g.FillRectangle(shine, barX, barY, filled, barH / 2);
                }
            }

            // ── 12. Percentage ────────────────────────────────────────────
            using (Font f = new Font("Segoe UI", 9, FontStyle.Bold))
            using (Brush b = new SolidBrush(Gold))
                g.DrawString($"{progressValue}%", f, b, W - 62, 286);

            // ── 13. Copyright footer ──────────────────────────────────────
            using (Font f = new Font("Segoe UI", 8))
            using (Brush b = new SolidBrush(Color.FromArgb(80, 190, 210, 245)))
                g.DrawString("© 2025 CAALAMI HOTEL, Somaliland  •  All rights reserved", f, b, 32, H - 26);

            // ── 14. Fade overlay ──────────────────────────────────────────
            if (fadeAlpha < 1f)
            {
                int alpha = (int)((1f - fadeAlpha) * 240);
                using (Brush fade = new SolidBrush(Color.FromArgb(alpha, 8, 20, 50)))
                    g.FillRectangle(fade, 0, 0, W, H);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private void DrawCrest(Graphics g, int cx, int cy)
        {
            PointF[] hex = HexPoints(cx, cy, 52);
            using (var gb = new LinearGradientBrush(
                       new RectangleF(cx-52, cy-52, 104, 104), NavyMid,
                       Color.FromArgb(25, 55, 110), LinearGradientMode.Vertical))
                g.FillPolygon(gb, hex);
            using (Pen p = new Pen(Gold, 2.5f)) g.DrawPolygon(p, hex);

            using (Font f = new Font("Segoe UI", 34, FontStyle.Bold))
            using (var gb = new LinearGradientBrush(
                       new RectangleF(cx-25, cy-30, 50, 60), Gold, GoldLight,
                       LinearGradientMode.Vertical))
            {
                SizeF sz = g.MeasureString("M", f);
                g.DrawString("M", f, gb, cx - sz.Width / 2, cy - sz.Height / 2);
            }
        }

        private static PointF[] HexPoints(int cx, int cy, int r)
        {
            var pts = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double a = Math.PI / 180 * (60 * i - 30);
                pts[i] = new PointF(cx + r * (float)Math.Cos(a), cy + r * (float)Math.Sin(a));
            }
            return pts;
        }

        private static GraphicsPath RoundRect(int x, int y, int w, int h, int r)
        {
            var path = new GraphicsPath();
            path.AddArc(x,       y,       r*2, r*2, 180, 90);
            path.AddArc(x+w-r*2, y,       r*2, r*2, 270, 90);
            path.AddArc(x+w-r*2, y+h-r*2, r*2, r*2,   0, 90);
            path.AddArc(x,       y+h-r*2, r*2, r*2,  90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animTimer?.Dispose();
                hotelLogo?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
