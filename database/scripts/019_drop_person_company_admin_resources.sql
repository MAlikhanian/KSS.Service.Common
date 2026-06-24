-- 019_drop_person_company_admin_resources.sql
--
-- Removes the now-unused Resources:
--   • Person.Admin   (Id 019f1000-...000005)
--   • Company.Admin  (Id 019f1000-...000011)
--
-- Run AFTER Auth migration 019 (which deletes the Permissions referencing
-- these Resources). The Permission → Resource FK doesn't cascade-delete the
-- Resource, so we tidy it up explicitly here.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rPersonAdmin  UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000005';
DECLARE @rCompanyAdmin UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000011';

BEGIN TRANSACTION;

DELETE FROM dbo.ResourceTranslation WHERE ResourceId IN (@rPersonAdmin, @rCompanyAdmin);
DELETE FROM dbo.Resource             WHERE Id         IN (@rPersonAdmin, @rCompanyAdmin);

COMMIT TRANSACTION;

SELECT r.Code, m.Code AS Module
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
WHERE m.Code IN ('person', 'company')
ORDER BY m.Code, r.Code;

PRINT '019_drop_person_company_admin_resources.sql applied successfully.';
