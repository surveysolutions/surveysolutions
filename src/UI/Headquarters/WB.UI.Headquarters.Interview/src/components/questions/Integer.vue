<template>
    <div class="question" v-if="$me">
        <div class="question-editor integer-question">
            <wb-title />
            <wb-instructions />
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field answered">
                            <input type="number" class="field-to-fill" placeholder="Tap to enter integer" v-model="answer" v-on:focusout="answerIntegerQuestion">
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
        name: 'IntegerQuestion',
        mixins: [entityDetails],
        data: () => {
            return {
                text: ''
            }
        },
        methods: {
            answerIntegerQuestion: function () {
                this.$store.dispatch('answerIntegerQuestion', { identity: this.entity.id, text: this.text})
            }
        }
    }
</script>
