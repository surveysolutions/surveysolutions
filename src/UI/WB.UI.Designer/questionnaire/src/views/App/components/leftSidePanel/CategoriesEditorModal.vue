<template>
    <teleport to="body">
        <div v-if="isOpen" class="modal fade in options-editor-modal" role="dialog"
            tabindex="-1" aria-labelledby="options-editor-modal-title" style="z-index: 1050; display: block;">
            <div class="modal-dialog options-editor-modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" aria-label="Close" @click="close"></button>
                        <h3 class="modal-title" id="options-editor-modal-title">{{ formTitle }}</h3>
                    </div>
                    <div class="modal-body options-editor-modal-body">
                        <div v-if="errors.length > 0" class="alert alert-danger options-editor-errors">
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
                                    :is-category="isCategory" :is-cascading="isCascadingEnabled"
                                    :readonly="isReadonly"
                                    @setCascading="setCascadingCategory"
                                    @update-categories="updateCategories" />
                            </v-window-item>
                            <v-window-item value="strings">
                                <category-strings v-if="tab === 'strings'" ref="stringsEditor"
                                    :loading="loading"
                                    :show-parent-value="isCascadingEnabled"
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
                    <div class="modal-footer options-editor-modal-footer">
                        <div class="options-editor-footer-left">
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
                        <div class="options-editor-footer-right">
                            <label v-if="!isReadonly" class="btn btn-default options-editor-upload-label">
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
import { sanitize } from '../../../../services/utilityService';

export default {
    name: 'OptionsEditorModal',

    components: {
        CategoryTable,
        CategoryStrings
    },

    data() {
        return {
            isOpen: false,
            questionnaireId: null,
            entityId: null,
            isCategory: false,
            forcedCascading: false,
            isCascading: false,
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

        isCascadingEnabled() {
            return this.forcedCascading || this.isCascading;
        },

        formTitle() {
            if (this.options) {
                if (!this.isCategory && this.isCascadingEnabled) {
                    return (
                        this.$t('QuestionnaireEditor.CascadingOptionsWindowTitle') +
                        ': ' +
                        sanitize(this.options.questionTitle)
                    );
                }

                return (
                    this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                    ': ' +
                    (this.isCategory
                        ? this.options.categoriesName
                        : sanitize(this.options.questionTitle))
                );
            }
            return '';
        },

        exportOptionsAsTabUri() {
            return optionsApi.getExportOptionsAsTabUri(
                this.questionnaireId,
                this.entityId,
                this.isCategory,
                this.isCascadingEnabled
            );
        },

        exportOptionsAsExlsUri() {
            return optionsApi.getExportOptionsAsExlsUri(
                this.questionnaireId,
                this.entityId,
                this.isCategory,
                this.isCascadingEnabled
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
        open(questionnaireId, entityId, { isCategory = false, isCascading = false } = {}) {
            this.questionnaireId = questionnaireId;
            this.entityId = entityId;
            this.isCategory = isCategory;
            this.forcedCascading = isCascading;
            this.isCascading = isCascading;
            // Increment session token so any in-flight requests from a previous open()
            // will be ignored when they resolve.
            this.sessionToken++;
            // Reset all state before showing so stale data from a previous session
            // is never visible if the modal is reopened without closing first.
            this.options = null;
            this.categories = [];
            this.initialCategories = [];
            this.parentCategories = null;
            this.errors = [];
            this.submitting = false;
            this.ajax = false;
            this.convert = false;
            this.inEditMode = false;
            this.readonly = true;
            this.stringsIsValid = true;
            this.stringsIsDirty = false;
            this.tab = '';
            this.isOpen = true;
            this.reloadCategories();
        },

        close() {
            this.sessionToken++;
            this.isOpen = false;
            this.questionnaireId = null;
            this.entityId = null;
            this.isCategory = false;
            this.forcedCascading = false;
            this.isCascading = false;
            this.options = null;
            this.categories = [];
            this.initialCategories = [];
            this.parentCategories = null;
            this.errors = [];
            this.ajax = false;
            this.convert = false;
            this.inEditMode = false;
            this.submitting = false;
            this.readonly = true;
            this.stringsIsValid = true;
            this.stringsIsDirty = false;
            this.tab = '';
        },

        setCascadingCategory(cascadingCategory) {
            this.isCascading = !!cascadingCategory;
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
                const data = this.isCategory
                    ? await optionsApi.getCategoryOptions(
                        this.questionnaireId,
                        this.entityId
                    )
                    : await optionsApi.getOptions(
                        this.questionnaireId,
                        this.entityId,
                        this.isCascadingEnabled
                    );
                if (token !== this.sessionToken) return;

                this.categories = data.options;
                this.initialCategories = cloneDeep(data.options);
                this.readonly = data.isReadonly;

                if (this.isCategory) {
                    this.isCascading = data.options.some(o => o.parentValue != null);
                }

                delete data.options;
                this.options = data;

                if (
                    !this.isCategory &&
                    this.options.cascadeFromQuestionId
                ) {
                    const parent = await optionsApi.getOptions(
                        this.questionnaireId,
                        this.options.cascadeFromQuestionId,
                        false
                    );
                    if (token !== this.sessionToken) return;
                    this.parentCategories = parent.options;
                }

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
                const apiResponse = this.isCategory
                    ? await optionsApi.uploadCategory(file)
                    : await optionsApi.uploadOptions(
                        this.questionnaireId,
                        this.entityId,
                        file
                    );

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
                    this.entityId,
                    this.isCascadingEnabled,
                    this.isCategory
                );
                if (token !== this.sessionToken) return;
                if (response.isSuccess || response.IsSuccess) {
                    const entityId = this.entityId;
                    this.close();
                    this.$emit('applied', { entityId });
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
.options-editor-modal-dialog {
    width: min(96vw, 1680px);
    max-width: calc(100vw - 32px);
}

.options-editor-modal-body {
    padding: 0;
    min-height: 400px;
    max-height: calc(100vh - 180px);
    overflow: auto;
}

.options-editor-errors {
    margin: 12px;
}

.options-editor-modal-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 12px;
}

.options-editor-footer-left {
    display: flex;
    gap: 8px;
    align-items: center;
    flex-wrap: wrap;
}

.options-editor-footer-right {
    display: flex;
    gap: 8px;
    align-items: center;
    flex-wrap: wrap;
}

.options-editor-upload-label {
    cursor: pointer;
    margin-bottom: 0;
}
</style>
