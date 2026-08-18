IF OBJECT_ID(N'dbo.Classes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Classes
    (
        ClassId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        StartTime DATETIMEOFFSET NOT NULL,
        Capacity INT NOT NULL,
        CONSTRAINT PK_Classes PRIMARY KEY (ClassId)
    );
END;

IF OBJECT_ID(N'dbo.ClassRegistrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassRegistrations
    (
        RegistrationId UNIQUEIDENTIFIER NOT NULL,
        ClassId UNIQUEIDENTIFIER NOT NULL,
        MemberId UNIQUEIDENTIFIER NOT NULL,
        RegisteredDate DATETIMEOFFSET NOT NULL,
        Status INT NOT NULL,
        CONSTRAINT PK_ClassRegistrations PRIMARY KEY (RegistrationId),
        CONSTRAINT FK_ClassRegistrations_Classes_ClassId
            FOREIGN KEY (ClassId) REFERENCES dbo.Classes (ClassId),
        CONSTRAINT FK_ClassRegistrations_Members_MemberId
            FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
END;
