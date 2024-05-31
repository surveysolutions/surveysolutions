<template>
    <div id="verification-modal" v-if="visible" :class="typeOfMessageToBeShown">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" @click="close()" aria-hidden="true"></button>
                <h3 class="modal-title">
                    <span>{{ $t('QuestionnaireEditor.CompilationLabel') }}</span>
                    <span>&nbsp;</span>
                    <span v-if="typeOfMessageToBeShown === 'error'">{{
        $t('QuestionnaireEditor.CompilationErrorsCounter', {
            count: messagesToShow.length
        }) }}</span>
                    <span v-if="typeOfMessageToBeShown === 'warning'">{{
        $t('QuestionnaireEditor.CompilationWarningsCounter', {
            count: messagesToShow.length
        }) }}</span>
                </h3>
            </div>
            <div class="modal-body">
                <perfect-scrollbar class="scroller">
                    <div id="verify-popover-content">
                        <div class="question-list" ref="errorsList">
                            <div v-for="error in messagesToShow">
                                <div class="error-message">
                                    <span class="error-code">[{{ error.code }}]:</span>{{ error.message }}
                                </div>
                                <ul class="verification-items" :class="{
        singleError: !error.isGroupedMessage
    }">
                                    <template v-for="referencesWithErrors in error.errors">
                                        <li class="verification-item-container"
                                            v-for="reference in referencesWithErrors.references"
                                            @click="navigateTo(reference)">
                                            <a class="verification-item" href="javascript:void(0);"
                                                data-bs-placement="right" data-bs-toggle="tooltip" data-bs-html="true"
                                                :title="referencesWithErrors.compilationErrorMessages != null ? referencesWithErrors.compilationErrorMessages.slice(0, 10).join('<br />') + (referencesWithErrors.compilationErrorMessages.length > 10 ? '<br />...' : '') : ''"
                                                data-bs-custom-class="error-tooltip in" data-bs-container='body'>
                                                <span v-if="reference.type == 'Question'" class="icon"
                                                    :class="[reference.questionType, 'icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Questionnaire'"
                                                    class="icon icon-questionnaire"
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Macro'" class="icon icon-macro"
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'LookupTable'" class="icon icon-lookup "
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'StaticText'"
                                                    class="icon icon-statictext "
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Attachment'"
                                                    class="icon icon-attachment "
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Variable'" class="icon icon-variable"
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Translation'"
                                                    class="icon icon-translation "
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span v-if="reference.type == 'Categories'"
                                                    class="icon icon-categories "
                                                    :class="['icon-' + typeOfMessageToBeShown]"></span>
                                                <span class="title" v-sanitize-html="reference.title"></span>
                                                <span class="variable"
                                                    v-dompurify-html="reference.variable || '&nbsp;'">&nbsp;</span>
                                            </a>
                                        </li>
                                    </template>
                                </ul>
                            </div>
                        </div>
                    </div>
                </perfect-scrollbar>
            </div>
            <div class="modal-footer">
                <button class="btn btn-link" id="verificationRecompile" @click="verify()">
                    {{ $t('QuestionnaireEditor.Recompile') }}
                </button>
                <button class="btn btn-link" data-dismiss="modal" @click="close()">
                    {{ $t('QuestionnaireEditor.Close') }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>
import { useVerificationStore } from '../../../stores/verification';
import * as bootstrap from 'bootstrap';

export default {
    name: 'VerificationDialog',
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            messagesToShow: [],
            visible: false,
            typeOfMessageToBeShown: 'error',
            tooltipList: [],
        };
    },
    setup(props) {
        const verificationStore = useVerificationStore();

        return {
            verificationStore
        };
    },
    expose: ['openErrors', 'openWarnings', 'close'],
    computed: {
        messagesToShow() {
            if (this.typeOfMessageToBeShown === 'error')
                return this.verificationStore.status.errors;
            return this.verificationStore.status.warnings;
        }
    },
    methods: {
        openErrors() {
            this.typeOfMessageToBeShown = 'error';
            this.visible = true;
            this.initTooltips();
        },
        openWarnings() {
            this.typeOfMessageToBeShown = 'warning';
            this.visible = true;
            this.initTooltips();
        },
        close() {
            this.disposeTooltips();
            this.visible = false;
        },
        async verify() {
            await this.verificationStore.fetchVerificationStatus(
                this.questionnaireId
            );

            const errorsCount = this.verificationStore.status.errors.length;
            if (errorsCount == 0) {
                this.close()
            }
            else (
                this.initTooltips()
            )
        },
        navigateTo(reference) {
            if (reference.type.toLowerCase() === "questionnaire") {
                this.close();
                this.$emitter.emit("showShareInfo", {});
                //this.showShareInfo(); 
            } else if (reference.type.toLowerCase() === "macro") {
                this.close();
                this.$emitter.emit("openMacrosList", { focusOn: reference.itemId });
            } else if (reference.type.toLowerCase() === "lookuptable") {
                this.close();
                this.$emitter.emit("openLookupTables", { focusOn: reference.itemId });
            } else if (reference.type.toLowerCase() === "attachment") {
                this.close();
                this.$emitter.emit("openAttachments", { focusOn: reference.itemId });
            } else if (reference.type.toLowerCase() === "translation") {
                this.close();
                this.$emitter.emit("openTranslations", { focusOn: reference.itemId });
            } else if (reference.type.toLowerCase() === "categories") {
                this.close();
                this.$emitter.emit("openCategoriesList", { focusOn: reference.itemId });
            } else if (reference.type.toLowerCase() === "criticalrule") {
                this.close();
                this.$emitter.emit("openCriticalRules", { focusOn: reference.itemId });
            } else {
                const name = reference.type.toLowerCase();
                this.$router.push({
                    name: name,
                    params: {
                        chapterId: reference.chapterId,
                        entityId: reference.itemId,
                    },
                    force: true,
                    state: {
                        indexOfEntityInProperty: reference.indexOfEntityInProperty,
                        property: reference.property
                    }
                });
            }
        },
        initTooltips() {
            this.$nextTick(() => {
                this.disposeTooltips();

                const errorsList = this.$refs.errorsList;
                const tooltipTriggerList = errorsList.querySelectorAll('[data-bs-toggle="tooltip"]')
                this.tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl/*, {
                    customClass: 'right in',
                    placement: 'right'
                }*/))
            })
        },
        disposeTooltips() {
            [...this.tooltipList].map(tooltip => tooltip.dispose());
            this.tooltipList = [];
        },
    }
};
</script>
