-- ===============================================
-- Database Initialization Script for Alex Lee Exercise
-- This script sets up the complete database structure and data
-- ===============================================

-- Create the database
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

-- Check if the table already exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PurchaseDetailItem' AND xtype='U')
BEGIN
    -- Run the original SQLExerciseScript.sql content
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
        LastModifiedDateTime DATETIME NOT NULL
    )

    -- Add primary key constraint
    ALTER TABLE dbo.PurchaseDetailItem ADD CONSTRAINT PK_PurchaseDetailItem PRIMARY KEY (PurchaseDetailItemAutoId);

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

    PRINT 'PurchaseDetailItem table created and populated successfully!';

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
    PRINT 'PurchaseDetailItem table already exists.';
END
GO

PRINT 'Database initialization completed successfully!';
GO