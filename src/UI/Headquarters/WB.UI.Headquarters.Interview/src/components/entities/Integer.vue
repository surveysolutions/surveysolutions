<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" min="-2147483648" max="2147483647" placeholder="Tap to enter number" v-model="answer" @blur="answerIntegerQuestion" v-onlyNumbers="true">
                        <button v-if="$me.isAnswered" type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
                            <span></span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"
    import * as bootbox from "bootbox"

    export default {
        name: 'Integer',
        created: function () {
            this.$me.previousAnswer = this.$me.answer;
        },
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
                        this.$me.answer = null;
                        this.$me.isAnswered = false;
                    }
                }
            }
        },
        mixins: [entityDetails],
        methods: {
            markAnswerAsNotSavedWithMessage: function(message) {
                var id = this.id;
                this.$store.dispatch("setAnswerAsNotSaved", { id, message })
            },
            answerIntegerQuestion: function (evnt) {

                if (this.answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved');
                    return;
                }

                if (this.answer > 2147483647 || this.answer < -2147483648 || this.answer % 1 !== 0)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as integer value');
                    return;
                }

                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: this.answer });
                    return;
                }


                if (this.answer < 0)
                {
                    this.markAnswerAsNotSavedWithMessage('Answer ' + this.answer + ' is incorrect because question is used as size of roster and specified answer is negative');
                    return;
                }

                if (this.answer > this.$me.answerMaxValue)
                {
                    this.markAnswerAsNotSavedWithMessage('Answer ' + this.answer + ' is incorrect because answer is greater than Roster upper bound ' + this.$me.answerMaxValue + '.');
                    return;
                }

                var isNeedRemoveRosters = this.$me.previousAnswer != undefined && this.answer < this.$me.previousAnswer;

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: this.answer });
                    return;
                }

                var amountOfRostersToRemove = this.$me.previousAnswer - this.answer;
                var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';

                bootbox.confirm(confirmMessage, function (result) {
                    if (result) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: this.answer })
                    } else {
                        this.answer = this.$me.previousAnswer;
                        return;
                    }
                } );
            },
            removeAnswer: function() {

                if (!this.$me.isAnswered)
                {
                    this.answer = null;
                    return;
                }
                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch("removeAnswer", this.id);
                    return;
                }

                var amountOfRostersToRemove = this.$me.previousAnswer;
                var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';
                bootbox.confirm(confirmMessage, function (result) {
                    if (result) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: this.answer })
                    } else {
                        this.answer = this.$me.previousAnswer;
                    }
                });
            }
        },
        directives: {
            onlyNumbers: {
                bind: (el, binding) => {
                     if (binding.value == true) {
                        $(el).autoNumeric('init', {
                            aSep: '', //this.$me.useFormatting ? ',' : '',
                            mDec: 0,
                            vMin: -2147483648,
                            vMax: 2147483647
                        });
                     }
                }
            }
        }
    }
</script>
