<template>
    <wb-question :question="$me"
        questionCssClassName="multiselect-question"
        :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group"
                    v-for="(row) in this.selectedOptions"
                    :key="row.value">
                    <div class="field answered"
                        v-bind:class="{ 'unavailable-option locked-option': isProtected(row.value) }">
                        <div class="field-to-fill">
                            {{row.title}}
                        </div>
                        <button type="submit"
                            class="btn btn-link btn-clear"
                            v-if="$me.acceptAnswer && !isProtected(row.value)"
                            tabindex="-1"
                            @click="confirmAndRemoveRow(row.value)"><span></span>
                        </button>
                        <div class="lock"></div>
                    </div>
                    <wb-attachment :attachmentName="row.attachmentName"
                        :interviewId="interviewId"
                        customCssClass="static-text-image"
                        v-if="row.attachmentName" />
                </div>

                <div class="form-group">
                    <div class="field"
                        :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                            @input="appendCompboboxItem"
                            :disabled="!$me.acceptAnswer || allAnswersGiven"
                            :optionsSource="optionsSource"
                            :watermark="!$me.acceptAnswer && !$me.isAnswered ? $t('Details.NoAnswer') : null"/>
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

import { entityDetails } from '../mixins'
import Vue from 'vue'
import modal from '@/shared/modal'
import {find, map, includes, without, filter as loFilter} from 'lodash'

export default {
    name: 'MultiComboboxQuestion',
    mixins: [entityDetails],
    props: ['noComments'],
    data() {
        return {
            selectedOption: null,
        }
    },
    computed: {
        selectedOptions() {
            var self = this
            return map(self.$me.answer, (val) => {
                const option = find(self.$me.options, (opt) => { return opt.value === val })
                return {
                    title: option.title,
                    value: val,
                    attachmentName: option.attachmentName,
                }
            })
        },
        allAnswersGiven() {
            return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
        },
    },
    methods: {
        isProtected(val){
            return includes(this.$me.protectedAnswer, val)
        },
        appendCompboboxItem(newValue) {
            if(includes(this.$me.answer, newValue)) return

            let newAnswer = this.$me.answer.slice()
            newAnswer.push(newValue)
            this.$store.dispatch('answerMultiOptionQuestion', { answer: newAnswer, identity: this.$me.id })
        },
        optionsSource(filter) {
            const self = this
            const interviewId = this.$route.params.interviewId
            const excludedOptionIds = self.$me.answer
            const optionsPromise = Vue.$api.interview.get('getTopFilteredOptionsForQuestionWithExclude', {interviewId, id:this.$me.id, filter, count:20, excludedOptionIds})
            return optionsPromise
                .then(options => {
                    return loFilter(options, (o) => {
                        return !includes(self.$me.answer, o.value)
                    })
                })
        },
        confirmAndRemoveRow(valueToRemove){
            if(!includes(this.$me.answer, valueToRemove)) return

            const newAnswer = without(this.$me.answer, valueToRemove)

            if (this.$me.isRosterSize) {
                const confirmMessage = this.$t('WebInterviewUI.ConfirmRosterRemove')
                modal.confirm(confirmMessage, result => {
                    if (result) {
                        this.$store.dispatch('answerMultiOptionQuestion', { answer: newAnswer, identity: this.$me.id })
                        return
                    }
                })
            }
            else {
                this.$store.dispatch('answerMultiOptionQuestion', { answer: newAnswer, identity: this.$me.id })
            }
        },
    },
}

</script>
