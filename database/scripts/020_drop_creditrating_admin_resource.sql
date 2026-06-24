-- 020_drop_creditrating_admin_resource.sql
--
-- Drops the now-unused "Admin" Resource under the creditrating module
-- (Id 019f1000-0000-7003-8000-000000000031). The matching Permissions
-- (CreditRating.Admin.Read/Modify) were deleted by Auth migration 020.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rCRAdmin UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000031';

BEGIN TRANSACTION;

DELETE FROM dbo.ResourceTranslation WHERE ResourceId = @rCRAdmin;
DELETE FROM dbo.Resource WHERE Id = @rCRAdmin;

COMMIT TRANSACTION;

SELECT r.Code, m.Code AS Module
FROM dbo.Resource r JOIN dbo.Module m ON m.Id = r.ModuleId
WHERE m.Code = 'creditrating'
ORDER BY r.Code;

PRINT '020_drop_creditrating_admin_resource.sql applied successfully.';
