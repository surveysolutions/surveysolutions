<template>
   <HqLayout :title="$config.model.title" :hasFilter="false">
       <div>
            <ol class="breadcrumb">
                <li>
                    @Html.ActionLink(MainMenu.SurveySetup, "Index", "SurveySetup")
                </li>
                <li>
                    @Html.ActionLink(QuestionnaireImport.ListOfMyQuestionnaires, "Import")
                </li>
            </ol>
            <h1>{{this.$t('ImportQuestionnaire.PageHeader')}}</h1>
            <div class="signed-name">
                @Html.Raw(string.Format(@ImportQuestionnaire.SignedInBlock, string.Format("<span>{0}</span>", @Model.DesignerUserName))) 
                <a href="@Url.Action("LogoutFromDesigner")">{{this.$t('ImportQuestionnaire.SignOut')}}</a>
            </div>
        </div>
        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
        ></DataTables>

        <ModalFrame ref="deleteQuestionnaireModal" :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Pages.GlobalSettings_DeleteQuestionnareConfirmation" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    @click="deleteQuestionnaire"
                >{{ $t("Common.Delete") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                >{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>
export default {

}
</script>

<style>

</style>
