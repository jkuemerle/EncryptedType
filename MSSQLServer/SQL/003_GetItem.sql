IF(SELECT COUNT(*) FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[GetItem]') AND type IN (N'P', N'PC')) <> 0
BEGIN
	DROP PROCEDURE [dbo].[GetItem]
END
GO

CREATE PROCEDURE [dbo].[GetItem]
	@ID nvarchar(100)
WITH EXECUTE AS OWNER
AS 
BEGIN
	SELECT ID, CONVERT(nvarchar, DECRYPTBYKEYAUTOCERT(CERT_ID('key_cert'),null,KeyVal,1,ID)) AS KeyVal
		FROM [dbo].[EncryptionKeys]
		WHERE ID = @ID
END
GO

GRANT EXECUTE ON [dbo].[GetItem] TO KeyReader
GO

