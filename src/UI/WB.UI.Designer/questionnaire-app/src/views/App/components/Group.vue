<template>
    <form role="form" id="roster-editor" name="groupForm" unsaved-warning-form v-if="activeGroup">
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="breadcrumbs">
            </Breadcrumbs>

            <div class="form-group">
                <label for="edit-group-title-highlight" class="wb-label">
                    {{ $t('QuestionnaireEditor.GroupTitle') }}
                </label>
                <br />
                <ExpressionEditor v-model="activeGroup.title"></ExpressionEditor>
            </div>

            <div class="row">
                <div class="form-group input-variable-name col-xs-12 pull-right">
                    <label for="edit-group-variableName" class="wb-label">
                        {{ $t('QuestionnaireEditor.SectionVariableName') }}
                        <help link="variableName" placement="left" />
                    </label>
                    <br />
                    <input id="edit-group-variableName" type="text" class="wb-input bg-white width-auto"
                        v-model="activeGroup.variableName" maxlength="32" spellcheck="false">
                </div>
            </div>

            <div class="form-group"
                v-if="!isCoverPage && !((showEnablingConditions === undefined && activeGroup.enablementCondition) || showEnablingConditions)">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true">
                    {{ $t('QuestionnaireEditor.AddEnablingCondition') }}
                </button>
            </div>

            <div class="row"
                v-if="!isCoverPage && ((showEnablingConditions === undefined && activeGroup.enablementCondition) || showEnablingConditions)">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': activeGroup.hideIfDisabled }"></div>
                    <label for="edit-group-condition">{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help link="conditionExpression" />
                    </label>

                    <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
                        v-if="questionnaire.hideIfDisabled" :title="$t('QuestionnaireEditor.HideIfDisabledNested')" />
                    <input v-if="!questionnaire.hideIfDisabled" id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox"
                        v-model="activeGroup.hideIfDisabled" />
                    <label for="cb-hideIfDisabled">
                        <span :title="questionnaire.hideIfDisabled ? $t('QuestionnaireEditor.HideIfDisabledNested') : ''">
                        </span>
                        {{ $t('QuestionnaireEditor.HideIfDisabled') }}
                        <help link="hideIfDisabled" />
                    </label>
                    <br>
                    <ExpressionEditor v-model="activeGroup.enablementCondition" mode="expression"></ExpressionEditor>
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeGroup.enablementCondition = ''; activeGroup.hideIfDisabled = false;"></button>
                </div>
            </div>

        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-if="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="edit-chapter-save-button" class="btn btn-lg" :class="{ 'btn-primary': isDirty }"
                    @click="saveGroup()" unsaved-warning-clear :disabled="!isDirty">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="button" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" @click="cancelGroup()"
                    unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="add-comment-button" class="btn btn-lg btn-link" @click="toggleComments(activeQuestion)"
                    unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}
                    </span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">{{
                        $t('QuestionnaireEditor.EditorShowComments', {
                            count: commentsCount
                        }) }}
                    </span>
                    <span v-show="isCommentsBlockVisible">
                        {{ $t('QuestionnaireEditor.EditorHideComment') }}
                    </span>
                </button>
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    v-if="!isCoverPage" id="edit-chapter-delete-button" class="btn btn-lg btn-link" @click="deleteGroup()"
                    unsaved-warning-clear>{{ $t('QuestionnaireEditor.Delete') }}</button>
                <MoveToChapterSnippet :item-id="groupId" :item-type="'Group'"
                    v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    v-if="!isChapter && !isCoverPage">
                </MoveToChapterSnippet>
            </div>
        </div>
    </form>
</template>

<script>
import { useGroupStore } from '../../../stores/group';
import { deleteGroup, updateGroup } from '../../../services/groupService';
import { useCommentsStore } from '../../../stores/comments';
import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService'
import { setFocusIn } from '../../../services/utilityService'

import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'
import { useMagicKeys } from '@vueuse/core';

export default {
    name: 'Group',
    components: { MoveToChapterSnippet, ExpressionEditor, Breadcrumbs, Help },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        groupId: { type: String, required: true }
    },
    data() {
        return {
            showEnablingConditions: undefined
        };
    },
    watch: {
        async groupId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.groupStore.clear();
                await this.fetch();
                this.scrollTo();
            }
        },
        ctrl_s: function (newValue) {
            if (newValue) {
                this.saveGroup();
            }
        }
    },
    setup() {
        const groupStore = useGroupStore();
        const commentsStore = useCommentsStore();

        commentsStore.registerEntityInfoProvider(function () {
            const initial = groupStore.getInitialGroup;

            return {
                title: initial.title,
                variable: initial.variableName,
                type: 'group'
            };
        });

        const { ctrl_s } = useMagicKeys({
            passive: false,
            onEventFired(e) {
                if (e.ctrlKey && e.key === 's' && e.type === 'keydown')
                    e.preventDefault()
            },
        });

        return {
            groupStore, commentsStore, ctrl_s
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.scrollTo();
    },
    computed: {
        activeGroup() {
            return this.groupStore.getGroup;
        },
        breadcrumbs() {
            return this.groupStore.getBreadcrumbs;
        },
        isDirty() {
            return this.groupStore.getIsDirty;
        },
        typeName() {
            if (!this.activeGroup.typeOptions) return null;

            const option = this.activeGroup.typeOptions.find(
                p => p.value == this.activeGroup.type
            );
            return option != null ? option.text : null;
        },
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        },
        isChapter() {
            if (this.currentChapter && this.currentChapter.chapter && this.activeGroup.id)
                return this.activeGroup.id.replaceAll('-', '') == this.currentChapter.chapter.itemId;
            return true;
        },
        isCoverPage() {
            return this.isChapter && this.currentChapter.isCover;
        }
    },
    methods: {
        async fetch() {
            await this.groupStore.fetchGroupData(this.questionnaireId, this.groupId);
            this.showEnablingConditions = this.activeGroup.enablementCondition ? true : false;
        },
        saveGroup() {
            if (this.questionnaire.isReadOnlyForUser) return;
            if (this.isDirty == false) return;

            updateGroup(this.questionnaireId, this.activeGroup);
        },
        cancelGroup() {
            this.groupStore.discardChanges();
            this.showEnablingConditions = this.activeGroup.enablementCondition ? true : false;
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        deleteGroup() {
            var itemIdToDelete = this.groupId;
            var questionnaireId = this.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.activeGroup.title ||
                this.$t('QuestionnaireEditor.UntitledGroupOrRoster')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteGroup(questionnaireId, itemIdToDelete).then(() => {
                        const chapterId = this.isChapter
                            ? this.questionnaire.chapters[0].itemId
                            : this.currentChapter.chapter.itemId;
                        this.$router.push({
                            name: 'group',
                            params: {
                                chapterId: chapterId,
                                entityId: chapterId
                            },
                            force: true
                        });
                    });
                }
            };

            this.$confirm(params);
        },

        scrollTo() {
            //const state = this.$route.state
            const state = window.history.state;
            const property = (state || {}).property;
            if (!property)
                return;

            var focusId = null;
            switch (property) {
                case 'Title':
                    focusId = 'edit-group-title';
                    break;
                case 'EnablingCondition':
                    focusId = 'edit-group-condition';
                    break;

                default:
                    break;
            }

            setFocusIn(focusId);
        },
    }
};
</script>
