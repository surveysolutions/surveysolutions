<template>
    <li class="lookup-table-panel-item" :class="{ 'dragover': $refs.upload && $refs.upload.dropActive }">
        <form name="table.form">
            <a href="javascript:void(0);" @click.self="deleteLookupTable()" v-if="!isReadOnlyForUser" class="btn delete-btn"
                tabindex="-1"></a>
            <input focus-on-out="focusLookupTable{{table.itemId}}" required=""
                :placeholder="$t('QuestionnaireEditor.SideBarLookupTableName')" maxlength="32" spellcheck="false"
                autocomplete="off" v-model="table.name" name="name" class="form-control table-name" type="text" />
            <div class="divider"></div>
            <input :placeholder="$t('QuestionnaireEditor.SideBarLookupTableFileName')" required="" spellcheck="false"
                v-model="table.fileName" name="fileName" class="form-control" disabled="" type="text" />

            <div class="drop-box">{{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}</div>

            <div class="actions clearfix" :class="{ dirty: dirty }">
                <div v-show="dirty" class="pull-left">
                    <button type="button" :disabled="isReadOnlyForUser || isInvalid" class="btn lighter-hover"
                        @click.self="saveLookupTable()">{{
                            $t('QuestionnaireEditor.Save') }}</button>
                    <button type="button" class="btn lighter-hover" @click.self="cancel()">{{
                        $t('QuestionnaireEditor.Cancel')
                    }}</button>
                </div>
                <div class="permanent-actions clearfix">
                    <a :href="downloadUrl" v-show="hasUploadedFile" class="btn btn-default pull-right" target="_blank"
                        rel="noopener noreferrer">{{ $t('QuestionnaireEditor.Download') }}</a>
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
    </li>
</template>
  
<script>

import { updateLookupTable, deleteLookupTable } from '../../../../services/lookupTableService'
import { isEmpty, isUndefined, isNull } from 'lodash'
import { newGuid } from '../../../../helpers/guid';
import { createQuestionForDeleteConfirmationPopup, trimText } from '../../../../services/utilityService'


export default {
    name: 'LookupTableItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        table: { type: Object, required: true },
    },
    data() {
        return {
            downloadLookupFileBaseUrl: '/Questionnaire/ExportLookupTable/',
            originName: null,
            file: [],
        }
    },
    beforeMount() {
        const lt = this.findLookupTable(this.table.itemId);
        this.originName = lt.name;
    },
    computed: {
        dirty() {
            return this.table.name != this.originName || this.file.length > 0;
        },
        downloadUrl() {
            return this.downloadLookupFileBaseUrl + this.questionnaireId + '?lookupTableId=' + this.table.itemId;
        },
        isInvalid() {
            return (this.table.name) ? false : true;
        },
        hasUploadedFile() {
            return !isEmpty(this.table.fileName)
        }
    },
    methods: {
        findLookupTable(itemId) {
            const item = this.questionnaire.lookupTables.find(
                p => p.itemId == itemId
            );
            return item;
        },

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
            table.meta.lastUpdated = moment();
        },

        async saveLookupTable() {
            let response = null;

            try {
                this.table.oldItemId = this.table.itemId;
                this.table.itemId = newGuid();

                response = await updateLookupTable(this.questionnaireId, this.table)

                if (this.table.file) notice(response);
                this.table.file = null;
                this.file = [];
            }
            catch (e) {
                const lt = this.findLookupTable(this.table.oldItemId);
                table.itemId = lt.itemId;
                table.oldItemId = lt.oldItemId;
            }

            this.originName = this.table.name;
        },

        cancel() {
            this.table.name = this.originName;
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

        async setDefaultTranslation(isDefault) {
            await setDefaultTranslation(this.questionnaireId, isDefault ? this.translation.translationId : null)
        },

        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
    }
}
</script>
  