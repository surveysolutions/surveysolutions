declare var require: any

const JQuery = require('JQuery')
const  Vue = require('vue')
const Vuex = require('vuex')

Vue.use(Vuex)

import router from './router'
import store from './store'

var App = require('./app.vue')

const vueApp = new Vue({
  el: '#app',
  template: '<App/>',
  components: { App },
  store,
  router
})