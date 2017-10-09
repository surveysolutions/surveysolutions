<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
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
                <div v-if="noOptions" class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import * as find from "lodash/find"
    import * as isEqual from "lodash/isequal"

    export default {
        name: "LinkedSingle",
        computed: {
            answer: {
                get() {
                    if (this.$me.options == null || this.$me.answer == null)
                        return;
                    return find(this.$me.options, (a) => { return isEqual(a.rosterVector, this.$me.answer) }).value;
                },
                set(value) {
                    const selectedOption = find(this.$me.options, { 'value': value });
                    this.$store.dispatch("answerLinkedSingleOptionQuestion", { answer: selectedOption.rosterVector, questionIdentity: this.$me.id })
                }
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        mixins: [entityDetails]
    }

</script>
