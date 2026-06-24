-- 015_add_system_module_and_security_resources.sql
--
-- Adds the System module (a new top-level group in the catalog) and its
-- first three Resources under the Security sub-group. These power the new
-- /system/security/{roles,permissions,role-permissions} pages.
--
-- Resource Codes intentionally contain a dot ("Security.Role" etc.) so the
-- 6 derived Permission codes nest cleanly as "System.Security.X.Action".
--
-- Apply with `sqlcmd -f 65001` to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

DECLARE @modSystem               UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000005';
DECLARE @rSecurityRole           UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000050';
DECLARE @rSecurityPermission     UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000051';
DECLARE @rSecurityRolePermission UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000052';

BEGIN TRANSACTION;

-- ── Step 1: Module
IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE Id = @modSystem)
BEGIN
    INSERT INTO dbo.Module (Id, Code, CreatedBy, CreatedAt)
    VALUES (@modSystem, 'system', @system, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modSystem AND LanguageId = @fa)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modSystem, @fa, N'سیستم', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modSystem AND LanguageId = @en)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modSystem, @en, N'System', @system, @now);

-- ── Step 2: Resources under System
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rSecurityRole)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rSecurityRole, @modSystem, 'Security.Role', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rSecurityPermission)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rSecurityPermission, @modSystem, 'Security.Permission', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rSecurityRolePermission)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rSecurityRolePermission, @modSystem, 'Security.RolePermission', @system, @now);

-- ── Step 3: Resource translations
-- Security.Role
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityRole AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityRole, @fa, N'نقش‌ها', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityRole AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityRole, @en, N'Security Roles', @system, @now);

-- Security.Permission
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityPermission AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityPermission, @fa, N'دسترسی‌ها', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityPermission AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityPermission, @en, N'Security Permissions', @system, @now);

-- Security.RolePermission
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityRolePermission AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityRolePermission, @fa, N'دسترسی نقش‌ها', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rSecurityRolePermission AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rSecurityRolePermission, @en, N'Security Role-Permissions', @system, @now);

COMMIT TRANSACTION;

SELECT r.Code, mt.Name AS Module_fa
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
JOIN dbo.ModuleTranslation mt ON mt.ModuleId = m.Id AND mt.LanguageId = 12
WHERE m.Code = 'system'
ORDER BY r.Code;

PRINT '015_add_system_module_and_security_resources.sql applied successfully.';
