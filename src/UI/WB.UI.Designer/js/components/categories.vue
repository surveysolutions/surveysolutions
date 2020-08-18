<template>
    <v-app>
        <v-dialog v-model="dialog" max-width="500">
            <v-card>
                <div class="modal-content">
                    <div class="modal-header blue-strip">
                        <button
                            type="button"
                            class="close"
                            aria-hidden="true"
                            @click="close"
                        >
                            <!--&times;-->
                        </button>
                        <h3>{{ formTitle }}</h3>
                    </div>

                    <div class="modal-body share-question-dialog">
                        <div class="tab-content">
                            <div
                                role="tabpanel"
                                class="tab-pane active"
                                id="questionnaireSettingsTab"
                            >
                                <div class="well-sm">
                                    <form>
                                        <div class="form-group">
                                            <label
                                                class="control-label"
                                                for="value"
                                                >{{
                                                    $t('OptionsUploadValue')
                                                }}</label>
                                            <input
                                                v-model="editedItem.value"
                                                id="value"
                                                type="number"
                                                class="form-control"/>
                                        </div>
                                        <div class="form-group" v-if="$config.isCascading">
                                            <label
                                                class="control-label"
                                                for="parent"
                                                >{{ $t('OptionsUploadParent')}}</label
                                            >
                                            <input
                                                v-model="editedItem.parentValue"
                                                id="parent"
                                                type="text"
                                                class="form-control"
                                            />
                                        </div>
                                        <div class="form-group">
                                            <label
                                                class="control-label"
                                                for="title"
                                                >{{ $t('OptionsUploadTitle') }}</label
                                            >
                                            <input
                                                v-model="editedItem.title"
                                                id="title"
                                                type="text"
                                                class="form-control"
                                            />
                                        </div>
                                        <div class="form-group">
                                            <button
                                                type="button"
                                                class="btn btn-lg update-button"
                                                @click="save">
                                                {{ $t('Save') }}
                                            </button>
                                            <button
                                                    type="reset"
                                                    class="btn btn-lg btn-link"
                                                    @click="close">
                                                {{ $t('Cancel') }}
                                            </button>
                                        </div>
                                    </form>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </v-card>
        </v-dialog>
        <div id="content" class="container">
            <div v-if="hasErrors" class="alert alert-danger">
                <p v-for="error in errors" :key="error">{{ error }}</p>
            </div>
            <v-data-table
                :headers="headers"
                :items="categories"
                :search="search"
                :items-per-page="15"
                :loading="loading"
                class="elevation-1"
                dense>
                <template v-slot:top>
                    <v-toolbar flat color="white">
                        <v-text-field
                            v-model="search"
                            append-icon="mdi-magnify"
                            label="Search"
                            single-line
                            hide-details
                        >
                        </v-text-field>
                        <v-spacer></v-spacer>
                        <button
                            type="button"
                            class="btn btn-default"
                            @click="$refs.uploader.click()"
                        >
                            {{ $t('Upload') }}
                        </button>
                        <input
                            name="file"
                            ref="uploader"
                            v-show="false"
                            accept=".tab, .txt, .tsv"
                            type="file"
                            @change="upload"
                            class="btn btn-default btn-lg btn-action-questionnaire"
                        />&nbsp;
                        <a
                            class="btn btn-default"
                            :href="$config.exportOptionsUrl"
                            >{{ $t('Export') }}</a
                        >&nbsp;
                        <a class="btn btn-primary"  @click="editItem(editedItem)">{{ $t('NewItem') }}</a>
                    </v-toolbar>
                </template>
                <template v-slot:item.actions="{ item }">
                    <v-icon small class="mr-2" @click="editItem(item)">
                        mdi-pencil
                    </v-icon>
                    <v-icon small @click="deleteItem(item)">
                        mdi-delete
                    </v-icon>
                </template>
                <template v-slot:no-data>
                    {{ $t('OptionsUploadLimit').replace('{0}', 15000) }}
                </template>
            </v-data-table>
        </div>
        <nav
            id="bottomNav"
            class="navbar navbar-default navbar-fixed-bottom"
            role="navigation"
        >
            <div class="container">
                <button class="btn btn-lg btn-primary navbar-btn" type="button" @click="apply">{{$t('OptionsUploadApply')}}</button>
                <a class="btn btn-link navbar-btn" @click="cancel">{{
                    $t('Cancel')
                }}</a>
                <a
                    class="btn btn-link navbar-btn pull-right"
                    @click="closeWindow"
                    >{{ $t('Close') }}</a
                >
            </div>
        </nav>
    </v-app>
</template>

<script>
import Vue from 'vue';

export default {
    data() {
        return {
            errors: [],
            categories: [],
            dialog: false,
            loading: true,
            search: '',
            headers: [],
            editedIndex: -1,
            editedItem: {
                value: 0,
                title: '',
                parentValue: 0,
            },
            defaultItem: {
                value: 0,
                title: '',
                parentValue: 0,
            },
        };
    },
    watch: {
        dialog(val) {
            val || this.close();
        },
    },
    mounted() {
        this.initialize();
        this.update();
    },
    computed: {
        formTitle() {
            return this.editedIndex === -1 ? 'New Item' : 'Edit Item';
        },
        hasErrors: function () {
            return this.errors.length > 0;
        },
    },
    methods: {
        initialize() {
            this.headers = [
                {
                    text: this.$t('OptionsUploadValue'),
                    sortable: true,
                    value: 'value',
                },
                {
                    text: this.$t('OptionsUploadTitle'),
                    sortable: true,
                    value: 'title',
                    width: '50%',
                },
            ];
            if (this.$config.isCascading) {
                this.headers.push({
                    text: this.$t('OptionsUploadParent'),
                    sortable: true,
                    value: 'parentValue',
                });
            }

            this.headers.push({ value: 'actions', sortable: false });
        },

        editItem(item) {
            this.editedIndex = this.categories.indexOf(item);
            this.editedItem = Object.assign({}, item);
            this.dialog = true;
        },

        deleteItem(item) {
            const index = this.categories.indexOf(item);
            if (confirm(this.$t('DeleteItemCofirm'))) {            
                this.categories.splice(index, 1);

                $.post(this.$config.deleteOptionUrl, 
                {optionIndex: index},
                function (response) {
                if (response.isSuccess || response.IsSuccess) {
                    close();
                } else {
                    $(document).scrollTop(0);
                    self.errors = [response.Error];
                }
            });
                
            }
        },

        close() {
            this.dialog = false;
            this.$nextTick(() => {
                this.editedItem = Object.assign({}, this.defaultItem);
                this.editedIndex = -1;
            });
        },

        save() {
            if (this.editedIndex > -1) {
                Object.assign(
                    this.categories[this.editedIndex],
                    this.editedItem
                );
            } else {
                this.categories.push(this.editedItem);
            }

            $.post(this.$config.addOrUpdateOptionUrl, 
                {  optionIndex: this.editedIndex,
                   optionValue: this.editedItem.value,
                   optionTitle: this.editedItem.title,
                },
                function (response) {
                if (response.isSuccess || response.IsSuccess) {
                    close();
                } else {
                    $(document).scrollTop(0);
                    self.errors = [response.Error];
                }
                });

            this.close();
        },
        closeWindow: function () {
            if (confirm(this.$t('OptionsCloseWindow'))) {
                close();
            }
        },
        upload: function (e) {
            const files = e.target.files || e.dataTransfer.files;
            if (!files.length) return;

            const file = files[0];

            const formData = new FormData();
            formData.append('csvFile', file);

            this.clearErrors();

            const self = this;
            $.ajax({
                url: this.$config.editOptionsUrl,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
            }).done(function (response) {
                self.errors = response;
                self.update();
            });
        },
        update: function () {
            const self = this;

            self.loading = true;
            $.get(this.$config.getOptionsUrl, function (response) {
                self.categories = response;
                self.loading = false;
            });
        },
        cancel: function () {
            this.clearErrors();

            const self = this;
            $.post(this.$config.resetOptionsUrl, function () {
                self.update();
            });

            //this.update();
        },
        apply: function () {
            const self = this;
            $.post(this.$config.applyUrl, function (response) {
                if (response.isSuccess || response.IsSuccess) {
                    close();
                } else {
                    $(document).scrollTop(0);
                    self.errors = [response.Error];
                }
            });
        },
        clearErrors: function () {
            this.errors = [];
        },
    },
};
</script>

<style>
    .modal-header h3 {
        margin: 0;
    }
    .v-dialog__container {
        display: block !important;
    }
</style>
