-- ===============================================
-- Database Initialization Script for Alex Lee Exercise
-- This script sets up the complete database structure and data using the original SQLExerciseScript.sql
-- ===============================================

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AlexLeeDB')
BEGIN
    CREATE DATABASE AlexLeeDB;
    PRINT 'AlexLeeDB database created successfully!';
END
ELSE
BEGIN
    PRINT 'AlexLeeDB database already exists.';
END
GO

-- Switch to the AlexLeeDB database
USE AlexLeeDB;
GO

-- Check if the table already exists and has data
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PurchaseDetailItem')
BEGIN
    PRINT 'Creating PurchaseDetailItem table and loading data from SQLExerciseScript.sql...';
    
    -- Execute the original SQLExerciseScript.sql content
    BEGIN TRANSACTION;

    BEGIN TRY

    CREATE TABLE dbo.PurchaseDetailItem 
    (
        PurchaseDetailItemAutoId BIGINT IDENTITY(1, 1) NOT NULL,
        PurchaseOrderNumber VARCHAR(20) NOT NULL,
        ItemNumber INT NOT NULL,
        ItemName VARCHAR(50) NOT NULL,
        ItemDescription VARCHAR(250) NULL,
        PurchasePrice DECIMAL(10, 2) NOT NULL,
        PurchaseQuantity INT NOT NULL,
        LastModifiedByUser VARCHAR(50) NOT NULL,
        LastModifiedDateTime DATETIME NOT NULL,
        CONSTRAINT PK_PurchaseDetailItem PRIMARY KEY (PurchaseDetailItemAutoId)
    )

    -- Insert all data from the original SQLExerciseScript.sql
    INSERT INTO dbo.PurchaseDetailItem
    (
        PurchaseOrderNumber,
        ItemNumber,
        ItemName,
        ItemDescription,
        PurchasePrice,
        PurchaseQuantity,
        LastModifiedByUser,
        LastModifiedDateTime
    )
    VALUES
    ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
    ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
    ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
    ('112335', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE()),
    ('112334', 4030, 'Kiwis', 'Bag of kiwis', 153.88, 100, 'system', GETDATE()),
    ('112334', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE()),
    ('112335', 4030, 'Kiwis', 'Bag of kiwis', 109.88, 76, 'system', GETDATE()),
    ('112335', 4011, 'Banana', 'Box of Green Bananas', 67.45, 26, 'system', GETDATE()),
    ('112335', 4011, 'Banana', 'Box of Green Bananas', 67.45, 26, 'system', GETDATE()),
    ('112335', 4030, 'Kiwis', 'Bag of kiwis', 122.88, 90, 'system', GETDATE()),
    ('112334', 4030, 'Kiwis', 'Bag of kiwis', 153.88, 100, 'system', GETDATE()),
    ('112335', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE());

    PRINT 'PurchaseDetailItem table created and populated with 12 records from SQLExerciseScript.sql!';

    END TRY
    BEGIN CATCH
        SELECT
            ERROR_NUMBER() AS ErrorNumber
           ,ERROR_SEVERITY() AS ErrorSeverity
           ,ERROR_STATE() AS ErrorState
           ,ERROR_PROCEDURE() AS ErrorProcedure
           ,ERROR_LINE() AS ErrorLine
           ,ERROR_MESSAGE() AS ErrorMessage;

        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
END
ELSE
BEGIN
    -- Check if we have data
    DECLARE @RecordCount INT;
    SELECT @RecordCount = COUNT(*) FROM dbo.PurchaseDetailItem;
    
    IF @RecordCount = 0
    BEGIN
        PRINT 'Table exists but is empty. Loading data from SQLExerciseScript.sql...';
        
        -- Insert the data
        INSERT INTO dbo.PurchaseDetailItem
        (
            PurchaseOrderNumber,
            ItemNumber,
            ItemName,
            ItemDescription,
            PurchasePrice,
            PurchaseQuantity,
            LastModifiedByUser,
            LastModifiedDateTime
        )
        VALUES
        ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
        ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
        ('112334', 4011, 'Banana', 'Box of Green Bananas', 112.19, 50, 'system', GETDATE()),
        ('112335', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE()),
        ('112334', 4030, 'Kiwis', 'Bag of kiwis', 153.88, 100, 'system', GETDATE()),
        ('112334', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE()),
        ('112335', 4030, 'Kiwis', 'Bag of kiwis', 109.88, 76, 'system', GETDATE()),
        ('112335', 4011, 'Banana', 'Box of Green Bananas', 67.45, 26, 'system', GETDATE()),
        ('112335', 4011, 'Banana', 'Box of Green Bananas', 67.45, 26, 'system', GETDATE()),
        ('112335', 4030, 'Kiwis', 'Bag of kiwis', 122.88, 90, 'system', GETDATE()),
        ('112334', 4030, 'Kiwis', 'Bag of kiwis', 153.88, 100, 'system', GETDATE()),
        ('112335', 4035, 'Gala Apple', 'Bag of gala apples', 212.33, 125, 'system', GETDATE());
        
        PRINT 'Data loaded successfully! 12 records inserted from SQLExerciseScript.sql.';
    END
    ELSE
    BEGIN
        PRINT 'Table already exists and contains data. Record count: ' + CAST(@RecordCount AS VARCHAR(10));
    END
END
GO

-- ===============================================
-- Application Logs Table Setup
-- ===============================================

-- Create ApplicationLogs table for structured logging
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ApplicationLogs')
BEGIN
    CREATE TABLE [dbo].[ApplicationLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Level] NVARCHAR(50) NOT NULL,
        [Category] NVARCHAR(200) NOT NULL,
        [Message] NVARCHAR(MAX) NOT NULL,
        [Exception] NVARCHAR(MAX) NULL,
        [Properties] NVARCHAR(MAX) NULL,
        [TraceId] NVARCHAR(32) NULL,
        [SpanId] NVARCHAR(16) NULL,
        [UserId] NVARCHAR(100) NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [MachineName] NVARCHAR(100) NOT NULL DEFAULT HOST_NAME(),
        [ProcessId] INT NOT NULL DEFAULT @@SPID,
        
        CONSTRAINT [PK_ApplicationLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Create indexes for performance
    CREATE INDEX [IX_ApplicationLogs_Level_Timestamp] 
    ON [dbo].[ApplicationLogs] ([Level] ASC, [Timestamp] DESC);

    CREATE INDEX [IX_ApplicationLogs_Category_Timestamp] 
    ON [dbo].[ApplicationLogs] ([Category] ASC, [Timestamp] DESC);

    CREATE INDEX [IX_ApplicationLogs_TraceId] 
    ON [dbo].[ApplicationLogs] ([TraceId] ASC);

    PRINT 'ApplicationLogs table created successfully with indexes.';
    
    -- Insert a test log entry
    INSERT INTO [dbo].[ApplicationLogs] 
        ([Level], [Category], [Message])
    VALUES 
        ('Information', 'AlexLee.Database.Migration', 'ApplicationLogs table initialized successfully');
        
    PRINT 'Test log entry inserted.';
END
ELSE
BEGIN
    PRINT 'ApplicationLogs table already exists.';
END
GO

PRINT 'Database initialization completed successfully!';
GO