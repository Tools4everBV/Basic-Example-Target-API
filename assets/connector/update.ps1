#################################################
# HelloID-Conn-Prov-Target-Example-Target-Update
#
# Version: 1.0.0
#################################################
# Initialize default values
$config = $configuration | ConvertFrom-Json
$p = $person | ConvertFrom-Json
$aRef = $AccountReference | ConvertFrom-Json
$success = $false
$auditLogs = [System.Collections.Generic.List[PSCustomObject]]::new()

# Account mapping
$account = [PSCustomObject]@{
    employeeId = $p.ExternalId
    firstName  = $p.Name.GivenName
    lastName   = $p.Name.FamilyName
    email      = $p.Contact.Business.Email
}

# Enable TLS1.2
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12

# Set debug logging
switch ($($config.IsDebug)) {
    $true { $VerbosePreference = 'Continue' }
    $false { $VerbosePreference = 'SilentlyContinue' }
}

try {
    # Verify if the account must be updated
    # Always compare the HelloID account against the account in target system
    $responseUser = Invoke-RestMethod -Method 'GET' -Uri "$($config.BaseUrl)/api/Users/$aRef"
    $patchOperations = [System.Collections.Generic.List[object]]::new()
    $propertiesChanged = @()

    foreach ($property in $account.PSObject.properties) {
        if ($property.Value -ne $responseUser.$($property.Name)) {
            $propertiesChanged += $($property.Name)
            $operation = [ordered]@{
                OP    = 'replace'
                PATH  = $($property.Name)
                VALUE = $property.Value
            }
            $patchOperations.Add($operation)
        }
    }

    if ($propertiesChanged -and ($null -ne $responseUser)) {
        $action = 'Update'
        $dryRunMessage = "Account property(s) required to update: [$($propertiesChanged -join ',')]"
    } elseif (-not($propertiesChanged)) {
        $action = 'NoChanges'
        $dryRunMessage = 'No changes will be made to the account during enforcement'
    } elseif ($null -eq $responseUser) {
        $action = 'NotFound'
        $dryRunMessage = "Example-Target account for: [$($p.DisplayName)] not found. Possibly deleted"
    }
    Write-Verbose $dryRunMessage

    # Add a informational message showing what will happen during enforcement
    if ($dryRun -eq $true) {
        Write-Information "[DryRun] $dryRunMessage"
    }

    if (-not($dryRun -eq $true)) {
        switch ($action) {
            'Update' {
                Write-Verbose "Updating Example-Target account with accountReference: [$aRef]"
                $null = Invoke-RestMethod -Method 'PATCH' -Uri "$($config.BaseUrl)/api/Users/$aRef" -Body (, $patchOperations | ConvertTo-Json) -ContentType 'application/json'
                $success = $true
                $auditLogs.Add([PSCustomObject]@{
                    Message = 'Update account was successful'
                    IsError = $false
                })
                break
            }

            'NoChanges' {
                Write-Verbose "No changes to Example-Target account with accountReference: [$aRef]"

                $success = $true
                $auditLogs.Add([PSCustomObject]@{
                    Message = 'No changes will be made to the account during enforcement'
                    IsError = $false
                })
                break
            }

            'NotFound' {
                Write-Verbose "Example-Target account for: [$($p.DisplayName)] not found. Possibly deleted"

                $success = $false
                $auditLogs.Add([PSCustomObject]@{
                    Message = "Example-Target account for: [$($p.DisplayName)] not found. Possibly deleted"
                    IsError = $true
                })
                break
            }
        }
    }
} catch {
    $success = $false
    # The <ErrorDetails.Message> object might be empty depending on the API. Some API's do net send back an error message. Please refer to the readme for more information.
    $auditMessage = "Could not update Example-Target account. Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    Write-Verbose "Error at Line '$($_.InvocationInfo.ScriptLineNumber)': $($_.InvocationInfo.Line). Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    $auditLogs.Add([PSCustomObject]@{
            Message = $auditMessage
            IsError = $true
        })
} finally {
    $result = [PSCustomObject]@{
        Success   = $success
        Account   = $account
        Auditlogs = $auditLogs
    }
    Write-Output $result | ConvertTo-Json -Depth 10
}
