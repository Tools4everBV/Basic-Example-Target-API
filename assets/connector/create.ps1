#################################################
# HelloID-Conn-Prov-Target-Example-Target-Create
#
# Version: 1.0.0
#################################################

# Initialize default values
$config = $configuration | ConvertFrom-Json
$p = $person | ConvertFrom-Json
$success = $false
$auditLogs = [System.Collections.Generic.List[PSCustomObject]]::new()

# Account mapping
$account = [PSCustomObject]@{
    employeeId = $p.ExternalId
    firstName  = $p.Name.GivenName
    lastName   = $p.Name.FamilyName
    email      = $p.Contact.Business.Email
    active     = $false
}

# Enable TLS1.2
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12

# Set debug logging
switch ($($config.IsDebug)) {
    $true { $VerbosePreference = 'Continue' }
    $false { $VerbosePreference = 'SilentlyContinue' }
}

try {
    # Verify if a user must be either [created and correlated], [updated and correlated] or just [correlated]
    try {
        $responseUser = Invoke-RestMethod -Method 'GET' -Uri "$($config.BaseUrl)/api/Users/ByEmployeeId/$($account.EmployeeId)"
    } catch {
        Write-Verbose "Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
        if ($_.Exception.Response.StatusCode -eq 'NotFound') {
            $responseUser = $null
        } else {
            throw
        }
    }

    if ($null -eq $responseUser){
        $action = 'Create-Correlate'
    } elseif ($updatePerson -eq $true) {
        $action = 'Update-Correlate'
    } else {
        $action = 'Correlate'
    }

    # Add a informational message showing what will happen during enforcement
    if ($dryRun -eq $true) {
        Write-Information "[DryRun] $action Example-Target account for: [$($p.DisplayName)], will be executed during enforcement"
    }

    if (-not($dryRun -eq $true)) {
        switch ($action) {
            'Create-Correlate' {
                Write-Verbose 'Creating and correlating Example-Target account'
                $createdUser = Invoke-RestMethod -Method 'POST' -Uri "$($config.BaseUrl)/api/Users" -Body ($account | ConvertTo-Json) -ContentType 'application/json'
                $accountReference = $createdUser.Id
                break
            }

            'Correlate' {
                Write-Verbose 'Correlating Example-Target account'
                $accountReference = $responseUser.Id
                break
            }
        }

        $success = $true
        $auditLogs.Add([PSCustomObject]@{
                Message = "$action account was successful. AccountReference is: [$accountReference]"
                IsError = $false
            })
    }
} catch {
    $success = $false
    $auditMessage = "Could not $action Example-Target account. Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    Write-Verbose "Error at Line '$($_.InvocationInfo.ScriptLineNumber)': $($_.InvocationInfo.Line). Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    $auditLogs.Add([PSCustomObject]@{
            Message = $auditMessage
            IsError = $true
        })
} finally {
    $result = [PSCustomObject]@{
        Success          = $success
        AccountReference = $accountReference
        Auditlogs        = $auditLogs
        Account          = $account
    }
    Write-Output $result | ConvertTo-Json -Depth 10
}
