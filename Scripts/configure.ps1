<#
    .SYNOPSIS
    Validates and applies AIMS HTTP agent configuration.

    .DESCRIPTION
    Validates the configuration provided and applies to the AIMS HTTP agent.
    The script will create systems and nodes that are present in the configuration.

    .PARAMETER Configuration
    The configuration file.

    .INPUTS
    None. You cannot pipe objects to the script.

    .OUTPUTS
    None.

    .EXAMPLE
    C:\PS> configure -configuration "my.configuration.json"
#>
param (
    [Parameter(Mandatory=$true)] [string]$Configuration = 'C:\Users\tigran\Documents\projects\http.config.json'
)

function GetInstallPath() {
    return (Get-ItemProperty  -Path "HKLM:\SOFTWARE\AIMS Innovation\HTTP Endpoints Activity Agent").'(default)'
}

function CheckSchema([string]$config) {
    Import-Module -Name "Microsoft.PowerShell.Utility"   
    [string]$installpath = GetInstallPath
    [string]$path = "{0}\config.schema.json" -f $installpath
    [string]$schema = Get-Content -Path $path
    Test-Json -Json $config -Schema $schema -ErrorAction Stop | out-null
}

function ResolveReferences ([object]$json, [object]$sub) {
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
                $_.Value = $referenced
            }
        } elseif ($_.TypeNameOfValue -eq 'System.Object[]') {
            $_.Value | ForEach-Object { ResolveReferences -json $json -sub $_}
        }
    }
}

function ParseJson([string]$in)
{
    [object]$json = ConvertFrom-Json -InputObject $in
    ResolveReferences -json $json -ErrorAction Stop
    return $json
}

[string]$NewConfig = Get-Content -Path $Configuration 

CheckSchema -config $NewConfig

[object]$json = ParseJson -in $NewConfig

