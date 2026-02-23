-- ============================================================
-- Database: KSS_Common_Prod (microservice: common)
-- Delete data from all tables (child-before-parent order).
-- Uses DELETE so FK-referenced parents can be cleared; TRUNCATE would fail on them.
-- ============================================================
USE [KSS_Common_Prod];
GO

-- CityTranslation → City
DELETE FROM dbo.[CityTranslation];
DELETE FROM dbo.[City];
-- RegionTranslation → Region
DELETE FROM dbo.[RegionTranslation];
DELETE FROM dbo.[Region];
-- CountryTranslation → Country
DELETE FROM dbo.[CountryTranslation];
DELETE FROM dbo.[Country];
-- Language
DELETE FROM dbo.[Language];
GO
