-- ===============================================
-- SQL Server Express Setup Script for Alex Lee Exercise
-- This script creates stored procedures required for the exercise
-- ===============================================

USE AlexLeeDB;
GO

-- ===============================================
-- Stored Procedure for Question #6: 
-- List all purchase detail records with line numbers
-- ===============================================
IF OBJECT_ID('dbo.GetPurchaseDetailsWithLineNumbers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetPurchaseDetailsWithLineNumbers;
GO

CREATE PROCEDURE dbo.GetPurchaseDetailsWithLineNumbers
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Query that implements line numbering per item per purchase order (Question #4)
    SELECT 
        PurchaseDetailItemAutoId,
        PurchaseOrderNumber,
        ItemNumber,
        ItemName,
        ItemDescription,
        PurchasePrice,
        PurchaseQuantity,
        LastModifiedByUser,
        LastModifiedDateTime,
        ROW_NUMBER() OVER (
            PARTITION BY PurchaseOrderNumber, ItemNumber 
            ORDER BY PurchaseDetailItemAutoId
        ) AS LineNumber
    FROM dbo.PurchaseDetailItem
    ORDER BY 
        PurchaseOrderNumber,
        ItemNumber,
        LineNumber;
END
GO

-- ===============================================
-- Query for Question #5: 
-- Identify duplicate records with same purchase order, item number, price, and quantity
-- ===============================================
IF OBJECT_ID('dbo.GetDuplicatePurchaseDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetDuplicatePurchaseDetails;
GO

CREATE PROCEDURE dbo.GetDuplicatePurchaseDetails
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Find duplicates based on the specified criteria
    SELECT 
        PurchaseDetailItemAutoId,
        PurchaseOrderNumber,
        ItemNumber,
        ItemName,
        ItemDescription,
        PurchasePrice,
        PurchaseQuantity,
        LastModifiedByUser,
        LastModifiedDateTime,
        COUNT(*) OVER (
            PARTITION BY PurchaseOrderNumber, ItemNumber, PurchasePrice, PurchaseQuantity
        ) AS DuplicateCount
    FROM dbo.PurchaseDetailItem
    WHERE EXISTS (
        SELECT 1
        FROM dbo.PurchaseDetailItem AS pdi2
        WHERE pdi2.PurchaseOrderNumber = PurchaseDetailItem.PurchaseOrderNumber
          AND pdi2.ItemNumber = PurchaseDetailItem.ItemNumber
          AND pdi2.PurchasePrice = PurchaseDetailItem.PurchasePrice
          AND pdi2.PurchaseQuantity = PurchaseDetailItem.PurchaseQuantity
        GROUP BY pdi2.PurchaseOrderNumber, pdi2.ItemNumber, pdi2.PurchasePrice, pdi2.PurchaseQuantity
        HAVING COUNT(*) > 1
    )
    ORDER BY 
        PurchaseOrderNumber,
        ItemNumber,
        PurchaseDetailItemAutoId;
END
GO

-- ===============================================
-- Utility procedure to check if database is ready
-- ===============================================
IF OBJECT_ID('dbo.CheckDatabaseReady', 'P') IS NOT NULL
    DROP PROCEDURE dbo.CheckDatabaseReady;
GO

CREATE PROCEDURE dbo.CheckDatabaseReady
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Database Ready' AS Status,
        COUNT(*) AS RecordCount
    FROM dbo.PurchaseDetailItem;
END
GO

-- ===============================================
-- Grant permissions (if needed for application user)
-- ===============================================
-- Note: In a production environment, you would create a dedicated application user
-- For this exercise, we'll use SA but this is not recommended for production

PRINT 'Alex Lee SQL Server setup completed successfully!';
PRINT 'Available stored procedures:';
PRINT '  - dbo.GetPurchaseDetailsWithLineNumbers (Question #6)';
PRINT '  - dbo.GetDuplicatePurchaseDetails (Question #5)';
PRINT '  - dbo.CheckDatabaseReady (Utility)';
GO