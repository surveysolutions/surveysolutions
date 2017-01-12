<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="text" class="field-to-fill" :placeholder="$me.mask || 'Tap to enter text'" :value="$me.answer" v-on:focusout="answerTextQuestion"
                            v-mask="$me.mask">
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"

    export default {
        name: 'TextQuestion',
        mixins: [entityDetails],
        methods: {
            answerTextQuestion(evnt) {
                this.$store.dispatch('answerTextQuestion', { identity: this.id, text: $(evnt.target).val() })
            }
        },
        directives: {
            mask: {
                update: (el, binding) => {
                    if (binding.value) {
                        $(el).mask(binding.value, {
                            translation: {
                                "~": { pattern: /[a-zA-Z]/ },
                                "#": { pattern: /\d/ },
                                "*": { pattern: /[a-zA-Z0-9]/ }
                            }
                        })
                    }
                }
            }
        }
    }
</script>
