<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" placeholder="Tap to enter number" v-model="$me.answer" @blur="answerDoubleQuestion"
                            v-numericFormatting="{aSep: formattingChar, mDec: countOfDecimalPlaces, vMin: -9999999999999999, vMax: 9999999999999999, aPad: false }">
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
        mixins: [entityDetails],
        computed: {
            formattingChar() {
                return this.$me.useFormatting ? ',' : ''
            },
            countOfDecimalPlaces() {
                return this.$me.countOfDecimalPlaces || 15
            }
        },
        methods: {
            markAnswerAsNotSavedWithMessage(message) {
                const id = this.id;
                this.$store.dispatch("setAnswerAsNotSaved", { id, message })
            },
            answerDoubleQuestion(evnt) {

                const answerString = $(evnt.target).autoNumeric('get');
                const answer = answerString != undefined && answerString != ''
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
        }
    }
</script>
