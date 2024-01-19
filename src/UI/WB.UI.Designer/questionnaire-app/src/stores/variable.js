import { defineStore } from 'pinia';
import { getVariable } from '../services/variableService';
import emitter from '../services/emitter';

export const useVariableStore = defineStore('variable', {
    state: () => ({
        variable: {},
        initialVariable: {}
    }),
    getters: {
        getData: state => state.variable
    },
    actions: {
        setupListeners() {
            emitter.on('variableUpdated', this.variableUpdated);
            emitter.on('variableDeleted', this.variableDeleted);          
        },
        variableUpdated(payload) {
            if ((this.variable.id = payload.itemId)) {
                this.setVariableData(payload);
            }
        },
        variableDeleted(payload) {
            if ((this.variable.id = payload.itemId)) {
                this.clear();
            }
        },
        async fetchVarableData(questionnaireId, variableId) {
            const data = await getVariable(questionnaireId, variableId);
            this.setVariableData(data);
        },
        setVariableData(data) {
            this.initialVariable = Object.assign({}, data);
            this.variable = this.initialVariable;            
        },
        clear() {
            this.variable = {};
            this.initialVariable = {};
        },        
        discardChanges() {
            Object.assign(this.variable, this.initialVariable);            
        }
    }
});
