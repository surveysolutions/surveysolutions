<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :userName="userInfo.userName"
        :canChangePassword="userInfo.canChangePassword"
        :userId="userInfo.userId"
        :currentTab="currentTab">
        <div class="block-filter">
            <p>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Description')}}</p>
        </div>
        <div class="block-filter">
            <h3>
                {{$t('Pages.AccountManage_Status2fa')}}
                <span style="color:green;"
                    v-if="is2faEnabled"> {{$t('Strings.HQ_Views_TwoFactorAuthentication_Enabled')}}
                </span>
                <span style="color:red;"
                    v-if="!is2faEnabled"> {{$t('Strings.HQ_Views_TwoFactorAuthentication_Disabled')}}
                </span>
            </h3>
        </div>

        <div class="alert alert-warning"
            v-if="is2faEnabled && recoveryCodesLeft <= 3">
            <strong>{{$t('Pages.RecoveryCodesLeft')}} {{recoveryCodesLeft}}.</strong>
            <p>{{$t('Pages.RecoveryCodesYouCan')}} <a :href="getUrl('../../Users/GenerateRecoveryCodes')">{{$t('Pages.GenerateRecoveryCodesLink')}}</a>.</p>
        </div>

        <div class="row flex-row">

            <div class="col-sm-4">
                <div class="flex-block selection-box">
                    <div class="block">
                        <div class="block-filter">
                            <h3>{{$t('Pages.SetupAuthenticator')}}</h3>
                            <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_SetupAuthenticator_Description')}}</span>
                        </div>
                        <div >
                            <button id="enable-authenticator"
                                type="submit"
                                @click="navigateTo('../../Users/SetupAuthenticator')"
                                style="display: inline-block;"
                                v-bind:disabled="userInfo.isObserving"
                                class="btn btn-success">{{$t('Pages.SetupAuthenticator')}}</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-sm-4">
                <div class="flex-block selection-box">
                    <div class="block">
                        <div class="block-filter">
                            <h3>{{$t('Pages.ResetAuthenticator')}}</h3>
                            <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_ResetAuthenticator_Description')}}</span>
                        </div>
                        <div>
                            <button id="reset-authenticator"
                                type="submit"
                                @click="navigateTo('../../Users/ResetAuthenticator')"
                                style="display: inline-block;"
                                v-bind:disabled="userInfo.isObserving"
                                class="btn btn-success">{{$t('Pages.ResetAuthenticator')}}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-sm-4"
                v-if="is2faEnabled">
                <div class="flex-block selection-box">
                    <div class="block">
                        <div class="block-filter">
                            <h3>{{$t('Pages.ResetRecoveryCodes')}}</h3>
                            <span>{{$t('Strings.HQ_Views_TwoFactorAuthentication_ResetRecoveryCodes_Description')}}</span>
                        </div>
                        <div >
                            <button id="reset-codes"
                                type="submit"
                                @click="navigateTo('../../Users/ResetRecoveryCodes')"
                                style="display: inline-block;"
                                v-bind:disabled="userInfo.isObserving"
                                class="btn btn-success">{{$t('Pages.ResetRecoveryCodes')}} </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="is2faEnabled">
            <p>{{$t('Strings.HQ_Views_TwoFactorAuthentication_Disable2fa_Description')}}</p>
            <button
                id="disable-2fa"
                type="submit"
                @click="navigateTo('../../Users/Disable2fa')"
                class="btn btn-danger"
                v-bind:disabled="userInfo.isObserving">{{$t('Pages.Disable2fa')}}</button>
        </div >
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
        is2faEnabled(){
            return this.userInfo.is2faEnabled
        },
        recoveryCodesLeft(){
            return this.userInfo.recoveryCodesLeft
        },
        model() {
            return this.$config.model
        },
        userInfo() {
            return this.model.userInfo
        },
    },
    methods: {
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.model.userInfo.userId
        },
        navigateTo: function(location){
            window.location.href = this.getUrl(location)
            return false
        },
        navigate: function(target){
            this.$router.push({ name: target, params: {userId: this.isOwnProfile ? '' : this.model.userInfo.userId}}, () => this.$router.go(0))
        },
    },
}
</script>
