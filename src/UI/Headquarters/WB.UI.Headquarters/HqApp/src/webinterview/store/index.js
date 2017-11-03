import { safeStore } from "../errors"
import actions from "./store.actions"
import connection from "./store.connection"
import { fetch } from "./store.fetch.js"
import mutations from "./store.mutations"
import sidebar from "./store.sidebar"

const store = safeStore({
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
        },
        interviewCompleted: false
    },
    actions,
    mutations,
    getters: {
        loadingProgress(state) {
            const loadedCount = state.loadedEntitiesCount
            const totalCount = state.entities != null 
                ? state.entities.length
                : 0

            return loadedCount === 0 || totalCount === 0 || (loadedCount < totalCount)
        },
        addCommentsAllowed(state){
            return !state.receivedByInterviewer;
        }
    }
})

export default store
