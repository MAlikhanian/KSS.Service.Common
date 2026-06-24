-- 014_fix_company_access_resource_utf.sql
--
-- Companion to Auth migration 010. Common migration 013 stored the
-- Persian name "دسترسی شرکت" for the Company Access resource via
-- sqlcmd without -f 65001, mojibaking it into "Ø¯Ø³ØªØ±Ø³ÛŒ Ø´Ø±Ú©Øª".
--
-- Re-apply with `sqlcmd -f 65001` so the encoding is correct.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @fa SMALLINT = 12;
DECLARE @rCompanyAccess UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000012';

BEGIN TRANSACTION;

UPDATE dbo.ResourceTranslation
SET Name = N'دسترسی شرکت'
WHERE ResourceId = @rCompanyAccess AND LanguageId = @fa;

COMMIT TRANSACTION;

SELECT r.Id, r.Code, rt.LanguageId, rt.Name
FROM dbo.Resource r
JOIN dbo.ResourceTranslation rt ON rt.ResourceId = r.Id
WHERE r.Id = @rCompanyAccess
ORDER BY rt.LanguageId;

PRINT '014_fix_company_access_resource_utf.sql applied successfully.';
