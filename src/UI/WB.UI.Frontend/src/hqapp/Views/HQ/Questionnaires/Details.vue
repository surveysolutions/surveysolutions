<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="../../SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('Pages.Questionnaire_Info')}}</h1>
        </div>

        <div class="row">
            <div class="col-sm-6">
                <h3>
                    {{ model.title}} (ver. {{model.version}})
                    <a
                        :href="model.designerUrl"
                        target="_blank"
                        v-if="model.designerUrl != null">
                        <span
                            :title="$t('Dashboard.ShowOnDesigner')"
                            class="glyphicon glyphicon-link"/>
                    </a>
                </h3>
                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr>
                            <td>{{$t('Dashboard.ImportDate')}}</td>
                            <td>{{formatUtcDate(model.importDateUtc)}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.ImportedBy')}}</td>
                            <td>{{model.importedBy != null ? $t('Dashboard.ImportedByText', { role: $t('Roles.' + model.importedBy.role), name: model.importedBy.name }) : ""}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.LastEntryDate')}}</td>
                            <td>{{formatUtcDate(model.lastEntryDateUtc)}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.CreationDate')}}</td>
                            <td>{{formatUtcDate(model.creationDateUtc)}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.WebMode')}}</td>
                            <td>{{model.webMode ? $t('Common.Cawi') : $t('Common.Capi')}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.RecordAudio')}}</td>
                            <td>
                                <form v-on:submit.prevent="false">
                                    <div class="form-group mb-20">
                                        <input
                                            class="checkbox-filter"
                                            id="recordAudio"
                                            type="checkbox"
                                            v-model="audioAudit"
                                            @change="recordAudioChanged"/>
                                        <label for="recordAudio">
                                            <span class="tick"></span>
                                        </label>
                                    </div>
                                </form>
                            </td>
                        </tr>
                        <tr>
                            <td>{{$t('Assignments.DetailsComments')}}</td>
                            <td>{{model.comment}}</td>
                        </tr>
                        <tr v-if="model.mainPdfUrl">
                            <td>PDF</td>
                            <td>
                                <ul class="list-unstyled">
                                    <li>
                                        <a :href="model.mainPdfUrl">{{getDefaultTranslationName()}}</a>
                                    </li>
                                    <li
                                        v-for="lang in model.translatedPdfVersions"
                                        v-bind:key="lang.name">
                                        <a :href="lang.pdfUrl">{{lang.name}}</a>
                                    </li>
                                </ul>
                            </td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.ExposedVariables')}}</td>
                            <td>
                                <a :href="model.exposedVariablesUrl + '/' + model.questionnaireId + '$' + model.version">
                                    {{$t('Dashboard.Edit')}}</a>
                            </td>
                        </tr>

                    </tbody>
                </table>
            </div>
            <div class="col-sm-6">
                <h3>{{$t('Pages.Questionnaire_Stats')}}</h3>
                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_Variable')}}</td>
                            <td>{{model.variable}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_SectionsCount')}}</td>
                            <td>{{model.sectionsCount}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_SubSectionsCount')}}</td>
                            <td>{{model.subSectionsCount}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_RostersCount')}}</td>
                            <td>{{model.rostersCount}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_QuestionsCount')}}</td>
                            <td>{{model.questionsCount}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.Questionnaire_QuestionsCountWithCond')}}</td>
                            <td>{{model.questionsWithConditionsCount}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <ModalFrame ref="audioAuditModal"
            :title="$t('Pages.ConfirmationNeededTitle')"
            :canClose="false">
            <p>{{ $t("Pages.GlobalSettings_TurningAudioAuditOn" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    v-bind:disabled="model.isObserving"
                    @click="recordAudioSend">{{ $t("Common.Ok") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    @click="cancelSetAudio">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

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
            if(response.status !== 204)
                this.audioAudit = !this.audioAudit
            this.$refs.audioAuditModal.modal('hide')
        },
        cancelSetAudio() {
            this.audioAudit = false
        },
        getDefaultTranslationName(){
            return this.model.defaultLanguageName === null ? this.$t('WebInterview.Original_Language') : this.model.defaultLanguageName
        },
    },
}
</script>
