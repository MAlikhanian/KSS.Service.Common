-- ============================================================
-- Database: KSS_Common (microservice: common)
-- Table order follows dependencies:
--   Section 1: Language (core lookup)
--   Section 2: Country + CountryTranslation
--   Section 3: Region + RegionTranslation (depends on Country)
--   Section 4: City + CityTranslation (depends on Country, Region)
-- LanguageId used in all Translation tables as part of composite key.
-- If the database already exists, connections are force-closed and the database is dropped before create.
-- ============================================================

USE [master];
GO

-- ============================================================
-- Uncomment to recreate Prod (CAUTION: destroys all data)
-- ============================================================
/* IF DB_ID(N'KSS_Common_Prod') IS NOT NULL
BEGIN
    ALTER DATABASE [KSS_Common_Prod] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [KSS_Common_Prod];
END
GO

CREATE DATABASE [KSS_Common_Prod];
GO

USE [KSS_Common_Prod];
GO */

-- ============================================================
-- Create Dev database
-- ============================================================
IF DB_ID(N'KSS_Common_Dev') IS NOT NULL
BEGIN
    ALTER DATABASE [KSS_Common_Dev] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [KSS_Common_Dev];
END
GO

CREATE DATABASE [KSS_Common_Dev];
GO

USE [KSS_Common_Dev];
GO

-- ============================================================
-- SECTION 1: Language (core lookup)
-- ============================================================
CREATE TABLE dbo.[Language] (
    Id         SMALLINT       IDENTITY(1, 1) NOT NULL, -- شناسه
    Code       NVARCHAR(10)   NOT NULL,                -- کد زبان (e.g. en, fa)
    Name       NVARCHAR(100)  NOT NULL,                -- نام (English)
    NativeName NVARCHAR(100)  NULL,                    -- نام بومی
    IsActive   BIT            NOT NULL CONSTRAINT DF_Language_IsActive DEFAULT 1, -- فعال
    CreatedAt  DATETIME2(7)   NOT NULL CONSTRAINT DF_Language_CreatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ ایجاد
    UpdatedAt  DATETIME2(7)   NOT NULL CONSTRAINT DF_Language_UpdatedAt DEFAULT SYSUTCDATETIME(), -- تاریخ به‌روزرسانی
    CONSTRAINT PK_Language PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Language_Code UNIQUE (Code)
);
CREATE NONCLUSTERED INDEX IX_Language_Code ON dbo.[Language] (Code);
CREATE NONCLUSTERED INDEX IX_Language_IsActive ON dbo.[Language] (IsActive) WHERE IsActive = 1;
GO

-- ============================================================
-- SECTION 2: Country + CountryTranslation
-- ============================================================
CREATE TABLE dbo.[Country] (
    Id          SMALLINT      IDENTITY(1, 1) NOT NULL, -- شناسه
    Code        NVARCHAR(2)   NOT NULL,                -- ISO 3166-1 alpha-2 (e.g. IR, US)
    Code3       NVARCHAR(3)   NOT NULL,                -- ISO 3166-1 alpha-3 (e.g. IRN, USA)
    NativeName  NVARCHAR(80)  NULL,                    -- نام بومی
    CallingCode SMALLINT      NULL,                    -- پیش‌شماره تلفن (e.g. 98, 1)
    CONSTRAINT PK_Country PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Country_Code UNIQUE (Code),
    CONSTRAINT UQ_Country_Code3 UNIQUE (Code3)
);
CREATE NONCLUSTERED INDEX IX_Country_CallingCode ON dbo.[Country] (CallingCode) WHERE CallingCode IS NOT NULL;
GO

CREATE TABLE dbo.[CountryTranslation] (
    CountryId  SMALLINT      NOT NULL, -- شناسه کشور
    LanguageId SMALLINT      NOT NULL, -- شناسه زبان
    Name       NVARCHAR(80)  NOT NULL, -- نام
    CONSTRAINT PK_CountryTranslation PRIMARY KEY CLUSTERED (CountryId, LanguageId),
    CONSTRAINT FK_CountryTranslation_Country FOREIGN KEY (CountryId) REFERENCES dbo.[Country] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_CountryTranslation_Language FOREIGN KEY (LanguageId) REFERENCES dbo.[Language] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_CountryTranslation_LanguageId ON dbo.[CountryTranslation] (LanguageId);
GO

-- ============================================================
-- SECTION 3: Region + RegionTranslation (depends on Country)
-- ============================================================
CREATE TABLE dbo.[Region] (
    Id        SMALLINT     IDENTITY(1, 1) NOT NULL, -- شناسه
    CountryId SMALLINT     NOT NULL,                -- شناسه کشور
    Code      VARCHAR(3)   NOT NULL,                -- کد استان/منطقه
    CONSTRAINT PK_Region PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Region_Country FOREIGN KEY (CountryId) REFERENCES dbo.[Country] (Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Region_Country_Code UNIQUE (CountryId, Code)
);
CREATE NONCLUSTERED INDEX IX_Region_CountryId ON dbo.[Region] (CountryId);
GO

CREATE TABLE dbo.[RegionTranslation] (
    RegionId   SMALLINT      NOT NULL, -- شناسه استان
    LanguageId SMALLINT      NOT NULL, -- شناسه زبان
    Name       NVARCHAR(80)  NOT NULL, -- نام
    CONSTRAINT PK_RegionTranslation PRIMARY KEY CLUSTERED (RegionId, LanguageId),
    CONSTRAINT FK_RegionTranslation_Region FOREIGN KEY (RegionId) REFERENCES dbo.[Region] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_RegionTranslation_Language FOREIGN KEY (LanguageId) REFERENCES dbo.[Language] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_RegionTranslation_LanguageId ON dbo.[RegionTranslation] (LanguageId);
GO

-- ============================================================
-- SECTION 4: City + CityTranslation (depends on Country, Region)
-- ============================================================
CREATE TABLE dbo.[City] (
    Id        INT          IDENTITY(1, 1) NOT NULL, -- شناسه
    CountryId SMALLINT     NOT NULL,                -- شناسه کشور
    RegionId  SMALLINT     NULL,                    -- شناسه استان (NULL = city without region)
    Code      VARCHAR(10)  NULL,                    -- کد شهر
    CONSTRAINT PK_City PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_City_Country FOREIGN KEY (CountryId) REFERENCES dbo.[Country] (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_City_Region FOREIGN KEY (RegionId) REFERENCES dbo.[Region] (Id) ON DELETE NO ACTION
);
CREATE NONCLUSTERED INDEX IX_City_CountryId ON dbo.[City] (CountryId);
CREATE NONCLUSTERED INDEX IX_City_RegionId ON dbo.[City] (RegionId) WHERE RegionId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_City_Code ON dbo.[City] (Code) WHERE Code IS NOT NULL;
GO

CREATE TABLE dbo.[CityTranslation] (
    CityId     INT           NOT NULL, -- شناسه شهر
    LanguageId SMALLINT      NOT NULL, -- شناسه زبان
    Name       NVARCHAR(64)  NOT NULL, -- نام
    CONSTRAINT PK_CityTranslation PRIMARY KEY CLUSTERED (CityId, LanguageId),
    CONSTRAINT FK_CityTranslation_City FOREIGN KEY (CityId) REFERENCES dbo.[City] (Id) ON DELETE CASCADE,
    CONSTRAINT FK_CityTranslation_Language FOREIGN KEY (LanguageId) REFERENCES dbo.[Language] (Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_CityTranslation_LanguageId ON dbo.[CityTranslation] (LanguageId);
GO

-- ============================================================
-- UpdatedAt trigger for Language (only table with UpdatedAt)
-- NOTE: SET QUOTED_IDENTIFIER ON required for EF Core compatibility
-- ============================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
CREATE TRIGGER dbo.TR_Language_SetUpdatedAt ON dbo.[Language] AFTER UPDATE
AS
BEGIN
  SET NOCOUNT ON;
  IF UPDATE(UpdatedAt) RETURN;
  UPDATE c SET UpdatedAt = SYSUTCDATETIME()
  FROM dbo.[Language] c INNER JOIN inserted i ON i.Id = c.Id;
END;
GO

-- ============================================================
-- SECTION 5: Copy data from Prod to Dev
-- Run this section only if KSS_Common_Prod exists on the same server.
-- ============================================================
IF DB_ID(N'KSS_Common_Prod') IS NOT NULL
BEGIN
    PRINT 'Copying data from KSS_Common_Prod to KSS_Common_Dev...';

    -- Language
    SET IDENTITY_INSERT dbo.[Language] ON;
    INSERT INTO dbo.[Language] (Id, Code, Name, NativeName, IsActive, CreatedAt, UpdatedAt)
    SELECT Id, Code, Name, NativeName, IsActive, CreatedAt, UpdatedAt
    FROM KSS_Common_Prod.dbo.[Language];
    SET IDENTITY_INSERT dbo.[Language] OFF;

    -- Country
    SET IDENTITY_INSERT dbo.[Country] ON;
    INSERT INTO dbo.[Country] (Id, Code, Code3, NativeName, CallingCode)
    SELECT Id, Code, Code3, NativeName, CallingCode
    FROM KSS_Common_Prod.dbo.[Country];
    SET IDENTITY_INSERT dbo.[Country] OFF;

    -- CountryTranslation
    INSERT INTO dbo.[CountryTranslation] (CountryId, LanguageId, Name)
    SELECT CountryId, LanguageId, Name
    FROM KSS_Common_Prod.dbo.[CountryTranslation];

    -- Region
    SET IDENTITY_INSERT dbo.[Region] ON;
    INSERT INTO dbo.[Region] (Id, CountryId, Code)
    SELECT Id, CountryId, Code
    FROM KSS_Common_Prod.dbo.[Region];
    SET IDENTITY_INSERT dbo.[Region] OFF;

    -- RegionTranslation
    INSERT INTO dbo.[RegionTranslation] (RegionId, LanguageId, Name)
    SELECT RegionId, LanguageId, Name
    FROM KSS_Common_Prod.dbo.[RegionTranslation];

    -- City
    SET IDENTITY_INSERT dbo.[City] ON;
    INSERT INTO dbo.[City] (Id, CountryId, RegionId, Code)
    SELECT Id, CountryId, RegionId, Code
    FROM KSS_Common_Prod.dbo.[City];
    SET IDENTITY_INSERT dbo.[City] OFF;

    -- CityTranslation
    INSERT INTO dbo.[CityTranslation] (CityId, LanguageId, Name)
    SELECT CityId, LanguageId, Name
    FROM KSS_Common_Prod.dbo.[CityTranslation];

    PRINT 'Data copy complete.';
END
ELSE
BEGIN
    PRINT 'KSS_Common_Prod not found on this server. Skipping data copy.';
END
GO
