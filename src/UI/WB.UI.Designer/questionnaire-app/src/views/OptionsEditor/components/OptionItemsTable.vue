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
            <v-btn class="ma-2" color="primary" @click="newRow">
                <v-icon left>mdi-plus</v-icon
                >{{ $t('QuestionnaireEditor.NewItem') }}
            </v-btn>
        </v-card-title>

        <category-dialog
            v-if="dialog"
            :title="formTitle"
            :item="editedItem"
            :shown="dialog"
            :show-parent-value="showParentValue"
            @cancel="dialog = false"
            @change="save"
        />

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
            <!-- <template #item.parentValue="{ item }">
                {{ item.parentValue }}
                <span
                    class="caption text--disabled .d-none .d-md-flex .d-lg-none"
                    >{{
                        captionForParentValue(
                            item.parentValue
                        )
                    }}</span
                >
            </template> -->
            <template v-slot:item.title="props">
                <v-edit-dialog :return-value.sync="props.item.title" large>
                    <div>{{ props.item.title }}</div>
                    <template v-slot:input>
                        <v-text-field
                            v-model="props.item.title"
                            :rules="[required]"
                            single-line
                            counter
                            autofocus
                        ></v-text-field>
                    </template>
                </v-edit-dialog>
            </template>

            <template v-slot:item.value="props">
                <v-edit-dialog :return-value.sync="props.item.value" large>
                    <div>{{ props.item.value }}</div>
                    <template v-slot:input>
                        <v-text-field
                            v-model="props.item.value"
                            :rules="[required, maxValue]"
                            single-line
                            counter
                            autofocus
                            type="number"
                        ></v-text-field>
                    </template>
                </v-edit-dialog>
            </template>

            <template v-slot:item.parentValue="props">
                <v-edit-dialog
                    :return-value.sync="props.item.parentValue"
                    large
                >
                    <div>
                        {{ props.item.parentValue }}
                    </div>
                    <template v-slot:input>
                        <v-text-field
                            v-model="props.item.parentValue"
                            @
                            :rules="[required, maxValue]"
                            single-line
                            counter
                            type="number"
                            autofocus
                        ></v-text-field>
                    </template>
                </v-edit-dialog>
            </template>
            <template #item.actions="{ item }">
                <div>
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
        showParentValue: { type: Boolean, required: true },
        loading: { type: Boolean, required: true }
    },

    data() {
        return {
            search: null,
            editedItem: null,
            editedIndex: null,
            dialog: false,
            required: value => !!value || 'Required.',
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
                    sortable: true,
                    width: '10%',
                    value: 'value'
                },
                {
                    text: this.$t('QuestionnaireEditor.OptionsUploadTitle'),
                    sortable: true,
                    value: 'title',
                    width: this.showParentValue ? '60%' : '70%'
                }
            ];

            if (this.showParentValue) {
                headers.push({
                    text: this.$t('QuestionnaireEditor.OptionsUploadParent'),
                    sortable: true,
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
        newRow() {
            this.editedIndex = -1;
            this.editedItem = {};
            this.dialog = true;
        },

        editItem(item) {
            console.log('editItem', Object.assign({}, item));
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
                this.search = item.title;
            }

            this.close();
        }
    }
};
</script>
