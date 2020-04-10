<template>
    <FilterBlock :title="question.questionText">
        <Typeahead fuzzy
            v-if="isCategorical"
            :control-id="'question-' + question.variable"           
            :placeholder="$t('Common.SelectOptionValue')"            
            :values="options"
            :value="selectedOption"
            v-on:selected="optionSelected"/>
    </FilterBlock>   
</template>
<script>

import gql from 'graphql-tag'
import { find, sortBy } from 'lodash'

export default {
    props: {
        question: {type: Object },
        
        /** @type: {variable: string, value: string} */
        condition: { type: Object },
    },

    methods: {
        getTypeaheadValues(options) {
            return options.map(o => {
                return {
                    key: o.value.toString(),
                    value: o.title,
                }
            })
        },

        optionSelected(option) {
            this.$emit('change', {
                variable: this.question.variable,
                value: option == null ? null : option.key,
            })
        },
    },

    computed: {
        isCategorical() {
            return this.question != null && this.question.type == 'SINGLEOPTION'
        },

        options() {
            return this.getTypeaheadValues(sortBy(this.question.options, ['title']))
        },

        selectedOption() {
            return find(this.options, {key: this.condition.value})
        },
    },
}
</script>