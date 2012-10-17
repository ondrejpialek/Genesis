# Copyright (c) Microsoft Corporation.  All rights reserved.

$InitialDatabase = '0'

$knownExceptions = @(
    'System.Data.Entity.Migrations.Infrastructure.MigrationsException',
    'System.Data.Entity.Migrations.Infrastructure.AutomaticMigrationsDisabledException',
    'System.Data.Entity.Migrations.Infrastructure.AutomaticDataLossException',
    'System.Data.Entity.Migrations.MigrationsPendingException',
    'System.Data.Entity.Migrations.ProjectTypeNotSupportedException'
)

<#
.SYNOPSIS
    Enables Code First Migrations in a project.

.DESCRIPTION
    Enables Migrations by scaffolding a migrations configuration class in the project. If the
    target database was created by an initializer, an initial migration will be created (unless
    automatic migrations are enabled via the EnableAutomaticMigrations parameter).

.PARAMETER ContextTypeName
    Specifies the context to use. If omitted, migrations will attempt to locate a
    single context type in the target project.

.PARAMETER EnableAutomaticMigrations
    Specifies whether automatic migrations will be enabled in the scaffolded migrations configuration.
    If omitted, automatic migrations will be disabled.

.PARAMETER MigrationsDirectory
    Specifies the name of the directory that will contain migrations code files.
    If omitted, the directory will be named "Migrations". 

.PARAMETER ProjectName
    Specifies the project that the scaffolded migrations configuration class will
    be added to. If omitted, the default project selected in package manager
    console is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.

.PARAMETER Force
    Specifies that the migrations configuration be overwritten when running more
    than once for a given project.
#>
function Enable-Migrations
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')] 
    param (
        [string] $ContextTypeName,
        [alias('Auto')]
        [switch] $EnableAutomaticMigrations,
        [string] $MigrationsDirectory,
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName,
        [switch] $Force
    )

    $runner = New-MigrationsRunner $ProjectName $StartUpProjectName $null $ConnectionStringName $ConnectionString $ConnectionProviderName

    try
    {
        Invoke-RunnerCommand $runner System.Data.Entity.Migrations.EnableMigrationsCommand @( $EnableAutomaticMigrations.IsPresent, $Force.IsPresent ) @{ 'ContextTypeName' = $ContextTypeName; 'MigrationsDirectory' = $MigrationsDirectory }
        $error = Get-RunnerError $runner
        
        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }

            throw $error.Message
        }
    }
    finally
    {
        Remove-Runner $runner
    }
}

<#
.SYNOPSIS
    Scaffolds a migration script for any pending model changes.

.DESCRIPTION
    Scaffolds a new migration script and adds it to the project.

.PARAMETER Name
    Specifies the name of the custom script.

.PARAMETER Force
    Specifies that the migration user code be overwritten when re-scaffolding an
    existing migration.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If omitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.

.PARAMETER IgnoreChanges
    Scaffolds an empty migration ignoring any pending changes detected in the current model.
    This can be used to create an initial, empty migration to enable Migrations for an existing
    database. N.B. Doing this assumes that the target database schema is compatible with the
    current model.

#>
function Add-Migration
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [parameter(Position = 0,
            Mandatory = $true)]
        [string] $Name,
        [switch] $Force,
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName,
        [switch] $IgnoreChanges)

    $runner = New-MigrationsRunner $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName

    try
    {
        Invoke-RunnerCommand $runner System.Data.Entity.Migrations.AddMigrationCommand @( $Name, $Force.IsPresent, $IgnoreChanges.IsPresent )
        $error = Get-RunnerError $runner
        
        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }

            throw $error.Message
        }
    }
    finally
    {
        Remove-Runner $runner
    }
}

<#
.SYNOPSIS
    Applies any pending migrations to the database.

.DESCRIPTION
    Updates the database to the current model by applying pending migrations.

.PARAMETER SourceMigration
    Only valid with -Script. Specifies the name of a particular migration to use
    as the update's starting point. If omitted, the last applied migration in
    the database will be used.

.PARAMETER TargetMigration
    Specifies the name of a particular migration to update the database to. If
    omitted, the current model will be used.

.PARAMETER Script
    Generate a SQL script rather than executing the pending changes directly.

.PARAMETER Force
    Specifies that data loss is acceptable during automatic migration of the
    database.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If omitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.
#>
function Update-Database
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [string] $SourceMigration,
        [string] $TargetMigration,
        [switch] $Script,
        [switch] $Force,
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName)

    $runner = New-MigrationsRunner $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName

    try
    {
        Invoke-RunnerCommand $runner System.Data.Entity.Migrations.UpdateDatabaseCommand @( $SourceMigration, $TargetMigration, $Script.IsPresent, $Force.IsPresent, $Verbose.IsPresent )
        $error = Get-RunnerError $runner
        
        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }

            throw $error.Message
        }
    }
    finally
    {
        Remove-Runner $runner
    }
}

<#
.SYNOPSIS
    Displays the migrations that have been applied to the target database.

.DESCRIPTION
    Displays the migrations that have been applied to the target database.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If omitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.
#>
function Get-Migrations
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName)

    $runner = New-MigrationsRunner $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName

    try
    {
        Invoke-RunnerCommand $runner System.Data.Entity.Migrations.GetMigrationsCommand
        $error = Get-RunnerError $runner
        
        if ($error)
        {
            if ($knownExceptions -notcontains $error.TypeName)
            {
                Write-Host $error.StackTrace
            }

            throw $error.Message
        }
    }
    finally
    {
        Remove-Runner $runner
    }
}

function New-MigrationsRunner($ProjectName, $StartUpProjectName, $ConfigurationTypeName, $ConnectionStringName, $ConnectionString, $ConnectionProviderName)
{
    $startUpProject = Get-MigrationsStartUpProject $StartUpProjectName $ProjectName
    Build-Project $startUpProject

    $project = Get-MigrationsProject $ProjectName
    Build-Project $project

    $installPath = Get-EntityFrameworkInstallPath $project
    $toolsPath = Join-Path $installPath tools

    $info = New-Object System.AppDomainSetup -Property @{
            ShadowCopyFiles = 'true';
            ApplicationBase = $installPath;
            PrivateBinPath = 'tools';
            ConfigurationFile = ([AppDomain]::CurrentDomain.SetupInformation.ConfigurationFile)
        }
    
    $targetFrameworkVersion = (New-Object System.Runtime.Versioning.FrameworkName ($project.Properties.Item('TargetFrameworkMoniker').Value)).Version

    if ($targetFrameworkVersion -lt (New-Object Version @( 4, 5 )))
    {
        $info.PrivateBinPath += ';lib\net40'
    }
    else
    {
        $info.PrivateBinPath += ';lib\net45'
    }

    $domain = [AppDomain]::CreateDomain('Migrations', $null, $info)
    $domain.SetData('project', $project)
    $domain.SetData('startUpProject', $startUpProject)
    $domain.SetData('configurationTypeName', $ConfigurationTypeName)
    $domain.SetData('connectionStringName', $ConnectionStringName)
    $domain.SetData('connectionString', $ConnectionString)
    $domain.SetData('connectionProviderName', $ConnectionProviderName)
    
    [AppDomain]::CurrentDomain.SetShadowCopyFiles()
    $utilityAssembly = [System.Reflection.Assembly]::LoadFrom((Join-Path $toolsPath EntityFramework.PowerShell.Utility.dll))
    $dispatcher = $utilityAssembly.CreateInstance(
        'System.Data.Entity.Migrations.Utilities.DomainDispatcher',
        $false,
        [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Public,
        $null,
        $PSCmdlet,
        $null,
        $null)        
    $domain.SetData('efDispatcher', $dispatcher)

    return @{
        Domain = $domain;
        ToolsPath = $toolsPath
    }
}

function Remove-Runner($runner)
{
    [AppDomain]::Unload($runner.Domain)
}

function Invoke-RunnerCommand($runner, $command, $parameters, $anonymousArguments)
{
    $domain = $runner.Domain

	if ($anonymousArguments)
	{
		$anonymousArguments.GetEnumerator() | %{
			$domain.SetData($_.Name, $_.Value)
		}
	}

    $domain.CreateInstanceFrom(
        (Join-Path $runner.ToolsPath EntityFramework.PowerShell.dll),
        $command,
        $false,
        0,
        $null,
        $parameters,
        $null,
        $null) | Out-Null
}

function Get-RunnerError($runner)
{
    $domain = $runner.Domain

    if (!$domain.GetData('wasError'))
    {
        return $null
    }

    return @{
            Message = $domain.GetData('error.Message');
            TypeName = $domain.GetData('error.TypeName');
            StackTrace = $domain.GetData('error.StackTrace')
    }
}

function Get-MigrationsProject($name, $hideMessage)
{
    if ($name)
    {
        return Get-SingleProject $name
    }

    $project = Get-Project
    $projectName = $project.Name

    if (!$hideMessage)
    {
        Write-Verbose "Using NuGet project '$projectName'."
    }

    return $project
}

function Get-MigrationsStartUpProject($name, $fallbackName)
{    
    $startUpProject = $null

    if ($name)
    {
        $startUpProject = Get-SingleProject $name
    }
    else
    {
        $startupProjectPaths = $DTE.Solution.SolutionBuild.StartupProjects

        if ($startupProjectPaths)
        {
            if ($startupProjectPaths.Length -eq 1)
            {
                $startupProjectPath = $startupProjectPaths[0]

                if (!(Split-Path -IsAbsolute $startupProjectPath))
                {
                    $solutionPath = Split-Path $DTE.Solution.Properties.Item('Path').Value
                    $startupProjectPath = Join-Path $solutionPath $startupProjectPath -Resolve
                }

                $startupProject = Get-SolutionProjects | ?{
					try
					{
						$fullName = $_.FullName
					}
					catch [NotImplementedException]
					{
						return $false
					}

                    if ($fullName -and $fullName.EndsWith('\'))
                    {
                        $fullName = $fullName.Substring(0, $fullName.Length - 1)
                    }

                    return $fullName -eq $startupProjectPath
                }
            }
            else
            {
                Write-Verbose 'More than one start-up project found.'
            }
        }
        else
        {
            Write-Verbose 'No start-up project found.'
        }
    }

    if (!($startUpProject -and (Test-StartUpProject $startUpProject)))
    {
        $startUpProject = Get-MigrationsProject $fallbackName $true
        $startUpProjectName = $startUpProject.Name

        Write-Warning "Cannot determine a valid start-up project. Using project '$startUpProjectName' instead. Your configuration file and working directory may not be set as expected. Use the -StartUpProjectName parameter to set one explicitly. Use the -Verbose switch for more information."
    }
    else
    {
        $startUpProjectName = $startUpProject.Name

        Write-Verbose "Using StartUp project '$startUpProjectName'."
    }

    return $startUpProject
}

function Get-SolutionProjects()
{
    $projects = New-Object System.Collections.Stack
    
    $DTE.Solution.Projects | %{
        $projects.Push($_)
    }
    
    while ($projects.Count -ne 0)
    {
        $project = $projects.Pop();
        
        # NOTE: This line is similar to doing a "yield return" in C#
        $project

        if ($project.ProjectItems)
        {
            $project.ProjectItems | ?{ $_.SubProject } | %{
                $projects.Push($_.SubProject)
            }
        }
    }
}

function Get-SingleProject($name)
{
    $project = Get-Project $name

    if ($project -is [array])
    {
        throw "More than one project '$name' was found. Specify the full name of the one to use."
    }

    return $project
}

function Test-StartUpProject($project)
{    
    if ($project.Kind -eq '{cc5fd16d-436d-48ad-a40c-5a424c6e3e79}')
    {
        $projectName = $project.Name
        Write-Verbose "Cannot use start-up project '$projectName'. The Windows Azure Project type isn't supported."
        
        return $false
    }
    
    return $true
}

function Build-Project($project)
{
    $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name

    $DTE.Solution.SolutionBuild.BuildProject($configuration, $project.UniqueName, $true)

    if ($DTE.Solution.SolutionBuild.LastBuildInfo)
    {
        $projectName = $project.Name

        throw "The project '$projectName' failed to build."
    }
}

function Get-EntityFrameworkInstallPath($project)
{
    $package = Get-Package -ProjectName $project.FullName | ?{ $_.Id -eq 'EntityFramework' }

    if (!$package)
    {
        $projectName = $project.Name

        throw "The EntityFramework package is not installed on project '$projectName'."
    }
    
    return Get-PackageInstallPath $package
}
    
function Get-PackageInstallPath($package)
    {
    $componentModel = Get-VsComponentModel
    $packageInstallerServices = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])

    $vsPackage = $packageInstallerServices.GetInstalledPackages() | ?{ $_.Id -eq $package.Id -and $_.Version -eq $package.Version }
    
    return $vsPackage.InstallPath
}

Export-ModuleMember @( 'Enable-Migrations', 'Add-Migration', 'Update-Database', 'Get-Migrations' ) -Variable InitialDatabase

# SIG # Begin signature block
# MIIaXAYJKoZIhvcNAQcCoIIaTTCCGkkCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUxSlWxHe0+ZgwfZR9Dzu4QZn3
# 0WygghUvMIIEmTCCA4GgAwIBAgITMwAAAJ0ejSeuuPPYOAABAAAAnTANBgkqhkiG
# 9w0BAQUFADB5MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSMw
# IQYDVQQDExpNaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQTAeFw0xMjA5MDQyMTQy
# MDlaFw0xMzAzMDQyMTQyMDlaMIGDMQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
# aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
# cnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMR4wHAYDVQQDExVNaWNyb3NvZnQgQ29y
# cG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC6pElsEPsi
# nGWiFpg7y2Fi+nQprY0GGdJxWBmKXlcNaWJuNqBO/SJ54B3HGmGO+vyjESUWyMBY
# LDGKiK4yHojbfz50V/eFpDZTykHvabhpnm1W627ksiZNc9FkcbQf1mGEiAAh72hY
# g1tJj7Tf0zXWy9kwn1P8emuahCu3IWd01PZ4tmGHmJR8Ks9n6Rm+2bpj7TxOPn0C
# 6/N/r88Pt4F+9Pvo95FIu489jMgHkxzzvXXk/GMgKZ8580FUOB5UZEC0hKo3rvMA
# jOIN+qGyDyK1p6mu1he5MPACIyAQ+mtZD+Ctn55ggZMDTA2bYhmzu5a8kVqmeIZ2
# m2zNTOwStThHAgMBAAGjggENMIIBCTATBgNVHSUEDDAKBggrBgEFBQcDAzAdBgNV
# HQ4EFgQU3lHcG/IeSgU/EhzBvMOzZSyRBZgwHwYDVR0jBBgwFoAUyxHoytK0FlgB
# yTcuMxYWuUyaCh8wVgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2NybC5taWNyb3Nv
# ZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljQ29kU2lnUENBXzA4LTMxLTIwMTAu
# Y3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDovL3d3dy5taWNy
# b3NvZnQuY29tL3BraS9jZXJ0cy9NaWNDb2RTaWdQQ0FfMDgtMzEtMjAxMC5jcnQw
# DQYJKoZIhvcNAQEFBQADggEBACqk9+7AwyZ6g2IaeJxbxf3sFcSneBPRF1MoCwwA
# Qj84D4ncZBmENX9Iuc/reomhzU+p4LvtRxD+F9qHiRDRTBWg8BH/2pbPZM+B/TOn
# w3iT5HzVbYdx1hxh4sxOZLdzP/l7JzT2Uj9HQ8AOgXBTwZYBoku7vyoDd3tu+9BG
# ihcoMaUF4xaKuPFKaRVdM/nff5Q8R0UdrsqLx/eIHur+kQyfTwcJ7SaSbrOUGQH4
# X4HnrtqJj39aXoRftb58RuVHr/5YK5F/h9xGH1GVzMNiobXHX+vJaVxxkamNViAs
# Ok6T/ZsGj62K+Gh+O7p5QpM5SfXQXuxwjUJ1xYJVkBu1VWEwggTDMIIDq6ADAgEC
# AhMzAAAAKzkySMGyyUjzAAAAAAArMA0GCSqGSIb3DQEBBQUAMHcxCzAJBgNVBAYT
# AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYD
# VQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAfBgNVBAMTGE1pY3Jvc29mdCBU
# aW1lLVN0YW1wIFBDQTAeFw0xMjA5MDQyMTEyMzRaFw0xMzEyMDQyMTEyMzRaMIGz
# MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVk
# bW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQLEwRN
# T1BSMScwJQYDVQQLEx5uQ2lwaGVyIERTRSBFU046QzBGNC0zMDg2LURFRjgxJTAj
# BgNVBAMTHE1pY3Jvc29mdCBUaW1lLVN0YW1wIFNlcnZpY2UwggEiMA0GCSqGSIb3
# DQEBAQUAA4IBDwAwggEKAoIBAQCmtjAOA2WuUFqGa4WfSKEeycDuXkkHheBwlny+
# uV9iXwYm04s5uxgipS6SrdhLiDoar5uDrsheOYzCMnsWeO03ODrxYvtoggJo7Ou7
# QIqx/qEsNmJgcDlgYg77xhg4b7CS1kANgKYNeIs2a4aKJhcY/7DrTbq7KRPmXEiO
# cEY2Jv40Nas04ffa2FzqmX0xt00fV+t81pUNZgweDjIXPizVgKHO6/eYkQLcwV/9
# OID4OX9dZMo3XDtRW12FX84eHPs0vl/lKFVwVJy47HwAVUZbKJgoVkzh8boJGZaB
# SCowyPczIGznacOz1MNOzzAeN9SYUtSpI0WyrlxBSU+0YmiTAgMBAAGjggEJMIIB
# BTAdBgNVHQ4EFgQUpRgzUz+VYKFDFu+Oxq/SK7qeWNAwHwYDVR0jBBgwFoAUIzT4
# 2VJGcArtQPt2+7MrsMM1sw8wVAYDVR0fBE0wSzBJoEegRYZDaHR0cDovL2NybC5t
# aWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljcm9zb2Z0VGltZVN0YW1w
# UENBLmNybDBYBggrBgEFBQcBAQRMMEowSAYIKwYBBQUHMAKGPGh0dHA6Ly93d3cu
# bWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljcm9zb2Z0VGltZVN0YW1wUENBLmNy
# dDATBgNVHSUEDDAKBggrBgEFBQcDCDANBgkqhkiG9w0BAQUFAAOCAQEAfsywe+Uv
# vudWtc9z26pS0RY5xrTN+tf+HmW150jzm0aIBWZqJoZe/odY3MZjjjiA9AhGfCtz
# sQ6/QarLx6qUpDfwZDnhxdX5zgfOq+Ql8Gmu1Ebi/mYyPNeXxTIh+u4aJaBeDEIs
# ETM6goP97R2zvs6RpJElcbmrcrCer+TPAGKJcKm4SlCM7i8iZKWo5k1rlSwceeyn
# ozHakGCQpG7+kwINPywkDcZqJoFRg0oQu3VjRKppCMYD6+LPC+1WOuzvcqcKDPQA
# 0yK4ryJys+fEnAsooIDK4+HXOWYw50YXGOf6gvpZC3q8qA3+HP8Di2OyTRICI08t
# s4WEO+KhR+jPFTCCBbwwggOkoAMCAQICCmEzJhoAAAAAADEwDQYJKoZIhvcNAQEF
# BQAwXzETMBEGCgmSJomT8ixkARkWA2NvbTEZMBcGCgmSJomT8ixkARkWCW1pY3Jv
# c29mdDEtMCsGA1UEAxMkTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9y
# aXR5MB4XDTEwMDgzMTIyMTkzMloXDTIwMDgzMTIyMjkzMloweTELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEjMCEGA1UEAxMaTWljcm9zb2Z0IENv
# ZGUgU2lnbmluZyBQQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCy
# cllcGTBkvx2aYCAgQpl2U2w+G9ZvzMvx6mv+lxYQ4N86dIMaty+gMuz/3sJCTiPV
# cgDbNVcKicquIEn08GisTUuNpb15S3GbRwfa/SXfnXWIz6pzRH/XgdvzvfI2pMlc
# RdyvrT3gKGiXGqelcnNW8ReU5P01lHKg1nZfHndFg4U4FtBzWwW6Z1KNpbJpL9oZ
# C/6SdCnidi9U3RQwWfjSjWL9y8lfRjFQuScT5EAwz3IpECgixzdOPaAyPZDNoTgG
# hVxOVoIoKgUyt0vXT2Pn0i1i8UU956wIAPZGoZ7RW4wmU+h6qkryRs83PDietHdc
# pReejcsRj1Y8wawJXwPTAgMBAAGjggFeMIIBWjAPBgNVHRMBAf8EBTADAQH/MB0G
# A1UdDgQWBBTLEejK0rQWWAHJNy4zFha5TJoKHzALBgNVHQ8EBAMCAYYwEgYJKwYB
# BAGCNxUBBAUCAwEAATAjBgkrBgEEAYI3FQIEFgQU/dExTtMmipXhmGA7qDFvpjy8
# 2C0wGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwHwYDVR0jBBgwFoAUDqyCYEBW
# J5flJRP8KuEKU5VZ5KQwUAYDVR0fBEkwRzBFoEOgQYY/aHR0cDovL2NybC5taWNy
# b3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvbWljcm9zb2Z0cm9vdGNlcnQuY3Js
# MFQGCCsGAQUFBwEBBEgwRjBEBggrBgEFBQcwAoY4aHR0cDovL3d3dy5taWNyb3Nv
# ZnQuY29tL3BraS9jZXJ0cy9NaWNyb3NvZnRSb290Q2VydC5jcnQwDQYJKoZIhvcN
# AQEFBQADggIBAFk5Pn8mRq/rb0CxMrVq6w4vbqhJ9+tfde1MOy3XQ60L/svpLTGj
# I8x8UJiAIV2sPS9MuqKoVpzjcLu4tPh5tUly9z7qQX/K4QwXaculnCAt+gtQxFbN
# LeNK0rxw56gNogOlVuC4iktX8pVCnPHz7+7jhh80PLhWmvBTI4UqpIIck+KUBx3y
# 4k74jKHK6BOlkU7IG9KPcpUqcW2bGvgc8FPWZ8wi/1wdzaKMvSeyeWNWRKJRzfnp
# o1hW3ZsCRUQvX/TartSCMm78pJUT5Otp56miLL7IKxAOZY6Z2/Wi+hImCWU4lPF6
# H0q70eFW6NB4lhhcyTUWX92THUmOLb6tNEQc7hAVGgBd3TVbIc6YxwnuhQ6MT20O
# E049fClInHLR82zKwexwo1eSV32UjaAbSANa98+jZwp0pTbtLS8XyOZyNxL0b7E8
# Z4L5UrKNMxZlHg6K3RDeZPRvzkbU0xfpecQEtNP7LN8fip6sCvsTJ0Ct5PnhqX9G
# uwdgR2VgQE6wQuxO7bN2edgKNAltHIAxH+IOVN3lofvlRxCtZJj/UBYufL8FIXri
# lUEnacOTj5XJjdibIa4NXJzwoq6GaIMMai27dmsAHZat8hZ79haDJLmIz2qoRzEv
# mtzjcT3XAH5iR9HOiMm4GPoOco3Boz2vAkBq/2mbluIQqBC0N1AI1sM9MIIGBzCC
# A++gAwIBAgIKYRZoNAAAAAAAHDANBgkqhkiG9w0BAQUFADBfMRMwEQYKCZImiZPy
# LGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJbWljcm9zb2Z0MS0wKwYDVQQDEyRN
# aWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMDcwNDAzMTI1
# MzA5WhcNMjEwNDAzMTMwMzA5WjB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
# aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
# cnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwggEi
# MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCfoWyx39tIkip8ay4Z4b3i48WZ
# USNQrc7dGE4kD+7Rp9FMrXQwIBHrB9VUlRVJlBtCkq6YXDAm2gBr6Hu97IkHD/cO
# BJjwicwfyzMkh53y9GccLPx754gd6udOo6HBI1PKjfpFzwnQXq/QsEIEovmmbJNn
# 1yjcRlOwhtDlKEYuJ6yGT1VSDOQDLPtqkJAwbofzWTCd+n7Wl7PoIZd++NIT8wi3
# U21StEWQn0gASkdmEScpZqiX5NMGgUqi+YSnEUcUCYKfhO1VeP4Bmh1QCIUAEDBG
# 7bfeI0a7xC1Un68eeEExd8yb3zuDk6FhArUdDbH895uyAc4iS1T/+QXDwiALAgMB
# AAGjggGrMIIBpzAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBQjNPjZUkZwCu1A
# +3b7syuwwzWzDzALBgNVHQ8EBAMCAYYwEAYJKwYBBAGCNxUBBAMCAQAwgZgGA1Ud
# IwSBkDCBjYAUDqyCYEBWJ5flJRP8KuEKU5VZ5KShY6RhMF8xEzARBgoJkiaJk/Is
# ZAEZFgNjb20xGTAXBgoJkiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1p
# Y3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eYIQea0WoUqgpa1Mc1j0
# BxMuZTBQBgNVHR8ESTBHMEWgQ6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20v
# cGtpL2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5jcmwwVAYIKwYBBQUH
# AQEESDBGMEQGCCsGAQUFBzAChjhodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtp
# L2NlcnRzL01pY3Jvc29mdFJvb3RDZXJ0LmNydDATBgNVHSUEDDAKBggrBgEFBQcD
# CDANBgkqhkiG9w0BAQUFAAOCAgEAEJeKw1wDRDbd6bStd9vOeVFNAbEudHFbbQwT
# q86+e4+4LtQSooxtYrhXAstOIBNQmd16QOJXu69YmhzhHQGGrLt48ovQ7DsB7uK+
# jwoFyI1I4vBTFd1Pq5Lk541q1YDB5pTyBi+FA+mRKiQicPv2/OR4mS4N9wficLwY
# Tp2OawpylbihOZxnLcVRDupiXD8WmIsgP+IHGjL5zDFKdjE9K3ILyOpwPf+FChPf
# wgphjvDXuBfrTot/xTUrXqO/67x9C0J71FNyIe4wyrt4ZVxbARcKFA7S2hSY9Ty5
# ZlizLS/n+YWGzFFW6J1wlGysOUzU9nm/qhh6YinvopspNAZ3GmLJPR5tH4LwC8cs
# u89Ds+X57H2146SodDW4TsVxIxImdgs8UoxxWkZDFLyzs7BNZ8ifQv+AeSGAnhUw
# ZuhCEl4ayJ4iIdBD6Svpu/RIzCzU2DKATCYqSCRfWupW76bemZ3KOm+9gSd0BhHu
# diG/m4LBJ1S2sWo9iaF2YbRuoROmv6pH8BJv/YoybLL+31HIjCPJZr2dHYcSZAI9
# La9Zj7jkIeW1sMpjtHhUBdRBLlCslLCleKuzoJZ1GtmShxN1Ii8yqAhuoFuMJb+g
# 74TKIdbrHk/Jmu5J4PcBZW+JC33Iacjmbuqnl84xKf8OxVtc2E0bodj6L54/LlUW
# a8kTo/0xggSXMIIEkwIBATCBkDB5MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
# aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
# cnBvcmF0aW9uMSMwIQYDVQQDExpNaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQQIT
# MwAAAJ0ejSeuuPPYOAABAAAAnTAJBgUrDgMCGgUAoIGwMBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMGCSqG
# SIb3DQEJBDEWBBTHiFJk5ZIZNUAItXgX4i+xDzJBGjBQBgorBgEEAYI3AgEMMUIw
# QKAigCAARQBuAHQAaQB0AHkAIABGAHIAYQBtAGUAdwBvAHIAa6EagBhodHRwOi8v
# bXNkbi5jb20vZGF0YS9lZiAwDQYJKoZIhvcNAQEBBQAEggEAKsItWK1uKi7JG85C
# wocX1Hg2jZek6Dfd5JQS99eCbgNAsJzw1tfVRDUv7qaPlxeMiVgfWBomO+L/uD/m
# Wh9sAULKnA/XpX5oXglQ6mymiacajAPE6aX6sPB71A8DE9NfDu8/xBnYdo73Przn
# YSD4X+xYPqufj3DzJTVKsN0jIWV2oZBs4xpYSJJAvJRQXF/HgT00Oa+Au21C6Aea
# +uurEOqCMkXxYFIsT+mw3AHhCdbqgjx2lCTlpBiaZNxxgCbeYC36c2AEKttodJnl
# yYvoFT/2wmVZvWi4R9Umel+MOfDJp4VOOXrMsHXA3oXqhYmUvJ4i6RXBO3bMWhpG
# 6heH96GCAigwggIkBgkqhkiG9w0BCQYxggIVMIICEQIBATCBjjB3MQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQg
# VGltZS1TdGFtcCBQQ0ECEzMAAAArOTJIwbLJSPMAAAAAACswCQYFKw4DAhoFAKBd
# MBgGCSqGSIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTEyMTAx
# NzE2MDQwN1owIwYJKoZIhvcNAQkEMRYEFD/kMxe8N3M3A/gIuO0gl9SWuEBjMA0G
# CSqGSIb3DQEBBQUABIIBAKCpbQQA0sdEmf9skotqkzfo5vnKF9/s33lE+zCu8sAp
# FeOwg9OsmFGUxNBMc8ZwSphjjv/YSs7HKPugi7PaAKAWhGhpnAUamvg3VsLwsezf
# 5HMTg6REoBPW4Oz/f7BzsBuFm3bIaycvDIRgnvdGXUCeifu9butuEs1AvY0QSUzP
# 2A7G7n03mUDITFm9dFHSGdyEfyO/XZcVRNhZgrkEv7HGSJKqFQN0IS8T3bWrXmD0
# HV+5RkIw+VA4Y28DMXJfVKJFGMUJCkoiX2vIJoOc74T6TBRJhZ0BkRejQnumVxa+
# zuzBS3csYZeA7Mx/juI0WW7QvElNqp6FjRLhDf75xLY=
# SIG # End signature block
