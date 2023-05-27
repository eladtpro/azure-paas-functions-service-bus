CREATE OR ALTER PROCEDURE GetLeftoverFiles
    @Count INT,
    @Status INT,
    @ThresholdHours INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ThresholdDateTime DATETIME;
    SET @ThresholdDateTime = DATEADD(HOUR, -@ThresholdHours, GETDATE());

    SELECT TOP (@Count) *
    FROM Files
    WHERE
        [Status] = @Status AND
        Modified >= @ThresholdDateTime
    ORDER BY Modified DESC;
END
