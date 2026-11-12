# Prerequisties


`Nodejs` and `Yarn` required for project to be build

- `NodeJS` https://nodejs.org/en/download/current/
- `Yarn` https://yarnpkg.com/en/docs/install

# Setup development environment


For better developer experience we recommend `VSCode`, but any modern editor can be used

## Tasks


There are several `VS Code` tasks set up to simplify the work:

* `gulp default` - this is default task to build project in developer mode. Can be executed with `CTRL+Shift+B` hotkey
* `gulp: watch` - will run webpack in watch mode with notification on build (no hot reload for now). Much faster then build
* `gulp --production` - will create production build
* `Rebuild` - will run dev build with `dist` folder cleanup

# Build scripts


`Webpack` is used to split all js/css into several bundles. 

Build orchestration is done with `gulp`.

On each build `gulp` ensures that: 
 - resx files are converted to json
 - tests are run
 - shared vendor libs are created
 - build is run

The build is in two steps: first, `shared_vendor.dll.js` is generated - the library containing all the shared and essential libs. 
And then, the main app is compiled. During development, `shared_vendor.dll.js` will be compiled only once.

`resx2json` task generates localization json files from C# Resources.


There are 2 webpack configuration files in the `.build` directory, one for `shared_vendor.dll.js` and another for application entries.

# Web interview RTL

Web interview pages and WebTester use the server-resolved UI culture for their
HTML `lang` and `dir`. The existing request-localization providers select that
culture (including the browser's `Accept-Language`, unless a culture query
parameter or cookie takes precedence). Arabic selects RTL; changing the
questionnaire translation does not change direction in this first version.
Designer and questionnaire documents have no RTL setting.

Vite uses `tools/postcss-webinterview-rtl.js` in `WB.UI.Frontend` to generate both
directions for `markup.scss` and `markup-web-interview.scss`. RTL rules only apply
to `html.web-interview-page[dir="rtl"]`; other HQ pages retain LTR. Zero-specificity
prefixes preserve the existing CSS cascade. Vendor CSS is not transformed:
AG Grid uses its native RTL option, while exceptions and toast adjustments live in
`_web-interview-rtl.scss`. Use the `sprite-position` Sass mixin for physical
coordinates within sprite sheets. Other background positions (including roster
error and warning markers) mirror with their tooltip targets. Saved numeric
roster answers use `<bdi dir="ltr">` to preserve sign and digit order.
The date picker retains LTR day order and native arrow-key navigation, with
localized month/day labels. Numeric inputs, URLs and coordinates also retain LTR.
Scoped Vue styles are not processed by the RTL generator. Completion-page labels,
error stripes and reconnect-banner spacing use logical CSS properties so they
follow the page direction without separate RTL overrides.

Run `npm run test:rtl` and `npm run build` from `src/UI/WB.UI.Frontend`.
Check Arabic and English on desktop and mobile, including start/resume/finish,
sidebar navigation, all question types, table/matrix rosters, dialogs and dates.
Numeric wire formats, calendar system and questionnaire translations are unchanged.

# Store Modules


Headquarters app store uses modules.

Code introduces term `RootStore`, root store, which contains all other application parts as modules.

rootStore {
    state: {}, actions: {}, mutations: {},
    modules: {
        route: { this module added by `vuex-route-sync` package} - all other modules can depend on it
        webinterview: { all web interview related store actions,state, mutations},
        review: { interview review tool related actions, etc... thats where all work will be done for interview detalica } 
    }
}

Please read more about vuex stores at https://vuex.vuejs.org/en/modules.html

# HQ View Components


HQ is split into `View Components` - essentially these are groups of views, which have their own routing and own one (or more) store modules.

Currently there is the following structure:

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
