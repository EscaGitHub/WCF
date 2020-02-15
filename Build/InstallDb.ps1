
$Server = "localhost"
$User = "sa"
$Password = "newtime"

Invoke-sqlcmd -ServerInstance $Server -InputFile ($PSScriptRoot + "\Db\CreateTable_LogEvent.sql") -Username $User -Password $Password | Out-File -FilePath ($PSScriptRoot + "\Db\ScriptLogs.txt")

Invoke-sqlcmd -ServerInstance $Server -InputFile ($PSScriptRoot + "\Db\CreateJob_LogMessageCleaning.sql") -Username $User -Password $Password | Out-File -FilePath ($PSScriptRoot + "\Db\ScriptLogs.txt")
