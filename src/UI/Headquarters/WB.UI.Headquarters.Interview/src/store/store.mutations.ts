import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        Vue.set(state.interview, "questionnaire", questionnaireInfo)
    },
    SET_ENTITY_DETAILS(state, entity: IInterviewEntity) {
        Vue.set(state.details.entities, entity.id, entity)
    },
    SET_SECTION_DATA(state, sectionData: ISectionData) {
        Vue.set(state.interview, "currentSection", sectionData.info.id)
        Vue.set(state.details.sections, sectionData.info.id, sectionData)
    },
    SET_INTERVIEW_SECTIONS(state, sections: ISectionInfo[]) {
        Vue.set(state.interview, "sections", sections)

        sections.forEach(sectionInfo => {
            const details = state.details.sections[sectionInfo.id]

            if (details) {
                Vue.set(details, "info", sectionInfo)
            } else {
                Vue.set(state.details.sections, sectionInfo.id, {
                    info: sectionInfo
                })
            }
        })
    },
    SET_ANSWER_NOT_SAVED(state, {id, message}) {
        const validity = state.details.entities[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    }
}
