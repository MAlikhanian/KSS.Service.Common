-- 018_add_market_module_and_reference_resource.sql
--
-- Adds a Market module and its Reference resource so the 4 Market pages
-- (/market/{assets,sectors,asset-types,market-types}) can be gated by
-- catalog permissions. Until now they were ungated.
--
-- Apply with `sqlcmd -f 65001` to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

DECLARE @modMarket    UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000006';
DECLARE @rReference   UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000060';

BEGIN TRANSACTION;

-- Module
IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE Id = @modMarket)
    INSERT INTO dbo.Module (Id, Code, CreatedBy, CreatedAt)
    VALUES (@modMarket, 'market', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modMarket AND LanguageId = @fa)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modMarket, @fa, N'بازار', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ModuleTranslation WHERE ModuleId = @modMarket AND LanguageId = @en)
    INSERT INTO dbo.ModuleTranslation (ModuleId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@modMarket, @en, N'Market', @system, @now);

-- Resource: Reference (covers all 4 Market reference tables)
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rReference)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rReference, @modMarket, 'Reference', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rReference AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rReference, @fa, N'داده‌های مرجع بازار', @system, @now);
IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation WHERE ResourceId = @rReference AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rReference, @en, N'Market Reference', @system, @now);

COMMIT TRANSACTION;

SELECT r.Code,
       MAX(CASE WHEN rt.LanguageId = @fa THEN rt.Name END) AS Persian,
       MAX(CASE WHEN rt.LanguageId = @en THEN rt.Name END) AS English
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
LEFT JOIN dbo.ResourceTranslation rt ON rt.ResourceId = r.Id
WHERE m.Code = 'market'
GROUP BY r.Code
ORDER BY r.Code;

PRINT '018_add_market_module_and_reference_resource.sql applied successfully.';
