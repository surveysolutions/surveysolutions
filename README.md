# Dev environment

Prerequesties:

- Install PostgreSQL.
- Install LTS version of node JS.
- Install yarn package manager.

Build is executed in 2 steps:

## Frontend

Can be executed by running either `build.all.deps.bat` or `build_deps.sh` scripts. Front end is built for Designer, Headquarters and WebTester applications.

## Backend

In order to build entier solution you can open src/WB.sln file.

[Release notes](https://github.com/surveysolutions/surveysolutions/wiki/Release-notes)

## Docker

### Build

It should be possible to build all local docker images with `docker-compose build`

### Running

Local env in docker can be run with 

```
docker-compose up
```

Hq will be available at http://hq.lvh.me
Designer at http://designer.lvh.me


