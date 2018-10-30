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
                                fetchOnMount
                                noComments="true"
                                ></component>


                        <wb-question :question="assignToQuestion" 
                                    noValidation="true" 
                                    :noComments="true"
                                    :no-title="false"
                                    questionCssClassName="single-select-question">

                            <h5>
                                {{$t("Assignments.CreateAssignment_ResponsibleInstruction")}}
                            </h5>
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

                        <wb-question :question="sizeQuestion" 
                                     noValidation="true"
                                     noComments="true"
                                     
                                     questionCssClassName="numeric-question">
                            <h5>
                                {{ this.$t("Assignments.Size") }}
                            </h5>
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field answered">
                                            <input v-model="sizeQuestion.answer" 
                                                :title="this.$t('Assignments.SizeExplanation')"
                                                v-validate="'regex:^-?([0-9]+)$|min_value:-1'"
                                                maxlength="9"
                                                type="text" autocomplete="off" inputmode="numeric" class="field-to-fill"/>
                                        </div>
                                    </div>
                                  
                                </div>
                                
                            </div>
                        </wb-question>

                        <div class="action-container">
                            <form ref="createForm" :action="config.createNewAssignmentUrl" method="post">
                                <input type="hidden" name="interviewId" :vaue="config.id" />
                                <input type="hidden" name="responsibleId" :value="responsibleId"/>
                                <input type="hidden" name="size" :value="sizeQuestion.answer"/>

                                <button type="button" @click="create" class="btn btn-success btn-lg">
                                    {{ $t('Common.Create') }}
                                </button>
                            </form>
                            
                        </div>
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
            assignToQuestion: {
                id: "assignTo",
                acceptAnswer: true,
                isAnswered: false,
                validity: {
                    isValid: true
                }
            },
            sizeQuestion: {
                id: "size",
                acceptAnswer: true,
                isAnswered: true,
                answer: "1",
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
            return this.$store.state.takeNew.takeNew.interview
                .questionnaireTitle;
        },
        config() {
            return this.$config.model;
        },
        responsibleId() {
            return this.newResponsibleId != null
                ? this.newResponsibleId.key
                : null;
        }
    },

    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth;
            this.$store.dispatch("screenWidthChanged", screenWidth);
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
            this.assignToQuestion.isAnswered = this.newResponsibleId != null
            this.assignToQuestion.validity.isValid = this.newResponsibleId != null
        },
        async create(ev) {
            const validationResult = await this.$validator.validateAll()
           
            this.sizeQuestion.validity.isValid = validationResult
            
            if(this.newResponsibleId == null) {
                this.assignToQuestion.validity.isValid = false
            }

            const submitAllowed = validationResult && this.newResponsibleId != null
            if (submitAllowed) {
                this.$refs.createForm.submit()
            }
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
