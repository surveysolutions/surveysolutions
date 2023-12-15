<template>
    <form role="form" id="question-editor" name="variableForm" unsaved-warning-form v-if="activeVariable">
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="activeVariable.breadcrumbs">
            </Breadcrumbs>

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
                                <a role="menuitem" tabindex="-1" @click="
                                    activeVariable.type = typeOption.value
                                    ">
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
                <div class="pseudo-form-control">
                    <!--div
                        id="edit-variable-title-highlight"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        ng-model="activeVariable.label"
                    ></div-->
                    <ExpressionEditor v-model="activeVariable.label"></ExpressionEditor>
                </div>
            </div>
            <div class="form-group">
                <label for="edit-group-condition">{{ $t('QuestionnaireEditor.VariableExpression') }}
                    <help link="expression" />
                </label>
                <input id="cb-do-not-export" type="checkbox" class="wb-checkbox" v-model="activeVariable.doNotExport" />
                <label for="cb-do-not-export"><span></span>{{ $t('QuestionnaireEditor.VariableNoExport') }}</label>
                <help link="doNotExport"></help>
                <div class="pseudo-form-control">
                    <div id="edit-group-condition" ui-ace="{ onLoad : aceLoaded , require: ['ace/ext/language_tools']}"
                        ng-model="activeVariable.expression"></div>
                    <ExpressionEditor v-model="activeVariable.expression"></ExpressionEditor>
                </div>
            </div>
        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="edit-chapter-save-button"
                    class="btn btn-lg" :class="{ 'btn-primary': dirty }" @click="saveVariable()" unsaved-warning-clear
                    :disabled="!valid">
                    {{ $t('QuestionnaireEditor.Save') }}
                </button>
                <button type="reset" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" @click="cancel()"
                    unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="add-comment-button"
                    class="btn btn-lg btn-link" @click="toggleComments()" unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}</span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">{{
                        $t('QuestionnaireEditor.EditorShowComments', {
                            count: commentsCount
                        })
                    }}</span>
                    <span v-show="isCommentsBlockVisible">{{
                        $t('QuestionnaireEditor.EditorHideComment')
                    }}</span>
                </button>
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="edit-chapter-delete-button"
                    class="btn btn-lg btn-link" @click="deleteVariable()" unsaved-warning-clear>
                    {{ $t('QuestionnaireEditor.Delete') }}
                </button>
                <MoveToChapterSnippet :item-id="variableId" v-show="!questionnaire.isReadOnlyForUser">
                </MoveToChapterSnippet>
            </div>
        </div>
    </form>
</template>

<script>
import { useVariableStore } from '../../../stores/variable';
import { useCommentsStore } from '../../../stores/comments';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'

export default {
    name: 'Variable',
    components: { MoveToChapterSnippet, ExpressionEditor, Breadcrumbs, Help },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        variableId: { type: String, required: true }
    },
    data() {
        return {
            activeVariable: null,
            dirty: false,
            valid: true
        };
    },
    watch: {
        activeVariable: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.dirty = true;
            },
            deep: true
        },

        async variableId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.variableStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const variableStore = useVariableStore();
        const commentsStore = useCommentsStore();

        return {
            variableStore, commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        typeName() {
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
            await this.variableStore.fetchVarableData(
                this.questionnaireId,
                this.variableId
            );

            this.activeVariable = this.variableStore.getData;
        },
        saveVariable() {
            this.variableStore.saveVariableData();
            this.dirty = false;
        },
        cancel() {
            this.variableStore.discardChanges();
            this.activeVariable = this.variableStore.getData;
            this.dirty = false;
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        deleteVariable() {
            //
        }
    }
};
</script>
