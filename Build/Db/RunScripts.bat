setlocal

set logServer=localhost
set logUserName=sa
set logPassword=newtime

sqlcmd.exe -S %logServer% -U %logUserName% -P %logPassword% -i CreateTable_LogEvent.sql -o ScriptLogs.txt
sqlcmd.exe -S %logServer% -U %logUserName% -P %logPassword% -i CreateJob_LogMessageCleaning.sql -o ScriptLogs.txt

pause

endlocal