<template>
    <main class="web-interview web-interview-for-supervisor">
        <div class="container-fluid">
            <div class="row">
                <div v-if="!isLoaded"
                    class="loading">
                    <div style="margin-top:90px">
                        {{ $t("WebInterviewUI.LoadingWait") }}
                    </div>
                </div>
                <div class="unit-section complete-section"
                    v-else>
                    <div class="wrapper-info error">
                        <div class="container-info">
                            <h2>
                                {{ $t('Assignments.CreatingNewAssignment', {questionnaire: questionnaireTitle}) }}
                                <span :title="$t('Reports.Version')">({{ this.$t('Assignments.QuestionnaireVersion', { version: this.questionnaireVersion}) }})</span>
                            </h2>
                        </div>
                    </div>
                    <component
                        v-for="entity in entities"
                        :key="`${entity.identity}-${entity.entityType}`"
                        :is="entity.entityType"
                        :id="entity.identity"
                        fetchOnMount
                        noComments="true"></component>
                    <wb-question
                        ref="ref_newResponsibleId"
                        :question="assignToQuestion"
                        noValidation="true"
                        :noComments="true"
                        :no-title="false"
                        questionCssClassName="single-select-question">
                        <h5>{{$t("Assignments.CreateAssignment_ResponsibleInstruction")}}</h5>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field"
                                        :class="{answered: newResponsibleId != null}">
                                        <Typeahead
                                            v-validate="responsibleValidations"
                                            control-id="newResponsibleId"
                                            :placeholder="$t('Common.Responsible')"
                                            :value="newResponsibleId"
                                            :ajax-params="{ }"
                                            @selected="newResponsibleSelected"
                                            :fetch-url="config.responsiblesUrl"></Typeahead>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="information-block text-danger"
                            v-if="!assignToQuestion.validity.isValid">
                            <p v-for="error in errors.collect('newResponsibleId')">{{ error }}</p>
                        </div>
                    </wb-question>

                    <wb-question
                        ref="ref_size"
                        :question="sizeQuestion"
                        noValidation="true"
                        noComments="true"
                        questionCssClassName="numeric-question">
                        <h5>{{ this.$t("Assignments.Size") }}</h5>
                        <div class="instructions-wrapper">
                            <div class="information-block instruction">
                                <p>{{ this.$t("Assignments.SizeInstructions") }}</p>
                            </div>
                        </div>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <input
                                            v-model="sizeQuestion.answer"
                                            :title="this.$t('Assignments.SizeExplanation')"
                                            v-validate="sizeValidations"
                                            name="size"
                                            maxlength="5"
                                            type="text"
                                            autocomplete="off"
                                            inputmode="numeric"
                                            class="field-to-fill">
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="information-block text-danger"
                            v-if="!sizeQuestion.validity.isValid">
                            <p>{{ this.$t("Assignments.InvalidSizeMessage") }}</p>
                            <p>{{ errors.first('size') }}</p>
                        </div>
                    </wb-question>

                    <wb-question
                        :question="webMode"
                        noValidation="true"
                        noComments="true"
                        questionCssClassName="multiselect-question">
                        <h5>{{ this.$t("Assignments.WebMode") }} <a target="_blank"
                            href="https://support.mysurvey.solutions/headquarters/cawi">
                            (?)
                        </a></h5>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <input
                                            id="webModeId"
                                            @change="webModeChange"
                                            checked="checked"
                                            v-model="webMode.answer"
                                            data-val="true"
                                            type="checkbox"
                                            class="wb-checkbox">
                                        <label for="webModeId">
                                            <span class="tick"></span>
                                            {{$t("Assignments.CawiActivated")}}
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </wb-question>

                    <wb-question
                        ref="ref_email"
                        :question="emailQuestion"
                        noValidation="true"
                        noComments="true"
                        :isDisabled="!webMode.answer"
                        questionCssClassName="text-question">
                        <h5>{{ this.$t("Assignments.Email") }}</h5>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <input
                                            v-model="emailQuestion.answer"
                                            :title="this.$t('Assignments.EmailExplanation')"
                                            :placeholder="$t('Assignments.EnterEmail')"
                                            v-validate="'email'"
                                            name="email"
                                            type="text"
                                            autocomplete="off"
                                            class="field-to-fill">
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="information-block text-danger"
                            v-if="!emailQuestion.validity.isValid">
                            <p>{{ this.$t("Assignments.InvalidEmail") }}</p>
                        </div>
                    </wb-question>

                    <wb-question
                        ref="ref_password"
                        :question="passwordQuestion"
                        noValidation="true"
                        noComments="true"
                        :isDisabled="!webMode.answer"
                        questionCssClassName="text-question">
                        <h5>{{ this.$t("Assignments.Password") }}</h5>
                        <div class="instructions-wrapper">
                            <div class="information-block instruction">
                                <p>{{ this.$t("Assignments.PasswordInstructions") }}</p>
                            </div>
                        </div>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <input
                                            v-model="passwordQuestion.answer"
                                            :placeholder="$t('Assignments.EnterPassword')"
                                            :title="this.$t('Assignments.PasswordExplanation')"
                                            v-validate="passwordValidations"
                                            name="password"
                                            type="text"
                                            autocomplete="off"
                                            class="field-to-fill">
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="information-block text-danger"
                            v-if="!passwordQuestion.validity.isValid">
                            <p>{{ this.$t("Assignments.InvalidPassword") }}</p>
                        </div>
                    </wb-question>

                    <wb-question
                        :question="isAudioRecordingEnabled"
                        noValidation="true"
                        noComments="true"
                        :isDisabled="webMode.answer"
                        questionCssClassName="multiselect-question">
                        <h5>{{ this.$t("Assignments.IsAudioRecordingEnabled") }} <a target="_blank"
                            href="https://support.mysurvey.solutions/headquarters/audio-audit/">
                            (?)
                        </a></h5>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <input
                                            id="isAudioRecordingEnabledId"
                                            checked="checked"
                                            v-model="isAudioRecordingEnabled.answer"
                                            data-val="true"
                                            type="checkbox"
                                            class="wb-checkbox">
                                        <label for="isAudioRecordingEnabledId">
                                            <span class="tick"></span>
                                            {{$t("Assignments.Activated")}}
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </wb-question>

                    <wb-question
                        :question="commentsQuestion"
                        noValidation="true"
                        noComments="true"
                        questionCssClassName="text-question">
                        <h5>{{ this.$t("Assignments.Comments") }}</h5>
                        <div class="question-unit">
                            <div class="options-group">
                                <div class="form-group">
                                    <div class="field answered">
                                        <textarea
                                            v-model="commentsQuestion.answer"
                                            :placeholder="$t('Assignments.EnterComments')"
                                            name="comments"
                                            rows="6"
                                            maxlength="500"
                                            class="form-control" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </wb-question>

                    <div class="action-container">
                        <button
                            :class="{'shake' : buttonAnimated}"
                            type="button"
                            @click="create($event)"
                            class="btn btn-success btn-lg">{{ $t('Common.Create') }}</button>
                    </div>
                </div>
            </div>
        </div>
        <IdleTimeoutService />
        <signalr @connected="connected"
            mode="takeNew"
            :interviewId="interviewId" />
    </main>
</template>

<style scoped>

.shake {
  animation: shake 0.82s cubic-bezier(.36,.07,.19,.97) both;
  transform: translate3d(0, 0, 0);
}
@keyframes shake {
  10%, 90% {
    transform: translate3d(-1px, 0, 0);
  }
  20%, 80% {
    transform: translate3d(2px, 0, 0);
  }
  30%, 50%, 70% {
    transform: translate3d(-4px, 0, 0);
  }
  40%, 60% {
    transform: translate3d(4px, 0, 0);
  }
}
</style>

<script>
import Vue from 'vue'
import { Validator } from 'vee-validate'
import * as toastr from 'toastr'
import http from '~/webinterview/api/http'
import {RoleNames} from '~/shared/constants'
import { filter } from 'lodash'
import '@/assets/css/markup-web-interview.scss'

const validationTranslations = {
    custom: {
        newResponsibleId: {
            required: () => Vue.$t('Assignments.ResponsibleRequired'),
        },
    },
}

Validator.localize('en', validationTranslations)

const emailOrPasswordRequired = {
    getMessage() {
        return Vue.$t('Assignments.SizeForWebMode')
    },
    validate(value, [email, password]) {
        return (email !== null && email !== '') || (password !== null && password !== '')
    },
    hasTarget: true,
}

const emailShouldBeEmpty = {
    getMessage() {
        return Vue.$t('Assignments.InvalidSizeWithEmail')
    },
    validate(value, [email]) {
        return email === null || email === ''
    },
    hasTarget: true,
}

Validator.extend('emailOrPasswordRequired', emailOrPasswordRequired)
Validator.extend('emailShouldBeEmpty', emailShouldBeEmpty)

Validator.extend('responsibleShouldBeInterviewer', {
    getMessage() {
        return Vue.$t('Assignments.WebModeNonInterviewer')
    },
    validate(value, [webMode]) {
        if (!webMode) return true

        return value.iconClass.toLowerCase() == RoleNames.INTERVIEWER.toLowerCase()
    },
})



export default {
    data() {
        return {
            buttonAnimated: false,
            assignToQuestion: {
                id: 'assignTo',
                acceptAnswer: true,
                isAnswered: false,
                validity: {
                    isValid: true,
                },
            },
            sizeQuestion: {
                id: 'size',
                acceptAnswer: true,
                isAnswered: true,
                answer: '1',
                validity: {
                    isValid: true,
                },
            },
            newResponsibleId: null,

            emailQuestion: {
                id: 'email',
                acceptAnswer: true,
                isAnswered: false,
                answer: null,
                validity: {
                    isValid: true,
                },
            },
            passwordQuestion: {
                id: 'password',
                acceptAnswer: true,
                isAnswered: false,
                answer: null,
                validity: {
                    isValid: true,
                },
            },
            webMode: {
                id: 'webMode',
                acceptAnswer: true,
                isAnswered: true,
                answer: false,
                validity: {
                    isValid: true,
                },
            },
            isAudioRecordingEnabled: {
                id: 'isAudioRecordingEnabled',
                acceptAnswer: true,
                isAnswered: true,
                answer: false,
                validity: {
                    isValid: true,
                },
            },
            commentsQuestion: {
                id: 'comments',
                acceptAnswer: true,
                isAnswered: true,
                answer: null,
                validity: {
                    isValid: true,
                },
            },
        }
    },
    computed: {
        sizeValidations() {
            let validations = {
                regex: '^-?([0-9]+)$',
                min_value: -1,
                max_value: this.config.maxInterviewsByAssignment,
            }

            if (this.webMode.answer) {
                if (this.sizeQuestion.answer === '1') {
                    validations.emailOrPasswordRequired = [this.emailQuestion.answer, this.passwordQuestion.answer]
                } else {
                    validations.emailShouldBeEmpty = [this.emailQuestion.answer]
                }
            }

            return validations
        },
        responsibleValidations(){
            return {
                required: true,
                responsibleShouldBeInterviewer: [ this.webMode.answer],
            }
        },
        passwordValidations() {
            return {
                regex: /^([0-9A-Z]{6,})$|^(\?)$/,
            }
        },
        entities() {
            var filteredSectionData = filter(this.$store.state.webinterview.entities, d => d.identity != 'NavigationButton')
            return filteredSectionData
        },
        isLoaded() {
            return this.$store.state.takeNew.takeNew.isLoaded
        },
        questionnaireTitle() {
            return this.$store.state.takeNew.takeNew.interview.questionnaireTitle
        },
        questionnaireVersion() {
            return this.$store.state.takeNew.takeNew.interview.questionnaireVersion
        },
        config() {
            return this.$config.model
        },
        responsibleId() {
            return this.newResponsibleId != null ? this.newResponsibleId.key : null
        },

        interviewId() {
            return this.config.id
        },
    },

    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth
            this.$store.dispatch('screenWidthChanged', screenWidth)
        },
        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
            this.assignToQuestion.isAnswered = this.newResponsibleId != null
            this.assignToQuestion.validity.isValid = this.newResponsibleId != null
        },
        async create(evnt) {
            evnt.target.disabled = true
            var validationResult = await this.$validator.validateAll()
            const self = this
            this.sizeQuestion.validity.isValid = !this.errors.has('size')
            this.emailQuestion.validity.isValid = !this.errors.has('email')
            this.passwordQuestion.validity.isValid = !this.errors.has('password')
            this.assignToQuestion.validity.isValid = !this.errors.has('newResponsibleId')

            const submitAllowed = validationResult
            if (submitAllowed) {
                this.$http
                    .post(this.config.createNewAssignmentUrl, {
                        interviewId: this.interviewId,
                        responsibleId: this.responsibleId,
                        quantity: this.sizeQuestion.answer,
                        email: this.emailQuestion.answer,
                        password: this.passwordQuestion.answer,
                        webMode: this.webMode.answer,
                        isAudioRecordingEnabled: this.isAudioRecordingEnabled.answer,
                        comments: this.commentsQuestion.answer,
                    })
                    .then(response => {
                        window.location.href = self.config.assignmentsUrl
                    })
                    .catch(e => {
                        if (e.response.data.message) toastr.error(e.response.data.message)
                        else if (e.response.data.ExceptionMessage) toastr.error(e.response.data.ExceptionMessage)
                        else toastr.error(self.$t('Pages.GlobalSettings_UnhandledExceptionMessage'))
                    })
            }
            else {
                evnt.target.disabled = false
                self.buttonAnimated = true

                setTimeout(() => {
                    self.buttonAnimated = false

                    const firstField = Object.keys(self.errors.collect())[0]

                    self.$nextTick(() => {
                        var elToScroll = self.$refs[`ref_${firstField}`]
                        if (elToScroll)
                            elToScroll.$el.scrollIntoView()
                        return
                    })

                }, 1000)
            }
        },

        webModeChange() {
            if (this.webMode.answer == false) {
                this.passwordQuestion.answer = null
                this.emailQuestion.answer = null
                this.passwordQuestion.validity.isValid = true
                this.emailQuestion.validity.isValid = true
            } else if (this.webMode.answer == true) {
                this.isAudioRecordingEnabled.answer = null
            }
        },

        connected() {
            this.$store.dispatch('loadTakeNew', { interviewId: this.interviewId })
        },
    },

    mounted() {
        const self = this

        this.$nextTick(function() {
            window.addEventListener('resize', self.onResize)
            self.onResize()
        })
    },

    updated() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight()
            window.ajustDetailsPanelHeight()
        })
    },

    components: {
        signalr: () => import(/* webpackChunkName: "core-signalr" */ '~/webinterview/components/signalr/core.signalr'),
    },

    beforeMount() {
        Vue.use(http, { store: this.$store })
    },

    beforeDestroy() {
        window.removeEventListener('resize', this.onResize)
    },
}
</script>
