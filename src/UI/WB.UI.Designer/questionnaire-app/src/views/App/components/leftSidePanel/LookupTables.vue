<template>
    <div class="lookup-tables">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarLookupTablesCounter', { count: lookupTables.length }) }}</span>
            </h3>
            <div class="empty-list" v-show="lookupTables.length === 0">
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine2') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarLookupEmptyLine3') }}</p>
            </div>
            <form role="form" name="lookupTablesForm" novalidate>
                <ul>
                    <LookupTableItem v-for="table in lookupTables" :table="table" :questionnaire-id="questionnaireId">
                    </LookupTableItem>
                    <!--li class="lookup-table-panel-item" v-for=" table  in  lookupTables " ngf-drop=""
                        ngf-change="fileSelected(table, $file)" ngf-max-size="1MB" accept=".tab,.txt"
                        ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                        <form name="table.form">
                            <a href="javascript:void(0);" @click="deleteLookupTable($index)"
                                :disabled="questionnaire.isReadOnlyForUser" v-if="!questionnaire.isReadOnlyForUser"
                                class="btn delete-btn" tabindex="-1"></a>
                            <input focus-on-out="focusLookupTable{{table.itemId}}" required=""
                                :placeholder="$t('QuestionnaireEditor.SideBarLookupTableName')" maxlength="32"
                                spellcheck="false" autocomplete="off" v-model="table.name" name="name"
                                class="form-control table-name" type="text" />
                            <div class="divider"></div>
                            <input :placeholder="$t('QuestionnaireEditor.SideBarLookupTableFileName')" required=""
                                spellcheck="false" v-model="table.fileName" name="fileName" class="form-control" disabled=""
                                type="text" />

                            <div class="drop-box">{{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}</div>

                            <div class="actions clearfix" :class="{ dirty: table.form.$dirty }">
                                <div v-show="table.form.$dirty" class="pull-left">
                                    <button type="submit" :disabled="questionnaire.isReadOnlyForUser || table.form.$invalid"
                                        class="btn lighter-hover"
                                        @click="saveLookupTable(table); $event.stopPropagation()">{{
                                            $t('QuestionnaireEditor.Save') }}</button>
                                    <button type="button" class="btn lighter-hover"
                                        @click="cancel(table); $event.stopPropagation()">{{ $t('QuestionnaireEditor.Cancel')
                                        }}</button>
                                </div>
                                <div class="permanent-actions clearfix">
                                    <a href="{{downloadLookupFileBaseUrl + questionnaire.questionnaireId +'?lookupTableId='+ table.itemId}}"
                                        v-show="table.hasUploadedFile" class="btn btn-default pull-right" target="_blank"
                                        rel="noopener noreferrer">{{ $t('QuestionnaireEditor.Download') }}</a>
                                    <button class="btn btn-default pull-right" ngf-select=""
                                        ngf-change="fileSelected(table, $file);$event.stopPropagation()" accept=".tab,.txt"
                                        ngf-max-size="2MB" type="file">
                                        <span v-show="!table.hasUploadedFile">{{
                                            $t('QuestionnaireEditor.SideBarLookupTableSelectFile') }}</span>
                                        <span v-show="table.hasUploadedFile">{{
                                            $t('QuestionnaireEditor.SideBarLookupTableUpdateFile') }}</span>
                                    </button>
                                </div>
                            </div>
                        </form>
                    </li-->
                </ul>
            </form>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" :disabled="isReadOnlyForUser"
                    :value="$t('QuestionnaireEditor.SideBarLookupTableAdd')" value="ADD NEW Lookup table"
                    @click="addNewLookupTable()">
            </div>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import LookupTableItem from './LookupTableItem.vue';
import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import { addLookupTable } from '../../../../services/lookupTableService'
import { newGuid } from '../../../../helpers/guid';

export default {
    name: 'LookupTables',
    inject: ['isReadOnlyForUser'],
    components: { LookupTableItem },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {

        }
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    computed: {
        lookupTables() {
            return this.questionnaireStore.getEdittingLookupTables;
        },
    },
    methods: {
        addNewLookupTable() {
            const newLookupTable = {
                itemId: newGuid()
            };

            addLookupTable(this.questionnaireId, newLookupTable);
        },
    }
}
</script>
  