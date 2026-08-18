IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_ClassRegistrations_ActiveMemberClass'
        AND object_id = OBJECT_ID(N'dbo.ClassRegistrations')
)
BEGIN
    CREATE UNIQUE INDEX UX_ClassRegistrations_ActiveMemberClass
        ON dbo.ClassRegistrations (MemberId, ClassId)
        WHERE Status = 0;
END;
