<template>
    <div class="question" v-if="$me">
        <div class="question-editor real-question">
            <wb-title />
            <wb-instructions />
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field answered">
                            <input type="number" class="field-to-fill" placeholder="Tap to enter numeric" v-model="answer" v-on:focusout="answerRealQuestion">
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
        name: 'RealQuestion',
        mixins: [entityDetails],
        data: () => {
            return {
                text: ''
            }
        },
        methods: {
            answerRealQuestion: function () {
                this.$store.dispatch('answerRealQuestion', { identity: this.entity.id, text: this.text})
            }
        }
    }
</script>
