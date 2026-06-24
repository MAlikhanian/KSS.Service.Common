-- 017_rename_admin_to_brokerage_add_invfunds.sql
--
-- Splits the Members module's single "Admin" resource into two:
--   • Renames existing Members.Admin → Members.Brokerage (same Id)
--   • Adds new Members.InvestmentFunds resource
--
-- The Members.Directory resource was dropped earlier (migration 016).
-- This leaves the Members module with exactly two resources: Brokerage,
-- InvestmentFunds. Each gets its own Read/Modify permission pair in Auth
-- (handled by Auth migration 015).
--
-- Apply with `sqlcmd -f 65001` to KSS_Common_Prod and KSS_Common_Dev.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @system UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @fa SMALLINT = 12;
DECLARE @en SMALLINT = 10;

DECLARE @modMembers       UNIQUEIDENTIFIER = '019f1000-0000-7001-8000-000000000003';
DECLARE @rBrokerage       UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000021'; -- was Admin
DECLARE @rInvestmentFunds UNIQUEIDENTIFIER = '019f1000-0000-7003-8000-000000000022'; -- new

BEGIN TRANSACTION;

-- ── Step 1: Rename existing Admin resource → Brokerage
UPDATE dbo.Resource
SET Code = 'Brokerage'
WHERE Id = @rBrokerage AND Code = 'Admin';

UPDATE dbo.ResourceTranslation
SET Name = N'کارگزاری‌ها'
WHERE ResourceId = @rBrokerage AND LanguageId = @fa;

UPDATE dbo.ResourceTranslation
SET Name = N'Brokerage'
WHERE ResourceId = @rBrokerage AND LanguageId = @en;

-- ── Step 2: Insert new InvestmentFunds resource (idempotent)
IF NOT EXISTS (SELECT 1 FROM dbo.Resource WHERE Id = @rInvestmentFunds)
    INSERT INTO dbo.Resource (Id, ModuleId, Code, CreatedBy, CreatedAt)
    VALUES (@rInvestmentFunds, @modMembers, 'InvestmentFunds', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation
               WHERE ResourceId = @rInvestmentFunds AND LanguageId = @fa)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rInvestmentFunds, @fa, N'صندوق‌های سرمایه‌گذاری', @system, @now);

IF NOT EXISTS (SELECT 1 FROM dbo.ResourceTranslation
               WHERE ResourceId = @rInvestmentFunds AND LanguageId = @en)
    INSERT INTO dbo.ResourceTranslation (ResourceId, LanguageId, Name, CreatedBy, CreatedAt)
    VALUES (@rInvestmentFunds, @en, N'Investment Funds', @system, @now);

COMMIT TRANSACTION;

SELECT r.Code,
       MAX(CASE WHEN rt.LanguageId = @fa THEN rt.Name END) AS Persian,
       MAX(CASE WHEN rt.LanguageId = @en THEN rt.Name END) AS English
FROM dbo.Resource r
JOIN dbo.Module m ON m.Id = r.ModuleId
LEFT JOIN dbo.ResourceTranslation rt ON rt.ResourceId = r.Id
WHERE m.Code = 'members'
GROUP BY r.Code
ORDER BY r.Code;

PRINT '017_rename_admin_to_brokerage_add_invfunds.sql applied successfully.';
