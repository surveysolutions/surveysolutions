<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :userName="userInfo.userName"
        :canChangePassword="userInfo.canChangePassword"
        :userId="userInfo.userId"
        :currentTab="currentTab">
        <div >
            <h2>{{$t('Strings.HQ_Views_ShowRecoveryCodes_Title')}}</h2>
        </div>
        <div>
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
            </div>
        </div>
        <div>
            <ul>
                <li v-for="code in recoveryCodes"
                    :key="code">
                    <code  class="recovery-code">{{code}}</code>
                </li>
            </ul>
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
        recoveryCodes(){
            return this.userInfo.recoveryCodes.split(' ')
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
}
</script>
