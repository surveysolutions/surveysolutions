<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="../../SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
                <li>
                    <a
                        href="../../Assignments/Upload"
                    >{{$t('BatchUpload.BreadCrumbs_CreatingMultipleInterviews')}}</a>
                </li>
            </ol>
            <h1>{{$t('BatchUpload.CreatingMultipleAssignments')}}</h1>
        </div>
        <div class="row">
            <div class="col-sm-7">
                <h3>{{$t('BatchUpload.Import_VerificationOfAssignments_ForQuestionnaire', { title: $t('Pages.QuestionnaireNameFormat', { name: questionnaire.title, version: questionnaire.version }) })}}</h3>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-7 col-xs-12 action-block">
                <div class="import-progress">
                    <p
                        class="success-text"
                    >{{$t('BatchUpload.Import_VerificationOfDataFile_Succeeded')}}</p>
                </div>
                <div class="import-progress">
                    <p>
                        {{$t('BatchUpload.Import_VerificationOfAssignmentData')}}
                        <span>{{$t('BatchUpload.Import_VerificationOfDataFile_Progress', { verifiedCount: status.verifiedCount, totalCount: status.totalCount})}}</span>
                    </p>
                    <p
                        class="success-text"
                        v-if="status.verifiedCount > 0 && ((status.verifiedCount - status.failedVerifications) == 1)"
                    >{{$t('BatchUpload.Import_Verification_1_AssignmentVerified')}}</p>
                    <p
                        class="success-text"
                        v-if="status.verifiedCount > 0 && ((status.verifiedCount - status.failedVerifications) > 1)"
                    >{{$t('BatchUpload.Import_Verification_AssignmentsVerified', {count: status.verifiedCount - status.failedVerifications})}}</p>
                    <p
                        class="default-text"
                        v-if="!status.hasErrors"
                    >{{$t('BatchUpload.ImportInterviews_NoneFailed')}}</p>
                    <p
                        class="error-text"
                        v-if="status.hasErrors && status.failedVerifications == 1"
                    >{{$t('BatchUpload.Import_Verification_1_Error')}}</p>
                    <p
                        class="error-text"
                        v-if="status.hasErrors && status.failedVerifications > 1"
                    >{{$t('BatchUpload.Import_Verification_Errors', {count: status.failedVerifications})}}</p>
                </div>
                <div class="cancelable-progress">
                    <div class="progress">
                        <div
                            class="progress-bar progress-bar-success"
                            data-bind="style: { width: (100 * status.verifiedCount()/status.totalCount()) + '%' }, attr: {'aria-valuemax': status.totalCount, 'aria-valuenow': status.verifiedCount}"
                            role="progressbar"
                            aria-valuenow="@Model.Status.VerifiedCount"
                            aria-valuemin="0"
                            aria-valuemax="@Model.Status.TotalCount"
                        >
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
    computed: {
        status() {
            return this.$store.getters.upload.progress
        },
    },
}
</script>
