import * as Vue from "vue"
import * as Vuex from "vuex"

import { safeStore } from "../errors"
import actions from "./store.actions"
import fetch from "./store.fetch"
import mutations from "./store.mutations"
import sidebar from "./store.sidebar"

const store = new Vuex.Store(safeStore({
    modules: { fetch, sidebar },
    state: {
        entities: [], /* IInterviewEntity[] */
        entityDetails: { /* string: object */ },
        breadcrumbs: {
            breadcrumbs: []
        },
        sidebar: [ /* ISidebarPanel */]
    },
    actions,
    mutations
}))

export default store
