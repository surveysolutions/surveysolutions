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
                    <a class="btn btn-default" :href="downloadBaseUrl + '/templateTab'" target="_blank" rel="noopener">{{
                        $t('QuestionnaireEditor.SideBarTab') }}
                    </a>
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarCategoriesAddNew')" value="ADD new category"
                        class="btn lighter-hover" @click.stop="addNewCategory()" v-if="!isReadOnlyForUser" />
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

import CategoriesItem from './CategoriesItem.vue';

import { newGuid } from '../../../../helpers/guid';
import { isNull, isUndefined, some } from 'lodash'
import { updateCategories } from '../../../../services/categoriesService'
import { notice } from '../../../../services/notificationService';
import moment from 'moment';

export default {
    name: 'Categories',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: { CategoriesItem, },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            shouldUserSeeReloadPromt: false,
            openEditor: null,
            bcChannel: null,

            downloadBaseUrl: '/categories',
            file: [],
        }
    },
    mounted() {
        // https://developer.mozilla.org/en-US/docs/Web/API/Broadcast_Channel_API
        // Automatically reload window on popup close. If supported by browser
        if ('BroadcastChannel' in window) {
            this.bcChannel = new BroadcastChannel("editcategory")
            this.bcChannel.onmessage = ev => {
                console.log(ev.data)
                if (ev.data === 'close#' + this.openEditor) {
                    window.location.reload();
                }
            }
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
            categories.meta.lastUpdated = moment();

            const suspectedCategories = categories.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedCategories && suspectedCategories.length > 0)
                categories.name = suspectedCategories[0];
            else
                categories.name = categories.meta.fileName.replace(/\.[^/.]+$/, "");

            const maxNameLength = 32;
            const fileNameLength = categories.name.length;
            categories.name = categories.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);

            var response = await updateCategories(this.questionnaireId, categories, true);

            if (this.file.length > 0)
                notice(response);

            this.file = [];
        },

        async addNewCategory() {
            const categories = {
                categoriesId: newGuid()
            };

            await updateCategories(this.questionnaireId, categories)
        },

        editCategoriesOpen(event) {
            this.shouldUserSeeReloadPromt = true;
            this.openEditor = event.categoriesId

            window.open("/questionnaire/editcategories/" + this.questionnaireId + "?categoriesid=" + event.categoriesId,
                "", "scrollbars=yes, center=yes, modal=yes, width=960, height=745, top=" + (screen.height - 745) / 4
                + ", left= " + (screen.width - 960) / 2, true);
        }
    },
}
</script>
  