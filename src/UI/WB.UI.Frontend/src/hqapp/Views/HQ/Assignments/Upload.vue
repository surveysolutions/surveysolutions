<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="../../SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('BatchUpload.CreatingMultipleAssignments')}}</h1>
        </div>
        <div class="row">
            <div class="col-sm-8">
                <h2>{{$t('Pages.QuestionnaireNameFormat', { name : questionnaire.title, version : questionnaire.version})}}</h2>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block">
                <p>
                    {{$t('BatchUpload.UploadDescription')}}
                    <br />
                    <i>{{$t('BatchUpload.FileTypeDescription')}}</i>
                </p>
            </div>
        </div>
        <div
            class="row"
            v-if="questionnaire.identifyingQuestions.length > 0 || questionnaire.hiddenQuestions > 0 || questionnaire.rosterSizeQuestions > 0"
        >
            <div
                v-if="!showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block short-prefilled-data-info"
            >
                <a
                    class="list-required-prefilled-data"
                    href="javascript:void(0);"
                    @click="showQuestions = true"
                >{{$t('BatchUpload.ViewListPreloadedData')}}</a>
            </div>
            <div
                v-if="showQuestions"
                class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info"
            >
                <h3
                    v-if="questionnaire.identifyingQuestions.length > 0"
                >{{$t('BatchUpload.IdentifyingQuestions')}}</h3>

                <ul
                    v-if="questionnaire.identifyingQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li v-for="item in questionnaire.identifyingQuestions">{{ item.caption}}</li>
                </ul>
                <h3
                    v-if="questionnaire.hiddenQuestions.length > 0"
                >{{$t('BatchUpload.HiddenQuestions')}}</h3>
                <ul
                    v-if="questionnaire.hiddenQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li v-for="item in questionnaire.hiddenQuestions">{{item}}</li>
                </ul>

                <h3
                    v-if="questionnaire.rosterSizeQuestions.length > 0"
                >{{$t('BatchUpload.RosterSizeQuestions')}}</h3>
                <ul
                    v-if="questionnaire.rosterSizeQuestions.length > 0"
                    class="list-unstyled prefilled-data"
                >
                    <li v-for="item in questionnaire.rosterSizeQuestions">{{item}}</li>
                </ul>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6 col-xs-10 prefilled-data-info info-block full-prefilled-data-info">
                <h3>{{$t('BatchUpload.Select_Responsible')}}</h3>
                <p>{{$t('BatchUpload.Select_Responsible_Description')}}</p>
                <form-group>
                    <div class="field" :class="{answered: responsible != null}">
                        <Typeahead
                            control-id="responsible"
                            :value="responsible"
                            :ajax-params="{ }"
                            :fetch-url="api.responsiblesUrl"
                            @selected="responsibleSelected"
                        ></Typeahead>
                    </div>
                </form-group>
            </div>
        </div>
        <div class="row flex-row">
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block">
                        <h3>{{$t('BatchUpload.SimpleTitle')}}</h3>
                        <p v-html="$t('BatchUpload.SimpleDescription')"></p>
                    </div>
                    <div>
                        <a v-bind:href="api.simpleTemplateDownloadUrl">{{$t('BatchUpload.DownloadTabTemplate')}}</a>
                        <!-- @using (Html.BeginForm("AssignmentsBatchUploadAndVerify", "SurveySetup", new { questionnaireId = Model.QuestionnaireId, version = Model.QuestionnaireVersion }, FormMethod.Post, new { enctype = "multipart/form-data" }))
                        {
                            @Html.AntiForgeryToken()
                            @Html.HiddenFor(x => x.QuestionnaireId)
                            @Html.HiddenFor(x => x.QuestionnaireVersion)
                            @Html.HiddenFor(x => x.ResponsibleId, new { data_bind= "value: selectedResponsibleId" })
                            
                            @Html.Partial("ClientTimezoneOffset")
                        <label class="btn btn-success btn-file">
                            @BatchUpload.UploadTabFile
                            @Html.TextBoxFor(m => m.File, new { type = "file", accept = ".tab, .txt, .zip", @class = "file" })
                        </label>
                            @Html.ValidationMessageFor(x => x.File, null, new { @class = "help-block" })
                        } -->
                    </div>
                </div>
            </div>
            <div class="col-sm-6">
                <div class="flex-block selection-box">
                    <div class="block">
                        <h3>{{$t('BatchUpload.BatchTitle')}}</h3>
                        <p v-html="$t('BatchUpload.BatchDescription')"></p>
                    </div>
                    <div>
                        <a v-bind:href="api.templateDownloadUrl">{{$t('BatchUpload.DownloadTemplateArchive')}}</a>
                        <!-- @using (Html.BeginForm("PanelBatchUploadAndVerify", "SurveySetup", routeValues: new { id = Model.QuestionnaireId, version = Model.QuestionnaireVersion }, method: FormMethod.Post, htmlAttributes: new { enctype = "multipart/form-data" }))
                        {
                            @Html.AntiForgeryToken()
                            @Html.HiddenFor(x => x.QuestionnaireId)
                            @Html.HiddenFor(x => x.QuestionnaireVersion)
                            @Html.HiddenFor(x => x.ResponsibleId, new { data_bind= "value: selectedResponsibleId" })

                            @Html.Partial("ClientTimezoneOffset")
                        <label class="btn btn-success btn-file">
                            @BatchUpload.UploadZipFile
                            @Html.TextBoxFor(m => m.File, new { type = "file", accept = ".zip", @class = "file" })
                        </label>
                            @Html.ValidationMessageFor(x => x.File, null, new { @class = "help-block" })
                        } -->
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-4 col-sm-5 text-page">
                <p v-html="manualModeDescription"></p>
            </div>
        </div>
    </HqLayout>
</template>
<script>
export default {
    data() {
        return {
            showQuestions: false,
            responsible: null,
        }
    },
    computed: {
        model() {
            return this.$config.model
        },
        questionnaire() {
            return this.model.questionnaire
        },
        api() {
            return this.model.api
        },
        responsibleId() {
            return this.newResponsibleId != null ? this.newResponsibleId.key : null
        },
        manualModeDescription() {
            return this.$t('BatchUpload.ManualModeDescription', {
                url:
                    "<a href='" +
                    this.api.createAssignmentUrl +
                    "'>" +
                    this.$t('BatchUpload.ManualModeLinkTitle') +
                    '</a>',
            })
        },
    },
    methods: {
        responsibleSelected(newValue) {
            this.responsible = newValue
        },
    },
}
</script>
