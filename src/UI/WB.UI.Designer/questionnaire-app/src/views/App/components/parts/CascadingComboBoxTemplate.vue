<template>
    <div id="sourceOfLinkedEntity" class="dropdown-with-breadcrumbs-and-icons"
        ng-class="{'has-error': !questionForm.linkedToEntity.$valid}">
        <input type="hidden" name="cascadeFromQuestionId" ng-model="activeQuestion.cascadeFromQuestionId" ng-update-hidden
            ng-required="true" />
        <label for="singleOptionQuestionSource" ng-i18next="SelectParentQuestion"></label>
        <div class="btn-group dropdown" uib-dropdown>
            <button class="btn dropdown-toggle" uib-dropdown-toggle id="singleOptionQuestionSource" type="button">
                <span class="select-placeholder" ng-if="(activeQuestion.cascadeFromQuestion.title || '') == ''"
                    ng-i18next="SelectQuestion"></span>

                <div class="selected-item" ng-if="(activeQuestion.cascadeFromQuestion.title || '') !== ''">
                    <span class="path">{{ activeQuestion.cascadeFromQuestion.breadcrumbs }}</span>
                    <div class="selected-block">
                        <div>
                            <span class="chosen-item "><i class="dropdown-icon"
                                    ng-class="'icon-'+activeQuestion.cascadeFromQuestion.type"></i>{{ activeQuestion.cascadeFromQuestion.title }}</span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" ng-bind-html="activeQuestion.cascadeFromQuestion.varName"></span>
                        </div>
                    </div>
                </div>

                <span class="dropdown-arrow"></span>
            </button>

            <ul class="dropdown-menu" role="menu">
                <li role="presentation" ng-class="{'dropdown-header': breadCrumb.isSectionPlaceHolder}"
                    ng-repeat="breadCrumb in sourceOfSingleQuestions track by $index">
                    <span ng-if="breadCrumb.isSectionPlaceHolder">{{:: breadCrumb.title }}</span>
                    <a ng-if="!breadCrumb.isSectionPlaceHolder" ng-click="setCascadeSource(breadCrumb.id)" role="menuitem"
                        tabindex="-1" href="javascript:void(0);">
                        <div>
                            <i class="dropdown-icon icon-{{breadCrumb.type}}"></i>
                            <span ng-bind-html="breadCrumb.title"></span>
                        </div>
                        <div class="var-block">
                            <span class="var-name" ng-bind-html="breadCrumb.varName"></span>
                        </div>
                    </a>
                </li>
            </ul>
        </div>
        <p class="help-block ng-cloak" ng-show="questionForm.cascadeFromQuestionId.$error.required"
            ng-i18next="SelectParentQuestionError"></p>
    </div>
    <div class="row">
        <div class="col-md-6">
            <div class="checkbox checkbox-in-column">
                <input id="cb-as-list" type="checkbox" class="wb-checkbox" ng-model="activeQuestion.showAsList"
                    ng-change="showAsListChange()" />
                <label for="cb-as-list"><span></span>{{ 'ShowAsList' | i18next }}</label>
            </div>
        </div>

        <div class="col-md-5 inline-inputs">
            <div class="form-group checkbox checkbox-in-column" ng-show="activeQuestion.showAsList"
                ng-class="{'has-error': !questionForm.showAsListThreshold.$valid}">
                <label for="edit-question-count-for-list" ng-i18next="QuestionShowListLimit"></label>
                <input id="edit-question-count-for-list" type="number" step="1" maxlength="2" name="showAsListThreshold"
                    min="1" max="50" ng-pattern="/^\d+$/" ng-model="activeQuestion.showAsListThreshold"
                    class="form-control small-numeric-input wb-input">
                <p class="help-block ng-cloak" ng-show="questionForm.showAsListThreshold.$error.pattern"
                    ng-i18next="QuestionOnlyInts"></p>
                <p class="help-block ng-cloak"
                    ng-show="questionForm.showAsListThreshold.$error.max || questionForm.showAsListThreshold.$error.min"
                    ng-i18next="QuestionOneToFiftyAllowed"></p>
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'

export default {
    name: 'CascadingComboBoxTemplate',
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
