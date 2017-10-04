import "babel-polyfill";

import Vue from 'vue'
import './init'

import "./misc/audioRecorder.js"
import "./misc/htmlPoly.js"
import "./components"
import "shared/components/questions"
import "shared/components/questions/parts"
import "./directives"

import "./errors"
import router from "./router"
import store from "./store"

import App from "./App"

export default new Vue({
    el: "#app",
    render: h => h(App),
    components: { App },
    store,
    router
})
