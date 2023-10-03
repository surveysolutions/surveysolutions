<template>
    <section id="header" class="row">
        <a class="designer-logo" href="../../"></a>
        <div class="header-line">
            <div class="header-menu">
                <div class="buttons">
                    <a
                        class="btn"
                        href="http://support.mysurvey.solutions/designer"
                        target="_blank"
                        >{{ $t('QuestionnaireEditor.Help') }}</a
                    >
                    <a
                        class="btn"
                        href="https://forum.mysurvey.solutions"
                        target="_blank"
                        >{{ $t('QuestionnaireEditor.Forum') }}
                    </a>
                    <a
                        class="btn"
                        :href="
                            '/questionnaire/questionnairehistory/' +
                                questionnaire.questionnaireId
                        "
                        target="_blank"
                        v-if="
                            questionnaire.hasViewerAdminRights ||
                                questionnaire.isSharedWithUser
                        "
                        >{{ $t('QuestionnaireEditor.History') }}</a
                    >
                    <button
                        class="btn"
                        type="button"
                        @click="showDownloadPdf()"
                    >
                        {{ $t('QuestionnaireEditor.DownloadPdf') }}
                    </button>
                    <a
                        class="btn"
                        type="button"
                        v-if="questionnaire.hasViewerAdminRights"
                        @click="saveAsQuestionnaire"
                        >{{ $t('QuestionnaireEditor.SaveAs') }}</a
                    >

                    <a
                        class="btn"
                        v-if="
                            questionnaire.questionnaireRevision ||
                                questionnaire.isReadOnlyForUser
                        "
                        :href="
                            '/questionnaire/clone/' +
                                questionnaire.questionnaireId +
                                (questionnaire.questionnaireRevision
                                    ? '$' + questionnaire.questionnaireRevision
                                    : '')
                        "
                        target="_blank"
                        >{{ $t('QuestionnaireEditor.CopyTo') }}</a
                    >
                    <button
                        class="btn"
                        type="button"
                        v-disabled="
                            questionnaire.isReadOnlyForUser &&
                                !questionnaire.hasViewerAdminRights &&
                                !questionnaire.isSharedWithUser
                        "
                        @click="showShareInfo()"
                    >
                        {{ $t('QuestionnaireEditor.Settings') }}
                    </button>

                    <div class="btn-group" v-if="currentUserIsAuthenticated">
                        <a class="btn btn-default">{{
                            $t('QuestionnaireEditor.HellowMessageBtn', {
                                currentUserName: currentUserName
                            })
                        }}</a>
                        <a
                            class="btn btn-default dropdown-toggle"
                            data-toggle="dropdown"
                            aria-haspopup="true"
                            aria-expanded="false"
                        >
                            <span class="caret"></span>
                            <span class="sr-only">{{
                                $t('QuestionnaireEditor.ToggleDropdown')
                            }}</span>
                        </a>
                        <ul class="dropdown-menu dropdown-menu-right">
                            <li>
                                <a href="/identity/account/manage">{{
                                    $t('QuestionnaireEditor.ManageAccount')
                                }}</a>
                            </li>
                            <li>
                                <a href="/identity/account/logout">{{
                                    $t('QuestionnaireEditor.LogOut')
                                }}</a>
                            </li>
                        </ul>
                    </div>
                    <a
                        class="btn"
                        href="/"
                        type="button"
                        v-if="!currentUserIsAuthenticated"
                        >{{ $t('QuestionnaireEditor.Login') }}</a
                    >
                    <a
                        class="btn"
                        href="/identity/account/register"
                        type="button"
                        v-if="!currentUserIsAuthenticated"
                        >{{ $t('QuestionnaireEditor.Register') }}</a
                    >
                </div>
            </div>
            <div class="questionnarie-title">
                <div class="title">
                    <div class="questionnarie-title-text">
                        {{ questionnaire.title }}
                    </div>
                    <div class="questionnarie-title-buttons">
                        <span class="text-muted">
                            {{
                                $t('QuestionnaireEditor.QuestionnaireSummary', {
                                    questionsCount:
                                        questionnaire.questionsCount,
                                    groupsCount: questionnaire.groupsCount,
                                    rostersCount: questionnaire.rostersCount
                                })
                            }}
                        </span>
                        <button
                            id="verification-btn"
                            type="button"
                            class="btn"
                            @click="verify()"
                            v-if="questionnaire.questionnaireRevision === null"
                        >
                            {{ $t('QuestionnaireEditor.Compile') }}
                        </button>
                        <span
                            v-if="
                                verificationStatus.warnings != null &&
                                    verificationStatus.errors != null
                            "
                        >
                            <span
                                data-toggle="modal"
                                v-if="
                                    verificationStatus.warnings.length +
                                        verificationStatus.errors.length >
                                        0
                                "
                                class="error-message v-hide"
                                v-class="{
                                    'no-errors':
                                        verificationStatus.errors.length == 0
                                }"
                            >
                                <a
                                    href="javascript:void(0);"
                                    @click="showVerificationErrors()"
                                >
                                    {{
                                        $t(
                                            'QuestionnaireEditor.ErrorsCounter',
                                            {
                                                count:
                                                    verificationStatus.errors
                                                        .length
                                            }
                                        )
                                    }}
                                </a>
                            </span>
                            <span
                                data-toggle="modal"
                                v-if="verificationStatus.warnings.length > 0"
                                class="warniv-message v-hide"
                            >
                                <a
                                    href="javascript:void(0);"
                                    @click="showVerificationWarnings()"
                                >
                                    {{
                                        $t(
                                            'QuestionnaireEditor.WarningsCounter',
                                            {
                                                count:
                                                    verificationStatus.warnings
                                                        .length
                                            }
                                        )
                                    }}
                                </a>
                            </span>
                            <span
                                class="text-success"
                                v-if="
                                    verificationStatus.warnings.length +
                                        verificationStatus.errors.length ===
                                        0
                                "
                            >
                                {{ $t('QuestionnaireEditor.Ok') }}
                            </span>

                            <span class="text-success">
                                <!-- <em>{{ $t('QuestionnaireEditor.SavedAtTimestamp',{ dateTime: verificationStatus.time}) }}</em> -->
                            </span>
                        </span>
                        <span
                            class="error-message strong"
                            v-if="questionnaire.isReadOnlyForUser"
                        >
                            {{ $t('QuestionnaireEditor.ReadOnly') }}</span
                        >
                        <button
                            id="webtest-btn"
                            type="button"
                            class="btn"
                            v-if="
                                questionnaire.webTestAvailable &&
                                    questionnaire.questionnaireRevision === null
                            "
                            @click="webTest()"
                        >
                            {{ $t('QuestionnaireEditor.Test') }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <section id="spacer" class="row">
        <div class="left"></div>
        <div class="right"></div>
    </section>

    <VerificationDialog ref="verificationDialog" />
</template>

<script>
import VerificationDialog from './VerificationDialog.vue';

import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useUserStore } from '../../../stores/user';
import { useVerificationStore } from '../../../stores/verification';

import WebTesterApi from '../../../api/webTester';

import { ref } from 'vue';

export default {
    name: 'QuestionnaireHeader',
    //inject: ["useQuestionnaireStore"],
    components: {
        VerificationDialog
    },
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            questionnaire: {
                questionnaireId: 0,
                hasViewerAdminRights: false,
                isSharedWithUser: false,
                questionnaireRevision: null,
                isReadOnlyForUser: false,
                webTestAvailable: false,
                title: '',
                questionsCount: 0,
                groupsCount: 0,
                rostersCount: 0
            },
            verificationStatus: {
                warnings: null, //[],
                errors: null, //[],
                time: null //''
            }
        };
    },
    setup(props) {
        const questionnaireStore = useQuestionnaireStore();
        const userStore = useUserStore();
        const verificationStore = useVerificationStore();

        //const verificationDialog = VerificationDialog;
        //const verificationDialog = .$refsref(null);
        const verificationDialog = ref(null);

        return {
            questionnaireStore,
            userStore,
            verificationStore,
            verificationDialog
        };
    },
    async beforeMount() {
        await this.fetch();

        this.questionnaire = this.questionnaireStore.info;
    },
    computed: {
        currentUserIsAuthenticated() {
            return this.userStore.isAuthenticated;
        },
        currentUserName() {
            return this.userStore.userName;
        }
    },
    methods: {
        async fetch() {
            await this.userStore.fetchUserInfo();
            await this.questionnaireStore.fetchQuestionnaireInfo(
                this.questionnaireId
            );
        },
        webTest() {
            WebTesterApi.run(this.questionnaireId);
        },
        showDownloadPdf() {
            // TODO
        },
        saveAsQuestionnaire() {
            // TODO
        },
        async verify() {
            this.verificationStatus.errors = null;
            this.verificationStatus.warnings = null;
            // $rootScope.$broadcast("verifing", {});

            await this.verificationStore.fetchVerificationStatus(
                this.questionnaireId
            );
            const data = this.verificationStore.status;

            this.verificationStatus.errors = data.errors;
            this.verificationStatus.warnings = data.warnings;
            this.verificationStatus.time = new Date();
            //this.verificationStatus.typeOfMessageToBeShown = ERROR;

            if (this.verificationStatus.errors.length > 0) {
                this.showVerificationErrors();
            } else {
                this.verificationDialog.close();
            }
        },
        showShareInfo() {
            // TODO
        },
        showVerificationErrors() {
            if (this.verificationStatus.errors.length === 0) return;
            //this.verificationStatus.typeOfMessageToBeShown = ERROR;
            //this.verificationStatus.messagesToShow = this.verificationStatus.errors;
            //this.verificationStatus.visible = true;
            this.verificationDialog.openErrors();
        },
        showVerificationWarnings() {
            if (this.verificationStatus.warnings.length === 0) return;
            //this.verificationStatus.typeOfMessageToBeShown = WARNING;
            //this.verificationStatus.messagesToShow = this.verificationStatus.warnings;
            //this.verificationStatus.visible = true;
            this.verificationDialog.openWarnings();
        }
    }
};
</script>
