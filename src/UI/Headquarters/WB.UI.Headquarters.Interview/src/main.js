require("babel-polyfill");
global.jQuery = require('jquery');

import { assetsPath } from "./config"

if (process.env.NODE_ENV === "production") {
    __webpack_public_path__ = assetsPath
}

import Vue from "vue"

import * as poly from "smoothscroll-polyfill"
poly.polyfill()

import "./misc/audioRecorder.js"
import "./misc/htmlPoly.js"

import "./components"
import "./components/entities"
import "./components/entities/parts"
import "./directives"

import "./errors"
import router from "./router"
import store from "./store"

import App from "./App"

const vueApp = new Vue({
    el: "#app",
    render: h => h(App),
    components: { App },
    store,
    router
})
