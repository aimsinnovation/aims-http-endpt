<#
    .SYNOPSIS
    Validates and applies AIMS HTTP agent configuration.

    .DESCRIPTION
    Validates the configuration provided and applies to the AIMS HTTP agent.
    The script will create systems and nodes that are present in the configuration.

    .PARAMETER Configuration
    The configuration file.

    .PARAMETER Username
    The AIMS username.
    Note, you need to have administrative priviledges in order to connect the agent.

    .PARAMETER Password
    The AIMS password.

    .INPUTS
    None. You cannot pipe objects to the script.

    .OUTPUTS
    None.

    .EXAMPLE
    C:\PS> configure -configuration "my.configuration.json"
#>
param (
    [Parameter(Mandatory=$true)] [string]$Configuration,
    [Parameter(Mandatory=$true)] [string]$Username,
    [Parameter(Mandatory=$true)] [SecureString]$Password
)

function GetInstallPath() {
    return (Get-ItemProperty  -Path "HKLM:\SOFTWARE\AIMS Innovation\HTTP Endpoints Activity Agent").'(default)'
}

function GetInternalConfig() {
    [string]$path = "{0}\aims\httpagent\agent.config" -f $env:LOCALAPPDATA
    [string]$content = Get-Content -Path $path
    return ParseJson -in $content
}

function WriteInternalConfig($config) {
    [string]$path = "{0}\aims\httpagent\agent.config" -f $env:LOCALAPPDATA
    $config | ConvertTo-Json -Depth 10 | Out-File $path
}

function ApiInstance([string]$endpoint, [string]$usename, [string]$password) {
    [string]$installpath = GetInstallPath
    [string]$path = "{0}\Aims.Sdk.dll" -f $installpath
    Add-Type -Path $path

    [object]$credentials = New-Object Aims.Sdk.HttpBasicCredentials($usename, $password)
    [object]$uri = New-Object System.Uri($endpoint)

    [object]$api = New-Object Aims.Sdk.Api($uri, $credentials)

    return $api
}

function CheckSchema([string]$config) {
    Import-Module -Name "Microsoft.PowerShell.Utility"   
    [string]$installpath = GetInstallPath
    [string]$path = "{0}\config.schema.json" -f $installpath
    [string]$schema = Get-Content -Path $path
    Test-Json -Json $config -Schema $schema -ErrorAction Stop | out-null
}

function CheckReferences ([object]$json, [object]$sub) {
    [object]$current = if ($null -ne $sub) { $sub } else { $json }
    $current.psobject.Properties | ForEach-Object {
        if ($_.TypeNameOfValue -eq 'System.Management.Automation.PSCustomObject') {
            if ($_.Value.'$ref') {
                $referenced = $_.Value.'$ref'.Replace("#/","").split("/") | ForEach-Object {$accumulator = $json} { 
                    if ($_ -match "^\d+$") { $accumulator = $accumulator[$_] } else { $accumulator = $accumulator.$_ } } { $accumulator }
                if ($null -eq $referenced) {
                    [string]$message = "Invalid reference: {0}" -f $_.Value.'$ref'
                    throw $message
                }
            }
        } elseif ($_.TypeNameOfValue -eq 'System.Object[]') {
            $_.Value | ForEach-Object { CheckReferences -json $json -sub $_}
        }
    }
}

function ParseJson([string]$in)
{
    [object]$json = ConvertFrom-Json -InputObject $in
    CheckReferences -json $json -ErrorAction Stop
    return $json
}

function Protect([string]$data) {
    $utf8 = [System.Text.Encoding]::UTF8;
    $bytes = $utf8.GetBytes($data)
    $protected = [System.Security.Cryptography.ProtectedData]::Protect($bytes, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
    return [System.Convert]::ToBase64String($protected)
}

function InjectToken([object]$system, [string]$token, [int]$id) {
    Add-Member -InputObject $system -NotePropertyValue $id -NotePropertyName id
    Add-Member -InputObject $system -NotePropertyValue $token -NotePropertyName 'token'
}

function CreateSystem([object]$system, [object]$api, [string]$environment, $existing) {
    "Registering system '{0}'..." -f $system.name | Write-Host

    [object]$AimsSystem = $existing | Where-Object { $_.name -like $system.name }

    if ($null -eq $AimsSystem) {
        "System '{0}' not found, creating..." -f $system.name | Write-Host
        [object]$SystemToAdd = New-Object Aims.SDK.System
        $SystemToAdd.Name = $system.name
        $SystemToAdd.AgentId = "aims.http-endpt"
        $SystemToAdd.Version = "v1.5"
        $SystemToAdd.EnvironmentId = $environment
        $AimsSystem = $api.ForEnvironment($environment).Systems.Add($SystemToAdd)
    } else {
        "System '{0}' found, using existing..." -f $system.name | Write-Host
        if ($AimsSystem -is [array]) {
            $AimsSystem = $AimsSystem[0]
        }
    }

    $token = $api.Auth.GetAgentToken($AimsSystem)
    $ProtectedToken = Protect -data $token
    InjectToken -system $system -token $ProtectedToken -id $AimsSystem.id
    Write-Host "Ready"
}

function GetSystems([object]$api, [string]$environment) {
    "Checking if environment '{0}' exists..." -f $environment | Write-Host

    $systems = $api.ForEnvironment($environment).Systems.Get()
    if ($null -eq $systems) {
        throw "Environment {0} does not exist" -f $environment
    }
    return $systems
}

function GetPassword([System.Security.SecureString]$password)
{
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password);
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr);
    }
    finally {
        [Runtime.InteropServices.Marshal]::FreeBSTR($bstr);
    }
}

function RestartService() {
    [string]$name = "aims-http-endpt-agent"
    if ((Get-Service -Name $name).Status -eq 'Running')
    {
        Get-Service -Name $name | Stop-Service
        Get-Service -Name $name | Start-Service
    }
    else
    {
        Get-Service -Name $name | Start-Service
    }
}

$ErrorActionPreference = "Stop"

Write-Output "Validating the config..."
[string]$NewJson = Get-Content -Path $Configuration 

CheckSchema -config $NewJson

[object]$NewConfig = ParseJson -in $NewJson

Write-Output "Loading existing config..."
[object]$OldConfig = GetInternalConfig

[string]$PlainPassword = GetPassword -password $Password
[object]$api = ApiInstance -endpoint $NewConfig.endpoint -usename $Username -password $PlainPassword

$systems = GetSystems -api $api -environment $NewConfig.environment

Write-Output "Applying changes..."
$NewConfig.systems | ForEach-Object { 
    [object]$system = $_
    [object]$existing = $OldConfig.systems | Where-Object { $_.name -like $system.name }
    if ( $null -ne $existing ) {
        InjectToken -system $system -token $existing.token -id $existing.id
    } else {
        CreateSystem -system $system -api $api -environment $NewConfig.environment -existing $systems
    }
}

if ( $null -ne $NewConfig.authentication.basic ) {
    $NewConfig.authentication.basic | ForEach-Object { $_.password = Protect -data $_.password }
}

Write-Output "Updating the config..."
WriteInternalConfig -config $NewConfig
Write-Output "Ready"

Write-Output "Restarting the agent service..."
RestartService
Write-Output "Ready"
