<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="radio"  v-for="option in $me.options">
                    <div class="field">
                        <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer">
                        <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                        </label>
                        <wb-remove-answer />
                    </div>
                </div>
                <div v-if="noOptions">Options will be available after answering referenced question</div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as find from "lodash/find"

    export default {
        name: "LinkedSingle",
        computed: {
            answer: {
                get() {
                    if (this.$me.options == null || this.$me.answer == null)
                        return;
                    return find(this.$me.options, { 'rosterVector': this.$me.answer }).value;
                },
                set(value) {
                    const selectedOption = this.$me.options.find((option) => { return option.value == value; });
                    if (this.$me.isLinkedToList){
                        this.$store.dispatch("answerLinkedToListSingleQuestion", { answer: value[0], questionIdentity: this.$me.id })
                    } else {
                        this.$store.dispatch("answerLinkedSingleOptionQuestion", { answer: selectedOption.rosterVector, questionIdentity: this.$me.id })
                    }
                }
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        mixins: [entityDetails]
    }
</script>
