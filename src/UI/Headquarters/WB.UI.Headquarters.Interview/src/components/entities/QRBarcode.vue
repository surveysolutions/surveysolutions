<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <input autocomplete="off" type="text" class="field-to-fill" placeholder="Enter text" title="Enter text" :value="$me.answer"
                            v-blurOnEnterKey @blur="answerQRBarcodeQuestion">
                            <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"

    export default {
        name: 'QRBarcode',
        mixins: [entityDetails],
        methods: {
            answerQRBarcodeQuestion(evnt) {
                const target = $(evnt.target)
                const answer = target.val()

                if(this.handleEmptyAnswer(answer)) {
                    return
                }

                if (answer) {
                    this.$store.dispatch('answerQRBarcodeQuestion', { identity: this.id, text: answer })
                }
            }
        }
    }
</script>
