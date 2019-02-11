<template>
    <wb-question :question="$me"
                 questionCssClassName="multiselect-question"
                 :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="(row) in this.selectedOptions" :key="row.value">
                    <div class="field answered" v-bind:class="{ 'unavailable-option locked-option': row.isProtected }">
                        <input autocomplete="off" type="text" class="field-to-fill" 
                            :value="row.title"
                            :disabled="!$me.acceptAnswer || row.isProtected" />
                        <button type="submit" class="btn btn-link btn-clear" 
                            v-if="$me.acceptAnswer && !row.isProtected"
                            tabindex="-1"
                            @click="confirmAndRemoveRow(row.value)"><span></span>
                        </button>
                        <div class="lock"></div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                        <wb-typeahead :questionId="$me.id"
                                       @input="appendCompboboxItem" 
                                      :disabled="!$me.acceptAnswer"
                                      :optionsSource="optionsSource"
                                      :watermark="!$me.acceptAnswer && !$me.isAnswered ? $t('Details.NoAnswer') : null"/>
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

    import { entityDetails } from "../mixins"
    import Vue from 'vue'
    import modal from "../modal"
    import {find, map, includes, without} from "lodash"
    
    export default {
        name: 'MultiComboboxQuestion',
        mixins: [entityDetails],
        props: ['noComments'],
        data() {
            return {
                selectedOption: null
            }
        },

        watch: {
            selectedOption(newValue) {
                
            }
        },
        computed: {
            selectedOptions() {
                var self = this;
                return map(self.$me.answer, (val) => {
                    return {
                        title: find(self.$me.options, (opt) => { return opt.value === val }).title,
                        value: val
                    }
                })
            }
        },
        methods: {
            appendCompboboxItem(newValue) {
                if(includes(this.$me.answer, newValue)) return

                let newAnswer = this.$me.answer
                newAnswer.push(newValue)
                this.$store.dispatch("answerMultiOptionQuestion", { answer: newAnswer, questionId: this.$me.id })
            },
            optionsSource(filter) {
                return Vue.$api.call(api => api.getTopFilteredOptionsForQuestion(this.$me.id, filter, 50))
            },
            confirmAndRemoveRow(valueToRemove){
                if(!includes(this.$me.answer, valueToRemove)) return

                const newAnswer = without(this.$me.answer, valueToRemove)

                if (this.$me.isRosterSize) {
                    modal.confirm(confirmMessage, result => {
                        if (result) {
                            this.$store.dispatch("answerMultiOptionQuestion", { answer: newAnswer, questionId: this.$me.id })
                            return
                        } 
                    })
                }
                else {
                    this.$store.dispatch("answerMultiOptionQuestion", { answer: newAnswer, questionId: this.$me.id })
                }
            }
        }
    }

</script>
