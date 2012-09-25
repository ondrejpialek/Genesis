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
# MIIabAYJKoZIhvcNAQcCoIIaXTCCGlkCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUxSlWxHe0+ZgwfZR9Dzu4QZn3
# 0WygghU/MIIEqTCCA5GgAwIBAgITMwAAAIhZDjxRH+JqZwABAAAAiDANBgkqhkiG
# 9w0BAQUFADB5MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
# A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSMw
# IQYDVQQDExpNaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQTAeFw0xMjA3MjYyMDUw
# NDFaFw0xMzEwMjYyMDUwNDFaMIGDMQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
# aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
# cnBvcmF0aW9uMQ0wCwYDVQQLEwRNT1BSMR4wHAYDVQQDExVNaWNyb3NvZnQgQ29y
# cG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCzdHTQgjyH
# p5rUjrIEQoCXJS7kQc6TYzZfE/K0eJiAxih+zIoT7z03jDsJoNgUxVxe2KkdfwHB
# s5gbUHfs/up8Rc9/4SEOxYTKnw9rswk4t3TEVx6+8EioeVrfDpscmqi8yFK1DGmP
# hM5xVXv/CSC/QHc3ITB0W5Xfd8ug5cFyEgY98shVbK/B+2oWJ8j1s2Hj2c4bDx70
# 5M1MNGw+RxHnAitfFHoEB/XXPYvbZ31XPjXrbY0BQI0ah5biD3dMibo4nPuOApHb
# Ig/l0DapuDdF0Cr8lo3BYHEzpYix9sIEMIdbw9cvsnkR2ItlYqKKEWZdfn8FenOK
# H3qF5c0oENE9AgMBAAGjggEdMIIBGTATBgNVHSUEDDAKBggrBgEFBQcDAzAdBgNV
# HQ4EFgQUJls+W12WX+L3d4h/XkVTWKguW7gwDgYDVR0PAQH/BAQDAgeAMB8GA1Ud
# IwQYMBaAFMsR6MrStBZYAck3LjMWFrlMmgofMFYGA1UdHwRPME0wS6BJoEeGRWh0
# dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY0NvZFNp
# Z1BDQV8wOC0zMS0yMDEwLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKG
# Pmh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljQ29kU2lnUENB
# XzA4LTMxLTIwMTAuY3J0MA0GCSqGSIb3DQEBBQUAA4IBAQAP3kBJiJHRMTejRDhp
# smor1JH7aIWuWLseDI9W+pnXypcnTOiFjnlpLOS9lj/lcGaXlTBlKa3Gyqz1D3mo
# Z79p9A+X4woPv+6WdimyItAzxv+LSa2usv2/JervJ1DA6xn4GmRqoOEXWa/xz+yB
# qInosdIUBuNqbXRSZNqWlCpcaWsf7QWZGtzoZaqIGxWVGtOkUZb9VZX4Y42fFAyx
# nn9KBP/DZq0Kr66k3mP68OrDs7Lrh9vFOK22c9J4ZOrsIVtrO9ZEIvSBUqUrQymL
# DKEqcYJCy6sbftSlp6333vdGms5DOegqU+3PQOR3iEK/RxbgpTZq76cajTo9MwT2
# JSAjMIIEwzCCA6ugAwIBAgITMwAAACs5MkjBsslI8wAAAAAAKzANBgkqhkiG9w0B
# AQUFADB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
# BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYD
# VQQDExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EwHhcNMTIwOTA0MjExMjM0WhcN
# MTMxMjA0MjExMjM0WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
# b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
# dGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkMw
# RjQtMzA4Ni1ERUY4MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2
# aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAprYwDgNlrlBahmuF
# n0ihHsnA7l5JB4XgcJZ8vrlfYl8GJtOLObsYIqUukq3YS4g6Gq+bg67IXjmMwjJ7
# FnjtNzg68WL7aIICaOzru0CKsf6hLDZiYHA5YGIO+8YYOG+wktZADYCmDXiLNmuG
# iiYXGP+w6026uykT5lxIjnBGNib+NDWrNOH32thc6pl9MbdNH1frfNaVDWYMHg4y
# Fz4s1YChzuv3mJEC3MFf/TiA+Dl/XWTKN1w7UVtdhV/OHhz7NL5f5ShVcFScuOx8
# AFVGWyiYKFZM4fG6CRmWgUgqMMj3MyBs52nDs9TDTs8wHjfUmFLUqSNFsq5cQUlP
# tGJokwIDAQABo4IBCTCCAQUwHQYDVR0OBBYEFKUYM1M/lWChQxbvjsav0iu6nljQ
# MB8GA1UdIwQYMBaAFCM0+NlSRnAK7UD7dvuzK7DDNbMPMFQGA1UdHwRNMEswSaBH
# oEWGQ2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01p
# Y3Jvc29mdFRpbWVTdGFtcFBDQS5jcmwwWAYIKwYBBQUHAQEETDBKMEgGCCsGAQUF
# BzAChjxodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY3Jvc29m
# dFRpbWVTdGFtcFBDQS5jcnQwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcN
# AQEFBQADggEBAH7MsHvlL77nVrXPc9uqUtEWOca0zfrX/h5ltedI85tGiAVmaiaG
# Xv6HWNzGY444gPQIRnwrc7EOv0Gqy8eqlKQ38GQ54cXV+c4HzqvkJfBprtRG4v5m
# MjzXl8UyIfruGiWgXgxCLBEzOoKD/e0ds77OkaSRJXG5q3Kwnq/kzwBiiXCpuEpQ
# jO4vImSlqOZNa5UsHHnsp6Mx2pBgkKRu/pMCDT8sJA3GaiaBUYNKELt1Y0SqaQjG
# A+vizwvtVjrs73KnCgz0ANMiuK8icrPnxJwLKKCAyuPh1zlmMOdGFxjn+oL6WQt6
# vKgN/hz/A4tjsk0SAiNPLbOFhDvioUfozxUwggW8MIIDpKADAgECAgphMyYaAAAA
# AAAxMA0GCSqGSIb3DQEBBQUAMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAXBgoJ
# kiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1pY3Jvc29mdCBSb290IENl
# cnRpZmljYXRlIEF1dGhvcml0eTAeFw0xMDA4MzEyMjE5MzJaFw0yMDA4MzEyMjI5
# MzJaMHkxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
# EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xIzAhBgNV
# BAMTGk1pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBMIIBIjANBgkqhkiG9w0BAQEF
# AAOCAQ8AMIIBCgKCAQEAsnJZXBkwZL8dmmAgIEKZdlNsPhvWb8zL8epr/pcWEODf
# OnSDGrcvoDLs/97CQk4j1XIA2zVXConKriBJ9PBorE1LjaW9eUtxm0cH2v0l3511
# iM+qc0R/14Hb873yNqTJXEXcr6094CholxqnpXJzVvEXlOT9NZRyoNZ2Xx53RYOF
# OBbQc1sFumdSjaWyaS/aGQv+knQp4nYvVN0UMFn40o1i/cvJX0YxULknE+RAMM9y
# KRAoIsc3Tj2gMj2QzaE4BoVcTlaCKCoFMrdL109j59ItYvFFPeesCAD2RqGe0VuM
# JlPoeqpK8kbPNzw4nrR3XKUXno3LEY9WPMGsCV8D0wIDAQABo4IBXjCCAVowDwYD
# VR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUyxHoytK0FlgByTcuMxYWuUyaCh8wCwYD
# VR0PBAQDAgGGMBIGCSsGAQQBgjcVAQQFAgMBAAEwIwYJKwYBBAGCNxUCBBYEFP3R
# MU7TJoqV4ZhgO6gxb6Y8vNgtMBkGCSsGAQQBgjcUAgQMHgoAUwB1AGIAQwBBMB8G
# A1UdIwQYMBaAFA6sgmBAVieX5SUT/CrhClOVWeSkMFAGA1UdHwRJMEcwRaBDoEGG
# P2h0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL21pY3Jv
# c29mdHJvb3RjZXJ0LmNybDBUBggrBgEFBQcBAQRIMEYwRAYIKwYBBQUHMAKGOGh0
# dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljcm9zb2Z0Um9vdENl
# cnQuY3J0MA0GCSqGSIb3DQEBBQUAA4ICAQBZOT5/Jkav629AsTK1ausOL26oSffr
# X3XtTDst10OtC/7L6S0xoyPMfFCYgCFdrD0vTLqiqFac43C7uLT4ebVJcvc+6kF/
# yuEMF2nLpZwgLfoLUMRWzS3jStK8cOeoDaIDpVbguIpLV/KVQpzx8+/u44YfNDy4
# VprwUyOFKqSCHJPilAcd8uJO+IyhyugTpZFOyBvSj3KVKnFtmxr4HPBT1mfMIv9c
# Hc2ijL0nsnljVkSiUc356aNYVt2bAkVEL1/02q7UgjJu/KSVE+Traeepoiy+yCsQ
# DmWOmdv1ovoSJgllOJTxeh9Ku9HhVujQeJYYXMk1Fl/dkx1Jji2+rTREHO4QFRoA
# Xd01WyHOmMcJ7oUOjE9tDhNOPXwpSJxy0fNsysHscKNXkld9lI2gG0gDWvfPo2cK
# dKU27S0vF8jmcjcS9G+xPGeC+VKyjTMWZR4Oit0Q3mT0b85G1NMX6XnEBLTT+yzf
# H4qerAr7EydAreT54al/RrsHYEdlYEBOsELsTu2zdnnYCjQJbRyAMR/iDlTd5aH7
# 5UcQrWSY/1AWLny/BSF64pVBJ2nDk4+VyY3YmyGuDVyc8KKuhmiDDGotu3ZrAB2W
# rfIWe/YWgyS5iM9qqEcxL5rc43E91wB+YkfRzojJuBj6DnKNwaM9rwJAav9pm5bi
# EKgQtDdQCNbDPTCCBgcwggPvoAMCAQICCmEWaDQAAAAAABwwDQYJKoZIhvcNAQEF
# BQAwXzETMBEGCgmSJomT8ixkARkWA2NvbTEZMBcGCgmSJomT8ixkARkWCW1pY3Jv
# c29mdDEtMCsGA1UEAxMkTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9y
# aXR5MB4XDTA3MDQwMzEyNTMwOVoXDTIxMDQwMzEzMDMwOVowdzELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWljcm9zb2Z0IFRp
# bWUtU3RhbXAgUENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn6Fs
# sd/bSJIqfGsuGeG94uPFmVEjUK3O3RhOJA/u0afRTK10MCAR6wfVVJUVSZQbQpKu
# mFwwJtoAa+h7veyJBw/3DgSY8InMH8szJIed8vRnHCz8e+eIHernTqOhwSNTyo36
# Rc8J0F6v0LBCBKL5pmyTZ9co3EZTsIbQ5ShGLieshk9VUgzkAyz7apCQMG6H81kw
# nfp+1pez6CGXfvjSE/MIt1NtUrRFkJ9IAEpHZhEnKWaol+TTBoFKovmEpxFHFAmC
# n4TtVXj+AZodUAiFABAwRu233iNGu8QtVJ+vHnhBMXfMm987g5OhYQK1HQ2x/Peb
# sgHOIktU//kFw8IgCwIDAQABo4IBqzCCAacwDwYDVR0TAQH/BAUwAwEB/zAdBgNV
# HQ4EFgQUIzT42VJGcArtQPt2+7MrsMM1sw8wCwYDVR0PBAQDAgGGMBAGCSsGAQQB
# gjcVAQQDAgEAMIGYBgNVHSMEgZAwgY2AFA6sgmBAVieX5SUT/CrhClOVWeSkoWOk
# YTBfMRMwEQYKCZImiZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJbWljcm9z
# b2Z0MS0wKwYDVQQDEyRNaWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3Jp
# dHmCEHmtFqFKoKWtTHNY9AcTLmUwUAYDVR0fBEkwRzBFoEOgQYY/aHR0cDovL2Ny
# bC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvbWljcm9zb2Z0cm9vdGNl
# cnQuY3JsMFQGCCsGAQUFBwEBBEgwRjBEBggrBgEFBQcwAoY4aHR0cDovL3d3dy5t
# aWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNyb3NvZnRSb290Q2VydC5jcnQwEwYD
# VR0lBAwwCgYIKwYBBQUHAwgwDQYJKoZIhvcNAQEFBQADggIBABCXisNcA0Q23em0
# rXfbznlRTQGxLnRxW20ME6vOvnuPuC7UEqKMbWK4VwLLTiATUJndekDiV7uvWJoc
# 4R0Bhqy7ePKL0Ow7Ae7ivo8KBciNSOLwUxXdT6uS5OeNatWAweaU8gYvhQPpkSok
# InD79vzkeJkuDfcH4nC8GE6djmsKcpW4oTmcZy3FUQ7qYlw/FpiLID/iBxoy+cwx
# SnYxPStyC8jqcD3/hQoT38IKYY7w17gX606Lf8U1K16jv+u8fQtCe9RTciHuMMq7
# eGVcWwEXChQO0toUmPU8uWZYsy0v5/mFhsxRVuidcJRsrDlM1PZ5v6oYemIp76Kb
# KTQGdxpiyT0ebR+C8AvHLLvPQ7Pl+ex9teOkqHQ1uE7FcSMSJnYLPFKMcVpGQxS8
# s7OwTWfIn0L/gHkhgJ4VMGboQhJeGsieIiHQQ+kr6bv0SMws1NgygEwmKkgkX1rq
# Vu+m3pmdyjpvvYEndAYR7nYhv5uCwSdUtrFqPYmhdmG0bqETpr+qR/ASb/2KMmyy
# /t9RyIwjyWa9nR2HEmQCPS2vWY+45CHltbDKY7R4VAXUQS5QrJSwpXirs6CWdRrZ
# kocTdSIvMqgIbqBbjCW/oO+EyiHW6x5PyZruSeD3AWVviQt9yGnI5m7qp5fOMSn/
# DsVbXNhNG6HY+i+ePy5VFmvJE6P9MYIElzCCBJMCAQEwgZAweTELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEjMCEGA1UEAxMaTWljcm9zb2Z0IENv
# ZGUgU2lnbmluZyBQQ0ECEzMAAACIWQ48UR/iamcAAQAAAIgwCQYFKw4DAhoFAKCB
# sDAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYK
# KwYBBAGCNwIBFTAjBgkqhkiG9w0BCQQxFgQUx4hSZOWSGTVACLV4F+IvsQ8yQRow
# UAYKKwYBBAGCNwIBDDFCMECgIoAgAEUAbgB0AGkAdAB5ACAARgByAGEAbQBlAHcA
# bwByAGuhGoAYaHR0cDovL21zZG4uY29tL2RhdGEvZWYgMA0GCSqGSIb3DQEBAQUA
# BIIBAJWzkle4CtQI5g4iqh8a4kxtITPptkKDndnik8PsxuZtwROU6WUyy+CXrpUU
# U6T36fpSf/t1aSgGmv3E1Lkp4qjnchUHdLBwySlGyNPKmsJhNpdREVmDl6ylcv/i
# jGOH0yIc2yQQyV8rQK6rlvDfIncznO0xQIehBF4THR7Ofsl3EmaKvnad+swtF+rZ
# QWoTz9UARxQBV0nwH+RHRgVFfhEIG9S+ZWEGQBqXA9sUmiPyi9LThfiU+n8ooceX
# vDKu1ckn6gplkp0PR3Uxcncnd3noRvxv7/FpnYrZ7B+CjITyPO5ol3RQtPorhL/0
# Bw93xrh9mWWHF7AR3tM4fEY3OdKhggIoMIICJAYJKoZIhvcNAQkGMYICFTCCAhEC
# AQEwgY4wdzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
# BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8G
# A1UEAxMYTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBAhMzAAAAKzkySMGyyUjzAAAA
# AAArMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqG
# SIb3DQEJBTEPFw0xMjA5MjQxNjAyMDVaMCMGCSqGSIb3DQEJBDEWBBTCJN00jLSI
# cgh5CR59bBnZaZQsHzANBgkqhkiG9w0BAQUFAASCAQBYVOpNkGsYqXUEJ36kb6OW
# zPIGjyjF39m3JWTeg0zw/er2pHQeDcSw8PTtXGQhRghBLvxj5dnT7RpT9UWheKm9
# eJ6iLL+8NzpI5LGtxrFDNmA7Di70/TFaw5s3jqBxD+P1jc6q6MJf3gZH8zdXTFwI
# eQr+4T6MHSshUIuY9jKMHfnx/ab4c7PjuzcgDAK0Vv4D+iHSyIMq8ZZjY6sz6Hme
# bGeso33l0EsGMQxjotXMnhknXiQTZhjZccahye6OvkqYnZGRUOC1usoRCRaDmJzr
# fNrykQrnyFxofZrZ4gg1zydGXWayx9d2laKcXPqWtRpQsVETtWaq94U8jsDfBFA5
# SIG # End signature block
