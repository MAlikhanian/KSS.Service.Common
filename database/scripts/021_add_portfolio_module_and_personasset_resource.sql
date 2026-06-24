-- 021_add_portfolio_module_and_personasset_resource.sql
--
-- Adds a Portfolio module and its PersonAsset resource so the new
-- Portfolio.PersonAsset.{Read,Modify} permissions can FK to a real
-- Module/Resource pair. The old "Assets" resource under the Person module
-- (used by Person.Assets.* perms) is intentionally NOT touched here — it
-- gets dropped in 023 AFTER the Auth migration drops the old perms.
--
-- Apply with `sqlcmd -f 65001` to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

DECLARE @modPortfolio   UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000007';
DECLARE @rPersonAsset   UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000080';

BEGIN TRANSACTION;

-- Module: portfolio
IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE Id = @modPortfolio)
    INSERT INTO dbo.Module (Id, Code, CreatedBy, CreatedAt)
    VALUES (@modPortfolio, 'portfolio', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modPortfolio AND LanguageId = @fa)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modPortfolio, @fa, N'پرتفوی', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modPortfolio AND LanguageId = @en)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modPortfolio, @en, N'Portfolio', @system, @now);

-- Resource: PersonAsset (entity = Portfolio.PersonAsset, joins Person to Asset)
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rPersonAsset)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rPersonAsset, @modPortfolio, 'PersonAsset', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rPersonAsset AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rPersonAsset, @fa, N'دارایی‌های اشخاص', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rPersonAsset AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rPersonAsset, @en, N'Person Assets', @system, @now);

COMMIT TRANSACTION;

SELECT m.Code AS Module, r.Code AS Resource,
       MAX(CASE WHEN rt.LanguageId = @fa THEN rt.Name END) AS Persian,
       MAX(CASE WHEN rt.LanguageId = @en THEN rt.Name END) AS English
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
LEFT JOIN dbo.ResourceTranslation rt ON rt.ResourceId = r.Id
WHERE m.Code = 'portfolio'
GROUP BY m.Code, r.Code
ORDER BY r.Code;

PRINT '021_add_portfolio_module_and_personasset_resource.sql applied successfully.';
