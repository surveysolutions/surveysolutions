<template>
    <HqLayout :hasFilter="false">
        <div class="row extra-margin-bottom contain-input">
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
                            type="checkbox"
                        />
                        <label for="isEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.EnableEncryption')}}
                        </label>
                    </div>
                </div>
                <div class="block-filter">
                    <label for="exportPassword">{{$t('Settings.Password')}}:</label>
                    <div class="form-group">
                        <div class="input-group">
                            <input
                                id="exportPassword"
                                type="text"
                                v-model="encryptionPassword"
                                readonly="readonly"
                                class="form-control"
                            />
                            <span class="input-group-btn">
                                <button
                                    class="btn btn-default"
                                    @click="regenPassword"
                                    :disabled="!encryptionEnabled"
                                >
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
                        <label for="notificationText">{{$t('Settings.GlobalNotice')}}:</label>
                        <textarea
                            class="form-control"
                            id="notificationText"
                            type="text"
                            v-model="globalNotice"
                            maxlength="1000"
                        ></textarea>
                    </div>
                </div>
                <div class="block-filter">
                    <button
                        type="button"
                        class="btn btn-success"
                        @click="updateMessage"
                    >{{$t('Common.Save')}}</button>
                    <button
                        type="button"
                        class="btn btn-link"
                        @click="clearMessage"
                    >{{$t('Common.Delete')}}</button>
                    <span
                        class="text-success"
                        v-if="globalNoticeUpdated"
                    >{{$t('Settings.GlobalNoteSaved')}}</span>
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
                            data-bind="checked: isAllowInterviewerUpdateProfile, click: updateAllowInterviewerUpdateProfile"
                            id="allowInterviewerUpdateProfile"
                            type="checkbox"
                        />
                        <label for="allowInterviewerUpdateProfile" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.AllowInterviewerUpdateProfile')}}
                            <p
                                style="font-weight: normal"
                            >{{$t('Settings.AllowInterviewerUpdateProfileDesc')}}</p>
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
                            data-bind="checked: isInterviewerAutomaticUpdatesEnabled, click: updateDeviceSettings"
                            id="interviewerAutomaticUpdatesEnabled"
                            type="checkbox"
                        />
                        <label for="interviewerAutomaticUpdatesEnabled" style="font-weight: bold">
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
                            data-bind="checked: isDeviceNotificationsEnabled, click: updateDeviceSettings"
                            id="deviceNotificationsEnabled"
                            type="checkbox"
                        />
                        <label for="deviceNotificationsEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.DeviceNotifications')}}
                            <p
                                style="font-weight: normal"
                            >{{$t('Settings.DeviceNotificationsDescription')}}</p>
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
                            data-bind="checked: isEmailAllowed, click: updateWebInterviewEmailNotifications"
                            id="allowWebInterviewEmailNotifications"
                            type="checkbox"
                        />
                        <label for="allowWebInterviewEmailNotifications" style="font-weight: bold">
                            <span class="tick"></span>
                            {{$t('Settings.AllowWebInterviewEmailNotifications')}}
                            <p
                                style="font-weight: normal"
                            >{{$t('Settings.AllowWebInterviewEmailNotificationsDesc')}}</p>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-7">
                <h2>{{$t('Settings.LogoSettings')}}</h2>
                <p>{{$t('Settings.LogoSettings_Description')}}</p>
            </div>
            <form
                action="@Url.Action('UpdateLogo', 'Settings')"
                method="post"
                enctype="multipart/form-data"
                class="col-sm-7"
                data-bind="submit: onLogoSubmit"
            >
                <input
                    name="__RequestVerificationToken"
                    type="hidden"
                    :value="this.$hq.Util.getCsrfCookie()"
                />
                <div class="block-filter">
                    <div class="form-group">
                        <label for="companyLogo">{{$t('Settings.Logo')}}</label>
                        <input
                            type="file"
                            id="companyLogo"
                            name="logo"
                            accept="image/gif, image/jpeg, image/png"
                        />
                    </div>
                </div>
                <div class="block-filter">
                    <button
                        type="submit"
                        class="btn btn-success"
                        data-bind="click: onLogoSubmit"
                    >{{$t('Common.Save')}}</button>
                </div>
            </form>
            <div class="col-sm-7">
                <div class="block-filter">
                    <figure class="logo-wrapper">
                        <figcaption>@Settings.CurrentLogo:</figcaption>
                        <img class="logo extra-margin-bottom" />
                    </figure>
                </div>
                <div class="block-filter">
                    <form action="@Url.Action('RemoveLogo', 'Settings')" method="post">
                        <button type="submit" class="btn btn-danger">{{$t('Settings.RemoveLogo')}}</button>
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
import modal from '@/shared/modal'

export default {
    data() {
        return {
            encryptionEnabled: false,
            encryptionPassword: null,
            globalNotice: null,
            globalNoticeUpdated: false
        }
    },
    mounted() {
        this.getFormData()
    },
    methods: {
        async getFormData(){
            const response = await this.$hq.ExportSettings.getEncryption()
            this.encryptionEnabled = response.data.isEnabled
            this.encryptionPassword = response.data.password

            const globalNoticeResponse = await this.$hq.AdminSettings.getGlobalNotice()

            this.globalNotice = globalNoticeResponse.data.globalNotice
        },
        async regenPassword() {
            const response = await this.$hq.ExportSettings.regenPassword()
            this.encryptionPassword = response.data.password
            this.encryptionEnabled = response.data.isEnabled
        },
        async updateMessage(){
            const response = await this.$hq.AdminSettings.setGlobalNotice(this.globalNotice)
            if(response.status === 200)
            {
                this.globalNoticeUpdated = true
            }
        },
        async clearMessage(){
            this.globalNotice = ""
            return this.updateMessage()
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
                            const response = await self.$hq.ExportSettings.setEncryption(self.encryptionEnabled)
                            self.encryptionEnabled = response.data.isEnabled
                            self.encryptionPassword = response.data.password
                        }
                    }
                }
            })
        }
    }
}
</script>
<style scoped>
.logo {
    max-width: 365px;
    max-height: 329px;
}
</style>
