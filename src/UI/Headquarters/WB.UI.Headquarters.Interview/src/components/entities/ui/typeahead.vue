<template>
    <div class="btn-group btn-input clearfix">
        <button type="button" class="btn dropdown-toggle" data-toggle="dropdown">
            <span data-bind="label" v-if="value === null" class="gray-text">Click to answer</span>
            <span data-bind="label" v-else >{{value.title}}</span>
        </button>
        <ul class="dropdown-menu" role="menu">
            <li>
                <input type="text" :id="searchBoxId" placeholder="Search" @input="updateOptionsList" />
            </li>
            <li v-for="option in options" :key="option.value">
                <a href="javascript:void(0);" @click="selectOption(option.value)">{{option.title}}</a>
            </li>
        </ul>
    </div>
</template>
<script lang="ts">
    import "bootstrap"
    import * as $ from "jquery"

    import { apiCaller } from "src/api"

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
                control: null,
                options: []
            }
        },
        methods: {
            updateOptionsList(e) {
                this.loadOptions(e.target.value)
            },
            async loadOptions(filter:string) {
                const options = await apiCaller(api => api.getTopFilteredOptionsForQuestion(this.questionId, filter, 30))
                this.options = options
            },
            selectOption(value) {
                this.$emit('input', value)
            }
        },
        mounted() {
            const jqEl = $(this.$el)
            const focusTo = jqEl.find(`#${this.searchBoxId}`)
            jqEl.on('shown.bs.dropdown', () => {
                    focusTo.focus()
                })

            jqEl.on('hidden.bs.dropdown', () => {
                focusTo.text('')
            })

            this.loadOptions(null)
        }
    }
</script>
