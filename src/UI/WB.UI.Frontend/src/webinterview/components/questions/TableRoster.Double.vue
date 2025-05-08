<template>
    <input type="text" autocomplete="off" inputmode="decimal" class="ag-cell-edit-input" ref="inputDouble"
        :placeholder="noAnswerWatermark" :title="noAnswerWatermark" :value="$me.answer" :disabled="!$me.acceptAnswer"
        v-numericFormatting="{
            minimumValue: '-99999999999999.99999999999999',
            maximumValue: '99999999999999.99999999999999',
            digitGroupSeparator: groupSeparator,
            decimalCharacter: decimalSeparator,
            decimalPlaces: decimalPlacesCount
        }" />
</template>

<script lang="js">
import { nextTick } from 'vue'
import { entityDetails, tableCellEditor } from '../mixins'
import { getGroupSeparator, getDecimalSeparator, getDecimalPlacesCount } from './question_helpers'

export default {
    name: 'TableRoster_Double',
    mixins: [entityDetails, tableCellEditor],

    data() {
        return {
            //autoNumericElement: null,
            cancelBeforeStart: true,
        }
    },
    computed: {
        autoNumericElement() {
            return this.$refs.inputDouble.autoNumericElement
        },
        noAnswerWatermark() {
            return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : this.$t('WebInterviewUI.DecimalEnter')
        },
        groupSeparator() {
            return getGroupSeparator(this.$me)
        },
        decimalSeparator() {
            return getDecimalSeparator(this.$me)
        },
        decimalPlacesCount() {
            return getDecimalPlacesCount(this.$me)
        },
    },
    methods: {
        saveAnswer() {
            this.answerDoubleQuestion()
        },

        answerDoubleQuestion(evnt) {
            const answerString = this.autoNumericElement.getNumericString()
            if (answerString.replace(/[^0-9]/g, '').length > 15) {
                this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.DecimalTooBig') + " '" + answerString + "'")
                return
            }

            const answer = answerString != undefined && answerString != ''
                ? parseFloat(answerString)
                : null

            this.saveAnswerValue(answer)
        },
        saveAnswerValue(answer) {
            this.sendAnswer(() => {
                if (this.handleEmptyAnswer(answer)) {
                    return
                }

                if (answer > 999999999999999 || answer < -999999999999999) {
                    this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.DecimalCannotParse') + " '" + answer + "'")
                    return
                }

                this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: answer })
            })
        },

        isCancelBeforeStart() {
            return this.cancelBeforeStart
        },

        destroy() {
            if (this.autoNumericElement) {
                this.autoNumericElement.remove()
            }
        },
    },
    created() {
        // only start edit if key pressed is a number, not a letter
        this.cancelBeforeStart = this.editorParams.charPress && ('1234567890'.indexOf(this.editorParams.charPress) < 0)
    },
    mounted() {
        nextTick(() => {
            if (this.$refs.inputDouble) {
                this.$refs.inputDouble.select()
                this.$refs.inputDouble.focus()
            }
        })
    },
    beforeDestroy() {
        this.destroy()
    },
}
</script>
