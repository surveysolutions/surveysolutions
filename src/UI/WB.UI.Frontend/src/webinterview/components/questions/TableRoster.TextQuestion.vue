<template>
    <input ref="input" autocomplete="off" type="text" class="ag-cell-edit-input" :maxlength="$me.maxLength"
        :placeholder="noAnswerWatermark" :value="$me.answer" :disabled="!$me.acceptAnswer" v-maskedText="$me.mask"
        :data-mask-completed="$me.isAnswered" />
</template>

<script lang="js">
import { nextTick } from 'vue'
import { entityDetails, tableCellEditor } from '../mixins'

export default {
    name: 'TableRoster_TextQuestion',
    mixins: [entityDetails, tableCellEditor],

    data() {
        return {

        }
    },
    computed: {
        hasMask() {
            return this.$me.mask != null
        },
        noAnswerWatermark() {
            return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') :
                this.$t('WebInterviewUI.TextEnterMasked', { userFriendlyMask: this.userFriendlyMask })
        },
        userFriendlyMask() {
            if (this.$me.mask) {
                const resultMask = this.$me.mask.replace(/\*/g, 'ˍ').replace(/#/g, 'ˍ').replace(/~/g, 'ˍ')
                return ` (${resultMask})`
            }

            return ''
        },
    },
    methods: {
        saveAnswer() {
            this.answerTextQuestion()
        },
        answerTextQuestion() {
            this.sendAnswer(() => {
                const target = $(this.$refs.input)
                const answer = target.val()?.trim()

                if (this.handleEmptyAnswer(answer)) {
                    target.value = this.$me.answer || ''
                    return
                }

                if (this.$me.mask && !target.data('maskCompleted')) {
                    this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.TextRequired'), answer)
                }
                else {
                    this.$store.dispatch('answerTextQuestion', { identity: this.id, text: answer })
                }
            })
        },
    },
    mounted() {
        nextTick(() => {
            const input = $(this.$refs.input)
            if (input) {
                input.focus()
                input.select()
            }
        })
    },
}
</script>
