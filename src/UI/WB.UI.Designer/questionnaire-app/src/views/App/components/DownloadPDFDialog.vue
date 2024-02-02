<template>
    <teleport to="body">
        <div v-if="visible" uib-modal-window="modal-window" class="modal share-window fade ng-scope ng-isolate-scope in"
            role="dialog" index="0" animate="animate" tabindex="-1" uib-modal-animation-class="fade" modal-in-class="in"
            modal-animation="true" style="z-index: 1050; display: block;">
            <div class="modal-dialog ">
                <div class="modal-content" uib-modal-transclude="">
                    <div class="modal-header blue-strip">
                        <button type="button" class="close" aria-hidden="true" @click="cancel()"></button>
                        <h1>{{ $t('QuestionnaireEditor.DownloadPdf') }}</h1>
                    </div>
                    <div class="modal-body blue-strip">
                        <div class="start-pdf-generation" id="startPdf">
                            <p>{{ $t('QuestionnaireEditor.ChooseLanguageTitle') }}</p>

                            <div class="btn-group type-container-dropdown dropdown">
                                <button class="btn btn-default form-control ng-binding dropdown-with-fixed-width"
                                    id="selectedTranslation" data-bs-toggle="dropdown" type="button">
                                    {{ selectedTranslation.name }}
                                    <span class="dropdown-arrow"></span>
                                </button>
                                <ul class="dropdown-menu" role="menu" aria-labelledby="dropdownMenu10">
                                    <li role="presentation" v-for="translation in translations">
                                        <a role="menuitem" tabindex="-1" href="javascript:void(0);"
                                            @click="selectTranslation(translation)">
                                            {{ translation.name }}
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <p></p>
                        <p>
                        <pre v-if="isGenerating">{{ generateStatusMessage }}</pre>
                        </p>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-lg btn-primary" @click="generate()" :disabled="isGenerating"
                            v-if="!canRetryGenerate">
                            {{ $t('QuestionnaireEditor.GeneratePdf') }}
                        </button>
                        <button type="button" class="btn btn-lg btn-primary" @click="retryGenerate()"
                            v-if="canRetryGenerate">
                            {{ $t('QuestionnaireEditor.RetryExportPDF') }}
                        </button>
                        <button type="button" class="btn btn-lg pull-right" @click="cancel()">
                            {{ $t('QuestionnaireEditor.Cancel') }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </teleport>
</template>

<script>
import _ from 'lodash';
import Help from './Help.vue';
import { retryExportPdf, updateExportPdfStatus } from '../../../services/pdfService'

export default {
    name: 'DownloadPDFDialog',
    components: {
        Help
    },
    inject: ['questionnaire', 'currentUser'],
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            isGenerating: false,
            canRetryGenerate: false,
            generateStatusMessage: '',

            selectedTranslation: {},
            generateTimerId: null,
            visible: false,
        };
    },
    computed: {
        translations() {
            const translationsView = _.map(this.questionnaire.translations, _.clone);
            translationsView.splice(0, 0, {
                translationId: null,
                name: !this.questionnaire.defaultLanguageName ? this.$t("QuestionnaireEditor.Translation_Original") : this.questionnaire.defaultLanguageName
            });

            return translationsView;
        }
    },
    setup(props) { },
    expose: ['open', 'close'],
    methods: {
        open() {
            this.selectedTranslation = this.translations[0];
            this.generateStatusMessage = '';

            this.isGenerating = false;
            this.visible = true;
        },
        close() {
            this.visible = false;
        },
        selectTranslation(translation) {
            this.selectedTranslation = translation;
        },
        generate() {
            this.generateStatusMessage = this.$t("QuestionnaireEditor.Initializing") + '...';
            this.isGenerating = true;

            this.updateExportPdfStatus(this.selectedTranslation.translationId);
        },
        retryGenerate() {
            var translationId = this.selectedTranslation.translationId;

            retryExportPdf(this.questionnaireId, translationId);
            this.generate();
            this.canRetryGenerate = false;
        },

        updateExportPdfStatus(translationId) {
            var request = updateExportPdfStatus(this.questionnaireId, translationId);
            self = this;

            request.then(function (result) {
                self.onExportPdfStatusReceived(result, translationId);
            });
        },
        onExportPdfStatusReceived(data, translationId) {
            self = this;
            if (data.message !== null)
                this.generateStatusMessage = data.message;
            else
                this.generateStatusMessage =
                    "Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.";

            this.canDownload = data.readyForDownload;
            this.canRetryGenerate = data.canRetry;

            if (!this.canDownload) {
                if (this.canRetryGenerate) {
                    clearTimeout(this.generateTimerId);
                    this.isGenerating = false;
                } else {
                    this.generateTimerId = setTimeout(function () {
                        self.updateExportPdfStatus(translationId);
                    },
                        1500);
                }
            }
            else {
                this.cancel();
                window.location = '/pdf/downloadPdf/' + this.questionnaireId + '?translation=' + translationId;
            }
        },
        cancel() {
            clearTimeout(this.generateTimerId);
            this.generateStatusMessage = '';
            this.close();
        }
    }
};
</script>
