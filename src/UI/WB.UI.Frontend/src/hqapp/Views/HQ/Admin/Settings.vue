<template>
    <HqLayout :mainClass="'interview-setup'"
        :title="$t('Settings.WorkspaceSettings')">
        <div class="col-md-12">
            <div class="welcome-page">
                <ul class="nav nav-tabs"
                    role="tablist"
                    id="settingsTabs">
                    <li role="presentation"
                        class="nav-item">
                        <a href="#export"
                            @click.prevent="activateTab('export', 'welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="export"
                            role="tab"
                            :aria-selected="activeTab === 'export'"
                            class="nav-link"
                            :class="{ active: activeTab === 'export' }">
                            {{ $t('Settings.Export') }}
                        </a>
                    </li>
                    <li role="presentation"
                        class="nav-item">
                        <a href="#note"
                            @click.prevent="activateTab('note', 'welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="note"
                            role="tab"
                            :aria-selected="activeTab === 'note'"
                            class="nav-link"
                            :class="{ active: activeTab === 'note' }">
                            {{ $t('Settings.GlobalNote') }}
                        </a>
                    </li>
                    <li role="presentation"
                        class="nav-item">
                        <a href="#profile"
                            @click.prevent="activateTab('profile', 'welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="profile"
                            role="tab"
                            :aria-selected="activeTab === 'profile'"
                            class="nav-link"
                            :class="{ active: activeTab === 'profile' }">
                            {{ $t('Settings.UserProfile') }}
                        </a>
                    </li>
                    <li role="presentation"
                        class="nav-item">
                        <a href="#devices"
                            @click.prevent="activateTab('devices', 'welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="devices"
                            role="tab"
                            :aria-selected="activeTab === 'devices'"
                            class="nav-link"
                            :class="{ active: activeTab === 'devices' }">
                            {{ $t('Settings.Devices') }}
                        </a>
                    </li>
                    <li role="presentation"
                        class="nav-item">
                        <a href="#logo"
                            @click.prevent="activateTab('logo', 'welcomeTextTitle', 'welcomeTextDescription')"
                            aria-controls="logo"
                            role="tab"
                            :aria-selected="activeTab === 'logo'"
                            class="nav-link"
                            :class="{ active: activeTab === 'logo' }">
                            {{ $t('Settings.Logo') }}
                        </a>
                    </li>

                </ul>
                <div class="tab-content">
                    <Export v-show="activeTab === 'export'"
                        :class="{ active: activeTab === 'export' }"
                        v-model:encryptionEnabled="encryptionEnabled"
                        v-model:encryptionPassword="encryptionPassword"
                        v-model:isRetentionEnabled="isRetentionEnabled"
                        v-model:retentionLimitInDays="retentionLimitInDays"
                        v-model:retentionLimitQuantity="retentionLimitQuantity"
                        v-model:retentionLimitInDaysCancel="retentionLimitInDaysCancel"
                        v-model:retentionLimitQuantityCancel="retentionLimitQuantityCancel"
                        v-model:geographyExportFormat="geographyExportFormat" />
                    <Note v-show="activeTab === 'note'"
                        :class="{ active: activeTab === 'note' }"
                        v-model="globalNotice" />
                    <Profile v-show="activeTab === 'profile'"
                        :class="{ active: activeTab === 'profile' }"
                        v-model="isAllowInterviewerUpdateProfile" />
                    <Devices v-show="activeTab === 'devices'"
                        :class="{ active: activeTab === 'devices' }"
                        v-model:isInterviewerAutomaticUpdatesEnabled="isInterviewerAutomaticUpdatesEnabled"
                        v-model:isDeviceNotificationsEnabled="isDeviceNotificationsEnabled"
                        v-model:isPartialSynchronizationEnabled="isPartialSynchronizationEnabled"
                        v-model:geographyQuestionAccuracyInMeters="geographyQuestionAccuracyInMeters"
                        v-model:geographyQuestionPeriodInSecondsCancel="geographyQuestionPeriodInSecondsCancel"
                        v-model:geographyQuestionPeriodInSeconds="geographyQuestionPeriodInSeconds"
                        v-model:geographyQuestionAccuracyInMetersCancel="geographyQuestionAccuracyInMetersCancel"
                        v-model:esriApiKey="esriApiKey"
                        v-model:esriApiKeyInitial="esriApiKeyInitial"
                        v-model:allowSupervisorChangeAssignmentStatus="allowSupervisorChangeAssignmentStatus"
                        v-model:allowInterviewerChangeAssignmentStatus="allowInterviewerChangeAssignmentStatus" />

                    <Logo v-show="activeTab === 'logo'"
                        :class="{ active: activeTab === 'logo' }" />
                </div>
            </div>
        </div>
    </HqLayout>
</template>


<script>
import emitter from '~/shared/emitter'

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
            allowSupervisorChangeAssignmentStatus: true,
            allowInterviewerChangeAssignmentStatus: true,

            isRetentionEnabled: false,
            retentionLimitInDays: null,
            retentionLimitQuantity: null,
            retentionLimitInDaysCancel: null,
            retentionLimitQuantityCancel: null,
            geographyExportFormat: 'Wkt',
            activeTab: 'export',
        }
    },
    async beforeMount() {
        await this.getFormData()

        const hashTab = window.location.hash?.replace('#', '')
        const availableTabs = ['export', 'note', 'profile', 'devices', 'logo']
        if (availableTabs.includes(hashTab)) {
            this.activeTab = hashTab
        }
    },
    methods: {
        noAction() { },
        activateTab(tab, titleType, messageType) {
            this.activeTab = tab
            if (window.location.hash !== `#${tab}`) {
                window.history.replaceState(null, '', `#${tab}`)
            }
            this.setPageActive(titleType, messageType)
        },
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
            this.allowSupervisorChangeAssignmentStatus = workspaceSettings.data.allowSupervisorChangeAssignmentStatus ?? true
            this.allowInterviewerChangeAssignmentStatus = workspaceSettings.data.allowInterviewerChangeAssignmentStatus ?? true

            this.encryptionEnabled = workspaceSettings.data.exportSettings.isEnabled
            this.encryptionPassword = workspaceSettings.data.exportSettings.password
            this.globalNotice = workspaceSettings.data.globalNotice
            this.isAllowInterviewerUpdateProfile = workspaceSettings.data.allowInterviewerUpdateProfile

            this.isRetentionEnabled = workspaceSettings.data.exportSettings.isRetentionEnabled
            this.retentionLimitInDays = workspaceSettings.data.exportSettings.retentionLimitInDays
            this.retentionLimitQuantity = workspaceSettings.data.exportSettings.retentionLimitQuantity
            this.retentionLimitInDaysCancel = workspaceSettings.data.exportSettings.retentionLimitInDays
            this.retentionLimitQuantityCancel = workspaceSettings.data.exportSettings.retentionLimitQuantity
            this.geographyExportFormat = workspaceSettings.data.exportSettings.geographyExportFormat ?? 'Wkt'
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
