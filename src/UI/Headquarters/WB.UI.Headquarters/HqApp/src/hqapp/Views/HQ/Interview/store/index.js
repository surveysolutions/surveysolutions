import Vue from "vue"
import Vuex from "vuex"
Vue.use(Vuex)

import webinterview from "~/webinterview/store/store.object"

const store = {
    key: "webinterview",
    value: webinterview
}

export default store;