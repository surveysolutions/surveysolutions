<template>
    <form role="form" id="question-editor" name="variableForm" unsaved-warning-form v-if="activeVariable">
        <div class="form-holder">
            <Breadcrumbs :breadcrumbs="activeVariable.breadcrumbs" />
            <div class="row">
                <div class="form-group col-xs-6">
                    <label class="wb-label">{{
                        $t('QuestionnaireEditor.VariableType')
                    }}</label><br />
                    <div class="btn-group type-container-dropdown">
                        <button id="variableTypeBtn" class="btn btn-default form-control dropdown-toggle"
                            data-bs-toggle="dropdown" aria-expanded="false" type="button">
                            {{ typeName }}
                            <span class="dropdown-arrow"></span>
                        </button>

                        <ul class="dropdown-menu" role="menu">
                            <li class="dropdown-item" role="presentation" v-for="typeOption in activeVariable.typeOptions">
                                <a role="menuitem" tabindex="-1" @click="activeVariable.type = typeOption.value">
                                    {{ typeOption.text }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                <div class="form-group input-variable-name col-xs-5 pull-right">
                    <label for="edit-question-variable-name" class="wb-label">{{
                        $t('QuestionnaireEditor.VariableVariableName') }}
                        <help link="variableName" placement="left" />
                    </label>
                    <br />
                    <input id="edit-question-variable-name" type="text" v-model="activeVariable.variable" spellcheck="false"
                        autocomplete="false" class="form-control" maxlength="32" />
                </div>
            </div>
            <div class="form-group">
                <label for="edit-variable-title-highlight" class="wb-label">{{ $t('QuestionnaireEditor.VariableLabel') }}
                    <help link="variableDescription" />
                </label>
                <ExpressionEditor id="edit-variable-title-highlight" v-model="activeVariable.label" />
            </div>
            <div class="form-group">
                <label for="edit-group-condition">{{ $t('QuestionnaireEditor.VariableExpression') }}
                    <help link="expression" />
                </label>
                <input id="cb-do-not-export" type="checkbox" class="wb-checkbox" v-model="activeVariable.doNotExport" />
                <label for="cb-do-not-export"><span></span>{{ $t('QuestionnaireEditor.VariableNoExport') }}</label>
                <help link="doNotExport"></help>
                <ExpressionEditor id="edit-group-condition" v-model="activeVariable.expression" mode="expression" />
            </div>
        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-if="!questionnaire.isReadOnlyForUser" id="edit-chapter-save-button"
                    class="btn btn-lg" :class="{ 'btn-primary': isDirty }" :disabled="!isDirty" @click="saveVariable()">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="button" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" @click="cancel()">
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-if="!questionnaire.isReadOnlyForUser" id="add-comment-button"
                    class="btn btn-lg btn-link" @click="toggleComments()">
                    <span v-if="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}</span>
                    <span v-if="!isCommentsBlockVisible && commentsCount > 0">{{
                        $t('QuestionnaireEditor.EditorShowComments', {
                            count: commentsCount
                        })
                    }}</span>
                    <span v-if="isCommentsBlockVisible">{{
                        $t('QuestionnaireEditor.EditorHideComment')
                    }}</span>
                </button>
                <button type="button" v-if="!questionnaire.isReadOnlyForUser" id="edit-chapter-delete-button"
                    class="btn btn-lg btn-link" @click="deleteVariable()" unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Delete') }}
                </button>
                <MoveToChapterSnippet :item-id="variableId" :item-type="'Variable'"
                    v-if="!questionnaire.isReadOnlyForUser" />
            </div>
        </div>
    </form>
</template>

<script>
import { useVariableStore } from '../../../stores/variable';
import { updateVariable, deleteVariable } from '../../../services/variableService';
import { useCommentsStore } from '../../../stores/comments';
import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService'
import { setFocusIn } from '../../../services/utilityService'

import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'
import { useMagicKeys } from '@vueuse/core';

export default {
    name: 'Variable',
    components: { MoveToChapterSnippet, ExpressionEditor, Breadcrumbs, Help },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        variableId: { type: String, required: true }
    },
    data() {
        return {};
    },
    watch: {
        async variableId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.variableStore.clear();
                await this.fetch();
                this.scrollTo();
            }
        },
        ctrl_s: function (value) {
            if (value)
                this.saveVariable();
        },
        $route: function (oldValue, newValue) {
            this.scrollTo();
        }
    },
    setup() {
        const variableStore = useVariableStore();
        const commentsStore = useCommentsStore();

        commentsStore.registerEntityInfoProvider(function () {
            const initial = variableStore.getInitialVariable;

            return {
                title: initial.label,
                variable: initial.variable,
                type: 'variable'
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
            variableStore, commentsStore, ctrl_s
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.scrollTo();
    },
    computed: {
        activeVariable() {
            return this.variableStore.getVariable;
        },
        isDirty() {
            return this.variableStore.getIsDirty;
        },
        typeName() {
            if (!this.activeVariable.typeOptions) return null;

            const option = this.activeVariable.typeOptions.find(
                p => p.value == this.activeVariable.type
            );
            return option != null ? option.text : null;
        },
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        }
    },
    methods: {
        async fetch() {
            await this.variableStore.fetchVarableData(this.questionnaireId, this.variableId);
        },
        saveVariable() {
            if (this.questionnaire.isReadOnlyForUser) return;
            if (!this.isDirty) return;
            updateVariable(this.questionnaireId, this.activeVariable);
        },
        cancel() {
            this.variableStore.discardChanges();
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        deleteVariable() {
            var itemIdToDelete = this.activeVariable.id;
            var questionnaireId = this.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.activeVariable.label || this.$t('QuestionnaireEditor.UntitledVariable')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteVariable(questionnaireId, itemIdToDelete).then(() => {
                        const chapterId = this.currentChapter.chapter.itemId;
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
            const state = window.history.state;
            const property = (state || {}).property;
            if (!property)
                return;

            var focusId = null;
            switch (property) {
                case 'VariableName':
                    focusId = 'edit-question-variable-name';
                    break;
                case 'VariableContent':
                    focusId = "edit-group-condition";
                    break;
                case 'VariableLabel':
                    focusId = "edit-variable-title-highlight";
                    break;
                default:
                    break;
            }

            setFocusIn(focusId);
        },
    }
};
</script>
