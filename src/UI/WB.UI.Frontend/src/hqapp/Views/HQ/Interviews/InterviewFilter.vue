<template>
    <div class="block-filter"
        v-if="question != null && isSupported">
        <h5 :title="sanitizeHtml(question.label || question.questionText)">
            {{sanitizeHtml(question.label || question.questionText)}}
            <div>
                <inline-selector :options="fieldOptions"
                    no-empty
                    :id="`filter_selector_${condition.variable}`"
                    v-if="fieldOptions != null"
                    v-model="field" />
            </div>
        </h5>

        <Typeahead
            v-if="question.type == 'SINGLEOPTION'"
            :control-id="'filter_input_' + condition.variable"
            :placeholder="$t('Common.SelectOption')"
            :values="options"
            :value="selectedOption"
            v-on:selected="optionSelected"/>

        <filter-input v-if="question.type == 'TEXT'"
            :value="condition.value"
            @input="input"
            :id="'filter_input_' + condition.variable" />

        <filter-input v-if="question.type == 'NUMERIC'"
            :value="condition.value"
            type="number"
            @input="input"
            :id="'filter_input_' + condition.variable" />

    </div>
</template>
<script>

import gql from 'graphql-tag'
import { find, sortBy } from 'lodash'
import sanitizeHtml  from 'sanitize-html'

export default {
    props: {
        question: {type: Object },

        /** @type: {variable: string, value: string} */
        condition: { type: Object },
    },

    data() {
        return {
            field: null,
        }
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

        input(value) {
            this.$emit('change', {
                variable: this.question.variable,
                field: this.field.id,
                value: (value == null || value == '') ? null : value.toLowerCase(),
            })
        },

        optionSelected(option) {
            this.$emit('change', {
                variable: this.question.variable,
                field: this.field.id || 'answerCode',
                value: option == null ? null : parseInt(option.key),
            })
        },

        sanitizeHtml: sanitizeHtml,
    },

    watch: {
        field(to) {
            this.$emit('change', {
                variable: this.question.variable,
                field: to.id,
                value: this.condition.value,
            })
        },
    },

    computed: {
        options() {
            return this.getTypeaheadValues(this.question.options)
        },

        selectedOption() {
            let key = this.condition.value
            if(key != null) key = key.toString()
            return find(this.options, { key })
        },

        isSupported() {
            const supported = ['SINGLEOPTION', 'TEXT', 'NUMERIC']
            return find(supported, s => s == this.question.type)
        },

        fieldOptions() {
            switch(this.question.type) {
                case 'SINGLEOPTION': return [
                    { id: 'answerCode', value: this.$t('Common.Equals') },
                    { id: 'answerCode_not', value: this.$t('Common.NotEquals') },
                ]
                case 'TEXT': return [
                    { id: 'answerLowerCase_starts_with', value: this.$t('Common.StartsWith') },
                    { id: 'answerLowerCase', value: this.$t('Common.Equals') },

                ]
                case 'NUMERIC': return [
                    { id: 'answer', value: this.$t('Common.Equals')},
                ]}
            return null
        },
    },
}
</script>