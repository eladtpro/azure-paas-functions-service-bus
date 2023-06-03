CREATE USER functionuser WITH PASSWORD = 'P@ssw0rdWonderfull2020!';
EXEC sp_addrolemember 'db_datareader', 'functionuser';
EXEC sp_addrolemember 'db_datawriter', 'functionuser';

-- ALTER ROLE db_datareader ADD MEMBER db_datareader;
-- ALTER ROLE db_datawriter ADD MEMBER db_datareader;
GO

select dp.NAME AS principal_name,
       dp.type_desc AS principal_type_desc,
       o.NAME AS object_name,
       p.permission_name,
       p.state_desc AS permission_state_desc
from   sys.database_permissions p
left   OUTER JOIN sys.all_objects o
on     p.major_id = o.OBJECT_ID
inner  JOIN sys.database_principals dp
on     p.grantee_principal_id = dp.principal_id
