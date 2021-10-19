<template>
    <ProfileLayout ref="profile"
        :role="userInfo.role"
        :isOwnProfile="userInfo.isOwnProfile"
        :forceChangePassword="userInfo.forceChangePassword"
        :canChangePassword="userInfo.canChangePassword"
        :userName="userInfo.userName"
        :userId="userInfo.userId"
        :currentTab="currentTab">

        <div>
            <h4>
                {{$t('Users.ApiTokenTitle')}}
            </h4>

            <div>
                <div class="block-filter">
                    <button
                        type="submit"
                        class="btn btn-success"
                        style="margin-right:5px"
                        id="btnCreateToken"
                        v-bind:disabled="userInfo.isObserving || !canGenerate"
                        @click="generateApiKey">{{$t('Pages.Create')}}</button>

                </div>
            </div>

            <div>
                <pre v-text="apiToken"
                    style="white-space:normal;">
                </pre>
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
            apiToken: null,
        }
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