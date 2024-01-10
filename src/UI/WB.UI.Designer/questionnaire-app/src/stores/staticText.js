import { defineStore } from 'pinia';
import { commandCall } from '../services/commandService';
import { get } from '../services/apiService';
import { isEmpty, isNull, filter } from 'lodash';
import moment from 'moment/moment';
import emitter from '../services/emmiter';

export const useStaticTextStore = defineStore('staticText', {
    state: () => ({
        staticText: {},
        initialStaticText: {},
        questionnaireId: null
    }),
    getters: {
        getStaticText: state => state.staticText,
        getBreadcrumbs: state => state.staticText.breadcrumbs,
        getInitialStaticText: state => state.initialStaticText
    },
    actions: {
        async fetchStaticTextData(questionnaireId, staticTextId) {
            const data = await get(
                '/api/questionnaire/editStaticText/' + questionnaireId,
                {
                    staticTextId: staticTextId
                }
            );
            this.questionnaireId = questionnaireId;
            this.setStaticTextData(data);
        },

        setStaticTextData(data) {
            this.staticText = data;
            this.initialStaticText = Object.assign({}, data);
        },

        clear() {
            this.staticText = {};
            this.initialStaticText = {};
            this.questionnaireId = null;
        },

        async saveStaticTextData() {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: this.staticText.id,
                text: this.staticText.text,
                attachmentName: this.staticText.attachmentName,
                enablementCondition: this.staticText.enablementCondition,
                hideIfDisabled: this.staticText.hideIfDisabled,
                validationConditions: this.staticText.validationConditions
            };

            var commandName = 'UpdateStaticText';
            return commandCall(commandName, command).then(async response => {
                this.initialStaticText = Object.assign({}, this.staticText);

                emitter.emit('staticTextUpdated', {
                    itemId: this.staticText.id,
                    text: this.staticText.text,
                    attachmentName: this.staticText.attachmentName,

                    hasCondition: this.hasEnablementConditions(this.staticText),
                    hasValidation: this.hasValidations(this.staticText),
                    hideIfDisabled: this.staticText.hideIfDisabled
                });
            });
        },

        hasEnablementConditions(staticText) {
            return (
                staticText.enablementCondition !== null &&
                /\S/.test(staticText.enablementCondition)
            );
        },

        hasValidations(staticText) {
            return staticText.validationConditions.length > 0;
        },

        //....

        discardChanges() {
            Object.assign(this.staticText, this.initialStaticText);
        },

        deleteStaticText(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: itemId
            };

            return commandCall('DeleteStaticText', command).then(result => {
                if ((this.staticText.id = itemId)) {
                    this.clear();
                }

                emitter.emit('staticTextDeleted', {
                    itemId: itemId
                });
            });
        }
    }
});
