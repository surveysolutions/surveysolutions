<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input autocomplete="off" type="text" class="field-to-fill" :placeholder="'Enter answer ' + userFriendlyMask" :value="$me.answer"
                            @blur="answerTextQuestion" v-mask="$me.mask">
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
        computed: {
            userFriendlyMask(){
                if (this.$me.mask){
                    const resultMask = this.$me.mask.replace(/\*/g, "_").replace(/\#/g, "_").replace(/\~/g, "_")
                    return `(${resultMask})`
                }
            }
        },
        methods: {
            answerTextQuestion(evnt) {
                let answer:string = $(evnt.target).val()
                answer = answer ? answer.trim() : null
                if (answer) {
                    this.$store.dispatch('answerTextQuestion', { identity: this.id, text: answer })
                }
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
