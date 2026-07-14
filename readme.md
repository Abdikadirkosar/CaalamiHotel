# 🏨 Caalami Hotel Management System

<p align="center">
  <img src="WindowsFormsApp1/assets/hotel_logo.jpg" alt="Caalami Hotel Logo" width="300"/>
</p>

<p align="center">
  <b>Comfort & Hospitality — Hotel Management System v2.0</b><br/>
  Built with C# Windows Forms + SQL Server LocalDB
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?logo=windows" />
  <img src="https://img.shields.io/badge/Language-C%23-purple?logo=csharp" />
  <img src="https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver" />
  <img src="https://img.shields.io/badge/Framework-.NET%204.8-blueviolet?logo=dotnet" />
  <img src="https://img.shields.io/badge/Version-2.0-gold" />
  <img src="https://img.shields.io/badge/Status-Active-brightgreen" />
  <img src="https://img.shields.io/badge/Location-Hargeisa%2C%20Somaliland-orange" />
</p>

---

## 📋 About

**Caalami Hotel Management System** is a full-featured desktop application built for **Caalami Hotel, Hargeisa — Somaliland**. It provides a complete solution for managing hotel rooms, bookings, guests, staff, and revenue reporting.

> 🌍 *Nidaamkan waxaa loogu talagalay maamulka hudheelka Caalami Hotel, Hargeysa — Somaliland.*

---

## ✨ Features

### 🔐 Authentication & Security
- Secure Login with **role-based access** (Admin / Staff)
- Password-protected accounts with session management
- User registration & account management
- Activity audit log (who did what, when)

### 🏠 Room Management
- Interactive **Room Map** with color-coded status tiles
- Floor tabs for floor-by-floor navigation
- 5 room types: **Standard · Deluxe · Suite · Family · VIP**
- Room statuses:
  - 🟢 `Clean / Ready` — Available for booking
  - 🟡 `Dirty / Cleaning` — Needs housekeeping
  - 🟠 `Maintenance` — Under repair
  - 🔴 `Occupied` — Guest currently staying
- **Auto-Dirty** — Room status auto-changes to Dirty after guest checkout

### 👥 Guest & Booking Management
- Full booking flow: **Book → Check-in → Stay → Check-out**
- Auto-generated **Invoice** (saved to Desktop as text file)
- Guest profile with full details (name, contact, address, gender)
- Booking history with search & filter
- 62+ guest records pre-loaded as sample data

### 📊 Dashboard & Reports
- **Revenue bar chart** (7-day rolling revenue)
- Room occupancy overview (counts by status)
- Real-time statistics cards (Rooms · Guests · Revenue · Users)
- Today's revenue at a glance

### 👤 User Management (Admin Only)
- Add / Edit / Deactivate staff accounts
- Role assignment: **Admin** or **Staff**
- View all staff with their status
- Full activity audit trail

### 💼 Staff Panel
- Simplified interface for non-admin staff
- Book rooms for guests
- View current bookings

---

## 🗃️ Database Schema

```sql
-- Users Table
users (id, username, password, role, status, date_register)

-- Rooms Table
rooms (id, room_id, type, room_name, price, image_path, status, date_register)

-- Customers / Bookings Table
customer (id, book_id, full_name, email, contact, gender, address,
          room_id, price, status_payment, status,
          date_from, date_to, date_book, checkout_date)

-- Activity Log Table
activity_log (id, username, action, details, log_time)
```

---

## 🖥️ Screenshots

| Login Screen | Admin Dashboard | Room Map |
|:---:|:---:|:---:|
| ![Login](WindowsFormsApp1/assets/icons8-hotel-door-80.png) | ![Dashboard](WindowsFormsApp1/assets/icons8-stack-of-money-60.png) | ![Rooms](WindowsFormsApp1/assets/icons8-room-60.png) |

---

## 🚀 Installation — New Computer Setup

### ✅ Requirements
- Windows 10 / 11 (64-bit)
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) *(usually pre-installed on Windows 10/11)*
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) *(free — included with Visual Studio)*

### Step 1 — Setup Database
```
2_DATABASE_SETUP\SETUP_DATABASE.bat
→ Right-click → "Run as Administrator"
→ Wait for: "DATABASE DIYAAR / SETUP COMPLETE"
```

### Step 2 — Run the App
```
1_APP_FUR_HALKAN\CaalamiHotel_App.exe
→ Double-click to launch
→ Username : admin
→ Password : admin123
```

> ⚠️ **Important:** Always run the database setup (Step 1) before launching the app on a new computer.

---

## 🏗️ Build from Source

### Requirements
- Visual Studio 2022 (Community edition is free)
- .NET Framework 4.8 SDK

### Steps
```bash
# 1. Clone the repository
git clone https://github.com/Abdikadirkosar/CaalamiHotel.git

# 2. Navigate to the project
cd CaalamiHotel/WindowsFormsApp1

# 3. Open solution in Visual Studio
# → Double-click "Hotel Management System 2.sln"

# 4. Build
# → Build menu → Build Solution (Ctrl+Shift+B)

# 5. Run
# → F5 or click the green ▶ button
```

---

## 🗂️ Project Structure

```
CaalamiHotel/
│
├── WindowsFormsApp1/               # Main C# WinForms project
│   ├── AdminMainForm.cs            # Admin main navigation panel
│   ├── staffMainForm.cs            # Staff main navigation panel
│   ├── Form1.cs                    # Login screen
│   ├── SplashForm.cs               # Splash / loading screen
│   ├── admin_dashboard.cs          # Dashboard + revenue charts
│   ├── admin_rooms.cs              # Room management + interactive map
│   ├── admin_customers.cs          # Guest & booking management
│   ├── admin_addUser.cs            # User management (admin only)
│   ├── staff_bookRoom.cs           # Staff booking form
│   ├── clientinfo.cs               # Guest detail view
│   ├── hotelData.cs                # DB connection + invoice generator
│   ├── customersData.cs            # Customer data model
│   ├── roomsData.cs                # Room data model
│   ├── usersData.cs                # User data model
│   ├── Program.cs                  # App entry + DB auto-setup
│   ├── UiHelper.cs                 # UI helper utilities
│   └── assets/                     # Logo + icons
│       ├── hotel_logo.jpg          # Caalami Hotel logo
│       └── icons8-*.png            # Dashboard icons
│
├── SQLQuery1.sql                   # Database schema reference
├── Hotel Management System 2.sln  # Visual Studio solution file
└── README.md
```

---

## 🛠️ Tech Stack

| Component | Technology |
|---|---|
| Language | C# (.NET Framework 4.8) |
| UI Framework | Windows Forms (WinForms) |
| Database | SQL Server LocalDB (free) |
| Charts | Custom GDI+ drawing |
| Invoice | Text-based invoice generator |
| IDE | Visual Studio 2022 |
| Version Control | Git + GitHub |

---

## 🔄 Room Status Lifecycle

```
🟢 Clean / Ready
      ↓  Guest books a room
🔴 Occupied  (set automatically on booking)
      ↓  Guest checks out
🟡 Dirty / Cleaning  (set AUTOMATICALLY on checkout)
      ↓  Housekeeping staff cleans the room
      ↓  Staff manually updates status
🟢 Clean / Ready  (ready for next booking)
```

> 💡 The system **automatically** marks rooms as Dirty after checkout — preventing re-booking before cleaning is complete.

---

## 📦 Default Data (Pre-loaded)

| Data | Count |
|---|---|
| 👤 User Accounts | 8 (1 admin + 7 staff) |
| 🏠 Rooms | 32 (across 5 types) |
| 📋 Guest Records | 62 bookings |

**Room Types & Prices (sample):**

| Type | Price/Night |
|---|---|
| Standard | $30 – $50 |
| Deluxe | $60 – $80 |
| Suite | $100 – $150 |
| Family | $80 – $120 |
| VIP | $150 – $200 |

---

## 🐛 Troubleshooting

| Problem | Solution |
|---|---|
| `Cannot connect to database` | Run `SETUP_DATABASE.bat` as Administrator |
| App won't launch | Install [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) |
| Login fails | Username: `admin` / Password: `admin123` |
| Room map is empty | Check database connection in `hotelData.cs` |
| Build errors in VS | Right-click project → Restore NuGet Packages |

---

## 📝 Changelog

### v2.0 — July 2026
- ✅ Rebranded from Maansoor Hotel → **Caalami Hotel**
- ✅ New professional logo (Gold & Navy)
- ✅ Modern dark-navy UI redesign
- ✅ Interactive Room Map with floor tabs
- ✅ Auto-Dirty room status on checkout
- ✅ Revenue bar chart (7-day)
- ✅ User Management module
- ✅ 100 sample data records added
- ✅ USB deployment package

### v1.0 — June 2026
- ✅ Initial hotel management system
- ✅ Basic room & booking management
- ✅ Login system
- ✅ Invoice generation

---

## 👨‍💻 Developer

<table>
  <tr>
    <td align="center">
      <b>Abdikadir Kosar</b><br/>
      📧 abdikadirkosara@gmail.com<br/>
      🌍 Hargeisa, Somaliland<br/>
      🐙 <a href="https://github.com/Abdikadirkosar">github.com/Abdikadirkosar</a>
    </td>
  </tr>
</table>

---

## 📄 License

This project is developed for **Caalami Hotel, Hargeisa — Somaliland**.  
All rights reserved © 2026 Caalami Hotel.

---

<p align="center">
  <b>🏨 Caalami Hotel — Comfort & Hospitality</b><br/>
  <i>Hargeisa, Somaliland 🌍</i>
</p>