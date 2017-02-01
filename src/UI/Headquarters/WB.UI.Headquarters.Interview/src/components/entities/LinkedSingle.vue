<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="radio" v-for="option in $me.options">
                    <div class="field">
                        <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer">
                        <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                        </label>
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: "LinkedSingle",
        computed: {
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    const selectedOption = this.$me.options.find((option) => { return option.value == value; });
                    this.$store.dispatch("answerLinkedSingleOptionQuestion", { answer: selectedOption.rosterVector, questionId: this.$me.id })
                }
            }
        },
        mixins: [entityDetails]
    }
</script>
