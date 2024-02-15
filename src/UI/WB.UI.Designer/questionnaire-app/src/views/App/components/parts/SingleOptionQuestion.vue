<template>
    <div class="row">
        <div class="col-xs-12">
            <label class="wb-label" for="cb-categorical-kind">{{ $t('QuestionnaireEditor.QuestionDisplayMode') }}</label>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <div class="dropdown-with-breadcrumbs-and-icons">
                <div class="btn-group" uib-dropdown>
                    <button class="btn dropdown-toggle" id="cb-categorical-kind" uib-dropdown-toggle type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ displayMode }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" @click="setQuestionAsRadioButtons()">{{
                                $t('QuestionnaireEditor.QuestionRadioButtonList') }}</a>
                        </li>
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" @click="setQuestionAsCombobox()">{{
                                $t('QuestionnaireEditor.QuestionComboBox') }}</a>
                        </li>
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" @click="setQuestionAsCascading()">{{
                                $t('QuestionnaireEditor.QuestionCascading') }}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <label class="wb-label" for="cb-categories-type">{{ $t('QuestionnaireEditor.SourceOfCategories') }}</label>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <div class="dropdown-with-breadcrumbs-and-icons">
                <div class="btn-group" uib-dropdown>
                    <button class="btn dropdown-toggle" id="cb-categorical-type" uib-dropdown-toggle type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        {{ categoriesSource }}
                        <span class="dropdown-arrow"></span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" @click="setUserDefinedCategories()">{{
                                $t('QuestionnaireEditor.UserDefinedCategories') }}</a>
                        </li>
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" @click="setIsReusableCategories()">{{
                                $t('QuestionnaireEditor.ReusableCategories') }}</a>
                        </li>
                        <li role="presentation">
                            <a role="menuitem" tabindex="-1" v-if="!isCascade" @click="setIsLinkedQuestion()">{{
                                $t('QuestionnaireEditor.RostersQuestion') }}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row" v-if="hasOwnCategories">
        <div class="col-xs-12">
            <div class="well well-sm" v-if="activeQuestion.wereOptionsTruncated">{{
                $t('QuestionnaireEditor.QuestionOptionsCut', { count: 200 }) }}</div>
            <!--ng-include src="'views/question-details/OptionsEditor-template.html'"></ng-include-->
            <OptionsEditorTemplate ref="options" :activeQuestion="activeQuestion" :questionnaireId="questionnaireId">
            </OptionsEditorTemplate>

            <p></p>
        </div>
    </div>
    <div class="row" v-if="isLinkedToReusableCategories === true">
        <div class="col-xs-12">
            <label class="wb-label" for="cb-categories">{{ $t('QuestionnaireEditor.BindToReusableCategories') }}</label>
        </div>
    </div>
    <div class="row" v-if="isLinkedToReusableCategories === true">
        <div class="col-xs-12">
            <div class="dropdown-with-breadcrumbs-and-icons">
                <div class="btn-group" uib-dropdown>
                    <button class="btn dropdown-toggle" uib-dropdown-toggle id="cb-categories" type="button"
                        data-bs-toggle="dropdown" aria-expanded="false">
                        <span class="select-placeholder" v-if="(activeQuestion.categoriesId || '') == ''">{{
                            $t('QuestionnaireEditor.SelectCategories') }}</span>
                        <span class="selected-item" v-if="(activeQuestion.categoriesId || '') !== ''">
                            {{ getSelectedCategories.name }}
                        </span>
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation" v-for="categories in questionnaire.categories">
                            <a @click="setCategories(categories)" role="menuitem" tabindex="-1"
                                class="linked-question-source" href="javascript:void(0);">
                                <div>
                                    <span v-sanitize-text="categories.name"></span>
                                </div>
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <p></p>
    <div class="row" v-if="activeQuestion.isFilteredCombobox && !isCascade && !isLinkedToReusableCategories && !isLinked">
        <div class="col-xs-12">
            <FilteredComboboxOptionsTemplate :active-question="activeQuestion" :questionnaire-id="questionnaireId">
            </FilteredComboboxOptionsTemplate>
            <a href="javascript:void(0);" class="btn btn-link" @click="showAddClassificationModal()">{{
                $t('QuestionnaireEditor.QuestionAddClassification') }}
            </a>
            <p></p>
            <add-classification ref="classification" :activeQuestion='activeQuestion' :questionnaireId="questionnaireId">
            </add-classification>
        </div>
    </div>
    <div class="row" v-if="isLinked">
        <div class="col-xs-12">
            <LinkTemplate :activeQuestion="activeQuestion">
            </LinkTemplate>
            <p></p>
        </div>
    </div>
    <div class="row" v-if="isCascade === false">
        <div class="col-xs-12">
            <!--ng-include src="'categorical-filter-expression'"></ng-include-->
            <CategoricalFilterExpression :activeQuestion="activeQuestion">
            </CategoricalFilterExpression>
        </div>
    </div>
    <div class="row" v-if="isCascade === true">
        <div class="col-xs-12">
            <div class="form-group">
                <CascadingComboBoxTemplate :activeQuestion="activeQuestion">
                </CascadingComboBoxTemplate>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row"
        v-if="isCascade && (activeQuestion.cascadeFromQuestionId || '') !== '' && !isLinkedToReusableCategories">
        <div class="col-xs-12">
            <a href="javascript:void(0);" class="btn btn-link" @click="editCascadingComboboxOptions()">{{
                $t('QuestionnaireEditor.QuestionUploadOptions') }}
            </a>
            <p></p>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'
import { useQuestionStore } from '../../../../stores/question';
import OptionsEditorTemplate from './OptionsEditorTemplate.vue'
import CategoricalFilterExpression from './CategoricalFilterExpression.vue'
import CascadingComboBoxTemplate from './CascadingComboBoxTemplate.vue'
import LinkTemplate from './LinkTemplate.vue'
import AddClassification from './AddClassification.vue';
import FilteredComboboxOptionsTemplate from './FilteredComboboxOptionsTemplate.vue'
import _ from 'lodash';

export default {
    name: 'SingleOptionQuestion',
    inject: ['questionnaire', 'currentChapter', 'openExternalEditor'],
    components: {
        Help,
        OptionsEditorTemplate,
        CategoricalFilterExpression,
        CascadingComboBoxTemplate,
        LinkTemplate,
        AddClassification,
        FilteredComboboxOptionsTemplate,
    },
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            isLinkedToReusableCategories: false,
            isCascade: null,
            isLinked: false,
        }
    },
    setup() {
        const questionStore = useQuestionStore();

        return {
            questionStore
        };
    },
    mounted() {
        this.$emitter.on('questionChangesDiscarded', this.questionChangesDiscarded);
        this.reset();
    },
    unmounted() {
        this.$emitter.off('questionChangesDiscarded', this.questionChangesDiscarded);
    },
    watch: {
        categoriesId: function (v) {
            this.isLinkedToReusableCategories = this.activeQuestion.categoriesId !== null;
        }
    },
    computed: {
        hasOwnCategories() {
            var hasCategories = !this.activeQuestion.isFilteredCombobox
                && !this.isLinked
                && !this.isCascade
                && !this.isLinkedToReusableCategories;

            return hasCategories;
        },
        getSelectedCategories() {
            return this.getCategoriesList().find(c => c.categoriesId === this.activeQuestion.categoriesId) || {};
        },
        displayMode() {
            if (this.isCascade)
                return this.$t('QuestionnaireEditor.QuestionCascading');

            if (this.activeQuestion.isFilteredCombobox === true)
                return this.$t('QuestionnaireEditor.QuestionComboBox');

            else return this.$t('QuestionnaireEditor.QuestionRadioButtonList');
        },
        categoriesSource() {
            if (this.isLinked)
                return this.$t('QuestionnaireEditor.RostersQuestion');

            return this.isLinkedToReusableCategories === true
                ? this.$t('QuestionnaireEditor.ReusableCategories')
                : this.$t('QuestionnaireEditor.UserDefinedCategories');
        },
        categoriesId() {
            return this.activeQuestion.categoriesId;
        },
        initialQuestion() {
            return this.questionStore.getInitialQuestion;
        },
    },
    methods: {
        reset() {
            this.isLinkedToReusableCategories = !_.isEmpty(this.activeQuestion.categoriesId);
            this.isCascade = !_.isEmpty(this.activeQuestion.cascadeFromQuestionId);
            this.isLinked = !_.isEmpty(this.activeQuestion.linkedToEntityId);
        },
        questionChangesDiscarded() {
            this.reset()
        },
        prepareToSave() {
            if (this.hasOwnCategories)
                this.$refs.options.showOptionsInList();
        },

        setQuestionAsRadioButtons() {
            this.isCascade = false;
            this.isLinked = false;
            this.activeQuestion.isFilteredCombobox = false;
            this.activeQuestion.cascadeFromQuestionId = null;

            this.activeQuestion.showAsListThreshold = null;
            this.activeQuestion.showAsList = false;
        },

        setQuestionAsCombobox() {
            if (this.activeQuestion.isFilteredCombobox === true) return;

            this.isCascade = false;
            this.activeQuestion.cascadeFromQuestionId = null;
            this.activeQuestion.isFilteredCombobox = true;

            this.activeQuestion.showAsListThreshold = null;
            this.activeQuestion.showAsList = false;
        },

        setQuestionAsCascading() {
            if (this.isCascade === true) return;

            this.isLinked = false;
            this.activeQuestion.isFilteredCombobox = false;
            this.isCascade = true;
        },

        setIsReusableCategories() {
            if (this.isLinkedToReusableCategories === true) return;

            this.isLinked = false;
            this.activeQuestion.categoriesId = null;
            this.isLinkedToReusableCategories = true;
        },

        setUserDefinedCategories() {

            this.isLinked = false;
            this.activeQuestion.categoriesId = null;
            this.isLinkedToReusableCategories = false;
        },

        setIsLinkedQuestion() {
            if (this.isLinked === true) return;

            this.isLinked = true;
            this.isCascade = false;
            this.isLinkedToReusableCategories = null;
            this.activeQuestion.categoriesId = null;
        },

        getCategoriesList() {
            return (this.questionnaire || {}).categories || [];
        },

        showAddClassificationModal() {
            this.$refs.classification.openDialog();
        },
        setCategories(categories) {
            if (this.activeQuestion.categoriesId === categories.categoriesId) return;

            this.activeQuestion.categoriesId = categories.categoriesId;
        },

        editCascadingComboboxOptions() {
            const wasCascadeFromQuestionIdChanged = (this.activeQuestion.cascadeFromQuestionId != this.initialQuestion.cascadeFromQuestionId);
            if (this.questionStore.getIsDirty || wasCascadeFromQuestionIdChanged) {
                const params = {
                    title: this.$t('QuestionnaireEditor.QuestionOpenEditorConfirm'),
                    okButtonTitle: this.$t('QuestionnaireEditor.Save'),
                    cancelButtonTitle: this.$t('QuestionnaireEditor.Cancel'),
                    isReadOnly: this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly,
                    callback: async confirm => {
                        if (confirm) {
                            await this.questionStore.saveQuestionData(this.questionnaireId, this.activeQuestion);

                            const alertParams = {
                                title: this.$t('QuestionnaireEditor.QuestionOpenEditorSaved'),
                                okButtonTitle: this.$t('QuestionnaireEditor.Ok'),
                                isReadOnly: this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly,
                                isAlert: true,
                                callback: async alertConfirm => {
                                    this.openCascadeOptionsEditor();
                                }
                            }

                            this.$confirm(alertParams);
                        }
                    }
                };

                this.$confirm(params);
            } else {
                this.openCascadeOptionsEditor();
            }
        },

        openCascadeOptionsEditor() {
            this.openExternalEditor(this.activeQuestion.id, "/questionnaire/editoptions/" + this.questionnaireId + "?questionid=" + this.activeQuestion.id + "&cascading=true")
        }
    }
}
</script>
