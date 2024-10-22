<template>
    <input :ref="'input'" type="text" autocomplete="off" inputmode="numeric" class="ag-cell-edit-input"
        :value="$me.answer" v-numericFormatting="{
            digitGroupSeparator: groupSeparator,
            decimalCharacter: decimalSeparator,
            decimalPlaces: 0,
            minimumValue: '-2147483648',
            maximumValue: '2147483647'
        }" />
</template>

<script lang="js">
import { nextTick } from 'vue'
import { entityDetails, tableCellEditor } from '../mixins'
import { getGroupSeparator, getDecimalSeparator } from './question_helpers'
import modal from '@/shared/modal'

export default {
    name: 'TableRoster_Integer',
    mixins: [entityDetails, tableCellEditor],

    data() {
        return {
            //autoNumericElement: null,
            cancelBeforeStart: true,
        }
    },
    computed: {
        autoNumericElement() {
            return this.$refs.input.autoNumericElement
        },
        groupSeparator() {
            return getGroupSeparator(this.$me)
        },
        decimalSeparator() {
            return getDecimalSeparator(this.$me)
        },
    },
    methods: {

        saveAnswer() {
            this.answerIntegerQuestion()
        },

        answerIntegerQuestion() {
            const answerString = this.autoNumericElement.getNumericString()
            const answer = answerString != undefined && answerString != ''
                ? parseInt(answerString)
                : null

            this.saveIntegerAnswer(answer)
        },

        saveIntegerAnswer(answer) {
            this.sendAnswer(() => {
                if (this.handleEmptyAnswer(answer)) {
                    return
                }

                const previousAnswer = this.$me.answer

                if (answer > 2147483647 || answer < -2147483648 || answer % 1 !== 0) {
                    this.markAnswerAsNotValidWithMessage(this.$t('WebInterviewUI.NumberCannotParse'), answer)
                    return
                }

                if (this.$me.isProtected && this.$me.protectedAnswer > answer) {
                    this.markAnswerAsNotValidWithMessage(this.$t('WebInterviewUI.NumberCannotBeLessThanProtected'), answer)
                    return
                }

                if (!this.$me.isRosterSize) {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer }, answer)
                    return
                }

                if (answer < 0) {
                    this.markAnswerAsNotValidWithMessage(this.$t('WebInterviewUI.NumberRosterError', { answer }), answer)
                    return
                }

                if (answer > this.$me.answerMaxValue) {
                    this.markAnswerAsNotValidWithMessage(this.$t('WebInterviewUI.NumberRosterUpperBound', { answer, answerMaxValue: this.$me.answerMaxValue }), answer)
                    return
                }

                const isNeedRemoveRosters = previousAnswer != undefined && answer < previousAnswer

                if (!isNeedRemoveRosters) {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }

                const amountOfRostersToRemove = previousAnswer - Math.max(answer, 0)
                const confirmMessage = this.$t('WebInterviewUI.NumberRosterRemoveConfirm', { amountOfRostersToRemove })

                if (amountOfRostersToRemove > 0) {
                    modal.confirm(confirmMessage, result => {
                        if (result) {
                            this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                            return
                        } else {
                            this.fetch()
                            return
                        }
                    })
                }
                else {
                    this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                    return
                }
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
            if (this.$refs.input) {
                this.$refs.input.focus()
                this.$refs.input.select()
            }
        })
    },
    beforeDestroy() {
        this.destroy()
    },
}
</script>
