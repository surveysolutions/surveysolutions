<template>
    <form role="form" id="roster-editor" name="editRosterForm" unsaved-warning-form v-show="activeRoster">
        <div class="form-holder">

            <Breadcrumbs :breadcrumbs="breadcrumbs">
            </Breadcrumbs>

            <div class="row">
                <div class="form-group col-xs-6">
                    <label for="edit-group-title" class="wb-label">
                        {{ $t('QuestionnaireEditor.RosterSource') }}
                    </label><br>
                    <div class="btn-group type-container-dropdown" uib-dropdown>
                        <button id="rosterTypeBtn" class="btn btn-default dropdown-toggle form-control" uib-dropdown-toggle
                            type="button" data-bs-toggle="dropdown" aria-expanded="false">
                            {{ typeName }}
                            <span class="dropdown-arrow"></span>
                        </button>
                        <ul class="dropdown-menu" role="menu" aria-labelledby="rosterTypeBtn">
                            <li role="presentation" v-for="typeOption in activeRoster.rosterTypeOptions">
                                <a role="menuitem" tabindex="-1" @click="activeRoster.type = typeOption.value">
                                    {{ typeOption.text }}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>

                <div class="form-group input-variable-name col-xs-5 pull-right">
                    <label for="edit-group-variableName" class="wb-label">{{ $t('QuestionnaireEditor.RosterVariableName') }}
                        <help link="variableName" placement="left" />
                    </label><br>
                    <input id="edit-group-variableName" type="text" class="wb-input bg-white width-auto"
                        v-model="activeRoster.variableName" maxlength="32" spellcheck="false">
                </div>
            </div>

            <div class="form-group">
                <label for="edit-roster-title-highlight" class="wb-label">
                    {{ $t('QuestionnaireEditor.RosterName') }}
                </label>

                <br />
                <div class="pseudo-form-control">
                    <!--div id="edit-roster-title-highlight"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        v-model="activeRoster.title"></div-->
                    <ExpressionEditor v-model="activeRoster.title"></ExpressionEditor>
                </div>
                <div class="roster-type-specific-block" v-if="activeRoster.type != undefined">


                    <div class="dropdown-with-breadcrumbs-and-icons" v-if="activeRoster.type == 'List'">
                        <label>{{ $t('QuestionnaireEditor.RosterSourceQuestion') }}</label>
                        <div class="btn-group" uib-dropdown>
                            <button class="btn dropdown-toggle" uib-dropdown-toggle type="button" data-bs-toggle="dropdown"
                                aria-expanded="false">
                                <span class="select-placeholder" v-if="selectedListQuestion == null">
                                    {{ $t('QuestionnaireEditor.SelectQuestion') }}
                                </span>
                                <span class="selected-item" v-if="selectedListQuestion !== null">
                                    <span class="path">{{ selectedListQuestion.breadcrumbs }}</span>
                                    <span class="chosen-item">
                                        <i class="dropdown-icon" :class="['icon-' + selectedListQuestion.type]"></i>
                                        {{ selectedListQuestion.title }}
                                        (<span class="var-name-line">{{ selectedListQuestion.varName }}</span>)
                                    </span>
                                </span>
                                <span class="dropdown-arrow"></span>
                            </button>

                            <ul class="dropdown-menu" role="menu">
                                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                                    v-for="breadCrumb in activeRoster.textListsQuestions">
                                    <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>

                                    <a v-if="!breadCrumb.isSectionPlaceHolder" @click="selectListQuestion(breadCrumb.id)"
                                        role="menuitem" tabindex="-1" href="javascript:void(0);">
                                        <div>
                                            <i :class="['dropdown-icon', 'icon-' + breadCrumb.type]"></i>
                                            <span v-dompurify-html="breadCrumb.title"></span>
                                        </div>
                                        <div class="var-block">
                                            <span class="var-name" v-dompurify-html="breadCrumb.varName"></span>
                                        </div>
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>




                    <div class="dropdown-with-breadcrumbs-and-icons" v-if="activeRoster.type == 'Numeric'">
                        <label>{{ $t('QuestionnaireEditor.RosterSourceNumericQuestion') }}</label>
                        <div class="btn-group" uib-dropdown>
                            <button class="btn dropdown-toggle" uib-dropdown-toggle type="button" data-bs-toggle="dropdown"
                                aria-expanded="false">
                                <span class="select-placeholder" v-if="selectedNumericQuestion == null">
                                    {{ $t('QuestionnaireEditor.SelectQuestion') }}
                                </span>
                                <span class="selected-item" v-if="selectedNumericQuestion !== null">
                                    <span class="path">{{ selectedNumericQuestion.breadcrumbs }}</span>
                                    <span class="chosen-item"><i class="dropdown-icon"
                                            :class="['icon-' + selectedNumericQuestion.type]"></i>
                                        {{ selectedNumericQuestion.title }}
                                        (<span class="var-name-line">{{ selectedNumericQuestion.varName }}</span>)
                                    </span>
                                </span>
                                <span class="dropdown-arrow"></span>
                            </button>

                            <ul class="dropdown-menu" role="menu">
                                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                                    v-for="breadCrumb in activeRoster.numericIntegerQuestions">
                                    <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>

                                    <a v-if="!breadCrumb.isSectionPlaceHolder" @click="selectNumericQuestion(breadCrumb.id)"
                                        role="menuitem" tabindex="-1" href="javascript:void(0);">
                                        <div>
                                            <i class="dropdown-icon icon-{{breadCrumb.type}}"></i>
                                            <span v-dompurify-html="breadCrumb.title"></span>
                                        </div>
                                        <div class="var-block">
                                            <span class="var-name" v-dompurify-html="breadCrumb.varName"></span>
                                        </div>
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>

                    <div class="dropdown-with-breadcrumbs-and-icons" v-if="activeRoster.type == 'Numeric'">
                        <label>{{ $t('QuestionnaireEditor.RosterSourceNumericTitles') }}</label>
                        <div class="btn-group" uib-dropdown>
                            <button class="btn dropdown-toggle" uib-dropdown-toggle type="button" data-bs-toggle="dropdown"
                                aria-expanded="false">
                                <span class="select-placeholder" v-if="selectedTitleQuestion == null">
                                    {{ $t('QuestionnaireEditor.SelectQuestion') }}
                                </span>
                                <span class="selected-item" v-if="selectedTitleQuestion !== null">
                                    <span class="path">{{ selectedTitleQuestion.breadcrumbs }}</span>
                                    <span class="chosen-item"><i class="dropdown-icon"
                                            :class="['icon-' + selectedTitleQuestion.type]"></i>
                                        {{ selectedTitleQuestion.title }}
                                        (<span class="var-name-line">{{ selectedTitleQuestion.varName }}</span>)
                                    </span>
                                </span>
                                <span class="dropdown-arrow"></span>
                            </button>

                            <ul class="dropdown-menu" role="menu">
                                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                                    v-for="breadCrumb in activeRoster.numericIntegerTitles">
                                    <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>

                                    <a v-if="!breadCrumb.isSectionPlaceHolder" @click="selectTitleQuestion(breadCrumb.id)"
                                        role="menuitem" tabindex="-1" href="javascript:void(0);">
                                        <div>
                                            <i class="dropdown-icon icon-{{breadCrumb.type}}"></i>
                                            <span v-dompurify-html="breadCrumb.title"></span>
                                        </div>
                                        <div class="var-block">
                                            <span class="var-name" v-dompurify-html="breadCrumb.varName"></span>
                                        </div>
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>



                    <div class="form-group" v-if="activeRoster.type == 'Fixed'">
                        <label for="edit-fixed-roster-title" class="wb-label">
                            {{ $t('QuestionnaireEditor.RosterSourceFixed') }}
                            <help link="fixedTitles" />
                        </label>
                        <!--<textarea id="edit-fixed-roster-title" class="form-control" ng-model="activeRoster.fixedRosterTitles" msd-elastic split-array></textarea>-->
                        <div class="options-editor">
                            <div v-if="useListAsRosterTitleEditor" class="form-group">
                                <div class="table-holder">
                                    <div class="table-row fixed-roster-titles-editor"
                                        v-for="(title, index) in activeRoster.fixedRosterTitles">
                                        <div class="column-2">
                                            <input type="number" min="-2147483648" max="2147483647"
                                                v-model.number="title.value" :name="'title_value_' + index"
                                                :class="{ 'has-error': title.invalid }"
                                                class="form-control fixed-roster-value-editor border-right">
                                        </div>
                                        <div class="column-3">
                                            <input :attr-id="'fixed-item-' + index" type="text" v-model="title.title"
                                                ng-keypress="onKeyPressInOptions($event)" class="form-control border-right">
                                        </div>
                                        <div class="column-4">
                                            <a href class="btn" tabindex="-1" @click="removeFixedTitle(index)"></a>
                                        </div>
                                    </div>
                                </div>
                                <p>
                                    <button class="btn btn-link" type="button"
                                        v-show="activeRoster.fixedRosterTitles.length < fixedRosterLimit"
                                        @click="addFixedTitle()">{{ $t('QuestionnaireEditor.RosterAddItem') }}</button>
                                    <button class="btn btn-link pull-right" @click="showOptionsInTextarea()">
                                        {{ $t('QuestionnaireEditor.StringsView') }}
                                    </button>
                                </p>
                            </div>
                            <div v-if="!useListAsRosterTitleEditor">
                                <div class="form-group" :class="{ 'has-error': !stringifiedRosterTitles.valid }">
                                    <textarea name="stringifiedRosterTitles" class="form-control mono"
                                        v-model="activeRoster.stringifiedRosterTitles" match-options-pattern
                                        v-on:keyup="stringifiedRosterTitlesValidate" max-options-count
                                        msd-elastic></textarea>
                                    <p class="help-block">
                                        <button class="btn btn-link" type="button" @click="showRosterTitlesInList()"
                                            :disabled="!stringifiedRosterTitles.valid"
                                            v-t="{ path: 'QuestionnaireEditor.ShowList' }" />
                                    </p>
                                    <p class="help-block ng-cloak"
                                        v-show="stringifiedRosterTitles.$error.matchOptionsPattern">
                                        {{ $t('QuestionnaireEditor.OptionsListError') }}
                                    </p>
                                    <p class="help-block ng-cloak" v-show="stringifiedRosterTitles.$error.maxOptionsCount">
                                        {{ $t('QuestionnaireEditor.EnteredMoreThanAllowed', {
                                            max: fixedRosterLimit
                                        }) }}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>



                    <div class="dropdown-with-breadcrumbs-and-icons" v-if="activeRoster.type == 'Multi'">
                        <label>{{ $t('QuestionnaireEditor.RosterSourceQuestion') }}</label>
                        <div class="btn-group" uib-dropdown>
                            <button class="btn dropdown-toggle" uib-dropdown-toggle type="button" data-bs-toggle="dropdown"
                                aria-expanded="false">
                                <span class="select-placeholder" v-if="selectedMultiQuestion == null">{{
                                    $t('QuestionnaireEditor.SelectQuestion') }}</span>
                                <span class="selected-item" v-if="selectedMultiQuestion !== null">
                                    <span class="path">{{ selectedMultiQuestion.breadcrumbs }}</span>
                                    <span class="chosen-item"><i class="dropdown-icon"
                                            :class="['icon-' + selectedMultiQuestion.type]"></i>{{
                                                selectedMultiQuestion.title
                                            }}
                                        (<span class="var-name-line">{{ selectedMultiQuestion.varName }}</span>)
                                    </span>
                                </span>
                                <span class="dropdown-arrow"></span>
                            </button>

                            <ul class="dropdown-menu" role="menu">
                                <li role="presentation" :class="{ 'dropdown-header': breadCrumb.isSectionPlaceHolder }"
                                    v-for="breadCrumb in activeRoster.notLinkedMultiOptionQuestions">
                                    <span v-if="breadCrumb.isSectionPlaceHolder">{{ breadCrumb.title }}</span>

                                    <a v-if="!breadCrumb.isSectionPlaceHolder" @click="selectMultiQuestion(breadCrumb.id)"
                                        role="menuitem" tabindex="-1" href="javascript:void(0);">
                                        <div>
                                            <i :class="['dropdown-icon', 'icon-' + breadCrumb.type]"></i>
                                            <span v-dompurify-html="breadCrumb.title"></span>
                                        </div>
                                        <div class="var-block">
                                            <span class="var-name" v-dompurify-html="breadCrumb.varName"></span>
                                        </div>
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>

                </div>
            </div>

            <div class="form-group">
                <label for="cb-roster-display-mode">{{ $t('QuestionnaireEditor.RosterDisplayMode') }}&nbsp;</label>

                <div class="btn-group dropup " uib-dropdown>
                    <button class="btn dropdown-toggle" id="cb-roster-display-mode" uib-dropdown-toggle type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ $t('QuestionnaireEditor.RosterDisplayMode_' + activeRoster.displayMode) }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu dropdown-menu-right" role="menu" aria-labelledby="cb-roster-display-mode">
                        <li role="presentation" v-for="displayMode in activeRoster.displayModes">
                            <a role="menuitem" tabindex="-1" @click="activeRoster.displayMode = displayMode">
                                {{ $t('QuestionnaireEditor.RosterDisplayMode_' + displayMode) }}
                            </a>
                        </li>
                    </ul>
                </div>

                <label>
                    <help link="rosterDisplayMode" />
                </label>
            </div>

            <div class="form-group"
                v-show="(!((showEnablingConditions === undefined && activeRoster.enablementCondition) || showEnablingConditions))">
                <button type="button" class="btn btn-lg btn-link" @click="showEnablingConditions = true">
                    {{ $t('QuestionnaireEditor.AddEnablingCondition') }}
                </button>
            </div>

            <div class="row"
                v-show="(((showEnablingConditions === undefined && activeRoster.enablementCondition) || showEnablingConditions))">
                <div class="form-group col-xs-11">
                    <div class="enabling-group-marker" :class="{ 'hide-if-disabled': activeRoster.hideIfDisabled }"></div>
                    <label for="edit-group-condition">{{ $t('QuestionnaireEditor.EnablingCondition') }}
                        <help link="conditionExpression" />
                    </label>

                    <input type="checkbox" class="wb-checkbox" disabled="disabled" checked="checked"
                        v-if="questionnaire.hideIfDisabled" :title="$t('QuestionnaireEditor.HideIfDisabledNested')" />

                    <input id="cb-hideIfDisabled" type="checkbox" class="wb-checkbox" v-model="activeRoster.hideIfDisabled"
                        v-if="!questionnaire.hideIfDisabled" />
                    <label for="cb-hideIfDisabled">
                        <span
                            :title="questionnaire.hideIfDisabled ? $t('QuestionnaireEditor.HideIfDisabledNested') : ''"></span>
                        {{ $t('QuestionnaireEditor.HideIfDisabled') }}
                        <help link="hideIfDisabled" />
                    </label>
                    <br>
                    <div class="pseudo-form-control">
                        <!--div id="edit-group-condition" ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
                            v-model="activeRoster.enablementCondition"></div-->
                        <ExpressionEditor v-model="activeRoster.enablementCondition"></ExpressionEditor>
                    </div>
                </div>
                <div class="form-group col-xs-1">
                    <button type="button" class="btn cross instructions-cross"
                        @click="showEnablingConditions = false; activeRoster.enablementCondition = ''; activeRoster.hideIfDisabled = false; dirty = true;"></button>
                </div>

            </div>
        </div>

        <div class="form-buttons-holder">
            <div class="pull-left">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="edit-roster-save-button"
                    class="btn btn-lg" :class="{ 'btn-primary': dirty }" @click="saveRoster()" unsaved-warning-clear
                    :disabled="!valid">{{ $t('QuestionnaireEditor.Save') }}</button>
                <button type="reset" id="edit-chapter-cancel-button" class="btn btn-lg btn-link" unsaved-warning-clear
                    @click="cancel()">{{ $t('QuestionnaireEditor.Cancel') }}</button>
            </div>
            <div class="pull-right">
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="add-comment-button"
                    class="btn btn-lg btn-link" @click="toggleComments(activeQuestion)" unsaved-warning-clear>
                    <span v-show="!isCommentsBlockVisible && commentsCount == 0">{{
                        $t('QuestionnaireEditor.EditorAddComment') }}</span>
                    <span v-show="!isCommentsBlockVisible && commentsCount > 0">
                        {{ $t('QuestionnaireEditor.EditorShowComments', {
                            count: commentsCount
                        }) }}</span>
                    <span v-show="isCommentsBlockVisible">{{ $t('QuestionnaireEditor.EditorHideComment') }}</span>
                </button>
                <button type="button" v-show="!questionnaire.isReadOnlyForUser" id="edit-chapter-delete-button"
                    class="btn btn-lg btn-link" unsaved-warning-clear @click="deleteRoster()">{{
                        $t('QuestionnaireEditor.Delete') }}</button>
                <MoveToChapterSnippet :item-id="rosterId"
                    v-show="!questionnaire.isReadOnlyForUser && !currentChapter.isReadOnly">
                </MoveToChapterSnippet>
            </div>
        </div>

    </form>
</template>

<script>
import { useRosterStore } from '../../../stores/roster';
import { useCommentsStore } from '../../../stores/comments';
import MoveToChapterSnippet from './MoveToChapterSnippet.vue';
import ExpressionEditor from './ExpressionEditor.vue';
import Breadcrumbs from './Breadcrumbs.vue'
import Help from './Help.vue'
import { find } from 'lodash'
import { convertToText, validateText, convertToTable } from '../../OptionsEditor/utils/tableToString';

export default {
    name: 'Roster',
    components: { MoveToChapterSnippet, ExpressionEditor, Breadcrumbs, Help },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        rosterId: { type: String, required: true }
    },
    data() {
        return {
            activeRoster: {},
            breadcrumbs: [],
            showEnablingConditions: undefined,
            selectedListQuestion: null,
            selectedMultiQuestion: null,
            selectedNumericQuestion: null,
            selectedTitleQuestion: null,
            dirty: false,
            valid: true,
            stringifiedRosterTitles: {
                valid: true,
                $error: {
                    matchOptionsPattern: false,
                    maxOptionsCount: false
                }
            },
            useListAsRosterTitleEditor: true,
            fixedRosterLimit: 2,
        };
    },
    watch: {
        activeRoster: {
            handler(newVal, oldVal) {
                if (oldVal != null) this.dirty = true;
            },
            deep: true
        },

        async rosterId(newValue, oldValue) {
            if (newValue != oldValue) {
                this.rosterStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const rosterStore = useRosterStore();
        const commentsStore = useCommentsStore();

        return {
            rosterStore, commentsStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        typeName() {
            if (!this.activeRoster.rosterTypeOptions) return null;

            const option = this.activeRoster.rosterTypeOptions.find(
                p => p.value == this.activeRoster.type
            );
            return option != null ? option.text : null;
        },
        commentsCount() {
            return this.commentsStore.getCommentsCount;
        },
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        },
    },
    methods: {
        async fetch() {
            await this.rosterStore.fetchRosterData(
                this.questionnaireId,
                this.rosterId
            );

            this.activeRoster = this.rosterStore.getRoster;
            this.breadcrumbs = this.rosterStore.getBreadcrumbs;

            this.selectedNumericQuestion = this.findQuestion(this.activeRoster.numericIntegerQuestions, this.activeRoster.rosterSizeNumericQuestionId);
            this.selectedTitleQuestion = this.findQuestion(this.activeRoster.numericIntegerTitles, this.activeRoster.rosterTitleQuestionId);
            this.selectedListQuestion = this.findQuestion(this.activeRoster.textListsQuestions, this.activeRoster.rosterSizeListQuestionId);
            this.selectedMultiQuestion = this.findQuestion(this.activeRoster.notLinkedMultiOptionQuestions, this.activeRoster.rosterSizeMultiQuestionId);
            this.dirty = false;
        },
        saveRoster() {
            this.rosterStore.saveRosterData();
            this.dirty = false;
        },
        cancel() {
            this.rosterStore.discardChanges();
            this.activeGroup = this.rosterStore.getRoster;
            this.dirty = false;
        },
        toggleComments() {
            this.commentsStore.toggleComments();
        },
        async selectNumericQuestion(numericId) {
            this.activeRoster.rosterSizeNumericQuestionId = numericId;
            this.selectedNumericQuestion = this.findQuestion(this.activeRoster.numericIntegerQuestions, numericId);
            this.dirty = true;
            const result = await this.rosterStore.getQuestionsEligibleForNumericRosterTitle(this.activeRoster.rosterSizeNumericQuestionId);

            this.activeRoster.numericIntegerTitles = result;
            this.selectedTitleQuestion = this.findQuestion(this.activeRoster.numericIntegerTitles, this.activeRoster.rosterTitleQuestionId);
        },
        selectTitleQuestion(titleQuestionId) {
            this.activeRoster.rosterTitleQuestionId = titleQuestionId;
            this.selectedTitleQuestion = this.findQuestion(this.activeRoster.numericIntegerTitles, titleQuestionId);
            this.dirty = true;
        },
        selectListQuestion(listId) {
            this.activeRoster.rosterSizeListQuestionId = listId;
            this.selectedListQuestion = this.findQuestion(this.activeRoster.textListsQuestions, listId);
            this.dirty = true;
        },
        selectMultiQuestion(multiId) {
            this.activeRoster.rosterSizeMultiQuestionId = multiId;
            this.selectedMultiQuestion = this.findQuestion(this.activeRoster.notLinkedMultiOptionQuestions, multiId);
            this.dirty = true;
        },
        findQuestion(collection, itemId) {
            const item = find(collection, item => item.id == itemId && item.isSectionPlaceHolder == false);
            if (item)
                return item;

            return null;
        },
        deleteRoster() {
            //
        },
        addFixedTitle() {
            this.activeRoster.fixedRosterTitles.push({
                "value": null,
                "title": ''
            });
            this.dirty = true;
        },
        removeFixedTitle(index) {
            this.activeRoster.fixedRosterTitles.splice(index, 1);
            this.dirty = true;
        },
        async showOptionsInTextarea() {
            const text = await convertToText(this.activeRoster.fixedRosterTitles);
            this.activeRoster.stringifiedRosterTitles = text;
            this.useListAsRosterTitleEditor = false;
        },

        async showRosterTitlesInList() {
            if (this.useListAsRosterTitleEditor) {
                return;
            }

            this.stringifiedRosterTitlesValidate();

            if (!this.stringifiedRosterTitles.valid) {
                return;
            }
            if (this.activeRoster.stringifiedRosterTitles) {
                const titles = await convertToTable(this.activeRoster.stringifiedRosterTitles);
                this.activeRoster.fixedRosterTitles = titles;
            }
            this.useListAsRosterTitleEditor = true;
        },

        stringifiedRosterTitlesValidate() {
            if (this.useListAsRosterTitleEditor == true)
                return true;

            const lines = (this.activeRoster.stringifiedRosterTitles || '').split(/\r\n|\r|\n/);
            const lineCount = lines.length

            if (lineCount > this.fixedRosterLimit) {
                this.stringifiedRosterTitles.$error.maxOptionsCount = true
                this.stringifiedRosterTitles.valid = false;
                return;
            }
            else if (this.stringifiedRosterTitles.$error.maxOptionsCount == true) {
                this.stringifiedRosterTitles.$error.maxOptionsCount = false
            }

            const top5Errors = validateText(this.activeRoster.stringifiedRosterTitles, false).slice(0, 5);
            if (top5Errors.length > 0) {
                this.stringifiedRosterTitles.$error.matchOptionsPattern = true;
                this.stringifiedRosterTitles.valid = false;
                return;
            }
            else if (this.stringifiedRosterTitles.$error.matchOptionsPattern == true) {
                this.stringifiedRosterTitles.$error.matchOptionsPattern = false
            }

            if (this.stringifiedRosterTitles.valid == false) {
                this.stringifiedRosterTitles.valid = true;
            }
        }
    }
}
</script>
