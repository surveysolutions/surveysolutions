<template>
    <HqLayout :hasFilter="false"
        :fixedWidth="true"
        :hasRow="false">
        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.surveySetupUrl">{{this.$t('MainMenu.SurveySetup')}}</a>
                </li>
                <li>
                    <a
                        :href="this.$config.model.listOfMyQuestionnaires">{{this.$t('QuestionnaireImport.ListOfMyQuestionnaires')}}</a>
                </li>
            </ol>
            <h1>{{this.$t('QuestionnaireImport.ImportModePageTitle')}}</h1>
        </template>
        <div class="row"
            v-if="hasQuestionnaireInfo">
            <div class="col-sm-8">
                <h2>{{this.$config.model.questionnaireInfo.name}}</h2>
            </div>
        </div>
        <div class="row questionnaire-statistics"
            v-if="hasQuestionnaireInfo">
            <div class="col-md-4 col-sm-5">
                <ul>
                    <li>
                        {{$t('QuestionnaireImport.CreatedAt')}}
                        <span>{{formatDate($config.model.questionnaireInfo.createdAt)}}</span>
                    </li>
                    <li>
                        {{$t('QuestionnaireImport.LastModifiedAt')}}
                        <span>{{formatDate($config.model.questionnaireInfo.lastUpdatedAt)}}</span>
                    </li>
                </ul>
            </div>
            <div class="col-md-4 col-sm-5">
                <ul>
                    <li>
                        <ul class="questionnaire-content">
                            <li>
                                {{$t('QuestionnaireImport.Chapters')}}:
                                <span>{{$config.model.questionnaireInfo.chaptersCount}}</span>.
                            </li>
                            <li>
                                {{$t('QuestionnaireImport.Gropus')}}:
                                <span>{{$config.model.questionnaireInfo.groupsCount}}</span>.
                            </li>
                            <li>
                                {{$t('QuestionnaireImport.Rosters')}}:
                                <span>{{$config.model.questionnaireInfo.rostersCount}}</span>.
                            </li>
                        </ul>
                    </li>
                    <li>
                        {{$t('QuestionnaireImport.Questions')}}:
                        <span>{{$config.model.questionnaireInfo.questionsCount}}</span>
                        ({{$t('QuestionnaireImport.QuestionsWithConditions', {count: $config.model.questionnaireInfo.questionsWithConditionsCount})}})
                    </li>
                </ul>
            </div>
        </div>
        <form method="post"
            ref="importingForm"
            @submit.prevent="startImport"
            class="import-questionnaire-form"
            v-if="hasQuestionnaireInfo">
            <input
                name="__RequestVerificationToken"
                type="hidden"
                :value="this.$hq.Util.getCsrfCookie()"/>
            <div v-if="$config.model.newVersionNumber > 1">
                <div class="row">
                    <div class="col-sm-8">
                        <h3>{{$t('QuestionnaireImport.QuestionnaireExists')}}</h3>
                    </div>
                </div>
                <div class="row questionnaire-versioning">
                    <div class="col-sm-8">
                        <div class="form-group">
                            <div class="radio">
                                <input
                                    class="wb-radio"
                                    type="radio"
                                    name="optionsRadios"
                                    id="imortAsNewVersion"
                                    value="new-version"
                                    checked/>
                                <label for="imortAsNewVersion">
                                    <span class="tick"></span>
                                    {{$t('QuestionnaireImport.ImportAsNewVersion', {version: $config.model.newVersionNumber})}}
                                </label>
                            </div>
                        </div>
                    </div>
                    <div v-if="this.$config.model.questionnairesToUpgradeFrom.length">
                        <div class="col-sm-8">
                            <div class="form-group">
                                <input
                                    class="checkbox-filter single-checkbox"
                                    id="ckbUpgradeAssignments"
                                    type="checkbox"
                                    value="True"
                                    name="ShouldMigrateAssignments"
                                    v-model="shouldMigrateAssignments"/>
                                <label for="ckbUpgradeAssignments">
                                    <span class="tick"></span>
                                    {{$t('QuestionnaireImport.UpgradeAssignments')}}
                                </label>
                            </div>
                        </div>

                        <div class="col-sm-8"
                            id="templateSelectorBlock"
                            v-if="shouldMigrateAssignments">
                            <input type="hidden"
                                name="migrateFrom"
                                :value="(questionnaireId||{}).key">
                            <div class="form-group">
                                <Typeahead control-id="questionnaire"
                                    noSearch
                                    noClear
                                    :placeholder="$t('Common.Questionnaire')"
                                    :values="this.$config.model.questionnairesToUpgradeFrom"
                                    :value="questionnaireId"
                                    @selected="selectQuestionnaire" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row flex-row">
                <div class="col-sm-6">
                    <div class="flex-block selection-box">
                        <div class="block">
                            <h3>{{$t('QuestionnaireImport.RegularImportTitle')}}</h3>
                            <p>{{$t('QuestionnaireImport.RegularImportSubTitle')}}</p>
                        </div>
                        <div class="block">
                            <h3>{{$t('Assignments.DetailsComments')}}</h3>
                            <p>
                                <textarea
                                    name="Comment"
                                    class="form-control"
                                    rows="5"
                                    maxlength="500"></textarea>
                            </p>
                        </div>
                        <div>
                            <button v-if="!isImporting"
                                type="submit"
                                class="btn btn-primary">{{$t('QuestionnaireImport.RegularImportTitle')}}</button>
                            <span v-if="isImporting"
                                v-text="progressText"></span>
                        </div>
                    </div>
                </div>
            </div>
        </form>
        <div class="row col-sm-12"
            v-if="errorMessage">
            <div class="alert alert-danger">
                {{errorMessage}}
            </div>
        </div>
    </HqLayout>
</template>
<script>
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'
export default {
    data() {
        return {
            shouldMigrateAssignments: false,
            questionnaireId: null,
            isImporting: false,
            progressPercent: 0,
            errorMessage: null,
            dotsCount: 0,
        }
    },
    mounted() {
        if (this.$config.model.errorMessage)
            this.errorMessage = this.$config.model.errorMessage
    },
    methods: {
        formatDate(date) {
            return new moment(date).format(DateFormats.dateTime)
        },
        selectQuestionnaire(value) {
            this.questionnaireId = value
        },
        async startImport() {
            this.isImporting = true
            this.progressPercent = 0
            this.errorMessage = ''

            var formData = new FormData(this.$refs.importingForm)
            var currentStatus = await this.$http.post(window.location, formData)

            await this.timeout(1000)

            while (currentStatus.data.status.status == 'Prepare'
                    || currentStatus.data.status.status == 'Progress'
                    || currentStatus.data.status.status == 'NotStarted') {
                this.progressPercent = currentStatus.data.status.progressPercent

                if (currentStatus.data.redirectUrl) {
                    window.location.replace(currentStatus.data.redirectUrl)
                }

                if (currentStatus.data.status.importError) {
                    this.errorMessage = currentStatus.data.status.importError
                    break
                }

                await this.timeout(1000)

                this.dotsCount++
                if (this.dotsCount > 3)
                    this.dotsCount = 1

                currentStatus = await this.$http.get(this.$config.model.checkImportingStatus + '/' + currentStatus.data.status.processId)
            }

            if (currentStatus.data.status.status == 'Error') {
                this.errorMessage = currentStatus.data.status.importError
            }

            if (currentStatus.data.redirectUrl) {
                window.location.replace(currentStatus.data.redirectUrl)
                return
            }

            this.dotsCount = 0
            this.isImporting = false
        },
        timeout(ms) {
            return new Promise(resolve => setTimeout(resolve, ms))
        },
    },
    computed: {
        hasQuestionnaireInfo() {
            return this.$config.model.questionnaireInfo != null
        },
        progressText() {
            var text = ''
            if (this.progressPercent === 0) {
                text = this.$t('QuestionnaireImport.Prepare')
            }
            else {
                text = this.$t('QuestionnaireImport.Importing', { percent: this.progressPercent })
            }
            text += '...'.substring(0, this.dotsCount)
            return text
        },
    },
}
</script>
