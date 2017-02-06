<template>
    <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
        <div class="container-fluid ">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <a href="#" class="active-page">Web interview #23-44-32-12</a>
                <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar top-menu"></span>
                        <span class="icon-bar mid-menu"></span>
                        <span class="icon-bar bottom-menu"></span>
                    </button>
                <a class="navbar-brand  rotate-brand" href="#">
                    <div class="brand-name">Web interview #23-44-32-12</div>
                </a>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <p class="navbar-text"  v-bind:style="{ width: questionnaireTitleWidth + '%' }">{{questionnaireTitle}}</p>
                <ul class="nav navbar-nav navbar-right">
                    <li class="dropdown">
                        <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" v-bind:title="currentLanguage">{{currentLanguage}}<span class="caret"></span></a>
                        <ul class="dropdown-menu" v-if="canChangeLanguage">
                            <li v-if="currentLanguage != $store.state.originalLanguageName">
                                <a href="javascript:void(0)" @click="changeLanguage()">{{ $store.state.originalLanguageName }}</a>
                            </li>
                            <li v-for="language in $store.state.languages" v-if="language != $store.state.currentLanguage">
                                <a href="javascript:void(0)" @click="changeLanguage(language)">{{ language }}</a>
                            </li>
                        </ul>
                    </li>
                    <li><a href="#" title="Help">Help</a></li>
                </ul>
            </div>
            <!-- /.navbar-collapse -->
        </div>
        <!-- /.container-fluid -->
    </nav>
</template>
<script lang="ts">
    import * as $ from "jquery"

    export default {
        name: 'navbar',
        data: function() {
            return {
                questionnaireTitleWidth: 40,
            }
        },
        beforeMount() {
            this.$store.dispatch("getLanguageInfo")
            this.$store.dispatch("loadQuestionnaire")
        },
         mounted() {
            this.$nextTick(function() {
                window.addEventListener('resize', this.calcQuestionnaireTitleWidth);
                this.calcQuestionnaireTitleWidth()
            })
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
            }
        },
        methods: {
            calcQuestionnaireTitleWidth() {
                var navbar = $(".navbar .container-fluid").width();
                var brand = $(".navbar .navbar-brand").outerWidth();
                var menu_width = 0;
                $(".navbar .navbar-nav").each(function(e){
                    menu_width+= $(this).outerWidth()
                });
                this.questionnaireTitleWidth = 100*(navbar - brand - menu_width - 15)/navbar;
            },
            changeLanguage(language) {
                this.$store.dispatch("changeLanguage", { language: language })
            }
        }
    }
</script>
