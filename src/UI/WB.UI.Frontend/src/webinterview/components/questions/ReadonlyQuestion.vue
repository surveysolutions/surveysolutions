<template>
    <wb-question :question="$me" questionCssClassName="text-question" :no-comments="noComments">
        <div class="readonly-answer-group">
            <div class="container-info" :id="$me.id">
                <p>
                    <b v-if="entity.entityType == 'Gps'">
                        <a :href="getGpsUrl(entity)" target="_blank">{{ entity.answer }}</a>
                        <br />
                    </b>
                    <b v-else-if="entity.entityType == 'DateTime'" v-dateTimeFormatting v-html="entity.answer">
                    </b>
                    <b v-else>{{ entity.answer }}</b>
                    <wb-attachment :attachmentName="getAttachment(entity)" :interviewId="interviewId"
                        customCssClass="static-text-image" v-if="getAttachment(entity)" />
                </p>
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">
import { entityDetails } from '../mixins'
import { find } from 'lodash'

export default {
    name: 'ReadonlyQuestion',
    mixins: [entityDetails],
    props: ['noComments'],
    computed: {
        interviewId() {
            return this.$route.params.interviewId
        },
        entity() {
            return find(this.$store.state.webinterview.entities, d => d.identity == this.id)
        }
    },
    methods: {
        getGpsUrl(question) {
            return `http://maps.google.com/maps?q=${question.answer}`
        },
        getAttachment(question) {
            if (!question.answer) return null

            const details = this.$store.state.webinterview.entityDetails[question.identity]
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
