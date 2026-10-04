-- ============================================================
-- Run this ONCE in SQL Server Management Studio (SSMS) on your
-- ArtGallery database, BEFORE running the updated project.
-- This only ADDS a new table; it does not touch/change/delete
-- any of your existing tables or data.
-- ============================================================

USE ArtGallery;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    CREATE TABLE PaymentTransactions (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NULL,
        OrderNumber NVARCHAR(50) NULL,
        Gateway NVARCHAR(50) NULL,              -- 'JazzCash', 'Stripe', 'CashOnDelivery'
        TransactionReference NVARCHAR(200) NULL, -- gateway transaction id / dummy reference number
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NULL,                -- 'Success' or 'Failed'
        CustomerEmail NVARCHAR(200) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_PaymentTransaction_Order FOREIGN KEY (OrderId)
            REFERENCES [Order](OrderId)
    );

    PRINT 'PaymentTransactions table created successfully.';
END
ELSE
BEGIN
    PRINT 'PaymentTransactions table already exists - nothing to do.';
END
GO
