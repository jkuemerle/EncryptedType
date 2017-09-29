IF(SELECT COUNT(*) FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[ListItems]') AND type IN (N'P', N'PC')) <> 0
BEGIN
	DROP PROCEDURE [dbo].[ListItems]
END
GO

CREATE PROCEDURE [dbo].[ListItems]
AS
BEGIN
	SELECT ID FROM [dbo].[EncryptionKeys]
	ORDER BY ID 
END
GO

GRANT EXECUTE ON [dbo].[ListItems] TO KeyReader
GO

