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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4011,        -- ItemNumber - int
    'Banana',       -- ItemName - varchar(50)
    'Box of Green Bananas',       -- ItemDescription - varchar(250)
    112.19,     -- PurchasePrice - decimal(10, 2)
    50,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )

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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4011,        -- ItemNumber - int
    'Banana',       -- ItemName - varchar(50)
    'Box of Green Bananas',       -- ItemDescription - varchar(250)
    112.19,     -- PurchasePrice - decimal(10, 2)
    50,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )

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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4011,        -- ItemNumber - int
    'Banana',       -- ItemName - varchar(50)
    'Box of Green Bananas',       -- ItemDescription - varchar(250)
    112.19,     -- PurchasePrice - decimal(10, 2)
    50,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4035,        -- ItemNumber - int
    'Gala Apple',       -- ItemName - varchar(50)
    'Bag of gala apples',       -- ItemDescription - varchar(250)
    212.33,     -- PurchasePrice - decimal(10, 2)
    125,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4030,        -- ItemNumber - int
    'Kiwis',       -- ItemName - varchar(50)
    'Bag of kiwis',       -- ItemDescription - varchar(250)
    153.88,     -- PurchasePrice - decimal(10, 2)
    100,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4035,        -- ItemNumber - int
    'Gala Apple',       -- ItemName - varchar(50)
    'Bag of gala apples',       -- ItemDescription - varchar(250)
    212.33,     -- PurchasePrice - decimal(10, 2)
    125,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )


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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4030,        -- ItemNumber - int
    'Kiwis',       -- ItemName - varchar(50)
    'Bag of kiwis',       -- ItemDescription - varchar(250)
    109.88,     -- PurchasePrice - decimal(10, 2)
    76,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )

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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4011,        -- ItemNumber - int
    'Banana',       -- ItemName - varchar(50)
    'Box of Green Bananas',       -- ItemDescription - varchar(250)
    67.45,     -- PurchasePrice - decimal(10, 2)
    26,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4011,        -- ItemNumber - int
    'Banana',       -- ItemName - varchar(50)
    'Box of Green Bananas',       -- ItemDescription - varchar(250)
    67.45,     -- PurchasePrice - decimal(10, 2)
    26,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4030,        -- ItemNumber - int
    'Kiwis',       -- ItemName - varchar(50)
    'Bag of kiwis',       -- ItemDescription - varchar(250)
    122.88,     -- PurchasePrice - decimal(10, 2)
    90,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112334',       -- PurchaseOrderNumber - varchar(20)
    4030,        -- ItemNumber - int
    'Kiwis',       -- ItemName - varchar(50)
    'Bag of kiwis',       -- ItemDescription - varchar(250)
    153.88,     -- PurchasePrice - decimal(10, 2)
    100,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )
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
(   '112335',       -- PurchaseOrderNumber - varchar(20)
    4035,        -- ItemNumber - int
    'Gala Apple',       -- ItemName - varchar(50)
    'Bag of gala apples',       -- ItemDescription - varchar(250)
    212.33,     -- PurchasePrice - decimal(10, 2)
    125,        -- PurchaseQuantity - int
    'system',       -- LastModifiedByUser - varchar(50)
    GETDATE() -- LastModifiedDateTime - datetime
    )


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
END CATCH;

--IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
GO