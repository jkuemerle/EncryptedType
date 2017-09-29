IF(SELECT COUNT(*) FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[UpdateItem]') AND type IN (N'P', N'PC')) <> 0
BEGIN
	DROP PROCEDURE [dbo].[UpdateItem]
END
GO

CREATE PROCEDURE [dbo].[UpdateItem]
	@ID nvarchar(100), @KeyVal nvarchar(max)
AS
BEGIN
	OPEN SYMMETRIC KEY key_key DECRYPTION BY CERTIFICATE key_cert
	MERGE INTO [dbo].[EncryptionKeys] AS target 
	USING (SELECT @ID, @KeyVal ) 
		AS source (ID, KeYval)
	ON target.ID = source.ID
	WHEN MATCHED THEN
		UPDATE SET 
			Keyval = ENCRYPTBYKEY(Key_GUID('key_key'),source.KeyVal,1,source.ID) 
	WHEN NOT MATCHED THEN
		INSERT (ID, Keyval) 
		VALUES (source.ID,  
			ENCRYPTBYKEY(Key_GUID('key_key'),source.Keyval,1,source.ID)
		);
	CLOSE ALL SYMMETRIC KEYS
END
GO

DENY EXECUTE ON [dbo].[UpdateItem] TO KeyReader
GO

