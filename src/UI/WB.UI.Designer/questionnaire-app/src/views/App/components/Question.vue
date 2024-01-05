<template>
    <form role="form" method="POST" id="question-editor" name="questionForm" unsaved-warning-form v-show="activeQuestion">
        <div id="show-reload-details-promt" class="ng-cloak" v-show="shouldUserSeeReloadDetailsPromt">
            <div class="inner">{{ $t('QuestionnaireEditor.QuestionToUpdateOptions') }} <a @click="fetch()"
                    href="javascript:void(0);" v-t="{ path: 'QuestionnaireEditor.QuestionClickReload' }"></a></div>
        </div>
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="activeQuestion.breadcrumbs">
            </Breadcrumbs>

            <div class="row">
                <div class="form-group col-xs-6">
                    <label class="wb-label" v-t="{ path: 'QuestionnaireEditor.QuestionQuestionType' }"></label><br />
                    <div class="btn-group type-container-dropdown">
                        <button id="questionTypeBtn" class="btn btn-default form-control dropdown-toggle" type="button"
                            data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="icon" :class="[getAnswerTypeClass(activeQuestion.type)]"></i>
                            <span class="vertical-line"></span>
                            {{ activeQuestionType }}
                            <span class="dropdown-arrow"></span>
                        </button>

                        <ul class="dropdown-menu " role="menu" aria-labelledby="questionTypeBtn">
                            <li role="presentation" v-for="qtype in activeQuestion.questionTypeOptions">
                                <a role="menuitem" tabindex="-1" @click="setQuestionType(qtype.value)">
                                    <i class="icon" :class="[getAnswerTypeClass(qtype.value)]"></i>
                                    {{ qtype.text }}
                                </a>
                            </li>
                        </ul>
                    </div>
                    <input type="hidden" v-model="activeQuestion.type" required />
                </div>

                <div class="form-group input-variable-name col-xs-5 pull-right">
                    <label for="edit-question-variable-name" class="wb-label">{{
                        $t('QuestionnaireEditor.QuestionVariableName') }}
                        <help link="variableName" placement="left" />
                    </label><br />
                    <input id="edit-question-variable-name" type="text" v-model="activeQuestion.variableName"
                        spellcheck="false" class="form-control" maxlength="32">
                </div>
            </div>
            <div class="form-group"
                v-show="activeQuestion.type == 'GpsCoordinates' && activeQuestion.questionScope == 'Identifying'">
                <span class="edit-question-note" v-t="{ path: 'QuestionnaireEditor.QuestionGpsNavigation' }">
                </span>
            </div>
            <div class="form-group">
                <label for="edit-variable-label" class="wb-label">{{ $t('QuestionnaireEditor.QuestionVariableLabel') }}
                    <help link="variableLabel" />
                </label><br>
                <input id="edit-variable-label" type="text" v-model="activeQuestion.variableLabel" class="form-control"
                    maxlength="80">
            </div>

            <div class="form-group">
                <label for="edit-question-title-highlight" class="wb-label"
                    v-t="{ path: 'QuestionnaireEditor.QuestionText' }"></label><br>
                <div class="pseudo-form-control">
                    <!--div id="edit-question-title-highlight"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        v-model="activeQuestion.title"></div-->
                    <ExpressionEditor v-model="activeQuestion.title"></ExpressionEditor>
                </div>
                <div class="question-type-specific-block" v-if="activeQuestion.type != undefined
                    && (activeQuestion.type != 'GpsCoordinates'
                        && activeQuestion.type != 'QRBarcode'
                        && activeQuestion.type != 'Audio')">

                    <component ref="questionSpecific" :key="activeQuestion.id" :is="questionTemplate(activeQuestion.type)"
                        :activeQuestion="activeQuestion" :questionnaireId="questionnaireId">
                    </component>

                </div>
            </div>
            <div class="form-group"
                v-show="!((showInstruction === null && activeQuestion.instructions) || showInstruction)">
                <button type="button" class="btn btn-lg btn-link" @click="showInstruction = true"
                    v-t="{ path: 'QuestionnaireEditor.QuestionAddInstruction' }"></button>
            </div>

            <div class="row" v-show="(showInstruction === null && activeQuestion.instructions) || showInstruction">
                <div class="form-group col-xs-11">
                    <label for="edit-question-instructions"> {{ $t('QuestionnaireEditor.QuestionInstruction') }}
                        <help link="instruction" />
                    </label>

                    <input id="cb-hide-instructions" type="checkbox" class="wb-checkbox"
                        v-model="activeQuestion.hideInstructions" />
                    <label for="cb-hide-instructions"><span></span>{{ $t('QuestionnaireEditor.QuestionHideInstruction') }}
                        <help link="hideInstructions" />
                    </label>

                    <div class="pseudo-form-control">
                        <!--div id="edit-question-instructions"
                            ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                            v-model="activeQuestion.instructions"></div-->
                        <ExpressionEditor v-model="activeQuestion.instructions"></ExpressionEditor>
                    </div>
                </div>

                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showInstruction = false; activeQuestion.instructions = ''; activeQuestion.hideInstructions = false; setDirty();"></button>
                </div>
            </div>

            <div class="form-group"
                v-show="(doesQuestionSupportEnablementConditions() && !((showEnablingConditions === null && activeQuestion.enablementCondition) || showEnablingConditions))">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true"
                    v-t="{ path: 'QuestionnaireEditor.AddEnablingCondition' }"></button>
            </div>

            <div class="row"
                v-show="(doesQuestionSupportEnablementConditions() && ((showEnablingConditions === null && activeQuestion.enablementCondition) || showEnablingConditions))">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': activeQuestion.hideIfDisabled }"></div>
                    <label for="edit-question-enablement-condition">{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help link="conditionExpression" />
                    </label>

                    <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
                        v-if="questionnaire.hideIfDisabled" :title="$t('QuestionnaireEditor.HideIfDisabledNested')" />

                    <input id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox"
                        v-model="activeQuestion.hideIfDisabled" v-if="!questionnaire.hideIfDisabled" />
                    <label for="cb-hideIfDisabled"><span
                            :title="questionnaire.hideIfDisabled ? $t('QuestionnaireEditor.HideIfDisabledNested') : ''"></span>{{
                                $t('QuestionnaireEditor.HideIfDisabled') }}
                        <help link="hideIfDisabled" />
                    </label>

                    <div class="pseudo-form-control">
                        <!--div id="edit-question-enablement-condition"
                            ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
                            v-model="activeQuestion.enablementCondition"></div-->
                        <ExpressionEditor v-model="activeQuestion.enablementCondition"></ExpressionEditor>
                    </div>
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeQuestion.enablementCondition = ''; activeQuestion.hideIfDisabled = false; dirty = false;"></button>
                </div>
            </div>
            <div class="form-group validation-group" v-if="doesQuestionSupportValidations()"
                v-for="(validation, index) in activeQuestion.validationConditions" :id="'validationCondition' + index">
                <div class="validation-group-marker"></div>
                <label>{{ $t('QuestionnaireEditor.ValidationCondition') }} {{ index + 1 }}
                    <help link="validationExpression" />
                </label>

                <input :id="'cb-isWarning' + index" type="checkbox" class="wb-checkbox" v-model="validation.severity"
                    :true-value="'Warning'" :false-value="'Error'" />
                <label :for="'cb-isWarning' + index"><span></span>{{ $t('QuestionnaireEditor.IsWarning') }}</label>

                <button type="button" class="btn delete-btn-sm delete-validation-condition"
                    @click="removeValidationCondition(index)" tabindex="-1"></button>

                <div class="pseudo-form-control">
                    <!--div ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
                        ng-attr-id="{{'validation-expression-' + $index}}" v-model="validation.expression"
                        ng-attr-tabindex="{{$index + 1}}"></div-->
                    <ExpressionEditor v-model="validation.expression"></ExpressionEditor>
                </div>

                <label for="validationMessage{{$index}}" class="validation-message">{{
                    $t('QuestionnaireEditor.ErrorMessage') }}
                    <help link="validationMessage" />
                </label>
                <div class="pseudo-form-control">
                    <!--div ng-attr-id="{{'validation-message-' + $index}}" ng-attr-tabindex="{{$index + 1}}"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        v-model="validation.message"></div-->
                    <ExpressionEditor v-model="validation.message"></ExpressionEditor>
                </div>
            </div>
            <div class="form-group"
                v-if="doesQuestionSupportValidations() && activeQuestion.validationConditions.length < 10">
                <button type="button" class="btn btn-lg btn-link" @click="addValidationCondition()"
                    v-t="{ path: 'QuestionnaireEditor.AddValidationRule' }"></button>
            </div>

            <div class="form-group" v-if="doesQuestionSupportQuestionScope()">
                <div class="form-group pull-right">
                    <label for="Question-scope">{{ $t('QuestionnaireEditor.QuestionScope') }}&nbsp;</label>

                    <div class="btn-group dropup type-container-dropdown">
                        <button class="btn dropdown-toggle" id="Question-scope" type="button" data-bs-toggle="dropdown"
                            aria-expanded="false">
                            {{ $t(currentQuestionScope) }}
                            <span class="dropdown-arrow"></span>
                        </button>

                        <ul class="dropdown-menu dropdown-menu-end dropdown-menu-right" role="menu"
                            aria-labelledby="dropdownMenu1">
                            <li role="presentation" v-for="scope in getQuestionScopes(activeQuestion)">
                                <a role="menuitem" class="dropdown-item" tabindex="-1" @click="changeQuestionScope(scope)">
                                    {{ scope.text }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>


        </div>
        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="edit-chapter-save-button" class="btn btn-lg " :class="{ 'btn-primary': dirty }"
                    unsaved-warning-clear @click="saveQuestion()" :disabled="!valid">{{
                        $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" v-show="currentChapter.isReadOnly && currentChapter.isCover" id="jump-to-button"
                    class="btn btn-lg btn-link" unsaved-warning-clear
                    ui-sref="questionnaire.chapter.question({itemId: activeQuestion.itemId, chapterId: activeQuestion.chapterId })">{{
                        $t('QuestionnaireEditor.JumpToEdit') }}</button>
                <button type="reset" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
                    @click="cancel()">{{ $t('QuestionnaireEditor.Cancel') }}</button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="add-comment-button" class="btn btn-lg btn-link" @click="toggleComments(activeQuestion)"
                    unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}</span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">{{
                        $t('QuestionnaireEditor.EditorShowComments',
                            {
                                count: commentsCount
                            }) }}</span>
                    <span v-show="isCommentsBlockVisible">{{ $t('QuestionnaireEditor.EditorHideComment') }}</span>
                </button>
                <button type="button" v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly"
                    id="edit-chapter-delete-button" class="btn btn-lg btn-link" @click="deleteQuestion(activeQuestion)"
                    unsaved-warning-clear>{{
                        $t('QuestionnaireEditor.Delete') }}</button>
                <MoveToChapterSnippet :item-id="questionId"
                    v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly">
                </MoveToChapterSnippet>
            </div>
        </div>
    </form>
</template>

<script>
import { useQuestionStore } from '../../../stores/question';
import { useCommentsStore } from '../../../stores/comments';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'
import { indexOf, find, filter, isEmpty, isNull } from 'lodash'
import { answerTypeClass, geometryInputModeOptions } from '../../../helpers/question'

import AreaQuestion from './parts/AreaQuestion.vue'
import DateTimeQuestion from './parts/DateTimeQuestion.vue'
import GpsCoordinatesQuestion from './parts/GpsCoordinatesQuestion.vue'
import MultimediaQuestion from './parts/MultimediaQuestion.vue'
import MultyOptionQuestion from './parts/MultyOptionQuestion.vue'
import NumericQuestion from './parts/NumericQuestion.vue'
import QRBarcodeQuestion from './parts/QRBarcodeQuestion.vue'
import SingleOptionQuestion from './parts/SingleOptionQuestion.vue'
import TextListQuestion from './parts/TextListQuestion.vue'
import TextQuestion from './parts/TextQuestion.vue'

const questionsWithOnlyInterviewerScope = ['Multimedia', 'Audio', 'Area', 'QRBarcode'];
const questionTypesDoesNotSupportValidations = ["Multimedia", "Audio"];

export default {
    name: 'Question',
    components: {
        MoveToChapterSnippet,
        ExpressionEditor,
        Breadcrumbs,
        Help,

        AreaQuestion,
        DateTimeQuestion,
        GpsCoordinatesQuestion,
        MultimediaQuestion,
        MultyOptionQuestion,
        NumericQuestion,
        QRBarcodeQuestion,
        SingleOptionQuestion,
        TextListQuestion,
        TextQuestion,
    },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        questionId: { type: String, required: true }
    },
    data() {
        return {
            activeQuestion: {
                breadcrumbs: [],
                instructions: [],
                questionTypeOptions: [],
                validationConditions: [],
                instructions: '',
            },
            shouldUserSeeReloadDetailsPromt: true,
            showInstruction: null,
            showEnablingConditions: null,
            dirty: false,
            valid: true
        }
    },
    watch: {
        activeQuestion: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.setDirty();
            },
            deep: true
        },

        async questionId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.questionStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const questionStore = useQuestionStore();
        const commentsStore = useCommentsStore();

        return {
            questionStore, commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        currentQuestionScope() {
            const option = this.activeQuestion.allQuestionScopeOptions.find(
                p => p.value == this.activeQuestion.questionScope
            );
            return option != null ? option.text : null;
        },

        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },

        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        },

        activeQuestionType() {
            const option = this.activeQuestion.questionTypeOptions.find(
                p => p.value == this.activeQuestion.type
            );
            return option != null ? option.text : null;
        }
    },
    methods: {
        async fetch() {
            await this.questionStore.fetchQuestionData(
                this.questionnaireId,
                this.questionId
            );

            this.activeQuestion = this.questionStore.getQuestion;
            this.shouldUserSeeReloadDetailsPromt = false;
        },

        async saveQuestion() {

            if (this.dirty == false) return;

            const beforeSave = this.$refs.questionSpecific.preperaToSave;
            if (beforeSave != undefined) {
                beforeSave();
            }

            if (!this.valid) return;

            const componentValid = this.$refs.questionSpecific.valid || true;
            if (!componentValid) return;

            this.questionStore.saveQuestionData();

            this.dirty = false;
        },

        cancel() {
            this.questionStore.discardChanges();
            this.activeQuestion = this.questionStore.getData;
            this.dirty = false;
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        deleteQuestion() {
            //
        },
        questionTemplate(questionType) {
            return questionType + 'Question';
        },
        doesQuestionSupportEnablementConditions() {
            return this.activeQuestion
                && (this.activeQuestion.questionScope != 'Identifying')
                && !(this.activeQuestion.isCascade && this.activeQuestion.cascadeFromQuestionId)
                && !this.activeQuestion.parentIsCover;
        },
        doesQuestionSupportValidations() {
            return this.activeQuestion &&
                indexOf(questionTypesDoesNotSupportValidations, this.activeQuestion.type) < 0;
        },
        doesQuestionSupportQuestionScope() {
            return this.activeQuestion &&
                this.activeQuestion.allQuestionScopeOptions &&
                this.activeQuestion.allQuestionScopeOptions.length > 0 &&
                this.questionnaire &&
                (!this.questionnaire.isCoverPageSupported || !this.activeQuestion.parentIsCover);
        },
        doesQuestionSupportOptionsFilters() {
            if (this.activeQuestion) {
                if (this.activeQuestion.type === 'MultyOption' || this.activeQuestion.type === 'SingleOption') {
                    return true;
                }
            }

            return false;
        },
        getAnswerTypeClass(type) {
            return answerTypeClass[type];
        },
        getQuestionScopes() {
            if (!this.activeQuestion)
                return [];
            var allScopes = this.activeQuestion.allQuestionScopeOptions;

            if (indexOf(questionsWithOnlyInterviewerScope, this.activeQuestion.type) >= 0) {
                return allScopes.filter(function (o) {
                    return o.value === 'Interviewer';
                });
            }

            if (!this.activeQuestion.isCascade && !this.activeQuestion.isLinked &&
                indexOf(['TextList', 'GpsCoordinates', 'MultyOption'], this.activeQuestion.type) < 0)
                return allScopes;

            return allScopes.filter(o => {
                if (this.activeQuestion.type === 'MultyOption')
                    return o.value !== 'Identifying';

                if (this.activeQuestion.type === 'GpsCoordinates')
                    return o.value !== 'Supervisor';

                if (this.activeQuestion.type === 'SingleOption')
                    return o.value !== 'Identifying';

                return o.value !== 'Identifying' && o.value !== 'Supervisor';
            });
        },
        setQuestionType(type) {
            this.activeQuestion.type = type;
            this.activeQuestion.typeName = find(this.activeQuestion.questionTypeOptions, { value: type }).text;
            //this.activeQuestion.allQuestionScopeOptions = dictionaries.allQuestionScopeOptions;

            const isQuestionScopeSupervisorOrPrefilled = this.activeQuestion.questionScope === 'Supervisor' || this.activeQuestion.questionScope === 'Identifying';
            if (type === 'TextList' && isQuestionScopeSupervisorOrPrefilled) {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'DateTime') {
                this.activeQuestion.allQuestionScopeOptions = filter(this.activeQuestion.allQuestionScopeOptions, function (val) {
                    return val.value !== 'Supervisor';
                });
                if (this.activeQuestion.questionScope === 'Supervisor') {
                    this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
                }
            }

            if (indexOf(questionsWithOnlyInterviewerScope, type) >= 0) {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'GpsCoordinates' && this.activeQuestion.questionScope === 'Supervisor') {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'MultyOption' && this.activeQuestion.questionScope === 'Identifying') {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type !== "SingleOption" && type !== "MultyOption") {
                this.activeQuestion.isLinked = !isEmpty(null);
            }

            if (type === 'MultyOption' || type === "SingleOption") {
                if (this.activeQuestion.options.length === 0) {
                    //this.addOption();
                }
            }

            if (!this.doesQuestionSupportOptionsFilters()) {
                this.activeQuestion.optionsFilterExpression = null;
                this.activeQuestion.linkedFilterExpression = null;
            }

            if (type === "Area") {
                if (this.activeQuestion.geometryType === null)
                    this.activeQuestion.geometryType = this.activeQuestion.geometryTypeOptions[0].value;

                if (this.activeQuestion.geometryInputMode === null)
                    this.activeQuestion.geometryInputMode = geometryInputModeOptions[0].value;
            }
            else {
                this.activeQuestion.geometryType = null;
                this.activeQuestion.geometryInputMode = null;
                this.activeQuestion.geometryOverlapDetection = null;
            }

            this.setDirty();
        },

        getQuestionScopeByValue(value) {
            return find(this.activeQuestion.allQuestionScopeOptions, { value: value });
        },

        changeQuestionScope(scope) {
            this.activeQuestion.questionScope = scope.value;
            if (this.activeQuestion.questionScope === 'Identifying') {
                this.activeQuestion.enablementCondition = '';
            }
            this.setDirty();
        },



        removeValidationCondition(index) {
            this.activeQuestion.validationConditions.splice(index, 1);
            this.setDirty();
        },

        addValidationCondition() {
            this.activeQuestion.validationConditions.push({
                expression: '',
                message: ''
            });
            this.setDirty();
            /*_.defer(function () {
                $(".question-editor .form-holder").scrollTo({ top: '+=200px', left: "+=0" }, 250);
            })*/
        },

        setDirty() {
            this.dirty = true;
        },

    }
}
</script>
