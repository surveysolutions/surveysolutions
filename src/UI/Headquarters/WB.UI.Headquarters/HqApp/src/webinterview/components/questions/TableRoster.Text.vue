<template>
    <div class="ag-input-text-wrapper">
        <input :ref="'input'" class="ag-cell-edit-input" type="text" v-model="$me.answer" v-blurOnEnterKey />
    </div>
</template>

<script lang="js">
    import Vue from 'vue'
    import { entityDetails } from "../mixins"

    export default {
        name: 'TableRoster_Text',
        mixins: [entityDetails],
        
        data() {
            return {
                $me: null
            }
        }, 
        methods: {
            getValue() {
                return this.$me;
            },
            isCancelAfterEnd() {
                this.answerTextQuestion();
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
        created() {
            this.$me = this.params.value;
            this.$store = this.params.store;
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











