-- 012_section_resources.sql
--
-- Replaces the old entity-level Resources with section-level Resources matching
-- the new RBAC scheme. Old entity Resources (Address, Email, Phone, etc.) are
-- removed since the new permission scheme is section-based, not entity-based.
--
-- Also removes the auto-created modules (auth, marketdata, markethistory, portfolio)
-- that came from the Auth backfill; the 4 main modules (person, company, members,
-- creditrating) remain.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

-- The 4 main modules (already seeded in migration 011)
DECLARE @modulePerson      UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000001';
DECLARE @moduleCompany     UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000002';
DECLARE @moduleMembers     UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000003';
DECLARE @moduleCreditRate  UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000004';

BEGIN TRANSACTION;

-- ── Step 1: Clear all existing Resources (and their translations via cascade)
DELETE FROM dbo.ResourceTranslation;
DELETE FROM dbo.Resource;

-- ── Step 2: Remove auto-created modules (keep only the 4 main ones)
DELETE FROM dbo.ModuleTranslation
WHERE ModuleId NOT IN (@modulePerson, @moduleCompany, @moduleMembers, @moduleCreditRate);

DELETE FROM dbo.Module
WHERE Id NOT IN (@modulePerson, @moduleCompany, @moduleMembers, @moduleCreditRate);

-- ── Step 3: Insert section Resources

-- Person sections (5)
INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000001', @modulePerson, 'Information', @system, @now),
    ('019f1000-0000-7003-8000-000000000002', @modulePerson, 'Assets',      @system, @now),
    ('019f1000-0000-7003-8000-000000000003', @modulePerson, 'Access',      @system, @now),
    ('019f1000-0000-7003-8000-000000000004', @modulePerson, 'Security',    @system, @now),
    ('019f1000-0000-7003-8000-000000000005', @modulePerson, 'Admin',       @system, @now);

-- Company sections (2)
INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000010', @moduleCompany, 'Information', @system, @now),
    ('019f1000-0000-7003-8000-000000000011', @moduleCompany, 'Admin',       @system, @now);

-- Members sections (2)
INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000020', @moduleMembers, 'Directory', @system, @now),
    ('019f1000-0000-7003-8000-000000000021', @moduleMembers, 'Admin',     @system, @now);

-- CreditRating sections (2)
INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000030', @moduleCreditRate, 'Assessment', @system, @now),
    ('019f1000-0000-7003-8000-000000000031', @moduleCreditRate, 'Admin',      @system, @now);

-- ── Step 4: Translations
-- Person sections
INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000001', @fa, N'اطلاعات شخص', @system, @now),
    ('019f1000-0000-7003-8000-000000000001', @en, N'Information',  @system, @now),
    ('019f1000-0000-7003-8000-000000000002', @fa, N'دارایی‌ها',     @system, @now),
    ('019f1000-0000-7003-8000-000000000002', @en, N'Assets',       @system, @now),
    ('019f1000-0000-7003-8000-000000000003', @fa, N'دسترسی‌ها',     @system, @now),
    ('019f1000-0000-7003-8000-000000000003', @en, N'Access',       @system, @now),
    ('019f1000-0000-7003-8000-000000000004', @fa, N'امنیت',         @system, @now),
    ('019f1000-0000-7003-8000-000000000004', @en, N'Security',     @system, @now),
    ('019f1000-0000-7003-8000-000000000005', @fa, N'مدیریت',        @system, @now),
    ('019f1000-0000-7003-8000-000000000005', @en, N'Admin',        @system, @now);

-- Company sections
INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000010', @fa, N'اطلاعات شرکت', @system, @now),
    ('019f1000-0000-7003-8000-000000000010', @en, N'Information',  @system, @now),
    ('019f1000-0000-7003-8000-000000000011', @fa, N'مدیریت',        @system, @now),
    ('019f1000-0000-7003-8000-000000000011', @en, N'Admin',        @system, @now);

-- Members sections
INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000020', @fa, N'فهرست اعضا', @system, @now),
    ('019f1000-0000-7003-8000-000000000020', @en, N'Directory',  @system, @now),
    ('019f1000-0000-7003-8000-000000000021', @fa, N'مدیریت',      @system, @now),
    ('019f1000-0000-7003-8000-000000000021', @en, N'Admin',      @system, @now);

-- CreditRating sections
INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
    ('019f1000-0000-7003-8000-000000000030', @fa, N'ارزیابی',    @system, @now),
    ('019f1000-0000-7003-8000-000000000030', @en, N'Assessment', @system, @now),
    ('019f1000-0000-7003-8000-000000000031', @fa, N'مدیریت',      @system, @now),
    ('019f1000-0000-7003-8000-000000000031', @en, N'Admin',      @system, @now);

COMMIT TRANSACTION;

PRINT '012_section_resources.sql applied successfully.';
