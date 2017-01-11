<template>
    <div class="question" v-if="$me">
        <div class="question-editor single-select-question" :class="{answered: $me.isAnswered}">
            <wb-title />
            <wb-instructions />
            <div class="question-unit">
                <div class="options-group">
                    <div class="radio" v-for="option in $me.options">
                        <div class="field">
                            <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" name="$me.id" :value="option.value" v-model="answer">
                            <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                            </label>
                            <button type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
                                <span></span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'CategoricalSingle',
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    this.$store.dispatch("answerSingleOptionQuestion", { answer: value, questionId: this.$me.id })
                }
            }
        },
        mixins: [entityDetails],
        methods: {
            removeAnswer() {
                this.$store.dispatch("removeAnswer", this.$me.id)
            }
        }
    }
</script>
