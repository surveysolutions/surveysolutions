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

    let equals: (x: number[], y: number[])=>boolean = function (x: number[], y: number[]) : boolean {
        if (x == null || y == null)
            return false;
        if (x.length!=y.length)
            return false;
        return x.every((element, index) => {  return element == y[index]; });
    }

    export default {
        name: "LinkedSingle",
        computed: {
            answer: {
                get() {
                    const selectedOption = this.$me.options.find((option) => { return equals(option.rosterVector, this.$me.answer); });
                    return selectedOption == null ? null: selectedOption.value;
                },
                set(value) {
                    const selectedOption = this.$me.options.find((option) => { return option.value == value; });
                    if (this.$me.isLinkedToList){
                        this.$store.dispatch("answerLinkedToListSingleQuestion", { answer: value[0], questionIdentity: this.$me.id })
                    }
                    this.$store.dispatch("answerLinkedSingleOptionQuestion", { answer: selectedOption.rosterVector, questionIdentity: this.$me.id })
                }
            }
        },
        mixins: [entityDetails]
    }
</script>
