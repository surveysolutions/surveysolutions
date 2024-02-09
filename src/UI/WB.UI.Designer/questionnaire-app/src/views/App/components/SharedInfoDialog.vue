<template>
    <teleport to="body">
        <div v-if="visible" uib-modal-window="modal-window" class="modal share-window fade ng-scope ng-isolate-scope in"
            role="dialog" index="0" animate="animate" tabindex="-1" uib-modal-animation-class="fade" modal-in-class="in"
            modal-animation="true" style="z-index: 1050; display: block;">
            <div class="modal-dialog ">
                <div class="modal-content" uib-modal-transclude="">
                    <div class="modal-header blue-strip">
                        <button type="button" class="close" aria-hidden="true" @click="close()"></button>
                        <h1>
                            {{ $t('QuestionnaireEditor.SettingsTitle') }}
                        </h1>
                    </div>
                    <div class="modal-body share-question-dialog">
                        <ul class="nav nav-tabs" role="tablist">
                            <li role="presentation" :class="{ active: settings }">
                                <a role="tab" data-toggle="tab" href="#questionnaireSettingsTab" @click="togleTab(true)">
                                    {{ $t('QuestionnaireEditor.QuestionnaireSettings') }}
                                </a>
                            </li>
                            <li role="presentation" :class="{ active: !settings }">
                                <a role="tab" data-toggle="tab" href="#shareTab" v-if="!questionnaire.isReadOnlyForUser ||
                                    questionnaire.hasViewerAdminRights
                                    " @click="togleTab(false)">
                                    {{
                                        $t('QuestionnaireEditor.AccessSettings')
                                    }}
                                </a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div role="tabpanel" class="tab-pane" :class="{ active: settings }"
                                id="questionnaireSettingsTab">
                                <div class="row well-sm">
                                    <div class="col-xs-7">
                                        <div class="form-group">
                                            <label class="control-label" for="questionnaireTitle">
                                                {{ $t('QuestionnaireEditor.SettingsQuestionnaireName') }}
                                            </label>
                                            <input id="questionnaireTitle" type="text"
                                                class="form-control questionaire-title"
                                                :disabled="questionnaire.isReadOnlyForUser"
                                                v-model="questionnaireEdit.title" maxlength="2000" />
                                        </div>

                                        <div class="form-group input-variable-name">
                                            <label class="control-label" for="questionnaireTitle">
                                                {{ $t('QuestionnaireEditor.SettingsQuestionnaireVariable') }}
                                            </label>&nbsp;
                                            <help link="questionnaireVariableName" />
                                            <input id="questionnaireVariable" :disabled="questionnaire.isReadOnlyForUser"
                                                type="text" class="form-control questionaire-title"
                                                v-model="questionnaireEdit.variable" maxlength="32" spellcheck="false" />
                                        </div>

                                        <!--i know this is horrible, but i cannot remove paddings from checkbox image so that visually they inputs are aligned-->
                                        <div class="form-group" style="margin-left: -8px;">
                                            <input id="questionnaireHideIdDisabled" type="checkbox" class="wb-checkbox"
                                                :disabled="questionnaire.isReadOnlyForUser"
                                                v-model="questionnaireEdit.hideIfDisabled" />
                                            <label for="questionnaireHideIdDisabled">
                                                <span></span>
                                                {{ $t('QuestionnaireEditor.SettingsHideIfDisabled') }}
                                            </label>
                                            <help link="hideIfDisabled" />
                                        </div>
                                        <div class="form-group">
                                            <button type="button" class="btn btn-lg update-button"
                                                :class="{ 'btn-primary': dirty }" unsaved-warning-clear
                                                v-if="!questionnaire.isReadOnlyForUser" :disabled="!dirty"
                                                @click="updateTitle()">
                                                {{ $t('QuestionnaireEditor.Update') }}
                                            </button>
                                            <button type="button" id="edit-chapter-cancel-button"
                                                class="btn btn-lg btn-link" unsaved-warning-clear @click="cancel()">{{
                                                    $t('QuestionnaireEditor.Cancel') }}</button>
                                        </div>

                                    </div>
                                    <div class="col-xs-5">
                                        <img alt="qr code"
                                            :src="'/questionnaire/publicurl/' + questionnaire.questionnaireId" />
                                    </div>
                                </div>
                            </div>
                            <div role="tabpanel" class="tab-pane" :class="{ active: !settings }" id="shareTab">
                                <div class="well-sm">
                                    <h2>
                                        {{ $t('QuestionnaireEditor.CollaboratorsSettings') }}
                                    </h2>
                                    <em class="text-muted">
                                        {{ $t('QuestionnaireEditor.CollaboratorsDescriptionSettings') }}
                                    </em>
                                    <br />
                                    <ul class="accounts">
                                        <li v-for="s in questionnaire.sharedPersons">
                                            <a href="mailto:{{s.email}}">
                                                {{ s.email }}
                                            </a>
                                            ({{ s.login }}) -
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-link dropdown-toggle"
                                                    data-bs-toggle="dropdown" v-if="!s.isOwner">
                                                    {{ $t('QuestionnaireEditor.SettingsCan') }}
                                                    {{ getShareType(s.shareType).text }}
                                                    <span class="caret"></span>
                                                </button>
                                                <a href="javascript:void(0);" v-if="s.isOwner" class="owner">
                                                    <i>
                                                        {{ $t('QuestionnaireEditor.SettingsOwner') }}
                                                    </i>
                                                </a>
                                                <ul class="dropdown-menu" role="menu">
                                                    <li>
                                                        <a @click="revokeAccess(s)" href="javascript:void(0)">
                                                            {{ $t('QuestionnaireEditor.SettingsRevokeAccess') }}
                                                        </a>
                                                    </li>
                                                    <li v-if="isQuestionnaireOwner">
                                                        <a @click="passOwnership(s)" href="javascript:void(0)">
                                                            {{ $t('QuestionnaireEditor.SettingsMakeOwner') }}
                                                        </a>
                                                    </li>
                                                </ul>
                                            </div>
                                            <div v-if="passConfirmationOpen === s.email" class="well">
                                                <h2>
                                                    {{ $t('QuestionnaireEditor.AreYouSure') }}
                                                </h2>
                                                <p>
                                                    {{ $t('QuestionnaireEditor.MakeOwnerConfirmationMessage') }}
                                                </p>
                                                <div class="btn-group">
                                                    <button type="button" class="btn btn-default"
                                                        @click="passOwnershipConfirmation(s)">
                                                        {{ $t('QuestionnaireEditor.Yes') }}
                                                    </button>
                                                    <button type="button" class="btn btn-default"
                                                        @click="passOwnershipCancel()">
                                                        {{ $t('QuestionnaireEditor.Cancel') }}
                                                    </button>
                                                </div>
                                            </div>
                                        </li>
                                    </ul>
                                    <div class="row ng-pristine ng-valid">
                                        <div class="col-xs-7">
                                            <div class=" form-group"
                                                :class="{ 'has-error': viewModelData.doesUserExist == false }">
                                                <label class="control-label" for="questionnaireTitle">
                                                    {{ $t('QuestionnaireEditor.SettingsInviteCollaborators') }}
                                                </label>
                                                <div class="input-group">
                                                    <input type="text" class="form-control" name="shareWithInput"
                                                        v-model="viewModelData.shareWith" />
                                                    <div class="input-group-btn dropup share-type">
                                                        <button type="button" class="btn btn-default dropdown-toggle"
                                                            data-bs-toggle="dropdown" name="shareType" id="Share-type">
                                                            {{ viewModelData.shareType.text }}
                                                            <span class="caret"></span>
                                                        </button>
                                                        <ul class="dropdown-menu " role="menu"
                                                            aria-labelledby="dropdownMenu1">
                                                            <li role="presentation" v-for="shareType in shareTypeOptions">
                                                                <a role="menuitem" tabindex="-1" href="javascript:void(0);"
                                                                    @click="changeShareType(shareType)">
                                                                    {{ shareType.text }}
                                                                </a>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                                <p class="help-block ng-cloak" v-if="viewModelData.doesUserExist == false">
                                                    {{ $t('QuestionnaireEditor.SettingsProvideExistingEmail') }}
                                                </p>
                                            </div>
                                        </div>
                                        <div class="col-xs-5">
                                            <div class="form-group pull-right">
                                                <button class="btn btn-primary btn-lg invite-button" @click="invite()"
                                                    :disabled="!viewModelData.shareWith">
                                                    {{ $t('QuestionnaireEditor.SettingsInvite') }}
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <hr />
                                    <div v-if="isQuestionnaireOwner || questionnaire.hasViewerAdminRights">
                                        <h2>
                                            {{ $t('QuestionnaireEditor.AnonymousQuestionnaireSettings') }}
                                        </h2>
                                        <em class="text-muted">
                                            {{ $t('QuestionnaireEditor.AnonymousQuestionnaireDescriptionSettings') }}
                                        </em>
                                        <br />
                                        <span>
                                            {{
                                                questionnaire.isAnonymouslyShared
                                                ? $t('QuestionnaireEditor.SettingsStatusAllowAnonymousAccess')
                                                : $t('QuestionnaireEditor.SettingsStatusDontAllowAnonymousAccess')
                                            }}
                                        </span>
                                        <button v-if="isQuestionnaireOwner" class="btn btn-link answer"
                                            @click="updateAnonymousQuestionnaireSettings()">
                                            {{
                                                questionnaire.isAnonymouslyShared
                                                ? $t('QuestionnaireEditor.SettingsTurnOffAnonymousAccess')
                                                : $t('QuestionnaireEditor.SettingsTurnOnAnonymousAccess')
                                            }}
                                        </button>
                                        <br />
                                        <div v-if="questionnaire.isAnonymouslyShared" class="row">
                                            <div class="col-xs-7">
                                                <div style="margin-top: 15px">
                                                    {{ $t('QuestionnaireEditor.AnonymousLink') }}
                                                </div>
                                                <a v-href="getAnonymousQuestionnaireLink()" style="word-break: break-word;"
                                                    title="link" onclick="return false;">
                                                    {{
                                                        getAnonymousQuestionnaireLink()
                                                    }}
                                                </a>
                                                <br />
                                                <div>
                                                    {{ $t('QuestionnaireEditor.AnonymousQuestionnaireGeneratedDate', {
                                                        datetime: anonymousQuestionnaireShareDate
                                                    })
                                                    }}
                                                </div>
                                                <button class="btn btn-link answer" style="padding-left: 0px;"
                                                    @click="copyAnonymousQuestionnaireLink()">
                                                    {{ $t('QuestionnaireEditor.CopyLink') }}
                                                </button>
                                                <span v-if="isQuestionnaireOwner">
                                                    |
                                                </span>
                                                <button v-if="isQuestionnaireOwner" class="btn btn-link answer"
                                                    @click="regenerateAnonymousQuestionnaireLink()">
                                                    {{ $t('QuestionnaireEditor.RegenerateQuestionnaireSharingLink') }}
                                                </button>
                                            </div>
                                            <div class="col-xs-5">
                                                <img alt="qr code" v-if="questionnaire.isAnonymouslyShared"
                                                    :src="'/questionnaire/publicurl/' + questionnaire.anonymousQuestionnaireId" />
                                            </div>
                                        </div>
                                    </div>

                                    <hr v-if="questionnaire.hasViewerAdminRights
                                        " />
                                    <div v-if="questionnaire.hasViewerAdminRights
                                        ">
                                        <h2>
                                            {{ $t('QuestionnaireEditor.PublicAccessSettings') }}
                                        </h2>
                                        <em class="text-muted">
                                            {{ $t('QuestionnaireEditor.PublicAccessDescriptionSettings') }}
                                        </em>
                                        <br />
                                        <span>
                                            {{ questionnaire.isPublic ?
                                                $t('QuestionnaireEditor.PublicAccessSettingsStatusOn') :
                                                $t('QuestionnaireEditor.PublicAccessSettingsStatusOff') }}
                                        </span>
                                        <button class="btn btn-link answer" @click="togglePublicity()">
                                            {{
                                                questionnaire.isPublic
                                                ? $t('QuestionnaireEditor.PublicAccessSettingsOff')
                                                : $t('QuestionnaireEditor.PublicAccessSettingsOn')
                                            }}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div v-if="visible" uib-modal-backdrop="modal-backdrop" class="modal-backdrop fade ng-scope in"
            uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true"
            data-bootstrap-modal-aria-hidden-count="1" aria-hidden="true" style="z-index: 1040;" @click="cancel()"></div>
    </teleport>
</template>

<script>
import _ from 'lodash';
import Help from './Help.vue';
import { toLocalDateTime } from '../../../services/utilityService';
import { findUserByEmailOrLogin } from '../../../services/userService';
import {
    updateQuestionnaireSettings,
    regenerateAnonymousQuestionnaireLink,
    updateAnonymousQuestionnaireSettings,
    shareWith,
    removeSharedPerson,
    passOwnership
} from '../../../services/questionnaireService';
import { useQuestionnaireStore } from '../../../stores/questionnaire';

export default {
    name: 'SharedInfoDialog',
    components: {
        Help
    },
    inject: ['questionnaire', 'currentUser'],
    props: {
        questionnaireId: { type: String, required: true }
    },
    data() {
        return {
            viewModelData: {},
            shareTypeOptions: [
                {
                    name: 'Edit',
                    text: this.$t('QuestionnaireEditor.SettingsShareEdit')
                },
                {
                    name: 'View',
                    text: this.$t('QuestionnaireEditor.SettingsShareView')
                }
            ],

            visible: false,
            settings: true,
            passConfirmationOpen: null
        };
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    beforeMount() {
        this.viewModelData = {
            shareWith: '',
            shareForm: {},
            shareType: {
                name: 'Edit',
                text: this.$t('QuestionnaireEditor.SettingsShareEdit')
            },
            doesUserExist: true
        };
    },
    computed: {
        questionnaireEdit() {
            return this.questionnaireStore.getEdittingSharedInfo;
        },
        anonymousQuestionnaireShareDate() {
            return toLocalDateTime(this.questionnaire.anonymouslySharedAtUtc)
        },
        dirty() {
            return this.questionnaireStore.getQuestionnaireEditDataDirty;
        },
        isQuestionnaireOwner() {
            const userEmail = this.currentUser.email;
            var user = this.questionnaire.sharedPersons.find(item => item.email == userEmail);
            return user && user.isOwner;
        },
    },
    expose: ['open', 'close'],
    methods: {
        open() {
            this.visible = true;
        },
        close() {
            this.visible = false;
        },
        togglePublicity() {
            updateQuestionnaireSettings(this.questionnaireId, {
                isPublic: !this.questionnaire.isPublic,
                title: this.questionnaire.title,
                variable: this.questionnaire.variable,
                hideIfDisabled: this.questionnaire.hideIfDisabled,
                defaultLanguageName: this.questionnaire.defaultLanguageName
            });
        },
        async regenerateAnonymousQuestionnaireLink() {
            regenerateAnonymousQuestionnaireLink(this.questionnaireId);
        },
        copyAnonymousQuestionnaireLink() {
            const link =
                window.location.origin +
                '/q/details/' +
                this.questionnaire.anonymousQuestionnaireId;
            navigator.clipboard.writeText(link);
        },
        getAnonymousQuestionnaireLink() {
            return (
                window.location.origin +
                '/q/details/' +
                this.questionnaire.anonymousQuestionnaireId
            );
        },
        updateAnonymousQuestionnaireSettings() {
            updateAnonymousQuestionnaireSettings(
                this.questionnaireId,
                !this.questionnaire.isAnonymouslyShared);
        },
        passOwnershipCancel() {
            this.passConfirmationOpen = null;
        },
        passOwnershipConfirmation(newOwner) {
            passOwnership(
                this.questionnaire.questionnaireId,
                this.currentUser.email,
                newOwner.userId,
                newOwner.email
            );

            this.passConfirmationOpen = null;
        },
        passOwnership(personInfo) {
            this.passConfirmationOpen = personInfo.email;
        },
        revokeAccess(personInfo) {
            removeSharedPerson(
                this.questionnaireId,
                personInfo.userId,
                personInfo.email
            );
        },

        async invite() {
            const user = await findUserByEmailOrLogin(
                this.viewModelData.shareWith
            );
            this.viewModelData.doesUserExist = user.doesUserExist;

            if (user.doesUserExist) {
                self = this;
                shareWith(
                    user.email,
                    user.userName,
                    user.id,
                    this.questionnaire.questionnaireId,
                    this.viewModelData.shareType.name
                );

                self.viewModelData.shareWith = '';
                self.viewModelData.doesUserExist = true;
            }
        },
        changeShareType(shareType) {
            this.viewModelData.shareType = shareType;
        },
        getShareType(type) {
            if (type === 'Edit' || type === 'View')
                return _.find(this.shareTypeOptions, { name: type });
            else return _.find(this.shareTypeOptions, { name: type.name });
        },
        updateTitle() {
            updateQuestionnaireSettings(this.questionnaireId, {
                isPublic: this.questionnaire.isPublic,
                title: this.questionnaireEdit.title,
                variable: this.questionnaireEdit.variable,
                hideIfDisabled: this.questionnaireEdit.hideIfDisabled,
                defaultLanguageName: this.questionnaire.defaultLanguageName
            });

            this.close();
        },
        togleTab(value) {
            this.settings = value;
        },

        cancel() {
            this.questionnaireStore.resetSharedInfo();
        }
    }
};
</script>
