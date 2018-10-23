<template>
    <wb-question :question="$me" questionCssClassName="single-select-question" :noAnswer="noOptions" :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group" v-bind:class="{ 'dotted': noOptions }">
                <div class="radio" v-for="option in answeredOrAllOptions" :key="$me.id + '_' + option.value">
                    <div class="field">
                        <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" :name="$me.id" :value="option.value" :disabled="!$me.acceptAnswer" v-model="answer">
                        <label :for="$me.id + '_' + option.value">
                            <span class="tick"></span> {{option.title}}
                        </label>
                        <wb-remove-answer />
                    </div>
                </div>
                <button type="button" class="btn btn-link btn-horizontal-hamburger" @click="toggleOptions" v-if="shouldShowAnsweredOptionsOnly && !showAllOptions">
                    <span></span>
                </button>
                <div v-if="noOptions" class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import { find } from "lodash"
    import { shouldShowAnsweredOptionsOnlyForSingle } from "./question_helpers"

    export default {
        name: 'CategoricalSingle',
        props: ['noComments'],
        data(){
            return {
                showAllOptions: false
            }
        },
        computed: {
            shouldShowAnsweredOptionsOnly(){
                return shouldShowAnsweredOptionsOnlyForSingle(this);
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly)
                    return this.$me.options;
                
                var self = this;
                return [find(this.$me.options, function(o) { return o.value == self.answer; })];
            },
            answer: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    this.sendAnswer(() => {
                        this.$store.dispatch("answerSingleOptionQuestion", { answer: value, questionId: this.$me.id })
                    })
                }
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            }
        },
        mixins: [entityDetails],
        methods: {
            toggleOptions(){
                this.showAllOptions = !this.showAllOptions;
            }
        }
    }
</script>
