IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(320) NOT NULL,
        Status INT NOT NULL,
        JoinDate DATETIMEOFFSET NOT NULL,
        CONSTRAINT PK_Members PRIMARY KEY (MemberId)
    );
END;

IF OBJECT_ID(N'dbo.CheckIns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CheckIns
    (
        CheckInId UNIQUEIDENTIFIER NOT NULL,
        MemberId UNIQUEIDENTIFIER NOT NULL,
        CheckInDate DATETIMEOFFSET NOT NULL,
        CONSTRAINT PK_CheckIns PRIMARY KEY (CheckInId),
        CONSTRAINT FK_CheckIns_Members_MemberId
            FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
END;
