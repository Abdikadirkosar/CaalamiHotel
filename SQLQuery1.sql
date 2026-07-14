-- ============================================================
-- HOTEL MANAGEMENT SYSTEM - COMPLETE DATABASE SETUP SCRIPT
-- RUN THIS IN: SQL Server Management Studio (SSMS)
-- DATABASE: hotel1.mdf (LocalDB)
-- LOGIN: Username = Admin  |  Password = admin
-- ============================================================

-- STEP 1: Create Tables
-- ============================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U')
BEGIN
    CREATE TABLE users
    (
        id            INT PRIMARY KEY IDENTITY(1,1),
        username      VARCHAR(100) NOT NULL,
        password      VARCHAR(100) NOT NULL,
        role          VARCHAR(50)  NOT NULL,
        status        VARCHAR(50)  NOT NULL,
        date_register DATE         NULL
    );
    PRINT 'users table created.';
END
ELSE PRINT 'users table already exists.';
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='rooms' AND xtype='U')
BEGIN
    CREATE TABLE rooms
    (
        id            INT PRIMARY KEY IDENTITY(1,1),
        room_id       VARCHAR(50)  NULL,
        type          VARCHAR(100) NULL,
        room_name     VARCHAR(100) NULL,
        price         FLOAT        NULL,
        image_path    VARCHAR(500) NULL,
        status        VARCHAR(50)  NULL,   -- Active, Unavailable, Maintenance
        date_register DATE         NULL,
        date_update   DATE         NULL,
        date_delete   DATE         NULL
    );
    PRINT 'rooms table created.';
END
ELSE PRINT 'rooms table already exists.';
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='customer' AND xtype='U')
BEGIN
    CREATE TABLE customer
    (
        id             INT PRIMARY KEY IDENTITY(1,1),
        book_id        VARCHAR(50)   NULL,
        full_name      VARCHAR(200)  NULL,
        email          VARCHAR(200)  NULL,
        contact        VARCHAR(50)   NULL,
        gender         VARCHAR(20)   NULL,
        address        VARCHAR(300)  NULL,
        room_id        VARCHAR(50)   NULL,
        price          DECIMAL(10,2) NULL,
        status_payment VARCHAR(50)   NULL,
        status         VARCHAR(50)   NULL,  -- Checked In, Checked Out
        date_from      DATE          NULL,
        date_to        DATE          NULL,
        date_book      DATE          NULL,
        checkout_date  DATE          NULL   -- actual checkout date
    );
    PRINT 'customer table created.';
END
ELSE
BEGIN
    -- Add checkout_date if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customer') AND name = 'checkout_date')
    BEGIN
        ALTER TABLE customer ADD checkout_date DATE NULL;
        PRINT 'checkout_date column added to customer.';
    END
END
GO

-- Audit Log Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='audit_log' AND xtype='U')
BEGIN
    CREATE TABLE audit_log
    (
        id          INT PRIMARY KEY IDENTITY(1,1),
        username    VARCHAR(100) NOT NULL,
        action      VARCHAR(200) NOT NULL,
        details     VARCHAR(500) NULL,
        action_date DATETIME     DEFAULT GETDATE()
    );
    PRINT 'audit_log table created.';
END
ELSE PRINT 'audit_log table already exists.';
GO

-- ============================================================
-- STEP 2: Insert Admin User
-- Username: Admin  |  Password: admin
-- ============================================================
IF NOT EXISTS (SELECT * FROM users WHERE username = 'Admin' AND role = 'Admin')
BEGIN
    INSERT INTO users (username, password, role, status, date_register)
    VALUES ('Admin', 'admin', 'Admin', 'Active', CAST(GETDATE() AS DATE));
    PRINT 'Admin user created! Login: Admin / admin';
END
ELSE
BEGIN
    UPDATE users SET password = 'admin', status = 'Active'
    WHERE username = 'Admin' AND role = 'Admin';
    PRINT 'Admin user already exists - password confirmed: admin';
END
GO

-- ============================================================
-- STEP 3: Verification
-- ============================================================
SELECT 'users'     AS TableName, COUNT(*) AS RowCount FROM users    UNION ALL
SELECT 'rooms'     AS TableName, COUNT(*) AS RowCount FROM rooms     UNION ALL
SELECT 'customer'  AS TableName, COUNT(*) AS RowCount FROM customer  UNION ALL
SELECT 'audit_log' AS TableName, COUNT(*) AS RowCount FROM audit_log;

SELECT id, username, password, role, status, date_register FROM users;

PRINT '';
PRINT 'SETUP COMPLETE! Login: Admin / admin';