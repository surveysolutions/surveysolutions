<template>
    <div id="verification-modal" v-if="visible" :class="typeOfMessageToBeShown">
        <div class="modal-content">
            <div class="modal-header">
                <button
                    type="button"
                    class="close"
                    @click="close()"
                    aria-hidden="true"
                ></button>
                <h3 class="modal-title">
                    <span
                        v-t="{ path: 'QuestionnaireEditor.CompilationLabel' }"
                    ></span>

                    <span
                        v-if="typeOfMessageToBeShown === 'error'"
                        v-t="{
                            path:
                                'QuestionnaireEditor.CompilationErrorsCounter',
                            args: {
                                count: messagesToShow.length
                            }
                        }"
                    ></span>
                    <span
                        v-if="typeOfMessageToBeShown === 'warning'"
                        v-t="{
                            path:
                                'QuestionnaireEditor.CompilationWarningsCounter',
                            args: {
                                count: messagesToShow.length
                            }
                        }"
                    ></span>
                </h3>
            </div>
            <div class="modal-body">
                <perfect-scrollbar class="scroller">
                    <div id="verify-popover-content">
                        <div class="question-list">
                            <div v-for="error in messagesToShow">
                                <div class="error-message">
                                    <span class="error-code"
                                        >[{{ error.code }}]:</span
                                    >{{ error.message }}
                                </div>
                                <ul
                                    class="verification-items"
                                    :class="{
                                        singleError: !error.isGroupedMessage
                                    }"
                                >
                                    <empty-tag
                                        v-for="referencesWithErrors in error.errors"
                                    >
                                        <li
                                            class="verification-item-container"
                                            v-for="reference in referencesWithErrors.references"
                                            @click="navigateTo(reference)"
                                        >
                                            <a
                                                class="verification-item"
                                                href="javascript:void(0);"
                                                tooltip-class="error-tooltip"
                                                tooltip-append-to-body="true"
                                                tooltip-placement="right"
                                                uib-tooltip-html="referencesWithErrors.compilationErrorMessages != null ? referencesWithErrors.compilationErrorMessages.slice(0, 10).join('<br />') + (referencesWithErrors.compilationErrorMessages.length > 10 ? '<br />...' : '') : ''"
                                            >
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Question'
                                                    "
                                                    class="icon"
                                                    :class="[
                                                        reference.questionType,
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Questionnaire'
                                                    "
                                                    class="icon icon-questionnaire"
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Macro'
                                                    "
                                                    class="icon icon-macro"
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'LookupTable'
                                                    "
                                                    class="icon icon-lookup "
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'StaticText'
                                                    "
                                                    class="icon icon-statictext "
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Attachment'
                                                    "
                                                    class="icon icon-attachment "
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Variable'
                                                    "
                                                    class="icon icon-variable"
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Translation'
                                                    "
                                                    class="icon icon-translation "
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span
                                                    v-if="
                                                        reference.type ==
                                                            'Categories'
                                                    "
                                                    class="icon icon-categories "
                                                    :class="[
                                                        'icon-' +
                                                            typeOfMessageToBeShown
                                                    ]"
                                                ></span>
                                                <span class="title">
                                                    {{ reference.title }}
                                                </span>
                                                <span
                                                    class="variable"
                                                    v-bind-html="
                                                        reference.variable ||
                                                            '&nbsp;'
                                                    "
                                                    >&nbsp;</span
                                                >
                                            </a>
                                        </li>
                                    </empty-tag>
                                </ul>
                            </div>
                        </div>
                    </div>
                </perfect-scrollbar>
            </div>
            <div class="modal-footer">
                <button class="btn btn-link" @click="verify" v-i18next>
                    {{ $t('QuestionnaireEditor.Recompile') }}
                </button>
                <button
                    class="btn btn-link"
                    data-dismiss="modal"
                    @click="close"
                    v-i18next
                >
                    {{ $t('QuestionnaireEditor.Close') }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>
import { useVerificationStore } from '../../../stores/verification';

export default {
    name: 'VerificationDialog',
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            messagesToShow: [],
            visible: false,
            typeOfMessageToBeShown: 'error'
        };
    },
    setup(props) {
        const verificationStore = useVerificationStore();

        return {
            verificationStore
        };
    },
    expose: ['openErrors', 'openWarnings', 'close'],
    methods: {
        openErrors() {
            this.typeOfMessageToBeShown = 'error';
            this.messagesToShow = this.verificationStore.status.errors;
            this.visible = true;
        },
        openWarnings() {
            this.typeOfMessageToBeShown = 'warning';
            this.messagesToShow = this.verificationStore.status.warnings;
            this.visible = true;
        },
        close() {
            this.visible = false;
        },
        async verify() {
            await this.verificationStore.fetchVerificationStatus(
                this.questionnaireId
            );

            if (this.typeOfMessageToBeShown === 'error')
                this.messagesToShow = this.verificationStore.status.errors;
            else this.messagesToShow = this.verificationStore.status.warnings;
        }
    }
};
</script>
