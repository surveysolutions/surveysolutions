<template>
    <HqLayout :mainClass="'interview-setup'" :title="$t('Settings.WorkspaceSettings')">
        <div class="col-md-12">
            <div class="welcome-page">
                <ul class="nav nav-tabs" role="tablist" id="settingsTabs">
                    <li role="presentation">
                        <a href="#export" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="export" role="tab" data-bs-toggle="tab" class="active">
                            {{ $t('Settings.Export') }}
                        </a>
                    </li>
                    <li role="presentation">
                        <a href="#note" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="note" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.GlobalNote') }}
                        </a>
                    </li>
                    <li role="presentation">
                        <a href="#profile" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="profile" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.UserProfile') }}
                        </a>
                    </li>
                    <li role="presentation">
                        <a href="#devices" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="devices" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.Devices') }}
                        </a>
                    </li>
                    <li role="presentation">
                        <a href="#webinterview" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="webinterview" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.WebInterview') }}
                        </a>
                    </li>
                    <li role="presentation">
                        <a href="#logo" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="logo" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.Logo') }}
                        </a>
                    </li>

                </ul>
                <div class="tab-content">
                    <Export :encryptionEnabled="encryptionEnabled" :encryptionPassword="encryptionPassword"
                        :isRetentionEnabled="isRetentionEnabled" :retentionLimitInDays="retentionLimitInDays"
                        :retentionLimitQuantity="retentionLimitQuantity" />
                    <Note :globalNotice="globalNotice" />
                    <Profile :isAllowInterviewerUpdateProfile="isAllowInterviewerUpdateProfile" />
                    <Devices :isInterviewerAutomaticUpdatesEnabled="isInterviewerAutomaticUpdatesEnabled"
                        :isDeviceNotificationsEnabled="isDeviceNotificationsEnabled"
                        :isPartialSynchronizationEnabled="isPartialSynchronizationEnabled"
                        :geographyQuestionAccuracyInMeters="geographyQuestionAccuracyInMeters"
                        :geographyQuestionPeriodInSeconds="geographyQuestionPeriodInSeconds" :esriApiKey="esriApiKey" />
                    <WebInterview :isEmailAllowed="isEmailAllowed" />
                    <Logo />
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<style scoped>
.logo {
    max-width: 365px;
    max-height: 329px;
}

.input-group .form-control.number {
    border: 1px solid #ccc;
    border-radius: 4px;
}

.input-group .form-control.number[aria-invalid='true'] {
    color: red;
}

.form-group .btn-success {
    margin-left: 20px;
    margin-top: 2px;
}

.form-group .input-group-save {
    display: inline;
}

.block-filter .error {
    color: red;
}
</style>

<script>
import { Form, Field, ErrorMessage } from 'vee-validate'
import emitter from '~/shared/emitter';

import Export from './Settings/Export'
import Note from './Settings/Note'
import Profile from './Settings/Profile'
import Devices from './Settings/Devices'
import WebInterview from './Settings/WebInterview'
import Logo from './Settings/Logo'

export default {
    components: {
        Export,
        Note,
        Profile,
        Devices,
        WebInterview,
        Logo,

        Form,
        Field,
        ErrorMessage,
    },
    data() {
        return {
            encryptionEnabled: false,
            encryptionPassword: null,
            globalNotice: null,
            isAllowInterviewerUpdateProfile: false,
            isInterviewerAutomaticUpdatesEnabled: false,
            isPartialSynchronizationEnabled: false,
            isDeviceNotificationsEnabled: false,
            isEmailAllowed: false,
            files: [],
            geographyQuestionAccuracyInMeters: 10,
            geographyQuestionPeriodInSeconds: 10,
            geographyQuestionAccuracyInMetersCancel: 10,
            geographyQuestionPeriodInSecondsCancel: 10,
            allowToRemoveExportCache: true,
            statusDropExportCache: 'NotStarted',
            dropSchemaTimer: null,
            dropSchemaDots: 1,
            esriApiKey: null,
            esriApiKeyInitial: null,

            isRetentionEnabled: false,
            retentionLimitInDays: null,
            retentionLimitQuantity: null,
        }
    },
    mounted() {
        this.getFormData()
    },
    methods: {
        noAction() { },
        async getFormData() {

            const workspaceSettings = await this.$hq.AdminSettings.getWorkspaceSettings()
            this.isInterviewerAutomaticUpdatesEnabled =
                workspaceSettings.data.interviewerAutoUpdatesEnabled
            this.isDeviceNotificationsEnabled =
                workspaceSettings.data.notificationsEnabled
            this.isPartialSynchronizationEnabled =
                workspaceSettings.data.partialSynchronizationEnabled
            this.geographyQuestionAccuracyInMeters =
                workspaceSettings.data.geographyQuestionAccuracyInMeters
            this.geographyQuestionPeriodInSeconds =
                workspaceSettings.data.geographyQuestionPeriodInSeconds
            this.geographyQuestionAccuracyInMetersCancel =
                workspaceSettings.data.geographyQuestionAccuracyInMeters
            this.geographyQuestionPeriodInSecondsCancel =
                workspaceSettings.data.geographyQuestionPeriodInSeconds

            this.esriApiKey = workspaceSettings.data.esriApiKey
            this.esriApiKeyInitial = workspaceSettings.data.esriApiKey
            this.encryptionEnabled = workspaceSettings.data.exportSettings.isEnabled
            this.encryptionPassword = workspaceSettings.data.exportSettings.password
            this.globalNotice = workspaceSettings.data.globalNotice
            this.isAllowInterviewerUpdateProfile = workspaceSettings.data.allowInterviewerUpdateProfile
            this.isEmailAllowed = workspaceSettings.data.allowEmails

            this.isRetentionEnabled = workspaceSettings.data.exportSettings.isRetentionEnabled
            this.retentionLimitInDays = workspaceSettings.data.exportSettings.retentionLimitInDays
            this.retentionLimitQuantity = workspaceSettings.data.exportSettings.retentionLimitQuantity
        },

        setPageActive(titleType, messageType) {
            this.$nextTick(function () {
                emitter.emit('workspacesettings:page:active', {
                    titleType,
                    messageType,
                })
            })
        },
    },
}
</script>
