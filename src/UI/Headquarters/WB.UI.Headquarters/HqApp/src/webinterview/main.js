import Vue from "vue";
import Vuex from "vuex";
import { sync } from "vuex-router-sync";
Vue.use(Vuex);

import config from "~/shared/config";
Vue.use(config);

import VueTextareaAutosize from "vue-textarea-autosize";
Vue.use(VueTextareaAutosize);

import Vuei18n from "~/shared/plugins/locale";
import { browserLanguage } from "~/shared/helpers";
const i18n = Vuei18n.initialize(browserLanguage);

import "./init";
import "./errors";
import box from "./components/modal";

require("./componentsRegistry");

const createRouter = require("./router").default;

const store = new Vuex.Store({
  modules: {
    webinterview: require("./store").default
  }
});

const router = createRouter(store);

sync(store, router);

const App = require("./App").default;
const installApi = require("./api").install;

installApi(Vue, {
  store
});

box.init(i18n, browserLanguage);

window._api = {
  store,
  router
};

export default new Vue({
  el: "#app",
  render: h => h(App),
  components: {
    App
  },
  store,
  router
});
