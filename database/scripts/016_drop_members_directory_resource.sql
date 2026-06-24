-- 016_drop_members_directory_resource.sql
--
-- Drops the Members.Directory resource (Id 019f1000-0000-7003-8000-000000000020)
-- and its translations. The Members module no longer uses a Directory section —
-- everything collapsed to Members.Admin.
--
-- Run AFTER Auth migration 014 (which removes the Permissions referencing this
-- Resource).
--
-- Apply to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @rDirectory UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000020';

BEGIN TRANSACTION;

DELETE FROM dbo.ResourceTranslation WHERE ResourceId = @rDirectory;
DELETE FROM dbo.Resource WHERE Id = @rDirectory;

COMMIT TRANSACTION;

SELECT r.Code, m.Code AS Module
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
WHERE m.Code = 'members'
ORDER BY r.Code;

PRINT '016_drop_members_directory_resource.sql applied successfully.';
