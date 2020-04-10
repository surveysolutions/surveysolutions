<template>
    <div class="block-filter">
        <h5>
            <inline-selector                 
                :options="questionsList"
                keySelector="variable"
                valueSelector="questionText"
                :value="selectedQuestion"
                @input="questionSelected" />
            <span class="caret"></span>
        </h5>
        <Typeahead
            v-if="isCategorical(selectedQuestion)"
            :control-id="'question-' + selectedQuestion.variable"
            fuzzy
            :placeholder="$t('Common.SelectOptionValue')"            
            :values="options"
            :value="selectedOption"
            v-on:selected="optionSelected"/>
        
    </div>    
</template>
<script>

import gql from 'graphql-tag'
import InlineSelector from './InlineSelector'
import { find, sortBy } from 'lodash'

export default {
    props: {
        exludedQuestions: { type: Array },
        questions: {type: Array },
        
        /** @type: {variable: string, value: string} */
        condition: { type: Object },
    },

    methods: {
        addCondition() {
            this.conditions.push({})
        },

        isCategorical(question) {
            return this.selectedQuestion != null && this.selectedQuestion.type == 'SINGLEOPTION'
        },

        getTypeaheadValues(options) {
            return options.map(o => {
                return {
                    key: o.value.toString(),
                    value: o.title,
                }
            })
        },

        questionSelected(question) {
            this.$emit('remove', this.condition.variable)
            this.$emit('change', {
                variable: question.variable,
                value: null,
            })
        },

        optionSelected(option) {
            this.$emit('change', {
                variable: this.selectedQuestion.variable,
                value: option == null ? null : option.key,
            })
        },
    },

    computed: {
        questionsList() {
            const array = [...this.questions]
            array.sort(function (a, b) {
                return a.questionText.localeCompare(b.questionText)
            })
            return array
        },

        options() {
            return this.getTypeaheadValues(sortBy(this.selectedQuestion.options, ['title']))
        },

        selectedQuestion() {
            return find(this.questions, { variable: this.condition.variable })
        },

        selectedOption() {

            return find(this.options, {key: this.condition.value})
        },
    },

    components: {
        InlineSelector,
        //   QuestionCondition,
    },
}
</script>