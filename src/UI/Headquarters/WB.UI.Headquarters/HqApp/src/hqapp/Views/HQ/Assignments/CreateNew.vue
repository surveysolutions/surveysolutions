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
                                            <Typeahead control-id="newResponsibleId"
                                                    :placeholder="$t('Common.Responsible')"
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
                            <div class="instructions-wrapper">
                                <div class="information-block instruction">
                                    <p>{{ this.$t("Assignments.SizeInstructions") }}</p>
                                </div>
                            </div>
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field answered">
                                            <input v-model="sizeQuestion.answer" 
                                                :title="this.$t('Assignments.SizeExplanation')"
                                                v-validate="sizeValidations" name="size"
                                                maxlength="5"
                                                type="text" autocomplete="off" inputmode="numeric" class="field-to-fill" />
                                        </div>
                                    </div>                                  
                                </div>                                
                            </div>
                            <div class="information-block text-danger" v-if="!sizeQuestion.validity.isValid">
                                <p> {{ this.$t("Assignments.InvalidSizeMessage") }} </p>
                                <p>{{ errors.first('size') }}</p>
                                <p v-if="sizeQuestion.answer !== '1' && (emailQuestion.answer !== null || emailQuestion.answer !== '')"> {{ this.$t("Assignments.InvalidSizeWithEmail") }} </p>                                                             
                            </div>
                        </wb-question>

                        <wb-question :question="webMode" 
                                     noValidation="true"
                                     noComments="true"                                     
                                     questionCssClassName="multiselect-question">
                            <h5>
                                {{ this.$t("Assignments.WebMode") }}
                            </h5>
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field answered">
                                            <input id="webModeId"
                                                @change="webModeChange"
                                                checked="checked"                                            
                                                v-model="webMode.answer"   
                                                data-val="true"                                                                                 
                                                type="checkbox" class="wb-checkbox"/>
                                                <label for="webModeId">
                                                    <span class="tick"></span> {{$t("Assignments.Activated")}}
                                                </label>
                                        </div>
                                    </div>                                  
                                </div>                                
                            </div>
                        </wb-question>

                        <wb-question :question="emailQuestion" 
                                     noValidation="true"
                                     noComments="true"
                                     :isDisabled = "!webMode.answer"                                                                           
                                     questionCssClassName="text-question">
                            <h5>
                                {{ this.$t("Assignments.Email") }}
                            </h5>
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field answered">
                                            <input v-model="emailQuestion.answer" 
                                                :title="this.$t('Assignments.EmailExplanation')"
                                                :placeholder="$t('Assignments.EnterEmail')"
                                                v-validate="'email'" name="email"
                                                type="text" autocomplete="off" class="field-to-fill"/>
                                        </div>
                                    </div>                                  
                                </div>                                
                            </div>
                            <div class="information-block text-danger" v-if="!emailQuestion.validity.isValid">
                                <p> {{ this.$t("Assignments.InvalidEmail") }} </p>                                                             
                            </div>
                        </wb-question>
                        
                        <wb-question :question="passwordQuestion" 
                                     noValidation="true"
                                     noComments="true"
                                     :isDisabled = "!webMode.answer"                                                                         
                                     questionCssClassName="text-question">
                            <h5>
                                {{ this.$t("Assignments.Password") }}
                            </h5>
                            <div class="instructions-wrapper">
                                <div class="information-block instruction">
                                    <p>{{ this.$t("Assignments.PasswordInstructions") }}</p>
                                </div>
                            </div>
                            <div class="question-unit">
                                <div class="options-group">
                                    <div class="form-group">
                                        <div class="field answered">
                                            <input v-model="passwordQuestion.answer"
                                                :placeholder="$t('Assignments.EnterPassword')" 
                                                :title="this.$t('Assignments.PasswordExplanation')"
                                                v-validate="passwordValidations"                                                
                                                name="password"
                                                type="text" autocomplete="off" class="field-to-fill"/>
                                        </div>
                                    </div>                                  
                                </div>                                
                            </div>
                            <div class="information-block text-danger" v-if="!passwordQuestion.validity.isValid">
                                <p> {{ this.$t("Assignments.InvalidPassword") }} </p>
                            </div>
                        </wb-question>                       

                        <div class="action-container">
                            <form ref="createForm" :action="config.createNewAssignmentUrl" method="post">
                                <input type="hidden" name="interviewId" :vaue="config.id" />
                                <input type="hidden" name="responsibleId" :value="responsibleId"/>
                                <input type="hidden" name="size" :value="sizeQuestion.answer"/>
                                <input type="hidden" name="email" :value="emailQuestion.answer"/>
                                <input type="hidden" name="password" :value="passwordQuestion.answer"/>
                                <input type="hidden" name="webMode" :value="webMode.answer"/>

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
import { Validator } from 'vee-validate';

const sizeValidForWebMode = {
    getMessage(field, args) {
        const result = Vue.$t('Assignments.SizeForWebMode')
        return result
    },
    validate(value, [isWebMode, email, password]) {
        if(!isWebMode) return true
        if(value === "1") 
            return (email !== null && email !== "") || (password !== null && password !== "")

        if(value !== "1") {
            return email == null || email == ""
        }
    },
    hasTarget: true
}

Validator.extend('sizeValidForWebMode', sizeValidForWebMode);

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
            newResponsibleId: null,

            emailQuestion: {
                id: "email",
                acceptAnswer: true,
                isAnswered: false,
                answer:null,
                validity: {
                    isValid: true
                }
            },
            passwordQuestion: {
                id: "password",
                acceptAnswer: true,
                isAnswered: false,
                answer:null,
                validity: {
                    isValid: true
                }
            },
            webMode: {
                id: "webMode",
                acceptAnswer: true,
                isAnswered: true,
                answer: false,                
                validity: {
                    isValid: true
                }
            }

        };
    },
    computed: {
        sizeValidations(){
            return {
                regex: "^-?([0-9]+)$",
                min_value: -1,
                max_value: this.config.maxInterviewsByAssignment,
                sizeValidForWebMode: [this.webMode.answer, this.emailQuestion.answer, this.passwordQuestion.answer]
            };
        }, 
        passwordValidations(){
            return {
                regex: "^([0-9A-Z]{6,})|\\?$"
            };
        },        
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
            var validationResult = await this.$validator.validateAll();

            this.sizeQuestion.validity.isValid = !this.errors.has('size')            
            this.emailQuestion.validity.isValid = !this.errors.has('email')
            this.passwordQuestion.validity.isValid = !this.errors.has('password') 
            if(this.newResponsibleId == null) {
                this.assignToQuestion.validity.isValid = false
            }

            const submitAllowed = validationResult && this.newResponsibleId != null
            if (submitAllowed) {
                this.$refs.createForm.submit()
            }
        },
        webModeChange(){
            if(this.webMode.answer == false){
                this.passwordQuestion.answer = null;
                this.emailQuestion.answer = null;
                this.passwordQuestion.validity.isValid = true;
                this.emailQuestion.validity.isValid = true;
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
