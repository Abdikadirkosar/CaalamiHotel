using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.IO;

namespace WindowsFormsApp1
{
    // ── Shared hotel booking data (static pass-through) ──────────────────
    class hotelData
    {
        public static string roomID;
        public static DateTime fromDate;
        public static DateTime toDate;
        public static string price;

        // Centralized Connection String — no hardcoded database file paths
        public static readonly string ConnectionString = 
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=hotel1;Integrated Security=True;Connect Timeout=120;";
    }

    // ── Current session info ─────────────────────────────────────────────
    public static class AppSession
    {
        public static string CurrentUser { get; set; } = "System";
        public static string CurrentRole  { get; set; } = "";
    }

    // ── Audit Logger — records every action to audit_log table ───────────
    public static class AuditLogger
    {
        public static void Log(string action, string details = "")
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(hotelData.ConnectionString))
                {
                    connect.Open();
                    string sql = "INSERT INTO audit_log (username, action, details, action_date) " +
                                 "VALUES (@user, @action, @details, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@user",    AppSession.CurrentUser ?? "System");
                        cmd.Parameters.AddWithValue("@action",  action  ?? "");
                        cmd.Parameters.AddWithValue("@details", details ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Silent — never crash app because of logging */ }
        }
    }

    // ── Win32 helper — enables placeholder text on TextBox (.NET FX 4.8) ──
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool SendMessage(IntPtr hWnd, int msg, bool wParam, string lParam);
        internal const int EM_SETCUEBANNER = 0x1501;
    }

    // ── Invoice Text Generator ───────────────────────────────────────────
    public static class InvoiceGenerator
    {
        public static void GenerateInvoice(
            string bookID, string guestName, string email, string roomID,
            decimal totalPrice, DateTime fromDate, DateTime toDate, DateTime checkoutDate,
            decimal extraCharges = 0m, string extraDetails = "")
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filename = Path.Combine(desktopPath, $"Invoice_{bookID}.txt");

                int totalDays = (toDate.Date - fromDate.Date).Days;
                if (totalDays <= 0) totalDays = 1;

                decimal roomSubtotal = totalPrice - extraCharges;
                decimal roomRate     = roomSubtotal / totalDays;
                decimal vatTax       = totalPrice * 0.15m;
                decimal grandTotal   = totalPrice + vatTax;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==========================================================================");
                sb.AppendLine("            🏨 CAALAMI HOTEL — SOMALILAND                      ");
                sb.AppendLine("                      RASIIDKA MARTIDA (GUEST INVOICE)                    ");
                sb.AppendLine("==========================================================================");
                sb.AppendLine($"Lambar Rasiidka:   {bookID}");
                sb.AppendLine($"Taariikhda:        {checkoutDate:dddd, dd MMMM yyyy}");
                sb.AppendLine("--------------------------------------------------------------------------");
                sb.AppendLine($"Magaca Martida:    {guestName}");
                sb.AppendLine($"Email:             {email}");
                sb.AppendLine($"Qolka:             #{roomID}");
                sb.AppendLine("--------------------------------------------------------------------------");
                sb.AppendLine($"Taariikhda Gelitaanka:  {fromDate:dd/MM/yyyy}");
                sb.AppendLine($"Taariikhda Bixitaanka:  {checkoutDate:dd/MM/yyyy}");
                sb.AppendLine($"Muddada La Joogay:       {totalDays} habeenba");
                sb.AppendLine("--------------------------------------------------------------------------");
                sb.AppendLine($"Qiimaha Qolka (habeenba):          ${roomRate:F2}");
                sb.AppendLine($"Wadarta Qolka ({totalDays} habeenba):       ${roomSubtotal:F2}");
                if (extraCharges > 0)
                {
                    sb.AppendLine("--");
                    sb.AppendLine($"Adeegyada Dheeraadka:              ${extraCharges:F2}");
                    if (!string.IsNullOrEmpty(extraDetails))
                    {
                        foreach (string detail in extraDetails.Split(','))
                            if (!string.IsNullOrWhiteSpace(detail))
                                sb.AppendLine($"   • {detail.Trim()}");
                    }
                }
                sb.AppendLine($"Wadarta Hoose (Subtotal):           ${totalPrice:F2}");
                sb.AppendLine($"Canshuurta VAT (15%):               ${vatTax:F2}");
                sb.AppendLine("--------------------------------------------------------------------------");
                sb.AppendLine($"WADARTA GUUD (GRAND TOTAL):         ${grandTotal:F2}");
                sb.AppendLine("==========================================================================");
                sb.AppendLine("       Mahadsanid aad u mahadsan tahay — CAALAMI HOTEL!        ");
                sb.AppendLine("              Waxaanu rajeynaa inaaad mar kale noogu timaado!              ");
                sb.AppendLine("==========================================================================");

                File.WriteAllText(filename, sb.ToString());
            }
            catch { /* Silent */ }
        }
    }

    // ── Database Backup & CSV Exporter Utility ───────────────────────────
    public static class BackupUtility
    {
        public static string ExportTableToCSV(string tableName)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filename = Path.Combine(desktopPath, $"{tableName}_Backup_{DateTime.Today:yyyyMMdd}.csv");

                using (SqlConnection connect = new SqlConnection(hotelData.ConnectionString))
                {
                    connect.Open();
                    string query = "SELECT * FROM " + tableName; // Safe local admin utility
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        using (StreamWriter sw = new StreamWriter(filename))
                        {
                            // Write headers
                            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
                            sw.WriteLine(string.Join(",", columns.Select(c => $"\"{c}\"")));

                            // Write rows
                            while (reader.Read())
                            {
                                var fields = Enumerable.Range(0, reader.FieldCount)
                                    .Select(i => $"\"{reader.GetValue(i).ToString().Replace("\"", "\"\"")}\"")
                                    .ToArray();
                                sw.WriteLine(string.Join(",", fields));
                            }
                        }
                    }
                }
                return filename;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}
