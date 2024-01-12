<template>
    Metadata

    <!--div class="metadata">
        <perfect-scrollbar class="scroller">
            <h3 ng-i18next="SideBarMetadataHeader"></h3>

            <form role="form" name="metadataForm" novalidate>
                <ng-form name="metadata.form">

                    <ul class="list-unstyled metadata-blocks">
                        <li>
                            <h4>{{ 'SideBarMetadataBasicInfo' | i18next }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataTitle' | i18next }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="title" required="" minlength="1"
                                        ng-model="metadata.title" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataSubtitle' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="subTitle"
                                        ng-model="metadata.subTitle"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataVersionIdentificator' | i18next }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="version" ng-model="metadata.version">
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataVersionNotes' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="versionNotes"
                                        ng-model="metadata.versionNotes"></textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ 'SideBarMetadataSurveyDataInfo' | i18next }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataStudyTypes' | i18next }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.studyType }">
                                        <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown"
                                            aria-haspopup="true" aria-expanded="true">
                                            {{ metadata.studyType ? (questionnaire.studyTypes | filter :
                                                { 'code': metadata.studyType })[0].title : 'SelectStudyType' | i18next }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.studyType = null; metadata.form.$setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li ng-repeat="studyType in questionnaire.studyTypes">
                                                        <a @click="metadata.studyType = studyType.code; metadata.form.$setDirty();"
                                                            value="{{studyType.code}}" href="#">{{ studyType.title }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div>
                                    <input type="hidden" class="form-control" name="studyType"
                                        ng-model="metadata.studyType" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataKindOfData' | i18next }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.kindOfData }">
                                        <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown"
                                            aria-haspopup="true" aria-expanded="true">
                                            {{ metadata.kindOfData ? (questionnaire.kindsOfData | filter :
                                                { 'code': metadata.kindOfData })[0].title : 'SelectKindOfData' | i18next }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.kindOfData = null; metadata.form.$setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                            <ul class="scroller list-unstyled">
                                                <li ng-repeat="kindOfData in questionnaire.kindsOfData">
                                                    <a @click="metadata.kindOfData = kindOfData.code; metadata.form.$setDirty();"
                                                        value="{{kindOfData.code}}" href="#">{{ kindOfData.title }}</a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" name="kindOfData"
                                    ng-model="metadata.kindOfData" />
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataModeOfDataCollection' | i18next }}</span>

                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.modeOfDataCollection }">
                                        <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown"
                                            aria-haspopup="true" aria-expanded="true">
                                            {{ metadata.modeOfDataCollection ? (questionnaire.modesOfDataCollection
                                                | filter : { 'code': metadata.modeOfDataCollection })[0].title :
                                            'SelectModeOfDataCollection' | i18next }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.modeOfDataCollection = null; metadata.form.$setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu " aria-labelledby="dropdownMenu12">
                                            <ul class="scroller list-unstyled">
                                                <li ng-repeat="modeOfData in questionnaire.modesOfDataCollection">
                                                    <a @click="metadata.modeOfDataCollection = modeOfData.code; metadata.form.$setDirty();"
                                                        value="{{modeOfData.code}}" href="#">{{ modeOfData.title }}</a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" id="modeOfDataCollection"
                                    name="modeOfDataCollection" ng-model="metadata.modeOfDataCollection" />
                            </div>
                        </li>
                        <li>
                            <h4>{{ 'SideBarMetadataSurveyInfo' | i18next }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataCountry' | i18next }}</span>
                                <div class="form-group">
                                    <div class="btn-group dropdown" :class="{ 'has-value': metadata.country }">
                                        <button class="btn btn-default dropdown-toggle" type="button" data-toggle="dropdown"
                                            aria-haspopup="true" aria-expanded="true">
                                            {{ metadata.country ? (questionnaire.countries | filter :
                                                { 'code': metadata.country })[0].title : 'SelectCountry' | i18next }}
                                            <span class="dropdown-arrow" aria-labelledby="dropdownMenu12"></span>
                                        </button>
                                        <button type="button" class="btn btn-link btn-clear"
                                            @click="metadata.country = null; metadata.form.$setDirty();">
                                            <span></span>
                                        </button>
                                        <div class="dropdown-menu" aria-labelledby="dropdownMenu12">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li ng-repeat="country in questionnaire.countries">
                                                        <a @click="metadata.country = country.code; metadata.form.$setDirty();"
                                                            value="{{country.code}}" href="#">{{ country.title }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div>
                                </div>
                                <input type="hidden" class="form-control" name="country" ng-model="metadata.country" />
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataYear' | i18next }}</span>
                                <div class="form-group">
                                    <input type="text" min="0" max="9999" pattern="\d*"
                                        onkeypress='return event.charCode >= 48 && event.charCode <= 57' maxlength="4"
                                        ng-pattern="/^\d+$/" class="form-control date-field" name="year"
                                        ng-model="metadata.year" />
                                </div>
                                <p class="help-block ng-cloak" ng-show="metadata.form.year.$error.pattern"
                                    ng-i18next="QuestionOnlyInts">
                                </p>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataLanguages' | i18next }}</span>
                                <div class="form-group">
                                    <input type="text" class="form-control" name="language" ng-model="metadata.language" />
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataUnitOfAlalysis' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="unitOfAnalysis"
                                        ng-model="metadata.unitOfAnalysis"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataCoverage' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="coverage"
                                        ng-model="metadata.coverage"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataUniverse' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="universe"
                                        ng-model="metadata.universe"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataPrimaryInvestigator' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="primaryInvestigator"
                                        ng-model="metadata.primaryInvestigator"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataConsultants' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="consultant"
                                        ng-model="metadata.consultant">
                                            </textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataFunding' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="funding" ng-model="metadata.funding">
                                            </textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ 'SideBarMetadataAdditionalInfo' | i18next }}</h4>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataNotes' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="notes"
                                        ng-model="metadata.notes"></textarea>
                                </div>
                            </div>
                            <div class="field-wrapper">
                                <span class="label-title">{{ 'SideBarMetadataKeywords' | i18next }}</span>
                                <div class="form-group">
                                    <textarea class="form-control msd-elastic" name="keywords"
                                        ng-model="metadata.keywords"></textarea>
                                </div>
                            </div>
                        </li>
                        <li>
                            <h4>{{ 'SideBarMetadataQuestionnaireAccess' | i18next }}</h4>
                            <div class="checkbox">
                                <input id="agreeToMakeThisQuestionnairePublic" name="agreeToMakeThisQuestionnairePublic"
                                    ng-model="metadata.agreeToMakeThisQuestionnairePublic" type="checkbox"
                                    class="checkbox-filter" checked>
                                <label for="agreeToMakeThisQuestionnairePublic" class=""><span class="tick"></span>{{
                                    'SideBarMetadataAgreeToMakeThisQuestionnairePublic' |
                                    i18next }}</label>
                            </div>
                        </li>
                    </ul>
                    <div class="form-buttons-holder" :class="{ dirty: metadata.form.$dirty }">
                        <button type="submit" class="btn btn-lg ng-isolate-scope"
                            ng-disabled="questionnaire.isReadOnlyForUser || metadata.form.$invalid"
                            :class="{ 'btn-primary': metadata.form.$dirty }"
                            @click="saveMetadata(); $event.stopPropagation()" ng-i18next>Save</button>
                        <button type="button" class="btn btn-lg btn-link ng-isolate-scope"
                            @click="cancelMetadata(); $event.stopPropagation()" ng-i18next>Cancel</button>
                    </div>
                </ng-form>
            </form>
        </perfect-scrollbar>

    </div-->
</template>
  
<script>
export default {
    name: 'Metadata',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  