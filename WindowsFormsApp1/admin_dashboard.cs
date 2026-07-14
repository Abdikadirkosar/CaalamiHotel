using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class admin_dashboard : UserControl
    {
        private string conn = hotelData.ConnectionString;

        public admin_dashboard()
        {
            InitializeComponent();
            ApplyModernDashboardTheme();

            displayTotalStaff();

            displayAvailableRooms();

            displayProfitToday();

            displayProfitTotal();

            displayAllRooms();

            BuildRevenueChart();
        }

        private void ApplyModernDashboardTheme()
        {
            // Cards panel backgrounds
            panel4.BackColor = Color.FromArgb(240, 244, 255); // Clean light blue/gray bg
            panel5.BackColor = Color.White; // Grid container
            UiHelper.MakeRounded(panel5, 12);

            // Card 1: Total Staff
            panel6.BackColor = Color.FromArgb(26, 47, 80); // Deep Navy Blue
            panel6.BorderStyle = BorderStyle.None;
            UiHelper.MakeRounded(panel6, 12);
            StyleCard(panel6, totalStaff, "Total Staff");

            // Card 2: Available Rooms
            panel7.BackColor = Color.FromArgb(39, 174, 96); // Clean Emerald Green
            panel7.BorderStyle = BorderStyle.None;
            UiHelper.MakeRounded(panel7, 12);
            StyleCard(panel7, AvailableRooms, "Available Rooms");

            // Card 3: Today's Profit
            panel8.BackColor = Color.FromArgb(212, 160, 23); // Amber Gold
            panel8.BorderStyle = BorderStyle.None;
            UiHelper.MakeRounded(panel8, 12);
            StyleCard(panel8, profitToday, "Today's Profit");

            // Card 4: Total Profit
            panel9.BackColor = Color.FromArgb(142, 68, 173); // Purple/Royal Indigo
            panel9.BorderStyle = BorderStyle.None;
            UiHelper.MakeRounded(panel9, 12);
            StyleCard(panel9, totalprofit, "Total Profit");

            // Style DataGridView (All Rooms status list)
            UiHelper.StyleModernGrid(dataGridView1);

            // Title label for the grid
            label3.Text = "🏨 Room Status Directory (All Rooms)";
            label3.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(15, 32, 65);
        }

        private void StyleCard(Panel card, Label valueLabel, string subtitle)
        {
            // Align and format labels inside the card
            valueLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            valueLabel.ForeColor = Color.White;
            valueLabel.Location = new Point(105, 15);
            valueLabel.AutoSize = true;

            // Find descriptive labels and style them
            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is Label lbl && lbl != valueLabel)
                {
                    if (lbl.Text == "0" || lbl.Text == "$0.0" || lbl.Text == "$0.00" || lbl.Text == "0.00")
                    {
                        lbl.Visible = false; // Hide old duplicate labels if any
                    }
                    else
                    {
                        lbl.Text = subtitle;
                        lbl.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        lbl.ForeColor = Color.FromArgb(235, 240, 250);
                        lbl.Location = new Point(12, 90);
                        lbl.AutoSize = true;
                    }
                }
                else if (ctrl is PictureBox pic)
                {
                    pic.Location = new Point(15, 20);
                    pic.Size = new Size(50, 50);
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }
            displayTotalStaff();

            displayAvailableRooms();

            displayProfitToday();

            displayProfitTotal();

            displayAllRooms();

            BuildRevenueChart();
        }
        public void displayAllRooms()
        {
            roomsData rData = new roomsData();
            List<roomsData> listData = rData.roomsDataList();

            dataGridView1.DataSource = listData;
            BuildRoomStatusMap(listData);
        }
        public void displayTotalStaff()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string selectData = "SELECT COUNT(id) FROM users WHERE role = 'staff'";
                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                            totalStaff.Text = result.ToString();
                    }
                }
            }
            catch { totalStaff.Text = "0"; }
        }

        public void displayAvailableRooms()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string selectData = "SELECT COUNT(id) FROM rooms WHERE status = 'Active' OR status = 'Available'";
                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                            AvailableRooms.Text = result.ToString();
                    }
                }
            }
            catch { AvailableRooms.Text = "0"; }
        }

        public void displayProfitToday()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string selectData = "SELECT SUM(price) FROM customer WHERE date_book = @dbook";
                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@dbook", DateTime.Today);
                        object result = cmd.ExecuteScalar();
                        profitToday.Text = (result != DBNull.Value && result != null) ? "$" + result.ToString() + ".00" : "$0.00";
                    }
                }
            }
            catch { profitToday.Text = "$0.00"; }
        }

        public void displayProfitTotal()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string selectData = "SELECT SUM(price) FROM customer";
                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        object result = cmd.ExecuteScalar();
                        totalprofit.Text = (result != DBNull.Value && result != null) ? "$" + result.ToString() + ".00" : "$0.00";
                    }
                }
            }
            catch { totalprofit.Text = "$0.00"; }
        }

        // ── Dynamic Interactive Room Map with Floor Tabs ────────────────────
        private FlowLayoutPanel roomMapPanel;
        private Label roomMapLegend;
        private Panel floorTabBar;
        private List<roomsData> _allRoomsCache;
        private string _activeFloor = "All";

        private void BuildRoomStatusMap(List<roomsData> listData)
        {
            _allRoomsCache = listData;

            if (roomMapPanel == null)
            {
                dataGridView1.Width = 480;

                // ── Floor Tab Bar ─────────────────────────────────────────
                floorTabBar = new Panel();
                floorTabBar.Location = new Point(515, 10);
                floorTabBar.Size = new Size(515, 32);
                floorTabBar.BackColor = Color.Transparent;
                panel5.Controls.Add(floorTabBar);

                string[] floors = { "All", "1st", "2nd", "3rd", "4th" };
                int tabX = 0;
                foreach (string floor in floors)
                {
                    string floorCopy = floor;
                    Button tab = new Button();
                    tab.Text = floor == "All" ? "🏨 All" : floor + " Floor";
                    tab.Size = new Size(floor == "All" ? 70 : 85, 28);
                    tab.Location = new Point(tabX, 2);
                    tab.FlatStyle = FlatStyle.Flat;
                    tab.FlatAppearance.BorderSize = 0; // Flat borderless
                    tab.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    tab.Cursor = Cursors.Hand;
                    tab.Tag = floorCopy;
                    UiHelper.MakeRounded(tab, 8); // Modern rounded floor tabs
                    tab.Click += (s, ev) => {
                        _activeFloor = floorCopy;
                        RefreshFloorMap();
                        foreach (Control c in floorTabBar.Controls)
                        {
                            if (c is Button b)
                            {
                                bool isActive = b.Tag?.ToString() == _activeFloor;
                                b.BackColor = isActive ? Color.FromArgb(15, 32, 65) : Color.FromArgb(220, 228, 245);
                                b.ForeColor = isActive ? Color.White : Color.FromArgb(40, 60, 100);
                            }
                        }
                    };
                    bool isDefault = floor == "All";
                    tab.BackColor = isDefault ? Color.FromArgb(15, 32, 65) : Color.FromArgb(220, 228, 245);
                    tab.ForeColor = isDefault ? Color.White : Color.FromArgb(40, 60, 100);
                    floorTabBar.Controls.Add(tab);
                    tabX += tab.Width + 4;
                }

                // ── Room Map Panel ────────────────────────────────────────
                roomMapPanel = new FlowLayoutPanel();
                roomMapPanel.Location = new Point(515, 48);
                roomMapPanel.Size = new Size(515, 310);
                roomMapPanel.AutoScroll = true;
                roomMapPanel.BackColor = Color.FromArgb(240, 244, 255);
                roomMapPanel.Padding = new Padding(5);
                UiHelper.MakeRounded(roomMapPanel, 12);
                panel5.Controls.Add(roomMapPanel);

                // ── Legend ────────────────────────────────────────────────
                roomMapLegend = new Label();
                roomMapLegend.Text = "🟢 Diyaar/Clean   🔴 Deggan   🟡 Wasakh/Dayactir";
                roomMapLegend.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                roomMapLegend.ForeColor = Color.FromArgb(100, 100, 100);
                roomMapLegend.Location = new Point(515, 363);
                roomMapLegend.AutoSize = true;
                panel5.Controls.Add(roomMapLegend);
            }

            RefreshFloorMap();
        }

        private void RefreshFloorMap()
        {
            roomMapPanel.Controls.Clear();
            if (_allRoomsCache == null) return;

            var filtered = _allRoomsCache.Where(r =>
            {
                if (_activeFloor == "All") return true;
                string floorPrefix = _activeFloor == "1st" ? "1" :
                                     _activeFloor == "2nd" ? "2" :
                                     _activeFloor == "3rd" ? "3" : "4";
                return r.RoomID.StartsWith(floorPrefix);
            }).ToList();

            foreach (var room in filtered)
            {
                bool isAvail = room.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                               room.Status.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                               room.Status.Equals("Clean / Ready", StringComparison.OrdinalIgnoreCase);
                
                bool isMaint = room.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase) ||
                               room.Status.Equals("Dirty / Cleaning", StringComparison.OrdinalIgnoreCase);

                Button tile = new Button();
                tile.Size = new Size(92, 56);
                tile.FlatStyle = FlatStyle.Flat;
                tile.FlatAppearance.BorderSize = 0;
                tile.Cursor = Cursors.Hand;
                tile.Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold);
                tile.ForeColor = Color.White;
                tile.Text = $"#{room.RoomID}\n{(room.RoomName.Length > 10 ? room.RoomName.Substring(0, 10) + "…" : room.RoomName)}";
                tile.TextAlign = ContentAlignment.MiddleCenter;

                tile.BackColor = isAvail ? Color.FromArgb(39, 174, 96)
                               : isMaint  ? Color.FromArgb(243, 156, 18)
                               :            Color.FromArgb(231, 76, 60);

                UiHelper.MakeRounded(tile, 10);

                var roomCopy = room;
                tile.Click += (s, ev) => {
                    string statusIcon = isAvail ? "🟢" : isMaint ? "🟡" : "🔴";
                    MessageBox.Show(
                        $"🏨 Xogta Qolka:\n\n" +
                        $"• Lambarka: {roomCopy.RoomID}\n" +
                        $"• Magaca: {roomCopy.RoomName}\n" +
                        $"• Nooca: {roomCopy.RoomType}\n" +
                        $"• Qiimaha: ${roomCopy.Price}/habeenba\n" +
                        $"• Xaaladda: {statusIcon} {roomCopy.Status}",
                        $"Qolka {roomCopy.RoomID}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                roomMapPanel.Controls.Add(tile);
            }

            if (filtered.Count == 0)
            {
                var noRooms = new Label();
                noRooms.Text = "⚠️ Qol lama helin dabaqan";
                noRooms.Font = new Font("Segoe UI", 10, FontStyle.Italic);
                noRooms.ForeColor = Color.Gray;
                noRooms.AutoSize = true;
                noRooms.Margin = new Padding(20);
                roomMapPanel.Controls.Add(noRooms);
            }
        }

        // ── GDI+ Revenue Bar Chart ───────────────────────────────────────────
        private PictureBox chartBox;

        private void BuildRevenueChart()
        {
            try
            {
                // Fetch last 7 days revenue from DB
                var days = new List<string>();
                var revenues = new List<decimal>();

                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    for (int i = 6; i >= 0; i--)
                    {
                        DateTime day = DateTime.Today.AddDays(-i);
                        var cmd = new SqlCommand("SELECT ISNULL(SUM(price),0) FROM customer WHERE date_book=@d", connect);
                        cmd.Parameters.AddWithValue("@d", day);
                        object result = cmd.ExecuteScalar();
                        revenues.Add(result != DBNull.Value ? Convert.ToDecimal(result) : 0);
                        days.Add(day.ToString("ddd\ndd/MM"));
                    }
                }

                // Create or reuse PictureBox
                if (chartBox == null)
                {
                    chartBox = new PictureBox();
                    chartBox.Location = new Point(20, 380);
                    chartBox.Size = new Size(1060, 170);
                    chartBox.BackColor = Color.White;
                    chartBox.BorderStyle = BorderStyle.None;
                    panel5.Controls.Add(chartBox);

                    var chartTitle = new Label();
                    chartTitle.Text = "📊  Dakhliga 7 Maalmood ee Dambe (Revenue Trend)";
                    chartTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                    chartTitle.ForeColor = Color.FromArgb(15, 32, 65);
                    chartTitle.Location = new Point(20, 358);
                    chartTitle.AutoSize = true;
                    panel5.Controls.Add(chartTitle);
                }

                // Draw chart using GDI+
                Bitmap bmp = new Bitmap(chartBox.Width, chartBox.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.White);

                    int padLeft = 60, padBottom = 38, padTop = 15, padRight = 20;
                    int chartW = bmp.Width - padLeft - padRight;
                    int chartH = bmp.Height - padBottom - padTop;

                    decimal maxRev = revenues.Max() == 0 ? 1 : revenues.Max();
                    int barW = (chartW / days.Count) - 10;
                    int barSpacing = chartW / days.Count;

                    // Y axis grid lines
                    using (Pen gridPen = new Pen(Color.FromArgb(230, 234, 245), 1))
                    {
                        for (int yi = 0; yi <= 4; yi++)
                        {
                            int yPos = padTop + (int)(chartH * yi / 4);
                            g.DrawLine(gridPen, padLeft, yPos, bmp.Width - padRight, yPos);
                            decimal yVal = maxRev * (4 - yi) / 4;
                            g.DrawString($"${yVal:F0}", new Font("Segoe UI", 7), Brushes.Gray, 2, yPos - 8);
                        }
                    }

                    // Bars
                    Color barColor = Color.FromArgb(26, 47, 80);
                    Color barHover = Color.FromArgb(212, 160, 23);
                    for (int i = 0; i < days.Count; i++)
                    {
                        int barH = revenues[i] == 0 ? 4 : (int)(chartH * revenues[i] / maxRev);
                        int x = padLeft + i * barSpacing + (barSpacing - barW) / 2;
                        int y = padTop + chartH - barH;

                        // Bar gradient
                        using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Rectangle(x, y, barW, barH),
                            Color.FromArgb(52, 100, 180),
                            Color.FromArgb(26, 47, 80),
                            System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(brush, x, y, barW, barH);
                        }

                        // Value on top
                        if (revenues[i] > 0)
                        {
                            string valStr = $"${revenues[i]:F0}";
                            SizeF sz = g.MeasureString(valStr, new Font("Segoe UI", 7, FontStyle.Bold));
                            g.DrawString(valStr, new Font("Segoe UI", 7, FontStyle.Bold), Brushes.DimGray,
                                x + barW / 2 - sz.Width / 2, y - sz.Height - 2);
                        }

                        // Day label
                        SizeF labelSz = g.MeasureString(days[i], new Font("Segoe UI", 7));
                        g.DrawString(days[i], new Font("Segoe UI", 7), Brushes.DimGray,
                            x + barW / 2 - labelSz.Width / 2, padTop + chartH + 4);
                    }

                    // X axis line
                    g.DrawLine(new Pen(Color.FromArgb(15, 32, 65), 2), padLeft, padTop + chartH, bmp.Width - padRight, padTop + chartH);
                    // Y axis line
                    g.DrawLine(new Pen(Color.FromArgb(15, 32, 65), 2), padLeft, padTop, padLeft, padTop + chartH);
                }

                chartBox.Image?.Dispose();
                chartBox.Image = bmp;
            }
            catch { /* Silent if DB is not ready */ }
        }
    }
}
