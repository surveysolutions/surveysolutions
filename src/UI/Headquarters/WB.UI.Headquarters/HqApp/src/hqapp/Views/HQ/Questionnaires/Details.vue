<template>
    <HqLayout :hasFilter="false" >
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
                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr>
                            <td>{{$t('Dashboard.Questionnaire')}}</td>
                            <td>{{model.title}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.Version')}}</td>
                            <td>{{model.version}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.ImportDate')}}</td>
                            <td>{{formatUtcDate(model.importDateUtc)}}</td>
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
                            <td>{{model.webMode ? $t('Common.Yes') : $t('Common.No')}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Dashboard.RecordAudio')}}</td>
                            <td>{{model.audioAudit ? $t('Common.Yes') : $t('Common.No')}}</td>
                        </tr>
                        <tr v-if="model.mainPdfUrl">
                            <td>PDF</td>
                            <td>
                                <ul class="list-unstyled">
                                    <li><a :href="model.mainPdfUrl">{{$t('WebInterview.Original_Language')}}</a></li>
                                    <li v-for="lang in model.translatedPdfVersions" v-bind:key="lang.name">
                                        <a :href="lang.pdfUrl">{{lang.name}}</a>
                                    </li>
                                </ul>
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
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'

export default {
    computed: {
        model() {
            return this.$config.model
        },
    },
    methods: {
        formatUtcDate(date) {
            const momentDate = moment.utc(date)
            return momentDate.local().format(DateFormats.dateTime)
        },
    },
}
</script>