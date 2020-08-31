<template>

    <div class="centered-box-table">
        <h2>{{ $config.model.webSurveyHeader }}</h2>
        <div class="additional-info-block">
            <ul class="list-unstyled">
                <li>
                    {{ $config.model.finishInterview }}
                </li>
                <li>
                    {{ $config.model.surveyName }}
                </li>
            </ul>
        </div>
        <div class="additional-info-block">
            <ul class="list-unstyled">
                <li v-if="$config.model.startedDate"
                    v-dateTimeFormatting>
                    {{ $t('WebInterview.InterviewStartDateFormatText') }}
                    <time :datetime="$config.model.startedDate"></time>
                </li>
                <li v-if="$config.model.completedDate"
                    v-dateTimeFormatting>
                    {{ $t('WebInterview.InterviewCompletionDateFormatText') }}
                    <time :datetime="$config.model.completedDate"></time>
                </li>
                <li v-if="$config.model.pdfUrl">
                    <a id="btn_Print"
                        class="btn btn-link gray-action-unit"
                        v-bind:href="this.$config.model.pdfUrl"
                        target="_blank"
                        download>{{$t("WebInterview.DownloadAnswers")}}</a>
                </li>
            </ul>
        </div>
    </div>

</template>

<script>

export default {

}

</script>
