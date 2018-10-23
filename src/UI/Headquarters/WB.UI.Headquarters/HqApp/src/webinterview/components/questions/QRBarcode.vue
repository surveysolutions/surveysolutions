<template>
    <wb-question :question="$me" questionCssClassName="single-select-question" :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <input autocomplete="off" type="text" class="field-to-fill"
                            :disabled="!$me.acceptAnswer"
                            :placeholder="noAnswerWatermark" :title="noAnswerWatermark" :value="$me.answer"
                            v-blurOnEnterKey @blur="answerQRBarcodeQuestion">
                            <wb-remove-answer />
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"

    export default {
        name: 'QRBarcode',
        mixins: [entityDetails],
        computed: {
            noAnswerWatermark() {
                return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : this.$t('WebInterviewUI.TextEnter')
            }
        },
        methods: {
            answerQRBarcodeQuestion(evnt) {
                this.sendAnswer(() => {
                    const target = $(evnt.target)
                    const answer = target.val()

                    if(this.handleEmptyAnswer(answer)) {
                        return
                    }

                    if (answer) {
                        this.$store.dispatch('answerQRBarcodeQuestion', { identity: this.id, text: answer })
                    }
                })
            }
        }
    }
</script>
