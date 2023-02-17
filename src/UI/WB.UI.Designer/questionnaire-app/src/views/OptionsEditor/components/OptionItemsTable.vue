<template>
    
        <v-card-title class="d-flex">
            <v-text-field
                v-model="search"
                append-icon="mdi-magnify"
                :label="$t('QuestionnaireEditor.Filter')"
                single-line
                clearable
                hide-details
            ></v-text-field>
            <v-spacer></v-spacer>
            <v-btn
                v-if="!readonly"
                class="ma-2"
                color="primary"
                @click="newRow"
            >
                <v-icon left>mdi-plus</v-icon
                >{{ $t('QuestionnaireEditor.NewItem') }}
            </v-btn>
            <v-btn
                v-if="isCategory && !isCascading"
                class="ma-2"
                @click="setCascading(true)"
                >{{ $t('QuestionnaireEditor.ShowParentValues') }}</v-btn
            >
            <v-btn
                v-if="isCategory && isCascading"
                class="ma-2"
                @click="setCascading(null)"
                >{{ $t('QuestionnaireEditor.HideParentValues') }}</v-btn
            >
        </v-card-title>

        <category-dialog
            v-if="dialog"
            :title="formTitle"
            :item="editedItem"
            :parent-categories="parentCategories"
            :shown="dialog"
            :show-parent-value="isCascading"
            @cancel="dialog = false"
            @saveCategory="save"
        />

        <v-snackbar v-model="snacks.rowAdded" location='top' color="success">{{
            $t('QuestionnaireEditor.RowAdded')
        }}</v-snackbar>

        <v-data-table
            ref="table"
            :headers="headers"
            :items="categoriesLocal"
            :search="search"
            :items-per-page="10"
            :footer-props="{'items-per-page-options': [10, 20, 50, 100]}"
            :loading="loading"
            class="table-striped elevation-1 mb-14"
            style="overflow-wrap:anywhere;"
            density="compact"
        >
            <template #[`item.value`]="{ item }">
                <span class="text-no-wrap">{{ item.raw.value }}</span>
            </template>
            <template #[`item.parentValue`]="props">
                <div>
                    {{ props.item.parentValue }}
                    <span
                        v-if="parentCategories && parentCategories.length > 0"
                        class="caption text--disabled .d-none .d-md-flex .d-lg-none"
                        >{{
                            captionForParentValue(props.item.parentValue)
                        }}</span
                    >
                </div>
            </template>
            <template #[`item.actions`]="{ item }">
                <div v-if="!readonly">
                    <v-icon small class="mr-2" @click="editItem(item.raw)"
                        >mdi-pencil</v-icon
                    >
                    <v-icon small @click="deleteItem(item.raw)">mdi-delete</v-icon>
                </div>
            </template>
            <!-- <template #no-data>
                {{
                    $t('QuestionnaireEditor.OptionsUploadLimit', {
                        limit: 15000
                    })
                }}
            </template> -->
        </v-data-table>    
</template>

<script>
import CategoryDialog from './OptionItemDialog.vue';

export default {
    name: 'CategoryEditorTable',

    components: {
        CategoryDialog
    },

    props: {
        categories: { type: Array, required: true },
        parentCategories: { type: Array, required: false, default: () => [] },
        isCategory: { type: Boolean, required: true },
        isCascading: { type: Boolean, required: true },
        loading: { type: Boolean, required: true },
        readonly: { type: Boolean, required: true }
    },

    data() {
        return {
            search: null,
            editedItem: null,
            editedIndex: null,
            dialog: false,

            snacks: { rowAdded: false },

            required: value =>
                !!value || this.$t('QuestionnaireEditor.RequiredField'),
            maxValue: v =>
                Math.abs(parseInt(v)) < 2147483647 ||
                this.$t('QuestionnaireEditor.ValidationIntValue')
        };
    },

    computed: {
        headers() {
            const headers = [
                {
                    title: this.$t('QuestionnaireEditor.OptionsUploadValue'),
                    sortable: false,
                    width: '10%',
                    key: 'value'
                },
                {
                    title: this.$t('QuestionnaireEditor.OptionsUploadTitle'),
                    sortable: false,
                    key: 'title',
                    width: this.isCascading ? '45%' : '55%'
                },
                {
                    title: this.$t('QuestionnaireEditor.AttachmentName'),
                    sortable: false,
                    width: '15%',
                    key: 'attachmentName'
                }
            ];

            if (this.isCascading) {
                headers.push({
                    title: this.$t('QuestionnaireEditor.OptionsUploadParent'),
                    sortable: false,
                    width: '15%',
                    key: 'parentValue'
                });
            }

            headers.push({
                key: 'actions',
                sortable: false,
                width: '10%',
                align: 'end'
            });

            return headers;
        },

        formTitle() {
            return this.editedIndex === -1
                ? this.$t('QuestionnaireEditor.NewItem')
                : this.$t('QuestionnaireEditor.EditItem');
        },
        categoriesLocal: {
            get() {
                return this.categories;
            },
            set(value) {
                const categories = value;
                this.$emit('update-categories', categories);
            }
        }
    },
    methods: {
        reset() {
            this.search = null;
        },

        newRow() {
            this.editedIndex = -1;
            this.editedItem = {};
            this.dialog = true;
        },

        setCascading(value) {
            this.$emit('setCascading', value);
        },

        editItem(item) {
            this.editedIndex = this.categoriesLocal.indexOf(item);
            this.editedItem = Object.assign({}, item);
            this.dialog = true;
        },

        deleteItem(item) {
            const index = this.categoriesLocal.indexOf(item);
            if (confirm(this.$t('QuestionnaireEditor.DeleteItemCofirm'))) {
                var local = this.categoriesLocal.slice();
                local.splice(index, 1);
                this.categoriesLocal = local;
            }
        },

        close() {
            this.dialog = false;
            this.$nextTick(() => {
                this.editedItem = Object.assign({}, this.defaultItem);
                this.editedIndex = -1;
            });
        },

        save(item) {
            if (this.editedIndex > -1) {
                Object.assign(this.categories[this.editedIndex], item);
            } else {
                var local = this.categoriesLocal.slice();
                local.push(item);
                this.categoriesLocal = local;
                this.snacks.rowAdded = true;
            }

            this.close();
        },

        captionForParentValue(value) {
            const category = this.parentCategories.find(p => p.value == value);
            return category != null ? category.title : null;
        }
    }
};
</script>
