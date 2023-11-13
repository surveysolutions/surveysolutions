import { defineStore } from 'pinia';
import { mande } from 'mande';

const api = mande('/api/questionnaire/editVariable/' /*, globalOptions*/);

export const useVariableStore = defineStore('variable', {
    state: () => ({
        data: {}
    }),
    getters: {
        getData: state => state.data
    },
    actions: {
        async fetchVarableData(questionnaireId, variableId) {
            const data = await api.get(questionnaireId, {
                query: {
                    variableId: variableId
                }
            });
            this.setVariableData(data);
        },

        setVariableData(data) {
            this.data = data;
        }
    }
});
