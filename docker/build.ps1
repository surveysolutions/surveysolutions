$PG_PASSWORD = "Qwerty1234"
$PG_VERSION = "11.8"
$DOTNET_VERSION="3.1"
$NODEJS_VERSION = "12"

$NETWORK = "testing"

$ScriptPath = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
$root = Split-Path $ScriptPath -parent


function Get-Version() {
    $path = Join-Path $root "src/.version"
    if (Test-Path $path) {
        return (Get-Content $path)
    }
    else {
        Write-Error "You have to start from docker folder"
    }
}

function Cleanup() {
    docker stop db
    docker network rm $NETWORK
}

try {
    Cleanup
}
catch {}

$VERSION = Get-Version

docker network create $NETWORK
docker run --name db --rm --network $NETWORK -e POSTGRES_PASSWORD=$PG_PASSWORD -d postgres:$PG_VERSION

try {
    $dockerfile = Join-Path $root "docker/export/dockerfile"
    docker build -f $dockerfile --force-rm `
        --tag export:$VERSION `
        --network $NETWORK `
        --build-arg DOTNET_VERSION=$DOTNET_VERSION $root

    $dockerfile = Join-Path $root "docker/headquarters/dockerfile"
    docker build -f $dockerfile --force-rm `
        --tag headquarters:$VERSION `
        --network $NETWORK `
        --build-arg DOTNET_VERSION=$DOTNET_VERSION `
        --build-arg NODEJS_VERSION=$NODEJS_VERSION $root
}
finally {
    Cleanup
}