<template>
    <div :class='questionStyle' >
        <div class="cell-bordered d-flex" style="align-items:center;"
          v-for="option in editorParams.question.options"
          :key="$me.id + '_' + option.value"
          v-bind:class="{ 'unavailable-option locked-option': isProtected(option.value) }">
        <div style="width:220px;text-align: center;">
          <input
            v-if="answeredOrAllOptions.some(e => e.value === option.value)"
            class="wb-checkbox"
            type="checkbox"
            :id="$me.id + '_' + option.value"
            :name="$me.id"
            :value="option.value"
            :disabled="disabled"
            v-model="answer"
            @change="change"
            v-disabledWhenUnchecked="{
                                maxAnswerReached: allAnswersGiven,
                                answerNotAllowed: !$me.acceptAnswer,
                                forceDisabled: isProtected(option.value) }"/>
          <label :for="$me.id + '_' + option.value">
            <span class="tick"></span>            
          </label>          
          </div>
        </div>
      </div>
</template>

<script lang="js">
    import Vue from 'vue'
    import { filter } from "lodash"
    import { entityDetails, tableCellEditor } from "../mixins"
    import { shouldShowAnsweredOptionsOnlyForSingle } from "./question_helpers"

    export default {
        name: 'MatrixRoster_CategoricalMulti',
        mixins: [entityDetails, tableCellEditor],
        
        data() {
            return {
                showAllOptions: false,
                question: null,
                answer: [],
                lastUpdate: null,
                questionId: null
            }
        }, 
        watch: {
            ["$watchedQuestion"](watchedQuestion) {
                if (watchedQuestion.updatedAt != this.lastUpdate) {
                    this.question = watchedQuestion
                    this.cacheQuestionData()
                }
            }
        },
        computed: {
            $watchedQuestion() {
                return this.$store.state.webinterview.entityDetails[this.questionId] 
            },
            shouldShowAnsweredOptionsOnly(){
                return shouldShowAnsweredOptionsOnlyForSingle(this);
            },
            disabled() {
                if (this.$me.isDisabled || this.$me.isLocked || !this.$me.acceptAnswer)
                    return true
                return false
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly){
                    return this.$me.options;
                }
                
                var self = this;
                return filter(this.$me.options, function(o) { 
                    return self.$me.answer.indexOf(o.value) >= 0; 
                });
            },
            allAnswersGiven() {
                return this.$me.maxSelectedAnswersCount && this.$me.answer.length >= this.$me.maxSelectedAnswersCount;
            },
            questionStyle() {
                return [{
                    'disabled-question' : this.question.isDisabled,
                    'has-error' : !this.question.validity.isValid,
                    'has-warnings' : this.question.validity.warnings.length > 0,
                    'not-applicable' : this.question.isLocked,
                    'syncing': this.isFetchInProgress
                }, 'cell-unit', 'options-group', ' h-100',' d-flex']
            } 
        },
        methods: {
            cacheQuestionData() {
                this.lastUpdate = this.question.updatedAt
            },
            change() {
                this.sendAnswer(() => {
                    this.answerMulti(this.answer);
                });
            },
            //questionId()  {
            //    return this.params.value.identity
            //},
            answerMulti(value) {
                if (!this.$me.isRosterSize) {
                    this.$store.dispatch("answerMultiOptionQuestion", { answer: value, identity: this.$me.id })
                    return;
                }

                const currentAnswerCount = value.length;
                const previousAnswersCount = this.$me.answer.length;
                const isNeedRemoveRosters = currentAnswerCount < previousAnswersCount;

                if (!isNeedRemoveRosters) {
                    this.$store.dispatch('answerMultiOptionQuestion', { answer: value, identity: this.$me.id });
                    return;
                }

                const diff = _.difference(this.$me.answer, value)
                const rosterTitle = _.join(diff.map(v => {
                    return _.find(this.answeredOrAllOptions, { value: v }).title
                }), ', ')

                const confirmMessage = this.$t("WebInterviewUI.Interview_Questions_RemoveRowFromRosterMessage", {
                    rosterTitle
                } );

                modal.confirm(confirmMessage, result => {
                    if (result) {
                        this.$store.dispatch("answerMultiOptionQuestion", { answer: value, identity: this.$me.id })
                        return;
                    } else {
                        this.fetch()
                        return
                    }
                })
            },
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
        },
        created() {
            this.questionId = this.editorParams.value.identity
            this.question = this.$watchedQuestion
            this.cacheQuestionData()
        },
        mounted() {
            this.answer = this.$me.answer
        }
    }
</script>











