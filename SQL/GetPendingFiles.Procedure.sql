CREATE OR ALTER PROCEDURE GetPendingFiles
    @SizeInMB INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH CumulativeSize AS (
        SELECT [Name], Container, Created, Modified, [Length], [Status], [Namespace], BatchId, [Text], Completed, Faulted, TryCount, SUM([Length]) OVER (ORDER BY Created ASC) AS TotalSize
        FROM Files
        WHERE [Status] = 1 AND TryCount < 10
    )
    SELECT [Name], Container, Created, Modified, [Length], [Status], [Namespace], BatchId, [Text], Completed, Faulted, TryCount, TotalSize
    FROM CumulativeSize
    WHERE TotalSize <= @SizeInMB * 1024 * 1024
    ORDER BY Created ASC;
END
