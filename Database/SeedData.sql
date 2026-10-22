-- ============================================================================
-- Database: CRN_TechSoft_DB
-- Script: SeedData.sql
-- Description: Seeds initial test & demonstration data for Products, Items,
--              and a default Admin user.
-- Target RDBMS: Microsoft SQL Server 2017+ / Azure SQL / SQL Server LocalDB
-- ============================================================================

USE [CRN_TechSoft_DB];
GO

-- ============================================================================
-- 1. SEED DEFAULT USER
-- Credentials:
--   Username : admin
--   Password : Password123!
--   Role     : Admin
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = N'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [PasswordHash], [Role], [CreatedAt])
    VALUES 
    (
        N'admin', 
        N'admin@crntech.com', 
        -- BCrypt hash for 'Password123!' (work factor 11)
        N'$2a$11$eaccYoNOOZqCrqv27VoC.u9hDpsM2f12xN7tZtqgGk.K5m8D5P41C', 
        N'Admin', 
        GETUTCDATE()
    );

    PRINT 'Default Admin user created:';
    PRINT '  - Username : admin';
    PRINT '  - Password : Password123!';
    PRINT '  - Role     : Admin';
END
ELSE
BEGIN
    PRINT 'Admin user already exists.';
END
GO

-- ============================================================================
-- 2. SEED SAMPLE PRODUCTS
-- ============================================================================
SET IDENTITY_INSERT [dbo].[Product] ON;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [Id] = 1)
BEGIN
    INSERT INTO [dbo].[Product] ([Id], [ProductName], [CreatedBy], [CreatedOn], [ModifiedBy], [ModifiedOn])
    VALUES 
    (1, N'Dell UltraSharp 27 4K Monitor (U2723QE)', N'admin', DATEADD(DAY, -14, GETUTCDATE()), NULL, NULL),
    (2, N'Logitech MX Master 3S Wireless Mouse', N'admin', DATEADD(DAY, -12, GETUTCDATE()), NULL, NULL),
    (3, N'Keychron K2 V2 Wireless Mechanical Keyboard', N'admin', DATEADD(DAY, -10, GETUTCDATE()), NULL, NULL),
    (4, N'Apple MacBook Pro 16-inch M3 Max (36GB/1TB)', N'admin', DATEADD(DAY, -8, GETUTCDATE()), NULL, NULL),
    (5, N'Sony WH-1000XM5 Wireless Noise Canceling Headphones', N'admin', DATEADD(DAY, -7, GETUTCDATE()), NULL, NULL),
    (6, N'Samsung T7 Shield 2TB Rugged Portable SSD', N'admin', DATEADD(DAY, -5, GETUTCDATE()), NULL, NULL),
    (7, N'Anker Prime 20,000mAh Power Bank (200W Output)', N'admin', DATEADD(DAY, -4, GETUTCDATE()), NULL, NULL),
    (8, N'Autonomous ErgoChair Pro (Cool Gray)', N'admin', DATEADD(DAY, -3, GETUTCDATE()), NULL, NULL),
    (9, N'Ergotron LX Desk Mount Dual Monitor Arm', N'admin', DATEADD(DAY, -2, GETUTCDATE()), NULL, NULL),
    (10, N'CalDigit TS4 Thunderbolt 4 Docking Station (18 Ports)', N'admin', DATEADD(DAY, -1, GETUTCDATE()), NULL, NULL);

    PRINT '10 sample Products inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Products already seeded.';
END
GO

SET IDENTITY_INSERT [dbo].[Product] OFF;
GO

-- ============================================================================
-- 3. SEED INVENTORY STOCK ITEMS (Sub-resources of Products)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Item] WHERE [ProductId] = 1)
BEGIN
    INSERT INTO [dbo].[Item] ([ProductId], [Quantity])
    VALUES
    (1, 15), (1, 20),           -- Monitor batches: Warehouse A (15), Warehouse B (20)
    (2, 50), (2, 75),           -- Mouse batches: Warehouse A (50), Warehouse B (75)
    (3, 40),                    -- Keyboard batch: 40 units
    (4, 8),  (4, 12),           -- MacBook batches: 8 + 12 = 20 units
    (5, 30),                    -- Headphones: 30 units
    (6, 60), (6, 45),           -- SSD: 105 units
    (7, 85),                    -- Power Bank: 85 units
    (8, 14),                    -- Chairs: 14 units
    (9, 22),                    -- Monitor arms: 22 units
    (10, 18);                   -- Docks: 18 units

    PRINT 'Inventory stock Items seeded for products successfully.';
END
ELSE
BEGIN
    PRINT 'Inventory Items already seeded.';
END
GO

PRINT '=========================================================';
PRINT 'Data seeding completed successfully!';
PRINT '=========================================================';
