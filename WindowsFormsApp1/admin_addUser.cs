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
    public partial class admin_addUser : UserControl
    {
        private string conn = hotelData.ConnectionString;
        private List<usersData> _allUsers = new List<usersData>();
        private TextBox userSearchBox;

        public admin_addUser()
        {
            InitializeComponent();
            SetupUserSearch();
            displayData();
            ApplyModernStyles();
        }

        private void ApplyModernStyles()
        {
            UiHelper.StyleModernGrid(dataGridView1);
            UiHelper.StyleFlatButton(addUser_addBtn, Color.FromArgb(212, 160, 23), Color.FromArgb(180, 130, 10), 8);
            addUser_addBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(addUser_updateBtn, Color.FromArgb(15, 32, 65), Color.FromArgb(30, 50, 90), 8);
            addUser_updateBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(addUser_clearBtn, Color.FromArgb(120, 120, 120), Color.FromArgb(100, 100, 100), 8);
            addUser_clearBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(addUser_deleteBtn, Color.FromArgb(180, 40, 40), Color.FromArgb(150, 30, 30), 8);
            addUser_deleteBtn.ForeColor = Color.White;

            UiHelper.MakeRounded(panel1, 12);
            UiHelper.MakeRounded(panel2, 12);
        }

        private void SetupUserSearch()
        {
            var lbl = new Label();
            lbl.Text = "🔍 Search:";
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(15, 32, 65);
            lbl.AutoSize = true;
            lbl.Location = new Point(dataGridView1.Left, dataGridView1.Top - 32);

            userSearchBox = new TextBox();
            userSearchBox.Font = new Font("Segoe UI", 10);
            userSearchBox.Location = new Point(dataGridView1.Left + 80, dataGridView1.Top - 36);
            userSearchBox.Width = dataGridView1.Width - 90;
            userSearchBox.TextChanged += UserSearch_TextChanged;
            SetWatermark(userSearchBox, "Search by username or role...");

            panel2.Controls.Add(lbl);
            panel2.Controls.Add(userSearchBox);
        }

        private void UserSearch_TextChanged(object sender, EventArgs e)
        {
            string q = userSearchBox.Text.ToLower().Trim();
            dataGridView1.DataSource = string.IsNullOrEmpty(q)
                ? _allUsers
                : _allUsers.Where(u =>
                    u.Username.ToLower().Contains(q) ||
                    u.Role.ToLower().Contains(q) ||
                    u.Status.ToLower().Contains(q)
                  ).ToList();
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }
            displayData();
        }

        public void displayData()
        {
            try
            {
                usersData uData = new usersData();
                _allUsers = uData.listUsersData();
                dataGridView1.DataSource = _allUsers;
                if (userSearchBox != null) userSearchBox.Clear();
            }
            catch { dataGridView1.DataSource = null; }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void addUser_addBtn_Click(object sender, EventArgs e)
        {
            string rawUser = addUser_username.Text.Trim();
            if (rawUser == "" || addUser_password.Text == ""
                || addUser_role.SelectedIndex == -1 || addUser_status.SelectedIndex == -1)
            {
                MessageBox.Show("Fadlan buuxi dhammaan meelaha bannaan!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string checkUsern = "SELECT username FROM users WHERE username = @usern";
                    using (SqlCommand checkU = new SqlCommand(checkUsern, connect))
                    {
                        checkU.Parameters.AddWithValue("@usern", rawUser);
                        SqlDataAdapter adapter = new SqlDataAdapter(checkU);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count > 0)
                        {
                            string tempUsern = rawUser.Length > 1 
                                ? char.ToUpper(rawUser[0]) + rawUser.Substring(1) 
                                : rawUser.ToUpper();
                            MessageBox.Show($"Isticmaalaha '{tempUsern}' mar hore ayuu jiraa!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (addUser_password.Text.Length < 8)
                        {
                            MessageBox.Show("Furaha sirta ah (Password) waa inuu ka koobnaadaa ugu yaraan 8 xaraf!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            string insertData = "INSERT INTO users (username, password, role, status, date_register) VALUES(@usern, @pass, @role, @status, @date)";
                            using (SqlCommand cmd = new SqlCommand(insertData, connect))
                            {
                                cmd.Parameters.AddWithValue("@usern",  rawUser);
                                cmd.Parameters.AddWithValue("@pass",   addUser_password.Text.Trim());
                                cmd.Parameters.AddWithValue("@role",   addUser_role.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@status", addUser_status.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@date",   DateTime.Today);
                                cmd.ExecuteNonQuery();

                                AuditLogger.Log("Diiwaan-gelin Shaqaale", $"Wuxuu diiwaan-geliyey '{rawUser}' oo ah {addUser_role.SelectedItem}");
                                MessageBox.Show("Shaqaalaha waa la diiwaan-geliyey si guul leh!", "Guul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                clearFields();
                                displayData();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cillad ayaa dhacday: " + ex.Message, "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void clearFields()
        {
            addUser_username.Text = "";
            addUser_password.Text = "";
            addUser_role.SelectedIndex = -1;
            addUser_status.SelectedIndex = -1;
        }
        private void addUser_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private void addUser_updateBtn_Click(object sender, EventArgs e)
        {
            string rawUser = addUser_username.Text.Trim();
            if (rawUser == "" || addUser_password.Text == ""
                || addUser_role.SelectedIndex == -1 || addUser_status.SelectedIndex == -1)
            {
                MessageBox.Show("Fadlan buuxi dhammaan meelaha bannaan!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show($"Ma rabtaa inaad cusboonaysiiso xogta '{rawUser}'?", "Xaqiijin", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(conn))
                    {
                        connect.Open();
                        string updateData = "UPDATE users SET password=@pass, role=@role, status=@status WHERE username=@usern";
                        using (SqlCommand cmd = new SqlCommand(updateData, connect))
                        {
                            cmd.Parameters.AddWithValue("@pass",   addUser_password.Text.Trim());
                            cmd.Parameters.AddWithValue("@role",   addUser_role.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@status", addUser_status.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@usern",  rawUser);
                            cmd.ExecuteNonQuery();

                            AuditLogger.Log("Cusboonaysiin Shaqaale", $"Wuxuu cusboonaysiiyey '{rawUser}' (Role: {addUser_role.SelectedItem}, Status: {addUser_status.SelectedItem})");
                            displayData();
                            MessageBox.Show("Xogta shaqaalaha waa la cusboonaysiiyey si guul leh!", "Guul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cillad ayaa dhacday: " + ex.Message, "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int getID;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                getID = (int)row.Cells[0].Value;
                addUser_username.Text = row.Cells[1].Value.ToString();
                addUser_password.Text = row.Cells[2].Value.ToString();
                addUser_role.Text = row.Cells[3].Value.ToString();
                addUser_status.Text = row.Cells[4].Value.ToString();
            }
        }

        private void addUser_deleteBtn_Click(object sender, EventArgs e)
        {
            string rawUser = addUser_username.Text.Trim();
            if (rawUser == "")
            {
                MessageBox.Show("Fadlan dooro isticmaalaha aad rabto inaad tirtirto!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show($"Ma rabtaa inaad tirtirto isticmaalaha '{rawUser}'? Tallaabadan dib looma soo celin karo.",
                "Tirtirid Isticmaale", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(conn))
                    {
                        connect.Open();
                        string deleteData = "DELETE FROM users WHERE username = @usern";
                        using (SqlCommand cmd = new SqlCommand(deleteData, connect))
                        {
                            cmd.Parameters.AddWithValue("@usern", rawUser);
                            cmd.ExecuteNonQuery();

                            AuditLogger.Log("Tirtirid Shaqaale", $"Wuxuu tirtiray isticmaalaha '{rawUser}'");
                            displayData();
                            clearFields();
                            MessageBox.Show("Isticmaalaha waa la tirtiray si guul leh!", "Guul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cillad ayaa dhacday: " + ex.Message, "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Win32 watermark (placeholder text) helper for .NET Framework 4.8 ──
        private static void SetWatermark(TextBox tb, string text)
        {
            NativeMethods.SendMessage(tb.Handle, NativeMethods.EM_SETCUEBANNER, true, text);
        }
    }
}
