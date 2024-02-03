import { defineStore } from 'pinia';
import { getRoster } from '../services/rosterService';
import { get, commandCall } from '../services/apiService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useRosterStore = defineStore('roster', {
    state: () => ({
        roster: {},
        initialRoster: {}
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
        },
        rosterUpdated(payload) {
            if ((this.roster.itemId = payload.itemId)) {
                this.setRosterData(payload);
            }
        },
        rosterDeleted(payload) {
            if ((this.roster.itemId = payload.itemId)) {
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
        async fetchRosterData(questionnaireId, rosterId) {
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
        },

        discardChanges() {
            this.roster = _.cloneDeep(this.initialRoster);
        },

        getQuestionsEligibleForNumericRosterTitle(rosterSizeQuestionId) {
            return get(
                '/api/questionnaire/getQuestionsEligibleForNumericRosterTitle/' +
                    this.questionnaireId,
                {
                    rosterId: this.roster.itemId,
                    rosterSizeQuestionId: rosterSizeQuestionId
                }
            );
        }
    }
});
