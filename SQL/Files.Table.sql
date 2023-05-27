IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Files')
BEGIN
    CREATE TABLE dbo.Files (
        [Name] NVARCHAR(450) PRIMARY KEY,
        [Container] NVARCHAR(255) NULL,
        [Created] DATETIME NOT NULL,
        [Modified] DATETIME NOT NULL,
        [Length] BIGINT NOT NULL,
        [Status] INT DEFAULT 0,
        [Namespace] NVARCHAR(255) NULL,
        [BatchId] NVARCHAR(1024) NULL,
        [Text] TEXT NULL,
        [Completed] BIT NOT NULL,
        [Faulted] BIT NOT NULL,
        [TryCount] INT DEFAULT 0,
        [Timestamp] DATETIME DEFAULT GETDATE()
    );
END


