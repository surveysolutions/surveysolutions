import { defineStore } from 'pinia';
import { commandCall } from '../services/commandService';
import { get } from '../services/apiService';

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
            const data = await get(
                '/api/questionnaire/editGroup/' + questionnaireId,
                {
                    groupId: groupId
                }
            );
            this.questionnaireId = questionnaireId;
            this.setGroupData(data);
        },

        setGroupData(data) {
            this.group = data;
            this.initialGroup = Object.assign({}, data.group);
        },

        clear() {
            this.group = {};
            this.initialGroup = {};
            this.questionnaireId = null;
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
