<template>
    <div class="btn-group btn-input clearfix">
        <button type="button"
            class="btn dropdown-toggle"
            :disabled="disabled"
            data-toggle="dropdown">
            <span data-bind="label"
                v-if="value === null"
                class="gray-text">{{ watermarkText }}</span>
            <span data-bind="label"
                v-else>{{value.title}}</span>
        </button>
        <ul ref="dropdownMenu"
            class="dropdown-menu"
            role="menu">
            <li>
                <input type="text"
                    ref="searchBox"
                    :id="searchBoxId"
                    :placeholder="$t('WebInterviewUI.Search')"
                    @input="updateOptionsList"
                    @keyup.down="onSearchBoxDownKey"
                    v-model="searchTerm" />
            </li>
            <li v-for="option in options"
                :key="option.value">
                <a href="javascript:void(0);"
                    @click="selectOption(option.value)"
                    v-html="highlight(option.title, searchTerm)"
                    @keydown.up="onOptionUpKey"></a>
            </li>
            <li v-if="isLoading"
                id="th_LoadingOptions">
                <a>{{ $t("WebInterviewUI.Loading") }}</a>
            </li>
            <li v-if="!isLoading && options.length === 0"
                id="th_NoResults">
                <a>{{ $t("WebInterviewUI.NoResultsFound") }}</a>
            </li>
        </ul>
    </div>
</template>
<script lang="js">
import { escape, escapeRegExp } from 'lodash'

export default {
    name: 'wb-typeahead',
    props: {
        value: {
            type: Object,
            default: null,
        },
        questionId: {
            type: String,
            default: null,
        },
        optionsSource: {
            type: Function,
            required: true,
        },
        disabled: {
            required: false,
            type: Boolean,
        },
        watermark: {
            required: false,
            type: String,
            default: null,
        },
    },
    computed: {
        watermarkText() {
            return this.watermark || this.$t('WebInterviewUI.ClickToAnswer')
        },
        searchBoxId() {
            return `sb_${this.questionId}`
        },
    },
    data() {
        return {
            searchTerm: '',
            options: [],
            isLoading: false,
            lastFilter: '',
        }
    },
    methods: {
        onSearchBoxDownKey() {
            const $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first()
            $firstOptionAnchor.focus()
        },
        onOptionUpKey(event) {
            const isFirstOption = $(event.target).parent().index() === 1

            if (isFirstOption) {
                this.$refs.searchBox.focus()
                event.stopPropagation()
            }
        },
        updateOptionsList(e) {
            this.loadOptions(e.target.value)
        },
        loadOptions(filter) {
            this.isLoading = true
            this.lastFilter = filter
            const currentFilter = filter

            return this.optionsSource(filter).then((options) => {
                this.isLoading = false
                if(this.lastFilter === currentFilter)
                    this.options = options || []
            })
        },
        selectOption(value) {
            this.$emit('input', value)
        },
        highlight(title, searchTerm) {
            const encodedTitle = escape(title)
            if (searchTerm) {
                const safeSearchTerm = escape(escapeRegExp(searchTerm))

                var iQuery = new RegExp(safeSearchTerm, 'ig')

                return encodedTitle.replace(iQuery, (matchedTxt) => {
                    return `<strong>${matchedTxt}</strong>`
                })
            }

            return encodedTitle
        },
    },
    mounted() {
        const jqEl = $(this.$el)
        const focusTo = jqEl.find(`#${this.searchBoxId}`)

        jqEl.on('shown.bs.dropdown', () => {
            focusTo.focus()
            this.loadOptions(this.searchTerm)
        })

        jqEl.on('hidden.bs.dropdown', () => {
            this.searchTerm = ''
        })
    },
}
</script>
