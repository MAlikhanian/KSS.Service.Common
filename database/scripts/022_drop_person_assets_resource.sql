-- 022_drop_person_assets_resource.sql
--
-- Drops the now-unused "Assets" Resource under the Person module
-- (Id 019f1000-0000-7003-8000-000000000002). The matching Permissions
-- (Person.Assets.Read/Modify) were deleted by Auth migration 021.
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rPersonAssets UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000002';

BEGIN TRANSACTION;

DELETE FROM dbo.ResourceTranslation WHERE ResourceId = @rPersonAssets;
DELETE FROM dbo.Resource WHERE Id = @rPersonAssets;

COMMIT TRANSACTION;

SELECT r.Code, m.Code AS Module
FROM dbo.Resource r JOIN dbo.Module m ON m.Id = r.ModuleId
WHERE m.Code = 'person'
ORDER BY r.Code;

PRINT '022_drop_person_assets_resource.sql applied successfully.';
