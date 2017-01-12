<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="number" class="field-to-fill" placeholder="Tap to enter number" v-model="answer" v-on:focusout="answerDoubleQuestion">
                        <button type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
                            <span></span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'Double',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    this.$me.answer = value
                }
            }
        },
        mixins: [entityDetails],
        methods: {
            answerDoubleQuestion: function () {
                this.$store.dispatch('answerDoubleQuestion', { identity: this.entity.id, answer: this.answer})
            },
            removeAnswer: function() {
                this.$store.dispatch("removeAnswer", this.$me.id)
            }
        }
    }
</script>
