<template>
    <wb-question :question="$me" :questionCssClassName="statusClass" noTitle="true" noValidation="true" noInstructions="true" noComments="true" noFlag="true">
        <div class="options-group">
                <router-link :to="navigateTo" class="btn btn-roster-section" :class="btnStatusClass">
                    <span v-html="$me.title"></span><span v-if="this.$me.isRoster"> - <i>{{rosterTitle}}</i></span>
                </router-link>
         </div>
         <div class="information-block roster-section-info" v-if="!$me.isDisabled">
                {{this.statisticsByAnswersAndSubsections}}<span v-if="hasInvalidAnswers">, </span>
                <span class="error-text" v-if="hasInvalidAnswers">{{this.statisticsByInvalidAnswers}}</span>
         </div>
    </wb-question>
</template>

<script lang="js">
    import { entityDetails } from "../mixins"
    import { GroupStatus } from "./index"
    import { debounce } from "lodash"

    export default {
        name: 'Group',
        mixins: [entityDetails],
        
        watch: {
            ["$store.getters.scrollState"]() {
                 this.scroll();
            }
        },

        mounted() {
            this.scroll();
        },

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
                return this.$me.status === GroupStatus.NotStarted
            },
            isStarted() {
                return this.$me.status === GroupStatus.Started
            },
            isCompleted() {
                return this.$me.status === GroupStatus.Completed
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
            },

            statisticsByAnswersAndSubsections() {
                switch(this.$me.status) {
                    case GroupStatus.NotStarted: 
                        return this.$t("WebInterview.Interview_Group_Status_NotStarted")
                    case GroupStatus.Started: 
                    case GroupStatus.StartedInvalid: 
                        return this.$t("WebInterview.Interview_Group_Status_StartedIncompleteFormat", {value: this.getInformationByQuestionsAndAnswers})
                    case GroupStatus.Completed: 
                    case GroupStatus.CompletedInvalid: 
                        return this.$t("WebInterview.Interview_Group_Status_CompletedFormat", {value: this.getInformationByQuestionsAndAnswers})
                }
            },
            
            statisticsByInvalidAnswers() {
                if(this.$me.stats.invalidCount > 0){
                    return this.getInformationByInvalidAnswers;
                } else return "";
            },

            getInformationByQuestionsAndAnswers() {
                return this.answeredQuestions + ", " + this.informationBySubgroups
            },

            answeredQuestions() {
                const answeredCount = this.$me.stats.answeredCount

                if(answeredCount === 1) {
                    return this.$t("WebInterview.Interview_Group_AnsweredQuestions_One")
                } else {
                    return this.$t("WebInterview.Interview_Group_AnsweredQuestions_Many", { value: answeredCount})
                }
            },

            getInformationByInvalidAnswers() {
                if(this.$me.stats.invalidCount === 1) {
                    return this.$t("WebInterview.Interview_Group_InvalidAnswers_One")
                }

                return this.$t("WebInterview.Interview_Group_InvalidAnswers_ManyFormat", { value: this.$me.stats.invalidCount })
            },

            informationBySubgroups() {
                switch(this.$me.stats.subSectionsCount) {
                    case 0: return this.$t("WebInterview.Interview_Group_Subgroups_Zero")
                    case 1: return this.$t("WebInterview.Interview_Group_Subgroups_One")
                    default: return this.$t("WebInterview.Interview_Group_Subgroups_ManyFormat", { value:  this.$me.stats.subSectionsCount })
                }
            }
        },
        methods : {
            doScroll: debounce(function() {
                if(this.$store.getters.scrollState ==  this.id){
                    window.scroll({ top: this.$el.offsetTop, behavior: "smooth" })
                    this.$store.dispatch("resetScroll")
                }
            }, 200),

            scroll() {
                if(this.$store && this.$store.state.route.hash === "#" + this.id) {
                    this.doScroll(); 
                }
            }
        }
    }
</script>
