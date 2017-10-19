<template>
    <wb-question :question="$me" :questionCssClassName="statusClass" noTitle="true" noValidation="true" noInstructions="true" noComments="true">
        <div class="options-group">
                <router-link :to="navigateTo" class="btn btn-roster-section" :class="btnStatusClass">
                    <span v-html="$me.title"></span><span v-if="this.$me.isRoster"> - <i>{{rosterTitle}}</i></span>
                </router-link>
         </div>
         <div class="information-block roster-section-info" v-if="!$me.isDisabled">
                {{this.$me.statisticsByAnswersAndSubsections}}<span v-if="hasInvalidAnswers">, </span>
                <span class="error-text" v-if="hasInvalidAnswers">{{this.$me.statisticsByInvalidAnswers}}</span>
         </div>
    </wb-question>
</template>

<script lang="js">
    import { entityDetails } from "../mixins"

    export default {
        name: 'Group',
        mixins: [entityDetails],
        computed: {
            navigateTo() {
                return {
                    name: 'section', params: {
                        sectionId: this.id,
                        interviewId: this.$route.params.interviewId
                    }
                }
            },
            rosterTitle(){
                return this.$me.rosterTitle ? `${this.$me.rosterTitle}` : "[...]"
            },
            isNotStarted() {
                return this.$me.status == "NotStarted"
            },
            isStarted() {
                return this.$me.status == "Started"
            },
            isCompleted() {
                return this.$me.status == "Completed"
            },
            hasInvalidAnswers() {
                return !this.$me.validity.isValid
            },
            btnStatusClass() {
                return [{
                    'btn-success': this.$me.validity.isValid && this.isCompleted,
                    'btn-danger': !this.$me.validity.isValid,
                    'btn-primary': !this.isCompleted ,
                    'disabled': this.$me.isDisabled
                }]
            },
            statusClass() {
                return ['roster-section-block', {
                    'started': this.$me.validity.isValid && this.isStarted,
                    'has-error': !this.$me.validity.isValid,
                    '': this.$me.validity.isValid && !this.isCompleted
                },
                {
                    'answered': this.isCompleted
                }]
            }
        }
    }
</script>
