import { defineStore } from 'pinia';
import { commandCall } from '../services/commandService';
import { get } from '../services/apiService';
import emitter from '../services/emitter';

export const useRosterStore = defineStore('roster', {
    state: () => ({
        roster: {},
        initialRoster: {},
        questionnaireId: null
    }),
    getters: {
        getRoster: state => state.roster,
        getBreadcrumbs: state => state.roster.breadcrumbs
    },
    actions: {
        async fetchRosterData(questionnaireId, rosterId) {
            const data = await get(
                '/api/questionnaire/editRoster/' + questionnaireId,
                {
                    rosterId: rosterId
                }
            );
            this.questionnaireId = questionnaireId;
            this.setRosterData(data);
        },

        setRosterData(data) {
            this.roster = data;
            this.initialRoster = Object.assign({}, data);
        },

        clear() {
            this.roster = {};
            this.initialRoster = {};
            this.questionnaireId = null;
        },

        saveRosterData() {
            var command = {
                questionnaireId: this.questionnaireId,
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
            Object.assign(this.roster, this.initialRoster);
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
