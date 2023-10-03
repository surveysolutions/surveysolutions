<template>
    <div
        id="verification-modal"
        ng-show="visible"
        ng-class="verificationStatus.typeOfMessageToBeShown"
    >
        <div class="modal-content">
            <div class="modal-header">
                <button
                    type="button"
                    class="close"
                    ng-click="closeVerifications()"
                    aria-hidden="true"
                ></button>
                <h3 class="modal-title">
                    <span ng-i18next="CompilationLabel"></span>

                    <span
                        ng-if="verificationStatus.typeOfMessageToBeShown === 'error'"
                        ng-i18next="({count: verificationStatus.messagesToShow.length})CompilationErrorsCounter"
                    ></span>
                    <span
                        ng-if="verificationStatus.typeOfMessageToBeShown === 'warning'"
                        ng-i18next="({count: verificationStatus.messagesToShow.length})CompilationWarningsCounter"
                    ></span>
                </h3>
            </div>
            <div class="modal-body">
                <perfect-scrollbar class="scroller">
                    <div id="verify-popover-content">
                        <div class="question-list">
                            <div
                                ng-repeat="error in verificationStatus.messagesToShow"
                            >
                                <div class="error-message">
                                    <span class="error-code"
                                        >[{{ error.code }}]:</span
                                    >{{ error.message }}
                                </div>
                                <ul
                                    class="verification-items"
                                    ng-class="{singleError: !error.isGroupedMessage}"
                                >
                                    <empty-tag
                                        ng-repeat="referencesWithErrors in error.errors"
                                    >
                                        <li
                                            class="verification-item-container"
                                            ng-repeat="reference in referencesWithErrors.references"
                                            ng-click="navigateTo(reference)"
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
                                                    ng-if="reference.type == 'Question'"
                                                    class="icon {{reference.questionType}}  icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Questionnaire'"
                                                    class="icon icon-questionnaire icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Macro'"
                                                    class="icon icon-macro icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'LookupTable'"
                                                    class="icon icon-lookup icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'StaticText'"
                                                    class="icon icon-statictext icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Attachment'"
                                                    class="icon icon-attachment icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Variable'"
                                                    class="icon icon-variable icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Translation'"
                                                    class="icon icon-translation icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span
                                                    ng-if="reference.type == 'Categories'"
                                                    class="icon icon-categories icon-{{verificationStatus.typeOfMessageToBeShown}}"
                                                ></span>
                                                <span class="title">{{
                                                    reference.title | escape
                                                }}</span>
                                                <span
                                                    class="variable"
                                                    ng-bind-html="reference.variable || '&nbsp;'"
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
                <button class="btn btn-link" ng-click="verify()" ng-i18next>
                    Recompile
                </button>
                <button
                    class="btn btn-link"
                    data-dismiss="modal"
                    ng-click="closeVerifications()"
                    ng-i18next
                >
                    Close
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
            verificationStatus: null,
            visible: false
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
            this.verificationStatus = this.verificationStore.status;
            this.visible = true;
        },
        openWarnings() {
            this.verificationStatus = this.verificationStore.status;
            this.visible = true;
        },
        close() {
            this.visible = false;
        }
    }
};
</script>
