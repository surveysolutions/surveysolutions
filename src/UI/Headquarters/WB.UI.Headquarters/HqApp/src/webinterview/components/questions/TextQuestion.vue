<template>
    <wb-question :question="$me"
                 questionCssClassName="text-question" :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                         <input v-if="hasMask"
                                ref="input"
                               autocomplete="off"
                               type="text"
                               class="field-to-fill"
                               :placeholder="noAnswerWatermark"
                               :value="$me.answer"
                               :disabled="!$me.acceptAnswer"
                               v-blurOnEnterKey
                               @blur="answerTextQuestion"
                               v-mask="$me.mask"
                               :data-mask-completed="$me.isAnswered" />
                        <textarea-autosize v-else ref="inputTextArea"
                               autocomplete="off"
                               rows="1"
                               :maxlength="$me.maxLength"
                               class="field-to-fill"
                               :placeholder="noAnswerWatermark"
                               :value="$me.answer"
                               :important="true"
                               :disabled="!$me.acceptAnswer"
                               v-blurOnEnterKey
                               @blur.native="answerTextQuestion"
                               @blur="answerTextQuestion">
                        </textarea-autosize>
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
        name: 'TextQuestion',
        mixins: [entityDetails],
        props: ['noComments'],
        computed: {
            hasMask(){
                return this.$me.mask!=null;
            },
            noAnswerWatermark() {
                return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : 
                    this.$t('WebInterviewUI.TextEnterMasked', {userFriendlyMask: this.userFriendlyMask})
            },
            userFriendlyMask() {
                if (this.$me.mask) {
                    const resultMask = this.$me.mask.replace(/\*/g, "_").replace(/\#/g, "_").replace(/\~/g, "_")
                    return ` (${resultMask})`
                }

                return ""
            }
        },
        methods: {
            answerTextQuestion() {
                this.sendAnswer(() => {
                    const target = $(this.$refs.input || this.$refs.inputTextArea.$el)
                    const answer = target.val()

                    if(this.handleEmptyAnswer(answer)) {
                        return
                    }

                    if (this.$me.mask && !target.data("maskCompleted")) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.TextRequired"))
                    }
                    else {
                        this.$store.dispatch('answerTextQuestion', { identity: this.id, text: answer })
                    }
                });
            }
        },
        directives: {
            mask: {
                bind(el, binding) {
                    if (binding.value) {
                        const input = $(el)
                        input.mask(binding.value, {
                            onChange: function () {
                                input.data("maskCompleted", false);
                            },
                            onComplete: function () {
                                input.data("maskCompleted", true);
                            },
                            translation: {
                                "0": { pattern: /0/, fallback: "0" },
                                "~": { pattern: /[a-zA-Z]/ },
                                "#": { pattern: /\d/ },
                                "*": { pattern: /[a-zA-Z0-9]/ },
                                "9": { pattern: /9/, fallback: "9" },
                                'A': { pattern: /A/, fallback: "A" },
                                'S': { pattern: /S/, fallback: "S" }
                            }
                        })
                    }
                }
            }
        }
    }

</script>
