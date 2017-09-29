IF(SELECT COUNT(*) FROM master.sys.symmetric_keys WHERE name like '%ServiceMasterKey%') = 0
BEGIN
	RAISERROR('No service master key defined, unable to proceed',18,-1) 
END

IF (SELECT COUNT(*) FROM sys.symmetric_keys WHERE name LIKE '%DatabaseMasterKey%') = 0
BEGIN
	CREATE MASTER KEY ENCRYPTION BY PASSWORD = '';
END

IF(SELECT COUNT(*) FROM sys.database_principals WHERE name='KeyReader' AND Type = 'R') = 0
BEGIN
	CREATE ROLE KeyReader AUTHORIZATION dbo
END


IF (SELECT COUNT(*) FROM sys.certificates WHERE name = 'key_cert') = 0
BEGIN
	CREATE CERTIFICATE key_cert AUTHORIZATION dbo WITH SUBJECT = 'Key Certificate'
END

IF(SELECT COUNT(*) FROM sys.symmetric_keys WHERE name = 'key_key') = 0
BEGIN
	CREATE SYMMETRIC KEY key_key WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE key_cert
END

GRANT VIEW DEFINITION ON CERTIFICATE::key_cert TO KeyReader
GO

GRANT VIEW DEFINITION ON SYMMETRIC KEY::key_key TO KeyReader
GO
