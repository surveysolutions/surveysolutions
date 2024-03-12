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
                    {{ $t('BatchUpload.ImportAssignmentsFor', {
        title: $t('Pages.QuestionnaireNameFormat', {
            name:
                questionnaire.title, version: questionnaire.version
        })
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
                    <p v-if="isInProgress">
                        {{ $t('BatchUpload.Importing') }}
                        {{ $t('BatchUpload.ImportProgressFormat', {
        createdCount: status.processedCount,
        totalCount: status.totalCount
    }) }}
                    </p>
                    <p v-if="!isInProgress">{{ $t('BatchUpload.ImportInterviews_Done') }}</p>
                    <p class="success-text" v-if="processedWithoutErrorsCount == 1">
                        {{ $t('BatchUpload.SingleAssignmentCreated') }}</p>
                    <p class="success-text" v-if="processedWithoutErrorsCount > 1">
                        {{ $t('BatchUpload.MultipleAssignmentsCreated', { count: processedWithoutErrorsCount }) }}</p>
                    <p class="default-text" v-if="status.withErrorsCount == 0">
                        {{ $t('BatchUpload.ImportInterviews_NoneFailed') }}</p>
                    <p class="error-text" v-if="status.withErrorsCount == 1">
                        {{ $t('BatchUpload.SingleAssignmentFailedToBeCreated') }}</p>
                    <p class="error-text" v-if="status.withErrorsCount > 1">
                        {{ $t('BatchUpload.MultipleAssignmentFailedToBeCreated', { count: status.withErrorsCount }) }}</p>
                </div>
                <a v-if="!isInProgress && status.withErrorsCount > 0"
                    :href="model.api.invalidAssignmentsUrl">{{ $t('BatchUpload.DownloadInvalidAssignments') }}</a>
                <div class="cancelable-progress" v-if="isInProgress">
                    <div class="progress">
                        <div class="progress-bar progress-bar-success"
                            v-bind:style="{ width: (100 * status.processedCount / status.totalCount) + '%' }"
                            role="progressbar" :aria-valuenow="status.processedCount" aria-valuemin="0"
                            :aria-valuemax="status.totalCount">
                            <span class="sr-only"></span>
                        </div>
                    </div>
                </div>
                <div class="action-buttons" v-else>
                    <a class="btn btn-primary" href="../../../Assignments">{{ $t('MainMenu.Assignments') }}</a>
                    <a class="btn btn-primary" href="../../../SurveySetup">{{ $t('MainMenu.SurveySetup') }}</a>

                    <a class="back-link" :href="assignmentsUploadUrl">{{$t('BatchUpload.BackToImport')}}</a>
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
        uploadInfo() {
            return this.$store.getters.upload
        },
        status() {
            return this.uploadInfo.progress
        },
        questionnaire() {
            return this.model.questionnaire
        },
        questionnaireId() {
            return this.$route.params.questionnaireId
        },
        assignmentsUploadUrl() {
            return '../../../Assignments/Upload/' + this.questionnaireId
        },
        isInProgress() {
            return this.status.processStatus != 'ImportCompleted'
        },
        processedWithoutErrorsCount() {
            return (this.isInProgress ? this.status.processedCount : this.status.totalCount) - this.status.withErrorsCount
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

                if (!self.isInProgress) window.clearInterval(self.timerId)
                if (response.data == null)
                    self.$router.push({
                        name: 'assignments-upload',
                        params: { questionnaireId: self.questionnaireId },
                    })
            })
        },
    },
}
</script>
