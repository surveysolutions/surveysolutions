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
            let loadedCount = 0;

            state.entities.forEach(entity => {
                if (state.entityDetails[entity.identity] != null) {
                    loadedCount = loadedCount + 1;
                }
            })

            const totalCount = state.entities != null
                ? state.entities.length
                : 0
                
            var result = loadedCount === 0 || totalCount === 0 || (loadedCount < totalCount);

            return result;
        },
        addCommentsAllowed(state) {
            return !state.receivedByInterviewer;
        }
    }
})

export default store
