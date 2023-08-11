<template>
    <main class="web-interview">
        <div class="container-fluid">
            <div class="row">
                <div class="panel panel-details contains-action-buttons">
                    <div class="panel-body clearfix">
                        <div class="about-questionnaire clearfix">
                            <div class="about-questionnaire-details clearfix">
                                <ul class="main-info-column list-unstyled pull-left">
                                    <li id="detailsInfo_qusetionnaireTitleListItem" class="questionnaire-title">
                                        [ver.{{ model.version }}]
                                        {{ model.title }}
                                        <a :href="model.designerUrl" target="_blank" v-if="model.designerUrl != null">
                                            <span :title="$t('Dashboard.ShowOnDesigner')"
                                                class="glyphicon glyphicon-link" />
                                        </a>
                                    </li>
                                    <li id="detailsInfo_interviewMode">
                                        <span class="data-label">{{ this.$t('Details.InterviewMode') }}: </span>
                                        <span>{{ model.webMode ? $t('Common.Cawi') : $t('Common.Capi') }}</span>
                                    </li>
                                </ul>
                                <ul class="list-unstyled pull-left table-info">

                                    <li id="detailsInfo_lastUpdatedListItem">
                                        <span class="data-label">{{
                                            this.$t(
                                                'Dashboard.ImportDate',
                                            )
                                        }}:</span>
                                        <span class="data">{{
                                            formatUtcDate(model.importDateUtc)
                                        }}</span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="questionnaire-details-actions clearfix">
                            <div class="buttons-container">
                                <div class="dropdown aside-menu">
                                    <button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
                                        class="btn btn-link">
                                        <span></span>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li>
                                            <a
                                                :href="model.takeNewInterviewUrl + '/' + model.questionnaireId + '$' + model.version">
                                                {{
                                                    $t('Dashboard.NewAssignment')
                                                }}
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                :href="model.batchUploadUrl + '/' + model.questionnaireId + '$' + model.version">
                                                {{
                                                    $t('Dashboard.UploadAssignments')
                                                }}
                                            </a>
                                        </li>
                                        <li>
                                            <a
                                                :href="this.$config.model.migrateAssignmentsUrl + '/' + model.questionnaireId + '?version=' + model.version">
                                                {{
                                                    $t('Dashboard.UpgradeAssignments')
                                                }}
                                            </a>
                                        </li>

                                        <li v-if="!model.isObserver">
                                            <a
                                                :href="this.$config.model.webInterviewUrl + '/' + model.questionnaireId + '$' + model.version">
                                                {{
                                                    $t('Dashboard.WebInterviewSetup')
                                                }}
                                            </a>
                                        </li>

                                        <li v-if="!model.isObserver">
                                            <a
                                                :href="this.$config.model.downloadLinksUrl + '/' + model.questionnaireId + '$' + model.version">
                                                {{
                                                    $t('Dashboard.DownloadLinks')
                                                }}
                                            </a>
                                        </li>

                                        <li v-if="!model.isObserver">
                                            <a
                                                :href="this.$config.model.sendInvitationsUrl + '/' + model.questionnaireId + '$' + model.version">
                                                {{
                                                    $t('Dashboard.SendInvitations')
                                                }}
                                            </a>
                                        </li>

                                        <li v-if="model.isAdmin">
                                            <a
                                                :href="this.$config.model.cloneQuestionnaireUrl + '/' + model.questionnaireId + '?version=' + model.version">
                                                {{
                                                    $t('Dashboard.CloneQuestionnaire')
                                                }}
                                            </a>
                                        </li>

                                        <li v-if="model.isAdmin">
                                            <a
                                                :href="this.$config.model.exportQuestionnaireUrl + '/' + model.questionnaireId + '?version=' + model.version">
                                                {{
                                                    $t('Dashboard.ExportQuestionnaire')
                                                }}
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!--  -->

                <div class="col-sm-6" style="padding-top: 30px;">
                    <table class="table table-striped table-bordered">
                        <tbody>

                            <tr>
                                <td>{{ $t('Dashboard.ImportedBy') }}</td>
                                <td>{{ model.importedBy != null ? $t('Dashboard.ImportedByText', {
                                    role: $t('Roles.' +
                                        model.importedBy.role), name: model.importedBy.name
                                }) : "" }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Dashboard.LastEntryDate') }}</td>
                                <td>{{ formatUtcDate(model.lastEntryDateUtc) }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Dashboard.CreationDate') }}</td>
                                <td>{{ formatUtcDate(model.creationDateUtc) }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Dashboard.RecordAudio') }}</td>
                                <td>
                                    <form v-on:submit.prevent="false">
                                        <div class="form-group mb-20">
                                            <input class="checkbox-filter" id="recordAudio" type="checkbox"
                                                v-model="audioAudit" @change="recordAudioChanged" />
                                            <label for="recordAudio">
                                                <span class="tick"></span>
                                            </label>
                                        </div>
                                    </form>
                                </td>
                            </tr>
                            <tr>
                                <td>{{ $t('Assignments.DetailsComments') }}</td>
                                <td>{{ model.comment }}</td>
                            </tr>
                            <tr v-if="model.mainPdfUrl">
                                <td>{{ $t('Dashboard.QuestionnairePreview') }}</td>
                                <td>
                                    <ul class="list-unstyled">
                                        <li>
                                            <a :href="model.mainPdfUrl">{{ getDefaultTranslationName() }}</a>
                                        </li>
                                        <li v-for="lang in model.translatedPdfVersions" v-bind:key="lang.name">
                                            <a :href="lang.pdfUrl">{{ lang.name }}</a>
                                        </li>
                                    </ul>
                                </td>
                            </tr>
                            <tr>
                                <td>{{ $t('Dashboard.ExposedVariables') }}</td>
                                <td>
                                    <a
                                        :href="model.exposedVariablesUrl + '/' + model.questionnaireId + '$' + model.version">
                                        {{ $t('Dashboard.Edit') }}</a>
                                </td>
                            </tr>

                        </tbody>
                    </table>
                </div>
                <div class="col-sm-6" style="padding-top: 30px;">
                    <h3>{{ $t('Pages.Questionnaire_Stats') }}</h3>
                    <table class="table table-striped table-bordered">
                        <tbody>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_Variable') }}</td>
                                <td>{{ model.variable }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_SectionsCount') }}</td>
                                <td>{{ model.sectionsCount }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_SubSectionsCount') }}</td>
                                <td>{{ model.subSectionsCount }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_RostersCount') }}</td>
                                <td>{{ model.rostersCount }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_QuestionsCount') }}</td>
                                <td>{{ model.questionsCount }}</td>
                            </tr>
                            <tr>
                                <td>{{ $t('Pages.Questionnaire_QuestionsCountWithCond') }}</td>
                                <td>{{ model.questionsWithConditionsCount }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <ModalFrame ref="audioAuditModal" :title="$t('Pages.ConfirmationNeededTitle')" :canClose="false">
                <p>{{ $t("Pages.GlobalSettings_TurningAudioAuditOn") }}</p>
                <div slot="actions">
                    <button type="button" class="btn btn-danger" v-bind:disabled="model.isObserving"
                        @click="recordAudioSend">{{
                            $t("Common.Ok") }}</button>
                    <button type="button" class="btn btn-link" data-dismiss="modal" @click="cancelSetAudio">{{
                        $t("Common.Cancel") }}</button>
                </div>
            </ModalFrame>

        </div>

    </main>
</template>

<script>
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'

import { escape, assign } from 'lodash'

import '@/assets/css/markup-web-interview.scss'
import '@/assets/css/markup-interview-review.scss'


export default {
    data() {
        return {
            audioAudit: false,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
    },
    mounted() {
        this.audioAudit = this.$config.model.audioAudit
    },
    methods: {

        assignSelected() { },
        formatUtcDate(date) {
            const momentDate = moment.utc(date)
            return momentDate.local().format(DateFormats.dateTime)
        },
        recordAudioChanged() {
            if (this.audioAudit)
                this.$refs.audioAuditModal.modal({
                    backdrop: 'static',
                    keyboard: false,
                })
            else
                return this.recordAudioSend()
        },
        async recordAudioSend() {
            const response = await this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
                .AudioAudit(this.audioAudit)
            if (response.status !== 204)
                this.audioAudit = !this.audioAudit
            this.$refs.audioAuditModal.modal('hide')
        },
        cancelSetAudio() {
            this.audioAudit = false
        },
        getDefaultTranslationName() {
            return this.model.defaultLanguageName === null ? this.$t('WebInterview.Original_Language') : this.model.defaultLanguageName
        },
    },
}
</script>
