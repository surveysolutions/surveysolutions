<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" autocomplete="off" inputmode="numeric" pattern="[0-9]*" class="field-to-fill" placeholder="Tap to enter number" :value="$me.answer" @blur="answerDoubleQuestion"
                            v-numericFormatting="{aSep: formattingChar, mDec: countOfDecimalPlaces, vMin: '-999999999999999', vMax: '999999999999999', aPad: false }">
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
                return this.$me.countOfDecimalPlaces || 16
            }
        },
        methods: {
            markAnswerAsNotSavedWithMessage(message) {
                const id = this.id
                this.$store.dispatch("setAnswerAsNotSaved", { id, message })
            },
            answerDoubleQuestion(evnt) {

                const answerString = $(evnt.target).autoNumeric('get');
                if (answerString.replace(/[^0-9]/g,"").length > 15)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value is bigger. Allow only 15 digits')
                    return
                }


                const answer = answerString != undefined && answerString != ''
                                ? parseFloat(answerString)
                                : null

                if (answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved')
                    return
                }

                if (answer > 999999999999999 || answer < -999999999999999)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as decimal value')
                    return
                }

                this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: answer })
            }
        }
    }
</script>
