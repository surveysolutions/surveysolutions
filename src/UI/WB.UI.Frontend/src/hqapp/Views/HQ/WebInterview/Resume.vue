<template>
    <div class="centered-box-table">
        <div class="row-element mb-20">
            <div class="h2">
                {{ $config.model.resumeWelcome }}
            </div>
        </div>
        <div class="row-element mb-40">
            <p class="font-bold">{{ $config.model.resumeInvitation }}</p>
        </div>
        <div class="row-element mb-40">
            <p class="font-bold primary-text">{{ $t('WebInterview.ResumeInterviewDescription') }}</p>
        </div>
        <div class="additional-info-block">
            <ul class="list-unstyled">
                <li v-if="$config.model.startedDate"
                    v-dateTimeFormatting>
                    {{ $t('WebInterview.InterviewStartDateFormatText') }} 
                    <time :datetime="$config.model.startedDate"></time>
                </li>
            </ul>
        </div>

        <StartOrResumeForm :buttonTitle="$t('WebInterview.ResumeInterview')" />
    </div>

</template>

<script>

import StartOrResumeForm from './_StartOrResumeForm'

export default {
    components: {
        StartOrResumeForm,
    },
}

</script>
