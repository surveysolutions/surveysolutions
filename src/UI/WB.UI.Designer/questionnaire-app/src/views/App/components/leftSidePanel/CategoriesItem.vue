<template>
    <form name="categories.form">
        <div class="categories-panel-item"
            :class="{ 'has-error': hasPatternError(), 'dragover': $refs.upload && $refs.upload.dropActive }" ngf-drop=""
            ngf-change="fileSelected(categories, $file)" ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
            <a href="javascript:void(0);" @click="deleteCategories" class="btn delete-btn" tabindex="-1"
                v-if="!isReadOnlyForUser"></a>
            <div class="categories-content">
                <input focus-on-out="focusCategories{{categories.categoriesId}}" required=""
                    :placeholder="$t('QuestionnaireEditor.SideBarCategoriesName')" maxlength="32" spellcheck="false"
                    v-model="categories.name" name="name" class="form-control table-name" type="text" />
                <div class="drop-box">
                    {{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}
                </div>
                <div class="actions" :class="{ 'dirty': isDirty }">
                    <div v-show="isDirty" class="pull-left">
                        <button type="button" :disabled="isInvalid" class="btn lighter-hover"
                            @click.stop="saveCategories(categories)">
                            {{ $t('QuestionnaireEditor.Save') }}
                        </button>
                        <button type="button" class="btn lighter-hover" @click.stop="cancel()">{{
                            $t('QuestionnaireEditor.Cancel') }}</button>
                    </div>
                    <div class="permanent-actions pull-right">
                        <a href="javascript:void(0);" class="btn btn-link" @click="editCategories()">{{
                            $t('QuestionnaireEditor.SideBarEditCategories') }}
                        </a>
                        <file-upload ref="upload" v-if="!isReadOnlyForUser" v-model="file" @input-file="fileSelected"
                            :size="10 * 1024 * 1024" :drop="true" :drop-directory="false"
                            :input-id="'cifu' + categoriesItem.categoriesId"
                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab">
                        </file-upload>
                        <button v-show="!isDirty" v-if="!isReadOnlyForUser" class="btn btn-default"
                            @click.stop="openFileDialog()" type="button">
                            <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                        </button>
                        {{ $t('QuestionnaireEditor.SideBarDownload') }}
                        <a :href="exportOptionsBaseUrl + questionnaire.questionnaireId + '?type=xlsx&isCategory=true&entityId=' + categories.categoriesId"
                            class="btn btn-default" target="_blank" rel="noopener">{{
                                $t('QuestionnaireEditor.SideBarXlsx') }}</a>
                        <a :href="exportOptionsBaseUrl + questionnaire.questionnaireId + '?type=csv&isCategory=true&entityId=' + categories.categoriesId"
                            class="btn btn-default" target="_blank" rel="noopener">{{
                                $t('QuestionnaireEditor.SideBarTab') }}</a>

                    </div>
                </div>
            </div>
        </div>
    </form>
</template>
  
<script>

import { isNull, isUndefined, cloneDeep } from 'lodash'
import { updateCategories, deleteCategories } from '../../../../services/categoriesService';
import { trimText } from '../../../../services/utilityService'
import { notice } from '../../../../services/notificationService';
import moment from 'moment';

export default {
    name: 'CategoriesItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        categoriesItem: { type: Object, required: true },
    },
    data() {
        return {
            exportOptionsBaseUrl: '/questionnaire/ExportOptions/',
            file: []
        }
    },
    computed: {
        categories() {
            return this.categoriesItem.editCategories;
        },
        isDirty() {
            return this.categories.name != this.categoriesItem.name || (this.categories.file !== null && this.categories.file !== undefined);
        },
        isInvalid() {
            return (this.categories.name) ? false : true;
        },
    },
    methods: {
        hasPatternError() {
            return (this.categories.name) ? false : true;
        },

        deleteCategories(event) {
            var categoriesName = this.categories.name || this.$t('QuestionnaireEditor.SideBarCategoriesNoName');

            var trimmedCategoriesName = trimText(categoriesName);
            var message = this.$t('QuestionnaireEditor.DeleteConfirmCategories', { trimmedTitle: trimmedCategoriesName });

            const confirmParams = {
                title: message,
                okButtonTitle: this.$t('QuestionnaireEditor.Delete'),
                cancelButtonTitle: this.$t('QuestionnaireEditor.Cancel'),
                callback: confirm => {
                    if (confirm) {
                        deleteCategories(this.questionnaireId, this.categories.categoriesId)
                    }
                }
            };

            this.$confirm(confirmParams);
        },

        async saveCategories() {
            const response = await updateCategories(this.questionnaireId, this.categories);

            if (this.file.length > 0)
                notice(response);

            this.categories.file = null;
            this.file = [];
        },

        cancel() {
            var clonned = cloneDeep(this.categoriesItem);
            clonned.editCategories = null;
            this.categoriesItem.editCategories = clonned;
        },

        editCategories() {
            this.$emit('editCategoriesOpen', { categoriesId: this.categories.categoriesId })
        },

        async fileSelected(newFile) {
            if (isUndefined(newFile) || isNull(newFile)) {
                return;
            }

            let categories = this.categories;

            categories.file = newFile.file;

            categories.content = {};
            categories.content.size = newFile.size;
            categories.content.type = newFile.type;

            categories.meta = {};
            categories.meta.fileName = newFile.name;
            categories.meta.lastUpdated = moment();


            var suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedCategories && suspectedCategories.length > 0)
                categories.name = suspectedCategories[0];
            else
                categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

            var maxNameLength = 32;
            var fileNameLength = categories.name.length;
            categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
        },

        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
    },
}
</script>
  