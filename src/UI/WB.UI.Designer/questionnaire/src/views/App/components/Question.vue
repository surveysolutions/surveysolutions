<template>
    <form role="form" method="POST" id="question-editor" name="questionForm" unsaved-warning-form
        v-show="activeQuestion">
        <div id="show-reload-details-promt" class="ng-cloak" v-show="shouldUserSeeReloadDetailsPromt">
            <div class="inner">{{ $t('QuestionnaireEditor.QuestionToUpdateOptions') }} <a @click="fetch()"
                    href="javascript:void(0);">{{ $t('QuestionnaireEditor.QuestionClickReload') }}</a></div>
        </div>
        <div class="form-holder">
            <Breadcrumbs :breadcrumbs="activeQuestion.breadcrumbs" />
            <div class="row">
                <div class="form-group col-xs-6">
                    <label class="wb-label">{{ $t('QuestionnaireEditor.QuestionQuestionType') }}</label><br />
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
                <span class="edit-question-note">
                    {{ $t('QuestionnaireEditor.QuestionGpsNavigation') }}
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
                <label for="edit-question-title-highlight" class="wb-label">{{ $t('QuestionnaireEditor.QuestionText')
                    }}</label><br>

                <ExpressionEditor id="edit-question-title-highlight" v-model="activeQuestion.title"></ExpressionEditor>

                <div class="question-type-specific-block" v-if="activeQuestion.type != undefined
                    && (activeQuestion.type != 'GpsCoordinates'
                        && activeQuestion.type != 'QRBarcode'
                        && activeQuestion.type != 'Audio')">

                    <component ref="questionSpecific" :key="activeQuestion.id"
                        :is="questionTemplate(activeQuestion.type)" :activeQuestion="activeQuestion"
                        :questionnaireId="questionnaireId">
                    </component>

                </div>
            </div>

            <div class="form-group"
                v-show="!((showInstruction === null && activeQuestion.instructions) || showInstruction)">
                <button type="button" class="btn btn-lg btn-link" @click="showInstruction = true">{{
                    $t('QuestionnaireEditor.QuestionAddInstruction') }}</button>
            </div>

            <div class="row" v-show="(showInstruction === null && activeQuestion.instructions) || showInstruction">
                <div class="form-group col-xs-11">
                    <label for="edit-question-instructions"> {{ $t('QuestionnaireEditor.QuestionInstruction') }}
                        <help link="instruction" />
                    </label>

                    <input id="cb-hide-instructions" type="checkbox" class="wb-checkbox"
                        v-model="activeQuestion.hideInstructions" />
                    <label for="cb-hide-instructions"><span></span>{{ $t('QuestionnaireEditor.QuestionHideInstruction')
                        }}
                        <help link="hideInstructions" />
                    </label>

                    <ExpressionEditor id="edit-question-instructions" v-model="activeQuestion.instructions" />
                </div>

                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showInstruction = false; activeQuestion.instructions = ''; activeQuestion.hideInstructions = false;"></button>
                </div>
            </div>

            <div class="form-group"
                v-show="(doesQuestionSupportEnablementConditions() && !((showEnablingConditions === null && activeQuestion.enablementCondition) || showEnablingConditions))">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true">{{
                    $t('QuestionnaireEditor.AddEnablingCondition') }}</button>
            </div>

            <div class="row"
                v-show="(doesQuestionSupportEnablementConditions() && ((showEnablingConditions === null && activeQuestion.enablementCondition) || showEnablingConditions))">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': activeQuestion.hideIfDisabled }">
                    </div>
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

                    <ExpressionEditor id="edit-question-enablement-condition"
                        v-model="activeQuestion.enablementCondition" mode="expression" />
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeQuestion.enablementCondition = ''; activeQuestion.hideIfDisabled = false;"></button>
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

                <ExpressionEditor :id="'validation-expression-' + index" mode="expression"
                    v-model="validation.expression" />

                <label for="validationMessage{{$index}}" class="validation-message">
                    {{ $t('QuestionnaireEditor.ErrorMessage') }}
                    <help link="validationMessage" />
                </label>
                <ExpressionEditor :id="'validation-message-' + index" v-model="validation.message" />
            </div>
            <div class="form-group"
                v-if="doesQuestionSupportValidations() && activeQuestion.validationConditions && activeQuestion.validationConditions.length < 10">
                <button type="button" class="btn btn-lg btn-link" @click="addValidationCondition()">{{
                    $t('QuestionnaireEditor.AddValidationRule') }}</button>
            </div>

            <div class="form-group">
                <div class="checkbox-in-column pull-left">
                    <div class="criticality-group-marker"></div>
                    <input id="cb-is-critical" type="checkbox" class="wb-checkbox"
                        v-model="activeQuestion.isCritical" />
                    <label for="cb-is-critical"><span></span>{{ $t('QuestionnaireEditor.QuestionIsCritical')
                        }} <help link="isCritical"></help></label>

                </div>
                <div class="form-group pull-right" v-if="doesQuestionSupportQuestionScope()">
                    <label for="Question-scope">{{ $t('QuestionnaireEditor.QuestionScope') }}&nbsp;</label>

                    <div class="btn-group dropup ">
                        <button class="btn dropdown-toggle" id="Question-scope" type="button" data-bs-toggle="dropdown"
                            aria-expanded="false">
                            {{ $t(currentQuestionScope) }}
                            <span class="dropdown-arrow"></span>
                        </button>

                        <ul class="dropdown-menu dropdown-menu-end dropdown-menu-right" role="menu"
                            aria-labelledby="dropdownMenu1">
                            <li role="presentation" v-for="scope in getQuestionScopes(activeQuestion)">
                                <a role="menuitem" class="dropdown-item" tabindex="-1"
                                    @click="changeQuestionScope(scope)">
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
                    id="edit-chapter-save-button" class="btn btn-lg " :class="{ 'btn-primary': isDirty }"
                    unsaved-warning-clear @click="saveQuestion()" :disabled="!isDirty || !isValid">{{
                        $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" v-show="currentChapter.isReadOnly && currentChapter.isCover" id="jump-to-button"
                    class="btn btn-lg btn-link" unsaved-warning-clear @click.stop="$router.push({
                        name: 'question',
                        params: {
                            chapterId: activeQuestion.chapterId,
                            entityId: activeQuestion.itemId,
                        }
                    })">{{ $t('QuestionnaireEditor.JumpToEdit') }}</button>
                <button type="button" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
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
                    id="edit-chapter-delete-button" class="btn btn-lg btn-link error" @click="deleteQuestion()"
                    unsaved-warning-clear>{{
                        $t('QuestionnaireEditor.Delete') }}</button>
                <MoveToChapterSnippet :item-id="questionId" :item-type="'Question'"
                    v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly" />

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
import _ from 'lodash'
import { deleteQuestion } from '../../../services/questionService';
import { answerTypeClass, geometryInputModeOptions, questionsWithOnlyInterviewerScope, questionTypesDoesNotSupportValidations } from '../../../helpers/question'
import { createQuestionForDeleteConfirmationPopup, scrollToValidationCondition, scrollToElement, setFocusIn } from '../../../services/utilityService'

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
import { useMagicKeys } from '@vueuse/core';
import emitter from '../../../services/emitter';

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
    provide() {
        return {
            openExternalEditor: this.openExternalEditor,
        };
    },
    data() {
        return {
            shouldUserSeeReloadDetailsPromt: false,
            openEditor: null,

            showInstruction: null,
            showEnablingConditions: null,
        }
    },
    watch: {
        async questionId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.questionStore.clear();
                await this.fetch();
                this.scrollTo();
            }
        },
        ctrl_s: function (v) {
            if (v)
                this.saveQuestion();
        },
        $route: function (oldValue, newValue) {
            this.scrollTo();
        },
        'activeQuestion.cascadeFromQuestionId'(newValue, oldValue) {
            if (this.activeQuestion) {
                if (newValue) {
                    this.activeQuestion.optionsFilterExpression = null;
                }
            }
        },
        'activeQuestion.linkedToEntityId'(newValue, oldValue) {
            if (oldValue == null && newValue != null && this.activeQuestion.questionScope == 'Identifying' && this.activeQuestion.type == 'SingleOption')
                this.activeQuestion.questionScope = null;
        },
    },
    setup() {
        const questionStore = useQuestionStore();
        const commentsStore = useCommentsStore();

        commentsStore.registerEntityInfoProvider(function () {
            const initial = questionStore.getInitialQuestion;

            return {
                title: initial.text,
                variable: initial.variableName,
                type: 'question'
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
            questionStore, commentsStore, ctrl_s
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.scrollTo();

        // https://developer.mozilla.org/en-US/docs/Web/API/Broadcast_Channel_API
        // Automatically reload window on popup close. If supported by browser
        if ('BroadcastChannel' in window) {
            this.bcChannel = new BroadcastChannel("editcategory")
            this.bcChannel.onmessage = ev => {
                console.log(ev.data)
                if (ev.data === 'close#' + this.openEditor) {
                    window.location.reload();
                }
            }
        }
    },
    computed: {
        currentQuestionScope() {
            const questionScope = this.activeQuestion.questionScope
            const option = this.activeQuestion.allQuestionScopeOptions.find(
                p => p.value == questionScope
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
            if (!this.activeQuestion.questionTypeOptions) return null;

            const option = _.find(this.activeQuestion.questionTypeOptions,
                p => p.value == this.activeQuestion.type
            );
            return option != null ? option.text : null;
        },
        activeQuestion() {
            return this.questionStore.getQuestion;
        },
        isDirty() {
            return this.questionStore.getIsDirty;
        },
        isValid() {
            return this.activeQuestion.questionScope != null && this.questionStore.getIsValid;
        }
    },
    methods: {
        async fetch() {
            await this.questionStore.fetchQuestionData(this.questionnaireId, this.questionId);
            this.shouldUserSeeReloadDetailsPromt = false;

            this.showInstruction = this.activeQuestion.instructions ? true : false;
            this.showEnablingConditions = this.activeQuestion.enablementCondition ? true : false;
        },

        async saveQuestion() {
            if (this.questionnaire.isReadOnlyForUser) return;
            if (this.isDirty == false || this.isValid !== true) return;


            if (this.$refs.questionSpecific != null) {
                const beforeSave = this.$refs.questionSpecific.prepareToSave;
                if (beforeSave != undefined) {
                    await beforeSave();
                }

                const componentValid = this.$refs.questionSpecific.valid || true;
                if (!componentValid) return;
            }

            await this.questionStore.saveQuestionData(this.questionnaireId, this.activeQuestion);
        },

        cancel() {
            this.questionStore.discardChanges();

            this.shouldUserSeeReloadDetailsPromt = false;
            this.showInstruction = this.activeQuestion.instructions ? true : false;
            this.showEnablingConditions = this.activeQuestion.enablementCondition ? true : false;

            emitter.emit('questionChangesDiscarded', this.activeQuestion);
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        deleteQuestion() {
            var itemIdToDelete = this.questionId;
            var questionnaireId = this.questionnaireId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.activeQuestion.title ||
                this.$t('QuestionnaireEditor.UntitledQuestion')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteQuestion(questionnaireId, itemIdToDelete).then(() => {
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
        questionTemplate(questionType) {
            return questionType + 'Question';
        },
        doesQuestionSupportEnablementConditions() {
            const isCover = this.currentChapter.isCover;
            if (isCover)
                return false;
            return this.activeQuestion
                && (this.activeQuestion.questionScope != 'Identifying')
                && this.activeQuestion.cascadeFromQuestionId == null;
        },
        doesQuestionSupportValidations() {
            return this.activeQuestion &&
                _.indexOf(questionTypesDoesNotSupportValidations, this.activeQuestion.type) < 0;
        },
        doesQuestionSupportQuestionScope() {
            return this.activeQuestion &&
                this.activeQuestion.allQuestionScopeOptions &&
                this.activeQuestion.allQuestionScopeOptions.length > 0 &&
                this.questionnaire &&
                (!this.questionnaire.isCoverPageSupported || !this.currentChapter.isCover);
        },
        doesQuestionSupportOptionsFilters() {
            if (this.activeQuestion) {
                if (this.activeQuestion.type === 'MultyOption' || this.activeQuestion.type === 'SingleOption') {
                    return true;
                }
            }

            return false;
        },
        doesQuestionSupportLinkedToEntity() {
            if (this.activeQuestion) {
                if (this.activeQuestion.type === 'MultyOption') {
                    return !this.activeQuestion.isFilteredCombobox && this.activeQuestion.yesNoView != true;
                }
                if (this.activeQuestion.type === 'SingleOption') {
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

            if (_.indexOf(questionsWithOnlyInterviewerScope, this.activeQuestion.type) >= 0) {
                return allScopes.filter(function (o) {
                    return o.value === 'Interviewer';
                });
            }

            if (this.activeQuestion.linkedToEntityId == null &&
                _.indexOf(['TextList', 'GpsCoordinates', 'MultyOption', 'DateTime'], this.activeQuestion.type) < 0)
                return allScopes;

            return allScopes.filter(o => {
                if (this.activeQuestion.type === 'MultyOption')
                    return o.value !== 'Identifying';

                if (this.activeQuestion.type === 'GpsCoordinates' || this.activeQuestion.type === 'DateTime')
                    return o.value !== 'Supervisor';

                if (this.activeQuestion.type === 'SingleOption')
                    return o.value !== 'Identifying';

                return o.value !== 'Identifying' && o.value !== 'Supervisor';
            });
        },
        setQuestionType(type) {
            this.activeQuestion.type = type;
            this.activeQuestion.typeName = _.find(this.activeQuestion.questionTypeOptions, { value: type }).text;

            const isQuestionScopeSupervisorOrPrefilled = this.activeQuestion.questionScope === 'Supervisor' || this.activeQuestion.questionScope === 'Identifying';
            if (type === 'TextList' && isQuestionScopeSupervisorOrPrefilled) {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'DateTime') {
                if (this.activeQuestion.questionScope === 'Supervisor') {
                    this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
                }
            }

            if (_.indexOf(questionsWithOnlyInterviewerScope, type) >= 0) {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'GpsCoordinates' && this.activeQuestion.questionScope === 'Supervisor') {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type === 'MultyOption' && this.activeQuestion.questionScope === 'Identifying') {
                this.changeQuestionScope(this.getQuestionScopeByValue('Interviewer'));
            }

            if (type !== "SingleOption" && type !== "MultyOption") {
                this.activeQuestion.linkedToEntityId = null;
                this.activeQuestion.linkedFilterExpression = null;
                this.activeQuestion.optionsFilterExpression = null;
            }

            if (type === 'MultyOption' || type === "SingleOption") {
                if (!this.activeQuestion.options || this.activeQuestion.options.length === 0) {
                    this.activeQuestion.options = [{ title: '', value: '', attachmentName: '' }];
                }
            }

            if (type === 'Numeric') {
                if (_.isNull(this.activeQuestion.isInteger) || _.isUndefined(this.activeQuestion.isInteger)) {
                    this.activeQuestion.isInteger = true;
                }

                if (!this.activeQuestion.options || this.activeQuestion.options.length === 0) {
                    this.activeQuestion.options = [{ title: '', value: '', attachmentName: '' }];
                }
            }

            if (!this.doesQuestionSupportOptionsFilters()) {
                this.activeQuestion.optionsFilterExpression = null;
                this.activeQuestion.linkedFilterExpression = null;
            }

            if (!this.doesQuestionSupportLinkedToEntity()) {
                this.activeQuestion.linkedToEntityId = null;
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
        },

        getQuestionScopeByValue(value) {
            return _.find(this.activeQuestion.allQuestionScopeOptions, { value: value });
        },

        changeQuestionScope(scope) {
            this.activeQuestion.questionScope = scope.value;
            if (this.activeQuestion.questionScope === 'Identifying') {
                this.activeQuestion.enablementCondition = '';
            }
        },

        removeValidationCondition(index) {
            this.activeQuestion.validationConditions.splice(index, 1);
        },

        addValidationCondition() {
            this.activeQuestion.validationConditions.push({
                expression: '',
                message: ''
            });
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
                    focusId = 'edit-question-title-highlight';
                    break;
                case 'VariableName':
                    focusId = 'edit-question-variable-name';
                    break;
                case 'EnablingCondition':
                    focusId = "edit-question-enablement-condition";
                    break;
                case 'ValidationExpression':
                    focusId = 'validation-expression-' + state.indexOfEntityInProperty;
                    break;
                case 'ValidationMessage':
                    focusId = 'validation-message-' + state.indexOfEntityInProperty;
                    break;
                case 'Option':
                    focusId = 'option-title-' + state.indexOfEntityInProperty;
                    break;
                case 'OptionsFilter':
                    focusId = 'optionsFilterExpression';
                    break;
                case 'Instructions':
                    focusId = 'edit-question-instructions';
                    break;
                default:
                    break;
            }

            if (!_.isNull((state || {}).indexOfEntityInProperty))
                scrollToValidationCondition(state.indexOfEntityInProperty);
            else {
                scrollToElement(".question-editor .form-holder", "#" + focusId);
            }

            setFocusIn(focusId);
        },

        openExternalEditor(id, url) {
            this.shouldUserSeeReloadDetailsPromt = true;
            this.openEditor = id

            window.open(url, "", "scrollbars=yes, center=yes, modal=yes, width=960, height=745, top=" + (screen.height - 745) / 4 + ", left= " + (screen.width - 960) / 2, true);
        },
    }
}
</script>
