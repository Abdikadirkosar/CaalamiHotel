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
    public partial class staff_bookRoom : UserControl
    {
        public staff_bookRoom()
        {
            InitializeComponent();
            displayRooms();
            ApplyModernStyles();
            bookRoom_from.ValueChanged += DateChanged;
            bookRoom_to.ValueChanged += DateChanged;
        }

        private void ApplyModernStyles()
        {
            UiHelper.StyleModernGrid(dataGridView1);
            UiHelper.StyleFlatButton(bookRoom_bookBtn, Color.FromArgb(212, 160, 23), Color.FromArgb(180, 130, 10), 8);
            bookRoom_bookBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(bookRoom_clearBtn, Color.FromArgb(120, 120, 120), Color.FromArgb(100, 100, 100), 8);
            bookRoom_clearBtn.ForeColor = Color.White;
            UiHelper.StyleFlatButton(bookRoom_scheduleBtn, Color.FromArgb(15, 32, 65), Color.FromArgb(30, 50, 90), 8);
            bookRoom_scheduleBtn.ForeColor = Color.White;

            UiHelper.MakeRounded(panel5, 12);
            UiHelper.MakeRounded(panel6, 12);
            UiHelper.MakeRounded(panel7, 12);
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }
            displayRooms();
        }
        public void displayRooms()
        {
            roomsData rData = new roomsData();

            List<roomsData> listData = rData.roomsDataList();

            dataGridView1.DataSource = listData;
        }

        private int getID = 0;

        private decimal regprice = 0;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                getID = (int)row.Cells[0].Value;
                bookRoom_roomID.Text = row.Cells[1].Value.ToString();
                bookRoom_roomType.Text = row.Cells[2].Value.ToString();
                bookRoom_roomName.Text = row.Cells[3].Value.ToString();
                bookRoom_regPrice.Text = (Convert.ToInt32(row.Cells[4].Value)).ToString("0.00");


                regprice = Convert.ToDecimal(row.Cells[4].Value);

                bookRoom_imageView.ImageLocation = row.Cells[5].Value.ToString();

                bookRoom_status.Text = row.Cells[6].Value.ToString();
                CalculateTotal();
            }
        }

        private void bookRoom_scheduleBtn_Click(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void DateChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (regprice == 0) return;
            DateTime fromDate = bookRoom_from.Value.Date;
            DateTime toDate = bookRoom_to.Value.Date;

            TimeSpan countDays = toDate - fromDate;
            int days = countDays.Days;

            if (days < 0)
            {
                bookRoom_total.Text = "0.00";
            }
            else if (days == 0)
            {
                bookRoom_total.Text = regprice.ToString("0.00");
            }
            else
            {
                bookRoom_total.Text = (days * regprice).ToString("0.00");
            }
        }

        private void bookRoom_bookBtn_Click(object sender, EventArgs e)
        {
            if(regprice == 0 || bookRoom_total.Text == "0.00")
            {
                MessageBox.Show("Please fill all info correctly", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (bookRoom_status.Text != "Active" && bookRoom_status.Text != "Available" && bookRoom_status.Text != "Clean / Ready")
            {
                MessageBox.Show("Macaan qolkani diyaar ma aha (waa la deggan yahay, waa wasakh, ama adeeg ayuu ku jiraa).", "Cillad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                hotelData.roomID = bookRoom_roomID.Text;
                hotelData.fromDate = bookRoom_from.Value;
                hotelData.toDate = bookRoom_to.Value;
                hotelData.price = bookRoom_total.Text;

                Form formbg = new Form();

                try
                {
                    using (clientinfo ciForm = new clientinfo())
                    {


                        ciForm.Owner = formbg;
                        ciForm.ShowDialog();

                        formbg.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    formbg.Dispose();
                }
            }
        }

        public void clearFields()
        {
            bookRoom_roomID.Text = "----------";
            bookRoom_roomType.Text = "----------";
            bookRoom_roomType.Text = "----------";
            bookRoom_status.Text = "----------";
            bookRoom_total.Text = "0.00";
            bookRoom_regPrice.Text = "0.00";

            bookRoom_imageView.Image = null;
        }
        private void bookRoom_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private void bookRoom_clearBtn_Click_1(object sender, EventArgs e)
        {
            clearFields();
        }
    }
}
