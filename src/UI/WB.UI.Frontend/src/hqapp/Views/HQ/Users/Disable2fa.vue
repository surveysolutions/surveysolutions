<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :userName="userInfo.userName"
        :canChangePassword="userInfo.canChangePassword"
        :userId="userInfo.userId"
        :currentTab="currentTab">

        <div >
            <h2>{{$t('Strings.HQ_Views_DisableTwoFactorAuth_Title')}}</h2>
        </div>
        <div  >
            <div class="alert alert-warning"
                role="alert">
                <p>
                    <strong>{{$t('Pages.Disable2faLine1')}}</strong>
                </p>
                <p>
                    {{$t('Pages.ChandgeKeyLine1')}} <a v-bind:href="getUrl('ResetAuthenticator')">{{$t('Pages.ChandgeKeyLine2')}}</a>
                </p>
            </div>
        </div>
        <div >
            <div class="block-filter">
                <button
                    type="submit"
                    class="btn btn-danger"
                    id="btnDisable2fa"
                    v-bind:disabled="userInfo.isObserving"
                    @click="disable2fa">{{$t('Pages.Disable2fa')}}</button>
            </div>
        </div>

    </ProfileLayout>
</template>

<script>
import Vue from 'vue'
import {each} from 'lodash'

export default {
    data() {
        return {
            modelState: {},
        }
    },
    computed: {
        currentTab(){
            return 'two-factor'
        },
        model() {
            return this.$config.model
        },
        userInfo() {
            return this.model.userInfo
        },
        isOwnProfile() {
            return this.userInfo.isOwnProfile
        },
    },
    methods: {
        disable2fa(){
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.disable2faUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    window.location.href = self.model.api.twoFactorAuthenticationUrl
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )

        },
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return `./${baseUrl}`
            else
                return `../${baseUrl}/` + this.model.userInfo.userId
        },
    },
}
</script>
