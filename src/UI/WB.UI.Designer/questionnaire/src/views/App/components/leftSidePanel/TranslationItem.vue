<template>
    <form name="translation.form">
        <div class="translations-panel-item"
            :class="{ 'has-error': hasPatternError, 'dragover': !translation.isOriginalTranslation && $refs.upload && $refs.upload.dropActive }"
            ngf-drop="" ngf-max-size="4MB" ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
            <a href="javascript:void(0);" @click="deleteTranslation($event)"
                v-if="!translation.isOriginalTranslation && !isReadOnlyForUser" class="btn delete-btn"
                tabindex="-1"></a>
            <div class="translation-content">
                <input focus-on-out="focusTranslation{{translation.translationId}}" required=""
                    :placeholder="$t('QuestionnaireEditor.SideBarTranslationName')" maxlength="32" spellcheck="false"
                    v-model="translation.name" name="name" class="form-control table-name" type="text" />
                <div class="drop-box">
                    {{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}
                </div>
                <div class="actions" :class="{ 'dirty': isDirty }">
                    <div v-show="isDirty" class="pull-left">
                        <button type="button" v-if="!isReadOnlyForUser" :disabled="isInvalid" class="btn lighter-hover"
                            @click.self="saveTranslation()">
                            {{ $t('QuestionnaireEditor.Save') }}
                        </button>
                        <button type="button" class="btn lighter-hover" @click.self="cancel()">{{
                            $t('QuestionnaireEditor.Cancel')
                            }}</button>
                    </div>

                    <span class="default-label" v-if="translation.isDefault">{{
                        $t('QuestionnaireEditor.Default')
                        }}</span>

                    <button type="button" class="btn btn-default" v-if="!isReadOnlyForUser"
                        v-show="translation.isDefault && !translation.isOriginalTranslation"
                        @click.self="setDefaultTranslation(false);">
                        {{ $t('QuestionnaireEditor.Reset') }}
                    </button>

                    <div class="permanent-actions pull-right">
                        <button type="button" class="btn lighter-hover" v-if="!isReadOnlyForUser"
                            @click.self="populateTranslation(true);">
                            {{ $t('QuestionnaireEditor.Populate') }}
                        </button>
                        <button type="button" class="btn lighter-hover" v-if="!isReadOnlyForUser"
                            v-show="!translation.isDefault" @click.self="setDefaultTranslation(true);">
                            {{ $t('QuestionnaireEditor.MarkAsDefault') }}
                        </button>

                        <a v-if="downloadUrl" :href="downloadUrl" class="btn btn-default" target="_blank"
                            rel="noopener noreferrer">{{
                                $t('QuestionnaireEditor.SideBarTranslationDownloadXlsx') }}</a>

                        <file-upload ref="upload" v-if="!isReadOnlyForUser"
                            :input-id="'tfu' + translation.translationId" v-model="file" @input-file="fileSelected"
                            :size="10 * 1024 * 1024" :drop="true" :drop-directory="false"
                            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,.txt,.tsv,.tab">
                        </file-upload>
                        <button v-show="!isDirty && !translation.isOriginalTranslation" v-if="!isReadOnlyForUser"
                            class="btn btn-default" @click.stop="openFileDialog()" type="button">
                            <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </form>
</template>

<script>

import { isUndefined, isNull, cloneDeep } from 'lodash'
import moment from 'moment'
import { notice } from '../../../../services/notificationService';
import { trimText, createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'
import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import {
    deleteTranslation,
    updateTranslation,
    setDefaultTranslation
} from '../../../../services/translationService';

import { updateQuestionnaireSettings } from '../../../../services/questionnaireService';

export default {
    name: 'TranslationItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        translationItem: { type: Object, required: true },
    },
    data() {
        return {
            downloadBaseUrl: '/translations',
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
        translation() {
            return this.translationItem.editTranslation;
        },
        questionnaire() {
            return this.questionnaireStore.getInfo;
        },
        isDirty() {
            return this.translation.name != this.translationItem.name || (this.translation.file !== null && this.translation.file !== undefined);
        },
        hasPatternError() {
            return (this.translation.name) ? false : true;
        },
        downloadUrl() {
            if (this.translation.isOriginalTranslation)
                return null;
            return this.downloadBaseUrl + '/' + this.questionnaireId + '/xlsx/' + this.translation.translationId;
        },
        isInvalid() {
            return (this.translation.name) ? false : true;
        },
    },
    methods: {
        fileSelected(file) {
            if (isUndefined(file) || isNull(file)) {
                return;
            }

            let translation = this.translation

            translation.file = file.file;

            translation.content = {};
            translation.content.size = file.size;
            translation.content.type = file.type;

            translation.meta = {};
            translation.meta.fileName = file.name;
            translation.meta.lastUpdated = moment();

            var maxNameLength = 32;

            var suspectedTranslations = translation.meta.fileName.match(/[^[\]]+(?=])/g);

            if (suspectedTranslations && suspectedTranslations.length > 0)
                translation.name = suspectedTranslations[0];
            else
                translation.name = translation.meta.fileName.replace(/\.[^/.]+$/, "");

            var fileNameLength = translation.name.length;
            translation.name = translation.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
        },

        async saveTranslation() {
            if (!this.translation.isOriginalTranslation) {
                const response = await updateTranslation(this.questionnaireId, this.translation)

                if (this.file.length > 0 && response != null)
                    notice(response);

                this.translation.file = null;
                this.file = [];
            }
            else {
                updateQuestionnaireSettings(this.questionnaireId, {
                    isPublic: this.questionnaire.isPublic,
                    title: this.questionnaire.title,
                    variable: this.questionnaire.variable,
                    hideIfDisabled: this.questionnaire.hideIfDisabled,
                    defaultLanguageName: this.translation.name
                });
            }
        },

        cancel() {
            var clonned = cloneDeep(this.translationItem);
            clonned.editTranslation = null;
            this.translationItem.editTranslation = clonned;
        },

        deleteTranslation(event) {
            event.preventDefault();

            var translationName = this.translation.name || this.$t('QuestionnaireEditor.SideBarTranslationNoName');

            var trimmedTranslationName = trimText(translationName);
            var confirmParams = createQuestionForDeleteConfirmationPopup(trimmedTranslationName)

            confirmParams.callback = confirm => {
                if (confirm) {
                    deleteTranslation(this.questionnaireId, this.translation.translationId)
                }
            };

            this.$confirm(confirmParams);
        },

        async setDefaultTranslation(isDefault) {
            await setDefaultTranslation(this.questionnaireId, isDefault ? this.translation.translationId : null)
        },

        async populateTranslation(isDefault) {
            await setDefaultTranslation(this.questionnaireId, isDefault ? this.translation.translationId : null)
            location.reload();
        },

        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
    }
}
</script>
