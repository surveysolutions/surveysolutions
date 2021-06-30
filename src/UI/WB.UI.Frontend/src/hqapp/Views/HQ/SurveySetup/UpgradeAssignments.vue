<template>
    <HqLayout :hasFilter="false"
        :title="$t('Pages.UpgradeAssignmentsTitle')">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>
                {{$t('Pages.UpgradeAssignmentsTitle')}}
            </h1>
        </div>
        <div class="row-fluid">
            <div class="col-sm-12 prefilled-data-info info-block">
                <p>
                    {{$t('Assignments.UpgradeDescription')}}
                </p>
            </div>
        </div>
        <div class="row-fluid">
            <div class="col-sm-12">
                <h3>{{$t('Assignments.SelectQuestionnaireToUpgradeFrom')}}</h3>
                <Typeahead control-id="questionnaire"
                    noSearch
                    :placeholder="$t('Common.Questionnaire')"
                    :values="questionnaires"
                    :value="questionnaireId"
                    @selected="selectQuestionnaire" />
            </div>
        </div>
        <form method="post">
            <input type="hidden"
                :value="this.$hq.Util.getCsrfCookie()"
                name="__RequestVerificationToken"/>
            <input type="hidden"
                :value="questionnaireId ? questionnaireId.key : ''"
                name="sourceQuestionnaireId"/>
            <div class="col-sm-7 col-xs-12 action-buttons">
                <button type="submit"
                    class="btn btn-success"
                    :disabled="!questionnaireId">
                    {{$t('Assignments.UpgradeBtn')}}
                </button>
                <a :href="$config.model.surveySetupUrl"
                    class="back-link">
                    {{$t('WebInterviewSetup.BackToQuestionnaires')}}
                </a>
            </div>
        </form>
    </HqLayout>
</template>

<script>
export default {
    data() {
        return { questionnaireId: null }
    },
    computed: {
        questionnaires() {
            return this.$config.model.questionnaires
        },
    },
    methods: {
        selectQuestionnaire(value) {
            this.questionnaireId = value
        },
    },
}
</script>
