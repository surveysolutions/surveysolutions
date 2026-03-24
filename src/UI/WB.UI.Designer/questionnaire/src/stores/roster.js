import { defineStore } from 'pinia';
import { getRoster } from '../services/rosterService';
import { get, commandCall } from '../services/apiService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useRosterStore = defineStore('roster', {
    state: () => ({
        roster: {},
        initialRoster: {},
        questionnaireId: null,
        rosterId: null
    }),
    getters: {
        getRoster: state => state.roster,
        getInitialRoster: state => state.initialRoster,
        getIsDirty: state => !_.isEqual(state.roster, state.initialRoster)
    },
    actions: {
        setupListeners() {
            emitter.on('rosterUpdated', this.rosterUpdated);
            emitter.on('rosterDeleted', this.rosterDeleted);
            emitter.on('questionDeleted', this.questionDeleted);
            emitter.on('questionAdded', _.debounce(payload => this.questionAdded(payload), 300));
        },
        rosterUpdated(payload) {
            if (this.roster.itemId === payload.roster.itemId) {
                this.setRosterData(payload.roster);
            }
        },
        rosterDeleted(payload) {
            if (this.roster.itemId === payload.id) {
                this.clear();
            }
        },
        questionDeleted(payload) {
            //TODO: remove question from
            //numericIntegerTitles
            //textListsQuestions
            //notLinkedMultiOptionQuestions
            //numericIntegerTitles
        },
        async questionAdded(payload) {
            if (!this.questionnaireId || !this.rosterId) return;

            const currentQuestionnaireId = this.questionnaireId;
            const currentRosterId = this.rosterId;

            try {
                const data = await getRoster(currentQuestionnaireId, currentRosterId);
                if (!data) return;

                // Ensure store has not been cleared or changed to another roster/questionnaire
                if (this.questionnaireId !== currentQuestionnaireId || this.rosterId !== currentRosterId) {
                    return;
                }

                this.roster.numericIntegerQuestions = data.numericIntegerQuestions;
                this.roster.numericIntegerTitles = data.numericIntegerTitles;
                this.roster.textListsQuestions = data.textListsQuestions;
                this.roster.notLinkedMultiOptionQuestions = data.notLinkedMultiOptionQuestions;
            } catch (error) {
                // Swallow error to prevent unhandled promise rejections from async event handler
            }
        },
        async fetchRosterData(questionnaireId, rosterId) {
            this.questionnaireId = questionnaireId;
            this.rosterId = rosterId;
            const data = await getRoster(questionnaireId, rosterId);
            this.setRosterData(data);
        },

        setRosterData(data) {
            this.initialRoster = _.cloneDeep(data);
            this.roster = _.cloneDeep(this.initialRoster);
        },

        clear() {
            this.roster = {};
            this.initialRoster = {};
            this.questionnaireId = null;
            this.rosterId = null;
        },

        discardChanges() {
            this.roster = _.cloneDeep(this.initialRoster);
        }
    }
});
