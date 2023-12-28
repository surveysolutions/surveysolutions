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
                            <a role="menuitem" tabindex="-1" v-if="!activeQuestion.isCascade"
                                @click="setIsLinkedQuestion()">{{ $t('QuestionnaireEditor.RostersQuestion') }}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <p></p>
        </div>
    </div>
    <div class="row"
        v-if="!activeQuestion.isFilteredCombobox && !activeQuestion.isLinked && !activeQuestion.isCascade && !activeQuestion.isLinkedToReusableCategories">
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
                        <li role="presentation" v-for="   categories    in    getCategoriesList()   ">
                            <a @click="setCategories(categories)" role="menuitem" tabindex="-1"
                                class="linked-question-source" href="javascript:void(0);">
                                <div>
                                    <span v-bind="categories.name | escape"></span>
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
                v-click="editFilteredComboboxOptions()">{{ $t('QuestionnaireEditor.QuestionUploadOptions') }}
            </a>
            <a href="javascript:void(0);" class="btn btn-link" v-click="showAddClassificationModal()">{{
                $t('QuestionnaireEditor.QuestionAddClassification') }}
            </a>
            <p></p>
        </div>
    </div>
    <div class="row" v-if="activeQuestion.isLinked">
        <div class="col-xs-12">
            <div class="form-group" ng-include="'linkTemplate.html'"
                v-class="{ 'has-error': !questionForm.linkedToEntity.$valid }"></div>
            <p></p>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12">
            <ng-include src="'categorical-filter-expression'"></ng-include>
        </div>
    </div>
    <div class="row" v-if="activeQuestion.isCascade">
        <div class="col-xs-12">
            <div class="form-group" ng-include="'views/question-details/CascadingComboBox-Template.html'"></div>
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

export default {
    name: 'SingleOptionQuestion',
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
