<template>
    Translations

    <!--div class="translations">
        <perfect-scrollbar class="scroller">
            <h3 ng-i18next="[i18next]({count: translations.length })SideBarTranslationsCounter"></h3>

            <div class="empty-list" ng-show="translations.length == 1">
                <p ng-i18next="SideBarTranslationsEmptyLine1"></p>
                <p ng-i18next="SideBarTranslationsEmptyLine2"></p>
                <p ng-i18next="SideBarTranslationsEmptyLine3"></p>
            </div>
            <form role="form" name="translationsForm" novalidate>
                <div class="translation-list">
                    <ng-form name="translation.form" ng-repeat="translation in translations">
                        <div class="translations-panel-item" :class="{ 'has-error': translation.form.name.$error.pattern }"
                            ngf-drop="" ngf-max-size="4MB" ngf-change="fileSelected(translation, $file)"
                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                            <a href @click="deleteTranslation($index)" ng-disabled="questionnaire.isReadOnlyForUser"
                                class="btn delete-btn" tabindex="-1"
                                ng-show="!translation.isOriginalTranslation && !questionnaire.isReadOnlyForUser"></a>
                            <div class="translation-content">
                                <input focus-on-out="focusTranslation{{translation.translationId}}" required=""
                                    ng-i18next="[placeholder]SideBarTranslationName" maxlength="32" spellcheck="false"
                                    ng-model="translation.name" name="name" class="form-control table-name" type="text" />
                                <div class="drop-box" ng-i18next="SideBarLookupTableDropFile">
                                    Drop File here
                                </div>
                                <div class="actions" :class="{ dirty: translation.form.$dirty }">
                                    <div ng-show="translation.form.$dirty" class="pull-left">
                                        <button type="submit"
                                            ng-disabled="questionnaire.isReadOnlyForUser || translation.form.$invalid"
                                            class="btn lighter-hover" @click="onSave($event, translation)" ng-i18next>
                                            Save
                                        </button>
                                        <button type="button" class="btn lighter-hover"
                                            @click="onCancel($event, translation)" ng-i18next>Cancel</button>
                                    </div>

                                    <button type="button" class="btn btn-default"
                                        ng-show="translation.isDefault && !translation.isOriginalTranslation"
                                        @click="setDefaultTranslation($index, false); $event.stopPropagation()" ng-i18next>
                                        UnMarkAsDefault
                                    </button>

                                    <div class="permanent-actions pull-right">
                                        <button type="button" class="btn lighter-hover" ng-disabled="isReadOnlyForUser"
                                            ng-show="!translation.isDefault"
                                            @click="setDefaultTranslation($index, true); $event.stopPropagation()"
                                            ng-i18next>
                                            MarkAsDefault
                                        </button>

                                        <a ng-if="translation.downloadUrl" href="{{translation.downloadUrl}}"
                                            class="btn btn-default" target="_blank" rel="noopener noreferrer"
                                            ng-i18next>SideBarTranslationDownloadXlsx</a>

                                        <button ng-hide="translation.form.$dirty || translation.isOriginalTranslation"
                                            ng-disabled="isReadOnlyForUser" class="btn btn-default" ngf-select=""
                                            ngf-max-size="10MB"
                                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                                            ngf-change="fileSelected(translation, $file);$event.stopPropagation()"
                                            type="button">
                                            <span ng-i18next>Update</span>
                                        </button>
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
                    <a class="btn btn-default"
                        href="{{downloadBaseUrl + '/' + questionnaire.questionnaireId + '/template' }}" target="_blank"
                        rel="noopener" ng-i18next>
                        SideBarTranslationGetTemplateLinkTextXlsx
                    </a>
                </p>
                <p>
                    <input type="button" ng-i18next="[value]SideBarTranslationsUploadNew" value="Upload new translation"
                        ng-disabled="isReadOnlyForUser" class="btn lighter-hover" ngf-select
                        ngf-change="createAndUploadFile($file);$event.stopPropagation()" ngf-max-size="10MB"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                        ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser" />
                </p>
            </div>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Translations',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  