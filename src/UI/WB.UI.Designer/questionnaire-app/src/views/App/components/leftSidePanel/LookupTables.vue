<template>
    LookupTables

    <!--div class="lookup-tables">
        <perfect-scrollbar class="scroller">
            <h3>
                <span ng-i18next="[i18next]({count: lookupTables.length})SideBarLookupTablesCounter"></span>
            </h3>
            <div class="empty-list" ng-show="lookupTables.length === 0">
                <p ng-i18next>SideBarLookupEmptyLine1</p>
                <p ng-i18next>SideBarLookupEmptyLine2</p>
                <p ng-i18next>SideBarLookupEmptyLine3</p>
            </div>
            <form role="form" name="lookupTablesForm" novalidate>
                <ul ng-model="lookupTables">
                    <li class="lookup-table-panel-item" ng-repeat="table in lookupTables" ngf-drop=""
                        ngf-change="fileSelected(table, $file)" ngf-max-size="1MB" accept=".tab,.txt"
                        ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                        <ng-form name="table.form">
                            <a href="javascript:void(0);" @click="deleteLookupTable($index)"
                                ng-disabled="questionnaire.isReadOnlyForUser" ng-if="!questionnaire.isReadOnlyForUser"
                                class="btn delete-btn" tabindex="-1"></a>
                            <input focus-on-out="focusLookupTable{{table.itemId}}" required=""
                                ng-i18next="[placeholder]SideBarLookupTableName" maxlength="32" spellcheck="false"
                                autocomplete="off" ng-model="table.name" name="name" class="form-control table-name"
                                type="text" />
                            <div class="divider"></div>
                            <input ng-i18next="[placeholder]SideBarLookupTableFileName" required="" spellcheck="false"
                                ng-model="table.fileName" name="fileName" class="form-control" disabled="" type="text" />

                            <div class="drop-box" ng-i18next>SideBarLookupTableDropFile</div>

                            <div class="actions clearfix" :class="{ dirty: table.form.$dirty }">
                                <div ng-show="table.form.$dirty" class="pull-left">
                                    <button type="submit"
                                        ng-disabled="questionnaire.isReadOnlyForUser || table.form.$invalid"
                                        class="btn lighter-hover" @click="saveLookupTable(table); $event.stopPropagation()"
                                        ng-i18next>Save</button>
                                    <button type="button" class="btn lighter-hover"
                                        @click="cancel(table); $event.stopPropagation()" ng-i18next>Cancel</button>
                                </div>
                                <div class="permanent-actions clearfix">
                                    <a href="{{downloadLookupFileBaseUrl + questionnaire.questionnaireId +'?lookupTableId='+ table.itemId}}"
                                        ng-show="table.hasUploadedFile" class="btn btn-default pull-right" target="_blank"
                                        rel="noopener noreferrer" ng-i18next>Download</a>
                                    <button class="btn btn-default pull-right" ngf-select=""
                                        ngf-change="fileSelected(table, $file);$event.stopPropagation()" accept=".tab,.txt"
                                        ngf-max-size="2MB" type="file">
                                        <span ng-hide="table.hasUploadedFile"
                                            ng-i18next="SideBarLookupTableSelectFile">Select file</span>
                                        <span ng-show="table.hasUploadedFile"
                                            ng-i18next="SideBarLookupTableUpdateFile">Update file</span>
                                    </button>
                                </div>
                            </div>
                        </ng-form>
                    </li>
                </ul>
            </form>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" ng-disabled="questionnaire.isReadOnlyForUser"
                    ng-i18next="[value]SideBarLookupTableAdd" value="ADD NEW Lookup table" @click="addNewLookupTable()">
            </div>
    </perfect-scrollbar>
</div-->
</template>
  
<script>
export default {
    name: 'LookupTables',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  