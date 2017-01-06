<template>
    <div class="question" v-if="$me">
        <div class="question-editor text-question">
            <wb-title  />
            <wb-instructions />
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field answered">
                            <input type="text" class="field-to-fill" placeholder="Tap to enter number" value="Selling groceries">
                            <button type="submit" class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'TextQuestion',
        mixins: [entityDetails],
        props: ['entity'],
        beforeMount() {
            this.fetch()
        },
        methods: {
            fetch() {
                this.$store.dispatch('fetchTextQuestion', this.entity)
            }
        }
    }
</script>
