-- 011_module_resource_lookup.sql
--
-- Adds two new lookup tables to KSS_Common:
--   * Module       — top-level domain category (Person, Company, Members, CreditRating, …)
--   * Resource     — entity within a Module (Address, Email, Phone, …)
-- Plus translation tables for both.
--
-- Used to categorize permissions and roles in the Auth service so the admin UI
-- can filter them by Module → Resource. IDs are uniqueidentifier (Guid v7) for
-- consistency with the rest of the codebase.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system_user UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

BEGIN TRANSACTION;

-- ── Module (parent lookup)
CREATE TABLE dbo.Module (
    Id          UNIQUEIDENTIFIER NOT NULL,
    Code        VARCHAR(30)      NOT NULL,
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    DeletedBy   UNIQUEIDENTIFIER NULL,
    DeletedAt   DATETIME2        NULL,
    IsActive    BIT              NOT NULL CONSTRAINT DF_Module_IsActive DEFAULT (1),
    CONSTRAINT PK_Module PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Module_Code UNIQUE (Code)
);

INSERT INTO dbo.Module (Id, Code, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7001-8000-000000000001', 'person',       @system_user, @now),
    ('019f1000-0000-7001-8000-000000000002', 'company',      @system_user, @now),
    ('019f1000-0000-7001-8000-000000000003', 'members',      @system_user, @now),
    ('019f1000-0000-7001-8000-000000000004', 'creditrating', @system_user, @now);

-- ── ModuleTranslation
CREATE TABLE dbo.ModuleTranslation (
    ModuleId    UNIQUEIDENTIFIER NOT NULL,
    LanguageId  SMALLINT         NOT NULL,
    Name        NVARCHAR(100)    NOT NULL,
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    DeletedBy   UNIQUEIDENTIFIER NULL,
    DeletedAt   DATETIME2        NULL,
    CONSTRAINT PK_ModuleTranslation PRIMARY KEY CLUSTERED (ModuleId, LanguageId),
    CONSTRAINT FK_ModuleTranslation_Module FOREIGN KEY (ModuleId)
        REFERENCES dbo.Module(Id) ON DELETE CASCADE
);

INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7001-8000-000000000001', @fa, N'اشخاص',                   @system_user, @now),
    ('019f1000-0000-7001-8000-000000000001', @en, N'Persons',                  @system_user, @now),
    ('019f1000-0000-7001-8000-000000000002', @fa, N'شرکت‌ها',                  @system_user, @now),
    ('019f1000-0000-7001-8000-000000000002', @en, N'Companies',                @system_user, @now),
    ('019f1000-0000-7001-8000-000000000003', @fa, N'اعضا',                     @system_user, @now),
    ('019f1000-0000-7001-8000-000000000003', @en, N'Members',                  @system_user, @now),
    ('019f1000-0000-7001-8000-000000000004', @fa, N'رتبه‌بندی اعتباری',        @system_user, @now),
    ('019f1000-0000-7001-8000-000000000004', @en, N'Credit Rating',            @system_user, @now);

-- ── Resource (child of Module)
CREATE TABLE dbo.Resource (
    Id          UNIQUEIDENTIFIER NOT NULL,
    ModuleId    UNIQUEIDENTIFIER NOT NULL,
    Code        VARCHAR(50)      NOT NULL,
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    DeletedBy   UNIQUEIDENTIFIER NULL,
    DeletedAt   DATETIME2        NULL,
    IsActive    BIT              NOT NULL CONSTRAINT DF_Resource_IsActive DEFAULT (1),
    CONSTRAINT PK_Resource PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Resource_Module FOREIGN KEY (ModuleId)
        REFERENCES dbo.Module(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Resource_ModuleId_Code UNIQUE (ModuleId, Code)
);

INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt) VALUES
    -- Person (12)
    ('019f1000-0000-7002-8000-000000000001', '019f1000-0000-7001-8000-000000000001', 'Person',       @system_user, @now),
    ('019f1000-0000-7002-8000-000000000002', '019f1000-0000-7001-8000-000000000001', 'Address',      @system_user, @now),
    ('019f1000-0000-7002-8000-000000000003', '019f1000-0000-7001-8000-000000000001', 'Email',        @system_user, @now),
    ('019f1000-0000-7002-8000-000000000004', '019f1000-0000-7001-8000-000000000001', 'Phone',        @system_user, @now),
    ('019f1000-0000-7002-8000-000000000005', '019f1000-0000-7001-8000-000000000001', 'Document',     @system_user, @now),
    ('019f1000-0000-7002-8000-000000000006', '019f1000-0000-7001-8000-000000000001', 'Employment',   @system_user, @now),
    ('019f1000-0000-7002-8000-000000000007', '019f1000-0000-7001-8000-000000000001', 'Education',    @system_user, @now),
    ('019f1000-0000-7002-8000-000000000008', '019f1000-0000-7001-8000-000000000001', 'Nationality',  @system_user, @now),
    ('019f1000-0000-7002-8000-000000000009', '019f1000-0000-7001-8000-000000000001', 'Relationship', @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000a', '019f1000-0000-7001-8000-000000000001', 'Status',       @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000b', '019f1000-0000-7001-8000-000000000001', 'Translation',  @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000c', '019f1000-0000-7001-8000-000000000001', 'Access',       @system_user, @now),
    -- Company (4)
    ('019f1000-0000-7002-8000-00000000000d', '019f1000-0000-7001-8000-000000000002', 'Company',      @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000e', '019f1000-0000-7001-8000-000000000002', 'Address',      @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000f', '019f1000-0000-7001-8000-000000000002', 'Email',        @system_user, @now),
    ('019f1000-0000-7002-8000-000000000010', '019f1000-0000-7001-8000-000000000002', 'Phone',        @system_user, @now),
    -- Members (1)
    ('019f1000-0000-7002-8000-000000000011', '019f1000-0000-7001-8000-000000000003', 'Member',       @system_user, @now),
    -- CreditRating (1)
    ('019f1000-0000-7002-8000-000000000012', '019f1000-0000-7001-8000-000000000004', 'Assessment',   @system_user, @now);

-- ── ResourceTranslation
CREATE TABLE dbo.ResourceTranslation (
    ResourceId  UNIQUEIDENTIFIER NOT NULL,
    LanguageId  SMALLINT         NOT NULL,
    Name        NVARCHAR(100)    NOT NULL,
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2        NOT NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    DeletedBy   UNIQUEIDENTIFIER NULL,
    DeletedAt   DATETIME2        NULL,
    CONSTRAINT PK_ResourceTranslation PRIMARY KEY CLUSTERED (ResourceId, LanguageId),
    CONSTRAINT FK_ResourceTranslation_Resource FOREIGN KEY (ResourceId)
        REFERENCES dbo.Resource(Id) ON DELETE CASCADE
);

-- Person resources (FA + EN)
INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7002-8000-000000000001', @fa, N'شخص',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000001', @en, N'Person',        @system_user, @now),
    ('019f1000-0000-7002-8000-000000000002', @fa, N'آدرس',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000002', @en, N'Address',       @system_user, @now),
    ('019f1000-0000-7002-8000-000000000003', @fa, N'ایمیل',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000003', @en, N'Email',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000004', @fa, N'تلفن',          @system_user, @now),
    ('019f1000-0000-7002-8000-000000000004', @en, N'Phone',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000005', @fa, N'مدارک',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000005', @en, N'Document',      @system_user, @now),
    ('019f1000-0000-7002-8000-000000000006', @fa, N'سوابق شغلی',    @system_user, @now),
    ('019f1000-0000-7002-8000-000000000006', @en, N'Employment',    @system_user, @now),
    ('019f1000-0000-7002-8000-000000000007', @fa, N'تحصیلات',       @system_user, @now),
    ('019f1000-0000-7002-8000-000000000007', @en, N'Education',     @system_user, @now),
    ('019f1000-0000-7002-8000-000000000008', @fa, N'تابعیت',        @system_user, @now),
    ('019f1000-0000-7002-8000-000000000008', @en, N'Nationality',   @system_user, @now),
    ('019f1000-0000-7002-8000-000000000009', @fa, N'نسبت',          @system_user, @now),
    ('019f1000-0000-7002-8000-000000000009', @en, N'Relationship',  @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000a', @fa, N'وضعیت',         @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000a', @en, N'Status',        @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000b', @fa, N'ترجمه',         @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000b', @en, N'Translation',   @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000c', @fa, N'دسترسی',        @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000c', @en, N'Access',        @system_user, @now),
    -- Company resources
    ('019f1000-0000-7002-8000-00000000000d', @fa, N'شرکت',         @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000d', @en, N'Company',       @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000e', @fa, N'آدرس',         @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000e', @en, N'Address',       @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000f', @fa, N'ایمیل',         @system_user, @now),
    ('019f1000-0000-7002-8000-00000000000f', @en, N'Email',         @system_user, @now),
    ('019f1000-0000-7002-8000-000000000010', @fa, N'تلفن',          @system_user, @now),
    ('019f1000-0000-7002-8000-000000000010', @en, N'Phone',         @system_user, @now),
    -- Members
    ('019f1000-0000-7002-8000-000000000011', @fa, N'عضو',           @system_user, @now),
    ('019f1000-0000-7002-8000-000000000011', @en, N'Member',        @system_user, @now),
    -- CreditRating
    ('019f1000-0000-7002-8000-000000000012', @fa, N'ارزیابی',       @system_user, @now),
    ('019f1000-0000-7002-8000-000000000012', @en, N'Assessment',    @system_user, @now);

COMMIT TRANSACTION;

PRINT '011_module_resource_lookup.sql applied successfully.';
