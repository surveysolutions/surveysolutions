Prerequisties
=============

`Nodejs` and `Yarn` required for project to be build

- `NodeJS` https://nodejs.org/en/download/current/
- `Yarn` https://yarnpkg.com/en/docs/install

Setup dev env
=============

For better dev experience `VSCode` recommended - but can be used any modern editor

Tasks
-----

There is several `VS Code` tasks set up to make live easier

* `gulp default` - this is default task to build project in developer mode. Can be executed with `CTRL+Shift+B` hotkey
* `gulp: watch` - will run webpack in watch mode with notification on build (no hot reload for now)
* `gulp: production` - will create production build
* `Rebuild` - will run dev build with `dist` folder cleanup

Build scripts
=============

Main build occur with `Webpack`. This tool helps to split all js/css into several bundles. 

Build orchestration is done with `gulp`.

There is a two step build for main app. One step generate `shared_vendor.dll.js` - one time compiled lib with all shared and essential libs for app. Second step compile app. During develop of application shared vendor lib will be compiled only once.

`resx2json` task - generate localization json files from C# Resources.

There is 2 webpack configuration files one for shared vendor dll lib and one for application entries.