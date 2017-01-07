# Web Interview

> A Vue.js project implementation of Web Interview

## Prerequisites

To build project `NodeJS` required.

\[optional\] [Yarn](https://yarnpkg.com/latest.msi) can be installed for faster and reliable installs instead of `npm install`

## Build Setup

``` bash
# install dependencies
npm install # yarn

# serve with hot reload at localhost:8080
npm run dev

# build for production with minification
npm run build
```

## How it works

`Headquarters` application has new controller `WebInterview` that load `WebInterview/Index.cshtml` view.
    This new view is generated from scratch by `Headquarters.Inteview` app.
    At production build there will be minified and ready for production view.
    For development mode there will be local dev server to generate view and corresponding js/css files on the fly.

Both build modes generate js/css assets into `WB.UI.Headquarters/InteviewApp` folder.
Images and fonts served from 'old' `Dependencies` folder

All stylesheets are compiled from `~/Dependencies/css/common-markup.scss`

### Local development mode

Execute in terminal

```
npm run dev
```

`Node` will run `Webpack` local development server that will serve all js/css dependencies.
You should see message about successful compilation and ready to serve requests.

Navigate browser to http://localhost/headquarters/webinterview

Webpack dev server will automatically reload page on any source code changes.

### Production build

```
npm run build
```

Will build minified versions of js/css scripts.

- `vendor.js` - will contain all dependencies from `.\node_module` folder
- `app.js` - all code from `.\src` folder
- `app.css` - all css code

Images and fonts served from 'old' `Dependencies` folder

### Designer mode

Can be used to apply changes on web interview app without need of bringing app entire Headquarters app.

```
npm run design
```

Designer can run web ui using local development server against superhq or any other dev server.

Url to server should be provided in `build/config/index.js` file at `designer/proxyTable/signalr` json property
