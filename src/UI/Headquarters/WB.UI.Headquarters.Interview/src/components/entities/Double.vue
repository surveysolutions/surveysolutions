<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" placeholder="Tap to enter number" v-model="answer" @blur="answerDoubleQuestion"
                            v-autoNumeric="{aSep: this.$me.useFormatting ? ',' : '', mDec: this.$me.countOfDecimalPlaces || 15 }">
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
        name: 'Double',
        computed: {
            answer: {
                get() {
                    return this.$me.answer;
                },
                set(value) {
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
            answerDoubleQuestion: function (evnt) {

                var answerString = $(evnt.target).autoNumeric('get');
                var answer = answerString != undefined && answerString != ''
                                ? parseFloat(answerString)
                                : null;

                if (answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved');
                    return;
                }

                if (answer > 9999999999999999 || answer < -9999999999999999)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as decimal value');
                    return;
                }

                this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: answer })
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
                            mDec: 15,
                            vMin: -9999999999999999,
                            vMax: 9999999999999999,
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
