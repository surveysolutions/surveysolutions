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

      async fetchQuestionnaireInfo() {
        const info = await api.get('c3e8c62233ef4b8ca0a186cb7eaf9d2e')
        this.setQuestionnaireInfo(info)
      },

      setQuestionnaireInfo(info) {
        this.info = info
        console.log('set info')
      }

    },    
})
