import { defineStore } from 'pinia';
import { getVariable } from '../services/variableService';
import emitter from '../services/emitter';
import { cloneDeep, isEqual } from 'lodash';

export const useVariableStore = defineStore('variable', {
    state: () => ({
        variable: {},
        initialVariable: {}
    }),
    getters: {
        getVariable: state => state.variable,
        getInitialVariable: state => state.initialVariable,
        getIsDirty: state => !isEqual(state.variable, state.initialVariable)
    },
    actions: {
        setupListeners() {
            emitter.on('variableUpdated', this.variableUpdated);
            emitter.on('variableDeleted', this.variableDeleted);
        },
        variableUpdated(payload) {
            if (this.variable.id === payload.id) {
                this.setVariableData(payload);
            }
        },
        variableDeleted(payload) {
            if (this.variable.id === payload.id) {
                this.clear();
            }
        },
        async fetchVarableData(questionnaireId, variableId) {
            const data = await getVariable(questionnaireId, variableId);
            this.setVariableData(data);
        },
        setVariableData(data) {
            this.initialVariable = cloneDeep(data);
            this.variable = cloneDeep(this.initialVariable);
        },
        clear() {
            this.variable = {};
            this.initialVariable = {};
        },
        discardChanges() {
            this.variable = cloneDeep(this.initialVariable);
        }
    }
});
