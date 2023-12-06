import { defineStore } from 'pinia';
import { mande } from 'mande';
import { commandCall } from '../services/commandService';

const api = mande('/api/questionnaire/editGroup/' /*, globalOptions*/);

export const useGroupStore = defineStore('group', {
    state: () => ({
        group: {},
        initialGroup: {},
        questionnaireId: null
    }),
    getters: {
        getGroup: state => state.group.group,
        getBreadcrumbs: state => state.group.breadcrumbs
    },
    actions: {
        async fetchGroupData(questionnaireId, groupId) {
            const data = await api.get(questionnaireId, {
                query: {
                    groupId: groupId
                }
            });
            this.questionnaireId = questionnaireId;
            this.setGroupData(data);
        },

        setGroupData(data) {
            this.group = data;
            this.initialGroup = Object.assign({}, data.group);
        },

        saveGroupData() {
            var command = {
                questionnaireId: this.questionnaireId,
                groupId: this.group.group.id,
                title: this.group.group.title,
                condition: this.group.group.enablementCondition,
                hideIfDisabled: this.group.group.hideIfDisabled,
                isRoster: false,
                rosterSizeQuestionId: null,
                rosterSizeSource: 'Question',
                rosterFixedTitles: null,
                rosterTitleQuestionId: null,
                variableName: this.group.group.variableName
            };

            return commandCall('UpdateGroup', command).then(response => {
                this.initialVariable = Object.assign({}, this.group.group);
            });
        },

        discardChanges() {
            Object.assign(this.group.group, this.initialGroup);
        }
    }
});
