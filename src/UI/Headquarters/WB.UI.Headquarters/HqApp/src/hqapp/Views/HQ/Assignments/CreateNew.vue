<template>
        <main class="web-interview web-interview-for-supervisor" >
            <div class="container-fluid">
                <div class="row">
                    <div class="unit-section complete-section">
                        <div class="wrapper-info error">
                            <div class="container-info">
                                <h2> {{ $t('Assignments.CreatingNewAssignment', {questionnaire: questionnaireTitle}) }} </h2>
                            </div>
                        </div>
                        <component v-for="entity in entities" 
                                :key="entity.identity"
                                 :is="entity.entityType"
                                  :id="entity.identity"
                                   fetchOnMount></component>


                        <wb-question :question="question" noValidation="true" noComments="true" showSideMenu="false"
                                    questionCssClassName="single-select-question">
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field" :class="{answered: newResponsibleId != null}">
                                            <Typeahead :placeholder="$t('Common.Responsible')"
                                                    control-id="newResponsibleId"
                                                    :value="newResponsibleId"
                                                    :ajax-params="{ }"
                                                    @selected="newResponsibleSelected"
                                                    :fetch-url="config.responsiblesUrl">
                                            </Typeahead>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </wb-question>
                    </div>
                </div>
            </div>
            <IdleTimeoutService />
        </main>
</template>

<script>
import Vue from "vue";

export default {
    data() {
        return {
            question: {
                id: 'assignTo',
                title: this.$t('Assignments.AssignTo'),
                acceptAnswer: true,
                validity: {
                    isValid: true
                }
            },
            newResponsibleId: null
        };
    },
    computed: {
        entities() {
            return this.$store.state.takeNew.takeNew.entities;
        },
        questionnaireTitle() {
            return this.$store.state.takeNew.takeNew.interview.questionnaireTitle;
        },
        config() {
            return this.$config.model;
        }
    },

    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth;
            this.$store.dispatch("screenWidthChanged", screenWidth);
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue;
        }
    },

    beforeMount() {
        return Vue.$api
            .hub({
                interviewId: window.CONFIG.model.id,
                review: false
            })
            .then(() => this.$store.dispatch("loadTakeNew"));
    },

    mounted() {
        const self = this;

        this.$nextTick(function() {
            window.addEventListener("resize", self.onResize);
            self.onResize();
        });
    },

    updated() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight();
            window.ajustDetailsPanelHeight();
        });
    },
    components: {},

    beforeDestroy() {
        window.removeEventListener("resize", this.onResize);
    }
};
</script>
