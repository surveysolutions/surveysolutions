# require docker
$ENV:COMPOSE_PROJECT_NAME="suso"
$ENV:DOCKER_BUILDKIT=1

dotnet tool install --global GitVersion.Tool
dotnet gitversion /updateprojectfiles
dotnet gitversion /showvariable AssemblySemFileVer > .version

docker build -f Dockerfile.js -o build/jsFiles .
docker-compose build --parallel
