<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" autocomplete="off" inputmode="numeric" class="field-to-fill" placeholder="Enter number" title="Enter number"
                            :value="$me.answer" v-blurOnEnterKey @blur="answerIntegerQuestion" v-numericFormatting="{aSep: groupSeparator, mDec: 0, vMin: '-2147483648', vMax: '2147483647', aPad: false }">
                            <button v-if="$me.isAnswered" type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
                            <span></span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"
    import modal from "../../modal"
    import numerics from "../../numerics"

    export default {
        name: 'Integer',
        mixins: [entityDetails],
        computed: {
            groupSeparator() {
                if (this.$me.useFormatting) {
                    var etalon = 1111
                    var localizedNumber = etalon.toLocaleString()
                    return localizedNumber.substring(1, localizedNumber.length - 3)
                }

                return ''
            }
        },
        methods: {
            async answerIntegerQuestion(evnt) {
                const answerString = await numerics().get($(evnt.target))
                const answer = answerString != undefined && answerString != ''
                    ? parseInt(answerString)
                    : null

                if(this.handleEmptyAnswer(answer)) {
                    return
                }

                if (answer > 2147483647 || answer < -2147483648 || answer % 1 !== 0) {
                    this.markAnswerAsNotSavedWithMessage('Entered value can not be parsed as integer value')
                    return
                }

                if (!this.$me.isRosterSize) {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }

                if (answer < 0) {
                    this.markAnswerAsNotSavedWithMessage(`Answer ${answer} is incorrect because question is used as size of roster and specified answer is negative`)
                    return;
                }

                if (answer > this.$me.answerMaxValue) {
                    this.markAnswerAsNotSavedWithMessage(`Answer ${answer} is incorrect because answer is greater than Roster upper bound ${this.$me.answerMaxValue}.`)
                    return;
                }

                const previousAnswer = this.$me.answer
                const isNeedRemoveRosters = previousAnswer != undefined && answer < previousAnswer

                if (!isNeedRemoveRosters) {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }

                const amountOfRostersToRemove = previousAnswer - answer;
                const confirmMessage = `Are you sure you want to remove ${amountOfRostersToRemove} row(s) from each related roster?`

                modal.confirm(confirmMessage, result => {
                    if (result) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                        return
                    } else {
                        this.fetch()
                        return
                    }
                });
            },
            removeAnswer() {

                if (!this.$me.isAnswered) {
                    return
                }
                if (!this.$me.isRosterSize) {
                    this.$store.dispatch("removeAnswer", this.id)
                    return
                }

                var amountOfRostersToRemove = this.$me.answer;
                var confirmMessage = `Are you sure you want to remove ${amountOfRostersToRemove} row(s) from each related roster?`

                modal.confirm(confirmMessage, result => {
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
