<template>
    <div>
        <nav id="topNav" class="navbar navbar-default navbar-fixed-top" role="navigation">
            <div class="container">
                <div id="uploadForm" class="navbar-form navbar-left">
                    <input name="file"
                           ref="uploader"
                           v-show="false"
                           accept=".tab, .txt, .tsv"
                           type="file"
                           @change="upload"
                           class="btn btn-default btn-lg btn-action-questionnaire" />
                    <button type="button"
                            class="btn btn-default"
                            @click="$refs.uploader.click()">
                        {{$t('Upload')}}
                    </button>
                </div>
                <div class="pull-right">
                    <a class="btn btn-default navbar-btn" :href="$config.exportOptionsUrl">{{$t('Export')}}</a>
                </div>
            </div>
        </nav>
        <div id="content" class="container">
            <div v-if="hasErrors" class="alert alert-danger">
                <p v-for="error in errors" :key="error">{{error}}</p>
            </div>
            <table class="table table-bordered table-hover table-condensed">
                <thead>
                <tr class="active">
                    <td nowrap>{{$t('OptionsUploadValue')}}</td>
                    <td>{{$t('OptionsUploadTitle')}}</td>
                    <td nowrap v-if="$config.isCascading">{{$t('OptionsUploadParent')}}</td>
                </tr>
                </thead>
                <tbody>
                <tr v-if="hasOptions" v-for="category in categories" :key="category.value">
                    <td>{{category.value}}</td>
                    <td>{{category.title}}</td>
                    <td v-if="$config.isCascading">{{category.parentValue}}</td>
                </tr>
                <tr v-else>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td v-if="$config.isCascading">&nbsp;</td>
                </tr>
                </tbody>
            </table>
            <p v-if="!hasOptions">{{$t('OptionsUploadLimit', {limit: 15000})}}</p>
        </div>
        <nav id="bottomNav" class="navbar navbar-default navbar-fixed-bottom" role="navigation">
            <div class="container">
                <a class="btn btn-success navbar-btn" @click="apply">{{$t('OptionsUploadApply')}}</a>
                <a class="btn btn-link navbar-btn" @click="cancel">{{$t('Cancel')}}</a>
                <a class="btn btn-link navbar-btn pull-right" @click="closeWindow">{{$t('Close')}}</a>
            </div>
        </nav>
    </div>
</template>

<script>
    import Vue from 'vue';

    export default {
        data() {
            return {
                errors: [],
                categories: []
            }
        },
        mounted() {
            const self = this
            Vue.nextTick(function(){
                self.ajustNavPanels();
                self.update();  
            })
        },
        computed: {
            hasOptions: function(){
                return this.categories.length > 0;
            },
            hasErrors: function(){
                return this.errors.length > 0;
            }
        },
        methods:{
            ajustNavPanels: function(){
                $("body").css("paddingTop", $("#topNav").outerHeight() + 3);
                $("body").css("paddingBottom", $("#bottomNav").outerHeight() + 3);
            },
            closeWindow: function(){
                if (confirm(this.$t('OptionsCloseWindow'))) {
                    close();
                }
            },
            upload: function(e){

                const files = e.target.files || e.dataTransfer.files
                if (!files.length) return

                const file = files[0];

                const formData = new FormData();
                formData.append('csvFile', file)

                this.clearErrors();

                const self = this;
                $.ajax({
                    url: this.$config.editOptionsUrl,
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false
                }).done(function(response){
                    self.errors = response;
                    self.update();
                });
            },
            update: function(){
                const self = this;

                $.get(this.$config.getOptionsUrl, function(response) {
                    self.categories = response;
                });
            },
            cancel: function(){
                this.clearErrors();

                const self = this
                $.post(this.$config.resetOptionsUrl, function() {
                    self.update();
                });

                this.update()
            },
            apply: function(){
                const self = this
                $.post(this.$config.applyUrl, function(response) {
                    if (response.isSuccess || response.IsSuccess) {
                        close();
                    } else {
                        //$(document).scrollTop(0);
                        self.errors = [response.Error];
                    }
                });
            },
            clearErrors: function(){
                this.errors = [];
            }
        }
    };
</script>
