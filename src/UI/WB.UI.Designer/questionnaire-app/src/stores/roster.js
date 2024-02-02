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
        saveRosterData(questionnaireId) {
            var command = {
                questionnaireId: questionnaireId,
                groupId: this.roster.itemId,
                title: this.roster.title,
                description: this.roster.description,
                condition: this.roster.enablementCondition,
                hideIfDisabled: this.roster.hideIfDisabled,
                variableName: this.roster.variableName,
                displayMode: this.roster.displayMode,
                isRoster: true
            };

            switch (this.roster.type) {
                case 'Fixed':
                    command.rosterSizeSource = 'FixedTitles';
                    command.fixedRosterTitles = this.roster.fixedRosterTitles;
                    break;
                case 'Numeric':
                    command.rosterSizeQuestionId = this.roster.rosterSizeNumericQuestionId;
                    command.rosterTitleQuestionId = this.roster.rosterTitleQuestionId;
                    break;
                case 'List':
                    command.rosterSizeQuestionId = this.roster.rosterSizeListQuestionId;
                    break;
                case 'Multi':
                    command.rosterSizeQuestionId = this.roster.rosterSizeMultiQuestionId;
                    break;
            }

            return commandCall('UpdateGroup', command).then(response => {
                this.initialRoster = Object.assign({}, this.roster);

                emitter.emit('rosterUpdated', {
                    itemId: this.roster.itemId,
                    variable: this.roster.variableName,
                    title: this.roster.title,
                    hasCondition:
                        this.roster.enablementCondition !== null &&
                        /\S/.test(this.roster.enablementCondition),
                    type: 'Roster',
                    hideIfDisabled: this.roster.hideIfDisabled
                });
            });
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
        },

        deleteRoster(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                groupId: itemId
            };

            return commandCall('DeleteGroup', command).then(result => {
                if ((this.roster.itemId = itemId)) {
                    this.clear();
                }

                emitter.emit('rosterDeleted', {
                    itemId: itemId
                });
            });
        }
    }
});
