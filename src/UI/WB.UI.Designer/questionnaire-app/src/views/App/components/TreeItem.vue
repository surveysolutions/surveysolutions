<template>
    <div class="section item" :class="itemClass" @mouseenter="is_highlighted = true"
        @mouseleave="is_highlighted = false" v-contextmenu="'treeitem-context-menu-' + item.itemId">
        <span class="cursor"></span>
        <a class="handler angular-ui-tree-handle" ui-tree-handle></a>

        <slot />

        <Teleport to="body">
            <div class="dropdown position-fixed" :id="'treeitem-context-menu-' + item.itemId">
                <ul class="dropdown-menu" role="menu">
                    <li>
                        <a @click="addQuestion()" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly
        ">{{ isGroup()
        ? $t('QuestionnaireEditor.TreeAddQuestion')
        : $t('QuestionnaireEditor.TreeAddQuestionAfter')
                            }}</a>
                    </li>
                    <li>
                        <a @click="addGroup()" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly &&
        !currentChapter.isCover
        ">{{ isGroup()
        ? $t('QuestionnaireEditor.TreeAddSection')
        : $t('QuestionnaireEditor.TreeAddSectionAfter')
                            }}</a>
                    </li>
                    <li>
                        <a @click="addRoster()" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly &&
        !currentChapter.isCover
        ">{{ isGroup()
        ? $t('QuestionnaireEditor.TreeAddRoster')
        : $t('QuestionnaireEditor.TreeAddRosterAfter')
                            }}</a>
                    </li>
                    <li>
                        <a @click="addStaticText()" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly
        ">{{ isGroup()
        ? $t('QuestionnaireEditor.TreeAddStaticText')
        : $t('QuestionnaireEditor.TreeAddStaticTextAfter')
                            }}</a>
                    </li>
                    <li>
                        <a @click="addVariable()" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly
        ">{{ isGroup()
        ? $t('QuestionnaireEditor.TreeAddVariable')
        : $t('QuestionnaireEditor.TreeAddVariableAfter')
                            }}</a>
                    </li>
                    <li>
                        <a @click="copyItem()">{{
        $t('QuestionnaireEditor.Copy')
    }}</a>
                    </li>
                    <li>
                        <a @click="pasteItemAfter()" :disabled="readyToPaste ? null : true" v-if="!questionnaire.isReadOnlyForUser &&
        !currentChapter.isReadOnly
        ">{{ $t('QuestionnaireEditor.PasteAfter') }}</a>
                    </li>
                    <li>
                        <a @click="deleteItem()" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            ">{{ $t('QuestionnaireEditor.Delete') }}</a>
                    </li>
                </ul>
            </div>
        </Teleport>
    </div>
</template>

<script>
import { useQuestionStore } from '../../../stores/question';
import { deleteQuestion } from '../../../services/questionService';

import { useGroupStore } from '../../../stores/group';
import { deleteGroup } from '../../../services/groupService';

import { useStaticTextStore } from '../../../stores/staticText';
import { deleteStaticText } from '../../../services/staticTextService';

import { useVariableStore } from '../../../stores/variable';
import { deleteVariable } from '../../../services/variableService';
import { pasteItemAfter, canPaste, copyItem } from '../../../services/copyPasteService'

import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useTreeStore } from '../../../stores/tree';

import { useRosterStore } from '../../../stores/roster';
import { deleteRoster } from '../../../services/rosterService';

import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService';

import { addGroup } from '../../../services/groupService';
import { addQuestion } from '../../../services/questionService';
import { addRoster } from '../../../services/rosterService';
import { addStaticText } from '../../../services/staticTextService';
import { addVariable } from '../../../services/variableService';

export default {
    name: 'TreeItem',
    props: {
        questionnaireId: { type: String, required: true },
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        selectedItemId: { type: String, required: false }
    },
    data() {
        return {
            is_highlighted: false,
        };
    },
    setup(props) {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();
        const groupStore = useGroupStore();
        const questionStore = useQuestionStore();
        const rosterStore = useRosterStore();
        const staticTextStore = useStaticTextStore();
        const variableStore = useVariableStore();

        return {
            treeStore,
            questionnaireStore,
            groupStore,
            questionStore,
            rosterStore,
            staticTextStore,
            variableStore,
            canPaste,
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        },
        itemClass() {
            let classes = [];
            if (this.item.itemId === this.selectedItemId) {
                //classes.push('highlight');
                classes.push('selected');
            }
            if (this.is_highlighted) classes.push('highlighted');
            if (this.item.itemType)
                classes.push(this.item.itemType.toLowerCase());
            if (this.item.itemType === 'Group' && this.item.isRoster)
                classes.push('roster');
            if (this.stat.parent && this.stat.parent.data.isRoster)
                classes.push('roster-items');
            return classes;
        },
        readyToPaste() {
            return this.canPaste;
        }
    },
    methods: {
        async addQuestion() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            const question = await addQuestion(this.questionnaireId, parent, afterItemId);

            this.$router.push({
                name: 'question',
                params: {
                    entityId: question.itemId
                }
            });
        },
        async addGroup() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            const group = await addGroup(this.questionnaireId, parent, afterItemId);

            this.$router.push({
                name: 'group',
                params: {
                    entityId: group.itemId
                }
            });
        },
        async addRoster() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            const roster = await addRoster(this.questionnaireId, parent, afterItemId);

            this.$router.push({
                name: 'roster',
                params: {
                    entityId: roster.itemId
                }
            });
        },
        async addStaticText() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            const statictext = await addStaticText(this.questionnaireId, parent, afterItemId);

            this.$router.push({
                name: 'statictext',
                params: {
                    entityId: statictext.itemId
                }
            });
        },
        async addVariable() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            const variable = await addVariable(this.questionnaireId, parent, afterItemId);

            this.$router.push({
                name: 'variable',
                params: {
                    entityId: variable.itemId
                }
            });
        },
        getParentItem() {
            if (this.isGroup()) return this.item;
            if (this.stat.parent && this.stat.parent.data)
                return this.stat.parent.data;
            return this.currentChapter.chapter;
        },
        getParentStat() {
            if (this.isGroup()) return this.stat;
            if (this.stat.parent) return this.stat.parent;
            return null;
        },
        getAfterItemId() {
            if (this.isGroup()) return null;
            return this.item.itemId;
        },
        isGroup() {
            return this.item.itemType == 'Group';
        },

        deleteItem() {
            if (this.item.itemType == 'Question')
                this.deleteQuestion();
            else if (this.item.itemType == 'StaticText')
                this.deleteStaticText();
            else if (this.item.itemType == 'Variable')
                this.deleteVariable();
            else if (this.isGroup)
                this.deleteGroupOrRoster();
        },

        deleteQuestion() {
            var itemIdToDelete = this.item.itemId;
            var questionnaireId = this.questionnaire.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.title ||
                this.$t('QuestionnaireEditor.UntitledQuestion')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteQuestion(questionnaireId, itemIdToDelete);
                }
            };

            this.$confirm(params);
        },

        deleteStaticText() {
            var itemIdToDelete = this.item.itemId;
            var questionnaireId = this.questionnaire.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.text ||
                this.$t('QuestionnaireEditor.UntitledStaticText')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteStaticText(questionnaireId, itemIdToDelete);
                }
            };

            this.$confirm(params);
        },

        deleteVariable() {
            var itemIdToDelete = this.item.itemId;
            var questionnaireId = this.questionnaire.questionnaireId;

            var label = this.item.variableData
                ? this.item.variableData.label
                : this.item.label;

            const params = createQuestionForDeleteConfirmationPopup(
                label || this.$t('QuestionnaireEditor.UntitledVariable')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteVariable(questionnaireId, itemIdToDelete);
                }
            };

            this.$confirm(params);
        },

        deleteGroupOrRoster() {
            var itemIdToDelete = this.item.itemId;
            var questionnaireId = this.questionnaire.questionnaireId;
            var isRoster = this.item.isRoster;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.title ||
                this.$t('QuestionnaireEditor.UntitledGroupOrRoster')
            );

            params.callback = confirm => {
                if (confirm) {
                    isRoster
                        ? deleteRoster(questionnaireId, itemIdToDelete)
                        : deleteGroup(questionnaireId, itemIdToDelete);
                }
            };

            this.$confirm(params);
        },

        copyItem() {
            copyItem(this.questionnaireId, this.item);
        },

        async pasteItemAfter() {
            if (!this.canPaste) return;

            const result = await pasteItemAfter(this.questionnaireId, this.item.itemId)

            this.$router.push({
                name: result.itemType.toLowerCase(),
                params: {
                    entityId: result.id
                }
            });
        }
    }
};
</script>
