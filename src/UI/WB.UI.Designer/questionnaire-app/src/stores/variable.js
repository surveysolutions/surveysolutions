import { defineStore } from 'pinia';
import { mande } from 'mande';
import { commandCall } from '../services/commandService';

const api = mande('/api/questionnaire/editVariable/' /*, globalOptions*/);

export const useVariableStore = defineStore('variable', {
    state: () => ({
        data: {},
        initialVariable: {},
        questionnaireId: null
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
            this.questionnaireId = questionnaireId;
            this.setVariableData(data);
        },

        setVariableData(data) {
            this.data = data;
            this.initialVariable = Object.assign({}, data);
        },

        saveVariableData() {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: this.data.id,
                variableData: {
                    expression: this.data.expression,
                    name: this.data.variable,
                    type: this.data.type,
                    label: this.data.label,
                    doNotExport: this.data.doNotExport
                }
            };

            return commandCall('UpdateVariable', command);
        },

        discardChanges() {
            this.data = Object.assign({}, this.initialVariable);
        }
    }
});
