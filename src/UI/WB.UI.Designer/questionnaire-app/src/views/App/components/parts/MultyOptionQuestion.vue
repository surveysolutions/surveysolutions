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
                    <button class="btn dropdown-toggle" id="cb-categorical-kind" uib-dropdown-toggle type="button">
                        {{ getCategoricalKind().text }}
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation" v-for="kind in activeQuestion.categoricalMultiKinds">
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
                    <button class="btn dropdown-toggle" id="cb-categorical-type" uib-dropdown-toggle type="button">
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
            <ng-include src="'views/question-details/OptionsEditor-template.html'"></ng-include>
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
                    <button class="btn dropdown-toggle" uib-dropdown-toggle id="cb-categories" type="button">
                        <span class="select-placeholder" v-if="(activeQuestion.categoriesId || '') == ''">{{
                            $t('QuestionnaireEditor.SelectCategories') }}</span>
                        <span class="selected-item" v-if="(activeQuestion.categoriesId || '') !== ''">
                            {{ getSelectedCategories().name }}
                        </span>
                        <span class="dropdown-arrow"></span>
                    </button>

                    <ul class="dropdown-menu" role="menu">
                        <li role="presentation" v-for="  categories   in   getCategoriesList()  ">
                            <a @click="setCategories(categories)" role="menuitem" tabindex="-1"
                                class="linked-question-source" href="javascript:void(0);">
                                <div>
                                    <span ng-bind="categories.name | escape"></span>
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
        </div>
    </div>
    <p></p>
    <div class="row" v-if="activeQuestion.isLinked">
        <div class="col-xs-12">
            <div class="form-group" ng-include="'linkTemplate.html'"
                :class="{ 'has-error': !questionForm.linkedToEntity.$valid }"></div>
            <p></p>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <ng-include src="'categorical-filter-expression'"></ng-include>
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
            <div class="form-group singleline-group checkbox-in-column"
                :class="{ 'has-error': !questionForm.editQuestionMaxAnswersNumber.$valid }">
                <label for="edit-question-max-answers-number">{{ $t('QuestionnaireEditor.QuestionMaxNumberOfAnswers')
                }}</label>
                <input maxlength="9" name="editQuestionMaxAnswersNumber" ng-pattern="/^\d+$/"
                    id="edit-question-max-answers-number" type="text" class="form-control small-numeric-input"
                    v-model="activeQuestion.maxAllowedAnswers" />
                <p class="help-block ng-cloak" v-show="questionForm.editQuestionMaxAnswersNumber.$error.pattern">{{
                    $t('QuestionnaireEditor.QuestionOnlyInts') }}
                </p>
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'

export default {
    name: 'MultyOptionQuestion',
    components: {
        Help,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
}
</script>
