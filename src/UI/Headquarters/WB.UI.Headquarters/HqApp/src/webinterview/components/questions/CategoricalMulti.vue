<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'" :noAnswer="noOptions">
        <button class="section-blocker" disabled="disabled" v-if="$me.fetching"></button>
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="form-group" v-for="option in answeredOrAllOptions" :key="$me.id + '_' + option.value"  v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" :disabled="!$me.acceptAnswer" v-model="answer" v-disabledWhenUnchecked="{maxAnswerReached: allAnswersGiven, answerNotAllowed: !$me.acceptAnswer, forceDisabled: isProtected(option.value) }">
                    <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                    <div class="badge" v-if="$me.ordered">{{ getAnswerOrder(option.value) }}</div>
                    <div class="lock"></div>
                </div>
                <button type="button" class="btn btn-link btn-horizontal-hamburger" @click="toggleOptions" v-if="shouldShowAnsweredOptionsOnly && !showAllOptions">
                    <span></span>
                </button>
                <div v-if="noOptions" class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import modal from "../modal"
    import { filter } from "lodash"
    import { shouldShowAnsweredOptionsOnlyForMulti } from "./question_helpers"

    export default {
        name: 'CategoricalMulti',
        data(){
            return {
                showAllOptions: false
            }
        },
        computed: {
            shouldShowAnsweredOptionsOnly(){
                 return shouldShowAnsweredOptionsOnlyForMulti(this);
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly)
                    return this.$me.options;
                
                var self = this;
                return filter(this.$me.options, function(o) { return self.$me.answer.indexOf(o.value) >= 0; });
            },
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    this.sendAnswer(() => {
                        this.answerMulti(value);
                    });
                }
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0;
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount;                                
            }
        },
        methods: {
            toggleOptions(){
                this.showAllOptions = !this.showAllOptions;
            },
            isProtected(answerValue) {
                if (!this.$me.protectedAnswer) return false;
                
                var answerIndex = this.$me.protectedAnswer.indexOf(answerValue)
                return answerIndex > -1;
            },
            getAnswerOrder(answerValue) {
                var answerIndex = this.$me.answer.indexOf(answerValue)
                return answerIndex > -1 ? answerIndex + 1 : ""
            },
            answerMulti(value) {
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

                const confirmMessage = this.$t("WebInterviewUI.ConfirmRosterRemove");

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
        mixins: [entityDetails]
    }

</script>
