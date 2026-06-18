<template>
    <Teleport to="body">
        <div v-if="store.visible" class="dropdown position-fixed open" id="treeitem-context-menu-shared"
            :style="{ top: store.y + 'px', left: store.x + 'px', display: 'block' }" ref="menuEl">
            <ul class="dropdown-menu" role="menu">
                <li>
                    <a @click="addQuestion()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly">{{
                            isGroup
                                ? $t('QuestionnaireEditor.TreeAddQuestion')
                                : $t('QuestionnaireEditor.TreeAddQuestionAfter')
                        }}</a>
                </li>
                <li>
                    <a @click="addGroup()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly &&
                        !store.currentChapter?.isCover">{{
                            isGroup
                                ? $t('QuestionnaireEditor.TreeAddSection')
                                : $t('QuestionnaireEditor.TreeAddSectionAfter')
                        }}</a>
                </li>
                <li>
                    <a @click="addRoster()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly &&
                        !store.currentChapter?.isCover">{{
                            isGroup
                                ? $t('QuestionnaireEditor.TreeAddRoster')
                                : $t('QuestionnaireEditor.TreeAddRosterAfter')
                        }}</a>
                </li>
                <li>
                    <a @click="addStaticText()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly">{{
                            isGroup
                                ? $t('QuestionnaireEditor.TreeAddStaticText')
                                : $t('QuestionnaireEditor.TreeAddStaticTextAfter')
                        }}</a>
                </li>
                <li>
                    <a @click="addVariable()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly">{{
                            isGroup
                                ? $t('QuestionnaireEditor.TreeAddVariable')
                                : $t('QuestionnaireEditor.TreeAddVariableAfter')
                        }}</a>
                </li>
                <li>
                    <a @click="copyItem()">{{ $t('QuestionnaireEditor.Copy') }}</a>
                </li>
                <li>
                    <a @click="pasteItemAfter()"
                        :aria-disabled="canPaste ? null : 'true'"
                        :class="{ disabled: !canPaste }"
                        :tabindex="canPaste ? null : -1"
                        v-if="!store.questionnaire?.isReadOnlyForUser && !store.currentChapter?.isReadOnly">
                        {{ $t('QuestionnaireEditor.PasteAfter') }}
                    </a>
                </li>
                <li>
                    <a @click="deleteItem()" v-if="!store.questionnaire?.isReadOnlyForUser &&
                        !store.currentChapter?.isReadOnly">
                        {{ $t('QuestionnaireEditor.Delete') }}
                    </a>
                </li>
            </ul>
        </div>
    </Teleport>
</template>

<script>
import { useTreeContextMenuStore } from '../../../stores/treeContextMenu';
import { deleteQuestion } from '../../../services/questionService';
import { deleteGroup } from '../../../services/groupService';
import { deleteStaticText } from '../../../services/staticTextService';
import { deleteVariable } from '../../../services/variableService';
import { pasteItemAfter, canPaste, copyItem } from '../../../services/copyPasteService';
import { deleteRoster } from '../../../services/rosterService';
import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService';
import { addGroup } from '../../../services/groupService';
import { addQuestion } from '../../../services/questionService';
import { addRoster } from '../../../services/rosterService';
import { addStaticText } from '../../../services/staticTextService';
import { addVariable } from '../../../services/variableService';

export default {
    name: 'TreeItemContextMenu',
    setup() {
        const store = useTreeContextMenuStore();
        return { store, canPaste };
    },
    computed: {
        isGroup() {
            return this.store.item?.itemType === 'Group';
        },
        parentItem() {
            if (this.isGroup) return this.store.item;
            if (this.store.stat?.parent?.data)
                return this.store.stat.parent.data;
            return this.store.currentChapter?.chapter;
        },
        afterItemId() {
            if (this.isGroup) return null;
            return this.store.item?.itemId ?? null;
        },
    },
    mounted() {
        document.addEventListener('click', this.onDocumentClick);
    },
    beforeUnmount() {
        document.removeEventListener('click', this.onDocumentClick);
    },
    methods: {
        onDocumentClick(event) {
            if (this.$refs.menuEl && !this.$refs.menuEl.contains(event.target)) {
                this.store.hide();
            }
        },
        async addQuestion() {
            this.store.hide();
            const question = await addQuestion(this.store.questionnaireId, this.parentItem, this.afterItemId);
            this.$router.push({ name: 'question', params: { entityId: question.itemId } });
        },
        async addGroup() {
            this.store.hide();
            const group = await addGroup(this.store.questionnaireId, this.parentItem, this.afterItemId);
            this.$router.push({ name: 'group', params: { entityId: group.itemId } });
        },
        async addRoster() {
            this.store.hide();
            const roster = await addRoster(this.store.questionnaireId, this.parentItem, this.afterItemId);
            this.$router.push({ name: 'roster', params: { entityId: roster.itemId } });
        },
        async addStaticText() {
            this.store.hide();
            const statictext = await addStaticText(this.store.questionnaireId, this.parentItem, this.afterItemId);
            this.$router.push({ name: 'statictext', params: { entityId: statictext.itemId } });
        },
        async addVariable() {
            this.store.hide();
            const variable = await addVariable(this.store.questionnaireId, this.parentItem, this.afterItemId);
            this.$router.push({ name: 'variable', params: { entityId: variable.itemId } });
        },
        copyItem() {
            this.store.hide();
            copyItem(this.store.questionnaireId, this.store.item);
        },
        async pasteItemAfter() {
            if (!this.canPaste) return;
            this.store.hide();
            const result = await pasteItemAfter(this.store.questionnaireId, this.store.item.itemId);
            this.$router.push({ name: result.itemType.toLowerCase(), params: { entityId: result.id } });
        },
        deleteItem() {
            this.store.hide();
            const item = this.store.item;
            if (item.itemType === 'Question') this.deleteQuestion(item);
            else if (item.itemType === 'StaticText') this.deleteStaticText(item);
            else if (item.itemType === 'Variable') this.deleteVariable(item);
            else this.deleteGroupOrRoster(item);
        },
        deleteQuestion(item) {
            const params = createQuestionForDeleteConfirmationPopup(
                item.title || this.$t('QuestionnaireEditor.UntitledQuestion')
            );
            params.callback = confirm => {
                if (confirm) deleteQuestion(this.store.questionnaireId, item.itemId);
            };
            this.$confirm(params);
        },
        deleteStaticText(item) {
            const params = createQuestionForDeleteConfirmationPopup(
                item.text || this.$t('QuestionnaireEditor.UntitledStaticText')
            );
            params.callback = confirm => {
                if (confirm) deleteStaticText(this.store.questionnaireId, item.itemId);
            };
            this.$confirm(params);
        },
        deleteVariable(item) {
            const label = item.variableData ? item.variableData.label : item.label;
            const params = createQuestionForDeleteConfirmationPopup(
                label || this.$t('QuestionnaireEditor.UntitledVariable')
            );
            params.callback = confirm => {
                if (confirm) deleteVariable(this.store.questionnaireId, item.itemId);
            };
            this.$confirm(params);
        },
        deleteGroupOrRoster(item) {
            const params = createQuestionForDeleteConfirmationPopup(
                item.title || this.$t('QuestionnaireEditor.UntitledGroupOrRoster')
            );
            params.callback = confirm => {
                if (confirm) {
                    item.isRoster
                        ? deleteRoster(this.store.questionnaireId, item.itemId)
                        : deleteGroup(this.store.questionnaireId, item.itemId);
                }
            };
            this.$confirm(params);
        },
    },
};
</script>
