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
                        <a href="#logo" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="logo" role="tab" data-bs-toggle="tab">
                            {{ $t('Settings.Logo') }}
                        </a>
                    </li>

                </ul>
                <div class="tab-content">
                    <Export v-model:encryptionEnabled="encryptionEnabled"
                        v-model:encryptionPassword="encryptionPassword" v-model:isRetentionEnabled="isRetentionEnabled"
                        v-model:retentionLimitInDays="retentionLimitInDays"
                        v-model:retentionLimitQuantity="retentionLimitQuantity"
                        v-model:retentionLimitInDaysCancel="retentionLimitInDaysCancel"
                        v-model:retentionLimitQuantityCancel="retentionLimitQuantityCancel" />
                    <Note v-model="globalNotice" />
                    <Profile v-model="isAllowInterviewerUpdateProfile" />
                    <Devices v-model:isInterviewerAutomaticUpdatesEnabled="isInterviewerAutomaticUpdatesEnabled"
                        v-model:isDeviceNotificationsEnabled="isDeviceNotificationsEnabled"
                        v-model:isPartialSynchronizationEnabled="isPartialSynchronizationEnabled"
                        v-model:geographyQuestionAccuracyInMeters="geographyQuestionAccuracyInMeters"
                        v-model:geographyQuestionPeriodInSecondsCancel="geographyQuestionPeriodInSecondsCancel"
                        v-model:geographyQuestionPeriodInSeconds="geographyQuestionPeriodInSeconds"
                        v-model:geographyQuestionAccuracyInMetersCancel="geographyQuestionAccuracyInMetersCancel"
                        v-model:esriApiKey="esriApiKey" v-model:esriApiKeyInitial="esriApiKeyInitial" />

                    <Logo />
                </div>
            </div>
        </div>
    </HqLayout>
</template>


<script>
import { Form, Field, ErrorMessage } from 'vee-validate'
import emitter from '~/shared/emitter';

import Export from './Settings/Export'
import Note from './Settings/Note'
import Profile from './Settings/Profile'
import Devices from './Settings/Devices'
import Logo from './Settings/Logo'

export default {
    components: {
        Export,
        Note,
        Profile,
        Devices,
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
            retentionLimitInDaysCancel: null,
            retentionLimitQuantityCancel: null,
        }
    },
    async beforeMount() {
        await this.getFormData()
    },
    methods: {
        noAction() { },
        async getFormData() {

            const workspaceSettings = await this.$hq.AdminSettings.getWorkspaceSettings()

            this.isInterviewerAutomaticUpdatesEnabled = workspaceSettings.data.interviewerAutoUpdatesEnabled
            this.isDeviceNotificationsEnabled = workspaceSettings.data.notificationsEnabled
            this.isPartialSynchronizationEnabled = workspaceSettings.data.partialSynchronizationEnabled

            this.geographyQuestionAccuracyInMeters = workspaceSettings.data.geographyQuestionAccuracyInMeters
            this.geographyQuestionAccuracyInMetersCancel = workspaceSettings.data.geographyQuestionAccuracyInMeters
            this.geographyQuestionPeriodInSeconds = workspaceSettings.data.geographyQuestionPeriodInSeconds
            this.geographyQuestionPeriodInSecondsCancel = workspaceSettings.data.geographyQuestionPeriodInSeconds
            this.esriApiKey = workspaceSettings.data.esriApiKey
            this.esriApiKeyInitial = workspaceSettings.data.esriApiKey

            this.encryptionEnabled = workspaceSettings.data.exportSettings.isEnabled
            this.encryptionPassword = workspaceSettings.data.exportSettings.password
            this.globalNotice = workspaceSettings.data.globalNotice
            this.isAllowInterviewerUpdateProfile = workspaceSettings.data.allowInterviewerUpdateProfile

            this.isRetentionEnabled = workspaceSettings.data.exportSettings.isRetentionEnabled
            this.retentionLimitInDays = workspaceSettings.data.exportSettings.retentionLimitInDays
            this.retentionLimitQuantity = workspaceSettings.data.exportSettings.retentionLimitQuantity
            this.retentionLimitInDaysCancel = workspaceSettings.data.exportSettings.retentionLimitInDays
            this.retentionLimitQuantityCancel = workspaceSettings.data.exportSettings.retentionLimitQuantity
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
