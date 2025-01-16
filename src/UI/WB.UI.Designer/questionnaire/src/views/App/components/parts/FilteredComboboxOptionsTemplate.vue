<template>
    <a href="javascript:void(0);" class="btn btn-link upload-categories-button" @click="editFilteredComboboxOptions()">{{
        $t('QuestionnaireEditor.QuestionUploadOptions') }}
    </a>
</template>

<script>

import { useQuestionStore } from '../../../../stores/question'

export default {
    name: 'FilteredComboboxOptions',
    inject: ['questionnaire', 'currentChapter', 'openExternalEditor'],
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
        }
    },
    setup() {
        const questionStore = useQuestionStore();

        return {
            questionStore
        };
    },
    methods: {
        editFilteredComboboxOptions() {
            if (this.questionStore.getIsDirty) {
                const params = {
                    title: this.$t('QuestionnaireEditor.QuestionOpenEditorConfirm'),
                    okButtonTitle: this.$t('QuestionnaireEditor.Save'),
                    cancelButtonTitle: this.$t('QuestionnaireEditor.Cancel'),
                    isReadOnly: this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly,
                    callback: async confirm => {
                        if (confirm) {
                            await this.questionStore.saveQuestionData(this.questionnaireId, this.activeQuestion);

                            const alertParams = {
                                title: this.$t('QuestionnaireEditor.QuestionOpenEditorSaved'),
                                okButtonTitle: this.$t('QuestionnaireEditor.Ok'),
                                isReadOnly: this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly,
                                isAlert: true,
                                callback: async alertConfirm => {
                                    this.openOptionsEditor();
                                }
                            }

                            this.$confirm(alertParams);
                        }
                    }
                };

                this.$confirm(params);
            } else {
                this.openOptionsEditor();
            }
        },

        openOptionsEditor() {
            this.openExternalEditor(this.activeQuestion.id, "/questionnaire/editoptions/" + this.questionnaireId + "?questionid=" + this.activeQuestion.id)
        },
    }
}
</script>
