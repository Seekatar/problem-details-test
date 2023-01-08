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
        $result.ToString() | ConvertFrom-Json | Format-List
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

"throw/details/$clientId/$marketId/2", # Info
"throw/details/$clientId/$marketId/3", # Warning
"throw/details/$clientId/$marketId/4", # Error
"throw/details/$clientId/$marketId/6", # None
"problem",
"throw/problem",
"validation-problem",
"throw/validation-problem",
"throw/not-implemented/$clientId/$marketId",
"thisIsntFound" | ForEach-Object {
    Write-Host "------------ $_"
    WriteResult (Invoke-WebRequest "$uri$_" -Headers $headers -SkipHttpErrorCheck -useBasicParsing)
}
