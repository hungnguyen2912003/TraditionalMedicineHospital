CREATE TRIGGER TR_TreatmentRecord_UpdateStatus
ON TreatmentRecord
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE tr
    SET tr.Status = 2 -- Assuming 2 is the value for "Completed" status
    FROM TreatmentRecord tr
    WHERE tr.EndDate < GETDATE()
    AND tr.Status != 2; -- Only update if not already completed
END 