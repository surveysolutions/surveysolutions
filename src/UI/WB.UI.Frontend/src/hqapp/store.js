import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const store = new Vuex.Store({
    getters:{
        workspace(){
            return window.CONFIG.workspace
        },
    },
})

export default store
