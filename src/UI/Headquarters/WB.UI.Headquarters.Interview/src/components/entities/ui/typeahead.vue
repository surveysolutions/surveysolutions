<template>
    <div class="btn-group btn-input clearfix">
        <button type="button" class="btn dropdown-toggle" data-toggle="dropdown">
            <span data-bind="label" v-if="value === null" class="gray-text">Click to answer</span>
            <span data-bind="label" v-else>{{value.title}}</span>
        </button>
        <ul ref="dropdownMenu" class="dropdown-menu" role="menu">
            <li>
                <input type="text" ref="searchBox" :id="searchBoxId" placeholder="Search" @input="updateOptionsList" @keyup.down="onSearchBoxDownKey" v-model="searchTerm" />
            </li>
            <li v-for="option in options" :key="option.value">
                <a href="javascript:void(0);" @click="selectOption(option.value)" v-html="highlight(option.title, searchTerm)" @keydown.up="onOptionUpKey"></a>
            </li>
            <li v-if="isLoading">
                <a>Loading...</a>
            </li>
            <li v-if="!isLoading && options.length === 0">
                <a>No results found</a>
            </li>
        </ul>
    </div>
</template>
<script lang="js">

    import * as $ from "jquery"

    import { apiCaller } from "src/api"
    import * as escape from "lodash/escape"
    import * as escapeRegExp from "lodash/escapeRegExp"

    export default {
        name: 'wb-typeahead',
        props: {
            value: {
                type: Object,
                default: null
            },
            questionId: {
                type: String,
                default: null
            }
        },
        computed: {
            searchBoxId() {
                return `sb_${this.questionId}`
            }
        },
        data() {
            return {
                searchTerm: '',
                options: [],
                isLoading: false
            }
        },
        methods: {
            onSearchBoxDownKey(event) {
                const $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first();
                $firstOptionAnchor.focus()
            },
            onOptionUpKey(event) {
                const isFirstOption = $(event.target).parent().index() === 1;

                if (isFirstOption) {
                    this.$refs.searchBox.focus()
                    event.stopPropagation()
                }
            },
            updateOptionsList(e) {
                this.loadOptions(e.target.value)
            },
            async loadOptions(filter) {
                this.isLoading = true
                const options = await apiCaller(api => api.getTopFilteredOptionsForQuestion(this.questionId, filter, 30))
                this.isLoading = false
                this.options = options || []
            },
            selectOption(value) {
                this.$emit('input', value)
            },
            highlight(title, searchTerm) {
                const encodedTitle = escape(title)
                if (searchTerm) {
                    const safeSearchTerm = escape(escapeRegExp(searchTerm))

                    var iQuery = new RegExp(safeSearchTerm, "ig")
                    return encodedTitle.replace(iQuery, (matchedTxt, a, b) => {
                        return `<strong>${matchedTxt}</strong>`
                    })
                }

                return encodedTitle
            }
        },
        mounted() {
            const jqEl = $(this.$el)
            const focusTo = jqEl.find(`#${this.searchBoxId}`)
            jqEl.on('shown.bs.dropdown', () => {
                focusTo.focus()
                this.loadOptions(this.searchTerm)
            })

            jqEl.on('hidden.bs.dropdown', () => {
                this.searchTerm = ""
            })
        }
    }
</script>
