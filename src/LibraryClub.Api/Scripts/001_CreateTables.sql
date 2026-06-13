CREATE TABLE Readers (
	Id UNIQUEIDENTIFIER NOT NULL,
	Name NVARCHAR(150) NOT NULL,
	Email NVARCHAR(255) NOT NULL,
	Status NVARCHAR(30) NOT NULL,
	CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Readers_CreatedAt DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Readers PRIMARY KEY (Id),
	CONSTRAINT UQ_Readers_Email UNIQUE (Email),
	CONSTRAINT CK_Readers_Status CHECK (Status IN ('Active', 'Inactive'))
);

CREATE TABLE ReadingClubs (
	Id UNIQUEIDENTIFIER NOT NULL,
	Name NVARCHAR(150) NOT NULL,
	Description NVARCHAR(1000) NULL,
	Genre NVARCHAR(100) NOT NULL,
	Status NVARCHAR(30) NOT NULL,
	CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ReadingClubs_CreatedAt DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_ReadingClubs PRIMARY KEY (Id),
	CONSTRAINT CK_ReadingClubs_Status CHECK (Status IN ('Active', 'Inactive', 'Archived'))
);

CREATE TABLE ClubSubscriptions (
	Id UNIQUEIDENTIFIER NOT NULL,
    ReaderId UNIQUEIDENTIFIER NOT NULL,
    ReadingClubId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ClubSubscriptions_CreatedAt DEFAULT SYSUTCDATETIME(),
    CanceledAt DATETIME2 NULL,

    CONSTRAINT PK_ClubSubscriptions PRIMARY KEY (Id),

    CONSTRAINT FK_ClubSubscriptions_Readers
		FOREIGN KEY (ReaderId)
        REFERENCES Readers(Id),

    CONSTRAINT FK_ClubSubscriptions_ReadingClubs
        FOREIGN KEY (ReadingClubId)
        REFERENCES ReadingClubs(Id),

    CONSTRAINT CK_ClubSubscriptions_Status CHECK (Status IN ('Active', 'Canceled'))
);

CREATE UNIQUE INDEX UX_ClubSubscriptions_Reader_Club_Active
ON ClubSubscriptions (ReaderId, ReadingClubId)
WHERE Status = 'Active';
