import "es6-promise/auto"
import * as Vue from "vue"
import * as Vuex from "vuex"

Vue.use(Vuex)

import "./components/questions"
import "./components/questions/parts"

import "./errors.ts"
import router from "./router"
import store from "./store"

import App from "./App"

const vueApp = new Vue({
  el: "#app",
  template: "<App/>",
  components: { App },
  store,
  router
})
