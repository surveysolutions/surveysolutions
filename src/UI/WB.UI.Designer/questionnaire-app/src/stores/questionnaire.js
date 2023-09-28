import { defineStore } from 'pinia'
import { debounce } from 'lodash'
import { mande } from 'mande'

const api = mande('/api/questionnaire/get/'/*, globalOptions*/)

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        lastActivityTimestamp: new Date(),
        title: '',
        coverSectionId: '',
        coverInfo: {
            entitiesWithComments: [],
        },
    }),
    getters: {
      /*basePath() {
          return import.meta.env.BASE_URL
      },*/
    },
    actions: {

      alert() {
        alert('alert')
      },

      async fetchQuestionnaireInfo() {
        debugger
        const info = await api.get('c3e8c62233ef4b8ca0a186cb7eaf9d2e')
        this.setQuestionnaireInfo(info)
      },

      setQuestionnaireInfo(info) {
        this.title = info.title
      }

    },    
})

//export default useQuestionnaireStore
