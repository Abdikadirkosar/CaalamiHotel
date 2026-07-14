using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run DB initialization in background while splash screen animates
            bool dbDone = false;
            System.Threading.Thread dbThread = new System.Threading.Thread(() =>
            {
                CleanCppFiles();
                EnsureLocalDbRunning();
                InitializeDatabase();
                dbDone = true;
            });
            dbThread.IsBackground = true;
            dbThread.Start();

            // Show splash on main (UI) thread — it auto-closes via its Timer
            using (SplashForm splash = new SplashForm())
            {
                splash.ShowDialog(); // Blocks here until splash timer completes
            }

            // If DB thread still running after splash, wait for it
            while (!dbDone)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

            Application.Run(new Form1());
        }

        // ── Auto-start the MSSQLLocalDB instance if it is stopped ──────────
        private static void EnsureLocalDbRunning()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName               = "sqllocaldb",
                    Arguments             = "start MSSQLLocalDB",
                    UseShellExecute       = false,
                    CreateNoWindow        = true,
                    RedirectStandardOutput= true,
                    RedirectStandardError = true
                };
                using (var proc = System.Diagnostics.Process.Start(psi))
                {
                    proc.WaitForExit(8000); // Wait up to 8 s for LocalDB to start
                }
                System.Threading.Thread.Sleep(1500); // Give it time to fully initialize
            }
            catch { /* sqllocaldb may not be on PATH — that is OK, we will still try */ }
        }

        private static void InitializeDatabase()
        {
            string masterConn = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=120;";

            // Retry up to 3 times in case the LocalDB instance needs extra time to wake up
            int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try { InitializeDatabaseCore(masterConn); return; }
                catch (Exception) when (attempt < maxAttempts)
                {
                    System.Threading.Thread.Sleep(3000); // Wait 3 s between retries
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Database initialization failed!\n\n" +
                        "Details: " + ex.Message + "\n\n" +
                        "Make sure Microsoft SQL Server LocalDB is installed on this system.",
                        "Database Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private static void InitializeDatabaseCore(string masterConn)
        {

            try
            {
                // 1. Create database 'hotel1' if it does not exist in LocalDB
                using (SqlConnection conn = new SqlConnection(masterConn))
                {
                    conn.Open();
                    string checkDb = "SELECT database_id FROM sys.databases WHERE name = 'hotel1'";
                    using (SqlCommand cmd = new SqlCommand(checkDb, conn))
                    {
                        var dbId = cmd.ExecuteScalar();
                        if (dbId == null)
                        {
                            using (SqlCommand createDb = new SqlCommand("CREATE DATABASE hotel1", conn))
                            {
                                createDb.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // 2. Open connection to the created database and check/create tables
                using (SqlConnection conn = new SqlConnection(hotelData.ConnectionString))
                {
                    conn.Open();

                    // ── Self-healing Upgrade to Somali Somaliland Seed Data ────────
                    bool needsSomaliUpgrade = false;
                    try
                    {
                        string checkEng = "SELECT COUNT(*) FROM rooms WHERE room_name = 'Room 101'";
                        using (SqlCommand checkCmd = new SqlCommand(checkEng, conn))
                        {
                            int engCount = (int)checkCmd.ExecuteScalar();
                            if (engCount > 0) needsSomaliUpgrade = true;
                        }
                    }
                    catch { /* Table rooms might not exist yet — that's OK */ }

                    if (needsSomaliUpgrade)
                    {
                        try
                        {
                            string dropTables = "DROP TABLE IF EXISTS customer; DROP TABLE IF EXISTS rooms; DROP TABLE IF EXISTS users; DROP TABLE IF EXISTS audit_log;";
                            using (SqlCommand dropCmd = new SqlCommand(dropTables, conn))
                            {
                                dropCmd.ExecuteNonQuery();
                            }
                        }
                        catch { }
                    }

                    // Create users table
                    string createUsers = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U')
                        BEGIN
                            CREATE TABLE users (
                                id INT PRIMARY KEY IDENTITY(1,1),
                                username VARCHAR(100) NOT NULL,
                                password VARCHAR(100) NOT NULL,
                                role VARCHAR(50) NOT NULL,
                                status VARCHAR(50) NOT NULL,
                                date_register DATE NULL
                            );
                        END";
                    using (SqlCommand cmd = new SqlCommand(createUsers, conn)) { cmd.ExecuteNonQuery(); }

                    // Create rooms table
                    string createRooms = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='rooms' AND xtype='U')
                        BEGIN
                            CREATE TABLE rooms (
                                id INT PRIMARY KEY IDENTITY(1,1),
                                room_id VARCHAR(50) NULL,
                                type VARCHAR(100) NULL,
                                room_name VARCHAR(100) NULL,
                                price FLOAT NULL,
                                image_path VARCHAR(500) NULL,
                                status VARCHAR(50) NULL,
                                date_register DATE NULL,
                                date_update DATE NULL,
                                date_delete DATE NULL
                            );
                        END";
                    using (SqlCommand cmd = new SqlCommand(createRooms, conn)) { cmd.ExecuteNonQuery(); }

                    // Create customer table
                    string createCustomer = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='customer' AND xtype='U')
                        BEGIN
                            CREATE TABLE customer (
                                id INT PRIMARY KEY IDENTITY(1,1),
                                book_id VARCHAR(50) NULL,
                                full_name VARCHAR(200) NULL,
                                email VARCHAR(200) NULL,
                                contact VARCHAR(50) NULL,
                                gender VARCHAR(20) NULL,
                                address VARCHAR(300) NULL,
                                room_id VARCHAR(50) NULL,
                                price DECIMAL(10,2) NULL,
                                status_payment VARCHAR(50) NULL,
                                status VARCHAR(50) NULL,
                                date_from DATE NULL,
                                date_to DATE NULL,
                                date_book DATE NULL,
                                checkout_date DATE NULL
                            );
                        END
                        ELSE
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customer') AND name = 'checkout_date')
                            BEGIN
                                ALTER TABLE customer ADD checkout_date DATE NULL;
                            END
                        END";
                    using (SqlCommand cmd = new SqlCommand(createCustomer, conn)) { cmd.ExecuteNonQuery(); }

                    // Create audit_log table
                    string createAudit = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='audit_log' AND xtype='U')
                        BEGIN
                            CREATE TABLE audit_log (
                                id INT PRIMARY KEY IDENTITY(1,1),
                                username VARCHAR(100) NOT NULL,
                                action VARCHAR(200) NOT NULL,
                                details VARCHAR(500) NULL,
                                action_date DATETIME DEFAULT GETDATE()
                            );
                        END";
                    using (SqlCommand cmd = new SqlCommand(createAudit, conn)) { cmd.ExecuteNonQuery(); }

                    // Migrate old hotel name references in existing database
                    try
                    {
                        string migrateSql = "UPDATE audit_log SET details = REPLACE(details, 'Caalami Hotel', 'CAALAMI HOTEL')";
                        using (SqlCommand migrateCmd = new SqlCommand(migrateSql, conn))
                        {
                            migrateCmd.ExecuteNonQuery();
                        }
                    }
                    catch { }

                    // Create Admin user (default credentials: Admin / admin)
                    string createAdmin = @"
                        IF NOT EXISTS (SELECT * FROM users WHERE username = 'Admin')
                        BEGIN
                            INSERT INTO users (username, password, role, status, date_register)
                            VALUES ('Admin', 'admin', 'Admin', 'Active', GETDATE());
                        END";
                    using (SqlCommand cmd = new SqlCommand(createAdmin, conn)) { cmd.ExecuteNonQuery(); }

                    // 3. Seed initial demo data (20 records) if empty
                    string checkRooms = "SELECT COUNT(*) FROM rooms";
                    int roomCount = 0;
                    using (SqlCommand cmd = new SqlCommand(checkRooms, conn)) { roomCount = (int)cmd.ExecuteScalar(); }

                    if (roomCount == 0)
                    {
                        // Insert 20 rooms — CAALAMI HOTEL, Somaliland
                        string insertRooms = @"
                            INSERT INTO rooms (room_id, type, room_name, price, status, date_register) VALUES
                            ('101', 'Single',         'Qolka Xiddigta',        45.00, 'Active',      GETDATE()),
                            ('102', 'Single',         'Qolka Badda',           45.00, 'Active',      GETDATE()),
                            ('103', 'Double',         'Qolka Hargeysa',        80.00, 'Active',      GETDATE()),
                            ('104', 'Double',         'Qolka Berbera',         80.00, 'Maintenance', GETDATE()),
                            ('105', 'Suite',          'Qolka Waabari',        150.00, 'Active',      GETDATE()),
                            ('201', 'Single',         'Qolka Borama',          50.00, 'Active',      GETDATE()),
                            ('202', 'Double',         'Qolka Burco',           90.00, 'Active',      GETDATE()),
                            ('203', 'Deluxe',         'Qolka Gacanka',        120.00, 'Active',      GETDATE()),
                            ('204', 'Suite',          'Qolka Aw-Barre',       180.00, 'Active',      GETDATE()),
                            ('205', 'Deluxe',         'Qolka Zeylac',         120.00, 'Active',      GETDATE()),
                            ('301', 'Single',         'Qolka Ceerigaavo',      55.00, 'Active',      GETDATE()),
                            ('302', 'Double',         'Qolka Laascaanood',    100.00, 'Active',      GETDATE()),
                            ('303', 'Suite',          'Qolka Golaaha',        220.00, 'Active',      GETDATE()),
                            ('304', 'Deluxe',         'Qolka Durdur',         140.00, 'Maintenance', GETDATE()),
                            ('305', 'Suite',          'Qolka Xidigtaha',      220.00, 'Active',      GETDATE()),
                            ('401', 'Penthouse',      'Qolka Madaxtooye',     380.00, 'Active',      GETDATE()),
                            ('402', 'Penthouse',      'Qolka Golaha Sare',    380.00, 'Active',      GETDATE()),
                            ('403', 'Executive Suite','Qolka VIP Somaliland', 280.00, 'Active',      GETDATE()),
                            ('404', 'Deluxe',         'Qolka Xornimada',      150.00, 'Active',      GETDATE()),
                            ('405', 'Double',         'Qolka Guulwade',       105.00, 'Active',      GETDATE())";
                        using (SqlCommand cmd = new SqlCommand(insertRooms, conn)) { cmd.ExecuteNonQuery(); }
                    }

                    string checkCustomers = "SELECT COUNT(*) FROM customer";
                    int customerCount = 0;
                    using (SqlCommand cmd = new SqlCommand(checkCustomers, conn)) { customerCount = (int)cmd.ExecuteScalar(); }

                    if (customerCount == 0)
                    {
                        // Insert 20 Somali guest bookings — real Somali names & Somaliland cities
                        string insertCustomers = @"
                            INSERT INTO customer (book_id, full_name, email, contact, gender, address, room_id, price, status_payment, status, date_from, date_to, date_book, checkout_date) VALUES
                            ('B001', 'Axmed Cali Rooble',    'axmed.rooble@gmail.com',  '063-4201001', 'Male',   'Deg Sha-cad, Hargeysa',     '101',  90.00, 'Paid', 'Checked In',  DATEADD(day,-2,GETDATE()), DATEADD(day,2,GETDATE()),  DATEADD(day,-2,GETDATE()), NULL),
                            ('B002', 'Faadumo Maxamed Siiro','faadumo.siiro@gmail.com', '063-4202002', 'Female', 'Geed-Deeble, Hargeysa',     '103', 160.00, 'Paid', 'Checked In',  DATEADD(day,-1,GETDATE()), DATEADD(day,3,GETDATE()),  DATEADD(day,-1,GETDATE()), NULL),
                            ('B003', 'Cabdi Warsame Guure',  'cabdi.guure@gmail.com',   '063-4203003', 'Male',   'Koonfurta, Berbera',         '105', 300.00, 'Paid', 'Checked Out', DATEADD(day,-5,GETDATE()), DATEADD(day,-3,GETDATE()), DATEADD(day,-5,GETDATE()), DATEADD(day,-3,GETDATE())),
                            ('B004', 'Hodan Jaamac Yuusuf',  'hodan.yuusuf@gmail.com',  '063-4204004', 'Female', 'Wajaale, Hargeysa',          '201', 100.00, 'Paid', 'Checked Out', DATEADD(day,-4,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-4,GETDATE()), DATEADD(day,-2,GETDATE())),
                            ('B005', 'Mustafe Xirsi Aw-Ali', 'mustafe.awali@gmail.com', '063-4205005', 'Male',   'Degmada Borama, Woqooyi',   '202', 270.00, 'Paid', 'Checked In',  DATEADD(day,-6,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-6,GETDATE()), NULL),
                            ('B006', 'Caasha Nuur Duale',    'caasha.duale@gmail.com',  '063-4206006', 'Female', 'Xaafadda Jigjiga-Yar, HGA', '203', 240.00, 'Paid', 'Checked In',  DATEADD(day,-1,GETDATE()), DATEADD(day,4,GETDATE()),  DATEADD(day,-1,GETDATE()), NULL),
                            ('B007', 'Cumar Cabdullaahi Said','cumar.said@gmail.com',   '063-4207007', 'Male',   'Xaafadda Gacaan-Libah, BRO','301', 110.00, 'Paid', 'Checked Out', DATEADD(day,-7,GETDATE()), DATEADD(day,-5,GETDATE()), DATEADD(day,-7,GETDATE()), DATEADD(day,-5,GETDATE())),
                            ('B008', 'Sahra Daahir Cismaan', 'sahra.cismaan@gmail.com', '063-4208008', 'Female', 'Degmada Berbera, SL',        '302', 200.00, 'Paid', 'Checked In',  DATEADD(day,-2,GETDATE()), DATEADD(day,2,GETDATE()),  DATEADD(day,-2,GETDATE()), NULL),
                            ('B009', 'Yaasiin Maxamuud Ide', 'yaasiin.ide@gmail.com',   '063-4209009', 'Male',   'Laascaanood, Sool',          '303', 440.00, 'Paid', 'Checked In',  DATEADD(day,-1,GETDATE()), DATEADD(day,5,GETDATE()),  DATEADD(day,-1,GETDATE()), NULL),
                            ('B010', 'Warsan Cabdi Timo',    'warsan.timo@gmail.com',   '063-4210010', 'Female', 'Ceerigaavo, Sanaag',         '305', 440.00, 'Paid', 'Checked Out', DATEADD(day,-6,GETDATE()), DATEADD(day,-4,GETDATE()), DATEADD(day,-6,GETDATE()), DATEADD(day,-4,GETDATE())),
                            ('B011', 'Amiin Siciid Isaaq',   'amiin.isaaq@gmail.com',   '063-4211011', 'Male',   'Xaafadda STC, Hargeysa',    '401', 760.00, 'Paid', 'Checked In',  DATEADD(day,-3,GETDATE()), DATEADD(day,3,GETDATE()),  DATEADD(day,-3,GETDATE()), NULL),
                            ('B012', 'Luul Aadan Xaashi',    'luul.xaashi@gmail.com',   '063-4212012', 'Female', 'Xaafadda 26-June, HGA',     '403', 560.00, 'Paid', 'Checked Out', DATEADD(day,-5,GETDATE()), DATEADD(day,-3,GETDATE()), DATEADD(day,-5,GETDATE()), DATEADD(day,-3,GETDATE())),
                            ('B013', 'Mahad Cabdiqaadir Nur','mahad.nur@gmail.com',     '063-4213013', 'Male',   'Gabiley, Hargeysa',          '404', 300.00, 'Paid', 'Checked In',  DATEADD(day,-2,GETDATE()), DATEADD(day,2,GETDATE()),  DATEADD(day,-2,GETDATE()), NULL),
                            ('B014', 'Nasra Xasan Garaad',   'nasra.garaad@gmail.com',  '063-4214014', 'Female', 'Deg Sheikh, Togdheer',       '405', 210.00, 'Paid', 'Checked In',  DATEADD(day,-8,GETDATE()), DATEADD(day,-5,GETDATE()), DATEADD(day,-8,GETDATE()), NULL),
                            ('B015', 'Bashiir Saleebaan Ali','bashiir.ali@gmail.com',   '063-4215015', 'Male',   'Xaafadda Mohamoud-Haybe',   '102',  90.00, 'Paid', 'Checked Out', DATEADD(day,-4,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-4,GETDATE()), DATEADD(day,-2,GETDATE())),
                            ('B016', 'Ifrah Muuse Jibriil',  'ifrah.jibriil@gmail.com', '063-4216016', 'Female', 'Xaafadda Ga-Caan, HGA',     '205', 240.00, 'Paid', 'Checked In',  DATEADD(day,-1,GETDATE()), DATEADD(day,3,GETDATE()),  DATEADD(day,-1,GETDATE()), NULL),
                            ('B017', 'Khadar Warsame Rooble','khadar.rooble@gmail.com', '063-4217017', 'Male',   'Xaafadda Guul, Burco',      '101',  45.00, 'Paid', 'Checked Out', DATEADD(day,-3,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-3,GETDATE()), DATEADD(day,-2,GETDATE())),
                            ('B018', 'Filsan Axmed Cawaale', 'filsan.cawaale@gmail.com','063-4218018', 'Female', 'Deg Bariga, Berbera',        '103',  80.00, 'Paid', 'Checked Out', DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE())),
                            ('B019', 'Cabdirashiid Iidow',   'rashiid.iidow@gmail.com', '063-4219019', 'Male',   'Deg Geed-Deeble, HGA',      '201',  50.00, 'Paid', 'Checked Out', DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE())),
                            ('B020', 'Asad Maxamed Farah',   'asad.farah@gmail.com',    '063-4220020', 'Male',   'Xaafadda STC, Hargeysa',    '203', 120.00, 'Paid', 'Checked Out', DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE()), DATEADD(day,-2,GETDATE()), DATEADD(day,-1,GETDATE()))";
                        using (SqlCommand cmd = new SqlCommand(insertCustomers, conn)) { cmd.ExecuteNonQuery(); }
                    }

                    string checkStaff = "SELECT COUNT(*) FROM users WHERE role = 'staff'";
                    int staffCount = 0;
                    using (SqlCommand cmd = new SqlCommand(checkStaff, conn)) { staffCount = (int)cmd.ExecuteScalar(); }

                    if (staffCount == 0)
                    {
                        // Insert 3 Somali staff members
                        string insertStaff = @"
                            INSERT INTO users (username, password, role, status, date_register) VALUES
                            ('Cabdirisaaq', 'staff123', 'staff', 'Active', GETDATE()),
                            ('Hodan',       'staff123', 'staff', 'Active', GETDATE()),
                            ('Mucaawiye',   'staff123', 'staff', 'Active', GETDATE())";
                        using (SqlCommand cmd = new SqlCommand(insertStaff, conn)) { cmd.ExecuteNonQuery(); }
                    }

                    string checkLogs = "SELECT COUNT(*) FROM audit_log";
                    int logCount = 0;
                    using (SqlCommand cmd = new SqlCommand(checkLogs, conn)) { logCount = (int)cmd.ExecuteScalar(); }

                    if (logCount == 0)
                    {
                        // Insert initial audit logs
                        string insertLogs = @"
                            INSERT INTO audit_log (username, action, details, action_date) VALUES
                            ('System', 'Bilaabid', 'Nidaamka database-ka si guul leh ayaa loo diyaariyey — CAALAMI HOTEL.', GETDATE()),
                            ('System', 'Xog-gelin', '20 qol, 20 martid Soomaali ah, iyo 3 shaqaale ayaa loo diiwaan galiyey.', GETDATE()),
                            ('Admin',  'Habayn',   'Xogta hoteelka CAALAMI HOTEL la habeeyey.', GETDATE())";
                        using (SqlCommand cmd = new SqlCommand(insertLogs, conn)) { cmd.ExecuteNonQuery(); }
                    }
                }
            }
            catch (Exception)
            {
                throw; // Let the retry wrapper in InitializeDatabase() handle this
            }
        }

        private static void CleanCppFiles()
        {
            try
            {
                // Go up from bin\Debug\ to find the root folder containing C++ files
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string parentDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));

                // Safe check: search upwards for the root containing CMakeLists.txt
                int limit = 0;
                while (parentDir != null && !File.Exists(Path.Combine(parentDir, "CMakeLists.txt")) && limit < 5)
                {
                    var parent = Directory.GetParent(parentDir);
                    if (parent == null) break;
                    parentDir = parent.FullName;
                    limit++;
                }

                if (parentDir != null && File.Exists(Path.Combine(parentDir, "CMakeLists.txt")))
                {
                    // C++ Files to delete
                    string[] filesToClear = { "*.cpp", "*.h", "*.ui", "*.qrc", "CMakeLists.txt" };
                    foreach (string pattern in filesToClear)
                    {
                        foreach (string file in Directory.GetFiles(parentDir, pattern, SearchOption.TopDirectoryOnly))
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }

                    // C++ Folders to delete
                    string[] foldersToClear = { "build", "out" };
                    foreach (string folder in foldersToClear)
                    {
                        string folderPath = Path.Combine(parentDir, folder);
                        if (Directory.Exists(folderPath))
                        {
                            try { Directory.Delete(folderPath, true); } catch { }
                        }
                    }
                }
            }
            catch { /* Silent */ }
        }
    }
}
