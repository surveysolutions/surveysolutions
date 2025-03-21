<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="devices">
        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-12">
                <h2>{{ $t('Settings.MobileAppSettings_Title') }}</h2>
                <p>{{ $t('Settings.MobileAppSettings_Description') }}</p>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="isInterviewerAutomaticUpdatesEnabled"
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
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="isDeviceNotificationsEnabled"
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
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="isPartialSynchronizationEnabled"
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
            <div class="col-sm-12">
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
                                <!-- <Field class="form-control number" v-model.number="geographyQuestionAccuracyInMeters"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 1,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" name="accuracy"
                                    label="Accuracy" id="interviewerGeographyQuestionAccuracyInMeters" type="number" /> -->
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
            <div class="col-sm-12">
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
                                <!-- <Field class="form-control number" v-model.number="geographyQuestionPeriodInSeconds"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 5,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" label="Period"
                                    id="interviewerGeographyQuestionPeriodInSeconds" name="period" type="number" /> -->
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

            <div class="col-sm-12">
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
                            <input class="form-control number" type="password" :value="esriApiKey" id="esriApiKey"
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
    </div>
</template>

<script>
import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    props: {
        isInterviewerAutomaticUpdatesEnabled: Boolean,
        isDeviceNotificationsEnabled: Boolean,
        isPartialSynchronizationEnabled: Boolean,
        geographyQuestionAccuracyInMeters: Number,
        geographyQuestionPeriodInSeconds: Number,
        esriApiKey: String,
    },
    components: {
        Form,
        Field,
        ErrorMessage,
    },

    data() {
        return {
            geographyQuestionAccuracyInMetersCancel: this.geographyQuestionAccuracyInMeters,
            geographyQuestionPeriodInSecondsCancel: this.geographyQuestionPeriodInSeconds,
            esriApiKeyInitial: this.esriApiKey,
        }
    },
    methods: {

        updateDeviceSettings() {
            return this.$hq.AdminSettings.setInterviewerSettings(
                this.isInterviewerAutomaticUpdatesEnabled,
                this.isDeviceNotificationsEnabled,
                this.isPartialSynchronizationEnabled,
            )
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
            this.geographyQuestionAccuracyInMeters = this.geographyQuestionAccuracyInMetersCancel
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
        async updateEsriKApiKey() {
            return this.$hq.AdminSettings.setEsriApiKey(
                this.esriApiKey,
            ).then(() => {
                this.esriApiKeyInitial = this.esriApiKey
            })
        },
        cancelEsriKApiKey() { this.esriApiKey = this.esriApiKeyInitial },
        noAction() {
            // Do nothing
        },
    }
}

</script>