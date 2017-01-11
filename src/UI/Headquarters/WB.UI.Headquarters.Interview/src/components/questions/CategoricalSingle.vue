<template>
    <div class="question" v-if="$me">
        <div class="question-editor single-select-question" :class="{answered: answer != null}">
            <wb-title />
            <wb-instructions />
            <div class="question-unit">
                <div class="options-group">
                    <div class="radio" v-for="option in $me.options">
                        <div class="field">
                            <input class="wb-radio" type="radio" :id="$me.id + '_' + option.value" :value="option.value" v-model="answer" @change="submitAnswer">
                            <label :for="$me.id + '_' + option.value">
                                <span class="tick"></span> {{option.title}}
                            </label>
                            <button type="submit" class="btn btn-link btn-clear" @click="removeAnswer">
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
        name: 'CategoricalSingle',
        data: () => {
            return {
                answer: null
            }
        },
        mixins: [entityDetails],
        methods: {
            submitAnswer() {
                this.$store.dispatch("answerSingleOptionQuestion", { answer: this.answer, questionId: this.$me.id })
            },
            removeAnswer() {
                this.$store.dispatch("removeAnswer", this.$me.id)
            }
        }
    }
</script>
