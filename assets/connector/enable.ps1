#################################################
# HelloID-Conn-Prov-Target-Example-Target-Enable
#
# Version: 1.0.0
#################################################
# Initialize default values
$config = $configuration | ConvertFrom-Json
$p = $person | ConvertFrom-Json
$aRef = $AccountReference | ConvertFrom-Json
$success = $false
$auditLogs = [System.Collections.Generic.List[PSCustomObject]]::new()

# Enable TLS1.2
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12

# Set debug logging
switch ($($config.IsDebug)) {
    $true { $VerbosePreference = 'Continue' }
    $false { $VerbosePreference = 'SilentlyContinue' }
}

#region functions
#endregion

try {
    # Verify if a user account exists in the target system
    # Make sure to fail the action if the account does not exist in the target system!
    $responseUser = Invoke-RestMethod -Method 'GET' -Uri "$($config.BaseUrl)/api/Users/$aRef"
    if ($responseUser) {
        $action = 'Found'
        $dryRunMessage = "[DryRun] Enable Example-Target account for: [$($p.DisplayName)] will be executed during enforcement"
    }

    # Add a informational message showing what will happen during enforcement
    if ($dryRun -eq $true) {
        Write-Information $dryRunMessage
    }

    if (-not($dryRun -eq $true)) {
        Write-Verbose "Enabling Example-Target account with accountReference: [$aRef]"

        switch ($action){
            'Found'{
                $patch = @(
                    [ordered]@{
                        OP    = 'replace'
                        PATH  = 'active'
                        VALUE = $true
                    }
                )
                $null = Invoke-RestMethod -Method 'PATCH' -Uri "$($config.BaseUrl)/api/Users/$aRef" -Body ($patch | ConvertTo-Json) -ContentType 'application/json'
                $success = $true
                $auditLogs.Add([PSCustomObject]@{
                        Message = 'Enable account was successful'
                        IsError = $false
                    })
                break
            }
        }
    }
} catch {
    $success = $false
    # The <ErrorDetails.Message> object might be empty depending on the API. Some API's do net send back an error message. Please refer to the readme for more information.
    $auditMessage = "Could not enable Example-Target account. Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    Write-Verbose "Error at Line '$($_.InvocationInfo.ScriptLineNumber)': $($_.InvocationInfo.Line). Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
    $auditLogs.Add([PSCustomObject]@{
            Message = $auditMessage
            IsError = $true
        })
} finally {
    $result = [PSCustomObject]@{
        Success   = $success
        Auditlogs = $auditLogs
    }
    Write-Output $result | ConvertTo-Json -Depth 10
}
