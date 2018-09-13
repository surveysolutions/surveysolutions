import "bootstrap";
import "bootstrap-select";

import Vue from "vue";
import { sync } from "vuex-router-sync";
import Vuei18n from "~/shared/plugins/locale";
import http from "~/shared/plugins/http";
import config from "~/shared/config";
import store from "./store";
import moment from "moment";

import "./components";
import "./compatibility.js";
import "~/webinterview/componentsRegistry";

import VueTextareaAutosize from "vue-textarea-autosize";
Vue.use(VueTextareaAutosize);

import box from "~/webinterview/components/modal";
import "flatpickr/dist/flatpickr.css";
import "toastr/build/toastr.css";
import * as poly from "smoothscroll-polyfill";
poly.polyfill();

import hqApi from "./api";

import { browserLanguage } from "~/shared/helpers";
moment.locale(browserLanguage);

const i18n = Vuei18n.initialize(browserLanguage);

Vue.use(config);
Vue.use(http);
Vue.use(hqApi);

const viewsProvider = require("./Views").default;
const Router = require("./router").default;

const views = viewsProvider(store);

const router = new Router({
  routes: views.routes
}).router;

sync(store, router);

box.init(i18n, browserLanguage);
export default new Vue({
  el: "#vueApp",
  render: h => h("router-view"),
  store,
  router
});
