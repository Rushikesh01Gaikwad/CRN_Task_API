-- ============================================================================
-- Database: CRN_TechSoft_DB
-- Script: CreateDatabase.sql
-- Description: Creates the Database, Tables, Primary Keys, Foreign Keys,
--              and Performance Indexes for the CRN Technical Solution.
-- Target RDBMS: Microsoft SQL Server 2017+ / Azure SQL / SQL Server LocalDB
-- ============================================================================

-- 1. Create Database if it does not already exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CRN_TechSoft_DB')
BEGIN
    CREATE DATABASE [CRN_TechSoft_DB];
    PRINT 'Database [CRN_TechSoft_DB] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [CRN_TechSoft_DB] already exists.';
END
GO

USE [CRN_TechSoft_DB];
GO

-- 2. Create Table: [dbo].[Product] (Assessment Specification)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Product]
    (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
        [ProductName] NVARCHAR(255) NOT NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedOn] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy] NVARCHAR(100) NULL,
        [ModifiedOn] DATETIME NULL
    );
    PRINT 'Table [dbo].[Product] created.';
END
GO

-- 3. Create Table: [dbo].[Item] (Assessment Specification)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Item]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Item]
    (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
        [ProductId] INT NOT NULL,
        [Quantity] INT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Item_Product] FOREIGN KEY ([ProductId]) 
            REFERENCES [dbo].[Product]([Id]) ON DELETE CASCADE
    );
    PRINT 'Table [dbo].[Item] created.';
END
GO

-- 4. Create Table: [dbo].[Users] (JWT Authentication System)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
        [Username] NVARCHAR(50) NOT NULL UNIQUE,
        [Email] NVARCHAR(150) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [Role] NVARCHAR(20) NOT NULL DEFAULT 'User',
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table [dbo].[Users] created.';
END
GO

-- 5. Create Table: [dbo].[RefreshTokens] (Token Rotation Strategy)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens]
    (
        [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
        [UserId] INT NOT NULL,
        [Token] NVARCHAR(256) NOT NULL,
        [ExpiresOn] DATETIME NOT NULL,
        [CreatedOn] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [RevokedOn] DATETIME NULL,
        [ReplacedByToken] NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );
    PRINT 'Table [dbo].[RefreshTokens] created.';
END
GO

-- 6. Performance Indexes (Query Optimization Strategy)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Item_ProductId' AND object_id = OBJECT_ID(N'[dbo].[Item]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Item_ProductId] ON [dbo].[Item]([ProductId]);
    PRINT 'Index [IX_Item_ProductId] created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Product_ProductName' AND object_id = OBJECT_ID(N'[dbo].[Product]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Product_ProductName] ON [dbo].[Product]([ProductName]);
    PRINT 'Index [IX_Product_ProductName] created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_Username' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
    PRINT 'Index [IX_Users_Username] created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_RefreshTokens_Token' AND object_id = OBJECT_ID(N'[dbo].[RefreshTokens]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens]([Token]);
    PRINT 'Index [IX_RefreshTokens_Token] created.';
END
GO

PRINT '=========================================================';
PRINT 'CRN_TechSoft_DB schema setup completed successfully!';
PRINT '=========================================================';
