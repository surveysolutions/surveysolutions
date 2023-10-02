import { defineStore } from 'pinia'
import { debounce } from 'lodash'
import { mande } from 'mande'

const api = mande('/api/questionnaire/get/'/*, globalOptions*/)

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {}
    }),
    getters: {
      getInfo: (state) => state.info,
    },
    actions: {

      async fetchQuestionnaireInfo(questionnaireId) {
        const info = await api.get(questionnaireId)
        this.setQuestionnaireInfo(info)
      },

      setQuestionnaireInfo(info) {
        this.info = info
      }

    },    
})
