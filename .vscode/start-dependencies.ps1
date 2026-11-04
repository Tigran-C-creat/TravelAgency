$ErrorActionPreference = "Stop"
$projectName = "travelagency-vscode"

Write-Host "Starting PostgreSQL and Redis in the separate '$projectName' Compose project."
& docker compose --project-name $projectName up -d db redis
if ($LASTEXITCODE -ne 0) {
    throw "Could not start PostgreSQL and Redis. Make sure Docker Desktop is running."
}

$services = @("db", "redis")
$containerIds = @{}
foreach ($service in $services) {
    $containerId = & docker compose --project-name $projectName ps -q $service
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($containerId)) {
        throw "Could not find the '$service' container after starting Compose."
    }
    $containerIds[$service] = $containerId.Trim()
}

$ready = $false
for ($attempt = 0; $attempt -lt 90; $attempt++) {
    $ready = $true
    foreach ($service in $services) {
        $health = & docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' $containerIds[$service]
        if ($LASTEXITCODE -ne 0) {
            throw "Could not inspect the '$service' container."
        }

        $status = $health.Trim()
        if ($status -eq "unhealthy" -or $status -eq "exited") {
            & docker compose --project-name $projectName logs --tail 50 $service
            throw "The '$service' container is '$status'. See the container logs above."
        }
        if ($status -ne "healthy") {
            $ready = $false
        }
    }

    if ($ready) { break }
    Start-Sleep -Seconds 2
}

if (-not $ready) {
    & docker compose --project-name $projectName logs --tail 50 db redis
    throw "PostgreSQL and Redis did not become healthy within 180 seconds. Check the container logs."
}

Write-Host "PostgreSQL and Redis are ready. TravelAgency will run locally in VS Code."
