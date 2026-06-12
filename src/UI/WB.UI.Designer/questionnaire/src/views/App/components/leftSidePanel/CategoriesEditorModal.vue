<template>
    <teleport to="body">
        <div v-if="isOpen" class="modal fade in categories-editor-modal" role="dialog"
            tabindex="-1" aria-labelledby="categories-editor-modal-title" style="z-index: 1050; display: block;">
            <div class="modal-dialog modal-xl">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" aria-label="Close" @click="close"></button>
                        <h3 class="modal-title" id="categories-editor-modal-title" v-if="options">{{ formTitle }}</h3>
                    </div>
                    <div class="modal-body categories-editor-modal-body">
                        <div v-if="errors.length > 0" class="alert alert-danger categories-editor-errors">
                            <div v-for="error in errors" :key="error">{{ error }}</div>
                        </div>

                        <v-tabs v-model="tab" color="primary" fixed-tabs grow>
                            <v-tab value="table" :disabled="!stringsIsValid">{{
                                $t('QuestionnaireEditor.TableView')
                            }}</v-tab>
                            <v-tab value="strings">{{
                                $t('QuestionnaireEditor.StringsView')
                            }}</v-tab>
                        </v-tabs>

                        <v-window v-model="tab">
                            <v-window-item value="table">
                                <category-table ref="table" :categories="categories"
                                    :parent-categories="parentCategories" :loading="loading"
                                    :is-category="true" :is-cascading="isCascadingCategory"
                                    :readonly="isReadonly"
                                    @setCascading="setCascadingCategory"
                                    @update-categories="updateCategories" />
                            </v-window-item>
                            <v-window-item value="strings">
                                <category-strings v-if="tab === 'strings'" ref="stringsEditor"
                                    :loading="loading"
                                    :show-parent-value="isCascadingCategory"
                                    :categories="categories"
                                    :readonly="isReadonly"
                                    @string-valid="v => (stringsIsValid = v)"
                                    @changeCategories="v => (categories = v)"
                                    @editing="v => (inEditMode = v)"
                                    @inprogress="v => (convert = v)"
                                    @is-dirty="v => (stringsIsDirty = v)" />
                            </v-window-item>
                        </v-window>
                    </div>
                    <div class="modal-footer categories-editor-modal-footer">
                        <div class="categories-editor-footer-left">
                            <button v-if="!isReadonly" type="button" class="btn btn-primary"
                                :disabled="!canApplyChanges || submitting" @click="apply">
                                {{ $t('QuestionnaireEditor.OptionsUploadApply') }}
                            </button>
                            <button v-if="!isReadonly" type="button" class="btn btn-default"
                                :disabled="!isDirty" @click="resetChanges">
                                {{ $t('QuestionnaireEditor.OptionsUploadRevert') }}
                            </button>
                            <button type="button" class="btn btn-link" @click="close">
                                {{ $t('QuestionnaireEditor.Close') }}
                            </button>
                        </div>
                        <div class="categories-editor-footer-right">
                            <label v-if="!isReadonly" class="btn btn-default categories-editor-upload-label">
                                {{ $t('QuestionnaireEditor.Upload') }}
                                <input type="file" style="display:none" ref="fileInput"
                                    accept=".tab,.txt,.tsv,.xls,.xlsx,.ods" @change="uploadFile">
                            </label>
                            <span>{{ $t('QuestionnaireEditor.SideBarDownload') }}</span>
                            <a :href="!canDownloadCategories ? null : exportOptionsAsExlsUri"
                                :class="{ 'disabled': !canDownloadCategories }"
                                @click="!canDownloadCategories && $event.preventDefault()"
                                class="btn btn-default">
                                {{ $t('QuestionnaireEditor.SideBarXlsx') }}
                            </a>
                            <a :href="!canDownloadCategories ? null : exportOptionsAsTabUri"
                                :class="{ 'disabled': !canDownloadCategories }"
                                @click="!canDownloadCategories && $event.preventDefault()"
                                class="btn btn-default">
                                {{ $t('QuestionnaireEditor.SideBarTab') }}
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div v-if="isOpen" class="modal-backdrop fade in" style="z-index: 1040;"></div>
    </teleport>
</template>

<script>
import 'vuetify/styles';
import CategoryTable from '../../../OptionsEditor/components/OptionItemsTable.vue';
import CategoryStrings from '../../../OptionsEditor/components/OptionItemsAsStrings.vue';
import { optionsApi } from '../../../OptionsEditor/services';
import { isEqual, cloneDeep } from 'lodash';

export default {
    name: 'CategoriesEditorModal',

    components: {
        CategoryTable,
        CategoryStrings
    },

    data() {
        return {
            isOpen: false,
            questionnaireId: null,
            categoriesId: null,
            sessionToken: 0,

            tab: '',
            categories: [],
            initialCategories: [],
            parentCategories: null,
            submitting: false,
            errors: [],

            options: null,

            ajax: false,
            convert: false,
            inEditMode: false,
            stringsIsDirty: false,

            readonly: true,
            isCascadingCategory: false,
            stringsIsValid: true,
        };
    },

    computed: {
        loading() {
            return this.ajax || this.convert;
        },

        isReadonly() {
            return this.readonly;
        },

        formTitle() {
            if (this.options) {
                return (
                    this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                    ': ' +
                    this.options.categoriesName
                );
            }
            return '';
        },

        exportOptionsAsTabUri() {
            return optionsApi.getExportOptionsAsTabUri(
                this.questionnaireId,
                this.categoriesId,
                true,
                this.isCascadingCategory
            );
        },

        exportOptionsAsExlsUri() {
            return optionsApi.getExportOptionsAsExlsUri(
                this.questionnaireId,
                this.categoriesId,
                true,
                this.isCascadingCategory
            );
        },

        canApplyChanges() {
            if (this.tab === 'strings' && !this.stringsIsValid) return false;
            return this.isDirty;
        },

        isDirty() {
            const equal = isEqual(this.categories, this.initialCategories);
            if (!equal) return true;
            return this.tab === 'strings' ? this.stringsIsDirty : false;
        },

        canDownloadCategories() {
            if (this.isDirty) return false;
            return this.tab === 'strings' && !this.stringsIsValid ? false : true;
        }
    },

    methods: {
        open(questionnaireId, categoriesId) {
            this.questionnaireId = questionnaireId;
            this.categoriesId = categoriesId;
            // Increment session token so any in-flight requests from a previous open()
            // will be ignored when they resolve.
            this.sessionToken++;
            // Reset all state before showing so stale data from a previous session
            // is never visible if the modal is reopened without closing first.
            this.options = null;
            this.categories = [];
            this.initialCategories = [];
            this.errors = [];
            this.submitting = false;
            this.ajax = false;
            this.convert = false;
            this.inEditMode = false;
            this.readonly = true;
            this.isCascadingCategory = false;
            this.stringsIsValid = true;
            this.stringsIsDirty = false;
            this.tab = '';
            this.isOpen = true;
            this.reloadCategories();
        },

        close() {
            this.sessionToken++;
            this.isOpen = false;
            this.options = null;
            this.categories = [];
            this.initialCategories = [];
            this.errors = [];
            this.ajax = false;
            this.convert = false;
            this.inEditMode = false;
            this.submitting = false;
            this.readonly = true;
            this.isCascadingCategory = false;
            this.stringsIsValid = true;
            this.stringsIsDirty = false;
            this.tab = '';
        },

        setCascadingCategory(cascadingCategory) {
            this.isCascadingCategory = cascadingCategory;
        },

        async reloadCategories(onDone) {
            if (this.ajax) return;
            if (this.inEditMode || this.convert) {
                setTimeout(() => { if (this.isOpen) this.reloadCategories(onDone); }, 100);
                return;
            }
            this.ajax = true;
            const token = this.sessionToken;

            try {
                const data = await optionsApi.getCategoryOptions(
                    this.questionnaireId,
                    this.categoriesId
                );
                if (token !== this.sessionToken) return;
                this.categories = data.options;
                this.initialCategories = cloneDeep(data.options);
                this.readonly = data.isReadonly;

                if (data.options.find(o => o.parentValue != null)) {
                    this.isCascadingCategory = true;
                }

                delete data.options;
                this.options = data;

                if (onDone) onDone.apply(this);
            } catch (e) {
                if (token !== this.sessionToken) return;
                this.errors = [this.$t('QuestionnaireEditor.CommunicationError')];
            } finally {
                if (token === this.sessionToken) {
                    this.ajax = false;
                }
            }
        },

        resetChanges() {
            this.reloadCategories(() => {
                this.errors = [];
            });
        },

        async uploadFile(event) {
            const file = event.target.files[0];
            if (!file) return;

            this.errors = [];
            const token = this.sessionToken;

            try {
                const apiResponse = await optionsApi.uploadCategory(file);

                if (token !== this.sessionToken) return;

                this.errors = apiResponse.errors || [];
                this.categories = apiResponse.options || [];

                if (this.$refs.fileInput) {
                    this.$refs.fileInput.value = '';
                }

                if (this.$refs.table != null) {
                    this.$refs.table.reset();
                }
            } catch (e) {
                if (token !== this.sessionToken) return;
                this.errors = [this.$t('QuestionnaireEditor.CommunicationError')];
            }
        },

        updateCategories(newCategories) {
            this.categories = newCategories;
        },

        async apply() {
            if (this.ajax) return;
            if (!this.canApplyChanges) return;

            if (this.inEditMode || this.convert) {
                setTimeout(() => { if (this.isOpen) this.apply(); }, 100);
                return;
            }

            this.ajax = true;
            this.submitting = true;
            this.errors = [];
            const token = this.sessionToken;

            try {
                const response = await optionsApi.applyOptions(
                    this.categories,
                    this.questionnaireId,
                    this.categoriesId,
                    this.isCascadingCategory,
                    true
                );
                if (token !== this.sessionToken) return;
                if (response.isSuccess || response.IsSuccess) {
                    this.close();
                } else {
                    this.ajax = false;
                    this.errors = [response.error];
                }
            } catch (e) {
                if (token !== this.sessionToken) return;
                this.errors = [this.$t('QuestionnaireEditor.CommunicationError')];
            } finally {
                if (token === this.sessionToken) {
                    this.ajax = false;
                    this.submitting = false;
                }
            }
        }
    }
};
</script>

<style scoped>
.categories-editor-modal-body {
    padding: 0;
    min-height: 400px;
}

.categories-editor-errors {
    margin: 12px;
}

.categories-editor-modal-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
}

.categories-editor-footer-left {
    display: flex;
    gap: 4px;
    align-items: center;
}

.categories-editor-footer-right {
    display: flex;
    gap: 4px;
    align-items: center;
}

.categories-editor-upload-label {
    cursor: pointer;
    margin-bottom: 0;
}
</style>
