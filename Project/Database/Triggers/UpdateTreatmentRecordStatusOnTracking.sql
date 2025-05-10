CREATE TRIGGER TR_TreatmentRecord_UpdateStatusOnTracking
ON TreatmentTracking
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update TreatmentRecord status to Cancelled when there are 3 consecutive "No Show" records
    UPDATE tr
    SET tr.Status = 3 -- Assuming 3 is the value for "Cancelled" status
    FROM TreatmentRecord tr
    INNER JOIN TreatmentRecordDetail trd ON tr.Id = trd.TreatmentRecordId
    INNER JOIN TreatmentTracking tt ON tt.TreatmentRecordDetailId = trd.Id
    WHERE tt.Status = 2 -- Assuming 2 is the value for "No Show" status
    AND tt.TrackingDate >= DATEADD(DAY, -3, GETDATE()) -- Check last 3 days
    AND tr.Status != 3 -- Only update if not already cancelled
    AND EXISTS (
        -- Check if there are 3 consecutive "No Show" records
        SELECT 1
        FROM TreatmentTracking tt2
        INNER JOIN TreatmentRecordDetail trd2 ON tt2.TreatmentRecordDetailId = trd2.Id
        WHERE trd2.TreatmentRecordId = tr.Id
        AND tt2.Status = 2 -- No Show status
        AND tt2.TrackingDate >= DATEADD(DAY, -3, GETDATE())
        GROUP BY trd2.TreatmentRecordId
        HAVING COUNT(*) >= 3
    );
END 