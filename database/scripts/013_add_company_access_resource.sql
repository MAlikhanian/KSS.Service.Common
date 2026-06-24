-- 013_add_company_access_resource.sql
--
-- Adds an "Access" section resource to the Company module so the new
-- /company/access page has a Permission catalog entry to reference.
--
-- Resource Id `019f1000-0000-7003-8000-000000000012` follows the same
-- numbering as 010 (Information) and 011 (Admin) under Company.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

DECLARE @moduleCompany    UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000002';
DECLARE @rCompanyAccess   UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000012';

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rCompanyAccess)
BEGIN
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rCompanyAccess, @moduleCompany, 'Access', @system, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation
               WHERE ResourceId = @rCompanyAccess AND LanguageId = @fa)
BEGIN
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
        (@rCompanyAccess, @fa, N'دسترسی شرکت', @system, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation
               WHERE ResourceId = @rCompanyAccess AND LanguageId = @en)
BEGIN
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt) VALUES
        (@rCompanyAccess, @en, N'Access', @system, @now);
END;

COMMIT TRANSACTION;

SELECT r.Id, r.Code, m.Code AS Module
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
WHERE m.Code = 'company'
ORDER BY r.Code;

PRINT '013_add_company_access_resource.sql applied successfully.';
