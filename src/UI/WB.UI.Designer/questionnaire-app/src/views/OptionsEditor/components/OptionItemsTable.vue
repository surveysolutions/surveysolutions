<template>
    <div>
        <v-card-title>
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
            @change="save"
        />

        <v-snackbar v-model="snacks.rowAdded" top color="success">{{
            $t('QuestionnaireEditor.RowAdded')
        }}</v-snackbar>

        <v-data-table
            ref="table"
            :headers="headers"
            :items="categories"
            :search="search"
            :items-per-page="10"
            :footer-props="{
                'items-per-page-options': [10, 20, 50, 100]
            }"
            :loading="loading"
            class="table-striped elevation-1 mb-14"
            style="overflow-wrap:anywhere;"
            dense
        >
            <template #item.value="{ item }">
                <span class="text-no-wrap">{{ item.value }}</span>
            </template>
            <template v-slot:item.parentValue="props">
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
            <template #item.actions="{ item }">
                <div v-if="!readonly">
                    <v-icon small class="mr-2" @click="editItem(item)"
                        >mdi-pencil</v-icon
                    >
                    <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
                </div>
            </template>
            <template v-slot:no-data>
                {{
                    $t('QuestionnaireEditor.OptionsUploadLimit', {
                        limit: 15000
                    })
                }}
            </template>
        </v-data-table>
    </div>
</template>

<script>
import CategoryDialog from './OptionItemDialog';

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
                    text: this.$t('QuestionnaireEditor.OptionsUploadValue'),
                    sortable: false,
                    width: '10%',
                    value: 'value'
                },
                {
                    text: this.$t('QuestionnaireEditor.OptionsUploadTitle'),
                    sortable: false,
                    value: 'title',
                    width: this.isCascading ? '60%' : '70%'
                }
            ];

            if (this.isCascading) {
                headers.push({
                    text: this.$t('QuestionnaireEditor.OptionsUploadParent'),
                    sortable: false,
                    width: '15%',
                    value: 'parentValue'
                });
            }

            headers.push({
                value: 'actions',
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
            this.editedIndex = this.categories.indexOf(item);
            this.editedItem = Object.assign({}, item);
            this.dialog = true;
        },

        deleteItem(item) {
            const index = this.categories.indexOf(item);
            if (confirm(this.$t('QuestionnaireEditor.DeleteItemCofirm'))) {
                this.categories.splice(index, 1);
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
                this.categories.push(item);
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

<style css scoped>
::v-deep tbody tr:nth-of-type(odd) {
    background-color: rgba(0, 0, 0, 0.03);
}
</style>
