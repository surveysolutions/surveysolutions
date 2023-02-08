<template>
    <v-container fluid>
        <v-snackbar v-model="snacks.fileUploaded" top color="success">{{
            $t('QuestionnaireEditor.FileUploaded')
        }}</v-snackbar>
        <v-snackbar v-model="snacks.formReverted" top color="success">{{
            $t('QuestionnaireEditor.DataChangesReverted')
        }}</v-snackbar>
        <v-snackbar v-model="snacks.ajaxError" top color="error">{{
            $t('QuestionnaireEditor.CommunicationError')
        }}</v-snackbar>
        <v-row align="start" justify="center">
            <v-col lg="10">
                <v-card class="mx-4 elevation-12" min-width="680">
                    <v-toolbar dense dark color="primary">
                        <v-toolbar-title v-if="options">{{
                            formTitle
                        }}</v-toolbar-title>
                    </v-toolbar>
                    <v-tabs v-model="tab" fixed-tabs grow>
                        <v-tab value="table" :disabled="!stringsIsValid">{{
                            $t('QuestionnaireEditor.TableView')
                        }}</v-tab>
                        <v-tab value="strings">{{
                            $t('QuestionnaireEditor.StringsView')
                        }}</v-tab>
                    </v-tabs>
                    <div v-if="errors.length > 0" class="alert alert-danger">
                        <v-card-text>
                            <v-alert
                                v-for="error in errors"
                                :key="error"
                                outlined
                                type="error"
                            >
                                {{ error }}
                            </v-alert>
                        </v-card-text>
                    </div>
                    <v-window v-model="tab">
                        <v-window-item value="table">
                            <category-table
                                ref="table"
                                :categories="categories"
                                :parent-categories="parentCategories"
                                :loading="loading"
                                :is-category="isCategory"
                                :is-cascading="isCascading"
                                :readonly="isReadonly"
                                @setCascading="setCascadingCategory"
                                @update-categories="updateCategories"
                            />
                        </v-window-item>
                        <v-window-item value="strings">
                            <category-strings
                            v-if="tab == 'strings'"
                                ref="strings"
                                :loading="loading"
                                :show-parent-value="isCascading"
                                :categories="categories"
                                :readonly="isReadonly"
                                @valid="v => (stringsIsValid = v)"
                                @change="v => (categories = v)"
                                @editing="v => (inEditMode = v)"
                                @inprogress="v => (convert = v)"
                            />
                        </v-window-item>
                    </v-window>
                </v-card>
            </v-col>
        </v-row>
        <v-footer app min-width="680">
            <v-btn
                v-if="!readonly"
                class="ma-2"
                color="success"
                :disabled="!canApplyChanges"
                :loading="submitting"
                @click="apply"
                >{{ $t('QuestionnaireEditor.OptionsUploadApply') }}</v-btn
            >
            <v-btn v-if="!readonly" @click="resetChanges">{{
                $t('QuestionnaireEditor.OptionsUploadRevert')
            }}</v-btn>
            <v-btn v-if="readonly" @click="close">{{
                $t('QuestionnaireEditor.Close')
            }}</v-btn>
            <v-spacer />

            <v-file-input
                v-if="!readonly"
                ref="file"
                v-model="file"
                class="pt-2"
                accept=".tab, .txt, .tsv, .xls, .xlsx, .ods"
                :label="$t('QuestionnaireEditor.Upload')"
                dense
                @change="uploadFile"
            ></v-file-input>
            <span>
                <v-icon>mdi-download</v-icon>
                {{ $t('QuestionnaireEditor.SideBarDownload') }}
            </span>
            <a
                :href="exportOptionsAsExlsUri"
                class="ma-2 v-btn v-size--default"
            >
                {{ $t('QuestionnaireEditor.SideBarXlsx') }}</a
            >
            <a :href="exportOptionsAsTabUri" class="ma-2 v-btn v-size--default">
                {{ $t('QuestionnaireEditor.SideBarTab') }}</a
            >
        </v-footer>
    </v-container>
</template>

<script>
import CategoryTable from './components/OptionItemsTable.vue';
import CategoryStrings from './components/OptionItemsAsStrings.vue';
import { optionsApi } from './services';

export default {
    name: 'CategoriesEditor',

    components: {
        CategoryTable,
        CategoryStrings
    },

    props: {
        questionnaireRev: { type: String, required: true },
        id: { type: String, required: true },
        isCategory: { type: Boolean, required: false },
        cascading: { type: Boolean, required: false, default: false }
    },

    data() {
        return {
            tab: '',
            categories: [],
            parentCategories: null,
            categoriesAsText: '',
            submitting: false,
            errors: [],

            options: null,

            ajax: false,
            readonly: true,
            convert: false,
            inEditMode: false,

            file: null,

            isCascadingCategory: false,
            stringsIsValid: true,

            snacks: {
                fileUploaded: false,
                formReverted: false,
                ajaxError: false
            },

            required: value =>
                !!value || this.$t('QuestionnaireEditor.RequiredField')
        };
    },

    computed: {
        loading() {
            return this.ajax || this.convert;
        },

        isCascading() {
            return this.cascading === true || this.isCascadingCategory === true;
        },

        isReadonly() {
            return this.readonly;
        },

        formTitle() {
            if (this.isCategory) {
                return (
                    this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                    ': ' +
                    this.options.categoriesName
                );
            }

            if (this.isCascading) {
                return (
                    this.$t('QuestionnaireEditor.CascadingOptionsWindowTitle') +
                    ': ' +
                    this.options.questionTitle
                );
            }

            return (
                this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                ': ' +
                this.options.questionTitle
            );
        },

        exportOptionsAsTabUri() {
            return optionsApi.getExportOptionsAsTabUri(
                this.questionnaireRev,
                this.id,
                this.isCategory,
                this.isCascading
            );
        },

        exportOptionsAsExlsUri() {
            return optionsApi.getExportOptionsAsExlsUri(
                this.questionnaireRev,
                this.id,
                this.isCategory,
                this.isCascading
            );
        },

        canApplyChanges() {
            return this.tab == 1 ? this.stringsIsValid : true;
        }
    },

    mounted() {
        const self = this;
        window.onbeforeunload = function() {
            if ('BroadcastChannel' in window) {
                new BroadcastChannel('editcategory').postMessage(
                    `close#${self.id}`
                );
            }
        };

        this.reloadCategories();

        document.title = this.$t('QuestionnaireEditor.OptionsWindowTitle');
    },

    methods: {
        setCascadingCategory(cascadingCategory) {
            this.isCascadingCategory = cascadingCategory;
        },

        async reloadCategories(onDone) {
            if (this.ajax) return;
            if (this.inEditMode || this.convert) {
                setTimeout(() => this.reloadCategories(), 100);
                return;
            }
            this.ajax = true;

            try {
                const query = this.isCategory
                    ? optionsApi.getCategoryOptions(
                          this.questionnaireRev,
                          this.id,
                          this.isCascading
                      )
                    : optionsApi.getOptions(
                          this.questionnaireRev,
                          this.id,
                          this.isCascading
                      );

                const data = await query;
                this.categories = data.options;
                this.readonly = data.isReadonly;

                if (
                    this.isCategory &&
                    data.options.find(o => o.parentValue != null)
                ) {
                    this.isCascadingCategory = true;
                }
                delete data.options;
                this.options = data;

                if (this.options.cascadeFromQuestionId) {
                    const parent = await optionsApi.getOptions(
                        this.questionnaireRev,
                        this.options.cascadeFromQuestionId,
                        false
                    );

                    this.parentCategories = parent.options;
                }

                if (onDone) onDone.apply(this);
            } catch (e) {
                //console.error(e);
                this.snacks.ajaxError = true;
            } finally {
                this.ajax = false;
            }
        },

        resetChanges() {
            this.reloadCategories(() => {
                this.snacks.formReverted = true;
                this.isCascadingCategory = this.cascading;
                this.errors = [];
            });
        },

        uploadFile(files) {
            if (!files) return;

            const file = files.length ? files[0] : files;

            this.errors = [];

            const apiRequest = this.isCategory
                ? optionsApi.uploadCategory(file)
                : optionsApi.uploadOptions(
                      this.questionnaireRev,
                      this.id,
                      file
                  );

            apiRequest.then(r => {
                this.errors = r.data.errors;
                this.categories = r.data.options;
                this.file = null;
                this.snacks.fileUploaded = true;

                if (this.$refs.table != null) {
                    this.$refs.table.reset();
                }
            });
        },

        close() {
            close();
        },

        updateCategories(newCategories) {
            this.categories = newCategories;
        },

        apply() {
            if (this.ajax) return;

            if (!this.canApplyChanges) return;

            if (this.inEditMode || this.convert) {
                setTimeout(() => this.apply(), 100);
                return;
            }

            this.ajax = true;
            this.submitting = true;
            this.errors = [];

            optionsApi
                .applyOptions(
                    this.categories,
                    this.questionnaireRev,
                    this.id,
                    this.isCascading,
                    this.isCategory
                )
                .then(response => {
                    if (response.isSuccess || response.IsSuccess) {
                        this.close();
                    } else {
                        this.ajax = false;
                        this.errors = [response.error];
                    }
                })
                .finally(() => {
                    this.ajax = false;
                    this.submitting = false;
                });
        }
    }
};
</script>
