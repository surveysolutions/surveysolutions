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
            markAsError: function(message) {
                var id = this.id;
                this.$store.dispatch("setAnswerAsNotSaved", { id, message })
            },
            answerIntegerQuestion: function (evnt) {

                if (this.$me.answer == undefined ||  $(evnt.target).val().length == 0)
                {
                    this.markAsError('Empty value cannot be saved');
                    return;
                }

                if (this.$me.answer > 2147483647 || this.$me.answer < -2147483648 || this.$me.answer % 1 !== 0)
                {
                    this.markAsError('Entered value can not be parsed as integer value');
                    return;
                }

                if (this.$me.isRosterSize < 0)
                {
                    if (this.$me.answer < 0)
                    {
                        this.markAsError('Answer ' + this.$me.answer + ' is incorrect because question is used as size of roster and specified answer is negative');
                        return;
                    }

                    if (this.$me.answer > this.$me.answerMaxValue)
                    {
                        this.markAsError('Answer ' + this.$me.answer + ' is incorrect because answer is greater than Roster upper bound ' + this.$me.answerMaxValue + '.');
                        return;
                    }

                    if (this.$me.previousAnswer != undefined && this.$me.answer < this.$me.previousAnswer)
                    {
                        var amountOfRostersToRemove = this.previousAnswer - this.Answer;
                        var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';
                        this.markAsError(confirmMessage);
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
                    var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';
                    this.markAsError(confirmMessage);
                    //var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    /*if (!(await this.userInteractionService.ConfirmAsync(message)))*/
                    {
                        this.Answer = this.previousAnswer;
                        return;
                    }
                }

                this.$store.dispatch("removeAnswer", this.id)
            }
        }
    }
</script>
