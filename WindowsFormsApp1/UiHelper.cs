using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public static class UiHelper
    {
        public static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            // Prevent diameter from exceeding bounds
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;
            if (diameter <= 0) diameter = 1;

            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void MakeRounded(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0) return;
            try
            {
                control.Region = new Region(GetRoundedRectanglePath(new Rectangle(0, 0, control.Width, control.Height), radius));
                control.SizeChanged += (s, e) =>
                {
                    if (control.Width > 0 && control.Height > 0)
                    {
                        control.Region = new Region(GetRoundedRectanglePath(new Rectangle(0, 0, control.Width, control.Height), radius));
                    }
                };
            }
            catch { /* protect against drawing crashes in designer */ }
        }

        public static void StyleFlatButton(Button button, Color backColor, Color hoverColor, int radius)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.Cursor = Cursors.Hand;
            MakeRounded(button, radius);
            button.MouseEnter += (s, e) => { button.BackColor = hoverColor; };
            button.MouseLeave += (s, e) => { button.BackColor = backColor; };
        }
        
        public static void StyleModernGrid(DataGridView grid)
        {
            if (grid == null) return;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 40;
            grid.GridColor = Color.FromArgb(240, 242, 245);

            // Grid header style
            grid.ColumnHeadersHeight = 45;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 32, 65); // Navy header
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Grid rows style
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 60);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 240, 250); // Light blue selection
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 32, 65); // Deep navy text for selection
            
            // Alternating rows style
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
        }
    }
}
