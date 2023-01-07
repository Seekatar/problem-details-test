[CmdletBinding()]
param (
    [switch] $Correlation,
    [switch] $ShowHeaders,
    [switch] $Json
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function WriteResult {
    [CmdletBinding()]
    param($result)

    Set-StrictMode -Version Latest

    "Status is $($result.StatusCode)"
    if ($ShowHeaders) {
        Write-Host $result.Headers
    }
    if (!$Json) {
        Write-Host ($result.ToString() | ConvertFrom-Json | out-string)
    } else {
        Write-Host $result.ToString()
    }


}

$headers = @{
    "Accept" = "application/json"
}
if ($Correlation) {
    $headers = @{
        "Content-Type"     = "application/json"
        "X-Correlation-Id" = [Guid]::NewGuid().ToString()
        "Accept"           = "application/json"
    }
}
$uri = "http://localhost:5138/api/"

$clientId = [Guid]::NewGuid()
$marketId = 1
Write-Host "------------ Throw Log Error"
WriteResult (Invoke-WebRequest "${uri}throw/details/$clientId/$marketId/4" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)

# Write-Host "------------ Throw Log Info"
# WriteResult (Invoke-WebRequest "${uri}throw/details/$clientId/$marketId/2" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)

# Write-Host "------------ Throw Log None"
# WriteResult (Invoke-WebRequest "${uri}throw/details/$clientId/$marketId/6" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)

# Write-Host "------------ Throw Log"
# WriteResult Invoke-WebRequest "${uri}throw/details-log/$clientId/$marketId" -Headers $headers -SkipHttpErrorCheck -useBasicParsing

# Write-Host "------------ Throw Invoke-Logged"
# WriteResult Invoke-WebRequest "${uri}throw/details-scope/$clientId/$marketId" -Headers $headers -SkipHttpErrorCheck -useBasicParsing

Write-Host "------------ NotImpl"
WriteResult (Invoke-WebRequest "${uri}throw/not-implemented/$clientId/$marketId" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)

# Write-Host "------------ NotFound"
# WriteResult (Invoke-WebRequest "${uri}thisIsntFound" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)
