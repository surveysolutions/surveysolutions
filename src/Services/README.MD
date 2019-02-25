# Building

`dotnet build --configuration Release --version-suffix %branchName% /p:VersionPrefix=%buildNo%`

# Versioning

Version prefix is read from `.version` file in folder above. Then suffix added and build number. In special case if --version-suffix equal 'release' 
then suffix will be ommited. Presence of suffix in semver is a mark of prerelease build.

## Examples :

Let's assume that `.version` file contains string `19.03`.

`> dotnet build --configuration Release --version-suffix KP-1233-export-speedup /p:VersionPrefix=678` 

=> Version product: `19.03.0-export-speedup.678`

`> dotnet build --configuration Release --version-suffix release /p:VersionPrefix=555`

=> Version product: `19.03.0.555`





