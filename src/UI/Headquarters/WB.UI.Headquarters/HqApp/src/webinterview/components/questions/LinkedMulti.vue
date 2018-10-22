<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'">
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="form-group" v-for="option in answeredOrAllOptions" :key="$me.id + '_' + option.value">
                    <input class="wb-checkbox" type="checkbox" 
                        :id="$me.id + '_' + option.value" 
                        :name="$me.id" 
                        :value="option.value" 
                        :disabled="!$me.acceptAnswer"
                        v-model="answer"
                        v-disabledWhenUnchecked="{maxAnswerReached: allAnswersGiven, answerNotAllowed: !$me.acceptAnswer}">
                        <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                        <div class="badge" v-if="$me.ordered">{{getAnswerOrder(option.value)}}</div>
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

    import { map, find, isEqual, filter } from "lodash";
    
    export default {
        name: 'LinkedMulti',
        data(){
            return {
                showAllOptions: false
            }
        },
        computed: {
            shouldShowAnsweredOptionsOnly(){
                var isSupervisorOnsupervisorQuestion = this.$me.isForSupervisor && !this.$me.isDisabled;
                return !isSupervisorOnsupervisorQuestion && !this.showAllOptions && this.$store.getters.isReviewMode && !this.noOptions && this.$me.answer.length > 0 && 
                        this.$me.answer.length < this.$me.options.length;
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly)
                    return this.$me.options;
                
                var self = this;
                return filter(this.$me.options, function(o) {
                        return find(self.$me.answer, (a) => {
                            return isEqual(a, o.rosterVector)
                            });               
                     });
            },
            answer: {
                get() {
                    return map(this.$me.answer, (x) => { return find(this.$me.options, (a) => { return isEqual(a.rosterVector, x) }).value; })
                },
                set(value) {
                    this.sendAnswer(() => {
                        const selectedOptions = map(value, (x) => { return find(this.$me.options, { 'value': x }).rosterVector; });
                        this.$store.dispatch("answerLinkedMultiOptionQuestion", { answer: selectedOptions, questionIdentity: this.$me.id })
                    })
                }
            },
            allAnswersGiven() {
                const maxReached = this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount;
                return maxReached;               
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        methods: {
            getAnswerOrder(answerValue) {
                var answerIndex = this.answer.indexOf(answerValue)
                return answerIndex > -1 ? answerIndex + 1 : ""
            },
            toggleOptions(){
                this.showAllOptions = !this.showAllOptions;
            },
        },
        mixins: [entityDetails]
    }

</script>
