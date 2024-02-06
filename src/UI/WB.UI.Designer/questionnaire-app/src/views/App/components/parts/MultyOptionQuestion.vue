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
                        {{ getCategoricalKind().text }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation" v-for="kind in getCategoricalMultiKinds()">
                            <a role="menuitem" tabindex="-1" @click="changeCategoricalKind(kind)">
                                {{ kind.text }}
                            </a>
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
                            <a role="menuitem" tabindex="-1"
                                v-if="!activeQuestion.isFilteredCombobox && !activeQuestion.yesNoView"
                                @click="setIsLinkedQuestion()">{{ $t('QuestionnaireEditor.RostersQuestion') }}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row"
        v-if="!activeQuestion.isFilteredCombobox && !activeQuestion.isLinked && !activeQuestion.isLinkedToReusableCategories">
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
                            {{ getSelectedCategories().name }}
                        </span>
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation" v-for="categories in getCategoriesList()  ">
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
    <div class="row" v-if="activeQuestion.isFilteredCombobox && !activeQuestion.isLinkedToReusableCategories">
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
    <p></p>
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
    <div class="row table-holder">
        <div class="col-xs-5">
            <div class="checkbox checkbox-in-column">
                <input id="cb-is-ordered" type="checkbox" class="wb-checkbox" v-model="activeQuestion.areAnswersOrdered" />
                <label for="cb-is-ordered"><span></span>{{ $t('QuestionnaireEditor.QuestionOrdered') }}</label>
            </div>
        </div>
        <div class="col-xs-6">
            <div class="form-group singleline-group checkbox-in-column" :class="{ 'has-error': !validMaxAllowedAnswers }">
                <label for="edit-question-max-answers-number">{{ $t('QuestionnaireEditor.QuestionMaxNumberOfAnswers')
                }}</label>
                <input maxlength="9" name="editQuestionMaxAnswersNumber" v-pattern="/^\d+$/"
                    id="edit-question-max-answers-number" type="text" class="form-control small-numeric-input"
                    v-model.number="activeQuestion.maxAllowedAnswers" />
                <p class="help-block ng-cloak" v-show="!validMaxAllowedAnswers">{{
                    $t('QuestionnaireEditor.QuestionOnlyInts') }}
                </p>
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'
import OptionsEditorTemplate from './OptionsEditorTemplate.vue'
import CategoricalFilterExpression from './CategoricalFilterExpression.vue'
import LinkTemplate from './LinkTemplate.vue'
import AddClassification from './AddClassification.vue';

import { categoricalMultiKinds } from '../../../../helpers/question'
import { isInteger } from '../../../../helpers/number';

export default {
    name: 'MultyOptionQuestion',
    components: {
        Help,
        OptionsEditorTemplate,
        CategoricalFilterExpression,
        LinkTemplate,
        AddClassification,
    },
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
    computed: {
        validMaxAllowedAnswers() {
            return this.activeQuestion.maxAllowedAnswers == null || this.activeQuestion.maxAllowedAnswers == '' || isInteger(this.activeQuestion.maxAllowedAnswers);
        },
    },
    methods: {
        preperaToSave() {
            this.$refs.options.showOptionsInList();
        },

        getCategoricalKind() {
            if (this.activeQuestion.isFilteredCombobox)
                return categoricalMultiKinds[2];
            else if (this.activeQuestion.yesNoView)
                return categoricalMultiKinds[1];
            else
                return categoricalMultiKinds[0];
        },

        getCategoricalMultiKinds() {
            return categoricalMultiKinds;
        },

        getSourceOfCategories() {
            if (this.activeQuestion.isLinked)
                return this.$t('QuestionnaireEditor.RostersQuestion');

            return this.activeQuestion.isLinkedToReusableCategories === true
                ? this.$t('QuestionnaireEditor.ReusableCategories')
                : this.$t('QuestionnaireEditor.UserDefinedCategories');
        },

        changeCategoricalKind(kind) {
            var isFilteredCombobox = kind.value === 3;
            var yesNoView = kind.value === 2;

            if (isFilteredCombobox === this.activeQuestion.isFilteredCombobox &&
                yesNoView === this.activeQuestion.yesNoView) return;

            this.activeQuestion.isFilteredCombobox = isFilteredCombobox;
            this.activeQuestion.yesNoView = yesNoView;

            if (this.activeQuestion.isLinked)
                this.activeQuestion.isLinked = !isFilteredCombobox && !yesNoView;

            this.markFormAsChanged();
        },

        setCategories(categories) {
            if (this.activeQuestion.categoriesId === categories.categoriesId) return;

            this.activeQuestion.categoriesId = categories.categoriesId;
            this.markFormAsChanged();
        },

        markFormAsChanged() {
            this.dirty = true;
        },

        setIsReusableCategories() {
            if (this.activeQuestion.isLinkedToReusableCategories === true) return;

            this.activeQuestion.isLinked = false;
            this.activeQuestion.isLinkedToReusableCategories = true;

            this.markFormAsChanged();
        },

        setUserDefinedCategories() {
            if (this.activeQuestion.isLinkedToReusableCategories === false) return;

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
    }

}
</script>
