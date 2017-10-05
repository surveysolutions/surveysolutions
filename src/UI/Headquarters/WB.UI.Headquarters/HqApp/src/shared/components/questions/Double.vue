<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" autocomplete="off" inputmode="numeric" class="field-to-fill"
                            :placeholder="$t('DecimalEnter')" :title="$t('DecimalEnter')"
                            :value="$me.answer" v-blurOnEnterKey @blur="answerDoubleQuestion"
                            v-numericFormatting="{aSep: groupSeparator, mDec: $me.countOfDecimalPlaces, vMin: '-99999999999999.99999999999999', vMax: '99999999999999.99999999999999', aPad: false }">
                            <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import * as $ from "jquery"

    export default {
        name: 'Double',
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
            answerDoubleQuestion(evnt) {
                const answerString = $(evnt.target).autoNumeric('get');
                if (answerString.replace(/[^0-9]/g, "").length > 15) {
                    this.markAnswerAsNotSavedWithMessage(this.$t("DecimalTooBig"))
                    return
                }

                const answer = answerString != undefined && answerString != ''
                    ? parseFloat(answerString)
                    : null

                if(this.handleEmptyAnswer(answer)) {
                    return
                }
                if (answer > 999999999999999 || answer < -999999999999999) {
                    this.markAnswerAsNotSavedWithMessage($t("DecimalCannotParse"))
                    return
                }

                this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: answer })
            }
        }
    }

</script>
