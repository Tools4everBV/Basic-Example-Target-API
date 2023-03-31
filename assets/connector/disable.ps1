##################################################
# HelloID-Conn-Prov-Target-Example-Target-Disable
#
# Version: 1.0.0
##################################################
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

try {
    # Verify if a user account exists in the target system
    $responseUser = Invoke-RestMethod -Method 'GET' -Uri "$($config.BaseUrl)/api/Users/$aRef"
    if ($responseUser){
        $action = 'Found'
        $dryRunMessage = "Disable Example-Target account for: [$($p.DisplayName)] will be executed during enforcement"
    } elseif($null -eq $responseUser) {
        $action = 'NotFound'
        $dryRunMessage = "Example-Target account for: [$($p.DisplayName)] not found. Possibly already deleted. Skipping action"
    }
    Write-Verbose $dryRunMessage

    # Add an auditMessage showing what will happen during enforcement
    if ($dryRun -eq $true) {
        Write-Warning "[DryRun] $dryRunMessage"
    }

    if (-not($dryRun -eq $true)) {
        Write-Verbose "Disable Example-Target account with accountReference: [$aRef]"

        switch ($action){
            'Found' {
                $patch = @(
                    [ordered]@{
                        OP    = 'replace'
                        PATH  = 'active'
                        VALUE = $false
                    }
                )
                $null = Invoke-RestMethod -Method 'PATCH' -Uri "$($config.BaseUrl)/api/Users/$aRef" -Body ($patch | ConvertTo-Json) -ContentType 'application/json'
                $auditLogs.Add([PSCustomObject]@{
                        Message = 'Disable account was successful'
                        IsError = $false
                    })
                break
            }

            'NotFound' {
                $auditLogs.Add([PSCustomObject]@{
                    Message = "Example-Target account for: [$($p.DisplayName)] not found. Possibly already deleted. Skipping action"
                    IsError = $false
                })
                break
            }
        }

        $success = $true
    }
} catch {
    $success = $false
    # The <ErrorDetails.Message> object might be empty depending on the API. Some API's do net send back an error message. Please refer to the readme for more information.
    $auditMessage = "Could not disable Example-Target account. Error: $($_.Exception.Message). Details: $($_.ErrorDetails)"
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
