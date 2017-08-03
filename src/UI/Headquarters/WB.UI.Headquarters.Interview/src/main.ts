import "es6-promise/auto"

import { assetsPath } from "./config"
declare const process: any
declare let __webpack_public_path__: any

if (process.env.NODE_ENV === "production") {
    __webpack_public_path__ = assetsPath
}

import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

import * as poly from "smoothscroll-polyfill"
poly.polyfill()

import "./misc/audioRecorder.js"
import "./misc/htmlPoly.js"

import "./components"
import "./components/entities"
import "./components/entities/parts"
import "./directives"

import "./errors.ts"
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
