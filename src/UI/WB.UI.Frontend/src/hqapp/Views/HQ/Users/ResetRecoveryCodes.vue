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
            <h2>{{$t('Strings.HQ_Views_ResetRecoveryCodes_Title')}}</h2>
        </div>

        <div >
            <div class="alert alert-warning"
                role="alert">
                <p>
                    <span class="glyphicon glyphicon-warning-sign"
                        style="margin-right: 5px;"></span>
                    <strong>{{$t('Pages.RecoveryCodesInfo')}}</strong>
                </p>
                <p>
                    {{$t('Pages.RecoveryCodesDescription')}}
                </p>
                <p>
                    {{$t('Pages.RecoveryCodesDescriptionLine1')}}

                    {{$t('Pages.ChandgeKeyLine1')}} <a v-bind:href="getUrl('ResetAuthenticator')">{{$t('Pages.ChandgeKeyLine2')}}</a>

                </p>
            </div>
        </div>
        <div >
            <div class="block-filter">
                <button
                    type="submit"
                    class="btn btn-danger"
                    id="btnGenerateRecoveryCodes"
                    v-bind:disabled="userInfo.isObserving"
                    @click="generateRecoveryCodes">{{$t('Pages.GenerateRecoveryCodes')}}</button>
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
        recoveryCodes(){
            return this.userInfo.recoveryCodes
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
        generateRecoveryCodes(){
            this.successMessage = null
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.generateRecoveryCodesUrl,
                data: {
                    userId: self.userInfo.userId,
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
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return `./${baseUrl}`
            else
                return `../${baseUrl}/` + this.model.userInfo.userId
        },
    },
}
</script>
