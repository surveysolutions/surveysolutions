<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :forceChangePassword="userInfo.forceChangePassword"
        :canChangePassword="userInfo.canChangePassword"
        :userName="userInfo.userName"
        :userId="userInfo.userId"
        :currentTab="currentTab"
        :canGenerateToken="userInfo.canGetApiToken">

        <div>
            <div class="block-filter">
                <h3>
                    {{$t('Pages.AccountManage_StatusApiToken')}}
                    <span style="color:green;"
                        v-if="tokenWasIssued"> {{$t('Strings.HQ_Views_Api_Token_Issued')}}
                    </span>
                    <span style="color:red;"
                        v-if="!tokenWasIssued"> {{$t('Strings.HQ_Views_Api_Token_Not_Issued')}}
                    </span>
                </h3>
            </div>
        </div>
        <div v-if="apiToken && tokenWasIssued"
            class="row">
            <div class="col-md-6 col-xs-12">
                <span style="color:red;">{{$t('Strings.HQ_Views_Api_Token_Generate_Description')}}</span>
                <pre v-text="apiToken"
                    style="white-space:normal;"></pre>
            </div>
        </div>
        <div>
            <div class="block-filter">
                <button
                    v-if="tokenWasIssued"
                    type="submit"
                    class="btn btn-danger"
                    id="btnDelete"
                    v-bind:disabled="userInfo.isObserving || !canGenerate"
                    @click="deleteToken">{{$t('Common.Disable')}}</button>
                <button
                    v-if="!tokenWasIssued"
                    type="submit"
                    class="btn btn-success"
                    style="margin-right:5px"
                    id="btnCreateToken"
                    v-bind:disabled="userInfo.isObserving && !canGenerate"
                    @click="generateApiKey">{{$t('Common.Enable')}}</button>
            </div>
        </div>

    </ProfileLayout>
</template>

<script>
import {each} from 'lodash'

export default {
    data() {
        return {
            modelState: {},
            apiToken: null,
            tokenWasIssued: false,
        }
    },
    mounted() {
        this.tokenWasIssued = this.userInfo.tokenIssued
    },
    computed: {
        currentTab(){
            return 'api-token'
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
        canChangePassword() {
            if(this.userInfo.isObserving)
                return false

            return true
        },
        referrerUrl() {
            return '/'
        },
        canGenerate(){
            return this.userInfo.canGetApiToken
        },
    },
    methods: {
        generateApiKey: function(event) {
            for (var error in this.modelState) {
                delete this.modelState[error]
            }

            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.generateApiKeyUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    self.apiToken = response.data
                    self.tokenWasIssued = true
                },
                error => {
                    self.processModelState(error.response.data, self)
                }
            )
        },
        deleteToken(){
            var self = this
            this.$http({
                method: 'post',
                url: this.model.api.deleteApiKeyUrl,
                data: {
                    userId: self.userInfo.userId,
                },
                headers: {
                    'X-CSRF-TOKEN': this.$hq.Util.getCsrfCookie(),
                },
            }).then(
                response => {
                    self.tokenWasIssued = false
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