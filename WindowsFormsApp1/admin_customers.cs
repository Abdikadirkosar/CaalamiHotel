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
    public partial class admin_customers : UserControl
    {
        private string conn = hotelData.ConnectionString;

        private List<customersData> _allCustomers = new List<customersData>();

        public admin_customers()
        {
            InitializeComponent();
            SetupUI();
            displayCustomers();
            ApplyModernStyles();
        }

        private void ApplyModernStyles()
        {
            UiHelper.StyleModernGrid(dataGridView1);
            UiHelper.StyleFlatButton(checkOutBtn, Color.FromArgb(39, 174, 96), Color.FromArgb(30, 140, 75), 8);
            checkOutBtn.ForeColor = Color.White;
            UiHelper.MakeRounded(panel1, 12);
        }

        // ── Setup extra controls programmatically ─────────────────────────
        private TextBox searchBox;
        private Button checkOutBtn;
        private Label searchLabel;
        private Label statusLegend;
        private int selectedCustomerRowIndex = -1;

        private void SetupUI()
        {
            // ── Search Bar ────────────────────────────────────────────────
            searchLabel = new Label();
            searchLabel.Text = "🔍 Search:";
            searchLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            searchLabel.ForeColor = Color.FromArgb(15, 32, 65);
            searchLabel.Location = new Point(14, 14);
            searchLabel.AutoSize = true;

            searchBox = new TextBox();
            searchBox.Font = new Font("Segoe UI", 11);
            searchBox.Location = new Point(90, 10);
            searchBox.Size = new Size(350, 28);
            searchBox.TextChanged += SearchBox_TextChanged;
            SetWatermark(searchBox, "Search by name, book ID, room ID...");

            // ── CheckOut Button ───────────────────────────────────────────
            checkOutBtn = new Button();
            checkOutBtn.Text = "✅  Check-Out Guest";
            checkOutBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            checkOutBtn.BackColor = Color.FromArgb(39, 174, 96);
            checkOutBtn.ForeColor = Color.White;
            checkOutBtn.FlatStyle = FlatStyle.Flat;
            checkOutBtn.FlatAppearance.BorderSize = 0;
            checkOutBtn.Location = new Point(460, 7);
            checkOutBtn.Size = new Size(180, 34);
            checkOutBtn.Cursor = Cursors.Hand;
            checkOutBtn.Click += CheckOutBtn_Click;

            // ── Legend ────────────────────────────────────────────────────
            statusLegend = new Label();
            statusLegend.Text = "  🟢 Checked In   🟠 Checked Out   🔴 Overdue";
            statusLegend.Font = new Font("Segoe UI", 9);
            statusLegend.ForeColor = Color.FromArgb(70, 90, 130);
            statusLegend.Location = new Point(660, 16);
            statusLegend.AutoSize = true;

            panel1.Controls.Add(searchLabel);
            panel1.Controls.Add(searchBox);
            panel1.Controls.Add(checkOutBtn);
            panel1.Controls.Add(statusLegend);

            // Move grid down to make room for search bar
            dataGridView1.Location = new Point(dataGridView1.Location.X, 50);
            dataGridView1.Size = new Size(dataGridView1.Width, dataGridView1.Height - 34);
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;

            label5.Text = "👥  Guest Management";
            label5.ForeColor = Color.FromArgb(15, 32, 65);
        }

        // ── Real-time search ──────────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string q = searchBox.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(q))
            {
                dataGridView1.DataSource = _allCustomers;
            }
            else
            {
                var filtered = _allCustomers.Where(c =>
                    c.FullName.ToLower().Contains(q) ||
                    c.BookID.ToLower().Contains(q) ||
                    c.RoomID.ToLower().Contains(q) ||
                    c.Status.ToLower().Contains(q) ||
                    c.Email.ToLower().Contains(q)
                ).ToList();
                dataGridView1.DataSource = filtered;
            }
        }

        // ── Color coding by status ────────────────────────────────────────
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].DataBoundItem == null) return;

            var row = dataGridView1.Rows[e.RowIndex];
            var item = row.DataBoundItem as customersData;
            if (item == null) return;

            string status = item.Status ?? "";
            DateTime checkoutDate;
            bool isOverdue = DateTime.TryParse(item.CheckOut, out checkoutDate)
                             && checkoutDate < DateTime.Today
                             && status == "Checked In";

            if (isOverdue)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 182, 193); // Light Red
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }
            else if (status == "Checked In")
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(198, 239, 206); // Light Green
                row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 100, 0);
            }
            else if (status == "Checked Out")
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 200); // Light Orange
                row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 75, 0);
            }
        }

        // ── Track selected row ────────────────────────────────────────────
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
                selectedCustomerRowIndex = dataGridView1.SelectedRows[0].Index;
        }

        // ── Check-Out action ──────────────────────────────────────────────
        private void CheckOutBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a customer to check out.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = dataGridView1.SelectedRows[0].DataBoundItem as customersData;
            if (selected == null) return;

            if (selected.Status == "Checked Out")
            {
                MessageBox.Show("This guest has already checked out.", "Already Checked Out",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ── Step 1: Extra Services Dialog ─────────────────────────────
            decimal extraTotal = 0m;
            string extraServicesSummary = "";

            using (Form extrasForm = new Form())
            {
                extrasForm.Text = $"🧾 Adeegyada Dheeraadka — {selected.FullName}";
                extrasForm.Size = new Size(440, 420);
                extrasForm.StartPosition = FormStartPosition.CenterParent;
                extrasForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                extrasForm.MaximizeBox = false;
                extrasForm.BackColor = Color.FromArgb(245, 248, 255);
                extrasForm.Font = new Font("Segoe UI", 10);

                var titleLbl = new Label { Text = "📋 Adeegyada Dheeraadka ah (Extras)", AutoSize = true,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(15, 32, 65),
                    Location = new Point(15, 12) };

                var subLbl = new Label { Text = "Geli lacagaha adeegyada martidu isticmaalay (0 = ma isticmaalin)",
                    AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8.5f),
                    Location = new Point(15, 38) };

                var services = new[] {
                    "🍽️  Cunto/Restaurant",
                    "👕  Dhaqista Dharka/Laundry",
                    "🚖  Gaadiidka/Transport",
                    "🛎️  Adeega Qolka/Room Service",
                    "🥤  Minibar",
                    "🅿️  Baakingga/Parking"
                };

                var inputs = new TextBox[services.Length];
                int y = 68;
                for (int i = 0; i < services.Length; i++)
                {
                    var svcLbl = new Label { Text = services[i], AutoSize = true,
                        Location = new Point(15, y + 4), ForeColor = Color.FromArgb(40, 60, 100) };
                    var tb = new TextBox { Text = "0", Location = new Point(270, y),
                        Size = new Size(120, 26), TextAlign = HorizontalAlignment.Right };
                    inputs[i] = tb;
                    extrasForm.Controls.Add(svcLbl);
                    extrasForm.Controls.Add(tb);
                    y += 38;
                }

                var totalLbl = new Label { Text = "Wadarta Dheeraadka: $0.00",
                    AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 32, 65), Location = new Point(15, y + 6) };

                // Live total update
                foreach (var tb in inputs)
                    tb.TextChanged += (s2, e2) =>
                    {
                        decimal sum = 0;
                        foreach (var t in inputs)
                            if (decimal.TryParse(t.Text, out decimal v)) sum += v;
                        totalLbl.Text = $"Wadarta Dheeraadka: ${sum:F2}";
                    };

                var proceedBtn = new Button { Text = "✅  Xaqiiji & Bax",
                    Location = new Point(230, y + 36), Size = new Size(160, 36),
                    BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    DialogResult = DialogResult.OK };
                proceedBtn.FlatAppearance.BorderSize = 0;

                var skipBtn = new Button { Text = "⏭  Dhaafo Extras",
                    Location = new Point(60, y + 36), Size = new Size(155, 36),
                    BackColor = Color.FromArgb(130, 140, 160), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel };
                skipBtn.FlatAppearance.BorderSize = 0;

                extrasForm.Controls.AddRange(new Control[] { titleLbl, subLbl, totalLbl, proceedBtn, skipBtn });
                extrasForm.AcceptButton = proceedBtn;

                var result = extrasForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var serviceNames = new[] { "Restaurant", "Laundry", "Transport", "Room Service", "Minibar", "Parking" };
                    var extraLines = new List<string>();
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (decimal.TryParse(inputs[i].Text, out decimal v) && v > 0)
                        {
                            extraTotal += v;
                            extraLines.Add($"{serviceNames[i]}: ${v:F2}");
                        }
                    }
                    extraServicesSummary = string.Join(", ", extraLines);
                }
                // If Cancel (Skip), extraTotal stays 0
            }

            if (MessageBox.Show(
                $"Xaqiiji Bixitaanka:\n\nMartida: {selected.FullName}\nQolka:    {selected.RoomID}\nQiimaha Qolka: ${selected.Price}\nAdeegyada Dheeraadka: ${extraTotal:F2}",
                "Xaqiiji Bixitaanka", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(conn))
                    {
                        connect.Open();

                        // 1. Update customer status
                        string updateCustomer = "UPDATE customer SET status='Checked Out', checkout_date=@today WHERE book_id=@bookID";
                        using (SqlCommand cmd = new SqlCommand(updateCustomer, connect))
                        {
                            cmd.Parameters.AddWithValue("@today",  DateTime.Today);
                            cmd.Parameters.AddWithValue("@bookID", selected.BookID);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Mark room as Dirty - needs cleaning before re-booking
                        string updateRoom = "UPDATE rooms SET status='Dirty / Cleaning' WHERE room_id=@roomID";
                        using (SqlCommand cmd = new SqlCommand(updateRoom, connect))
                        {
                            cmd.Parameters.AddWithValue("@roomID", selected.RoomID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Generate invoice with extras
                    try
                    {
                        DateTime fromDate = DateTime.Today.AddDays(-1);
                        DateTime toDate = DateTime.Today;
                        decimal roomPrice = 0.0m;

                        DateTime.TryParse(selected.CheckIn, out fromDate);
                        DateTime.TryParse(selected.CheckOut, out toDate);
                        decimal.TryParse(selected.Price, out roomPrice);

                        decimal totalWithExtras = roomPrice + extraTotal;

                        InvoiceGenerator.GenerateInvoice(
                            selected.BookID,
                            selected.FullName,
                            selected.Email,
                            selected.RoomID,
                            totalWithExtras,
                            fromDate,
                            toDate,
                            DateTime.Today,
                            extraTotal,
                            extraServicesSummary
                        );
                    }
                    catch { /* Silent */ }

                    AuditLogger.Log("Check-Out",
                        $"Guest '{selected.FullName}' (Book: {selected.BookID}) checked out. Extras: ${extraTotal:F2} ({extraServicesSummary}). Invoice saved.");

                    MessageBox.Show(
                        $"✅ {selected.FullName} waa la baxsiiyey!\n\n" +
                        $"🧹 Qolka #{selected.RoomID} xaaladdiisa:\n" +
                        $"   ➡️  'Dirty / Cleaning' — Nadiifin u baahan\n\n" +
                        $"⚠️  Staff-ku ha nadiifiyo ka dib 'Clean / Ready' u beddelo.\n" +
                        $"   Haddaanu la nadiifin, lama buukin karo!\n\n" +
                        $"📄 Rasiidka (Invoice) Desktop-kaaga ayaa lagu keydinayaa.",
                        "Bixitaan Guul ✅", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    displayCustomers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Check-out failed: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            displayCustomers();
        }

        public void displayCustomers()
        {
            try
            {
                customersData cData = new customersData();
                _allCustomers = cData.customerListData();
                dataGridView1.DataSource = _allCustomers;
                if (searchBox != null) searchBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Win32 watermark (placeholder text) helper for .NET Framework 4.8 ──
        private static void SetWatermark(TextBox tb, string text)
        {
            NativeMethods.SendMessage(tb.Handle, NativeMethods.EM_SETCUEBANNER, true, text);
        }
    }
}

