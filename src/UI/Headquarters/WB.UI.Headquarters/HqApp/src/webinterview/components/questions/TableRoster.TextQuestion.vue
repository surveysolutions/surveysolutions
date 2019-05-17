<template>
    <input v-if="hasMask"
        ref="inputWithMask"
        autocomplete="off"
        type="text"
        class="ag-cell-edit-input"
        :placeholder="noAnswerWatermark"
        :value="$me.answer"
        :disabled="!$me.acceptAnswer"
        v-mask="$me.mask"
        :data-mask-completed="$me.isAnswered" />
    <input v-else ref="inputWithoutMask"
        autocomplete="off"
        type="text"
        :maxlength="$me.maxLength"
        class="ag-cell-edit-input"
        :placeholder="noAnswerWatermark"
        :value="$me.answer"
        :disabled="!$me.acceptAnswer" />
</template>

<script lang="js">
    import Vue from 'vue'
    import { entityDetails } from "../mixins"

    export default {
        name: 'TableRoster_TextQuestion',
        mixins: [entityDetails],
        
        data() {
            return {
                
            }
        }, 
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
            saveAnswer() {
                this.answerTextQuestion()
            },
            answerTextQuestion() {
                this.sendAnswer(() => {
                    const target = $(this.$refs.inputWithMask || this.$refs.inputWithoutMask)
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
                })
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
        },
        mounted() {
            Vue.nextTick(() => {
                const input = $(this.$refs.inputWithMask || this.$refs.inputWithoutMask)
                if (input) {
                    input.focus();
                    input.select();
                }
            });
        }
    }
</script>











