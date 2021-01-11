<template>
    <div class="block-filter"
        v-if="item != null && isSupported">
        <h5 :title="sanitizeHtml(item.label || item.title)">
            {{sanitizeHtml(item.label || item.title)}}
            <div>
                <inline-selector :options="fieldOptions"
                    no-empty
                    :id="`filter_selector_${condition.variable}`"
                    v-if="fieldOptions != null"
                    v-model="field" />
            </div>
        </h5>

        <Typeahead
            v-if="item.type == 'SINGLEOPTION'"
            :control-id="'filter_input_' + condition.variable"
            :placeholder="$t('Common.SelectOption')"
            :values="options"
            :value="selectedOption"
            v-on:selected="optionSelected"/>

        <filter-input v-if="item.type == 'TEXT' || item.entityType == 'VARIABLE'"
            :value="condition.value"
            @input="input"
            :id="'filter_input_' + condition.variable" />

        <filter-input v-if="item.type == 'NUMERIC'"
            :value="condition.value"
            type="number"
            @input="input"
            :id="'filter_input_' + condition.variable" />

    </div>
</template>
<script>

import { find, sortBy } from 'lodash'
import sanitizeHtml  from 'sanitize-html'

export default {
    props: {
        item: {type: Object },

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
                variable: this.item.variable,
                field: this.field.id,
                value: (value == null || value == '') ? null : value.toLowerCase(),
            })
        },

        optionSelected(option) {
            this.$emit('change', {
                variable: this.item.variable,
                field: this.field.id || 'answerCode',
                value: option == null ? null : parseInt(option.key),
            })
        },

        sanitizeHtml: sanitizeHtml,
    },

    watch: {
        field(to) {
            this.$emit('change', {
                variable: this.item.variable,
                field: to.id,
                value: this.condition.value,
            })
        },
    },

    computed: {
        options() {
            return this.getTypeaheadValues(this.item.options)
        },

        selectedOption() {
            let key = this.condition.value
            if(key != null) key = key.toString()
            return find(this.options, { key })
        },

        isSupported() {
            if (this.item.entityType == 'VARIABLE')
                return true
            const supported = ['SINGLEOPTION', 'TEXT', 'NUMERIC']
            return find(supported, s => s == this.item.type)
        },

        fieldOptions() {
            if (this.item.entityType == 'VARIABLE') {
                return [
                    { id: 'valueLowerCase|startsWith', value: this.$t('Common.StartsWith') },
                    { id: 'valueLowerCase|eq', value: this.$t('Common.Equals') },
                ]
            }

            switch(this.item.type) {
                case 'SINGLEOPTION': return [
                    { id: 'answerCode|eq', value: this.$t('Common.Equals') },
                    { id: 'answerCode|neq', value: this.$t('Common.NotEquals') },
                ]
                case 'TEXT': return [
                    { id: 'valueLowerCase|startsWith', value: this.$t('Common.StartsWith') },
                    { id: 'valueLowerCase|eq', value: this.$t('Common.Equals') },

                ]
                case 'NUMERIC': return [
                    { id: 'value|eq', value: this.$t('Common.Equals')},
                ]}
            return null
        },
    },
}
</script>