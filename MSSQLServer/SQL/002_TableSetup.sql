IF(SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'EncryptionKeys') = 0
BEGIN
	CREATE TABLE [dbo].[EncryptionKeys] (
		ID nvarchar(100) not null PRIMARY KEY,
		KeyVal varbinary(max) 
	)
END
