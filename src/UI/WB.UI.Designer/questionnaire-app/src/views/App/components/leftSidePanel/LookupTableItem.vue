<template>
    <form name="table.form" :class="{ 'dragover': $refs.upload && $refs.upload.dropActive }">
        <a href="javascript:void(0);" @click.self="deleteLookupTable()" v-if="!isReadOnlyForUser" class="btn delete-btn"
            tabindex="-1"></a>
        <input focus-on-out="focusLookupTable{{table.itemId}}" required=""
            :placeholder="$t('QuestionnaireEditor.SideBarLookupTableName')" maxlength="32" spellcheck="false"
            autocomplete="off" v-model="table.name" name="name" class="form-control table-name" type="text" />
        <div class="divider"></div>
        <input :placeholder="$t('QuestionnaireEditor.SideBarLookupTableFileName')" required="" spellcheck="false"
            v-model="table.fileName" name="fileName" class="form-control" disabled="" type="text" />

        <div class="drop-box">{{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}</div>

        <div class="actions clearfix" :class="{ 'dirty': isDirty }">
            <div v-show="isDirty" class="pull-left">
                <button type="button" v-if="!isReadOnlyForUser" :disabled="isInvalid ? 'disabled' : null"
                    class="btn lighter-hover" @click.self="saveLookupTable()">{{
        $t('QuestionnaireEditor.Save') }}</button>
                <button type="button" class="btn lighter-hover" @click.self="cancel()">{{
        $t('QuestionnaireEditor.Cancel')
    }}</button>
            </div>
            <div class="permanent-actions clearfix">
                <a :href="sanitizeUrl(downloadLookupFileBaseUrl + questionnaireId + '?lookupTableId=' + table.itemId)"
                    v-if="hasUploadedFile" class="btn btn-default pull-right" target="_blank"
                    rel="noopener noreferrer">{{
        $t('QuestionnaireEditor.Download') }}</a>
                <button class="btn btn-default pull-right" type="button" @click.stop="openFileDialog()"
                    :disabled="isReadOnlyForUser">
                    <span v-show="!hasUploadedFile">{{
        $t('QuestionnaireEditor.SideBarLookupTableSelectFile') }}</span>
                    <span v-show="hasUploadedFile">{{
        $t('QuestionnaireEditor.SideBarLookupTableUpdateFile') }}</span>
                </button>
                <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'ltfu' + table.itemId" v-model="file"
                    @input-file="fileSelected" :size="2 * 1024 * 1024" :drop="true" :drop-directory="false"
                    accept=".tab,.txt">
                </file-upload>
            </div>
        </div>
    </form>
</template>

<script>
import { notice } from '../../../../services/notificationService';
import { updateLookupTable, deleteLookupTable } from '../../../../services/lookupTableService'
import { isEmpty, isUndefined, isNull, cloneDeep } from 'lodash'
import { createQuestionForDeleteConfirmationPopup, trimText } from '../../../../services/utilityService'

import { sanitizeUrl } from '@braintree/sanitize-url';

export default {
    name: 'LookupTableItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        tableItem: { type: Object, required: true },
    },
    data() {
        return {
            downloadLookupFileBaseUrl: '/Questionnaire/ExportLookupTable/',
            file: [],
        }
    },
    computed: {
        table() {
            return this.tableItem.editLookupTable;
        },
        isDirty() {
            return this.table.name != this.tableItem.name || (this.table.file !== null && this.table.file !== undefined);
        },
        isInvalid() {
            return (this.table.name) ? false : true;
        },
        hasUploadedFile() {
            return !isEmpty(this.table.fileName)
        }
    },
    methods: {
        fileSelected(file) {
            if (isUndefined(file) || isNull(file)) {
                return;
            }

            let table = this.table

            table.file = file.file;
            table.fileName = file.name;

            table.content = {};
            table.content.size = file.size;
            table.content.type = file.type;

            table.meta = {};
            table.meta.fileName = file.name;
        },

        async saveLookupTable() {
            var response = await updateLookupTable(this.questionnaireId, this.table)

            if (this.file.length > 0 && response != null)
                notice(response);

            this.table.file = null;
            this.file = [];
        },
        deleteLookupTable() {
            var tableName = this.table.name || this.$t('QuestionnaireEditor.SideBarTranslationNoName');

            var trimmedTableName = trimText(tableName);
            var confirmParams = createQuestionForDeleteConfirmationPopup(trimmedTableName)

            confirmParams.callback = confirm => {
                if (confirm) {
                    deleteLookupTable(this.questionnaireId, this.table.itemId)
                }
            };

            this.$confirm(confirmParams);
        },
        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
        cancel() {
            var clonned = cloneDeep(this.tableItem);
            clonned.editLookupTable = null;
            this.tableItem.editLookupTable = clonned;
        },
        sanitizeUrl(url) {
            return sanitizeUrl(url);
        }
    }
}
</script>