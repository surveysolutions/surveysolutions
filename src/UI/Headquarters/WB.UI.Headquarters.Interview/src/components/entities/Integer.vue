<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="number" class="field-to-fill" placeholder="Tap to enter number" maxlength="10" v-model="answer" v-on:focusout="answerIntegerQuestion" >
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"

    export default {
        name: 'Integer',
        computed: {
            answer: {
                get() {
                    return this.$me.answer;
                },
                set(value) {
                    if (value != undefined && value != '') {
                        this.$me.answer = value;
                        this.$me.isAnswered = true;
                    }
                    else {
                        this.$me.answer = undefined;
                        this.$me.isAnswered = false;
                    }
                }
            }
        },
        mixins: [entityDetails],
        methods: {
            answerIntegerQuestion: function () {

                markAsError('test');
                if (this.$me.answer == undefined)
                {
                    //alert('MarkAnswerAsNotSavedWithMessage');
                    //this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_EmptyValueError);
                    return;
                }

                if (this.$me.answer > 2147483647 || this.$me.answer < -2147483648)
                {
                    //alert('MarkAnswerAsNotSavedWithMessage');
                    //this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_ParsingError);
                    return;
                }

                if (this.$me.isRosterSize < 0)
                {
                    if (this.$me.answer < 0)
                    {
                        //alert('Interview_Question_Integer_NegativeRosterSizeAnswer');
                        //var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer, this.$me.answer);
                        //this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                        return;
                    }

                    if (this.$me.answer > this.$me.answerMaxValue)
                    {
                        //alert('Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue');
                        //var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue, this.$me.answer, this.$me.answerMaxValue);
                        //this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                        return;
                    }

                    if (this.$me.previousAnswer != undefined && this.$me.answer < this.$me.previousAnswer)
                    {
                        var amountOfRostersToRemove = this.previousAnswer - this.Answer;
                        //alert('Interview_Questions_RemoveRowFromRosterMessage');
                        //var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                        //if (!(await this.userInteractionService.ConfirmAsync(message)))
                        {
                            this.Answer = this.previousAnswer;
                            return;
                        }
                    }
                }
                this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: this.answer })
            },
            removeAnswer: function() {

                if (!this.$me.isAnswered)
                {
                    this.answer = null;
                    return;
                }
                if (this.$me.isRosterSize)
                {
                    var amountOfRostersToRemove = this.$me.previousAnswer;
                    //alert('Interview_Questions_RemoveRowFromRosterMessage');
                    //var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    /*if (!(await this.userInteractionService.ConfirmAsync(message)))
                    {
                        this.Answer = this.previousAnswer;
                        return;
                    }*/
                }

                this.$store.dispatch("removeAnswer", this.id)
            },
            markAsError: function(message) {
                this.$store.dispatch("setAnswerAsNotSaved", {this.id, message})
            }
        },
        directives: {
            mask: {
                bind: (el, binding) => {
                    console.log('enter')
                    $(el).mask('0#', { byPassKeys: [188, 190] });
                    console.log('init')
                }
            }
        }
    }
</script>
