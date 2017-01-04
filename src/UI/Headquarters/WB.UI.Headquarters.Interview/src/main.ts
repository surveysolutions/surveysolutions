declare var require: any

const  Vue = require('vue')
const Vuex = require('vuex')

Vue.use(Vuex)

import router from './router'
import store from './store'
import './services'

import App from './App'

const vueApp = new Vue({
  el: '#app',
  template: '<App/>',
  components: { App },
  store,
  router
})