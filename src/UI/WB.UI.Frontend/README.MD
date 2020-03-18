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
* `gulp: watch` - will run webpack in watch mode with notification on build (no hot reload for now). Much faster then build
* `gulp --production` - will create production build
* `Rebuild` - will run dev build with `dist` folder cleanup

Build scripts
=============

Main build occur with `Webpack`. This tool helps to split all js/css into several bundles. 

Build orchestration is done with `gulp`.

On each build gulp ensure that there is: 
 - converted resx files to json
 - tests run
 - shared vendor libs created
 - build run

There is a two step build for main app. One step generate `shared_vendor.dll.js` - one time compiled lib with all shared and essential libs for app. 

Second step compile app. During develop of application shared vendor lib will be compiled only once.

`resx2json` task - generate localization json files from C# Resources.


There is 2 webpack configuration files in `.build` directory one for shared vendor dll lib and one for application entries.

Store Modules
=============

Headquarters app store use modules.

Code introduce term - `RootStore`, root store that contain all other application parts as modules.

rootStore {
    state: {}, actions: {}, mutations: {},
    modules: {
        route: { this module added by `vuex-route-sync` package} - all other modules can depend on it
        webinterview: { all web interview related store actions,state, mutations},
        review: { interview review tool related actions, etc... thats where all work will be done for interview detalica } 
    }
}

Please read more about vuex stores in documentation - https://vuex.vuejs.org/en/modules.html

HQ View Components
===============

HQ is split on parts into `View Components` - essentially those are group of views that have own routing and own one (or more) store modules.

Current( on the time this guide written) there is following structure:

- `http://localhost/headquarters` - entry point for application, or `window.CONFIG.basePath`
    - `/Assignments` - assignments component - page on hq admin
    - `/Reports` - HQ Admin reports pages, with sub routes, and own store module:
        - `/Reports/InterviewersAndDevices`
        - `/Reports/StatusDuration`
    - `/InterviewerHq` - Interview on HQ Component with sub routes and own store for creating and opening interviews. 
        - `/InterviewerHq/CreateNew`
        - `/InterviewerHq/Completed`
        - `/InterviewerHq/Rejected`
        - `/InterviewerHq/Started`
    - `/Interview/Review` - entry point for interview details component. Provide own vuex store module, as well as `webinterview` store

All view components are processed and merged with `ComponentsProvider` class - it's responsible for `vue-router` intialization and `vuex` store modules initialization

Each view component can optionally provide array of `route`s to register, method `initialize` for one time initialization and `modules` hash object to register them in RootStore

Each view component initialized only once on first navigation.


