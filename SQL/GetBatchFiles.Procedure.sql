CREATE OR ALTER PROCEDURE GetBatchFiles
    @BatchId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * 
    FROM Files
    WHERE BatchId = @BatchId
    ORDER BY TryCount ASC;
END
