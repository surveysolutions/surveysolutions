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
            <div >
                <h2>{{$t('Strings.HQ_Views_EnableAuthenticator_Title')}}</h2>
            </div>
            <div >
                <p>{{$t('Pages.EnableAuthenticatorLine1')}}</p>
                <ol class="list">
                    <li>
                        <p>
                            {{$t('Pages.EnableAuthenticatorLine2')}}
                            <a href="https://go.microsoft.com/fwlink/?Linkid=825071">Windows Phone</a>,
                            <a href="https://go.microsoft.com/fwlink/?Linkid=825072">Android</a>,
                            <a href="https://go.microsoft.com/fwlink/?Linkid=825073">iOS</a>

                            {{$t('Pages.EnableAuthenticatorLine3')}}

                            <a href="https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2&amp;hl=en">Android</a>,
                            <a href="https://itunes.apple.com/us/app/google-authenticator/id388497605?mt=8">iOS</a>.
                        </p>
                    </li>
                    <li>
                        <p>{{$t('Pages.EnableAuthenticatorLine4')}}</p>
                        <p><b>{{$t('Pages.EnableAuthenticatorSharedKey')}} </b><kbd>{{sharedKey}}</kbd></p>
                        <canvas id="qrCode"></canvas>
                    </li>
                    <li>
                        <p>
                            {{$t('Pages.EnableAuthenticatorLine5')}}
                        </p>
                        <form>
                            <form-group
                                :label="$t('FieldsAndValidations.VerificationCodeFieldName')"
                                :error="modelState['VerificationCode']">
                                <TextInput
                                    v-model.trim="verificationCode"
                                    :haserror="modelState['VerificationCode'] !== undefined"
                                    id="VerificationCode"/>
                            </form-group>
                            <div class="block-filter">
                                <button
                                    type="submit"
                                    class="btn btn-success"

                                    id="btnVerify"
                                    v-bind:disabled="userInfo.isObserving"
                                    @click="verify">{{$t('Pages.Verify')}}</button>
                            </div>
                        </form>
                    </li>
                </ol>
            </div>
        </div>
    </ProfileLayout>
</template>

<script>
import Vue from 'vue'
import {each} from 'lodash'
import QRCode from 'qrcode'

export default {
    data() {
        return {
            modelState: {},
            personName: null,
            verificationCode: null,
        }
    },
    computed: {
        currentTab(){
            return 'two-factor'
        },
        sharedKey(){
            return this.userInfo.sharedKey
        },
        authenticatorUri(){
            return this.userInfo.authenticatorUri
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
    mounted() {
        QRCode.toCanvas(document.getElementById('qrCode'), this.authenticatorUri)
    },
    watch: {
        verificationCode: function(val) {
            Vue.delete(this.modelState, 'VerificationCode')
        },
    },
    methods: {
        verify: function(event) {
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }
            event.preventDefault()
            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.checkVerificationCodeUrl,
                data: {
                    userId: self.userInfo.userId,
                    personName: self.personName,
                    verificationCode: self.verificationCode == '' ? null : self.verificationCode,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    window.location.href = self.model.api.showRecoveryCodesUrl
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )
        },
        processModelState: function(response, vm) {
            if (response) {
                each(response, function(state) {
                    var message = ''
                    var stateErrors = state.value
                    if (stateErrors) {
                        each(stateErrors, function(stateError, j) {
                            if (j > 0) {
                                message += '; '
                            }
                            message += stateError
                        })
                        vm.$set(vm.modelState, state.key, message)
                    }
                })
            }
        },
    },
}
</script>
