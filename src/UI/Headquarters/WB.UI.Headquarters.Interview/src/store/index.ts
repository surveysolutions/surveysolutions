import Vue from "vue"
import Vuex from "vuex"

import { safeStore } from "../errors"

import actions from "./store.actions"
import connection from "./store.connection"
import fetch from "./store.fetch"
import mutations from "./store.mutations"
import sidebar from "./store.sidebar"

const store = new Vuex.Store(safeStore({
    modules: { fetch, sidebar, connection },
    state: {
        lastActivityTimestamp: new Date(),
        hasCoverPage: false,
        loadedEntitiesCount: 0,
        questionnaireTitle: "",
        interviewKey: "",
        firstSectionId: "",
        entities: [], /* IInterviewEntity[] */
        entityDetails: { /* string: object */ },
        breadcrumbs: {
            breadcrumbs: []
        },
        coverInfo: {
            entitiesWithComments: [],
            identifyingQuestions: []
        }
    },
    actions,
    mutations
}))

export default store
