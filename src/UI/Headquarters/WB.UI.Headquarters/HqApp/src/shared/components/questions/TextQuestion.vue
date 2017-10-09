<template>
    <wb-question :question="$me"
                 questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                        <input ref="input"
                               autocomplete="off"
                               type="text"
                               class="field-to-fill"
                               :placeholder="$t('WebInterviewUI.TextEnterMasked', { userFriendlyMask })"
                               :value="$me.answer"
                               v-blurOnEnterKey
                               @blur="answerTextQuestion"
                               v-mask="$me.mask"
                               :data-mask-completed="$me.isAnswered"
                               :title="$t('WebInterviewUI.TextEnter')">
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">
    import { entityDetails } from "../mixins"

    export default {
        name: 'TextQuestion',
        mixins: [entityDetails],
        computed: {
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
                const target = $(this.$refs.input)
                const answer = this.$refs.input.value

                if(this.handleEmptyAnswer(answer)) {
                    return
                }

                if (this.$me.mask && !target.data("maskCompleted")) {
                    this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.TextRequired"))
                }
                else {
                    this.$store.dispatch('answerTextQuestion', { identity: this.id, text: answer })
                }
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
