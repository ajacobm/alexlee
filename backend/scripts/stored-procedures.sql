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
    WITH DuplicateGroups AS (
    SELECT 
        PurchaseOrderNumber, 
        ItemNumber, 
        PurchasePrice, 
        PurchaseQuantity,
        COUNT(*) AS DuplicateCount
    FROM dbo.PurchaseDetailItem
    GROUP BY PurchaseOrderNumber, ItemNumber, PurchasePrice, PurchaseQuantity
    HAVING COUNT(*) > 1
    )
    SELECT 
        pdi.PurchaseDetailItemAutoId,
        pdi.PurchaseOrderNumber,
        pdi.ItemNumber,
        pdi.ItemName,
        pdi.ItemDescription,
        pdi.PurchasePrice,
        pdi.PurchaseQuantity,
        pdi.LastModifiedByUser,
        pdi.LastModifiedDateTime,
        dg.DuplicateCount
    FROM dbo.PurchaseDetailItem pdi
    JOIN DuplicateGroups dg
        ON pdi.PurchaseOrderNumber = dg.PurchaseOrderNumber
        AND pdi.ItemNumber = dg.ItemNumber
        AND pdi.PurchasePrice = dg.PurchasePrice
        AND pdi.PurchaseQuantity = dg.PurchaseQuantity
    ORDER BY 
        pdi.PurchaseOrderNumber,
        pdi.ItemNumber,
        pdi.PurchaseDetailItemAutoId;
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