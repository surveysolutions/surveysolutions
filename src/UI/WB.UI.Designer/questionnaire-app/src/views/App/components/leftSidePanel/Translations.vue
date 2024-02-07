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
                    <TranslationItem :translation="defaultTranslation" :questionnaire-id="questionnaireId">
                    </TranslationItem>

                    <template v-for="(translation, index) in translations">
                        <TranslationItem :translation="translation" :questionnaire-id="questionnaireId">
                        </TranslationItem>
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
                    <!--input type="button" :value="$t('QuestionnaireEditor.SideBarTranslationsUploadNew')"
                        value="Upload new translation" :disabled="isReadOnlyForUser" class="btn lighter-hover" ngf-select
                        ngf-change="createAndUploadFile($file);$event.stopPropagation()" ngf-max-size="10MB"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                        ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser" /-->

                    <input type="button" :value="$t('QuestionnaireEditor.SideBarTranslationsUploadNew')"
                        @click.stop="openFileDialog()" value="Upload new categories" class="btn lighter-hover" ngf-select
                        :disabled="isReadOnlyForUser" capture />

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

import { computed } from 'vue';
import moment from 'moment'
import { isUndefined, isNull, some } from 'lodash'
import TranslationItem from './TranslationItem.vue';

import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import { notice } from '../../../../services/notificationService';
import { newGuid } from '../../../../helpers/guid';
import { updateTranslation } from '../../../../services/translationService';

export default {
    name: 'Translations',
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
        questionnaire() {
            return this.questionnaireStore.getInfo;
        },

        translations() {
            return this.questionnaireStore.getEdittingTranslations;
        },

        isReadOnlyForUser() {
            return this.questionnaire.isReadOnlyForUser;
        },

        defaultTranslation() {
            return {
                translationId: null,
                name: !this.questionnaire.defaultLanguageName ? this.$t("QuestionnaireEditor.Translation_Original") : this.questionnaire.defaultLanguageName,
                file: null,
                isDefault: !some(this.translations, { isDefault: true }),
                content: { details: {} },
                isOriginalTranslation: true
            }
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

            let translation = {};
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
            translation.oldTranslationId = null;
            translation.translationId = newGuid();

            const response = await updateTranslation(this.questionnaireId, translation);

            if (translation.file) notice(response);
            translation.file = null;
            this.file = [];

            //setTimeout(function () { utilityService.focus("focusTranslation" + translation.translationId); }, 500);
        },
    },
}
</script>
  