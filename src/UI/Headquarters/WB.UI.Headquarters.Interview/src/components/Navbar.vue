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
                </a>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="navbar">
                <ul class="nav navbar-nav">
                    <li class="active"><a href="#" title="Web interview #23-44-32-12">Web interview #23-44-32-12</a></li>
                    <li><a href="#" title="Restart">Restart</a></li>
                    <li>
                        <router-link :to="{name: 'prefilled'}">List of pre-filled questions</router-link>
                    </li>
                </ul>
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
    export default {
        name: 'navbar',
        beforeMount() {
            this.$store.dispatch("getLanguageInfo")
        },
        computed: {
            canChangeLanguage() {
                return this.$store.state.languages != undefined && this.$store.state.languages.length > 0
            },
            currentLanguage(){
                return this.$store.state.currentLanguage || this.$store.state.originalLanguageName
            }
        },
        methods: {
            changeLanguage(language) {
                this.$store.dispatch("changeLanguage", { language: language })
            }
        }
    }
</script>