using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class AdminMainForm : Form
    {
        private string conn = hotelData.ConnectionString;

        // ── Audit Log panel (created dynamically) ────────────────────────
        private Panel auditLogPanel;
        private DataGridView auditGrid;
        private Button auditLog_btn;
        private Button reports_btn;
        private Button backup_btn;

        // ── Book Room Control for Admin Override ──────────────────────────
        private staff_bookRoom adminBookRoomControl;
        private Button bookRoom_btn;

        public AdminMainForm()
        {
            InitializeComponent();
            ApplyAdminTheme();
            BuildBookRoomPanel();
            BuildReportsPanel();
            BuildAuditLogPanel();
            BuildBackupButton();
            label1.Text = "CAALAMI HOTEL";
            this.Text = "🏨 CAALAMI HOTEL — Admin Dashboard";
            label2.Text = "Soo dhowow, " + AppSession.CurrentUser;
            RepositionSidebarButtons();
            this.Shown += AdminMainForm_Shown;
        }

        private void AdminMainForm_Shown(object sender, EventArgs e)
        {
            CheckOverdueGuests();
        }

        // ── Professional Navy + Gold Theme ────────────────────────────────
        private void ApplyAdminTheme()
        {
            // Top bar
            panel1.BackColor = Color.FromArgb(10, 25, 55);

            // Sidebar
            panel2.BackColor = Color.FromArgb(15, 32, 65);

            // Content area
            panel3.BackColor = Color.FromArgb(240, 244, 255);

            // Style all sidebar buttons
            Color sideBtn  = Color.FromArgb(25, 50, 100);
            Color sideHover= Color.FromArgb(212, 160, 23);

            StyleSideButton(dashboard_btn,  "📊  Dashboard",  sideBtn, sideHover);
            StyleSideButton(addUser_btn,    "👤  Manage Users", sideBtn, sideHover);
            StyleSideButton(rooms_btn,      "🛏   Rooms",       sideBtn, sideHover);
            StyleSideButton(customers_btn,  "👥  Guests",       sideBtn, sideHover);
            StyleSideButton(logout_btn,     "🚪  Logout",
                Color.FromArgb(180, 40, 40), Color.FromArgb(220, 60, 60));

            label2.ForeColor = Color.FromArgb(212, 160, 23);
            label2.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            label1.Text = "🏨  Hotel Management";
            close.ForeColor = Color.FromArgb(255, 100, 100);

            // Embedded Logo Loading
            try
            {
                pictureBox1.Image = Properties.Resources.hotel_logo;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch { /* fallback */ }
        }

        private void StyleSideButton(Button btn, string text, Color bg, Color hover)
        {
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hover;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(8, 0, 0, 0);
        }

        // ── Audit Log Panel (built programmatically) ──────────────────────
        private void BuildAuditLogPanel()
        {
            // Sidebar button
            auditLog_btn = new Button();
            auditLog_btn.Location = new Point(15, 410);
            auditLog_btn.Size = new Size(169, 32);
            StyleSideButton(auditLog_btn, "📋  Audit Log", Color.FromArgb(25, 50, 100), Color.FromArgb(212, 160, 23));
            auditLog_btn.Click += AuditLog_btn_Click;
            panel2.Controls.Add(auditLog_btn);

            // Content panel
            auditLogPanel = new Panel();
            auditLogPanel.Dock = DockStyle.Fill;
            auditLogPanel.BackColor = Color.FromArgb(240, 244, 255);
            auditLogPanel.Visible = false;

            var titleLbl = new Label();
            titleLbl.Text = "📋  Audit Log — System Activity";
            titleLbl.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(15, 32, 65);
            titleLbl.Location = new Point(20, 15);
            titleLbl.AutoSize = true;

            var refreshBtn = new Button();
            refreshBtn.Text = "🔄 Refresh";
            refreshBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            refreshBtn.BackColor = Color.FromArgb(15, 32, 65);
            refreshBtn.ForeColor = Color.White;
            refreshBtn.FlatStyle = FlatStyle.Flat;
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.Size = new Size(110, 32);
            refreshBtn.Location = new Point(900, 12);
            refreshBtn.Cursor = Cursors.Hand;
            refreshBtn.Click += (s, e) => LoadAuditLog();

            auditGrid = new DataGridView();
            auditGrid.Location = new Point(20, 55);
            auditGrid.Size = new Size(1040, 530);
            auditGrid.BackgroundColor = Color.White;
            auditGrid.BorderStyle = BorderStyle.None;
            auditGrid.RowHeadersVisible = false;
            auditGrid.AllowUserToAddRows = false;
            auditGrid.ReadOnly = true;
            auditGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            auditGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            auditGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 32, 65);
            auditGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            auditGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            auditGrid.EnableHeadersVisualStyles = false;

            auditLogPanel.Controls.Add(titleLbl);
            auditLogPanel.Controls.Add(refreshBtn);
            auditLogPanel.Controls.Add(auditGrid);

            panel3.Controls.Add(auditLogPanel);
        }

        // ── Reports Panel (basic summary) ─────────────────────────────────
        private Panel reportsPanel;
        private DataGridView reportsGrid;
        private Label reportsRevLabel;

        private void BuildReportsPanel()
        {
            reports_btn = new Button();
            reports_btn.Location = new Point(15, 354 + 56);
            reports_btn.Size = new Size(169, 32);
            StyleSideButton(reports_btn, "📈  Reports", Color.FromArgb(25, 50, 100), Color.FromArgb(212, 160, 23));
            reports_btn.Click += Reports_btn_Click;
            panel2.Controls.Add(reports_btn);

            reportsPanel = new Panel();
            reportsPanel.Dock = DockStyle.Fill;
            reportsPanel.BackColor = Color.FromArgb(240, 244, 255);
            reportsPanel.Visible = false;

            var titleLbl = new Label();
            titleLbl.Text = "📈  Revenue & Occupancy Reports";
            titleLbl.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(15, 32, 65);
            titleLbl.Location = new Point(20, 15);
            titleLbl.AutoSize = true;

            reportsRevLabel = new Label();
            reportsRevLabel.Font = new Font("Segoe UI", 11);
            reportsRevLabel.ForeColor = Color.FromArgb(40, 80, 140);
            reportsRevLabel.Location = new Point(20, 50);
            reportsRevLabel.Size = new Size(700, 30);

            var refreshBtn2 = new Button();
            refreshBtn2.Text = "🔄 Refresh";
            refreshBtn2.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            refreshBtn2.BackColor = Color.FromArgb(15, 32, 65);
            refreshBtn2.ForeColor = Color.White;
            refreshBtn2.FlatStyle = FlatStyle.Flat;
            refreshBtn2.FlatAppearance.BorderSize = 0;
            refreshBtn2.Size = new Size(110, 32);
            refreshBtn2.Location = new Point(900, 12);
            refreshBtn2.Cursor = Cursors.Hand;
            refreshBtn2.Click += (s, e) => LoadReports();

            reportsGrid = new DataGridView();
            reportsGrid.Location = new Point(20, 90);
            reportsGrid.Size = new Size(1040, 495);
            reportsGrid.BackgroundColor = Color.White;
            reportsGrid.BorderStyle = BorderStyle.None;
            reportsGrid.RowHeadersVisible = false;
            reportsGrid.AllowUserToAddRows = false;
            reportsGrid.ReadOnly = true;
            reportsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            reportsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            reportsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 32, 65);
            reportsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            reportsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            reportsGrid.EnableHeadersVisualStyles = false;

            reportsPanel.Controls.Add(titleLbl);
            reportsPanel.Controls.Add(reportsRevLabel);
            reportsPanel.Controls.Add(refreshBtn2);
            reportsPanel.Controls.Add(reportsGrid);

            panel3.Controls.Add(reportsPanel);
        }

        // ── Load Audit Log data ────────────────────────────────────────────
        private void LoadAuditLog()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string sql = "SELECT TOP 500 id AS '#', username AS 'User', action AS 'Action', " +
                                 "details AS 'Details', action_date AS 'Date & Time' " +
                                 "FROM audit_log ORDER BY action_date DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sql, connect);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    auditGrid.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load audit log: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Load Reports data ──────────────────────────────────────────────
        private void LoadReports()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();

                    var cmd2 = new SqlCommand("SELECT ISNULL(SUM(price),0) FROM customer WHERE date_book=@d", connect);
                    cmd2.Parameters.AddWithValue("@d", DateTime.Today);
                    object todayRevVal = cmd2.ExecuteScalar();

                    var cmd3 = new SqlCommand("SELECT ISNULL(SUM(price),0) FROM customer", connect);
                    object totalRev = cmd3.ExecuteScalar();

                    reportsRevLabel.Text = $"Today's Revenue: ${todayRevVal}   |   Total Revenue: ${totalRev}   |   Report Date: {DateTime.Now:dd/MM/yyyy HH:mm}";

                    // Booking history per room
                    string sql = "SELECT c.room_id AS 'Room ID', " +
                                 "COUNT(c.id) AS 'Bookings', " +
                                 "SUM(c.price) AS 'Total Revenue ($)', " +
                                 "SUM(CASE WHEN c.status='Checked In' THEN 1 ELSE 0 END) AS 'Active Stays', " +
                                 "SUM(CASE WHEN c.status='Checked Out' THEN 1 ELSE 0 END) AS 'Completed Stays' " +
                                 "FROM customer c GROUP BY c.room_id ORDER BY SUM(c.price) DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sql, connect);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    reportsGrid.DataSource = dt;

                    // Color top earners green
                    foreach (DataGridViewRow row in reportsGrid.Rows)
                    {
                        if (row.Index == 0)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(198, 239, 206);
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load reports: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Overdue guest check on startup ────────────────────────────────
        private void CheckOverdueGuests()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string sql = "SELECT COUNT(*) FROM customer WHERE status='Checked In' AND date_to < @today";
                    using (SqlCommand cmd = new SqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@today", DateTime.Today);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            customers_btn.Text = $"👥  Guests  ⚠️ ({count})";
                            customers_btn.ForeColor = Color.FromArgb(255, 220, 100);
                            MessageBox.Show(
                                $"⚠️  {count} guest(s) have OVERDUE check-out dates!\n\nPlease go to Guests to process their check-out.",
                                "Overdue Guests Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch { /* Silent if DB not ready */ }
        }

        // ── Navigation ────────────────────────────────────────────────────
        private void HideAll()
        {
            admin_dashboard1.Visible  = false;
            admin_addUser1.Visible    = false;
            admin_rooms1.Visible      = false;
            admin_customers1.Visible  = false;
            auditLogPanel.Visible     = false;
            reportsPanel.Visible      = false;
            if (adminBookRoomControl != null) adminBookRoomControl.Visible = false;
        }

        private void close_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirmation Message",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirmation Message",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AuditLogger.Log("Logout", $"User '{AppSession.CurrentUser}' logged out");
                Form1 loginForm = new Form1();
                loginForm.Show();
                this.Hide();
            }
        }

        private void admin_rooms1_Load(object sender, EventArgs e)
        {
            HideAll();
            admin_rooms1.Visible = true;
            var adRooms = admin_rooms1 as admin_rooms;
            if (adRooms != null) adRooms.refreshData();
        }

        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            admin_dashboard1.Visible = true;
            var adDashboard = admin_dashboard1 as admin_dashboard;
            if (adDashboard != null) adDashboard.refreshData();
        }

        private void addUser_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            admin_addUser1.Visible = true;
            var adUser = admin_addUser1 as admin_addUser;
            if (adUser != null) adUser.refreshData();
        }

        private void rooms_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            admin_rooms1.Visible = true;
            var adRooms = admin_rooms1 as admin_rooms;
            if (adRooms != null) adRooms.refreshData();
        }

        private void customers_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            admin_customers1.Visible = true;
            var adCustomer = admin_customers1 as admin_customers;
            if (adCustomer != null) adCustomer.refreshData();
        }

        private void AuditLog_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            auditLogPanel.Visible = true;
            LoadAuditLog();
        }

        private void Reports_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            reportsPanel.Visible = true;
            LoadReports();
        }

        private void BuildBackupButton()
        {
            backup_btn = new Button();
            backup_btn.Size = new Size(169, 32);
            StyleSideButton(backup_btn, "💾  Backup Data", Color.FromArgb(25, 50, 100), Color.FromArgb(212, 160, 23));
            backup_btn.Click += Backup_btn_Click;
            panel2.Controls.Add(backup_btn);
        }

        private void Backup_btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to export CSV backups of all database tables (Customers, Rooms, Users) to your Desktop?", 
                                "Export Database Backups", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string custCSV = BackupUtility.ExportTableToCSV("customer");
                string roomCSV = BackupUtility.ExportTableToCSV("rooms");
                string userCSV = BackupUtility.ExportTableToCSV("users");

                MessageBox.Show($"✅ Database Backups exported successfully to Desktop:\n\n• {System.IO.Path.GetFileName(custCSV)}\n• {System.IO.Path.GetFileName(roomCSV)}\n• {System.IO.Path.GetFileName(userCSV)}", 
                                "Backup Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Book Room for Admin Override ──────────────────────────────────
        private void BuildBookRoomPanel()
        {
            bookRoom_btn = new Button();
            bookRoom_btn.Size = new Size(169, 32);
            StyleSideButton(bookRoom_btn, "🛎️  Book Room", Color.FromArgb(25, 50, 100), Color.FromArgb(212, 160, 23));
            bookRoom_btn.Click += BookRoom_btn_Click;
            panel2.Controls.Add(bookRoom_btn);

            adminBookRoomControl = new staff_bookRoom();
            adminBookRoomControl.Dock = DockStyle.Fill;
            adminBookRoomControl.Visible = false;
            panel3.Controls.Add(adminBookRoomControl);
        }

        private void BookRoom_btn_Click(object sender, EventArgs e)
        {
            HideAll();
            adminBookRoomControl.Visible = true;
            adminBookRoomControl.refreshData();
        }

        private void RepositionSidebarButtons()
        {
            dashboard_btn.Location = new Point(15, 175);
            addUser_btn.Location   = new Point(15, 220);
            rooms_btn.Location     = new Point(15, 265);
            customers_btn.Location = new Point(15, 310);
            if (bookRoom_btn != null)  bookRoom_btn.Location  = new Point(15, 355);
            if (reports_btn != null)   reports_btn.Location   = new Point(15, 400);
            if (auditLog_btn != null)  auditLog_btn.Location  = new Point(15, 445);
            if (backup_btn != null)    backup_btn.Location    = new Point(15, 490);
            logout_btn.Location    = new Point(15, 545);
        }
    }
}
