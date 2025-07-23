-- Create ApplicationLogs table for structured logging
-- This script will be integrated into the existing init-database.sql

-- Create ApplicationLogs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ApplicationLogs' AND type = 'U')
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
END
ELSE
BEGIN
    PRINT 'ApplicationLogs table already exists.';
END

GO

-- Insert a test log entry to verify the table works
INSERT INTO [dbo].[ApplicationLogs] 
    ([Level], [Category], [Message], [MachineName], [ProcessId])
VALUES 
    ('Information', 'AlexLee.Database.Migration', 'ApplicationLogs table initialized successfully', HOST_NAME(), @@SPID);

GO

PRINT 'ApplicationLogs table setup completed.';