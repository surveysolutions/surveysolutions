<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" placeholder="Tap to enter number" v-model="answer" @blur="answerIntegerQuestion" v-onlyNumbers="true">
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
        computed: {
            answer: {
                get : function() {
                    return this.$me.answer;
                },
                set : function(value) {
                    if (value != undefined && value != '') {
                        this.$me.answer = value;
                    }
                    else {
                        this.$me.answer = null;
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

                var answerString = $(evnt.target).autoNumeric('get');
                var answer = answerString != undefined && answerString != ''
                                ? parseInt(answerString)
                                : null;

                if (answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved');
                    return;
                }

                if (answer > 2147483647 || answer < -2147483648 || answer % 1 !== 0)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as integer value');
                    return;
                }

                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer });
                    return;
                }


                if (answer < 0)
                {
                    this.markAnswerAsNotSavedWithMessage('Answer ' + answer + ' is incorrect because question is used as size of roster and specified answer is negative');
                    return;
                }

                if (answer > this.$me.answerMaxValue)
                {
                    this.markAnswerAsNotSavedWithMessage('Answer ' + answer + ' is incorrect because answer is greater than Roster upper bound ' + this.$me.answerMaxValue + '.');
                    return;
                }

                var isNeedRemoveRosters = this.$me.previousAnswer != undefined && answer < this.$me.previousAnswer;

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer });
                    return;
                }

                var amountOfRostersToRemove = this.$me.previousAnswer - answer;
                var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';

                bootbox.confirm(confirmMessage, function (result) {
                    if (result) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
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
                        });/*.on('change', (function(_this) {
                            return function() {
                                return _this.set($(_this.el).autoNumeric('get'));
                            };
                        }));*/
                     }
                }
            }
        }
    }
</script>
