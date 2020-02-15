
$User = ".\Admin"
$PWord = ConvertTo-SecureString -String "12345" -AsPlainText -Force
$Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User, $PWord

#Run by User:
#New-Service -Name "Swr.Capital1C.Service" -BinaryPathName ($PSScriptRoot + "\Swr.Capital1C.Service\Swr.Capital1C.WindowsService.exe") -Credential $Credential -Description "PDM integration module" -DisplayName "Swr.Capital1C.Service" -StartupType Automatic

#Run by System:
New-Service -Name "Swr.Capital1C.Service" -BinaryPathName ($PSScriptRoot + "\Swr.Capital1C.Service\Swr.Capital1C.WindowsService.exe") -Description "PDM integration module" -DisplayName "Swr.Capital1C.Service" -StartupType Automatic
Start-Service -Name "Swr.Capital1C.Service"

"Installation completed"