<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <input ref="input" autocomplete="off" type="text" class="field-to-fill" :placeholder="'Enter text' + userFriendlyMask" :value="$me.answer"
                            v-blurOnEnterKey @blur="answerTextQuestion" v-mask="$me.mask" :data-mask-completed="$me.isAnswered" title="Enter text">
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
            answerTextQuestion(evnt) {
                const target = $(this.$refs.input)
                const answer = this.$refs.input.value

                if(this.handleEmptyAnswer(answer)) {
                    return
                }

                if (this.$me.mask && !target.data("maskCompleted")) {
                    this.markAnswerAsNotSavedWithMessage("Please, fill in all the required values")
                }
                else {
                    this.$store.dispatch('answerTextQuestion', { identity: this.id, text: answer })
                }
            }
        },
        directives: {
            mask: {
                bind(el, binding, vnode) {
                    if (binding.value) {
                        const input = $(el)
                        input.mask(binding.value, {
                            onChange: function (val) {
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
