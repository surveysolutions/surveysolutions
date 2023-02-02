<template>
    <nav class="navbar navbar-inverse navbar-fixed-top navbar-web-interview"
        role="navigation">
        <div class="container-fluid">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <router-link
                    class="interview-ID"
                    active-class
                    :to="toFirstSection"
                    v-if="$store.state.webinterview.firstSectionId">{{interviewKey}}</router-link>
                <button
                    type="button"
                    class="navbar-toggle collapsed"
                    data-toggle="collapse"
                    data-target="#navbar"
                    aria-expanded="false">
                    <span class="sr-only">{{ $t('WebInterviewUI.ToggleNavigation') }}</span>
                    <span class="icon-bar top-menu"></span>
                    <span class="icon-bar mid-menu"></span>
                    <span class="icon-bar bottom-menu"></span>
                </button>
                <div class="navbar-brand">
                    <router-link class="logo"
                        :to="toCoverPage"
                        v-if="hqLink == null"></router-link>
                    <a :href="hqLink"
                        v-if="hqLink != null"
                        class="logo"></a>
                </div>
                <button
                    v-if="this.$config.inWebTesterMode"
                    type="button"
                    class="btn btn-default btn-link btn-icon"
                    @click="reloadQuestionnaire"
                    :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                    <span class="glyphicon glyphicon-sort"></span>
                </button>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse"
                id="navbar">
                <ul class="nav navbar-nav navbar-left">
                    <li>
                        <router-link
                            class="interview-ID"
                            active-class
                            :to="toFirstSection"
                            v-if="$store.state.webinterview.firstSectionId">{{interviewKey}}</router-link>
                    </li>
                </ul>
                <p class="navbar-text pull-left">{{questionnaireTitle}}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="help-link">
                        <a
                            href="https://support.mysurvey.solutions/getting-started/web-interview"
                            :title="$t('WebInterviewUI.Help')">{{ $t('WebInterviewUI.Help') }}</a>
                    </li>
                    <li class="dropdown language">
                        <a
                            href="javascript:void(0);"
                            class="dropdown-toggle"
                            data-toggle="dropdown"
                            role="button"
                            aria-haspopup="true"
                            aria-expanded="false"
                            :title="currentLanguage">
                            {{currentLanguage}}
                            <span class="caret"
                                v-if="canChangeLanguage"></span>
                        </a>
                        <ul class="dropdown-menu"
                            v-if="canChangeLanguage">
                            <li
                                v-if="currentLanguage != $store.state.webinterview.originalLanguageName">
                                <a
                                    href="javascript:void(0)"
                                    @click="changeLanguage()">{{ $store.state.webinterview.originalLanguageName }}</a>
                            </li>
                            <li :key="language.OriginalLanguageName"
                                v-for="language in languages">
                                <a
                                    href="javascript:void(0)"
                                    @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li v-if="showEmailPersonalLink">
                        <a
                            href="#"
                            @click="emailPersonalLink"
                            :title="$t('WebInterviewUI.EmailLink_EmailResumeLink')">{{$t('WebInterviewUI.EmailLink_EmailResumeLink')}}</a>
                    </li>
                    <li v-if="this.$config.inWebTesterMode">
                        <button
                            type="button"
                            class="btn btn-default btn-link btn-icon"
                            @click="reloadQuestionnaire"
                            :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                            <span class="glyphicon glyphicon-sort"></span>
                        </button>
                    </li>
                    <li v-if="this.$config.inWebTesterMode && this.$config.saveScenarioUrl">
                        <button
                            type="button"
                            class="btn btn-default btn-link btn-icon"
                            @click="showSaveScenario"
                            :title="$t('WebInterviewUI.SaveScenario')">
                            <span class="glyphicon glyphicon-floppy-disk"></span>
                        </button>
                        <div
                            class="modal fade"
                            id="saveScenarioModal"
                            ref="saveScenarioModalRef"
                            tabindex="-1"
                            role="dialog">
                            <div class="modal-dialog"
                                role="document">
                                <div class="modal-content">
                                    <div class="modal-header">
                                        <h2>{{this.$t('WebInterviewUI.SaveScenario')}}</h2>
                                    </div>
                                    <div class="modal-body">
                                        <form
                                            onsubmit="return false;"
                                            action="javascript:void(0)"
                                            class="panel-group"
                                            v-if="!this.designerCredentialsExpired">
                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <div class="radio mb-1">
                                                        <input
                                                            name="rbScenarioSaveOption"
                                                            type="radio"
                                                            id="rbScenarioSaveNew"
                                                            value="saveNew"
                                                            class="wb-radio"
                                                            v-model="selectedSaveOption"/>
                                                        <label for="rbScenarioSaveNew">
                                                            <span class="tick"></span>
                                                            {{this.$t('WebInterviewUI.SaveScenarioOptions_SaveNew')}}
                                                        </label>
                                                    </div>
                                                </div>
                                                <div
                                                    class="panel-body"
                                                    v-if="selectedSaveOption === 'saveNew'">
                                                    <div
                                                        class="form-group"
                                                        v-if="selectedSaveOption === 'saveNew'">
                                                        <label
                                                            for="txtScenarioName"
                                                            class="control-label">{{this.$t('WebInterviewUI.SaveScenarioName')}}</label>
                                                        <input
                                                            maxlength="32"
                                                            id="txtScenarioName"
                                                            class="form-control"
                                                            v-model="newScenarioName"/>
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <div class="radio mb-1">
                                                        <input
                                                            name="rbScenarioSaveOption"
                                                            type="radio"
                                                            id="rbScenarioUpdateExisting"
                                                            value="updateExisting"
                                                            class="wb-radio"
                                                            v-model="selectedSaveOption"/>
                                                        <label for="rbScenarioUpdateExisting">
                                                            <span class="tick"></span>
                                                            {{this.$t('WebInterviewUI.SaveScenarioOptions_ReplaceExisting')}}
                                                        </label>
                                                    </div>
                                                </div>
                                                <div
                                                    class="panel-body"
                                                    v-if="selectedSaveOption === 'updateExisting'">
                                                    <div class="form-group">
                                                        <label
                                                            for="slScenarioSaveOption"
                                                            class="control-label">{{this.$t('WebInterviewUI.SaveScenarioOptions')}}</label>
                                                        <select
                                                            id="slScenarioSaveOption"
                                                            class="form-control"
                                                            v-model="selectedScenarioOption">
                                                            <option
                                                                v-for="s in designerScenarios"
                                                                :value="s.id"
                                                                :key="s.id">{{s.title}}</option>
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                        </form>
                                        <div v-else>
                                            <p>{{this.$t('WebInterviewUI.SaveScenarioDesignerLogin')}}</p>
                                            <a
                                                :href="this.$config.designerUrl"
                                                target="_blank">{{this.$t('WebInterviewUI.SaveScenarioGoToDesigner')}}</a>
                                        </div>
                                    </div>
                                    <div
                                        class="modal-footer"
                                        v-if="!this.designerCredentialsExpired">
                                        <button
                                            type="button"
                                            class="btn btn-primary"
                                            :disabled="scenarioSaving || !scenarioValid"
                                            @click="saveScenario">{{$t("Common.Save")}}</button>
                                        <button
                                            type="button"
                                            class="btn btn-link"
                                            @click="hideScenarioSave">{{$t("Common.Cancel")}}</button>
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
    </nav>
</template>
<script lang="js">
import modal from '@/shared/modal'
import axios from 'axios'
import * as toastr from 'toastr'
import Vue from 'vue'
import { filter } from 'lodash'

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
        }
    },
    mounted(){
        $(window).on('resize', function() {
            if($(window).width() > 880) {
                if ($('.navbar-collapse.collapse.in').length > 0) {
                    $('main').addClass('display-block')
                }
            }else{
                $('main').removeClass('display-block')
            }
        })
        $('.navbar-toggle').click(function () {
            $('.navbar-collapse').fadeToggle()
            $('.navbar-collapse').animate({height: '100%'}, 0)
            $('.top-menu').toggleClass('top-animate')
            $('.mid-menu').toggleClass('mid-animate')
            $('.bottom-menu').toggleClass('bottom-animate')
            if($(window).width() < 880) {
                if ($('.navbar-collapse.collapse.in').length > 0) {
                    $('main').removeClass('display-block')
                    $('main').removeClass('hidden')
                }else{
                    $('main').addClass('hidden')
                }
            }
        })

        if (this.$config.askForEmail)
        {
            this.emailPersonalLink()
        }
    },
    updated(){
        document.title = this.$config.splashScreen ? this.$t('WebInterviewUI.LoadingQuestionnaire') : `${this.$store.state.webinterview.interviewKey} | ${this.questionnaireTitle} | ${this.$t('WebInterviewUI.WebInterview')}`
    },
    computed: {
        scenarioValid() {
            if(this.selectedSaveOption === 'saveNew') {
                return this.newScenarioName !== ''
            }
            else {
                return this.selectedScenarioOption !== -1
            }
        },
        canChangeLanguage() {
            return this.$store.state.webinterview.languages != undefined && this.$store.state.webinterview.languages.length > 0
        },
        currentLanguage(){
            return this.$store.state.webinterview.currentLanguage || this.$store.state.webinterview.originalLanguageName
        },
        languages() {
            const current = this.currentLanguage
            return filter(this.$store.state.webinterview.languages, l => l != current)
        },
        questionnaireTitle(){
            if(this.$config.splashScreen) return this.$t('WebInterviewUI.LoadingQuestionnaire')

            return this.$store.state.webinterview.questionnaireTitle || ''
        },
        toFirstSection(){
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
    },
    methods: {
        hideScenarioSave() {
            $(this.$refs.saveScenarioModalRef).modal('hide')
            this.newScenarioName = ''
            this.selectedSaveOption = 'saveNew'
            this.selectedScenarioOption = -1
        },
        emailPersonalLink(){
            var self = this
            let prompt = modal.prompt({
                title: this.$t('WebInterviewUI.EmailLink_Header'),

                inputType: 'email',
                callback: function(result) {
                    if(result === null || result === undefined) return

                    if (!self.validateEmail(result))
                    {
                        var input = $(this).find('input')
                        self.$nextTick(function() {
                            input.next('span').remove()
                            input.after('<span class=\'help-text text-danger\'>' + this.$t('WebInterviewUI.EmailLink_InvalidEmail', { email: result}) + '</span>')
                        })
                        return false
                    }

                    self.sendEmailWithPersonalLink(result)
                },
            })

            prompt.find('input').attr('placeholder', this.$t('WebInterviewUI.EmailLink_Placeholder'))
            prompt.find('input').before('<p>' + this.$t('WebInterviewUI.EmailLink_Message') + '</p>')
            prompt.find('input').before('<p>' + this.$t('WebInterviewUI.EmailLink_ResumeAnyTime') + '</p>')
            //debugger;
        },
        sendEmailWithPersonalLink(email){
            var self = this
            axios.post(this.$config.sendLinkUri, {
                interviewId:  this.$route.params.interviewId,
                email: email,
            }).then(function(response){
                self.showEmailPersonalLink = false
                if(response && response.data === 'fail')
                    toastr.error('Email was not sent')
            }).catch(function (error) {
                if(error && error.response)
                    Vue.config.errorHandler(error, self)
            })
        },
        validateEmail(email) {
            var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
            return re.test(String(email).toLowerCase())
        },
        changeLanguage(language) {

            this.$store.dispatch('changeLanguage', { language: language })

            modal.dialog({
                message: '<p>' + this.$t('WebInterviewUI.SwitchingLanguage') +'</p>',
                closeButton: false,
            })
        },
        reloadQuestionnaire() {
            window.location = this.$config.reloadQuestionnaireUrl
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
            catch(error) {
                this.handleDesignerApiResponse(error)
            }
            finally{
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
            if(error.isAxiosError)
                if(error.response.status === 401) {
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
