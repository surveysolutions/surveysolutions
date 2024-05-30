<template>
    <wb-question :question="$me" questionCssClassName="text-question" :no-comments="noComments">
        <div class="readonly-answer-group">
            <div class="container-info" :id="$me.identity">
                <p>
                    {{ $me.identity }}
                    <b v-if="$me.entityType == 'Gps'">
                        <a :href="getGpsUrl($me)" target="_blank">{{ $me.answer }}</a>
                        <br />
                    </b>
                    <b v-else-if="$me.entityType == 'DateTime'" v-dateTimeFormatting v-html="$me.answer">
                    </b>
                    <b v-else>{{ $me.answer }}</b>
                    <wb-attachment :attachmentName="getAttachment($me)" :interviewId="interviewId"
                        customCssClass="static-text-image" v-if="getAttachment($me)" />
                </p>
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">
import { entityDetails } from '../mixins'

export default {
    name: 'ReadonlyQuestion',
    mixins: [entityDetails],
    props: ['noComments'],
    computed: {
        interviewId() {
            return this.$route.params.interviewId
        },
    },
    methods: {
        getGpsUrl(question) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        getAttachment(question) {
            if (!question.answer) return null

            const details =
                this.$store.state.webinterview.entityDetails[question.identity]
            if (details && details.options) {
                const option = details.options.find(
                    (o) => o.value === details.answer,
                )
                if (option) return option.attachmentName
            }

            return null
        },
    },
}

</script>
