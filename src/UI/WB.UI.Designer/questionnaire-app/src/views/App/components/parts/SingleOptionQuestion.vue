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
                        {{ getCategoricalSingleDisplayMode() }}
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
                        {{ getSourceOfCategories() }}
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
                            <a role="menuitem" tabindex="-1" v-if="!activeQuestion.isCascade"
                                @click="setIsLinkedQuestion()">{{ $t('QuestionnaireEditor.RostersQuestion') }}</a>
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


    <div class="row" v-if="activeQuestion.isLinkedToReusableCategories === true">
        <div class="col-xs-12">
            <label class="wb-label" for="cb-categories">{{ $t('QuestionnaireEditor.BindToReusableCategories') }}</label>
        </div>
    </div>
    <div class="row" v-if="activeQuestion.isLinkedToReusableCategories === true">
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
    <div class="row"
        v-if="activeQuestion.isFilteredCombobox && !activeQuestion.isCascade && !activeQuestion.isLinkedToReusableCategories && !activeQuestion.isLinked">
        <div class="col-xs-12">
            <a href="javascript:void(0);" class="btn btn-link upload-categories-button"
                @click="editFilteredComboboxOptions()">{{ $t('QuestionnaireEditor.QuestionUploadOptions') }}
            </a>
            <a href="javascript:void(0);" class="btn btn-link" @click="showAddClassificationModal()">{{
                $t('QuestionnaireEditor.QuestionAddClassification') }}
            </a>
            <p></p>
            <add-classification ref="classification" :activeQuestion='activeQuestion' :questionnaireId="questionnaireId">
            </add-classification>
        </div>
    </div>
    <div class="row" v-if="activeQuestion.isLinked">
        <div class="col-xs-12">
            <LinkTemplate :activeQuestion="activeQuestion">
            </LinkTemplate>
            <p></p>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <!--ng-include src="'categorical-filter-expression'"></ng-include-->
            <CategoricalFilterExpression :activeQuestion="activeQuestion">
            </CategoricalFilterExpression>
        </div>
    </div>
    <div class="row" v-if="activeQuestion.isCascade">
        <div class="col-xs-12">
            <div class="form-group">
                <CascadingComboBoxTemplate :activeQuestion="activeQuestion">
                </CascadingComboBoxTemplate>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row"
        v-if="activeQuestion.isCascade && (activeQuestion.cascadeFromQuestionId || '') !== '' && !activeQuestion.isLinkedToReusableCategories">
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
import OptionsEditorTemplate from './OptionsEditorTemplate.vue'
import CategoricalFilterExpression from './CategoricalFilterExpression.vue'
import CascadingComboBoxTemplate from './CascadingComboBoxTemplate.vue'
import LinkTemplate from './LinkTemplate.vue'
import AddClassification from './AddClassification.vue';
import _ from 'lodash';

export default {
    name: 'SingleOptionQuestion',
    inject: ['questionnaire'],
    components: {
        Help,
        OptionsEditorTemplate,
        CategoricalFilterExpression,
        CascadingComboBoxTemplate,
        LinkTemplate,
        AddClassification,
    },
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            dirty: false,
        }
    },
    mounted() {
        //move to initial binding
        this.activeQuestion.isLinkedToReusableCategories = !_.isEmpty(this.activeQuestion.categoriesId);
        this.activeQuestion.isCascade = !_.isEmpty(this.activeQuestion.cascadeFromQuestionId);
        this.activeQuestion.isLinked = !_.isEmpty(this.activeQuestion.linkedToEntityId);

    },
    computed: {
        hasOwnCategories() {
            var hasCategories = !this.activeQuestion.isFilteredCombobox
                && !this.activeQuestion.isLinked
                && !this.activeQuestion.isCascade
                && !this.activeQuestion.isLinkedToReusableCategories;

            return hasCategories;
        },
        getSelectedCategories() {
            return this.getCategoriesList().find(c => c.categoriesId === this.activeQuestion.categoriesId) || {};
        }
    },
    methods: {
        prepareToSave() {
            if (this.hasOwnCategories)
                this.$refs.options.showOptionsInList();
        },

        getCategoricalSingleDisplayMode() {
            if (this.activeQuestion.isCascade || (this.activeQuestion.cascadeFromQuestionId || '') !== '')
                return this.$t('QuestionnaireEditor.QuestionCascading');

            if (this.activeQuestion.isFilteredCombobox === true)
                return this.$t('QuestionnaireEditor.QuestionComboBox');

            else return this.$t('QuestionnaireEditor.QuestionRadioButtonList');
        },

        getSourceOfCategories() {
            if (this.activeQuestion.isLinked)
                return this.$t('QuestionnaireEditor.RostersQuestion');

            return this.activeQuestion.isLinkedToReusableCategories === true
                ? this.$t('QuestionnaireEditor.ReusableCategories')
                : this.$t('QuestionnaireEditor.UserDefinedCategories');
        },

        setQuestionAsRadioButtons() {
            this.activeQuestion.isCascade = false;
            this.activeQuestion.isLinked = false;
            this.activeQuestion.isFilteredCombobox = false;

            this.markFormAsChanged();
        },

        setQuestionAsCombobox() {
            if (this.activeQuestion.isFilteredCombobox === true) return;

            this.activeQuestion.isCascade = false;
            this.activeQuestion.isFilteredCombobox = true;

            this.markFormAsChanged();
        },

        setQuestionAsCascading() {
            if (this.activeQuestion.isCascade === true) return;

            this.activeQuestion.isLinked = false;
            this.activeQuestion.isFilteredCombobox = false;
            this.activeQuestion.isCascade = true;

            this.markFormAsChanged();
        },

        markFormAsChanged() {
            this.dirty = true;
        },

        setIsReusableCategories() {
            //if (this.activeQuestion.isLinkedToReusableCategories === true) return;

            this.activeQuestion.isLinked = false;
            this.activeQuestion.categoriesId = null;
            this.activeQuestion.isLinkedToReusableCategories = true;

            this.markFormAsChanged();
        },

        setUserDefinedCategories() {
            //if (this.activeQuestion.isLinkedToReusableCategories === false) return;

            this.activeQuestion.isLinked = false;
            this.activeQuestion.categoriesId = null;
            this.activeQuestion.isLinkedToReusableCategories = false;

            this.markFormAsChanged();
        },

        setIsLinkedQuestion() {
            if (this.activeQuestion.isLinked === true) return;

            this.activeQuestion.isLinked = true;
            this.activeQuestion.isCascade = false;
            this.activeQuestion.isLinkedToReusableCategories = null;
            this.activeQuestion.categoriesId = null;

            this.markFormAsChanged();
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

            this.markFormAsChanged();
        },
    }
}
</script>
