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
            let localFetchInProgress = state.fetch.inProgress > 0

            if (localFetchInProgress) {
                // if there is any pending fetch state local to some questions,
                // then do not show global loading Progress

                if(!state.entities || !state.entityDetails) return true;

                const keys = Object.keys(state.entityDetails)
                for(var i  = 0, len = keys.length; i < len; i++) {
                    const key = keys[i]

                    if(state.entityDetails[key].fetching === true) {
                        return false;
                    }
                }

                return true
            }
            return false
        },
        addCommentsAllowed(state) {
            return !state.interviewCannotBeChanged;
        },
        basePath() {
            return window.input ? window.input.settings.config.basePath : window.CONFIG.basePath;
        }
    }
})

export default store
