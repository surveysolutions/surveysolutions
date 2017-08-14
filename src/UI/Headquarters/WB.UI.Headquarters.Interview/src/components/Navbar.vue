<template>
    <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
        <div class="container-fluid ">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <router-link class="interview-ID" active-class="" :to="toFirstSection" v-if="$store.state.firstSectionId">
                    {{interviewKey}}
                </router-link>
                <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false">
                    <span class="sr-only">{{ $t('ToggleNavigation') }}</span>
                    <span class="icon-bar top-menu"></span>
                    <span class="icon-bar mid-menu"></span>
                    <span class="icon-bar bottom-menu"></span>
                </button>
                <div class="navbar-brand">
                    <router-link class="logo" :to="toFirstSection" v-if="$store.state.firstSectionId && hqLink == null"></router-link>
                    <a :href="hqLink" v-if="hqLink != null" class="logo"></a>
                </div>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <p class="navbar-text">{{questionnaireTitle}}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="dropdown language">
                        <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false"
                            :title="currentLanguage">{{currentLanguage}}<span class="caret" v-if="canChangeLanguage"></span></a>
                        <ul class="dropdown-menu" v-if="canChangeLanguage">
                            <li v-if="currentLanguage != $store.state.originalLanguageName">
                                <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.originalLanguageName }}</a>
                            </li>
                            <li :key="language.OriginalLanguageName" v-for="language in $store.state.languages"
                                v-if="language != $store.state.currentLanguage">
                                <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li><a href="http://docs.mysurvey.solutions/web-interview" :title="$t('Help')">{{ $t('Help') }}</a></li>
                </ul>
            </div>
            <!-- /.navbar-collapse -->
        </div>
        <!-- /.container-fluid -->
    </nav>
</template>
<script lang="js">
    import * as $ from "jquery"
    import modal from "modal"
       import { hqLink } from "src/config"

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
            document.title = `${this.$store.state.interviewKey} | ${this.questionnaireTitle} | ${this.$t("WebInterview")}`
        },
        computed: {
            canChangeLanguage() {
                return this.$store.state.languages != undefined && this.$store.state.languages.length > 0
            },
            currentLanguage(){
                return this.$store.state.currentLanguage || this.$store.state.originalLanguageName
            },
            questionnaireTitle(){
                return this.$store.state.questionnaireTitle || ""
            },
            toFirstSection(){
                return { name: 'section', params: { sectionId: this.$store.state.firstSectionId } }
            },
            interviewKey() {
                return this.$store.state.interviewKey || this.$t("WebInterview");
            },
            hqLink() {
                return hqLink
            }
        },
        methods: {
            changeLanguage(language) {

                this.$store.dispatch("changeLanguage", { language: language })

                modal.dialog({
                    message: "<p>" + this.$t("SwitchingLanguage") +"</p>",
                    closeButton: false
                })
            }
        }
    }
</script>
