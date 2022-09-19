<template>
    <wb-question :question="$me"
        questionCssClassName="single-select-question"
        :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field"
                        :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                            :value="$me.answer"
                            :disabled="!$me.acceptAnswer"
                            :optionsSource="optionsSource"
                            @input="answerComboboxQuestion"
                            :watermark="!$me.acceptAnswer && !$me.isAnswered ? $t('Details.NoAnswer') : null"/>
                        <wb-remove-answer />
                    </div>
                    <wb-attachment :attachmentName="$me.answer.attachmentName"
                        :interviewId="interviewId"
                        v-if="$me.isAnswered && $me.answer.attachmentName" />
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

import { entityDetails } from '../mixins'
import Vue from 'vue'

export default {
    name: 'ComboboxQuestion',
    mixins: [entityDetails],
    props: ['noComments'],
    data() {
        return {
            answer: null,
        }
    },

    watch: {
        answer(newValue) {
            this.answerComboboxQuestion(newValue)
        },
    },

    methods: {
        answerComboboxQuestion(newValue) {
            this.sendAnswer(() => {
                this.$store.dispatch('answerSingleOptionQuestion', { answer: newValue, identity: this.$me.id })
            })
        },

        optionsSource(filter) {
            const interviewId = this.$route.params.interviewId
            return Vue.$api.interview.get('getTopFilteredOptionsForQuestion', {interviewId, id:this.$me.id, filter, count:50})
        },
    },
}

</script>
