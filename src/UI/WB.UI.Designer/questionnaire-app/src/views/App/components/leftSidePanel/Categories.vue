<template>
    Categories

    <!--div class="categories">
        <div id="show-reload-details-promt" class="ng-cloak" ng-show="shouldUserSeeReloadPromt">
            <div class="inner">{{ 'QuestionToUpdateOptions' | i18next }} <a href="#" onclick="window.location.reload(true);"
                    ng-i18next="QuestionClickReload"></a></div>
        </div>

        <perfect-scrollbar class="scroller">
            <h3 ng-i18next="[i18next]({count: categoriesList.length })SideBarCategoriesCounter"></h3>

            <div class="empty-list" ng-show="categoriesList.length == 0">
                <p ng-i18next="SideBarCategoriesEmptyLine1"></p>
                <p ng-i18next="SideBarCategoriesEmptyLine2"></p>
                <p>
                    <span class="variable-name" ng-i18next="VariableName"></span>
                    {{ 'SideBarCategoriesEmptyLine3' | i18next }}
                </p>
            </div>
            <form role="form" name="categoriesForm" novalidate>
                <div class="categories-list">
                    <ng-form name="categories.form" ng-repeat="categories in categoriesList">
                        <div class="categories-panel-item" :class="{ 'has-error': categories.form.name.$error.pattern }"
                            ngf-drop="" ngf-change="fileSelected(categories, $file)"
                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                            <a href @click="deleteCategories($index)" class="btn delete-btn" tabindex="-1"
                                ng-if="!questionnaire.isReadOnlyForUser"></a>
                            <div class="categories-content">
                                <input focus-on-out="focusCategories{{categories.categoriesId}}" required=""
                                    ng-i18next="[placeholder]SideBarCategoriesName" maxlength="32" spellcheck="false"
                                    ng-model="categories.name" name="name" class="form-control table-name" type="text" />
                                <div class="drop-box" ng-i18next="SideBarLookupTableDropFile">
                                    Drop File here
                                </div>
                                <div class="actions" :class="{ dirty: categories.form.$dirty }">
                                    <div ng-show="categories.form.$dirty" class="pull-left">
                                        <button type="submit" ng-disabled="categories.form.$invalid"
                                            class="btn lighter-hover"
                                            @click="saveCategories(categories); $event.stopPropagation()" ng-i18next>
                                            Save
                                        </button>
                                        <button type="button" class="btn lighter-hover"
                                            @click="cancel(categories); $event.stopPropagation()" ng-i18next>Cancel</button>
                                    </div>

                                    <div class="permanent-actions pull-right">
                                        <a href="javascript:void(0);" class="btn btn-link"
                                            @click="editCategories(questionnaire.questionnaireId, categories.categoriesId)"
                                            ng-i18next="SideBarEditCategories">
                                        </a>

                                        <button ng-hide="categories.form.$dirty" ng-disabled="isReadOnlyForUser"
                                            class="btn btn-default" ngf-select="" ngf-max-size="10MB"
                                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                                            ngf-change="fileSelected(categories, $file);$event.stopPropagation()"
                                            type="button">
                                            <span ng-i18next>Update</span>
                                        </button>

                                        {{ 'SideBarDownload' | i18next }}
                                        <a href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=xlsx&isCategory=true&entityId=' + categories.categoriesId}}"
                                            class="btn btn-default" target="_blank" rel="noopener"
                                            ng-i18next>SideBarXlsx</a>
                                        <a href="{{'/questionnaire/ExportOptions/' + questionnaire.questionnaireId  + '?type=csv&isCategory=true&entityId=' + categories.categoriesId}}"
                                            class="btn btn-default" target="_blank" rel="noopener" ng-i18next>SideBarTab</a>

                                    </div>
                                </div>
                            </div>
                        </div>
                    </ng-form>
                </div>
            </form>
            <div class="button-holder">
                <p>
                    <span ng-i18next="SideBarTranslationGetTemplate"></span>

                    <a class="btn btn-default" href="{{downloadBaseUrl + '/template' }}" target="_blank" rel="noopener"
                        ng-i18next>
                        SideBarXlsx
                    </a>
                    <a class="btn btn-default" href="{{downloadBaseUrl + '/templateTab' }}" target="_blank" rel="noopener"
                        ng-i18next>
                        SideBarTab
                    </a>
                </p>
                <p>
                    <input type="button" ng-i18next="[value]SideBarCategoriesAddNew" value="ADD new category"
                        class="btn lighter-hover" @click="addNewCategory(); $event.stopPropagation()"
                        ng-disabled="isReadOnlyForUser" />
                </p>
                <p>
                    <input type="button" ng-i18next="[value]SideBarCategoriesUploadNew" value="Upload new categories"
                        class="btn lighter-hover" ngf-select
                        ngf-change="createAndUploadFile($file);$event.stopPropagation()" ngf-max-size="10MB"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                        ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser"
                        ng-disabled="isReadOnlyForUser" />
                </p>
            </div>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Categories',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  