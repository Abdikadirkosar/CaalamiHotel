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
    public partial class RegistrationForm : Form
    {
        private string conn = hotelData.ConnectionString;
        public RegistrationForm()
        {
            InitializeComponent();
            LoadHotelLogo();
            ApplyModernStyles();
        }

        private void ApplyModernStyles()
        {
            panel1.BackColor = Color.FromArgb(15, 32, 65);
            this.BackColor = Color.FromArgb(240, 244, 255);

            UiHelper.StyleFlatButton(register_btn, Color.FromArgb(212, 160, 23), Color.FromArgb(180, 130, 10), 12);
            register_btn.ForeColor = Color.White;

            UiHelper.StyleFlatButton(register_signinBtn, Color.FromArgb(30, 60, 110), Color.FromArgb(40, 80, 140), 12);
            register_signinBtn.ForeColor = Color.FromArgb(212, 160, 23);

            UiHelper.MakeRounded(register_username, 6);
            UiHelper.MakeRounded(register_password, 6);
            UiHelper.MakeRounded(register_cpassword, 6);
        }

        private void LoadHotelLogo()
        {
            try
            {
                pictureBox1.Image = Properties.Resources.hotel_logo;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch { /* fallback */ }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void register_signinBtn_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();

            this.Hide();
        }

        private void register_showPass_CheckedChanged(object sender, EventArgs e)
        {
            register_password.PasswordChar = register_showPass.Checked ? '\0' : '*';
            register_cpassword.PasswordChar = register_showPass.Checked ? '\0' : '*';
        }

        private void register_btn_Click(object sender, EventArgs e)
        {
            if (register_username.Text == "" || register_password.Text == "" || register_cpassword.Text == "")
            {
                MessageBox.Show("Please fill all the blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();

                    string checkUsern = "SELECT username FROM users WHERE LOWER(username) = LOWER(@usern)";

                    using (SqlCommand checkU = new SqlCommand(checkUsern, connect))
                    {
                        checkU.Parameters.AddWithValue("@usern", register_username.Text.Trim());

                        SqlDataAdapter adapter = new SqlDataAdapter(checkU);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count != 0)
                        {
                            string tempEmail = register_username.Text.Substring(0, 1).ToUpper() + register_username.Text.Substring(1);
                            MessageBox.Show($"{tempEmail} is existing already", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (register_password.Text.Length < 8)
                        {
                            MessageBox.Show("Password must be at least 8 characters", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (register_password.Text != register_cpassword.Text)
                        {
                            MessageBox.Show("Passwords do not match", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            string query = "INSERT INTO users (username, password, role, status, date_register) " +
                                "VALUES(@usern, @pass, @role, @status, @regDate)";

                            using (SqlCommand cmd = new SqlCommand(query, connect))
                            {
                                cmd.Parameters.AddWithValue("@usern", register_username.Text.Trim());
                                cmd.Parameters.AddWithValue("@pass", register_password.Text.Trim());
                                cmd.Parameters.AddWithValue("@role", "staff");
                                cmd.Parameters.AddWithValue("@status", "Active");

                                DateTime today = DateTime.Today;
                                cmd.Parameters.AddWithValue("@regDate", today);

                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Registered successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                Form1 loginForm = new Form1();
                                loginForm.Show();
                                this.Hide();
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    "Database error!\n\nMake sure the database is set up correctly.\n\nError: " + sqlEx.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
