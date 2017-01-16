<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" placeholder="Tap to enter number" maxlength="16" v-model="answer" @blur="answerDoubleQuestion" v-onlyNumbersAndPoint="true">
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
            answerDoubleQuestion: function (evnt) {

                if (this.answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved');
                    return;
                }

                if (this.answer > 9999999999999999 || this.answer < -9999999999999999)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as decimal value');
                    return;
                }

                this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: this.answer })
            }
        },
        directives: {
            onlyNumbersAndPoint: {
                bind: (el, binding) => {
                     if (binding.value == true) {
                        $(el).autoNumeric('init', {
                            aSep: '', //this.$me.useFormatting ? ',' : '',
                            mDec: 20,
                            vMin: -9999999999999999,
                            vMax: 9999999999999999
                        });
                     }
                }
            }
        }
    }
</script>
