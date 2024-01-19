<template>
    <form name="categories.form">
        <div class="categories-panel-item"
            :class="{ 'has-error': hasPatternError(), 'dragover': $refs.upload && $refs.upload.dropActive }" ngf-drop=""
            ngf-change="fileSelected(categories, $file)" ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
            <a href @click="deleteCategories" class="btn delete-btn" tabindex="-1" v-if="!isReadOnlyForUser"></a>
            <div class="categories-content">
                <input focus-on-out="focusCategories{{categories.categoriesId}}" required=""
                    :placeholder="$t('QuestionnaireEditor.SideBarCategoriesName')" maxlength="32" spellcheck="false"
                    v-model="category.name" name="name" class="form-control table-name" type="text" />
                <div class="drop-box">
                    {{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}
                </div>
                <div class="actions" :class="{ dirty: dirty }">
                    <div v-show="dirty" class="pull-left">
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

                        <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'cfu' + categoryId" v-model="file"
                            @input-file="fileSelected" :size="10 * 1024 * 1024" :drop="true" :drop-directory="false"
                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab">
                        </file-upload>
                        <button v-show="!dirty" :disabled="isReadOnlyForUser" class="btn btn-default"
                            @click.stop="openFileDialog()" type="button">
                            <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                        </button>

                        <!--button v-show="!dirty" :disabled="isReadOnlyForUser" class="btn btn-default" ngf-select=""
                            ngf-max-size="10MB"
                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab"
                            @change.stop="fileSelected($file)" type="button">
                            <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                        </button-->
                        {{ $t('QuestionnaireEditor.SideBarDownload') }}
                        <a :href="exportOptionsBaseUrl + questionnaire.questionnaireId + '?type=xlsx&isCategory=true&entityId=' + category.categoriesId"
                            class="btn btn-default" target="_blank" rel="noopener">{{
                                $t('QuestionnaireEditor.SideBarXlsx') }}</a>
                        <a :href="exportOptionsBaseUrl + questionnaire.questionnaireId + '?type=csv&isCategory=true&entityId=' + category.categoriesId"
                            class="btn btn-default" target="_blank" rel="noopener">{{
                                $t('QuestionnaireEditor.SideBarTab') }}</a>

                    </div>
                </div>
            </div>
        </div>
    </form>
</template>
  
<script>

import { newGuid } from '../../../../helpers/guid';
import { find, isNull, isUndefined } from 'lodash'
import { updateCategories, deleteCategories } from '../../../../services/categoriesService';
import { trimText, createDeletePopup } from '../../../../services/utilityService'
import moment from 'moment';

export default {
    name: 'CategoryItem',
    inject: ['questionnaire'],
    props: {
        questionnaireId: { type: String, required: true },
        categoryId: { type: String, required: true },
    },
    data() {
        return {
            exportOptionsBaseUrl: '/questionnaire/ExportOptions/',
            category: {},
            originName: null,
            file: []
        }
    },
    beforeMount() {
        this.category = this.findCategory();
        this.originName = this.category.name;
    },
    computed: {
        isReadOnlyForUser() {
            return this.questionnaire.isReadOnlyForUser;
        },
        dirty() {
            return this.category.name != this.originName;
        },
        isInvalid() {
            return (this.category.name) ? false : true;
        },
    },
    methods: {
        findCategory() {
            const category = this.questionnaire.categories.find(
                p => p.categoriesId == this.categoryId
            );
            return category;
        },
        hasPatternError() {
            return (this.category.name) ? false : true;
        },


        deleteCategories(event) {
            event.preventDefault();

            var categoriesName = this.category.name || this.$t('QuestionnaireEditor.SideBarCategoriesNoName');

            var trimmedCategoriesName = trimText(categoriesName);
            var message = this.$t('QuestionnaireEditor.DeleteConfirmCategories', { trimmedTitle: trimmedCategoriesName });

            const confirmParams = {
                title: message,
                okButtonTitle: this.$t('QuestionnaireEditor.Delete'),
                cancelButtonTitle: this.$t('QuestionnaireEditor.Cancel'),
                callback: confirm => {
                    if (confirm) {
                        deleteCategories(this.questionnaireId, this.categoryId)
                    }
                }
            };

            this.$confirm(confirmParams);
        },

        saveCategories() {
            updateCategories(this.questionnaireId, this.category)
            /*.then(function (response) {
                    dataBind(categories.checkpoint, categories);
                    categories.form.$setPristine();

                    updateQuestionnaireCategories(categories);
                }).catch(function() {
                    categories.categoriesId = categories.oldCategoriesId;
                });*/
        },

        cancel() {
            this.category.name = this.originName;
        },

        editCategories() {

        },

        async fileSelected(newFile) {
            if (isUndefined(newFile) || isNull(newFile)) {
                return;
            }

            let categories = this.category;

            categories.file = newFile.file;

            categories.content = {};
            categories.content.size = newFile.size;
            categories.content.type = newFile.type;

            categories.meta = {};
            categories.meta.fileName = newFile.name;
            categories.meta.lastUpdated = moment();

            var maxNameLength = 32;

            var suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedCategories && suspectedCategories.length > 0)
                categories.name = suspectedCategories[0];
            else
                categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

            var fileNameLength = categories.name.length;
            categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
            categories.oldCategoriesId = categories.categoriesId;
            categories.categoriesId = newGuid();

            const response = await updateCategories(this.questionnaireId, categories)

            if (categories.file) notificationService.notice(response.data);

            categories.file = null;
            this.file = [];

        },

        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
    },
}
</script>
  