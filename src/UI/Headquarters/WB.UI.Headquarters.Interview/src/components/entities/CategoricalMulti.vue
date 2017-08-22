<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'" :noAnswer="noOptions">
        <button class="section-blocker" disabled="disabled" v-if="$me.fetching"></button>
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="form-group" v-for="option in $me.options" :key="$me.id + '_' + option.value">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer"
                        v-disabledWhenUnchecked="allAnswersGiven">
                        <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                        <div class="badge" v-if="$me.ordered">{{ getAnswerOrder(option.value) }}</div>
                </div>
                <div v-if="noOptions" class="options-not-available">{{ $t("OptionsAvailableAfterAnswer") }}</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"
    import modal from "../../modal"

    export default {
        name: 'CategoricalMulti',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    if (!this.$me.isRosterSize) {
                        this.$store.dispatch("answerMultiOptionQuestion", { answer: value, questionId: this.$me.id })
                        return;
                    }

                    const currentAnswerCount = value.length;
                    const previousAnswersCount = this.$me.answer.length;
                    const isNeedRemoveRosters = currentAnswerCount < previousAnswersCount;

                    if (!isNeedRemoveRosters) {
                        this.$store.dispatch('answerMultiOptionQuestion', { answer: value, questionId: this.$me.id });
                        return;
                    }

                    const confirmMessage = this.$t("ConfirmRosterRemove");

                    modal.confirm(confirmMessage, result => {
                        if (result) {
                            this.$store.dispatch("answerMultiOptionQuestion", { answer: value, questionId: this.$me.id })
                            return;
                        } else {
                            this.fetch()
                            return
                        }
                    })
                }
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount
            }
        },
        methods: {
            getAnswerOrder(answerValue) {
                var answerIndex = this.$me.answer.indexOf(answerValue)
                return answerIndex > -1 ? answerIndex + 1 : ""
            }
        },
        mixins: [entityDetails]
    }

</script>
