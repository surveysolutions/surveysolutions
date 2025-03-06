<template>
    <HqLayout :hasFilter="false">
        <div class="row extra-margin-bottom contain-input" data-suso="settings-page">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.ExportEncryption_Title') }}</h2>
                <p>{{ $t('Settings.ExportEncryption_Description') }}</p>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="encryptionEnabled"
                            @change="changeEncryptionEnabled" id="isEnabled" type="checkbox" />
                        <label for="isEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EnableEncryption') }}
                        </label>
                    </div>
                </div>
                <div class="block-filter">
                    <label for="exportPassword">
                        {{ $t('Settings.Password') }}:
                    </label>
                    <div class="form-group">
                        <div class="input-group">
                            <input id="exportPassword" type="text" v-model="encryptionPassword" readonly="readonly"
                                class="form-control" />
                            <span class="input-group-btn">
                                <button class="btn btn-default" @click="regenPassword" :disabled="!encryptionEnabled">
                                    <i class="glyphicon glyphicon-refresh"></i>
                                </button>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input" data-suso="settings-page">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.ClearExportCache_Title') }}</h2>
                <p>{{ $t('Settings.ClearExportCache_Description') }}</p>
            </div>
            <div class="col-sm-7">
                <div class="block-filter action-block">
                    <button type="button" class="btn btn-danger" @click="removeExportCache"
                        :disabled="!allowToRemoveExportCache" style="margin-right: 15px">
                        {{ $t('Settings.RemoveExportCache') }}
                    </button>
                    <span v-if="!allowToRemoveExportCache && statusDropExportCache == 'Removing'" style="color:blue">
                        {{ $t('Settings.RemovingExportCache') + ".".repeat(dropSchemaDots) }}
                    </span>
                    <span v-if="statusDropExportCache == 'Removed'" style="color:green">
                        {{ $t('Settings.RemoveExportCacheSuccess') }}
                    </span>
                    <div v-if="statusDropExportCache == 'Error'">
                        <br />
                        <span style="color:red"
                            v-dompurify-html="$t('Settings.RemoveExportCacheFail').replaceAll('\n', '<br/>')">
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.GlobalNoteSettings') }}</h2>
                <p>{{ $t('Settings.GlobalNoteSettings_Description') }}</p>
            </div>
            <form class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <label for="notificationText">
                            {{ $t('Settings.GlobalNotice') }}:
                        </label>
                        <textarea class="form-control" id="notificationText" type="text" v-model="globalNotice"
                            maxlength="1000"></textarea>
                    </div>
                </div>
                <div class="block-filter">
                    <button type="button" class="btn btn-success" @click="updateMessage">
                        {{ $t('Common.Save') }}
                    </button>
                    <button type="button" class="btn btn-link" @click="clearMessage">
                        {{ $t('Common.Delete') }}
                    </button>
                    <span class="text-success" v-if="globalNoticeUpdated">{{
                        $t('Settings.GlobalNoteSaved')
                    }}</span>
                </div>
            </form>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.UserProfileSettings_Title') }}</h2>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isAllowInterviewerUpdateProfile"
                            @change="updateAllowInterviewerUpdateProfile" id="allowInterviewerUpdateProfile"
                            type="checkbox" />
                        <label for="allowInterviewerUpdateProfile" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowInterviewerUpdateProfile') }}
                            <p style="font-weight: normal">
                                {{
                                    $t(
                                        'Settings.AllowInterviewerUpdateProfileDesc',
                                    )
                                }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.MobileAppSettings_Title') }}</h2>
                <p>{{ $t('Settings.MobileAppSettings_Description') }}</p>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isInterviewerAutomaticUpdatesEnabled"
                            @change="updateDeviceSettings" id="interviewerAutomaticUpdatesEnabled" type="checkbox" />
                        <label for="interviewerAutomaticUpdatesEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.InterviewerAutoUpdate') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.AutoUpdateDescription') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isDeviceNotificationsEnabled"
                            @change="updateDeviceSettings" id="deviceNotificationsEnabled" type="checkbox" />
                        <label for="deviceNotificationsEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.DeviceNotifications') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.DeviceNotificationsDescription') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isPartialSynchronizationEnabled"
                            @change="updateDeviceSettings" id="interviewerPartialSynchronizationEnabled"
                            type="checkbox" />
                        <label for="interviewerPartialSynchronizationEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.InterviewerPartialSynchronization') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.PartialSynchronizationDescription') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-7">
                <Form v-slot="{ meta }" @submit="noAction">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="interviewerGeographyQuestionAccuracyInMeters" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.InterviewerGeographyQuestionAccuracyInMeters') }}
                                <p style="font-weight: normal">
                                    {{ $t('Settings.GeographyQuestionAccuracyInMetersDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model.number="geographyQuestionAccuracyInMeters"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 1,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" name="accuracy"
                                    label="Accuracy" id="interviewerGeographyQuestionAccuracyInMeters" type="number" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="geographyQuestionAccuracyInMeters ==
                                geographyQuestionAccuracyInMetersCancel ||
                                geographyQuestionAccuracyInMeters < 1 ||
                                geographyQuestionAccuracyInMeters > 1000 ||
                                meta.valid == false
                                " @click="updateGeographyQuestionAccuracyInMeters">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link"
                                :disabled="geographyQuestionAccuracyInMeters == geographyQuestionAccuracyInMetersCancel"
                                @click="cancelGeographyQuestionAccuracyInMeters">
                                {{ $t('Common.Cancel') }}
                            </button>
                        </div>
                        <div class="error">
                            <ErrorMessage name="accuracy"></ErrorMessage>
                        </div>
                    </div>
                </Form>
            </div>
            <div class="col-sm-7">
                <Form v-slot="{ meta }" @submit="noAction">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="interviewerGeographyQuestionPeriodInSeconds" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.InterviewerGeographyQuestionPeriodInSeconds') }}
                                <p style="font-weight: normal">
                                    {{ $t('Settings.GeographyQuestionPeriodInSecondsDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model.number="geographyQuestionPeriodInSeconds"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 5,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" label="Period"
                                    id="interviewerGeographyQuestionPeriodInSeconds" name="period" type="number" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="geographyQuestionPeriodInSeconds ==
                                geographyQuestionPeriodInSecondsCancel ||
                                geographyQuestionPeriodInSeconds < 5 ||
                                geographyQuestionPeriodInSeconds > 1000 ||
                                meta.valid == false" @click="updateGeographyQuestionPeriodInSeconds">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link"
                                :disabled="geographyQuestionPeriodInSeconds == geographyQuestionPeriodInSecondsCancel"
                                @click="cancelGeographyQuestionPeriodInSeconds">
                                {{ $t('Common.Cancel') }}
                            </button>
                        </div>
                        <div class="error">
                            <ErrorMessage name="period"></ErrorMessage>
                        </div>
                    </div>
                </Form>
            </div>

            <div class="col-sm-7">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="esriApiKey" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EsriApiKey') }}
                            <p class="error" style="font-weight: normal">
                                {{ $t('Settings.EsriApiKeyDescription') }}
                            </p>
                        </label>
                    </div>
                    <div class="form-group">
                        <div class="input-group input-group-save">
                            <input class="form-control number" type="password" v-model="esriApiKey" id="esriApiKey"
                                name="esriKey" />
                        </div>
                        <button type="button" class="btn btn-success" :disabled="esriApiKey ==
                            esriApiKeyInitial" @click="updateEsriKApiKey">
                            {{ $t('Common.Save') }}
                        </button>
                        <button type="button" class="btn btn-link" :disabled="esriApiKey ==
                            esriApiKeyInitial" @click="cancelEsriKApiKey">
                            {{ $t('Common.Cancel') }}
                        </button>
                    </div>
                </div>
                <div class="block-filter" style="padding-left: 30px">
                    <input id="ShowKey" type="checkbox"
                        onclick="var pass = document.getElementById('esriApiKey');pass.type = (pass.type === 'text' ? 'password' : 'text');">
                    <label for="ShowKey" style="padding-left:5px;">
                        <span></span>{{ $t('Pages.ShowKey') }}
                    </label>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>
                    {{ $t('Settings.WebInterviewEmailNotifications_Title') }}
                </h2>
            </div>
            <div class="col-sm-7">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isEmailAllowed"
                            @change="updateWebInterviewEmailNotifications" id="allowWebInterviewEmailNotifications"
                            type="checkbox" />
                        <label for="allowWebInterviewEmailNotifications" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowWebInterviewEmailNotifications') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.AllowWebInterviewEmailNotificationsDesc') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.LogoSettings') }}</h2>
                <p>{{ $t('Settings.LogoSettings_Description') }}</p>
                <p>{{ $t('Settings.LogoSettings_Description1') }}</p>
            </div>
            <form :action="$config.model.updateLogoUrl" method="post" enctype="multipart/form-data" class="col-sm-7"
                @submit="onLogoSubmit">
                <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                <div class="block-filter">
                    <div class="form-group" :class="{ 'has-error': this.$config.model.invalidImage }">
                        <label for="companyLogo">
                            {{ $t('Settings.Logo') }}
                        </label>
                        <input type="file" id="companyLogo" ref="logoRef" name="logo" @change="changedFile"
                            accept="image/gif, image/jpeg, image/png" />
                        <span class="help-block" v-if="this.$config.model.invalidImage">{{
                            this.$t('Settings.LogoNotUpdated') }}</span>
                    </div>
                </div>
                <div class="block-filter">
                    <button :disabled="files.length == 0" type="submit" class="btn btn-success">
                        {{ $t('Common.Save') }}
                    </button>
                </div>
            </form>
            <div class="col-sm-7">
                <div class="block-filter">
                    <figure class="logo-wrapper">
                        <figcaption>
                            {{ $t('Settings.CurrentLogo') }}:
                        </figcaption>
                        <img class="logo extra-margin-bottom" ref="logoImage" :src="$config.model.logoUrl"
                            @error="logoError" alt="logo image" />
                    </figure>
                </div>
                <div class="block-filter action-block">
                    <form :action="$config.model.removeLogoUrl" method="post">
                        <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                        <button type="submit" class="btn btn-danger">
                            {{ $t('Settings.RemoveLogo') }}
                        </button>
                    </form>
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
import modal from '@/shared/modal'

export default {
    components: {
        Form,
        Field,
        ErrorMessage,
    },
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
                            const response =
                                await this.$hq.ExportSettings.regenPassword()
                            this.encryptionPassword = response.data.password
                            this.encryptionEnabled = response.data.isEnabled
                        },
                    },
                },
            })
        },
        async updateMessage() {
            const response = await this.$hq.AdminSettings.setGlobalNotice(
                this.globalNotice,
            )
            if (response.status === 200) {
                this.globalNoticeUpdated = true
            }
        },
        async clearMessage() {
            this.globalNotice = ''
            return this.updateMessage()
        },
        async updateGeographyQuestionAccuracyInMeters() {
            if (
                this.geographyQuestionAccuracyInMeters < 5 &&
                this.geographyQuestionAccuracyInMeters > 1000
            )
                return

            return this.$hq.AdminSettings.setGeographyQuestionAccuracyInMeters(
                this.geographyQuestionAccuracyInMeters,
            ).then(() => {
                this.geographyQuestionAccuracyInMetersCancel =
                    this.geographyQuestionAccuracyInMeters
            })
        },
        cancelGeographyQuestionAccuracyInMeters() {
            this.geographyQuestionAccuracyInMeters =
                this.geographyQuestionAccuracyInMetersCancel
        },
        async updateGeographyQuestionPeriodInSeconds() {
            if (
                this.geographyQuestionPeriodInSeconds < 5 &&
                this.geographyQuestionPeriodInSeconds > 1000
            )
                return

            return this.$hq.AdminSettings.setGeographyQuestionPeriodInSeconds(
                this.geographyQuestionPeriodInSeconds,
            ).then(() => {
                this.geographyQuestionPeriodInSecondsCancel =
                    this.geographyQuestionPeriodInSeconds
            })
        },
        cancelGeographyQuestionPeriodInSeconds() {
            this.geographyQuestionPeriodInSeconds =
                this.geographyQuestionPeriodInSecondsCancel
        },
        updateAllowInterviewerUpdateProfile() {
            this.$hq.AdminSettings.setProfileSettings(
                this.isAllowInterviewerUpdateProfile,
            )
        },
        async updateEsriKApiKey() {
            return this.$hq.AdminSettings.setEsriApiKey(
                this.esriApiKey,
            ).then(() => {
                this.esriApiKeyInitial = this.esriApiKey
            })
        },
        cancelEsriKApiKey() { this.esriApiKey = this.esriApiKeyInitial },
        updateDeviceSettings() {
            return this.$hq.AdminSettings.setInterviewerSettings(
                this.isInterviewerAutomaticUpdatesEnabled,
                this.isDeviceNotificationsEnabled,
                this.isPartialSynchronizationEnabled,
            )
        },
        updateWebInterviewEmailNotifications() {
            return this.$hq.AdminSettings.setWebInterviewSettings(
                this.isEmailAllowed,
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
        changeEncryptionEnabled() {
            var self = this
            modal.dialog({
                closeButton: false,
                message: self.encryptionEnabled
                    ? self.$t('Settings.ChangeStateConfirm')
                    : self.$t('Settings.ChangeStateDisabledConfirm'),
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
                            const response =
                                await self.$hq.ExportSettings.setEncryption(
                                    self.encryptionEnabled,
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
        removeExportCache() {
            var self = this
            modal.dialog({
                closeButton: true,
                onEscape: true,
                title:
                    '<h2 style="display:inline">' + self.$t('Pages.ConfirmationNeededTitle') + '</h2>',
                message:
                    `<p style="color: red;"> ${self.$t(
                        'Settings.RemoveExportCache_Warning',
                    )}</p>` +
                    `<p>${self.$t('Settings.RemoveExportCacheConfirm')}</p>`,
                buttons: {
                    success: {
                        label: self.$t('Common.Clear'),
                        className: 'btn btn-danger',
                        callback: async () => {
                            this.allowToRemoveExportCache = false
                            this.statusDropExportCache = 'Removing'
                            await this.runDropExportSchema()
                        },
                    },
                    cancel: {
                        label: self.$t('Common.Cancel'),
                        className: 'btn btn-link',
                        callback: () => { },
                    },
                },
            })
        },
        async runDropExportSchema() {
            const status = await this.$hq.ExportSettings.statusDropExportCache()
            if (status.data.status != 'Removing')
                await this.$hq.ExportSettings.dropExportCache()
                    .then((response) => {
                        const success = response.data.success

                        this.dropSchemaTimer = setInterval(async () => await this.checkStatusDropExportCache(), 1000);
                    })
                    .catch((e) => {
                        if (
                            e.response &&
                            e.response.data &&
                            e.response.data.message
                        ) {
                            this.showAlert(e.response.data.message)
                            return
                        } else {
                            this.statusDropExportCache = 'Error'
                            this.allowToRemoveExportCache = true;
                            return
                        }
                    })
        },
        async checkStatusDropExportCache() {
            const status = await this.$hq.ExportSettings.statusDropExportCache()
            this.statusDropExportCache = status.data.status

            if (this.dropSchemaDots >= 3)
                this.dropSchemaDots = 1;
            else
                this.dropSchemaDots++;

            if (this.statusDropExportCache != 'Removing' && this.dropSchemaTimer != null) {
                clearInterval(this.dropSchemaTimer)
                this.dropSchemaTimer = null
                this.allowToRemoveExportCache = true;
            }
        },
        showAlert(message) {
            modal.alert({
                message: message,
                callback: () => {
                    location.reload()
                },
                onEscape: false,
                closeButton: false,
                buttons: {
                    ok: {
                        label: this.$t('WebInterviewUI.Reload'),
                        className: 'btn-success',
                    },
                },
            })
        },
    },
}
</script>
