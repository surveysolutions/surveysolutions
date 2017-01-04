import * as Vue from "vue"
import * as Vuex from "vuex"

Vue.use(Vuex)

import router from "./router"
import store from "./store"
import "./services"

import App from "./App"

const vueApp = new Vue({
  el: "#app",
  template: "<App/>",
  components: { App },
  store,
  router
})
