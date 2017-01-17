<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" placeholder="Tap to enter number" v-model="$me.answer" @blur="answerIntegerQuestion"
                            v-autoNumeric="{aSep: this.$me.useFormatting ? ',' : '' }">
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
    import modal from "../Modal"

    export default {
        name: 'Integer',
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

                var previousAnswer = this.$me.answer;
                var isNeedRemoveRosters = previousAnswer != undefined && answer < previousAnswer;

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer });
                    return;
                }

                var amountOfRostersToRemove = previousAnswer - answer;
                var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';
                var oThis = this;


                modal.methods.confirm(confirmMessage,  function (result) {
                    if (result) {
                        oThis.$store.dispatch('answerIntegerQuestion', { identity: oThis.id, answer: answer });
                        return;
                    } else {
                        oThis.fetch();
                        return;
                    }
                } );
            },
            removeAnswer: function() {

                if (!this.$me.isAnswered)
                {
                    return;
                }
                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch("removeAnswer", this.id);
                    return;
                }

                var amountOfRostersToRemove = this.$me.answer;
                var confirmMessage = 'Are you sure you want to remove '+ amountOfRostersToRemove + ' row(s) from each related roster?';
                var oThis = this;
                modal.methods.confirm(confirmMessage, function (result) {
                    if (result) {
                        oThis.$store.dispatch('removeAnswer', oThis.id)
                    } else {
                        oThis.fetch();
                    }
                });
            }
        },
        directives: {
            autoNumeric: {
                bind: (el, binding) => {
                    $(el).autoNumeric('init');
                },
                update: (el, binding) => {
                    if (binding.value) {
                        var defaults = {
                            aSep: '',
                            mDec: 0,
                            vMin: -2147483648,
                            vMax: 2147483647,
                            aPad: false
                        };
                        var settings = $.extend( {}, defaults, binding.value );
                        $(el).autoNumeric('update', settings);
                    }
                },
                unbind: (el) => {
                    $(el).autoNumeric('destroy');
                }
            }
        }
    }
</script>
