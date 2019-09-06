<template>
    <main>
        <div class="container">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a href="../Assignments">{{$t('MainMenu.Assignments')}}</a>
                        </li>
                    </ol>
                </div>
            </div>
        </div>
        <div class="container-fluid">
            <div class="row">
                <div class="col-sm-8">
                    <h3>Detailed assignment info</h3>
                    <table class="table table-striped table-bordered">
                        <tbody>
                            <tr>
                                <td class="text-nowrap">{{$t('Assignments.AssignmentId')}}</td>
                                <td>{{model.id}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t('Assignments.Questionnaire')}}</td>
                                <td>{{model.questionnaire.title}} ({{$t('Assignments.QuestionnaireVersion', {version: model.questionnaire.version} )}})</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Common.Responsible")}}</td>
                                <td v-if="isInterviewerResponsible">
                                    <span class="interviewer">
                                        <a
                                            v-bind:href="interviewerProfileUrl"
                                        >{{model.responsible.name}}</a>
                                    </span>
                                </td>
                                <td v-else>
                                    <span class="supervisor">{{model.responsible.name}}</span>
                                </td>
                            </tr>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">{{$t("Assignments.Size")}}</td>
                                <td>{{quantity}}</td>
                            </tr>
                            <tr v-if="model.isHeadquarters">
                                <td class="text-nowrap">{{$t("Assignments.Count")}}</td>
                                <td>
                                    <a v-bind:href="interviewsUrl">{{model.interviewsProvided}}</a>
                                </td>
                            </tr>
                            <tr v-if="!model.isHeadquarters">
                                <td class="text-nowrap">{{$t("Assignments.InterviewsNeeded")}}</td>
                                <td>{{interviewsCount}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Assignments.IdentifyingQuestions")}}</td>
                                <td>
                                    <div
                                        v-bind:key="question.id"
                                        v-for="question in model.identifyingData"
                                        class="overview-item"
                                    >
                                        <div class="item-content">
                                            <h4>
                                                <span>{{ question.title }}</span>
                                            </h4>
                                            <div class="answer">
                                                <div>{{question.answer}}</div>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Assignments.UpdatedAt")}}</td>
                                <td>{{updatedDate}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Assignments.CreatedAt")}}</td>
                                <td>{{createdDate}}</td>
                            </tr>
                            <tr>
                                <td
                                    class="text-nowrap"
                                >{{$t("Assignments.IsAudioRecordingEnabled")}}</td>
                                <td>{{isAudioRecordingEnabled}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Assignments.Email")}}</td>
                                <td>{{model.email}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{this.$t("Assignments.Password")}}</td>
                                <td>{{model.password}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{$t("Assignments.ReceivedByTablet")}}</td>
                                <td>{{isReceivedByTablet}}</td>
                            </tr>
                            <tr>
                                <td class="text-nowrap">{{this.$t("Assignments.WebMode")}}</td>
                                <td>{{isWebMode}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </main>
</template>


<script>
import Vue from "vue";
export default {
    computed: {
        model() {
            return this.$config.model;
        },
        updatedDate() {
            var date = moment.utc(this.model.updatedAtUtc);
            return date.local().format(global.input.settings.clientDateTimeFormat);
        },
        createdDate() {
            var date = moment.utc(this.model.createdAtUtc);
            return date.local().format(global.input.settings.clientDateTimeFormat);
        },
        isAudioRecordingEnabled() {
            return this.model.isAudioRecordingEnabled ? this.$t("Common.Yes") : this.$t("Common.No");
        },
        isReceivedByTablet() {
            return this.model.receivedByTabletAtUtc != null ? this.$t("Common.Yes") : this.$t("Common.No");
        },
        isWebMode() {
            return this.model.webMode ? this.$t("Common.Yes") : this.$t("Common.No");
        },
        interviewsCount() {
            if (this.model.quantity == null || this.model.quantity < 0) return this.$t("Assignments.Unlimited");

            return this.model.interviewsProvided > this.model.quantity
                ? 0
                : this.model.quantity - this.model.interviewsProvided;
        },
        isInterviewerResponsible() {
            return this.model.responsible.role === "interviewer";
        },
        interviewerProfileUrl() {
            return "../Interviewer/Profile/" + this.model.responsible.id;
        },
        interviewsUrl() {
            return "../Interviews?assignmentId=" + this.model.id;
        },
        quantity() {
            return this.model.quantity == null ? this.$t("Assignments.Unlimited") : this.model.quantity;
        }
    },
    mounted() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight();
            window.ajustDetailsPanelHeight();
        });
    }
};
</script>
