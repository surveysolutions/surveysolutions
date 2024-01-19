import { defineStore } from 'pinia';
import { getStaticText } from '../services/staticTextService';
import emitter from '../services/emitter';

export const useStaticTextStore = defineStore('staticText', {
    state: () => ({
        staticText: {},
        initialStaticText: {}
    }),
    getters: {
        getStaticText: state => state.staticText,
        getBreadcrumbs: state => state.staticText.breadcrumbs,
        getInitialStaticText: state => state.initialStaticText
    },
    actions: {
        setupListeners() {
            emitter.on('staticTextUpdated', this.staticTextUpdated);
            emitter.on('staticTextDeleted', this.staticTextDeleted);          
        },
        staticTextUpdated(payload) {
            if ((this.staticText.id = payload.itemId)) {
                this.setStaticTextData(payload);
            }
        },
        staticTextDeleted(payload) {
            if ((this.staticText.id = payload.itemId)) {
                this.clear();
            }
        },
        async fetchStaticTextData(questionnaireId, staticTextId) {
            const data = await getStaticText(questionnaireId, staticTextId);            
            this.setStaticTextData(data);
        },
        setStaticTextData(data) {            
            this.initialStaticText = Object.assign({}, data);
            this.staticText = this.initialStaticText;
        },
        clear() {
            this.staticText = {};
            this.initialStaticText = {};
        },
        discardChanges() {
            Object.assign(this.staticText, this.initialStaticText);
        },        
    }
});
