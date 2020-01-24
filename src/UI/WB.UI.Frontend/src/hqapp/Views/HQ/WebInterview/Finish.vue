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
            <li v-if="$config.model.startedDate">{{ $t('WebInterview.InterviewStartDateFormatText') }} <time :datetime="startedDateString"></time></li>
            <li v-if="$config.model.completedDate">{{ $t('WebInterview.InterviewCompletionDateFormatText') }} <time :datetime="completedDateString"></time></li>
        </ul>
    </div>
</div>

</template>

<script>

export default {
    computed: {
        startedDateString() {
            return this.$config.model.startedDate.toISOString()
        },
        completedDateString() {
            return this.$config.model.completedDate.toISOString()
        }
    }
}

</script>
