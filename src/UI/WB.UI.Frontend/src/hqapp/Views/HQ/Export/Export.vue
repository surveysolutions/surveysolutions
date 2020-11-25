<template>
    <HqLayout :fixedWidth="false">
        <div slot="headers">
            <h1>{{$t('DataExport.DataExport_Title')}}</h1>
            <div class="row">
                <div class="col-lg-5">
                    <p>{{$t('DataExport.ExportExplanation')}}</p>
                </div>
            </div>
        </div>
        <div class="col-md-12">
            <div class="row"
                v-if="exportServiceIsUnavailable">
                <div class="col-md-12 mb-30">
                    {{$t('DataExport.DataExport_ServiceIsNotAvailable')}}
                </div>
            </div>

            <div class="row">
                <div class="export d-flex"
                    ref="list">
                    <div class="col-md-12">
                        <form v-if="!exportServiceIsUnavailable">
                            <div class="mb-30">
                                <h3>{{$t('DataExport.FilterTitle')}}</h3>
                                <div class="d-flex mb-20 filter-wrapper">
                                    <div class="filter-column">
                                        <h5>
                                            {{$t('DataExport.SurveyQuestionnaire')}}
                                            <span class="text-danger">*</span>
                                        </h5>
                                        <div
                                            class="form-group"
                                            :class="{ 'has-error': errors.has('questionnaireId') }">
                                            <Typeahead
                                                control-id="questionnaireId"
                                                :value="questionnaireId"
                                                :placeholder="$t('Common.AllQuestionnaires')"
                                                :fetch-url="questionnaireFetchUrl"
                                                :selectedKey="pageState.id"
                                                data-vv-name="questionnaireId"
                                                data-vv-as="questionnaire"
                                                v-validate="'required'"
                                                v-on:selected="questionnaireSelected"/>
                                            <span
                                                class="help-block">{{ errors.first('questionnaireId') }}</span>
                                        </div>
                                        <div
                                            class="form-group"
                                            :class="{ 'has-error': errors.has('questionnaireVersion') }">
                                            <Typeahead
                                                noClear
                                                control-id="questionnaireVersion"
                                                ref="questionnaireVersionControl"
                                                data-vv-name="questionnaireVersion"
                                                data-vv-as="questionnaire version"
                                                :selectedKey="pageState.version"
                                                v-validate="'required'"
                                                :value="questionnaireVersion"
                                                :fetch-url="questionnaireVersionFetchUrl"
                                                v-on:selected="questionnaireVersionSelected"
                                                :disabled="questionnaireVersionFetchUrl == null"
                                                :selectFirst="true"/>
                                            <span
                                                class="help-block">{{ errors.first('questionnaireVersion') }}</span>
                                        </div>
                                        <div class="form-group"
                                            v-if="translations.length > 0">
                                            <Typeahead
                                                noClear
                                                selectFirst
                                                control-id="questionnaireTranslation"
                                                ref="questionnaireTranslation"
                                                data-vv-name="questionnaireTranslation"
                                                data-vv-as="questionnaire translation"
                                                :placeholder="$t('DataExport.QuestionnaireTranslation')"
                                                :selectedKey="pageState.translation"
                                                :value="questionnaireTranslation"
                                                :values="translations"
                                                :disabled="questionnaireVersion == null"
                                                v-on:selected="translationSelected"/>
                                        </div>
                                    </div>

                                    <div class="filter-column">
                                        <h5>{{$t('DataExport.StatusOfExportTitle')}}</h5>
                                        <Typeahead
                                            control-id="status"
                                            :selectedKey="pageState.status"
                                            data-vv-name="status"
                                            data-vv-as="status"
                                            :placeholder="$t('Common.AllStatuses')"
                                            :value="status"
                                            :values="statuses"
                                            v-on:selected="statusSelected"/>
                                    </div>
                                </div>
                            </div>
                            <div class="mb-30"
                                v-if="hasBinaryData || questionnaireVersion">
                                <h3>{{$t('DataExport.DataType')}}</h3>
                                <div class="radio-btn-row"
                                    v-if="hasInterviews">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataType"
                                        id="surveyData"
                                        v-model="dataType"
                                        value="surveyData"/>
                                    <label for="surveyData"
                                        class>
                                        <span class="tick"></span>
                                        <span
                                            class="format-data Binary">{{$t('DataExport.DataType_MainSurveyData')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row"
                                    v-if="hasInterviews && hasBinaryData">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataType"
                                        id="binaryData"
                                        v-model="dataType"
                                        value="binaryData"/>
                                    <label for="binaryData">
                                        <span class="tick"></span>
                                        <span
                                            class="format-data Binary">{{$t('DataExport.DataType_Binary')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row"
                                    v-if="hasInterviews">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataType"
                                        id="paraData"
                                        v-model="dataType"
                                        value="paraData"/>
                                    <label for="paraData">
                                        <span class="tick"></span>
                                        <span
                                            class="format-data Tabular">{{$t('DataExport.DataType_Paradata')}}</span>
                                    </label>
                                </div>
                            </div>


                            <div
                                class="mb-30"
                                v-if="dataType == 'surveyData' && questionnaireVersion">
                                <h3>{{$t('DataExport.QuestionnaireInformation')}}</h3>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="includeMeta"
                                        id="metaInclude"
                                        v-model="includeMeta"
                                        value="True"/>
                                    <label for="metaInclude">
                                        <span class="tick"></span>
                                        <span
                                            class="format-data">{{$t('DataExport.QuestionnaireInformation_Include')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="includeMeta"
                                        id="metaExclude"
                                        v-model="includeMeta"
                                        value="False"/>
                                    <label for="metaExclude"
                                        class>
                                        <span class="tick"></span>
                                        <span
                                            class="format-data no-meta">{{$t('DataExport.QuestionnaireInformation_Exclude')}}</span>
                                    </label>
                                </div>
                            </div>

                            <div
                                class="mb-30"
                                v-if="dataType == 'surveyData' && questionnaireVersion">
                                <h3>{{$t('DataExport.DataFormat')}}</h3>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataFormat"
                                        id="separated"
                                        v-model="dataFormat"
                                        value="Tabular"/>
                                    <label for="separated">
                                        <span class="tick"></span>
                                        <span
                                            class="format-data Tabular">{{$t('DataExport.DataFormat_Tab')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataFormat"
                                        id="Stata"
                                        v-model="dataFormat"
                                        value="Stata"/>
                                    <label for="Stata"
                                        class>
                                        <span class="tick"></span>
                                        <span
                                            class="format-data STATA">{{$t('DataExport.DataFormat_Stata')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="dataFormat"
                                        id="Spss"
                                        v-model="dataFormat"
                                        value="Spss"/>
                                    <label for="Spss">
                                        <span class="tick"></span>
                                        <span
                                            class="format-data SPSS">{{$t('DataExport.DataFormat_Spss')}}</span>
                                    </label>
                                </div>
                            </div>
                            <div class="mb-30"
                                v-if="canExportExternally && questionnaireVersion">
                                <h3>{{$t('DataExport.DataDestination')}}</h3>
                                <div class="radio-btn-row">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="exportDestination"
                                        id="download"
                                        v-model="dataDestination"
                                        value="zip"/>
                                    <label for="download">
                                        <span class="tick"></span>
                                        <span
                                            class="export-destination">{{$t('DataExport.DataDestination_Zip')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row"
                                    v-if="isOneDriveSetUp">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="exportDestination"
                                        id="onedrive"
                                        v-model="dataDestination"
                                        value="oneDrive"/>
                                    <label for="onedrive">
                                        <span class="tick"></span>
                                        <span
                                            class="export-destination OneDrive">{{$t('DataExport.DataDestination_OneDrive')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row"
                                    v-if="isDropboxSetUp">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="exportDestination"
                                        id="dropbox"
                                        v-model="dataDestination"
                                        value="dropbox"/>
                                    <label for="dropbox">
                                        <span class="tick"></span>
                                        <span
                                            class="export-destination Dropbox">{{$t('DataExport.DataDestination_Dropbox')}}</span>
                                    </label>
                                </div>
                                <div class="radio-btn-row"
                                    v-if="isGoogleDriveSetUp">
                                    <input
                                        class="radio-row"
                                        type="radio"
                                        name="exportDestination"
                                        id="googleDrive"
                                        v-model="dataDestination"
                                        value="googleDrive"/>
                                    <label for="googleDrive">
                                        <span class="tick"></span>
                                        <span
                                            class="export-destination GoogleDrive">{{$t('DataExport.DataDestination_GoogleDrive')}}</span>
                                    </label>
                                </div>
                            </div>
                            <div class="mb-30">
                                <div class="structure-block">
                                    <button
                                        type="button"
                                        class="btn btn-success"
                                        @click="queueExport">{{$t('DataExport.AddToQueue')}}</button>
                                    <button
                                        type="button"
                                        class="btn btn-lg btn-link"
                                        @click="resetForm">{{$t('Strings.Cancel')}}</button>
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="col-md-12"
                        v-if="!exportServiceIsUnavailable">
                        <div
                            class="no-sets"
                            v-if="!exportServiceIsUnavailable && exportResults.length == 0">
                            <p v-if="exportServiceInitializing">{{$t('Common.Loading')}}</p>
                            <p v-else>{{$t('DataExport.NoDataSets')}}</p>
                        </div>
                        <div v-else>
                            <h3 class="mb-20">
                                {{$t('DataExport.GeneratedDataSets')}}
                            </h3>
                            <p>{{$t('DataExport.GeneratedDataSets_Desc')}}</p>
                            <ExportProcessCard
                                v-for="result in exportResults"
                                v-bind:key="result.id"
                                :data="result"></ExportProcessCard>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row"
                v-if="!exportServiceIsUnavailable">
                <div class="col-lg-5">
                    <h3 class="mb-20">
                        {{$t('DataExport.DataExportApi')}}
                    </h3>
                    <p>
                        {{$t('DataExport.DataExportApiDesc')}}
                        <a
                            href="https://support.mysurvey.solutions/headquarters/api/api-for-data-export/"
                            target="_blank"
                            class="underlined-link">{{$t('DataExport.DataExportApiInfoPage')}}</a>
                    </p>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'
import ExportProcessCard from './ExportProcessCard'
import gql from 'graphql-tag'
import { filter, toNumber, map} from 'lodash'

const dataFormatNum = {Tabular: 1, Stata: 2, Spss: 3, Binary: 4, Ddi: 5, Paradata: 6}
const ExternalStorageType = {dropbox: 1, oneDrive: 2, googleDrive: 3}

export default {
    data() {
        return {
            dataType: 'surveyData',
            dataFormat: 'Tabular',
            includeMeta: 'True',
            dataDestination: 'zip',
            questionnaireId: null,
            questionnaireVersion: null,
            questionnaireTranslation: null,
            translations: [],
            status: null,
            statuses: this.$config.model.statuses,
            isUpdatingDataAvailability: false,
            hasInterviews: false,
            hasBinaryData: false,
            externalStoragesSettings: (this.$config.model.externalStoragesSettings || {}).oAuth2 || {},
            pageState: {},
            updateInProgress: false,
            jobsLoadingBatchCount: 18,
        }
    },

    computed: {
        exportResults() {
            return filter(this.$store.state.export.jobs, j => j.deleted == false)
        },
        exportServiceIsUnavailable() {
            return this.$store.state.export.exportServiceIsUnavailable
        },
        exportServiceInitializing() {
            return this.$store.state.export.exportServiceInitializing
        },
        isDropboxSetUp() {
            var settings = this.externalStoragesSettings['dropbox'] || null
            return settings != null
        },
        isOneDriveSetUp() {
            var settings = this.externalStoragesSettings['oneDrive'] || null
            return settings != null
        },
        isGoogleDriveSetUp() {
            var settings = this.externalStoragesSettings['googleDrive'] || null
            return settings != null
        },
        canExportExternally() {
            return this.$config.model.externalStoragesSettings != null
        },
        questionnaireFetchUrl() {
            return this.$config.model.api.questionnairesUrl
        },
        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.$config.model.api.questionnairesUrl}/${this.questionnaireId.key}`
            return null
        },
    },

    mounted() {
        this.updateExportStatus()
    },

    methods: {
        updateExportStatus() {
            this.$store.dispatch('getExportStatus')
        },

        resetForm() {
            this.dataType = 'surveyData'
            this.dataFormat = 'Tabular'
            this.includeMeta = 'True'
            this.dataDestination = 'zip'
            this.questionnaireId = null
            this.questionnaireVersion = null
            this.status = null
            this.hasInterviews = false
            this.hasBinaryData = false
        },

        async queueExport() {
            if (this.dataDestination != 'zip') {
                this.redirectToExternalStorage()
                return
            }

            var self = this
            var validationResult = await this.$validator.validateAll()
            if (validationResult) {
                const exportParams = self.getExportParams(
                    self.questionnaireId.key,
                    self.questionnaireVersion.key,
                    self.dataType,
                    self.dataFormat,
                    self.dataDestination,
                    self.status,
                    self.questionnaireTranslation,
                    this.includeMeta
                )

                self.$store.dispatch('showProgress')

                this.$http
                    .post(this.$config.model.api.updateSurveyDataUrl, null, {params: exportParams})
                    .then(function() {
                        self.$validator.reset()
                        self.updateExportStatus()
                    })
                    .catch(function(error) {
                        Vue.config.errorHandler(error, self)
                    })
                    .then(function() {
                        self.$store.dispatch('hideProgress')
                    })
            } else {
                var fieldName = this.errors.items[0].field
                const $firstFieldWithError = $('#' + fieldName)
                $firstFieldWithError.focus()
            }
        },

        redirectToExternalStorage() {
            const exportParams = this.getExportParams(
                this.questionnaireId.key,
                this.questionnaireVersion.key,
                this.dataType,
                this.dataFormat,
                this.dataDestination,
                this.status,
                this.questionnaireTranslation,
                this.includeMeta
            )

            var state = {
                questionnaireIdentity: {
                    questionnaireId: exportParams.id,
                    version: exportParams.version,
                },
                format: exportParams.format,
                interviewStatus: exportParams.status,
                translationId: exportParams.translationId,
                type: ExternalStorageType[this.dataDestination],
            }

            let storageSettings = this.externalStoragesSettings[this.dataDestination]

            const jsonState = JSON.stringify(state)

            var request = {
                response_type: this.externalStoragesSettings.responseType,
                redirect_uri: encodeURIComponent(this.externalStoragesSettings.redirectUri),
                client_id: storageSettings.clientId,
                state: window.btoa(
                    window.location.href + ';' + this.$config.model.api.exportToExternalStorageUrl + ';' + jsonState
                ),
                scope: storageSettings.scope,
            }

            if(this.dataDestination === 'dropbox') {
                request.token_access_type = 'offline'
            }

            if(this.dataDestination === 'googleDrive') {
                request.access_type = 'offline'
                request.prompt = 'consent'
            }

            window.location = storageSettings.authorizationUri + '?' + decodeURIComponent($.param(request))
        },

        getExportParams(questionnaireId, questionnaireVersion, dataType, dataFormat, dataDestination, statusOption, translation, includeMeta) {
            var format = dataFormatNum.Tabular

            switch (dataType) {
                case 'surveyData':
                    format = dataFormatNum[dataFormat]
                    break
                case 'binaryData':
                    format = dataFormatNum.Binary
                    break
                case 'ddiData':
                    format = dataFormatNum.Ddi
                    break
                case 'paraData':
                    format = dataFormatNum.Paradata
                    break
            }

            const status = (statusOption || {key: null}).key
            const tr = (translation || {key: null}).key

            return {
                id: questionnaireId,
                version: questionnaireVersion,
                format: format,
                status: status,
                translationId: tr,
                includeMeta: includeMeta,
            }
        },

        statusSelected(newValue) {
            this.status = newValue
        },

        questionnaireSelected(newValue) {
            this.questionnaireId = newValue
            if (!newValue) {
                this.resetDataAvalability()
            }
        },

        translationSelected(newValue) {
            this.questionnaireTranslation = newValue
        },

        async questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue
            this.translations = []
            this.questionnaireTranslation = null

            if (this.questionnaireVersion)
                this.updateDataAvalability()
            else
                this.resetDataAvalability()

            const query = gql`query questionnaires ($workspace: String!, $id: Uuid, $version: Long) {
 questionnaires(workspace: $workspace, id: $id, version: $version) {
  nodes {
    defaultLanguageName,
    translations {
        id, 
        name
    }
  }}}`
            if (newValue == null) return

            const translationsResponse = await this.$apollo.query({
                query,
                variables: {
                    id: this.questionnaireId.key,
                    version: toNumber(newValue.key),
                    workspace: this.$store.getters.workspace,
                },
                fetchPolicy: 'network-only',
            })
            const data = translationsResponse.data.questionnaires.nodes[0]
            this.translations = map(data.translations, i => {return  {key: i.id, value: i.name } })
            if(this.translations.length > 0) {
                this.translations.unshift({
                    key: null,
                    value: data.defaultLanguageName || this.$t('WebInterview.Original_Language'),
                }
                )
            }

        },
        resetDataAvalability() {
            this.hasInterviews = null
            this.hasBinaryData = null
            this.dataType = null
        },

        updateDataAvalability() {
            this.isUpdatingDataAvailability = true

            this.$http
                .get(this.$config.model.api.dataAvailabilityUrl, {
                    params: {
                        id: this.questionnaireId.key,
                        version: this.questionnaireVersion.key,
                    },
                })
                .then(response => {
                    this.hasInterviews = response.data.hasInterviews
                    this.hasBinaryData = response.data.hasBinaryData
                    if (this.dataType == null) {
                        this.dataType = this.hasInterviews ? 'surveyData' : 'ddi'
                    }
                })
                .catch(error => {
                    Vue.config.errorHandler(error, self)
                })
                .then(() => {
                    this.isUpdatingDataAvailability = false
                })
        },
    },

    components: {ExportProcessCard},
}
</script>
