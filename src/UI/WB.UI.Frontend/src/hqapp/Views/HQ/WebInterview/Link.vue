<template>
    <div class="centered-box-table">
        <div class="row-element mb-20">
            <div class="h2">
                {{ $config.model.linkWelcome }}
            </div>
        </div>

        <div class="row-element mb-40">
            <p v-html="linkInvitation"></p>
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
    </div>

</template>

<script>
import { marked } from 'marked'

export default {
    computed: {
        linkInvitation() {
            return marked(this.$config.model.linkInvitation)
        },
    },
}

</script>
