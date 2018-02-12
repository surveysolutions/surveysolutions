<template>
    <HqLayout :title="$t('WebInterviewSetup.WebInterviewSetup_Title')">
        <div class="row">
            <div class="page-header">
                <ol class="breadcrumb">
                    <li>
                        <a :href="this.$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                    </li>
                </ol>
                <h1>
                    {{$t('WebInterviewSetup.WebInterviewSetup_PageHeader')}}
                </h1>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h3>
                    {{$t('WebInterviewSetup.StartInfo', {name: this.$config.model.questionnaireFullName})}}
                </h3>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-12"> 
                <ul class="nav nav-tabs" role="tablist">
                    <li  v-for="opt in editableStrings" :key="opt.value" :class="{active:opt.isActive}">
                        <a href="javascript:void(0);" role="tab" 
                                data-toggle="tab" @click.stop.prevent="setActive(opt)">{{ opt.title }}</a>
                    </li>
                </ul>
                    <div class="tab-content">
                    <div v-for="opt in editableStrings" :key="opt.value" role="tabpanel" class="tab-pane" :class="{active:opt.isActive}">
                        <textarea :id="'customTextOpt' + opt.value" rows="5"></textarea>
                    </div>
                </div>
            </div>
        </div>
        <div class="checkbox info-block">
            <input checked="checked" class="checkbox-filter" data-val="true"  
                    id="useCaptcha" name="UseCaptcha" type="checkbox" value="true">
            <label for="useCaptcha">
                <span class="tick"></span>
                {{$t('WebInterviewSetup.UseCaptcha')}}
            </label>
        </div>
        <div class="form-group">
            <div class="action-buttons">
                <button type="submit" class="btn btn-success">{{$t('WebInterviewSetup.Start')}}</button>
                <a href="#" class="back-link">
                    {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                </a>
            </div>
        </div>
    </HqLayout>
</template>
<script>
import Vue from "vue"
import index from 'vue';
export default {
    data(){
        return {
            editableStrings: []
        }
    },
    mounted() {
        this.editableStrings = _.map(this.$config.model.textOptions, (option, index) => {
            return {
                value: option.value,
                title: option.title,
                isActive: index === 0
            }
        });
    },
    methods: {
        setActive(opt) {
            _.map(this.editableStrings, (option) => {
                option.isActive = false;
            });
    
            opt.isActive = true;
        }
    }
}
</script>

