<template>
    <wb-question :question="$me" :questionCssClassName="$me.ordered ? 'ordered-question' : 'multiselect-question'">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="option in $me.options">
                    <input class="wb-checkbox" type="checkbox" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" v-model="answer"
                        v-disabledWhenUnchecked="allAnswersGiven">
                    <label :for="$me.id + '_' + option.value">
                        <span class="tick"></span> {{option.title}}
                    </label>
                    <!--<div class="badge" v-if="$me.ordered">{{getAnswerOrder(option.value)}}</div>-->
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"

    let equals: (x: number[], y: number[])=>boolean = function (x: number[], y: number[]) : boolean {
        if (x == null || y == null)
            return false;
        if (x.length!=y.length)
            return false;
        return x.every((element, index) => {  return element == y[index]; });
    }

    export default {
        name: 'LinkedMulti',
        computed: {
            answer: {
                get() {
                    const selectedOptions = this.$me.answer.map((a) => {return this.$me.options.find((option) => { return equals(option.rosterVector, a ); })});
                    return selectedOptions
                },
                set(value) {
                    if (this.$me.isLinkedToList){
                        const selectedOptions = value.map((v) => { return this.$me.options.find((option) => {return option.value == v; }).rosterVector[0]; });
                        this.$store.dispatch("answerLinkedToListMultiQuestion", { answer: value, questionIdentity: this.$me.id })
                    }else{
                        const selectedOptions = value.map((v) => { return this.$me.options.find((option) => {return option.value == v; }).rosterVector });
                        this.$store.dispatch("answerLinkedMultiOptionQuestion", { answer: value, questionIdentity: this.$me.id })
                    }

                    return;
                }
            },
            allAnswersGiven() {
                return false;
            }
        },
        directives: {
            disabledWhenUnchecked: {
                update: (el, binding) => {
                    $(el).prop("disabled", binding.value && !el.checked)
                }
            }
        },
        mixins: [entityDetails]
    }
</script>
