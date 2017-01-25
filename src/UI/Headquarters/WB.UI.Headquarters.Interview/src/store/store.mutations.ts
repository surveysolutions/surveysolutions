import * as _ from "lodash"
import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_ENTITY_DETAILS(state, entity: IInterviewEntity) {
        Vue.set(state.entityDetails, entity.id, entity)
    },
    SET_ENTITIES_DETAILS(state, entities: IInterviewEntity[]) {
        _.forEach(entities, entity => {
            Vue.set(state.entityDetails, entity.id, entity)
        })
    },
    SET_SECTION_DATA(state, sectionData: IInterviewEntityWithType[]) {
        state.entities = sectionData
    },
    CLEAR_ENTITY(state, id) {
        Vue.delete(state.entityDetails, id)
    },
    SET_ANSWER_NOT_SAVED(state, { id, message }) {
        const validity = state.entityDetails[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    },
    SET_BREADCRUMPS(state, crumps) {
        Vue.set(state, "breadcrumbs", crumps)
    }
}
