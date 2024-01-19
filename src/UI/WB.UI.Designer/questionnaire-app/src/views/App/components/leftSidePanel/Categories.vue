<template>
    <div class="categories">
        <div id="show-reload-details-promt" class="ng-cloak" v-show="shouldUserSeeReloadPromt">
            <div class="inner">{{ $t('QuestionnaireEditor.QuestionToUpdateOptions') }} <a href="#"
                    onclick="window.location.reload(true);">{{ $t('QuestionnaireEditor.QuestionClickReload') }}</a></div>
        </div>

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
                    <template v-for="(category, index) in categoriesList">
                        <CategoryItem :category-id="category.categoriesId" :questionnaire-id="questionnaireId">
                        </CategoryItem>
                    </template>
                </div>
            </form>
            <div class="button-holder">
                <p>
                    <span>{{ $t('QuestionnaireEditor.SideBarTranslationGetTemplate') }}</span>

                    <a class="btn btn-default" :href="downloadBaseUrl + '/template'" target="_blank" rel="noopener">{{
                        $t('QuestionnaireEditor.SideBarXlsx') }}
                    </a>
                    <a class="btn btn-default" :href="downloadBaseUrl + '/templateTab'" target="_blank" rel="noopener">{{
                        $t('QuestionnaireEditor.SideBarTab') }}
                    </a>
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarCategoriesAddNew')" value="ADD new category"
                        class="btn lighter-hover" @click.stop="addNewCategory()" :disabled="isReadOnlyForUser" />
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarCategoriesUploadNew')"
                        @click.stop="openFileDialog()" value="Upload new categories" class="btn lighter-hover" ngf-select
                        :disabled="isReadOnlyForUser" capture />

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

import CategoryItem from './CategoryItem.vue';

import { newGuid } from '../../../../helpers/guid';
import { isNull, isUndefined } from 'lodash'
import { updateCategories } from '../../../../services/categoriesService'
import moment from 'moment';


export default {
    name: 'Categories',
    inject: ['questionnaire'],
    components: { CategoryItem, },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            downloadBaseUrl: '/categories',
            file: [],
        }
    },
    computed: {
        shouldUserSeeReloadPromt() { return false; }, // TODO

        categoriesList() {
            return this.questionnaire.categories;
        },

        isReadOnlyForUser() {
            return this.questionnaire.isReadOnlyForUser;
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
                notificationService.notice(this.$t('QuestionnaireEditor.NoPermissions')); // TODO
                return;
            }

            var categories = { categoriesId: newGuid() };

            categories.file = file.file;

            categories.content = {};
            categories.content.size = file.size;
            categories.content.type = file.type;

            categories.meta = {};
            categories.meta.fileName = file.name;
            categories.meta.lastUpdated = moment();

            const suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedCategories && suspectedCategories.length > 0)
                categories.name = suspectedCategories[0];
            else
                categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

            const maxNameLength = 32;
            const fileNameLength = categories.name.length;
            categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
            categories.oldCategoriesId = categories.categoriesId;

            const response = await updateCategories(this.questionnaireId, categories)

            if (categories.file) notificationService.notice(response.data);

            categories.file = null;
            this.file = [];

            /*if (response.status !== 200) return;

            categories.checkpoint = categories.checkpoint || {};

            dataBind(categories.checkpoint, categories);
            $scope.categoriesList.push(categories);
            updateQuestionnaireCategories();

            setTimeout(function () {
                utilityService.focus("focusCategories" + categories.categoriesId);
            },
                500);*/
        },

        async addNewCategory() {
            if (this.isReadOnlyForUser) {
                notificationService.notice($i18next.t('NoPermissions'));
                return;
            }

            var categories = { categoriesId: newGuid() };

            await updateCategories(this.questionnaireId, categories)
            if (response.status !== 200) return;

            /*categories.checkpoint = categories.checkpoint || {};

            dataBind(categories.checkpoint, categories);
            $scope.categoriesList.push(categories);
            updateQuestionnaireCategories();

            setTimeout(function () {
                utilityService.focus("focusCategories" + categories.categoriesId);
            }, 500);*/
        }
    },
}
</script>
  