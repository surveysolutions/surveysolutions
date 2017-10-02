<template>
    <wb-question :question="$me" questionCssClassName="single-select-question" :noAnswer="noOptions">
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="radio" v-for="option in $me.options" :key="$me.id + '_' + option.value">
                    <div class="field">
                        <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer">
                        <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                        </label>
                        <wb-remove-answer />
                    </div>
                </div>
                <div v-if="noOptions" class="options-not-available">{{ $t("OptionsAvailableAfterAnswer") }}</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"

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
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        mixins: [entityDetails]
    }
</script>
