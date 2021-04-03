<template>
    <HqLayout :hasFilter="false">
        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href=" this.$hq.basePath + 'Workspaces'">{{$t('MainMenu.Workspaces')}} - {{this.$hq.basePath.replaceAll('/', '')}}</a>
                </li>
                <li>
                    <a :href=" this.$hq.basePath + 'Settings'">{{$t('Common.Settings')}}</a>
                </li>
            </ol>

        </template>

        <div class="row extra-margin-bottom contain-input"
            data-suso="settings-page">
            <div class="col-sm-7">
                <h2>{{$t('Settings.ExportEncryption_Title')}}</h2>
                <p>{{$t('Settings.ExportEncryption_Description')}}</p>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="encryptionEnabled"
                            @click="clickEncryptionEnabled"
                            id="isEnabled"
                            type="checkbox"/>
                        <label for="isEnabled"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.EnableEncryption')}}
                        </label>
                    </div>
                </div>
                <div class="block-filter">
                    <label for="exportPassword">
                        {{$t('Settings.Password')}}:
                    </label>
                    <div class="form-group">
                        <div class="input-group">
                            <input
                                id="exportPassword"
                                type="text"
                                v-model="encryptionPassword"
                                readonly="readonly"
                                class="form-control"/>
                            <span class="input-group-btn">
                                <button
                                    class="btn btn-default"
                                    @click="regenPassword"
                                    :disabled="!encryptionEnabled">
                                    <i class="glyphicon glyphicon-refresh"></i>
                                </button>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.GlobalNoteSettings')}}</h2>
                <p>{{$t('Settings.GlobalNoteSettings_Description')}}</p>
            </div>
            <form class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <label for="notificationText">
                            {{$t('Settings.GlobalNotice')}}:
                        </label>
                        <textarea
                            class="form-control"
                            id="notificationText"
                            type="text"
                            v-model="globalNotice"
                            maxlength="1000"></textarea>
                    </div>
                </div>
                <div class="block-filter">
                    <button
                        type="button"
                        class="btn btn-success"
                        @click="updateMessage">{{$t('Common.Save')}}</button>
                    <button
                        type="button"
                        class="btn btn-link"
                        @click="clearMessage">{{$t('Common.Delete')}}</button>
                    <span
                        class="text-success"
                        v-if="globalNoticeUpdated">{{$t('Settings.GlobalNoteSaved')}}</span>
                </div>
            </form>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.UserProfileSettings_Title')}}</h2>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="isAllowInterviewerUpdateProfile"
                            @change="updateAllowInterviewerUpdateProfile"
                            id="allowInterviewerUpdateProfile"
                            type="checkbox"/>
                        <label for="allowInterviewerUpdateProfile"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.AllowInterviewerUpdateProfile')}}
                            <p
                                style="font-weight: normal">{{$t('Settings.AllowInterviewerUpdateProfileDesc')}}</p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.MobileAppSettings_Title')}}</h2>
                <p>{{$t('Settings.MobileAppSettings_Description')}}</p>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="isInterviewerAutomaticUpdatesEnabled"
                            @change="updateDeviceSettings"
                            id="interviewerAutomaticUpdatesEnabled"
                            type="checkbox"/>
                        <label for="interviewerAutomaticUpdatesEnabled"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.InterviewerAutoUpdate')}}
                            <p style="font-weight: normal">{{$t('Settings.AutoUpdateDescription')}}</p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="isDeviceNotificationsEnabled"
                            @change="updateDeviceSettings"
                            id="deviceNotificationsEnabled"
                            type="checkbox"/>
                        <label for="deviceNotificationsEnabled"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.DeviceNotifications')}}
                            <p
                                style="font-weight: normal">{{$t('Settings.DeviceNotificationsDescription')}}</p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="isPartialSynchronizationEnabled"
                            @change="updateDeviceSettings"
                            id="interviewerPartialSynchronizationEnabled"
                            type="checkbox"/>
                        <label for="interviewerPartialSynchronizationEnabled"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.InterviewerPartialSynchronization')}}
                            <p style="font-weight: normal">{{$t('Settings.PartialSynchronizationDescription')}}</p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.WebInterviewEmailNotifications_Title')}}</h2>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input
                            class="checkbox-filter single-checkbox"
                            v-model="isEmailAllowed"
                            @change="updateWebInterviewEmailNotifications"
                            id="allowWebInterviewEmailNotifications"
                            type="checkbox"/>
                        <label for="allowWebInterviewEmailNotifications"
                            style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.AllowWebInterviewEmailNotifications')}}
                            <p style="font-weight: normal">{{$t('Settings.AllowWebInterviewEmailNotificationsDesc')}}</p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.LogoSettings')}}</h2>
                <p>{{$t('Settings.LogoSettings_Description')}}</p>
                <p>{{$t('Settings.LogoSettings_Description1')}}</p>
            </div>
            <form :action="$config.model.updateLogoUrl"
                method="post"
                enctype="multipart/form-data"
                class="col-sm-7"
                @submit="onLogoSubmit">
                <input
                    name="__RequestVerificationToken"
                    type="hidden"

                    :value="this.$hq.Util.getCsrfCookie()"/>
                <div class="block-filter">
                    <div class="form-group"
                        :class="{ 'has-error': this.$config.model.invalidImage }">
                        <label for="companyLogo">
                            {{$t('Settings.Logo')}}
                        </label>
                        <input
                            type="file"
                            id="companyLogo"
                            ref="logoRef"
                            name="logo"
                            @change="changedFile"
                            accept="image/gif, image/jpeg, image/png"/>
                        <span class="help-block"
                            v-if="this.$config.model.invalidImage">{{ this.$t('Settings.LogoNotUpdated') }}</span>
                    </div>
                </div>
                <div class="block-filter">
                    <button
                        :disabled="files.length == 0"
                        type="submit"
                        class="btn btn-success">{{$t('Common.Save')}}</button>
                </div>
            </form>
            <div class="col-sm-7">
                <div class="block-filter">
                    <figure class="logo-wrapper">
                        <figcaption>{{$t('Settings.CurrentLogo')}}:</figcaption>
                        <img class="logo extra-margin-bottom"
                            ref="logoImage"
                            :src="$config.model.logoUrl"
                            @error="logoError" />
                    </figure>
                </div>
                <div class="block-filter action-block">
                    <form :action="$config.model.removeLogoUrl"
                        method="post">
                        <input
                            name="__RequestVerificationToken"
                            type="hidden"
                            :value="this.$hq.Util.getCsrfCookie()"/>
                        <button type="submit"
                            class="btn btn-danger">
                            {{$t('Settings.RemoveLogo')}}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </HqLayout>
    <!-- <script type="text/html" id="confirm-regenerate-password">
            <h3>
                @Settings.RegeneratePasswordConfirm
            </h3>
        </script>
        <script type="text/html" id="confirm-note-clearing">
            <h3>
                @Settings.GlobalNoteClearingConfirm
            </h3>
    </script>-->
</template>
<script>
import Vue from 'vue'
import modal from '@/shared/modal'

export default {
    data() {
        return {
            encryptionEnabled: false,
            encryptionPassword: null,
            globalNotice: null,
            globalNoticeUpdated: false,
            isAllowInterviewerUpdateProfile: false,
            isInterviewerAutomaticUpdatesEnabled: false,
            isPartialSynchronizationEnabled: false,
            isDeviceNotificationsEnabled: false,
            isEmailAllowed: false,
            files: [],
        }
    },
    mounted() {
        this.getFormData()
    },
    methods: {
        async getFormData() {
            const response = await this.$hq.ExportSettings.getEncryption()
            this.encryptionEnabled = response.data.isEnabled
            this.encryptionPassword = response.data.password

            const globalNoticeResponse = await this.$hq.AdminSettings.getGlobalNotice()
            this.globalNotice = globalNoticeResponse.data.globalNotice

            const profile = await this.$hq.AdminSettings.getProfileSettings()
            this.isAllowInterviewerUpdateProfile =
                profile.data.allowInterviewerUpdateProfile

            const interviewerSettings = await this.$hq.AdminSettings.getInterviewerSettings()
            this.isInterviewerAutomaticUpdatesEnabled = interviewerSettings.data.interviewerAutoUpdatesEnabled
            this.isDeviceNotificationsEnabled = interviewerSettings.data.notificationsEnabled
            this.isPartialSynchronizationEnabled = interviewerSettings.data.partialSynchronizationEnabled

            const webInterviewSettings = await this.$hq.AdminSettings.getWebInterviewSettings()
            this.isEmailAllowed = webInterviewSettings.data.allowEmails
        },
        regenPassword() {
            const self = this
            modal.dialog({
                closeButton: false,
                message: self.$t('Settings.RegeneratePasswordConfirm'),
                buttons: {
                    cancel: {
                        label: self.$t('Common.No'),
                    },

                    success: {
                        label: self.$t('Common.Yes'),
                        callback: async () => {
                            const response = await this.$hq.ExportSettings.regenPassword()
                            this.encryptionPassword = response.data.password
                            this.encryptionEnabled = response.data.isEnabled
                        },
                    },
                },
            })
        },
        async updateMessage() {
            const response = await this.$hq.AdminSettings.setGlobalNotice(
                this.globalNotice
            )
            if (response.status === 200) {
                this.globalNoticeUpdated = true
            }
        },
        async clearMessage() {
            this.globalNotice = ''
            return this.updateMessage()
        },
        updateAllowInterviewerUpdateProfile() {
            this.$hq.AdminSettings.setProfileSettings(
                this.isAllowInterviewerUpdateProfile
            )
        },
        updateDeviceSettings() {
            return this.$hq.AdminSettings.setInterviewerSettings(
                this.isInterviewerAutomaticUpdatesEnabled,
                this.isDeviceNotificationsEnabled,
                this.isPartialSynchronizationEnabled
            )
        },
        updateWebInterviewEmailNotifications() {
            return this.$hq.AdminSettings.setWebInterviewSettings(
                this.isEmailAllowed
            )
        },
        onLogoSubmit() {
            if (
                window.File &&
                window.FileReader &&
                window.FileList &&
                window.Blob
            ) {
                //get the file size and file type from file input field
                var fsize = this.$refs.logoRef.files[0].size

                if (fsize > 1024 * 1024 * 10) {
                    alert('Logo image size should be less than 10mb')
                    return false
                } else {
                    return true
                }
            }
        },
        logoError() {
            if (this.$refs.logoImage.src !== this.$config.model.defaultLogoUrl)
                this.$refs.logoImage.src = this.$config.model.defaultLogoUrl
        },
        clickEncryptionEnabled() {
            var self = this
            modal.dialog({
                closeButton: false,
                message: self.$t('Settings.ChangeStateConfirm'),
                buttons: {
                    cancel: {
                        label: self.$t('Common.No'),
                        callback: () => {
                            self.encryptionEnabled = !self.encryptionEnabled
                        },
                    },
                    success: {
                        label: self.$t('Common.Yes'),
                        callback: async () => {
                            const response = await self.$hq.ExportSettings.setEncryption(
                                self.encryptionEnabled
                            )
                            self.encryptionEnabled = response.data.isEnabled
                            self.encryptionPassword = response.data.password
                        },
                    },
                },
            })
        },
        changedFile(e) {
            this.files = this.$refs.logoRef.files
        },
    },
}
</script>
<style scoped>
.logo {
    max-width: 365px;
    max-height: 329px;
}
</style>
