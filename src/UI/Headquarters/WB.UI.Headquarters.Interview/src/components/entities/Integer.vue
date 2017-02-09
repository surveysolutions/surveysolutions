<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" autocomplete="off" inputmode="numeric" pattern="[0-9]*" class="field-to-fill" placeholder="Tap to enter number" :value="$me.answer" v-blurOnEnterKey @blur="answerIntegerQuestion"
                            v-numericFormatting="{aSep: formattingChar, mDec: 0, vMin: '-2147483648', vMax: '2147483647', aPad: false }">
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
        computed: {
            formattingChar() {
                return this.$me.useFormatting ? ',' : ''
            }
        },
        methods: {
            answerIntegerQuestion(evnt) {

                const answerString = $(evnt.target).autoNumeric('get')
                const answer = answerString != undefined && answerString != ''
                                ? parseInt(answerString)
                                : null

                if (answer == null)
                {
                    this.markAnswerAsNotSavedWithMessage('Empty value cannot be saved')
                    return
                }

                if (answer > 2147483647 || answer < -2147483648 || answer % 1 !== 0)
                {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as integer value')
                    return
                }

                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }

                if (answer < 0)
                {
                    this.markAnswerAsNotSavedWithMessage(`Answer ${answer} is incorrect because question is used as size of roster and specified answer is negative`)
                    return;
                }

                if (answer > this.$me.answerMaxValue)
                {
                    this.markAnswerAsNotSavedWithMessage(`Answer ${answer} is incorrect because answer is greater than Roster upper bound ${this.$me.answerMaxValue}.`)
                    return;
                }

                const previousAnswer = this.$me.answer
                const isNeedRemoveRosters = previousAnswer != undefined && answer < previousAnswer

                if (!isNeedRemoveRosters)
                {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }

                const amountOfRostersToRemove = previousAnswer - answer;
                const confirmMessage = `Are you sure you want to remove ${amountOfRostersToRemove} row(s) from each related roster?`

                modal.methods.confirm(confirmMessage,  result => {
                    if (result) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                        return
                    } else {
                        this.fetch()
                        return
                    }
                } );
            },
            removeAnswer() {

                if (!this.$me.isAnswered)
                {
                    return
                }
                if (!this.$me.isRosterSize)
                {
                    this.$store.dispatch("removeAnswer", this.id)
                    return
                }

                var amountOfRostersToRemove = this.$me.answer;
                var confirmMessage = `Are you sure you want to remove ${amountOfRostersToRemove} row(s) from each related roster?`

                modal.methods.confirm(confirmMessage, result => {
                    if (result) {
                        this.$store.dispatch('removeAnswer', this.id)
                    } else {
                        this.fetch()
                    }
                });
            }
        }
    }
</script>
