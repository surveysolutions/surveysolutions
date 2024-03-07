import { defineStore } from 'pinia';
import { getStaticText } from '../services/staticTextService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useStaticTextStore = defineStore('staticText', {
    state: () => ({
        staticText: {},
        initialStaticText: {}
    }),
    getters: {
        getStaticText: state => state.staticText,
        getInitialStaticText: state => state.initialStaticText,
        getIsDirty: state =>
            !_.isEqual(state.staticText, state.initialStaticText)
    },
    actions: {
        setupListeners() {
            emitter.on('staticTextUpdated', this.staticTextUpdated);
            emitter.on('staticTextDeleted', this.staticTextDeleted);
        },
        staticTextUpdated(payload) {
            if ((this.staticText.id = payload.id)) {
                this.setStaticTextData(payload);
            }
        },
        staticTextDeleted(payload) {
            if ((this.staticText.id = payload.id)) {
                this.clear();
            }
        },
        async fetchStaticTextData(questionnaireId, staticTextId) {
            const data = await getStaticText(questionnaireId, staticTextId);
            this.setStaticTextData(data);
        },
        setStaticTextData(data) {
            this.initialStaticText = _.cloneDeep(data);
            this.staticText = _.cloneDeep(this.initialStaticText);
        },
        clear() {
            this.staticText = {};
            this.initialStaticText = {};
        },
        discardChanges() {
            this.staticText = _.cloneDeep(this.initialStaticText);
        }
    }
});
