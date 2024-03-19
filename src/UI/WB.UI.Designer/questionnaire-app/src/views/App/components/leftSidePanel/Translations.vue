<template>
    <div class="translations">
        <perfect-scrollbar class="scroller">
            <h3>{{ $t('QuestionnaireEditor.SideBarTranslationsCounter', { count: translations.length + 1 }) }}</h3>

            <div class="empty-list" v-show="translations.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarTranslationsEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarTranslationsEmptyLine2') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarTranslationsEmptyLine3') }}</p>
            </div>
            <form role="form" name="translationsForm" novalidate>
                <div class="translation-list">
                    <TranslationItem :translationItem="defaultTranslation" :questionnaire-id="questionnaireId">
                    </TranslationItem>

                    <template v-for="(translation, index) in translations">
                        <TranslationItem :translationItem="translation" :questionnaire-id="questionnaireId" />
                    </template>
                </div>
            </form>
            <div class="button-holder">
                <p>
                    <span>{{ $t('QuestionnaireEditor.SideBarTranslationGetTemplate') }}</span>
                    <a class="btn btn-default" :href="downloadBaseUrl + '/' + questionnaire.questionnaireId + '/template'"
                        target="_blank" rel="noopener">
                        {{ $t('QuestionnaireEditor.SideBarTranslationGetTemplateLinkTextXlsx') }}
                    </a>
                </p>
                <p>
                    <input type="button" :value="$t('QuestionnaireEditor.SideBarTranslationsUploadNew')"
                        @click.stop="openFileDialog()" value="Upload new categories" class="btn lighter-hover" ngf-select
                        v-if="!isReadOnlyForUser" capture />

                    <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'" v-model="file"
                        :size="10 * 1024 * 1024" :drop="false" :drop-directory="false" @input-file="createAndUploadFile"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab">
                    </file-upload>
                </p>
            </div>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import { reactive } from 'vue';
import moment from 'moment'
import { isUndefined, isNull, some, cloneDeep } from 'lodash'
import TranslationItem from './TranslationItem.vue';

import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import { notice } from '../../../../services/notificationService';
import { updateTranslation } from '../../../../services/translationService';
import { newGuid } from '../../../../helpers/guid';

export default {
    name: 'Translations',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: {
        TranslationItem,
    },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {
            downloadBaseUrl: '/translations',
            dirty: false,
            file: [],
        }
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    computed: {
        translations() {
            return this.questionnaireStore.getInfo.translations;
        },

        defaultTranslation() {
            var translation = {
                translationId: null,
                name: !this.questionnaire.defaultLanguageName ? this.$t("QuestionnaireEditor.Translation_Original") : this.questionnaire.defaultLanguageName,
                file: null,
                isDefault: !some(this.translations, { isDefault: true }),
                content: { details: {} },
                isOriginalTranslation: true,
            };

            var editTranslation = cloneDeep(translation);
            translation.editTranslation = editTranslation;

            return reactive(translation);
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

            let translation = { translationId: newGuid() };
            translation.file = file.file;

            translation.content = {};
            translation.content.size = file.size;
            translation.content.type = file.type;

            translation.meta = {};
            translation.meta.fileName = file.name;
            translation.meta.lastUpdated = moment();

            const suspectedTranslations = translation.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedTranslations && suspectedTranslations.length > 0)
                translation.name = suspectedTranslations[0];
            else
                translation.name = translation.meta.fileName.replace(/\.[^/.]+$/, "");

            const maxNameLength = 32;
            const fileNameLength = translation.name.length;
            translation.name = translation.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);

            const response = await updateTranslation(this.questionnaireId, translation, true);

            if (this.file)
                notice(response);

            translation.file = null;
            this.file = [];

        },
    },
}
</script>
  