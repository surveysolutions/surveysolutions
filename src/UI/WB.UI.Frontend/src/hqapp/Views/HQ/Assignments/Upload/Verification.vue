<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="../../../SurveySetup">{{ $t('MainMenu.SurveySetup') }}</a>
                </li>
                <li>
                    <a :href="assignmentsUploadUrl">{{ $t('BatchUpload.BreadCrumbs_CreatingMultipleInterviews') }}</a>
                </li>
            </ol>
            <h1>{{ $t('BatchUpload.CreatingMultipleAssignments') }}</h1>
        </div>
        <div class="row">
            <div class="col-sm-7">
                <h3>
                    {{ $t('BatchUpload.Import_VerificationOfAssignments_ForQuestionnaire', {
        title:
            $t('Pages.QuestionnaireNameFormat', { name: questionnaire.title, version: questionnaire.version })
    }) }}
                    <router-link :to="{ name: 'questionnairedetails', params: { questionnaireId: questionnaire.id } }"
                        target='_blank'>
                        <span :title="$t('Details.ShowQuestionnaireDetails')" class="glyphicon glyphicon-link" />
                    </router-link>
                </h3>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-12 action-block">
                <div class="import-progress">
                    <p class="success-text">{{ $t('BatchUpload.Import_VerificationOfDataFile_Succeeded') }}</p>
                </div>
                <div class="import-progress">
                    <p>
                        {{ $t('BatchUpload.Import_VerificationOfAssignmentData') }}
                        <span>{{ $t('BatchUpload.Import_VerificationOfDataFile_Progress', {
        verifiedCount:
            status.verifiedCount, totalCount: status.totalCount
    }) }}</span>
                    </p>
                    <p class="success-text"
                        v-if="status.verifiedCount > 0 && ((status.verifiedCount - status.withErrorsCount) == 1)">
                        {{ $t('BatchUpload.Import_Verification_1_AssignmentVerified') }}</p>
                    <p class="success-text"
                        v-if="status.verifiedCount > 0 && ((status.verifiedCount - status.withErrorsCount) > 1)">
                        {{ $t('BatchUpload.Import_Verification_AssignmentsVerified', {
        count: status.verifiedCount -
            status.withErrorsCount
    }) }}</p>
                    <p class="default-text" v-if="status.withErrorsCount == 0">
                        {{ $t('BatchUpload.ImportInterviews_NoneFailed') }}</p>
                    <p class="error-text" v-if="status.withErrorsCount == 1">
                        {{ $t('BatchUpload.Import_Verification_1_Error') }}</p>
                    <p class="error-text" v-if="status.withErrorsCount > 1">
                        {{ $t('BatchUpload.Import_Verification_Errors', { count: status.withErrorsCount }) }}</p>
                </div>
                <div class="cancelable-progress">
                    <div class="progress">
                        <div class="progress-bar progress-bar-success"
                            v-bind:style="{ width: (100 * status.verifiedCount/status.totalCount) + '%' }"
                            role="progressbar" :aria-valuenow="status.verifiedCount" aria-valuemin="0"
                            :aria-valuemax="status.totalCount">
                            <span class="sr-only"></span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
export default {
    data: function () {
        return {
            timerId: 0,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
        questionnaire() {
            return this.model.questionnaire
        },
        questionnaireId() {
            return this.$route.params.questionnaireId
        },
        status() {
            return this.$store.getters.upload.progress
        },
        assignmentsUploadUrl() {
            return '../../../Assignments/Upload/' + this.questionnaireId
        },
    },
    mounted() {
        this.timerId = window.setInterval(() => {
            this.updateStatus()
        }, 1500)
    },
    methods: {
        updateStatus() {
            var self = this
            this.$http.get(this.model.api.importStatusUrl).then(response => {
                self.$store.dispatch('setUploadStatus', response.data)

                if (response.data == null)
                    self.$router.push({
                        name: 'assignments-upload',
                        params: { questionnaireId: self.questionnaireId },
                    })
                if (response.data.processStatus == 'Import' || response.data.processStatus == 'ImportCompleted')
                    self.$router.push({
                        name: 'assignments-upload-progress',
                        params: { questionnaireId: response.data.questionnaireIdentity.id },
                    })
            })
        },
    },
}
</script>
