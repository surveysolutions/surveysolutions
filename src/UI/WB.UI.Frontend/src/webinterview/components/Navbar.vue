<template>
    <nav class="navbar navbar-inverse navbar-fixed-top navbar-web-interview" role="navigation">
        <div class="container-fluid">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <router-link class="interview-ID" active-class :to="toFirstSection"
                    v-if="$store.state.webinterview.firstSectionId">{{ interviewKey }}</router-link>
                <button type="button" class="navbar-toggle collapsed" data-bs-toggle="collapse" data-bs-target="#navbar"
                    aria-expanded="false">
                    <span class="sr-only">{{ $t('WebInterviewUI.ToggleNavigation') }}</span>
                    <span class="icon-bar top-menu"></span>
                    <span class="icon-bar mid-menu"></span>
                    <span class="icon-bar bottom-menu"></span>
                </button>
                <div class="navbar-brand">
                    <router-link class="logo" :to="toCoverPage" v-if="hqLink == null"></router-link>
                    <a :href="hqLink" v-if="hqLink != null" class="logo"></a>
                </div>
                <button v-if="this.$config.inWebTesterMode" type="button" class="btn btn-default btn-link btn-icon"
                    @click="reloadQuestionnaire" :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                    <span class="glyphicon glyphicon-sort"></span>
                </button>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <ul class="nav navbar-nav navbar-left">
                    <li>
                        <router-link class="interview-ID" active-class :to="toFirstSection"
                            v-if="$store.state.webinterview.firstSectionId">{{ interviewKey }}</router-link>
                    </li>
                </ul>
                <p class="navbar-text pull-left">{{ questionnaireTitle }}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="help-link">
                        <a href="https://support.mysurvey.solutions/getting-started/web-interview"
                            :title="$t('WebInterviewUI.Help')">{{ $t('WebInterviewUI.Help') }}</a>
                    </li>
                    <li class="dropdown language">
                        <a href="javascript:void(0);" class="dropdown-toggle" data-bs-toggle="dropdown" role="button"
                            aria-haspopup="true" aria-expanded="false" :title="currentLanguage">
                            {{ currentLanguage }}
                            <span class="caret" v-if="canChangeLanguage"></span>
                        </a>
                        <ul class="dropdown-menu" v-if="canChangeLanguage">
                            <li v-if="currentLanguage != $store.state.webinterview.originalLanguageName">
                                <a href="javascript:void(0)" @click="changeLanguage()">{{
                                    $store.state.webinterview.originalLanguageName }}</a>
                            </li>
                            <li :key="language.OriginalLanguageName" v-for="language in languages">
                                <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li v-if="showEmailPersonalLink">
                        <a href="#" @click="emailPersonalLink"
                            :title="$t('WebInterviewUI.EmailLink_EmailResumeLink')">{{
                                $t('WebInterviewUI.EmailLink_EmailResumeLink') }}</a>
                    </li>
                    <li v-if="this.$config.inWebTesterMode">
                        <button v-if="includeVariables" type="button" class="btn btn-default btn-link btn-icon"
                            @click="hideVariables" :title="$t('WebInterviewUI.HideVariables')">
                            <span class="glyphicon glyphicon-check"></span>
                        </button>
                        <button v-if="!includeVariables" type="button" class="btn btn-default btn-link btn-icon"
                            @click="showVariables" :title="$t('WebInterviewUI.ShowVariables')">
                            <span class="glyphicon glyphicon-unchecked"></span>
                        </button>
                    </li>
                    <li v-if="this.$config.inWebTesterMode">
                        <button type="button" class="btn btn-default btn-link btn-icon" @click="reloadQuestionnaire"
                            :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                            <span class="glyphicon glyphicon-sort"></span>
                        </button>
                    </li>
                    <li v-if="this.$config.inWebTesterMode && this.$config.saveScenarioUrl">
                        <button type="button" class="btn btn-default btn-link btn-icon" @click="showSaveScenario"
                            :title="$t('WebInterviewUI.SaveScenario')">
                            <span class="glyphicon glyphicon-floppy-disk"></span>
                        </button>
                        <div class="modal fade" id="saveScenarioModal" ref="saveScenarioModalRef" tabindex="-1"
                            role="dialog">
                            <div class="modal-dialog" role="document">
                                <div class="modal-content">
                                    <div class="modal-header">
                                        <h2>{{ this.$t('WebInterviewUI.SaveScenario') }}</h2>
                                    </div>
                                    <div class="modal-body">
                                        <form onsubmit="return false;" action="javascript:void(0)" class="panel-group"
                                            v-if="!this.designerCredentialsExpired">
                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <div class="radio mb-1">
                                                        <input name="rbScenarioSaveOption" type="radio"
                                                            id="rbScenarioSaveNew" value="saveNew" class="wb-radio"
                                                            v-model="selectedSaveOption" />
                                                        <label for="rbScenarioSaveNew">
                                                            <span class="tick"></span>
                                                            {{ this.$t('WebInterviewUI.SaveScenarioOptions_SaveNew') }}
                                                        </label>
                                                    </div>
                                                </div>
                                                <div class="panel-body" v-if="selectedSaveOption === 'saveNew'">
                                                    <div class="form-group" v-if="selectedSaveOption === 'saveNew'">
                                                        <label for="txtScenarioName" class="control-label">{{
                                                            this.$t('WebInterviewUI.SaveScenarioName') }}</label>
                                                        <input maxlength="32" id="txtScenarioName" class="form-control"
                                                            v-model="newScenarioName" />
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <div class="radio mb-1">
                                                        <input name="rbScenarioSaveOption" type="radio"
                                                            id="rbScenarioUpdateExisting" value="updateExisting"
                                                            class="wb-radio" v-model="selectedSaveOption" />
                                                        <label for="rbScenarioUpdateExisting">
                                                            <span class="tick"></span>
                                                            {{
                                                                this.$t('WebInterviewUI.SaveScenarioOptions_ReplaceExisting')
                                                            }}
                                                        </label>
                                                    </div>
                                                </div>
                                                <div class="panel-body" v-if="selectedSaveOption === 'updateExisting'">
                                                    <div class="form-group">
                                                        <label for="slScenarioSaveOption" class="control-label">{{
                                                            this.$t('WebInterviewUI.SaveScenarioOptions') }}</label>
                                                        <select id="slScenarioSaveOption" class="form-control"
                                                            v-model="selectedScenarioOption">
                                                            <option v-for="s in designerScenarios" :value="s.id"
                                                                :key="s.id">{{ s.title }}</option>
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                        </form>
                                        <div v-else>
                                            <p>{{ this.$t('WebInterviewUI.SaveScenarioDesignerLogin') }}</p>
                                            <a :href="this.$config.designerUrl" target="_blank">{{
                                                this.$t('WebInterviewUI.SaveScenarioGoToDesigner') }}</a>
                                        </div>
                                    </div>
                                    <div class="modal-footer" v-if="!this.designerCredentialsExpired">
                                        <button type="button" class="btn btn-primary"
                                            :disabled="scenarioSaving || !scenarioValid" @click="saveScenario">{{
                                                $t("Common.Save") }}</button>
                                        <button type="button" class="btn btn-link" @click="hideScenarioSave">{{
                                            $t("Common.Cancel") }}</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </li>
                </ul>
            </div>
            <!-- /.navbar-collapse -->
        </div>
        <!-- /.container-fluid -->

        <teleport to="body">
            <div ref="emailPersonalLinkModalRef" class="modal fade" id="emailPersonalLinkModal" tabindex="-1"
                role="dialog">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h2>{{ $t('WebInterviewUI.EmailLink_Header') }}</h2>
                        </div>
                        <div class="modal-body">
                            <form onsubmit="return false;" action="javascript:void(0)">
                                <p>{{ $t('WebInterviewUI.EmailLink_Message') }}</p>
                                <p>{{ $t('WebInterviewUI.EmailLink_ResumeAnyTime') }}</p>
                                <div class="form-group">
                                    <input type="email" id="txtEmail" class="form-control"
                                        :placeholder="this.$t('WebInterviewUI.EmailLink_Placeholder')" />
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" @click="sendEmailWithPersonalLink">{{
                                $t("Common.Ok") }}</button>
                            <button type="button" class="btn btn-link" @click="hideEmailPersonalLink"
                                data-bs-dismiss="modal">{{ $t("Common.Cancel")
                                }}</button>
                        </div>
                    </div>
                </div>
            </div>
        </teleport>

    </nav>
</template>
<script lang="js">
import axios from 'axios'
import * as toastr from 'toastr'
import { filter } from 'lodash'
import { Modal } from 'bootstrap'
import { nextTick } from 'vue'

export default {
    name: 'navbar',
    data() {
        return {
            showEmailPersonalLink: this.$config.askForEmail,
            scenarioText: null,
            designerScenarios: [],
            selectedScenarioOption: -1,
            scenarioSaving: false,
            designerCredentialsExpired: false,
            selectedSaveOption: 'saveNew',
            newScenarioName: '',
            emailModal: null,
        }
    },
    mounted() {

        $('.navbar-toggle').click(function () {
            $('.navbar-collapse').fadeToggle()
            $('.navbar-collapse').animate({ height: '100%' }, 0)
            $('.top-menu').toggleClass('top-animate')
            $('.mid-menu').toggleClass('mid-animate')
            $('.bottom-menu').toggleClass('bottom-animate')

        })

        if (this.$config.askForEmail) {
            this.emailPersonalLink()
        }
    },
    updated() {
        document.title = this.$config.splashScreen ? this.$t('WebInterviewUI.LoadingQuestionnaire') : `${this.$store.state.webinterview.interviewKey} | ${this.questionnaireTitle} | ${this.$t('WebInterviewUI.WebInterview')}`
    },
    computed: {
        scenarioValid() {
            if (this.selectedSaveOption === 'saveNew') {
                return this.newScenarioName !== ''
            }
            else {
                return this.selectedScenarioOption !== -1
            }
        },
        canChangeLanguage() {
            return this.$store.state.webinterview.languages != undefined && this.$store.state.webinterview.languages.length > 0
        },
        currentLanguage() {
            return this.$store.state.webinterview.currentLanguage || this.$store.state.webinterview.originalLanguageName
        },
        languages() {
            const current = this.currentLanguage
            return filter(this.$store.state.webinterview.languages, l => l != current)
        },
        questionnaireTitle() {
            if (this.$config.splashScreen) return this.$t('WebInterviewUI.LoadingQuestionnaire')

            return this.$store.state.webinterview.questionnaireTitle || ''
        },
        toFirstSection() {
            return { name: 'section', params: { sectionId: this.$store.state.webinterview.firstSectionId } }
        },
        toCoverPage() {
            return { name: 'prefilled' }
        },
        interviewKey() {
            return this.$store.state.webinterview.interviewKey || this.$t('WebInterviewUI.WebInterview')
        },
        hqLink() {
            return this.$config.hqLink
        },
        saveScenarioUrl() {
            return this.$config.saveScenarioUrl
        },
        getScenarioUrl() {
            return this.$config.getScenarioUrl
        },
        includeVariables() {
            return this.$store.state.webinterview.showVariables || false;
        },
    },
    methods: {
        hideScenarioSave() {
            $(this.$refs.saveScenarioModalRef).modal('hide')
            this.newScenarioName = ''
            this.selectedSaveOption = 'saveNew'
            this.selectedScenarioOption = -1
        },
        emailPersonalLink() {
            nextTick(() => {
                this.emailModal = new Modal(this.$refs.emailPersonalLinkModalRef)
                this.emailModal.show()
            })
        },
        sendEmailWithPersonalLink() {
            const emailInput = $('#txtEmail')
            const email = emailInput.val()
            if (email === null || email === undefined) return

            var self = this
            if (!self.validateEmail(email)) {
                self.$nextTick(function () {
                    emailInput.next('span').remove()
                    emailInput.after('<span class=\'help-text text-danger\'>' + this.$t('WebInterviewUI.EmailLink_InvalidEmail', { email: email }) + '</span>')
                })
                return false
            }

            axios.post(this.$config.sendLinkUri, {
                interviewId: this.$route.params.interviewId,
                email: email,
            }).then(function (response) {
                self.showEmailPersonalLink = false
                if (response && response.data === 'fail')
                    toastr.error('Email was not sent')
            }).catch(function (error) {
                if (error && error.response)
                    self.$errorHandler(error, self)
            })

            this.emailModal.hide()
            this.emailModal = null;
        },
        validateEmail(email) {
            var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
            return re.test(String(email).toLowerCase())
        },
        changeLanguage(language) {

            this.$store.dispatch('changeLanguage', language)

            modal.dialog({
                message: '<p>' + this.$t('WebInterviewUI.SwitchingLanguage') + '</p>',
                closeButton: false,
            })
        },
        reloadQuestionnaire() {
            window.location = this.$config.reloadQuestionnaireUrl
        },
        showVariables() {
            this.$store.dispatch('setShowVariables', { value: true })
            this.$store.dispatch('fetchSectionEntities')
        },
        hideVariables() {
            this.$store.dispatch('setShowVariables', { value: false })
            this.$store.dispatch('fetchSectionEntities')
        },
        async showSaveScenario() {
            this.scenarioSaving = true
            this.designerCredentialsExpired = false
            $(this.$refs.saveScenarioModalRef).appendTo('body').modal('show')
            try {
                this.designerScenarios = []
                var getScenarios = await axios.get(this.saveScenarioUrl, {
                    crossDomain: true,
                    withCredentials: true,
                })

                this.designerScenarios = getScenarios.data
            }
            catch (error) {
                this.handleDesignerApiResponse(error)
            }
            finally {
                this.scenarioSaving = false
            }
        },
        async saveScenario() {
            this.scenarioSaving = true
            try {
                const scenarioContentResponse = await axios.get(`${this.getScenarioUrl}/${this.$route.params.interviewId}`)
                const scenario = scenarioContentResponse.data

                await axios({
                    method: 'POST',
                    url: this.saveScenarioUrl,
                    data: {
                        scenarioText: JSON.stringify(scenario),
                        scenarioId: this.selectedSaveOption === 'updateExisting' ? this.selectedScenarioOption : '',
                        scenarioTitle: this.newScenarioName,
                    },
                    crossDomain: true,
                    withCredentials: true,
                })

                this.hideScenarioSave()
            } catch (error) {
                this.handleDesignerApiResponse(error)
            }
            finally {
                this.scenarioSaving = false
            }
        },
        handleDesignerApiResponse(error) {
            if (error.isAxiosError)
                if (error.response.status === 401) {
                    this.designerCredentialsExpired = true
                }
                else if (error.response.status === 403) {
                    this.hideScenarioSave()
                    throw this.$t('WebInterviewUI.NoQuestionnaireAccess')
                }
                else {
                    throw error
                }
            else {
                throw error
            }
        },
    },
}
</script>
