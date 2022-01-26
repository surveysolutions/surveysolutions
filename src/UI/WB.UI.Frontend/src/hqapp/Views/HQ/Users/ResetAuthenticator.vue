<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :userName="userInfo.userName"
        :canChangePassword="userInfo.canChangePassword"
        :userId="userInfo.userId"
        :currentTab="currentTab"
        :canGenerateToken="userInfo.canGetApiToken">

        <div >
            <h2>{{$t('Strings.HQ_Views_ResetAuthenticator_Title')}}</h2>
        </div>
        <div>
            <div class="alert alert-warning"
                role="alert">
                <p>
                    <span class="glyphicon glyphicon-warning-sign"
                        style="margin-right: 5px;"></span>
                    <strong>{{$t('Pages.ResetAuthenticatorLine1')}}</strong>
                </p>
                <p>
                    {{$t('Pages.ResetAuthenticatorLine2')}}
                </p>
            </div>
        </div>
        <div>
            <div class="block-filter">
                <button
                    type="submit"
                    class="btn btn-danger"
                    id="btnResetAuthenticator"
                    v-bind:disabled="userInfo.isObserving"
                    @click="resetAuthenticator">{{$t('Pages.ResetAuthenticator')}}</button>
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
            personName: null,
        }
    },
    computed: {
        currentTab(){
            return 'two-factor'
        },
        is2faEnabled(){
            return this.userInfo.is2faEnabled
        },
        recoveryCodesLeft(){
            return this.userInfo.recoveryCodesLeft
        },
        hasAuthenticator(){
            return this.userInfo.hasAuthenticator
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
        resetAuthenticator(){
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.resetAuthenticatorKeyUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    window.location.href = self.model.api.setupAuthenticatorUrl
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )

        },
    },
}
</script>
