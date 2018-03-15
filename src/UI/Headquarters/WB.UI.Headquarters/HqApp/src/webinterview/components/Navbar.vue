<template>
    <nav class="navbar navbar-inverse navbar-fixed-top navbar-web-interview" role="navigation">
        <div class="container-fluid ">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <router-link class="interview-ID" active-class="" :to="toFirstSection" v-if="$store.state.webinterview.firstSectionId">
                    {{interviewKey}}
                </router-link>
                <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false">
                    <span class="sr-only">{{ $t('WebInterviewUI.ToggleNavigation') }}</span>
                    <span class="icon-bar top-menu"></span>
                    <span class="icon-bar mid-menu"></span>
                    <span class="icon-bar bottom-menu"></span>
                </button>
                <div class="navbar-brand">
                    <router-link class="logo" :to="toFirstSection" v-if="$store.state.webinterview.firstSectionId && hqLink == null"></router-link>
                    <a :href="hqLink" v-if="hqLink != null" class="logo"></a>
                </div>  
                <button v-if="this.$config.inWebTesterMode" type="button" class="btn btn-default btn-link btn-icon" :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                    <span class="glyphicon glyphicon-sort"></span>
                </button>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <p class="navbar-text">{{questionnaireTitle}}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="help-link"><a href="http://docs.mysurvey.solutions/web-interview" :title="$t('WebInterviewUI.Help')">{{ $t('WebInterviewUI.Help') }}</a></li>
                    <li class="dropdown language">
                        <a href="javascript:void(0);" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false"
                            :title="currentLanguage">{{currentLanguage}}<span class="caret" v-if="canChangeLanguage"></span></a>
                        <ul class="dropdown-menu" v-if="canChangeLanguage">
                            <li v-if="currentLanguage != $store.state.webinterview.originalLanguageName">
                                <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.webinterview.originalLanguageName }}</a>
                            </li>
                            <li :key="language.OriginalLanguageName" v-for="language in $store.state.webinterview.languages"
                                v-if="language != $store.state.webinterview.currentLanguage">
                                <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li v-if="this.$config.inWebTesterMode">
                        <button type="button" class="btn btn-default btn-link btn-icon" @click="reloadQuestionnaire" :title="$t('WebInterviewUI.ReloadQuestionnaire')">
                            <span class="glyphicon glyphicon-sort"></span>
                        </button>
                    </li>                    
                </ul>
            </div>
            <!-- /.navbar-collapse -->
        </div>
        <!-- /.container-fluid -->
    </nav>
</template>
<script lang="js">
    import modal from "./modal"

    export default {
        name: 'navbar',
        beforeMount() {
            this.$store.dispatch("getLanguageInfo")
            this.$store.dispatch("loadInterview")
        },
        mounted(){
            $(window).on('resize', function() {
                if($(window).width() > 880) {
                    if ($(".navbar-collapse.collapse.in").length > 0) {
                      $("main").addClass("display-block");
                    }
                }else{
                        $("main").removeClass("display-block");
                    }
            });
            $(".navbar-toggle").click(function () {
                $(".navbar-collapse").fadeToggle();
                $(".navbar-collapse").animate({height: '100%'}, 0);
                $(".top-menu").toggleClass("top-animate");
                $(".mid-menu").toggleClass("mid-animate");
                $(".bottom-menu").toggleClass("bottom-animate");
                if($(window).width() < 880) {
                    if ($(".navbar-collapse.collapse.in").length > 0) {
                        $("main").removeClass("display-block");
                        $("main").removeClass("hidden");
                    }else{
                        $("main").addClass("hidden");
                    }
                }
            });
        },
        updated(){
            document.title = this.$config.splashScreen ? this.$t("WebInterviewUI.LoadingQuestionnaire") : `${this.$store.state.webinterview.interviewKey} | ${this.questionnaireTitle} | ${this.$t("WebInterviewUI.WebInterview")}`
        },
        computed: {
            canChangeLanguage() {
                return this.$store.state.webinterview.languages != undefined && this.$store.state.webinterview.languages.length > 0
            },
            currentLanguage(){
                return this.$store.state.webinterview.currentLanguage || this.$store.state.webinterview.originalLanguageName
            },
            questionnaireTitle(){
                if(this.$config.splashScreen) return this.$t("WebInterviewUI.LoadingQuestionnaire")

                return this.$store.state.webinterview.questionnaireTitle || ""
            },
            toFirstSection(){
                return { name: 'section', params: { sectionId: this.$store.state.webinterview.firstSectionId } }
            },
            interviewKey() {
                return this.$store.state.webinterview.interviewKey || this.$t("WebInterviewUI.WebInterview");
            },
            hqLink() {
                return this.$config.hqLink
            }
        },
        methods: {
            changeLanguage(language) {

                this.$store.dispatch("changeLanguage", { language: language })

                modal.dialog({
                    message: "<p>" + this.$t("WebInterviewUI.SwitchingLanguage") +"</p>",
                    closeButton: false
                })
            },
            reloadQuestionnaire() {
                window.location = this.$config.reloadQuestionnaireUrl;
            }
        }
    }
</script>
