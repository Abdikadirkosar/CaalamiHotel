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
using System.IO;

namespace WindowsFormsApp1
{
    public partial class admin_rooms : UserControl
    {
        private string conn = hotelData.ConnectionString;

        private List<roomsData> _allRooms = new List<roomsData>();
        private TextBox roomSearchBox;

        public admin_rooms()
        {
            InitializeComponent();
            SetupRoomSearch();
            displayRoomsData();
            ApplyModernStyles();
            rooms_type.SelectedIndexChanged += Rooms_type_SelectedIndexChanged;
        }

        private void ApplyModernStyles()
        {
            UiHelper.StyleModernGrid(dataGridView1);

            // Populate modern statuses (Active, Occupied, Maintenance, Housekeeping)
            rooms_status.Items.Clear();
            rooms_status.Items.AddRange(new object[] {
                "Active",
                "Occupied",
                "Maintenance",
                "Dirty / Cleaning",
                "Clean / Ready"
            });

            UiHelper.StyleFlatButton(rooms_addBtn, Color.FromArgb(212, 160, 23), Color.FromArgb(180, 130, 10), 8);
            rooms_addBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(rooms_updateBtn, Color.FromArgb(15, 32, 65), Color.FromArgb(30, 50, 90), 8);
            rooms_updateBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(rooms_clearBtn, Color.FromArgb(120, 120, 120), Color.FromArgb(100, 100, 100), 8);
            rooms_clearBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(rooms_deleteBtn, Color.FromArgb(180, 40, 40), Color.FromArgb(150, 30, 30), 8);
            rooms_deleteBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(rooms_importBtn, Color.FromArgb(30, 60, 110), Color.FromArgb(40, 80, 140), 6);
            rooms_importBtn.ForeColor = Color.White;

            // Make container panels slightly rounded for modern card UI look
            UiHelper.MakeRounded(panel1, 12);
            UiHelper.MakeRounded(panel2, 12);
        }

        private void SetupRoomSearch()
        {
            // Search label
            var lbl = new Label();
            lbl.Text = "🔍 Search Rooms:";
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(15, 32, 65);
            lbl.AutoSize = true;

            // Search textbox (placed in panel2 which has the grid)
            roomSearchBox = new TextBox();
            roomSearchBox.Font = new Font("Segoe UI", 10);
            roomSearchBox.TextChanged += RoomSearchBox_TextChanged;
            SetWatermark(roomSearchBox, "Filter by Room ID, Name, Type, Status...");

            // Set positions relative to dataGridView1 in panel2
            lbl.Location = new Point(dataGridView1.Left, dataGridView1.Top - 32);
            roomSearchBox.Location = new Point(dataGridView1.Left + 120, dataGridView1.Top - 36);
            roomSearchBox.Width = dataGridView1.Width - 130;

            // Add color legend
            var legend = new Label();
            legend.Text = "  🟢 Active/Clean   🔴 Occupied/Dirty   🟡 Maintenance";
            legend.Font = new Font("Segoe UI", 8.5f);
            legend.ForeColor = Color.FromArgb(70, 90, 130);
            legend.AutoSize = true;
            legend.Location = new Point(dataGridView1.Left, dataGridView1.Bottom + 4);

            panel1.Controls.Add(lbl);
            panel1.Controls.Add(roomSearchBox);
            panel1.Controls.Add(legend);

            dataGridView1.CellFormatting += Rooms_CellFormatting;
        }

        private void RoomSearchBox_TextChanged(object sender, EventArgs e)
        {
            string q = roomSearchBox.Text.ToLower().Trim();
            dataGridView1.DataSource = string.IsNullOrEmpty(q)
                ? _allRooms
                : _allRooms.Where(r =>
                    r.RoomID.ToLower().Contains(q) ||
                    r.RoomName.ToLower().Contains(q) ||
                    r.RoomType.ToLower().Contains(q) ||
                    r.Status.ToLower().Contains(q)
                  ).ToList();
        }

        private void Rooms_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].DataBoundItem == null) return;
            var item = dataGridView1.Rows[e.RowIndex].DataBoundItem as roomsData;
            if (item == null) return;

            switch (item.Status)
            {
                case "Active":
                case "Available":
                case "Clean / Ready":
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(198, 239, 206);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(0, 100, 0);
                    break;
                case "Unavailable":
                case "Occupied":
                case "Dirty / Cleaning":
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 199, 206);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    break;
                case "Maintenance":
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 156);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(120, 80, 0);
                    break;
            }
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }
            displayRoomsData();
        }

        public void displayRoomsData()
        {
            try
            {
                roomsData rData = new roomsData();
                _allRooms = rData.roomsDataList();
                dataGridView1.DataSource = _allRooms;
                if (roomSearchBox != null) roomSearchBox.Clear();
            }
            catch { dataGridView1.DataSource = null; }
        }
        public bool isEmpty()
        {
            if (string.IsNullOrEmpty(rooms_roomID.Text) || string.IsNullOrEmpty(rooms_roomName.Text)
                || rooms_type.SelectedIndex == -1 || string.IsNullOrEmpty(rooms_price.Text)
                || rooms_status.SelectedIndex == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void rooms_addBtn_Click(object sender, EventArgs e)
        {
            if (isEmpty())
            {
                MessageBox.Show("Fadlan buuxi dhammaan meelaha bannaan!", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(conn))
                    {
                        connect.Open();

                        string checkRoomRID = "SELECT room_id FROM rooms WHERE room_id = @roomID";
                        using (SqlCommand checkRID = new SqlCommand(checkRoomRID, connect))
                        {
                            checkRID.Parameters.AddWithValue("@roomID", rooms_roomID.Text.Trim());

                            SqlDataAdapter adapter = new SqlDataAdapter(checkRID);
                            DataTable table = new DataTable();

                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                MessageBox.Show($"Lambarka qolka '{rooms_roomID.Text.Trim()}' wuu diiwaan gashan yahay mar hore!", 
                                    "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                string insertData = "INSERT INTO rooms (room_id, type, room_name, price, image_path, status, date_register)" +
                                    "VALUES(@room_ID, @type, @name, @price, @path, @status, @date_reg)";

                                string path = "";

                                if (rooms_picture.Image != null && !string.IsNullOrEmpty(rooms_picture.ImageLocation))
                                {
                                    path = Path.Combine(
                                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                                        "hotel_rooms",
                                        rooms_roomID.Text.Trim() + ".jpg");

                                    string directoryPath = Path.GetDirectoryName(path);
                                    if (!Directory.Exists(directoryPath))
                                    {
                                        Directory.CreateDirectory(directoryPath);
                                    }

                                    File.Copy(rooms_picture.ImageLocation, path, true);
                                }

                                using (SqlCommand cmd = new SqlCommand(insertData, connect))
                                {
                                    cmd.Parameters.AddWithValue("@room_ID", rooms_roomID.Text.Trim());
                                    cmd.Parameters.AddWithValue("@type", rooms_type.SelectedItem.ToString());
                                    cmd.Parameters.AddWithValue("@name", rooms_roomName.Text.Trim());
                                    cmd.Parameters.AddWithValue("@price", rooms_price.Text.Trim());
                                    cmd.Parameters.AddWithValue("@path", path); // Empty string if no image

                                    cmd.Parameters.AddWithValue("@status", rooms_status.SelectedItem.ToString());

                                    DateTime today = DateTime.Today;
                                    cmd.Parameters.AddWithValue("@date_reg", today);

                                    cmd.ExecuteNonQuery();
                                    
                                    AuditLogger.Log("Diiwaan-gelin Qol", $"Wuxuu ku daray Qolka #{rooms_roomID.Text.Trim()} ({rooms_roomName.Text.Trim()})");
                                    
                                    clearFields();
                                    displayRoomsData();

                                    MessageBox.Show("Qolka waa la diiwaan-geliyey si guul leh!", "Guul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Waxaa dhacay fashil: " + ex.Message, "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Rooms_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rooms_type.SelectedIndex == -1) return;

            string selectedType = rooms_type.SelectedItem.ToString();
            string prefix = "1";
            decimal defaultPrice = 45.00m;

            if (selectedType == "Single") { prefix = "1"; defaultPrice = 45.00m; }
            else if (selectedType == "Double") { prefix = "2"; defaultPrice = 80.00m; }
            else if (selectedType == "Deluxe") { prefix = "3"; defaultPrice = 120.00m; }
            else if (selectedType == "Suite") { prefix = "3"; defaultPrice = 180.00m; }
            else if (selectedType == "Executive Suite") { prefix = "4"; defaultPrice = 280.00m; }
            else if (selectedType == "Penthouse") { prefix = "4"; defaultPrice = 380.00m; }

            // Find next available Room ID starting with prefix
            string nextRoomId = prefix + "01";
            try
            {
                using (SqlConnection connect = new SqlConnection(conn))
                {
                    connect.Open();
                    string query = "SELECT MAX(room_id) FROM rooms WHERE room_id LIKE @pattern AND date_delete IS NULL";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@pattern", prefix + "%");
                        object max = cmd.ExecuteScalar();
                        if (max != null && max != DBNull.Value)
                        {
                            if (int.TryParse(max.ToString(), out int maxId))
                            {
                                nextRoomId = (maxId + 1).ToString();
                            }
                        }
                    }
                }
            }
            catch { /* Fallback to default */ }

            // Auto-populate fields to make it super easy!
            rooms_roomID.Text = nextRoomId;
            rooms_roomName.Text = $"Qolka {nextRoomId} ({selectedType})";
            rooms_price.Text = defaultPrice.ToString("F2");
            rooms_status.Text = "Active";
        }

        private void rooms_importBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog file = new OpenFileDialog();
                string imagePath = "";

                file.Filter = "Image Files (*.jpg; *.png)|*.jpg;*.png";

                if (file.ShowDialog() == DialogResult.OK)
                {
                    imagePath = file.FileName;
                    rooms_picture.ImageLocation = imagePath;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rooms_updateBtn_Click(object sender, EventArgs e)
        {
            if (isEmpty())
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    if (MessageBox.Show("Are you want to UPDATE ID: " + id + "?", "Confirmation Message"
                   , MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        using (SqlConnection connect = new SqlConnection(conn))
                        {
                            connect.Open();

                            string updateData = "UPDATE rooms SET room_id = @roomID, type = @type, room_name = @name, price =@price" +
                                ", status = @status, date_update = @update WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {
                                cmd.Parameters.AddWithValue("@roomID", rooms_roomID.Text.Trim());
                                cmd.Parameters.AddWithValue("@type", rooms_type.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@name", rooms_roomName.Text.Trim());
                                cmd.Parameters.AddWithValue("@price", rooms_price.Text.Trim());
                                cmd.Parameters.AddWithValue("@status", rooms_status.SelectedItem.ToString());

                                DateTime today = DateTime.Today;
                                cmd.Parameters.AddWithValue("@update", today);
                                cmd.Parameters.AddWithValue("@id", id);

                                cmd.ExecuteNonQuery();

                                clearFields();
                                displayRoomsData();

                                MessageBox.Show("Updated successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    MessageBox.Show("Something went wrong.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int id;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                id = (int)row.Cells[0].Value;
                rooms_roomID.Text = row.Cells[1].Value.ToString();
                rooms_type.Text = row.Cells[2].Value.ToString();
                rooms_roomName.Text = row.Cells[3].Value.ToString();
                rooms_price.Text = row.Cells[4].Value.ToString();

                rooms_picture.ImageLocation = row.Cells[5].Value.ToString();

                rooms_status.Text = row.Cells[6].Value.ToString();


            }
        }

        public void clearFields()
        {
            rooms_roomID.Text = "";
            rooms_type.SelectedIndex = -1;
            rooms_roomName.Text = "";
            rooms_price.Text = "";
            rooms_picture.Image = null;
            rooms_status.SelectedIndex = -1;
        }
        private void rooms_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }


        private void rooms_deleteBtn_Click(object sender, EventArgs e)
        {
            if (isEmpty())
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    if (MessageBox.Show("Are you want to DELETE ID: " + id + "?", "Confirmation Message"
                   , MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        using (SqlConnection connect = new SqlConnection(conn))
                        {
                            connect.Open();

                            string updateData = "UPDATE rooms SET date_delete = @delete WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {

                                DateTime today = DateTime.Today;
                                cmd.Parameters.AddWithValue("@delete", today);
                                cmd.Parameters.AddWithValue("@id", id);

                                cmd.ExecuteNonQuery();

                                clearFields();
                                displayRoomsData();

                                MessageBox.Show("Deleted successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Something went wrong.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
