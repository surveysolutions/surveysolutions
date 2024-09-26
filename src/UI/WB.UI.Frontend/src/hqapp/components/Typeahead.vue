<template>
    <div class="combo-box" :title="value == null ? '' : value.value" :id="controlId">
        <div class="btn-group btn-input clearfix">
            <button type="button" :id="buttonId" class="btn dropdown-toggle" data-bs-toggle="dropdown"
                :disabled="disabled">
                <span data-bind="label" v-if="value == null" class="gray-text">{{ placeholderText }}</span>
                <span data-bind="label" :class="[value.iconClass]" v-else>{{ value.value }}</span>
            </button>
            <ul ref="dropdownMenu" class="dropdown-menu" role="menu">
                <li v-if="!noSearch">
                    <input type="text" ref="searchBox" :id="inputId" :placeholder="$t('Common.Search')"
                        @input="updateOptionsList" @keyup.down="onSearchBoxDownKey" v-model="searchTerm" />
                </li>
                <li v-if="forceLoadingState">
                    <a>{{ $t("Common.Loading") }}</a>
                </li>
                <template v-if="!forceLoadingState">
                    <li v-for="option in options" :key="keyFunc(option.item)">
                        <a :class="[option.item.iconClass]" href="javascript:void(0);"
                            @click="selectOption(option.item)" v-html="highlight(option, searchTerm)"
                            @keydown.up="onOptionUpKey"></a>
                    </li>
                </template>
                <li v-if="isLoading">
                    <a>{{ $t("Common.Loading") }}</a>
                </li>
                <li v-if="!isLoading && options.length === 0">
                    <a>{{ $t("Common.NoResultsFound") }}</a>
                </li>
            </ul>
        </div>
        <button v-if="value != null && !noClear" class="btn btn-link btn-clear" type="button" @click="clear">
            <span></span>
        </button>
    </div>
</template>

<script>
import { assign, chain, find, filter, escape, escapeRegExp } from 'lodash'


export default {
    name: 'Typeahead',

    emits: ['selected', 'update:modelValue'],

    //TODO: MIGRATION

    // http://vee-validate.logaretm.com/v2/concepts/components.html#component-constructor-options
    //$_veeValidate: {
    //    name: function () { return this.controlId },
    //},    

    props: {
        fetchUrl: String,
        name: String,
        controlId: {
            type: String,
            required: true,
        },
        value: Object,
        placeholder: String,
        ajaxParams: Object,
        forceLoadingState: {
            type: Boolean,
            default: false,
        },
        values: Array,
        noSearch: Boolean,
        noClear: Boolean,
        disabled: {
            type: Boolean,
            default: false,
        },
        selectFirst: {
            type: Boolean,
            default: false,
        },
        selectedKey: {
            type: String,
            default: null,
        },
        selectedValue: {
            type: String,
            default: null,
        },
    },
    watch: {
        fetchUrl(val) {
            this.clear()
            if (val) {
                this.fetchOptions()
            }
            else {
                this.options.splice(0, this.options.length)
            }
        },
        disabled(val) {
            if (val)
                this.clear()
            else
                this.fetchOptions()
        },
    },
    data() {
        return {
            options: [],
            isLoading: false,
            searchTerm: '',
        }
    },

    computed: {
        inputId() {
            return `sb_${this.controlId}`
        },
        buttonId() {
            return `btn_trigger_${this.controlId}`
        },
        placeholderText() {
            return this.placeholder || 'Select'
        },
    },

    mounted() {
        const jqEl = $(this.$el)
        const focusTo = jqEl.find(`#${this.inputId}`)

        jqEl.on('shown.bs.dropdown', () => {
            focusTo.focus()
            this.fetchOptions()
        })

        jqEl.on('hidden.bs.dropdown', () => {
            this.searchTerm = ''
        })

        this.fetchOptions(this.searchTerm || this.selectedValue)
    },

    methods: {
        onSearchBoxDownKey() {
            const $firstOptionAnchor = $(this.$refs.dropdownMenu)
                .find('a')
                .first()
            $firstOptionAnchor.focus()
        },
        onOptionUpKey(event) {
            const isFirstOption =
                $(event.target)
                    .parent()
                    .index() === 1

            if (isFirstOption) {
                this.$refs.searchBox.focus()
                event.stopPropagation()
            }
        },

        fetchOptions(query) {
            if (this.values) {
                this.fetchLocalOptions()
                return
            }

            this.isLoading = true

            const self = this
            const selectedKey = self.selectedKey
            const selectedValue = self.selectedValue

            const requestParams = assign(
                { query: query || this.searchTerm, cache: false },
                this.ajaxParams
            )
            return this.$http
                .get(this.fetchUrl, { params: requestParams })
                .then(response => {
                    if (response != null && response.data != null) {
                        self.options = self.setOptions(response.data.options || [])

                        if (self.options.length > 0) {
                            if (selectedKey != null) {
                                self.selectByKey(selectedKey)
                            }
                            else if (selectedValue != null) {
                                self.selectByValue(selectedValue)
                            }
                            else if (self.selectFirst && self.value == null) {
                                self.selectOption(self.options[0].item)
                            }
                        }
                    }

                    self.isLoading = false
                })
                .catch(() => (self.isLoading = false))
        },
        fetchLocalOptions() {
            if (this.searchTerm != '') {
                const search = this.searchTerm.toLowerCase()
                const filtered = filter(this.values, v => v.value.toLowerCase().indexOf(search) >= 0)
                this.options = this.setOptions(filtered)
            } else {
                this.options = this.setOptions(this.values)
            }

            if (this.selectedKey != null) {
                this.selectByKey(this.selectedKey)
            }
            else if (this.selectedValue != null) {
                this.selectByValue(this.selectedValue)
            }
            else if (this.selectFirst && this.value == null) {
                this.selectOption(this.options[0].item)
            }
        },
        setOptions(values, wrap = true) {
            if (wrap == false) return values

            return chain(values).filter(v => v != null).map(v => {
                return {
                    item: v,
                    matches: null,
                }
            }).value()
        },

        clear() {
            this.$emit('selected', null, this.controlId)
            this.$emit('update:modelValue', null);
            this.searchTerm = ''
        },
        selectOption(value) {
            this.$emit('selected', value, this.controlId)
            this.$emit('update:modelValue', value);
        },
        selectByKey(key) {
            const itemToSelect = find(this.options, o => o.item.key == key)
            if (itemToSelect != null) {
                this.selectOption(itemToSelect.item)
            }
        },
        selectByValue(value) {
            const itemToSelect = find(this.options, o => o.item.value == value)
            if (itemToSelect != null) {
                this.selectOption(itemToSelect.item)
            }
        },
        updateOptionsList(e) {
            this.searchTerm = e.target.value
            this.fetchOptions()
        },
        highlight(option, searchTerm) {
            if (option.matches == null) {
                const encodedTitle = escape(option.item.value)

                if (searchTerm) {
                    const safeSearchTerm = escape(escapeRegExp(searchTerm))
                    const iQuery = new RegExp(safeSearchTerm, 'ig')

                    return encodedTitle.replace(iQuery, matchedTxt => {
                        return `<strong>${matchedTxt}</strong>`
                    })
                }

                return encodedTitle
            } else {
                return generateHighlightedText(
                    option.item.value,
                    option.matches[0].indices
                )
            }
        },
        keyFunc(item) {
            return item == null ? 'null' : item.key + '$' + item.value
        },
    },
}

// Does not account for overlapping highlighted regions, if that exists at all O_o..
function generateHighlightedText(
    text,
    regions,
    start = '<strong>',
    end = '</strong>'
) {
    if (!regions) return text

    var content = '',
        nextUnhighlightedRegionStartingIndex = 0

    regions.forEach(region => {
        content +=
            '' +
            text.substring(nextUnhighlightedRegionStartingIndex, region[0]) +
            start +
            text.substring(region[0], region[1] + 1) +
            end +
            ''

        nextUnhighlightedRegionStartingIndex = region[1] + 1
    })

    content += text.substring(nextUnhighlightedRegionStartingIndex)

    return content
}
</script>
