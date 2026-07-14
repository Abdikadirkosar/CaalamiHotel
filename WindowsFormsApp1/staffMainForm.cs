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
    public partial class staffMainForm : Form
    {
        private string conn = hotelData.ConnectionString;

        public staffMainForm()
        {
            InitializeComponent();
            ApplyStaffTheme();
            this.Shown += StaffMainForm_Shown;
        }

        private void StaffMainForm_Shown(object sender, EventArgs e)
        {
            CheckOverdueGuests();
        }

        // ── Professional Navy + Gold Theme for Staff ─────────────────────
        private void ApplyStaffTheme()
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
            StyleSideButton(addUser_btn,    "🛎️  Book Room",  sideBtn, sideHover);
            StyleSideButton(customers_btn,  "👥  Guests",     sideBtn, sideHover);
            
            StyleSideButton(logout_btn,     "🚪  Logout",
                Color.FromArgb(180, 40, 40), Color.FromArgb(220, 60, 60));

            label2.ForeColor = Color.FromArgb(212, 160, 23);
            label2.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            label2.Text = "Soo dhowow, " + AppSession.CurrentUser;

            label1.ForeColor = Color.White;
            label1.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            label1.Text = "CAALAMI HOTEL";
            this.Text = "🏨 CAALAMI HOTEL — Staff Panel";
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

        private void logout_btn_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to Logout?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AuditLogger.Log("Logout", $"Staff '{AppSession.CurrentUser}' logged out");
                Form1 loginForm = new Form1();
                loginForm.Show();
                this.Hide();
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            admin_dashboard1.Visible = true;
            staff_bookRoom1.Visible = false;
            admin_customers1.Visible = false;

            admin_dashboard adDashboard = admin_dashboard1 as admin_dashboard;
            if(adDashboard != null)
            {
                adDashboard.refreshData();
            }
        }

        private void addUser_btn_Click(object sender, EventArgs e)
        {
            admin_dashboard1.Visible = false;
            staff_bookRoom1.Visible = true;
            admin_customers1.Visible = false;

            staff_bookRoom adBookRoom = staff_bookRoom1 as staff_bookRoom;
            if (adBookRoom != null)
            {
                adBookRoom.refreshData();
            }
        }

        private void customers_btn_Click(object sender, EventArgs e)
        {
            admin_dashboard1.Visible = false;
            staff_bookRoom1.Visible = false;
            admin_customers1.Visible = true;

            admin_customers adCustomers = admin_customers1 as admin_customers;
            if (adCustomers != null)
            {
                adCustomers.refreshData();
            }
        }
    }
}
