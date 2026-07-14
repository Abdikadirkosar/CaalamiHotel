using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private string conn = hotelData.ConnectionString;

        public Form1()
        {
            InitializeComponent();
            ApplyColorTheme();
        }

        // ── Professional Blue & Gold Color Theme ──────────────────────────
        private void ApplyColorTheme()
        {
            // Update Title
            this.Text = "🏨 CAALAMI HOTEL — Nidaamka Maamulka";

            // Left panel: deep navy-blue
            panel1.BackColor = Color.FromArgb(15, 32, 65);

            // Right side (form background): light steel
            this.BackColor = Color.FromArgb(240, 244, 255);

            // Modernize Login Form Frame and Login Button
            UiHelper.StyleFlatButton(login_btn, Color.FromArgb(212, 160, 23), Color.FromArgb(180, 130, 10), 12);
            login_btn.ForeColor = Color.White;

            // Register button
            UiHelper.StyleFlatButton(login_registerbtn, Color.FromArgb(30, 60, 110), Color.FromArgb(40, 80, 140), 12);
            login_registerbtn.ForeColor = Color.FromArgb(212, 160, 23);

            // Make text inputs slightly rounded for premium modern look
            UiHelper.MakeRounded(login_username, 6);
            UiHelper.MakeRounded(login_password, 6);

            // Labels on the right side
            label2.Text = "CAALAMI HOTEL";
            label2.ForeColor = Color.FromArgb(15, 32, 65);   // Title
            label3.Text = "Magaca Isticmaalaha (Username)";
            label3.ForeColor = Color.FromArgb(70, 90, 130);  // "Username"
            label4.Text = "Fungriga (Password)";
            label4.ForeColor = Color.FromArgb(70, 90, 130);  // "Password"
            login_showPass.Text = "Muuji erayga sirta ah";
            login_showPass.ForeColor = Color.FromArgb(70, 90, 130);

            // Labels on left panel
            label5.Text = "Caalami Hotel";
            label5.ForeColor = Color.White;
            label6.Text = "Diiwaan geli shaqaale cusub";
            label6.ForeColor = Color.FromArgb(212, 160, 23);

            // Close button
            close.ForeColor = Color.FromArgb(200, 80, 80);
            close.BackColor = Color.Transparent;

            // Embedded Logo Loading
            try
            {
                pictureBox1.Image = Properties.Resources.hotel_logo;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch { /* fallback */ }
        }

        private void login_registerbtn_Click(object sender, EventArgs e)
        {
            try
            {
                RegistrationForm regForm = new RegistrationForm();
                regForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening registration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void login_btn_Click(object sender, EventArgs e)
        {
            if (login_username.Text == "" || login_password.Text == "")
            {
                MessageBox.Show("Please fill all blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();

                    string selectData = "SELECT * FROM users WHERE (LOWER(username) = LOWER(@usern) AND password = @pass) AND LOWER(status) = 'active'";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@usern", login_username.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", login_password.Text.Trim());

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count != 0)
                        {
                            string selectRole = "SELECT role FROM users WHERE LOWER(username) = LOWER(@usern) AND password = @pass";

                            using (SqlCommand getRole = new SqlCommand(selectRole, connect))
                            {
                                getRole.Parameters.AddWithValue("@usern", login_username.Text.Trim());
                                getRole.Parameters.AddWithValue("@pass", login_password.Text.Trim());

                                string userRole = getRole.ExecuteScalar() as string;

                                // Set session — silent redirect like professional systems (no popup)
                                AppSession.CurrentUser = login_username.Text.Trim();
                                AppSession.CurrentRole  = userRole ?? "";
                                AuditLogger.Log("Login", $"User '{AppSession.CurrentUser}' logged in as {userRole}");

                                if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                                {
                                    AdminMainForm adminForm = new AdminMainForm();
                                    adminForm.Show();
                                    this.Hide();
                                }
                                else if (string.Equals(userRole, "staff", StringComparison.OrdinalIgnoreCase))
                                {
                                    staffMainForm staffForm = new staffMainForm();
                                    staffForm.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Unknown role: " + userRole, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Incorrect username or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    "Database connection failed!\n\n" +
                    "Please make sure:\n" +
                    "1. SQL Server LocalDB is installed\n" +
                    "2. You ran the Setup_Database.sql script in SSMS\n\n" +
                    "Error: " + sqlEx.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void login_showPass_CheckedChanged(object sender, EventArgs e)
        {
            login_password.PasswordChar = login_showPass.Checked ? '\0' : '*';
        }

        private void close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
