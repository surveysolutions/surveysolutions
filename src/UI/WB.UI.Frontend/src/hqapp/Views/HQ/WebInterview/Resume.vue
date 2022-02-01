<template>
    <div class="centered-box-table">
        <div class="row-element mb-20">
            <div class="h2">
                {{ $config.model.resumeWelcome }}
            </div>
        </div>
        <div class="row-element mb-40">
            <p v-html="resumeInvitation"></p>
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

        <StartOrResumeForm :buttonTitle="$config.model.resumeButton" />
    </div>

</template>

<script>

import { marked } from 'marked'
import StartOrResumeForm from './_StartOrResumeForm'

export default {
    components: {
        StartOrResumeForm,
    },
    computed: {
        resumeInvitation() {
            return marked(this.$config.model.resumeInvitation)
        },
    },
}

</script>
