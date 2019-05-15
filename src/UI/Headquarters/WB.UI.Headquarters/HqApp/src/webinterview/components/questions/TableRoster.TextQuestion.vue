<template>
    <input :ref="'input'" class="ag-cell-edit-input" type="text" :value="$me.answer" v-blurOnEnterKey />
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
        methods: {
            getValue() {
                return this.$me;
            },
            isCancelAfterEnd() {
                this.answerTextQuestion();
            },
            saveAnswer() {
                this.answerTextQuestion()
            },
            answerTextQuestion() {
                const target = $(this.$refs.input)
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
            }
        },
        mounted() {
            Vue.nextTick(() => {
                if (this.$refs.input) {
                    this.$refs.input.focus();
                    this.$refs.input.select();
                }
            });
        }
    }
</script>











