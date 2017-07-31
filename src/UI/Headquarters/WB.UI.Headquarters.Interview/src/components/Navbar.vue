<template>
    <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
        <div class="container-fluid ">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <router-link class="active-page" active-class="active-page" :to="toFirstSection" v-if="$store.state.firstSectionId">Web interview</router-link>
                <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar top-menu"></span>
                    <span class="icon-bar mid-menu"></span>
                    <span class="icon-bar bottom-menu"></span>
                </button>
                <router-link class="navbar-brand rotate-brand" :to="toFirstSection" active-class="rotate-brand" v-if="$store.state.firstSectionId && hqLink == null">
                    <div class="brand-name">{{interviewKey}}</div>
                </router-link>
                <a :href="hqLink" v-if="hqLink != null" class="navbar-brand rotate-brand">
                    <div class="brand-name">{{interviewKey}}</div>
                </a>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <p class="navbar-text">{{questionnaireTitle}}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="dropdown language">
                        <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" v-bind:title="currentLanguage">{{currentLanguage}}<span class="caret" v-if="canChangeLanguage"></span></a>
                        <ul class="dropdown-menu" v-if="canChangeLanguage">
                            <li v-if="currentLanguage != $store.state.originalLanguageName">
                                <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.originalLanguageName }}</a>
                            </li>
                            <li :key="language.OriginalLanguageName" v-for="language in $store.state.languages" v-if="language != $store.state.currentLanguage">
                                <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li><a href="http://docs.mysurvey.solutions/web-interview" title="Help">Help</a></li>
                </ul>
            </div>
            <!-- /.navbar-collapse -->
        </div>
        <!-- /.container-fluid -->
    </nav>
</template>
<script lang="ts">
    import * as $ from "jquery"
    import modal from "modal"
       import { hqLink } from "src/config"

    export default {
        name: 'navbar',
        beforeMount() {
            this.$store.dispatch("getLanguageInfo")
            this.$store.dispatch("loadInterview")
        },
        updated(){
            document.title = `${this.questionnaireTitle} | Web interview`
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
                return this.$store.state.interviewKey || "Web interview";
            },
            hqLink() {
                return hqLink
            }
        },
        methods: {
            changeLanguage(language) {

                this.$store.dispatch("changeLanguage", { language: language })

                modal.dialog({
                    message: "<p>Switching language. Please wait...</p>",

                    closeButton: false
                })
            }
        },
        directives:{
            init: {
                inserted(el, binding, vnode) {
                    $(".navbar-toggle").click(function () {
                        $(".navbar-collapse").fadeToggle();
                        $(".navbar-collapse").animate({height: '100%'}, 0);
                        $(".top-menu").toggleClass("top-animate");
                        $(".mid-menu").toggleClass("mid-animate");
                        $(".bottom-menu").toggleClass("bottom-animate");
                        $("main").toggleClass("hidden");
                    });
                }
            }
        }
    }
</script>
