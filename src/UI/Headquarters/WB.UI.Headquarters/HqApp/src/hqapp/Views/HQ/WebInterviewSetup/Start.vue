<template>
    <HqLayout :title="$t('WebInterviewSetup.WebInterviewSetup_Title')">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>
                {{$t('WebInterviewSetup.WebInterviewSetup_PageHeader')}}
            </h1>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h3>
                    {{$t('WebInterviewSetup.StartInfo', {name: this.$config.model.questionnaireFullName})}}
                </h3>
            </div>
        </div>
        <form method="post">
            
            <div class="checkbox info-block">
                <input checked="checked" class="checkbox-filter" data-val="true" id="useCaptcha" name="UseCaptcha" type="checkbox" value="true">
                <label for="useCaptcha">
                    <span class="tick"></span>
                    {{$t('WebInterviewSetup.UseCaptcha')}}
                </label>
            </div>
            <div class="form-group">
                <div class="action-buttons">
                    <button type="submit" class="btn btn-success">{{$t('WebInterviewSetup.Start')}}</button>
                    <a :href="this.$config.model.surveySetupUrl" class="back-link">
                        {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                    </a>
                </div>
            </div>
        </form>
    </HqLayout>
</template>
<script>

export default {
  
};
</script>

