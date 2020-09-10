<template>
    <wb-question :question="$me"
        questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group"
                v-bind:class="{ 'dotted': noOptions }">
                <div class="form-group"
                    v-if="$me.renderAsCombobox && !noOptions">
                    <div class="field"
                        :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                            :disabled="!$me.acceptAnswer"
                            :optionsSource="optionsSource"
                            :value="answer"
                            @input="sendLinkedAnswer"
                            :watermark="!$me.acceptAnswer && !$me.isAnswered ? $t('Details.NoAnswer') : null"/>
                        <wb-remove-answer />
                    </div>
                </div>
                <template v-if="!$me.renderAsCombobox">
                    <div class="radio"
                        v-for="option in answeredOrAllOptions"
                        :key="$me.id + '_' + option.value">
                        <div class="field">
                            <input class="wb-radio"
                                type="radio"
                                :id="$me.id + '_' + option.value"
                                :name="$me.id"
                                :value="option.value"
                                :disabled="!$me.acceptAnswer"
                                @input="sendLinkedAnswer(option.value)">
                            <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                            </label>
                            <wb-remove-answer :id-suffix="`_opt_${option.value}`"/>
                        </div>
                    </div>
                    <button type="button"
                        class="btn btn-link btn-horizontal-hamburger"
                        @click="toggleOptions"
                        v-if="shouldShowAnsweredOptionsOnly && !showAllOptions">
                        <span></span>
                    </button>
                </template>
                <div v-if="noOptions"
                    class="options-not-available">
                    {{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
import { entityDetails } from '../mixins'
import { isEqual } from 'lodash'
import { shouldShowAnsweredOptionsOnlyForSingle } from './question_helpers'

export default {
    name: 'LinkedSingle',
    data(){
        return {
            showAllOptions: false,
        }
    },
    computed: {
        shouldShowAnsweredOptionsOnly(){
            return shouldShowAnsweredOptionsOnlyForSingle(this)
        },
        answeredOrAllOptions(){
            if(!this.shouldShowAnsweredOptionsOnly)
                return this.$me.options

            var self = this
            return [find(this.$me.options, function(o) { return o.value == self.answer })]
        },
        answer() {

            if (this.$me.options == null || this.$me.answer == null)
                return
            return this.$me.options.find((a) => isEqual(a.rosterVector, this.$me.answer))
        },
        noOptions() {
            return this.$me.options == null || this.$me.options.length == 0
        },
    },
    mixins: [entityDetails],
    methods: {
        sendLinkedAnswer(value){
            this.sendAnswer(() => {
                var newAnswer = this.$me.options.find((a) => a.value === value)

                this.$store.dispatch('answerLinkedSingleOptionQuestion', {
                    answer: newAnswer.rosterVector,
                    identity: this.$me.id })
            })
        },
        toggleOptions(){
            this.showAllOptions = !this.showAllOptions
        },
        optionsSource(filter)
        {
            if(!filter)
                return Promise.resolve(this.$me.options)
            const search = filter.toLowerCase()
            const filtered = this.$me.options.filter(v => v.title.toLowerCase().indexOf(search) >= 0)

            return Promise.resolve(filtered)
        },
    },
}
</script>
