import { defineStore } from 'pinia';
import { mande } from 'mande';
import { commandCall } from '../services/commandService';
import { get, post, patch, del } from '../services/apiService';

//const api = mande('/api/questionnaire/editVariable/' /*, globalOptions*/);

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
            const data = await get(
                '/api/questionnaire/editVariable/' + questionnaireId,
                {
                    variableId: variableId
                }
            );
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

            return commandCall('UpdateVariable', command).then(response => {
                this.initialVariable = Object.assign({}, this.data);
            });
        },

        discardChanges() {
            Object.assign(this.data, this.initialVariable);
            //this.data = Object.assign({}, this.initialVariable);

            //this.data.expression = this.initialVariable.expression;
            //this.data.variable = this.initialVariable.variable;
            //this.data.type = this.initialVariable.type;
            //this.data.label = this.initialVariable.label;
            //this.data.doNotExport = this.initialVariable.doNotExport;
        }
    }
});
