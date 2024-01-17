import { defineStore } from 'pinia';
import { commandCall } from '../services/commandService';
import { get } from '../services/apiService';
import emitter from '../services/emitter';

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

                emitter.emit('groupUpdated', {
                    itemId: this.group.group.id,
                    variable: this.group.group.variableName,
                    title: this.group.group.title,
                    hasCondition:
                        this.group.group.enablementCondition !== null &&
                        /\S/.test(this.group.group.enablementCondition),
                    hideIfDisabled: this.group.group.hideIfDisabled
                });
            });
        },

        discardChanges() {
            Object.assign(this.group.group, this.initialGroup);
        },

        deleteGroup(questionnaireId, itemId) {
            var command = {
                questionnaireId: questionnaireId,
                groupId: itemId
            };

            return commandCall('DeleteGroup', command).then(result => {
                if ((this.group.group.id = itemId)) {
                    this.clear();
                }

                emitter.emit('groupDeleted', {
                    itemId: itemId
                });
            });
        }
    }
});
