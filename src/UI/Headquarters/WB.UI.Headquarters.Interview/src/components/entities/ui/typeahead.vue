<template>
    <select />
</template>
<script lang="ts">
    import "choices.js/assets/styles/css/base.css"
    import "choices.js/assets/styles/css/choices.css"

    import * as choices from "choices.js"

    import { apiCaller } from "src/api"

    export default {
        name: 'wb-typeahead',
        props: {
            value: {
                type: Number,
                default: null
            },
            options: {
                type: Array,
                default: () => []
            },
            questionId: {
                type: String,
                default: null
            }
        },
        data() {
            return {
                control: null
            }
        },
        watch: {
            value(newValue) {
                if (newValue === null || newValue === undefined) {
                    this.control.removeActiveItems()
                }
            }
        },
        mounted() {
            this.control = new choices(this.$el, {
                duplicateItems: false,
                searchFloor: 0,
                maxItemCount: 1,
                placeholderValue: "Enter answer",
                fuseOptions: {
                    keys: ["title"]
                },
            })
            this.control.ajax(async (callback) => {
                const options = await apiCaller(api => api.getTopFilteredOptionsForQuestion(this.questionId, null, 15000))
                callback(options, 'value', 'title')
                this.control.setValueByChoice(this.value)
            })

            this.$el.addEventListener("change", (value) => {
                this.$emit('input', value.detail.value)
            })
        },
        destroyed() {
            this.control.destroy()
            this.control = null
        }
    }
</script>
