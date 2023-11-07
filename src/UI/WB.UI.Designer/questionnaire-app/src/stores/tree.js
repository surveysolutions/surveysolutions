import { defineStore } from 'pinia';
import { mande } from 'mande';
import { useBlockUIStore } from './blockUI';
import { newGuid } from '../helpers/guid';
import { findIndex } from 'lodash';
import { i18n } from '../plugins/localization';

const api = mande('/api/questionnaire/chapter/' /*, globalOptions*/);
const commandsApi = mande('/api/command' /*, globalOptions*/);

export const useTreeStore = defineStore('tree', {
    state: () => ({
        info: {}
    }),
    getters: {
        getItems: state => (state.info.chapter || {}).items,
        getChapter: state => state.info.chapter
    },
    actions: {
        async fetchTree(questionnaireId, chapterId) {
            const info = await api.get(questionnaireId, {
                query: {
                    chapterId: chapterId
                }
            });
            this.setChapterInfo(info);
            this.questionnaireId = questionnaireId;
            this.chapterId = chapterId;
        },

        setChapterInfo(info) {
            this.info = info;
        },

        addQuestion(parent, afterNodeId, callback) {
            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );
            const emptyQuestion = this.createEmptyQuestion(parent);

            const command = {
                questionnaireId: this.questionnaireId,
                parentGroupId: parent.itemId,
                questionId: emptyQuestion.itemId
            };
            if (index >= 0) command.index = index + 1;

            return this.commandCall('AddDefaultTypeQuestion', command).then(
                function(result) {
                    parent.items.splice(index, 0, emptyQuestion);
                    callback(emptyQuestion, parent, index);
                    //emitAddedItemState('question', emptyQuestion.itemId);
                }
            );
        },
        createEmptyQuestion(parent) {
            var newId = newGuid();
            var emptyQuestion = {
                itemId: newId,
                title: '',
                type: 'Text',
                itemType: 'Question',
                hasCondition: false,
                hasValidation: false,
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyQuestion;
        },

        addGroup(parent, afterNodeId, callback) {
            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );
            const group = this.createEmptyGroup(parent);

            var command = {
                questionnaireId: this.questionnaireId,
                groupId: group.itemId,
                title: group.title,
                condition: '',
                hideIfDisabled: false,
                isRoster: false,
                rosterSizeQuestionId: null,
                rosterSizeSource: 'Question',
                rosterFixedTitles: null,
                rosterTitleQuestionId: null,
                parentGroupId: parent.itemId,
                variableName: null
            };
            if (index >= 0) command.index = index + 1;

            return this.commandCall('AddGroup', command).then(function(result) {
                parent.items.splice(index, 0, group);
                callback(group, parent, index);
            });
        },
        createEmptyGroup(parent) {
            var newId = newGuid();
            var emptyGroup = {
                itemId: newId,
                title: i18n.t('DefaultNewSubsection'),
                items: [],
                itemType: 'Group',
                hasCondition: false,
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyGroup;
        },
        addRoster(parent, afterNodeId, callback) {
            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );
            const group = this.createEmptyRoster(parent);

            var command = {
                questionnaireId: this.questionnaireId,
                groupId: group.itemId,
                title: group.title,
                condition: '',
                hideIfDisabled: false,
                isRoster: true,
                rosterSizeQuestionId: null,
                rosterSizeSource: 'FixedTitles',
                fixedRosterTitles: [
                    { value: 1, title: 'First Title' },
                    { value: 2, title: 'Second Title' }
                ],
                rosterTitleQuestionId: null,
                parentGroupId: parent.itemId,
                variableName: group.variableName
            };
            if (index >= 0) command.index = index + 1;

            return this.commandCall('AddGroup', command).then(function(result) {
                parent.items.splice(index, 0, group);
                callback(group, parent, index);
            });
        },
        createEmptyRoster(parent) {
            var newId = newGuid();
            var emptyRoster = {
                itemId: newId,
                title: i18n.t('DefaultNewRoster') + ' - %rostertitle%',
                items: [],
                itemType: 'Group',
                hasCondition: false,
                isRoster: true,
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyRoster;
        },
        addStaticText(parent, afterNodeId, callback) {
            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );
            const staticText = this.createEmptyStaticText(parent);

            const command = {
                questionnaireId: this.questionnaireId,
                parentId: parent.itemId,
                entityId: staticText.itemId,
                text: staticText.text
            };
            if (index >= 0) command.index = index + 1;

            return this.commandCall('AddStaticText', command).then(function(
                result
            ) {
                parent.items.splice(index, 0, staticText);
                callback(staticText, parent, index);
            });
        },
        createEmptyStaticText(parent) {
            var newId = newGuid();
            var emptyStaticText = {
                itemId: newId,
                text: i18n.t('DefaultNewStaticText'),
                itemType: 'StaticText',
                hasCondition: false,
                hasValidation: false,
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyStaticText;
        },
        addVariable(parent, afterNodeId, callback) {
            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );
            const variable = this.createEmptyVariable(parent);

            var command = {
                questionnaireId: this.questionnaireId,
                entityId: variable.itemId,
                parentId: parent.itemId,
                variableData: {}
            };
            if (index >= 0) command.index = index + 1;

            return this.commandCall('AddVariable', command).then(function(
                result
            ) {
                parent.items.splice(index, 0, variable);
                callback(variable, parent, index);
            });
        },
        createEmptyVariable(parent) {
            var newId = newGuid();
            var emptyVariable = {
                itemId: newId,
                itemType: 'Variable',
                variableData: {},
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyVariable;
        },

        pasteItemInto(root) {
            //treeStore.pasteItemInto(chapter);
        },

        async commandCall(type, command) {
            const blockUI = useBlockUIStore();
            if (type.indexOf('Move') < 0) {
                blockUI.start();
            }

            return commandsApi
                .post({
                    type: type,
                    command: JSON.stringify(command)
                })
                .then(response => {
                    blockUI.stop();
                    return response;
                });
        },

        getItemIndexByIdFromParentItemsList(parent, id) {
            if (!parent || !id) return -1;

            var index = findIndex(parent.items, function(i) {
                return i.itemId === id;
            });

            return index;
        },

        deleteGroup(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                groupId: itemId
            };

            return this.commandCall('DeleteGroup', command).then(function(
                result
            ) {
                parent.items.splice(index, 0, group);
                callback(group, parent, index);
            });
        },

        deleteQuestion(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                questionId: itemId
            };

            return this.commandCall('DeleteQuestion', command);
        },

        deleteVariable(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: itemId
            };

            return this.commandCall('DeleteVariable', command);
        },

        deleteStaticText(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: itemId
            };

            return this.commandCall('DeleteStaticText', command);
        }
    }
});
