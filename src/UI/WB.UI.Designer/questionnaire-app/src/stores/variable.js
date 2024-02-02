import { defineStore } from 'pinia';
import { getVariable } from '../services/variableService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useVariableStore = defineStore('variable', {
    state: () => ({
        variable: {},
        initialVariable: {}
    }),
    getters: {
        getVariable: state => state.variable,
        getInitialVariable: state => state.initialVariable,
        getIsDirty: state => !_.isEqual(state.variable, state.initialVariable)
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
            this.initialVariable = _.cloneDeep(data);
            this.variable = _.cloneDeep(this.initialVariable);
        },
        clear() {
            this.variable = {};
            this.initialVariable = {};
        },
        discardChanges() {
            this.variable = _.cloneDeep(this.initialVariable);
        }
    }
});
