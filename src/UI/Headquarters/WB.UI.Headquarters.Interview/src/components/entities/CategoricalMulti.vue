<template>
    <wb-question :question="$me" questionCssClassName="multiselect-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="radio" v-for="option in $me.options">
                    <div class="field">
                        <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer">
                        <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                        </label>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
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
                    this.$store.dispatch("answerMutliOptionQuestion", { answer: value, questionId: this.$me.id })
                }
            }
        },
        mixins: [entityDetails]
    }
</script>
