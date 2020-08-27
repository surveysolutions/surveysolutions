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
                <li v-if="$config.model.printPdf"
                    v-dateTimeFormatting>
                    {{ $t('WebInterview.DownloadAnswers') }}
                    <time :datetime="$config.model.pdfUrl"></time>
                </li>
            </ul>
        </div>
    </div>

</template>

<script>

export default {

}

</script>
