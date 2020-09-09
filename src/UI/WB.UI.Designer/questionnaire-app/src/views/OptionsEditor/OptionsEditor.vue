<template>
    <v-container fluid>
        <v-row align="start" justify="center">
            <v-col lg="10">
                <v-card class="mx-4 elevation-12">
                    <v-toolbar dark color="primary" dense>
                        <v-toolbar-title v-if="options">{{
                            formTitle
                        }}</v-toolbar-title>

                        <v-spacer></v-spacer>

                        <v-btn icon class="hidden-xs-only" @click="close()">
                            <v-icon>mdi-close</v-icon>
                        </v-btn>
                    </v-toolbar>
                    <v-tabs v-model="tab" grow @change="tabChange">
                        <v-tab key="table">{{
                            $t('QuestionnaireEditor.Table')
                        }}</v-tab>
                        <v-tab key="strings">{{
                            $t('QuestionnaireEditor.Strings')
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
                    <v-tabs-items v-model="currentTab">
                        <v-tab-item key="table">
                            <category-table
                                :categories="categories"
                                :loading="loading"
                                :show-parent-value="cascading || isCategory"
                            />
                        </v-tab-item>
                        <v-tab-item key="strings">
                            <category-strings
                                v-if="tab == 1"
                                ref="strings"
                                :loading="loading"
                                :show-parent-value="cascading || isCategory"
                                :categories="categories"
                                @change="v => (categories = v)"
                                @editing="v => (inEditMode = v)"
                                @inprogress="v => (convert = v)"
                            />
                        </v-tab-item>
                    </v-tabs-items>
                </v-card>
            </v-col>
        </v-row>
        <v-footer fixed>
            <v-btn
                class="ma-2"
                color="primary"
                :loading="submitting"
                @click="apply"
                >{{ $t('QuestionnaireEditor.OptionsUploadApply') }}</v-btn
            >
            <v-btn @click="resetChanges">{{
                $t('QuestionnaireEditor.OptionsUploadRevert')
            }}</v-btn>
            <v-spacer />

            <v-file-input
                ref="file"
                v-model="file"
                class="pt-2"
                accept=".tab, .txt, .tsv, .xls, .xlsx, .ods"
                :label="$t('QuestionnaireEditor.Upload')"
                dense
                @change="uploadFile"
            ></v-file-input>
            <a :href="exportOptionsUri" class="ma-2 v-btn v-size--default">
                <v-icon>mdi-download</v-icon>
                {{ $t('QuestionnaireEditor.Download') }}</a
            >
        </v-footer>
    </v-container>
</template>

<script>
import CategoryTable from './components/OptionItemsTable';
import CategoryStrings from './components/OptionItemsAsStrings';
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
        isCategory: { type: Boolean, required: true },
        cascading: { type: Boolean, required: false, default: true }
    },

    data() {
        return {
            tab: 'table',
            currentTab: 0,
            categories: [],
            categoriesAsText: '',

            search: null,
            submitting: false,
            errors: [],
            options: null,

            ajax: false,
            convert: false,
            inEditMode: false,

            file: null,

            required: value =>
                !!value || this.$t('QuestionnaireEditor.RequiredField')
        };
    },

    computed: {
        loading() {
            return this.ajax || this.convert;
        },

        formTitle() {
            if (this.isCategory) {
                return (
                    this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                    ': ' +
                    this.options.categoriesName
                );
            }

            return (
                this.$t('QuestionnaireEditor.OptionsWindowTitle') +
                ': ' +
                this.options.questionTitle
            );
        },

        exportOptionsUri() {
            return optionsApi.getExportOptionsUri(
                this.questionnaireRev,
                this.id,
                this.isCategory,
                this.cascading
            );
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
        reloadCategories() {
            if (this.ajax) return;
            if (this.inEditMode || this.convert) {
                setTimeout(() => this.reloadCategories(), 100);
                return;
            }
            this.ajax = true;

            const query = this.isCategory
                ? optionsApi.getCategoryOptions(
                      this.questionnaireRev,
                      this.id,
                      this.cascading
                  )
                : optionsApi.getOptions(
                      this.questionnaireRev,
                      this.id,
                      this.cascading
                  );
            query
                .then(data => {
                    this.categories = data.options;
                    delete data.options;
                    this.options = data;
                })
                .finally(() => (this.ajax = false));
        },

        resetChanges() {
            this.reloadCategories();
        },

        tabChange(tab) {
            if (
                this.loading ||
                (tab == 0 && this.currentTab == 1 && !this.$refs.strings.valid)
            ) {
                this.$nextTick(() => (this.tab = this.currentTab));
                return;
            }

            this.currentTab = this.tab;
        },

        uploadFile(files) {
            if (!files) return;

            const file = files.length ? files[0] : files;

            this.errors = [];

            const apiRequest = this.isCategory
                ? optionsApi.uploadCategory
                : optionsApi.uploadOptions;

            apiRequest(file).then(r => {
                this.errors = r.data;
                this.reloadCategories();
                this.file = null;
            });
        },

        close() {
            close();
        },

        apply() {
            if (this.ajax) return;

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
                    this.cascading,
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
