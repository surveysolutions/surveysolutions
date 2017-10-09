<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'">
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="form-group" v-for="option in $me.options" :key="$me.id + '_' + option.value">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer"
                        v-disabledWhenUnchecked="allAnswersGiven">
                        <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                        <div class="badge" v-if="$me.ordered">{{getAnswerOrder(option.value)}}</div>
                </div>
                <div v-if="noOptions" class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"

    import * as map from "lodash/map"
    import * as find from "lodash/find"
    import * as isEqual from "lodash/isequal"

    export default {
        name: 'LinkedMulti',
        computed: {
            answer: {
                get() {
                    return map(this.$me.answer, (x) => { return find(this.$me.options, (a) => { return isEqual(a.rosterVector, x) }).value; })
                },
                set(value) {
                    const selectedOptions = map(value, (x) => { return find(this.$me.options, { 'value': x }).rosterVector; });
                    this.$store.dispatch("answerLinkedMultiOptionQuestion", { answer: selectedOptions, questionIdentity: this.$me.id })
                }
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        methods: {
            getAnswerOrder(answerValue) {
                var answerIndex = this.answer.indexOf(answerValue)
                return answerIndex > -1 ? answerIndex + 1 : ""
            }
        },
        mixins: [entityDetails]
    }

</script>
