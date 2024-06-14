<template>
    <HqLayout :hasFilter="false">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a href="../../../SurveySetup">{{ $t('MainMenu.SurveySetup') }}</a>
                    </li>
                    <li>
                        <a :href="assignmentsUploadUrl">{{ $t('BatchUpload.BreadCrumbs_CreatingMultipleInterviews')
                            }}</a>
                    </li>
                </ol>
                <h1>{{ $t('BatchUpload.ImportAssignments_PageTitle') }}</h1>
            </div>
        </template>
        <div class="row">
            <div class="col-sm-7">
                <h3>
                    {{ $t('BatchUpload.ImportFrom', {
                        fileName: fileName, title: $t('Pages.QuestionnaireNameFormat', {
                            name: questionnaire.title, version: questionnaire.version
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
                    <p class="error-text">{{ $t('BatchUpload.BatchFileVerificationFailed') }}</p>
                    <p>{{ $t('BatchUpload.AssignmentsAreNotCreated') }}</p>
                </div>
                <h3 class="error-text" v-if="verificationErrors.length == 1">{{ $t('BatchUpload.SingleErrorFound') }}
                </h3>
                <h3 class="error-text" v-else>{{ $t('BatchUpload.MultipleErrorsWereFound', {
                    count:
                        verificationErrors.length
                }) }}</h3>

                <div class="error-block" v-for="(error, index) in verificationErrors" :key="index">
                    <h5 class="error-text">
                        <span>{{ error.code }}</span>
                        : {{ error.message }}
                    </h5>
                    <div v-for="(reference, index) in error.references" :key="index">
                        <p>{{ $t('BatchUpload.FileName') }}: {{ reference.dataFile }}</p>
                        <p v-if="!isEmptyText(reference.content)">
                            <span v-if="reference.type == 0">{{ $t('BatchUpload.Column') }}: {{ reference.content
                                }}</span>
                            <span v-if="reference.type == 1">{{ $t('BatchUpload.Row') }}: {{ reference.content }}</span>
                            <span v-if="reference.type == 2">{{ $t('BatchUpload.Cell') }}: {{ reference.content
                                }}</span>
                            <span v-if="reference.type == 3">{{ $t('BatchUpload.File') }}: {{ reference.content
                                }}</span>

                            <span v-else>{{ reference.type }}: {{ reference.content }}</span>
                            (
                            <span v-if="!isEmptyText(reference.column)">{{ $t('BatchUpload.Column') }}:
                                {{ reference.column }},</span>
                            <span v-if="reference.row != null">{{ $t('BatchUpload.Row') }}: {{ reference.row }}</span>)
                        </p>
                    </div>
                </div>

                <div class="action-buttons">
                    <a class="back-link" v-bind:href="assignmentsUploadUrl">
                        {{ $t('BatchUpload.BackToImport') }}
                    </a>
                </div>
            </div>
        </div>
    </HqLayout>
</template>


<script>
import { isEmpty } from 'lodash'

export default {
    computed: {
        model() {
            return this.$config.model
        },
        uploadInfo() {
            return this.$store.getters.upload
        },
        fileName() {
            return this.uploadInfo.fileName
        },
        verificationErrors() {
            return this.uploadInfo.verificationErrors
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
    },
    methods: {
        isEmptyText(text) {
            return isEmpty(text)
        },
    },
}
</script>
