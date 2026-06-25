<template>
    <div class="categories">
        <options-editor-modal v-if="modalEverOpened" ref="categoriesEditorModal" />

        <perfect-scrollbar class="scroller">
            <h3>{{ $t('QuestionnaireEditor.SideBarCategoriesCounter', { count: categoriesList.length }) }}</h3>

            <div class="empty-list" v-show="categoriesList.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarCategoriesEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarCategoriesEmptyLine2') }}</p>
                <p>
                    <span class="variable-name">{{ $t('QuestionnaireEditor.VariableName') }}</span>
                    {{ $t('QuestionnaireEditor.SideBarCategoriesEmptyLine3') }}
                </p>
            </div>
            <form role="form" name="categoriesForm" novalidate>
                <div class="categories-list">
                    <CategoriesItem v-for="(categories, index) in categoriesList" refs="categoriesItems"
                        @editCategoriesOpen="editCategoriesOpen" :categoriesItem="categories"
                        :questionnaire-id="questionnaireId" />
                </div>
            </form>
            <div class="button-holder">
                <p>
                    <span>{{ $t('QuestionnaireEditor.SideBarTranslationGetTemplate') }}</span>

                    <a class="btn btn-default" :href="downloadBaseUrl + '/template'" target="_blank" rel="noopener">{{
                        $t('QuestionnaireEditor.SideBarXlsx') }}
                    </a>
                    <a class="btn btn-default" :href="downloadBaseUrl + '/templateTab'" target="_blank"
                        rel="noopener">{{
                            $t('QuestionnaireEditor.SideBarTab') }}
                    </a>
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarCategoriesAddNew')"
                        value="ADD new category" class="btn lighter-hover" @click.stop="addNewCategory()"
                        v-if="!isReadOnlyForUser" />
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarCategoriesUploadNew')"
                        @click.stop="openFileDialog()" value="Upload new categories" class="btn lighter-hover"
                        v-if="!isReadOnlyForUser" capture />

                    <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'cfunew'" v-model="file"
                        :size="10 * 1024 * 1024" :drop="false" :drop-directory="false" @input-file="createAndUploadFile"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab">
                    </file-upload>
                </p>
            </div>
        </perfect-scrollbar>
    </div>
</template>

<script>
import { defineAsyncComponent } from 'vue';

import CategoriesItem from './CategoriesItem.vue';
import { newGuid } from '../../../../helpers/guid';
import { isNull, isUndefined } from 'lodash'
import { updateCategories } from '../../../../services/categoriesService'
import { notice } from '../../../../services/notificationService';
import dayjs from 'dayjs';

const loadOptionsEditorModal = () => import('./CategoriesEditorModal.vue');
const OptionsEditorModal = defineAsyncComponent(loadOptionsEditorModal);

export default {
    name: 'Categories',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: { CategoriesItem, OptionsEditorModal },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            downloadBaseUrl: '/categories',
            file: [],
            modalEverOpened: false,
        }
    },
    computed: {
        categoriesList() {
            return this.questionnaire.categories;
        },
    },
    methods: {

        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },

        async createAndUploadFile(file) {
            if (isNull(file) || isUndefined(file)) {
                return;
            }

            if (this.isReadOnlyForUser) {
                notice(this.$t('QuestionnaireEditor.NoPermissions'));
                return;
            }

            var categories = { categoriesId: newGuid() };

            categories.file = file.file;

            categories.content = {};
            categories.content.size = file.size;
            categories.content.type = file.type;

            categories.meta = {};
            categories.meta.fileName = file.name;
            categories.meta.lastUpdated = dayjs();

            const suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedCategories && suspectedCategories.length > 0)
                categories.name = suspectedCategories[0];
            else
                categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

            const maxNameLength = 32;
            const fileNameLength = categories.name.length;
            categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);

            var response = await updateCategories(this.questionnaireId, categories, true);

            if (this.file.length > 0 && response != null)
                notice(response);

            this.file = [];
        },

        async addNewCategory() {
            const categories = {
                categoriesId: newGuid()
            };

            await updateCategories(this.questionnaireId, categories)
        },

        async openCategoriesEditorModal(questionnaireId, categoriesId) {
            await loadOptionsEditorModal();
            await this.$nextTick();

            this.$refs.categoriesEditorModal?.open(questionnaireId, categoriesId, { isCategory: true });
        },

        async editCategoriesOpen(event) {
            this.modalEverOpened = true;
            await this.$nextTick();
            await this.openCategoriesEditorModal(this.questionnaireId, event.categoriesId);
        }
    },
}
</script>