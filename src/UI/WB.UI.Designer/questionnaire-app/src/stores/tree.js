import { defineStore } from 'pinia';
import { get } from '../services/apiService';
import { findIndex, isNull, isUndefined, cloneDeep, orderBy } from 'lodash';
import emitter from '../services/emitter';
import { isNotLinkedOrLinkedToTextList } from '../helpers/question';

export const useTreeStore = defineStore('tree', {
    state: () => ({
        info: {},
        variableNamesStore: {
            getTokens() {
                return this.variableNamesTokens;
            },
            getCompletions() {
                return this.variableNamesCompletions;
            },
            variableNamesTokens: '',
            variableNamesCompletions: [],
            lastUpdated: null
        }
    }),
    getters: {
        getItems: state => (state.info.chapter || {}).items,
        getChapterData: state => state.info.chapter,
        getChapter: state => state.info,
        getVariableNames: state => state.variableNamesStore
    },
    actions: {
        setupListeners() {
            emitter.on('questionUpdated', this.questionUpdated);
            emitter.on('staticTextUpdated', this.staticTextUpdated);
            emitter.on('variableUpdated', this.variableUpdated);
            emitter.on('groupUpdated', this.groupUpdated);
            emitter.on('rosterUpdated', this.rosterUpdated);

            emitter.on('questionDeleted', this.questionDeleted);
            emitter.on('staticTextDeleted', this.staticTextDeleted);
            emitter.on('variableDeleted', this.variableDeleted);
            emitter.on('groupDeleted', this.groupDeleted);
            emitter.on('rosterDeleted', this.rosterDeleted);

            emitter.on('groupMoved', this.groupMoved);
            emitter.on('questionMoved', this.questionMoved);
            emitter.on('staticTextMoved', this.staticTextMoved);
            emitter.on('variableMoved', this.variableMoved);

            emitter.on('questionAdded', this.questionAdded);
            emitter.on('groupAdded', this.groupAdded);
            emitter.on('rosterAdded', this.rosterAdded);
            emitter.on('staticTextAdded', this.staticTextAdded);
            emitter.on('variableAdded', this.variableAdded);

            emitter.on('itemPasted', this.itemPasted);
        },

        async fetchTree(questionnaireId, chapterId) {
            const info = await get(
                '/api/questionnaire/chapter/' + questionnaireId,
                {
                    chapterId: chapterId
                }
            );
            this.setChapterInfo(info);
            this.questionnaireId = questionnaireId;
            this.chapterId = chapterId;
        },

        setChapterInfo(info) {
            this.info = info;
            this.recalculateVariableNames();
        },

        questionAdded(event) {
            const question = cloneDeep(event.question);
            if (event.index == null) {
                event.parent.items.push(question);
            } else {
                event.parent.items.splice(event.index, 0, question);
            }
        },

        groupAdded(event) {
            const group = cloneDeep(event.group);
            if (event.index == null) {
                event.parent.items.push(group);
            } else {
                event.parent.items.splice(event.index, 0, group);
            }
        },

        rosterAdded(event) {
            const roster = cloneDeep(event.roster);
            if (event.index == null) {
                event.parent.items.push(roster);
            } else {
                event.parent.items.splice(event.index, 0, roster);
            }
        },

        staticTextAdded(event) {
            const staticText = cloneDeep(event.staticText);
            if (event.index == null) {
                event.parent.items.push(staticText);
            } else {
                event.parent.items.splice(event.index, 0, staticText);
            }
        },

        variableAdded(event) {
            const variable = cloneDeep(event.variable);
            if (event.index == null) {
                event.parent.items.push(variable);
            } else {
                event.parent.items.splice(event.index, 0, variable);
            }
        },

        getItemIndexByIdFromParentItemsList(parent, id) {
            if (!parent || !id) return null;

            var index = findIndex(parent.items, function(i) {
                return i.itemId === id;
            });

            return index < 0 ? null : index;
        },

        questionUpdated(data) {
            const itemId = data.id.replaceAll('-', '');
            var question = this.findTreeItem(itemId);
            if (isNull(question) || isUndefined(question)) return;

            question.title = data.title;
            question.variable = data.variableName;
            question.type = data.type;
            question.hasValidation = data.validationConditions.length > 0;
            question.hasCondition =
                data.enablementCondition !== null &&
                /\S/.test(data.enablementCondition);
            question.linkedToEntityId = data.linkedToEntityId;
            question.isInteger = data.isInteger;
            question.yesNoView = data.yesNoView;
            question.hideIfDisabled = data.hideIfDisabled;
            question.isCritical = data.isCritical;

            const varType = this.getUnderlyingQuestionType(question);
            this.updateVariableName(itemId, data.variableName, varType);
        },

        getUnderlyingQuestionType(question) {
            switch (question.type) {
                case 'Text':
                case 'Multimedia':
                case 'QRBarcode':
                    return 'string';
                case 'TextList':
                    return 'TextList'; // real type is TextListAnswerRow[]
                case 'DateTime':
                    return 'DateTime?';
                case 'GpsCoordinates':
                    return 'GeoLocation';
                case 'MultyOption':
                    if (question.yesNoView) {
                        return 'YesNoAnswers'; // real type is YesNoAndAnswersMissings
                    }
                    if (
                        isNotLinkedOrLinkedToTextList(question.linkedToEntityId)
                    ) {
                        return 'int[]';
                    }
                    return 'RosterVector[]';
                case 'SingleOption':
                    if (
                        isNotLinkedOrLinkedToTextList(question.linkedToEntityId)
                    ) {
                        return 'int?';
                    }

                    return 'RosterVector';
                case 'Numeric':
                    if (question.isInteger) {
                        return 'int?';
                    }

                    return 'double?';
            }

            return null;
        },

        staticTextUpdated(event) {
            const itemId = event.id.replaceAll('-', '');
            var staticText = this.findTreeItem(itemId);
            if (isNull(staticText) || isUndefined(staticText)) return;
            staticText.text = event.text;
            staticText.attachmentName = event.attachmentName;

            staticText.hasValidation = event.validationConditions.length > 0;
            staticText.hasCondition =
                event.enablementCondition !== null &&
                /\S/.test(event.enablementCondition);
            staticText.hideIfDisabled = event.hideIfDisabled;
        },

        variableUpdated(data) {
            var variable = this.findTreeItem(data.id);
            if (isNull(variable) || isUndefined(variable)) return;
            variable.variableData.name = data.variable;
            variable.variableData.label = data.label;
            variable.variableData.type = data.type;

            const varType = this.getUnderlyingVariableType(variable);
            this.updateVariableName(data.id, data.variable, varType);
        },

        getUnderlyingVariableType(variable) {
            switch (variable.variableData.type) {
                case 'LongInteger':
                    return 'long?';
                case 'String':
                    return 'string';
                case 'DateTime':
                    return 'DateTime?';
                case 'Double':
                    return 'double?';
                case 'Boolean':
                    return 'bool?';
            }

            return null;
        },

        groupUpdated(payload) {
            const itemId = payload.group.id.replaceAll('-', '');
            const hasCondition =
                payload.group.enablementCondition !== null &&
                /\S/.test(payload.group.enablementCondition);

            this.updateVariableName(
                itemId,
                payload.group.variableName,
                'Roster'
            );

            const chapter = this.getChapterData;
            if (chapter.itemId === itemId) {
                chapter.title = payload.group.title;
                chapter.hasCondition = hasCondition;
                chapter.hideIfDisabled = payload.group.hideIfDisabled;
            }

            var group = this.findTreeItem(itemId);
            if (isNull(group) || isUndefined(group)) return;
            group.title = payload.group.title;
            group.variable = payload.group.variableName;
            group.hasCondition = hasCondition;
            group.hideIfDisabled = payload.group.hideIfDisabled;
        },

        rosterUpdated(data) {
            const hasCondition =
                data.roster.enablementCondition !== null &&
                /\S/.test(data.roster.enablementCondition);

            const itemId = data.roster.itemId.replaceAll('-', '');
            var roster = this.findTreeItem(itemId);
            if (isNull(roster) || isUndefined(roster)) return;
            roster.title = data.roster.title;
            roster.variable = data.roster.variableName;
            roster.hasCondition = hasCondition;
            roster.hideIfDisabled = data.roster.hideIfDisabled;

            this.updateVariableName(itemId, data.roster.variableName, 'Roster');
        },

        updateVariableName(id, newName, type) {
            const trimId = id.replaceAll('-', '');
            var index = findIndex(this.info.variableNames, function(i) {
                return i.id === trimId;
            });
            if (index > -1) {
                this.info.variableNames[index].name = newName;
                this.info.variableNames[index].type = type;
            } else {
                this.info.variableNames.push({
                    id: trimId,
                    name: newName,
                    type: type
                });
            }
            this.recalculateVariableNames();
        },
        removeVariableName(id) {
            var index = findIndex(this.info.variableNames, function(i) {
                return i.id === id.replaceAll('-', '');
            });
            if (index > -1) {
                this.info.variableNames.splice(index, 1);
                this.recalculateVariableNames();
            }
        },

        recalculateVariableNames() {
            var i = 0;
            this.variableNamesStore.variableNamesCompletions = orderBy(
                this.info.variableNames,
                'name',
                'desc'
            ).map(function(variable) {
                return {
                    name: variable.name,
                    value: variable.name,
                    score: i++,
                    meta: variable.type
                };
            });

            this.variableNamesStore.variableNamesTokens = this.info.variableNames
                .map(function(el) {
                    return el.name;
                })
                .join('|');

            this.variableNamesStore.lastUpdated = new Date();

            emitter.emit('variablesRecalculated');
        },

        findTreeItem(itemId) {
            var o;
            const items = this.getItems;
            items.some(function iter(a) {
                if (a.itemId === itemId) {
                    o = a;
                    return true;
                }
                return Array.isArray(a.items) && a.items.some(iter);
            });
            return o;
        },

        questionDeleted(data) {
            this.deleteTreeNode(data.id);
            this.removeVariableName(data.id);
        },
        staticTextDeleted(data) {
            this.deleteTreeNode(data.id);
        },
        variableDeleted(data) {
            this.deleteTreeNode(data.id);
            this.removeVariableName(data.id);
        },
        groupDeleted(data) {
            this.deleteTreeNode(data.id);
            this.removeVariableName(data.id);
        },
        rosterDeleted(data) {
            this.deleteTreeNode(data.id);
            this.removeVariableName(data.id);
        },
        deleteTreeNode(itemId) {
            const id = itemId.replaceAll('-', '');
            var parent = this.findTreeItemParent(id);
            if (isNull(parent) || isUndefined(parent)) return;

            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                itemId
            );
            parent.items.splice(index, 1);
        },

        findTreeItemParent(value) {
            const chapter = this.getChapterData;
            return this.findParent(chapter, value);
        },

        findParent(parent, value) {
            if (!parent.items) {
                return;
            }

            for (const item of parent.items) {
                if (item.itemId === value) {
                    return parent;
                }

                const find = this.findParent(item.items, value);
                if (find) {
                    return find;
                }
            }
        },

        clear() {
            this.info = {};
        },

        questionMoved(event) {
            this.treeItemMoved(event.itemId, event.newParentId, event.newIndex);
        },
        staticTextMoved(event) {
            this.treeItemMoved(event.itemId, event.newParentId, event.newIndex);
        },
        variableMoved(event) {
            this.treeItemMoved(event.itemId, event.newParentId, event.newIndex);
        },
        groupMoved(event) {
            this.treeItemMoved(event.itemId, event.newParentId, event.newIndex);
        },
        treeItemMoved(itemId, newParentId, newIndex) {
            const id = itemId.replaceAll('-', '');
            var treeItem = this.findTreeItem(id);
            if (isNull(treeItem) || isUndefined(treeItem)) {
                return;
            }

            var parent = this.findTreeItemParent(id);
            if (isNull(parent) || isUndefined(parent)) {
                return;
            }

            const index = this.getItemIndexByIdFromParentItemsList(
                parent,
                itemId
            );
            parent.items.splice(index, 1);

            if (newParentId == this.chapterId) {
                this.info.chapter.items.splice(newIndex, 0, treeItem);
                return;
            }

            var newParent = this.findTreeItem(newParentId);
            if (isNull(newParent) || isUndefined(newParent)) {
                return;
            }

            newParent.items.splice(newIndex, 0, treeItem);
        },

        itemPasted(event) {
            this.fetchTree(this.questionnaireId, this.chapterId);
        }
    }
});
