import { defineStore } from 'pinia';
import { mande } from 'mande';
import { newGuid } from '../helpers/guid';
import { findIndex, isNull, isUndefined, find } from 'lodash';
import { i18n } from '../plugins/localization';
import { useCookies } from 'vue3-cookies';
import { commandCall } from '../services/commandService';
import emitter from '../services/emmiter';

const api = mande('/api/questionnaire/chapter/' /*, globalOptions*/);

export const useTreeStore = defineStore('tree', {
    state: () => ({
        info: {},
        readyToPaste: null
    }),
    getters: {
        getItems: state => (state.info.chapter || {}).items,
        getChapter: state => state.info.chapter,
        getChapterInfo: state => state.info
    },
    actions: {
        setupListeners() {
            emitter.on('questionUpdated', this.questionUpdated);
            emitter.on('staticTextUpdated', this.staticTextUpdated);
            emitter.on('variableUpdated', this.variableUpdated);
            emitter.on('groupUpdated', this.groupUpdated);
            emitter.on('rosterUpdated', this.rosterUpdated);
        },

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
            let index = this.getItemIndexByIdFromParentItemsList(
                parent,
                afterNodeId
            );

            const emptyQuestion = this.createEmptyQuestion(parent);

            const command = {
                questionnaireId: this.questionnaireId,
                parentGroupId: parent.itemId,
                questionId: emptyQuestion.itemId
            };
            if (index != null && index >= 0) {
                index = index + 1;
                command.index = index;
            }

            return commandCall('AddDefaultTypeQuestion', command).then(function(
                result
            ) {
                parent.items.splice(index, 0, emptyQuestion);
                callback(emptyQuestion, parent, index);
                //emitAddedItemState('question', emptyQuestion.itemId);
            });
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
                items: [],
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyQuestion;
        },

        addGroup(parent, afterNodeId, callback) {
            let index = this.getItemIndexByIdFromParentItemsList(
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
            if (index != null && index >= 0) {
                index = index + 1;
                command.index = index;
            }

            return commandCall('AddGroup', command).then(function(result) {
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
            let index = this.getItemIndexByIdFromParentItemsList(
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
            if (index != null && index >= 0) {
                index = index + 1;
                command.index = index;
            }

            return commandCall('AddGroup', command).then(function(result) {
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
            let index = this.getItemIndexByIdFromParentItemsList(
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
            if (index != null && index >= 0) {
                index = index + 1;
                command.index = index;
            }

            return commandCall('AddStaticText', command).then(function(result) {
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
                items: [],
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyStaticText;
        },
        addVariable(parent, afterNodeId, callback) {
            let index = this.getItemIndexByIdFromParentItemsList(
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
            if (index != null && index >= 0) {
                index = index + 1;
                command.index = index;
            }

            return commandCall('AddVariable', command).then(function(result) {
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
                items: [],
                getParentItem: function() {
                    return parent;
                }
            };
            return emptyVariable;
        },

        copyItem(item) {
            const cookies = useCookies();

            var itemIdToCopy = item.itemId;

            var itemToCopy = {
                questionnaireId: this.questionnaireId,
                itemId: itemIdToCopy,
                itemType: item.itemType
            };

            cookies.cookies.remove('itemToCopy');
            cookies.cookies.set('itemToCopy', itemToCopy, { expires: 7 });

            this.readyToPaste = true;
        },

        canPaste() {
            if (this.readyToPaste != null) return this.readyToPaste;
            const cookies = useCookies();
            this.readyToPaste = cookies.cookies.isKey('itemToCopy');
            return this.readyToPaste;
        },

        pasteItemInto(parent) {
            const cookies = useCookies();

            var itemToCopy = cookies.cookies.get('itemToCopy');
            if (!itemToCopy) return;

            const newId = newGuid();

            var command = {
                sourceQuestionnaireId: itemToCopy.questionnaireId,
                sourceItemId: itemToCopy.itemId,
                parentId: parent.itemId,
                entityId: newId,
                questionnaireId: this.questionnaireId
            };

            return commandCall('PasteInto', command).then(() =>
                this.fetchTree(this.questionnaireId, this.chapterId)
            );
        },

        pasteItemAfter(afterNode) {
            const cookies = useCookies();

            var itemToCopy = cookies.cookies.get('itemToCopy');
            if (!itemToCopy) return;

            const newId = newGuid();

            var command = {
                sourceQuestionnaireId: itemToCopy.questionnaireId,
                sourceItemId: itemToCopy.itemId,
                entityId: newId,
                questionnaireId: this.questionnaireId,
                itemToPasteAfterId: afterNode.itemId
            };

            return commandCall('PasteAfter', command).then(() =>
                this.fetchTree(this.questionnaireId, this.chapterId)
            );
        },

        getItemIndexByIdFromParentItemsList(parent, id) {
            if (!parent || !id) return null;

            var index = findIndex(parent.items, function(i) {
                return i.itemId === id;
            });

            return index < 0 ? null : index;
        },

        deleteGroup(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                groupId: itemId
            };

            return commandCall('DeleteGroup', command).then(function(result) {
                parent.items.splice(index, 0, group);
                callback(group, parent, index);
            });
        },

        deleteQuestion(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                questionId: itemId
            };

            return commandCall('DeleteQuestion', command);
        },

        deleteVariable(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: itemId
            };

            return commandCall('DeleteVariable', command);
        },

        deleteStaticText(itemId) {
            var command = {
                questionnaireId: this.questionnaireId,
                entityId: itemId
            };

            return commandCall('DeleteStaticText', command);
        },

        moveItem(item, newParentId, index) {
            if (item.itemType == 'Question')
                return this.moveQuestion(item.itemId, index, newParentId);
            else if (item.itemType == 'StaticText')
                return this.moveStaticText(item.itemId, index, newParentId);
            else if (item.itemType == 'Variable')
                return this.moveVariable(item.itemId, index, newParentId);
            else if (item.itemType == 'Group')
                return this.moveGroup(item.itemId, index, newParentId);
        },

        moveGroup(groupId, index, destGroupId) {
            var command = {
                targetGroupId: destGroupId,
                targetIndex: index,
                groupId: groupId,
                questionnaireId: this.questionnaireId
            };

            return commandCall('MoveGroup', command);
        },

        moveQuestion(questionId, index, destGroupId) {
            var command = {
                targetGroupId: destGroupId,
                targetIndex: index,
                questionId: questionId,
                questionnaireId: this.questionnaireId
            };

            return commandCall('MoveQuestion', command);
        },

        moveStaticText(entityId, index, destGroupId) {
            var command = {
                targetEntityId: destGroupId,
                targetIndex: index,
                entityId: entityId,
                questionnaireId: this.questionnaireId
            };

            return commandCall('MoveStaticText', command);
        },

        moveVariable(entityId, index, destGroupId) {
            var command = {
                targetEntityId: destGroupId,
                targetIndex: index,
                entityId: entityId,
                questionnaireId: this.questionnaireId
            };

            return commandCall('MoveVariable', command);
        },

        questionUpdated(data) {
            const itemId = data.itemId.replaceAll('-', '');
            var question = this.findTreeItem(itemId);
            if (isNull(question) || isUndefined(question)) return;

            question.title = data.title;
            question.variable = data.variable;
            question.type = data.type;
            question.hasValidation = data.hasValidation;
            question.hasCondition = data.hasCondition;
            question.linkedToEntityId = data.linkedToEntityId;
            question.linkedToType = data.linkedToType;
            question.isInteger = data.isInteger;
            question.yesNoView = data.yesNoView;
            question.hideIfDisabled = data.hideIfDisabled;
        },

        staticTextUpdated(data) {
            const itemId = data.itemId.replaceAll('-', '');
            var staticText = this.findTreeItem(itemId);
            if (isNull(staticText) || isUndefined(staticText)) return;
            staticText.text = data.text;
            staticText.attachmentName = data.attachmentName;

            staticText.hasValidation = data.hasValidation;
            staticText.hasCondition = data.hasCondition;
            staticText.hideIfDisabled = data.hideIfDisabled;
        },

        variableUpdated(data) {
            const itemId = data.itemId.replaceAll('-', '');
            var variable = this.findTreeItem(itemId);
            if (isNull(variable) || isUndefined(variable)) return;
            variable.variableData.name = data.name;
            variable.variableData.label = data.label;
        },

        groupUpdated(data) {
            const itemId = data.itemId.replaceAll('-', '');
            const chapter = this.getChapter;
            if (chapter.itemId === itemId) {
                chapter.title = data.title;
                chapter.hasCondition = data.hasCondition;
                chapter.hideIfDisabled = data.hideIfDisabled;
            }

            var group = this.findTreeItem(itemId);
            if (isNull(group) || isUndefined(group)) return;
            group.title = data.title;
            group.variable = data.variable;
            group.hasCondition = data.hasCondition;
            group.hideIfDisabled = data.hideIfDisabled;
        },

        rosterUpdated(data) {
            const itemId = data.itemId.replaceAll('-', '');
            var roster = this.findTreeItem(itemId);
            if (isNull(roster) || isUndefined(roster)) return;
            roster.title = data.title;
            roster.variable = data.variable;
            roster.hasCondition = data.hasCondition;
            roster.hideIfDisabled = data.hideIfDisabled;
        },

        findTreeItem(value) {
            var o;
            const items = this.getItems;
            items.some(function iter(a) {
                if (a.itemId === value) {
                    o = a;
                    return true;
                }
                return Array.isArray(a.items) && a.items.some(iter);
            });
            return o;
        }
    }
});
