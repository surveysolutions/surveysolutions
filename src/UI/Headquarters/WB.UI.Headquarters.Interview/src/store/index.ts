import * as Vue from "vue"
import * as Vuex from "vuex"

import { apiCaller } from "../api"
import { safeStore } from "../errors"
import router from "./../router"
import actions from "./store.actions"
import mutations from "./store.mutations"

const store = new Vuex.Store(safeStore({
    state: {
        questionnaire: null,
        prefilledQuestions: [],
        firstSectionId: null,
        entityDetails: {},
        section: {},
        interview: {
            id: null
        }
    },
    actions,
    mutations
}))

export default store
