param([string]$databaseName,
[string]$pathToPostgres='c:\Program Files\PostgreSQL\9.5\bin',
[string]$postgresHost='localhost',
[int]$postgresPort=5432,
[string]$postgresUser='postgres',
[string]$password='Qwerty1234')

#do not allow empty $databaseName
if([string]::IsNullOrWhiteSpace($databaseName)){
	$databaseName = Read-Host -Prompt "Please enter database name"
	
	if([string]::IsNullOrWhiteSpace($databaseName)){
		Write-Host "Database name is not entered."
		Exit
	}
}

$isExistsPostgres = Test-Path $pathToPostgres
if (!$isExistsPostgres) {
	Write-Host "Postgres path don't found. "+$pathToPostgres
	Exit
}


function RunMigrator()
{
	GetListDatabasesWithName
	CreateNewDatabase
	MigrateDb ($databaseName + "-Plain") "plainstore"
	MigrateDb ($databaseName + "-Views") "readside"
	MigrateDb ($databaseName + "-Evn")   "events"
}

function MigrateDb([string] $sourceDb, [string] $schemaName)
{
	$psqlPath = GetPathToPsql
	$pgrestorePath = GetPathToPgRestore
	
	$fileForBackup = (Join-Path (Get-Location).Path "db_backup.db")
	
	RunBackupForDb $sourceDb $fileForBackup
	CreateSchemaPublic
	RestoreBackupInNewDb $fileForBackup
	RenameSchemaPublicTo $schemaName
}

function CreateSchemaPublic()
{
	$psqlPath = GetPathToPsql
	
	Write-Host "Rename schema public to $schemaName"
	$command = """$psqlPath"" -h $postgresHost -p $postgresPort -U $postgresUser -d $databaseName -U $postgresUser -c ""CREATE SCHEMA IF NOT EXISTS public"""
	RunCommand $command
}

function RenameSchemaPublicTo([string] $schemaName)
{
	$psqlPath = GetPathToPsql
	
	Write-Host "Rename schema public to $schemaName"
	$command = """$psqlPath"" -h $postgresHost -p $postgresPort -U $postgresUser -d $databaseName -U $postgresUser -c ""ALTER SCHEMA public RENAME TO $schemaName"""
	RunCommand $command
}

function RestoreBackupInNewDb([string] $backupPath)
{
	$restorePath = GetPathToPgRestore
	
	Write-Host "Restore database $databaseName from $backupPath."
	$command = """$restorePath"" --host $postgresHost --port $postgresPort --username $postgresUser -O --dbname $databaseName --verbose ""$backupPath"""
	RunCommand $command
	Write-Host "Restore finished for $databaseName."
}

function RunBackupForDb([string] $sourceDb, [string] $backupPath)
{
	$pddumpPath = GetPathToPgDump
	
	Write-Host "Backup database $sourceDb."
	$command = """$pddumpPath"" -h $postgresHost -p $postgresPort -U $postgresUser -O -F t -v -f ""$backupPath"" -n public $sourceDb"
	RunCommand $command
	Write-Host "Backup finished for $sourceDb."
}

function CreateNewDatabase()
{
	$createDbPath = GetPathToCreateDb
	
	Write-Host "Create database $databaseName"
	#$command = """$psqlPath"" -h $postgresHost -p $postgresPort -U $postgresUser -c 'CREATE DATABASE ""$databaseName"" WITH ENCODING = ""UTF8""'"
	$command = """$createDbPath"" -h $postgresHost -p $postgresPort -U $postgresUser ""$databaseName"""
	RunCommand $command
}

function GetListDatabasesWithName()
{
	$psqlPath = GetPathToPsql
	
	Write-Host "Get list all db names."
	$command = """$psqlPath"" -h $postgresHost -p $postgresPort -U $postgresUser -c ""SELECT datname FROM pg_database WHERE datistemplate = false AND datname LIKE '$databaseName%';"""
	RunCommand $command
}

function RunCommand([string] $command)
{
	Write-Host $command

	Set-Item -path env:PGPASSWORD -value ($password)
	iex "& $command"
	Set-Item -path env:PGPASSWORD -value ("")
}

function GetPathToPgDump() {
	return Join-Path $pathToPostgres 'pg_dump.exe' 
}

function GetPathToPsql() {
	return Join-Path $pathToPostgres 'psql.exe'
}

function GetPathToPgRestore() {
	return Join-Path $pathToPostgres 'pg_restore.exe'
}

function GetPathToCreateDb() {
	return Join-Path $pathToPostgres 'createdb.exe'
}

function ThrowIfNotExists([string] $path)
{
	if (Test-Path $path) {
		return;
	}

	throw [System.IO.FileNotFoundException] "$path not found, see script for details"
}



# Main part
Write-Host "Migration is started."
ThrowIfNotExists (GetPathToPgDump)
ThrowIfNotExists (GetPathToPsql)
ThrowIfNotExists (GetPathToPgRestore)
ThrowIfNotExists (GetPathToCreateDb)
	
RunMigrator	
Write-Host "Migration is finished."