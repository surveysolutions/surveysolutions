<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="option in $me.options">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer"
                        v-disabledWhenUnchecked="allAnswersGiven">
                    <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                    <div class="badge" v-if="$me.ordered">{{getAnswerOrder(option.value)}}</div>
                </div>
                <div v-if="noOptions">Options will be available after answering referenced question</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    import * as map from "lodash/map"
    import * as find from "lodash/find"

    export default {
        name: 'LinkedMulti',
        computed: {
            answer: {
                get() {
                    return map(this.$me.answer, (x) => { return find(this.$me.options, { 'rosterVector': x }).value; })
                },
                set(value) {
                    const selectedOptions = map(value, (x) => { return find(this.$me.options, { 'value': x }).rosterVector; });

                    if (this.$me.isLinkedToList){
                        this.$store.dispatch("answerLinkedToListMultiQuestion", { answer: map(selectedOptions, (x) => { return x[0]; }), questionIdentity: this.$me.id })
                    }
                    else{
                        this.$store.dispatch("answerLinkedMultiOptionQuestion", { answer: selectedOptions, questionIdentity: this.$me.id })
                    }

                    return;
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
            getAnswerOrder(answerValue){
                var answerIndex = this.answer.indexOf(answerValue)
                return  answerIndex > -1 ? answerIndex + 1 : ""
            }
        },
        mixins: [entityDetails]
    }
</script>
