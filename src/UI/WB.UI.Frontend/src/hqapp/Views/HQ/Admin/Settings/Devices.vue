<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="devices">
        <div class="row contain-input">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.MobileAppSettings_Title') }}</h2>
                <p>{{ $t('Settings.MobileAppSettings_Description') }}</p>
            </div>
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox"
                            v-model="isInterviewerAutomaticUpdatesEnabledModel" @change="updateDeviceSettings"
                            id="interviewerAutomaticUpdatesEnabled" type="checkbox" />
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
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isDeviceNotificationsEnabledModel"
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
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isPartialSynchronizationEnabledModel"
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
            <div class="col-sm-9">
                <Form v-slot="{ meta }" @submit="noAction" :data-vv-scope="'geographyQuestion'">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="interviewerGeographyQuestionAccuracyInMeters" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.InterviewerGeographyQuestionAccuracyInMeters') }}
                                <p style="font-weight: normal;margin-bottom: 0px">
                                    {{ $t('Settings.GeographyQuestionAccuracyInMetersDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model="geographyQuestionAccuracyInMetersModel"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 1,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" name="accuracy"
                                    label="Accuracy" id="interviewerGeographyQuestionAccuracyInMeters" type="number"
                                    onkeypress="return (event.charCode != 8 && event.charCode == 0 || (event.charCode >= 48 && event.charCode <= 57))" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="geographyQuestionAccuracyInMetersModel ==
                                geographyQuestionAccuracyInMetersCancelModel ||
                                geographyQuestionAccuracyInMetersModel < 1 ||
                                geographyQuestionAccuracyInMetersModel > 1000 ||
                                meta.valid == false
                                " @click="updateGeographyQuestionAccuracyInMeters">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link"
                                :disabled="geographyQuestionAccuracyInMetersModel == geographyQuestionAccuracyInMetersCancelModel"
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
            <div class="col-sm-9">
                <Form v-slot="{ meta }" @submit="noAction">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="interviewerGeographyQuestionPeriodInSeconds" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.InterviewerGeographyQuestionPeriodInSeconds') }}
                                <p style="font-weight: normal;margin-bottom: 0px">
                                    {{ $t('Settings.GeographyQuestionPeriodInSecondsDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model="geographyQuestionPeriodInSecondsModel"
                                    :rules="{
                                        integer: true,
                                        required: true,
                                        min_value: 5,
                                        max_value: 1000,
                                    }" :validateOnChange="true" :validateOnInput="true" label="Period"
                                    id="interviewerGeographyQuestionPeriodInSeconds" name="period" type="number"
                                    onkeypress="return (event.charCode != 8 && event.charCode == 0 || (event.charCode >= 48 && event.charCode <= 57))" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="geographyQuestionPeriodInSecondsModel ==
                                geographyQuestionPeriodInSecondsCancelModel ||
                                geographyQuestionPeriodInSecondsModel < 5 ||
                                geographyQuestionPeriodInSecondsModel > 1000 ||
                                meta.valid == false" @click="updateGeographyQuestionPeriodInSeconds">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link"
                                :disabled="geographyQuestionPeriodInSecondsModel == geographyQuestionPeriodInSecondsCancelModel"
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

            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="esriApiKey" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EsriApiKey') }}
                            <p class="error" style="font-weight: normal;margin-bottom: 0px">
                                {{ $t('Settings.EsriApiKeyDescription') }}
                            </p>
                        </label>
                    </div>
                    <div class="form-group">
                        <div class="input-group input-group-save">
                            <input class="form-control number" type="password" v-model="esriApiKeyModel" id="esriApiKey"
                                name="esriKey" />
                        </div>
                        <button type="button" class="btn btn-success" :disabled="esriApiKeyModel ==
                            esriApiKeyInitialModel" @click="updateEsriApiKey">
                            {{ $t('Common.Save') }}
                        </button>
                        <button type="button" class="btn btn-link" :disabled="esriApiKeyModel ==
                            esriApiKeyInitialModel" @click="cancelEsriApiKey">
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
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="googleAndroidApiKey" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.GoogleAndroidApiKey') }}
                            <p class="error" style="font-weight: normal;margin-bottom: 0px">
                                {{ $t('Settings.GoogleAndroidApiKeyDescription') }}
                            </p>
                        </label>
                    </div>
                    <div class="form-group">
                        <div class="input-group input-group-save">
                            <input class="form-control number" type="password" v-model="googleAndroidApiKeyModel" id="googleAndroidApiKey" name="googleKey" />
                        </div>
                        <button type="button" class="btn btn-success" :disabled="googleAndroidApiKeyModel ==
                            googleAndroidApiKeyInitialModel" @click="updateGoogleAndroidApiKey">
                            {{ $t('Common.Save') }}
                        </button>
                        <button type="button" class="btn btn-link" :disabled="googleAndroidApiKeyModel ==
                            googleAndroidApiKeyInitialModel" @click="cancelGoogleAndroidApiKey">
                            {{ $t('Common.Cancel') }}
                        </button>
                    </div>
                </div>
                <div class="block-filter" style="padding-left: 30px">
                    <input id="ShowGoogleKey" type="checkbox"
                        onclick="var pass = document.getElementById('googleAndroidApiKey');pass.type = (pass.type === 'text' ? 'password' : 'text');">
                    <label for="ShowGoogleKey" style="padding-left:5px;">
                        <span></span>{{ $t('Pages.ShowKey') }}
                    </label>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.logo {
    max-width: 365px;
    max-height: 329px;
}

.input-group .form-control.number {
    border: 1px solid #ccc;
    border-radius: 4px;
}

.input-group .form-control.number[aria-invalid='true'] {
    color: red;
}

.form-group .btn-success {
    margin-left: 20px;
    margin-top: 2px;
}

.form-group .input-group-save {
    display: inline;
}

.block-filter .error {
    color: red;
}
</style>

<script>
import { Form, Field, ErrorMessage } from 'vee-validate'
import { nextTick } from 'vue'

export default {
    props: {
        isInterviewerAutomaticUpdatesEnabled: Boolean,
        isDeviceNotificationsEnabled: Boolean,
        isPartialSynchronizationEnabled: Boolean,
        geographyQuestionAccuracyInMeters: Number,
        geographyQuestionAccuracyInMetersCancel: Number,
        geographyQuestionPeriodInSeconds: Number,
        geographyQuestionPeriodInSecondsCancel: Number,
        esriApiKey: String,
        esriApiKeyInitial: String,
        googleAndroidApiKey: String,
        googleAndroidApiKeyInitial: String,
    },
    emits: ['update:isInterviewerAutomaticUpdatesEnabled',
        'update:isDeviceNotificationsEnabled',
        'update:isPartialSynchronizationEnabled',
        'update:geographyQuestionAccuracyInMeters',
        'update:geographyQuestionPeriodInSeconds',
        'update:geographyQuestionAccuracyInMetersCancel',
        'update:geographyQuestionPeriodInSecondsCancel',
        'update:esriApiKey',
        'update:esriApiKeyInitial',
        'update:googleAndroidApiKey',
        'update:googleAndroidApiKeyInitial',
    ],
    computed: {
        isInterviewerAutomaticUpdatesEnabledModel: {
            get() {
                return this.isInterviewerAutomaticUpdatesEnabled
            },
            set(value) {
                this.$emit('update:isInterviewerAutomaticUpdatesEnabled', value)
            }
        },
        isDeviceNotificationsEnabledModel: {
            get() {
                return this.isDeviceNotificationsEnabled
            },
            set(value) {
                this.$emit('update:isDeviceNotificationsEnabled', value)
            }
        },
        isPartialSynchronizationEnabledModel: {
            get() {
                return this.isPartialSynchronizationEnabled
            },
            set(value) {
                this.$emit('update:isPartialSynchronizationEnabled', value)
            }
        },

        geographyQuestionAccuracyInMetersModel: {
            get() {
                return this.geographyQuestionAccuracyInMeters
            },
            set(value) {
                this.$emit('update:geographyQuestionAccuracyInMeters', value)
            }
        },
        geographyQuestionAccuracyInMetersCancelModel: {
            get() {
                return this.geographyQuestionAccuracyInMetersCancel
            },
            set(value) {
                this.$emit('update:geographyQuestionAccuracyInMetersCancel', value)
            }
        },

        geographyQuestionPeriodInSecondsModel: {
            get() {
                return this.geographyQuestionPeriodInSeconds
            },
            set(value) {
                this.$emit('update:geographyQuestionPeriodInSeconds', value)
            }
        },
        geographyQuestionPeriodInSecondsCancelModel: {
            get() {
                return this.geographyQuestionPeriodInSecondsCancel
            },
            set(value) {
                this.$emit('update:geographyQuestionPeriodInSecondsCancel', value)
            }
        },
        esriApiKeyModel: {
            get() {
                return this.esriApiKey
            },
            set(value) {
                this.$emit('update:esriApiKey', value)
            }
        },
        esriApiKeyInitialModel: {
            get() {
                return this.esriApiKeyInitial
            },
            set(value) {
                this.$emit('update:esriApiKeyInitial', value)
            }
        },
        googleAndroidApiKeyModel: {
            get() {
                return this.googleAndroidApiKey
            },
            set(value) {
                this.$emit('update:googleAndroidApiKey', value)
            }
        },
        googleAndroidApiKeyInitialModel: {
            get() {
                return this.googleAndroidApiKeyInitial
            },
            set(value) {
                this.$emit('update:googleAndroidApiKeyInitial', value)
            }
        },
    },

    components: {
        Form,
        Field,
        ErrorMessage,
    },

    methods: {

        updateDeviceSettings() {
            nextTick(() => {
                this.$hq.AdminSettings.setInterviewerSettings(
                    this.isInterviewerAutomaticUpdatesEnabledModel,
                    this.isDeviceNotificationsEnabledModel,
                    this.isPartialSynchronizationEnabledModel,
                )
            })
        },

        async updateGeographyQuestionAccuracyInMeters() {
            if (this.geographyQuestionAccuracyInMetersModel < 1 && this.geographyQuestionAccuracyInMetersModel > 1000)
                this.geographyQuestionAccuracyInMetersModel = this.geographyQuestionAccuracyInMetersCancelModel
            else
                nextTick(() => {
                    this.$hq.AdminSettings.setGeographyQuestionAccuracyInMeters(
                        this.geographyQuestionAccuracyInMetersModel,
                    ).then(() => {
                        this.geographyQuestionAccuracyInMetersCancelModel =
                            this.geographyQuestionAccuracyInMetersModel
                    })
                })
        },
        cancelGeographyQuestionAccuracyInMeters() {
            this.geographyQuestionAccuracyInMetersModel = this.geographyQuestionAccuracyInMetersCancelModel
        },

        async updateGeographyQuestionPeriodInSeconds() {
            if (this.geographyQuestionPeriodInSecondsModel < 5 && this.geographyQuestionPeriodInSecondsModel > 1000)
                this.geographyQuestionPeriodInSecondsModel = this.geographyQuestionPeriodInSecondsCancelModel
            else
                nextTick(() => {
                    this.$hq.AdminSettings.setGeographyQuestionPeriodInSeconds(
                        this.geographyQuestionPeriodInSecondsModel,
                    ).then(() => {
                        this.geographyQuestionPeriodInSecondsCancelModel =
                            this.geographyQuestionPeriodInSecondsModel
                    })
                })
        },

        cancelGeographyQuestionPeriodInSeconds() {
            this.geographyQuestionPeriodInSecondsModel = this.geographyQuestionPeriodInSecondsCancelModel
        },
        async updateEsriApiKey() {
            nextTick(() => {
                return this.$hq.AdminSettings.setEsriApiKey(
                    this.esriApiKeyModel,
                ).then(() => {
                    this.esriApiKeyInitialModel = this.esriApiKeyModel
                })
            })
        },
        cancelEsriApiKey() { this.esriApiKeyModel = this.esriApiKeyInitialModel },
        async updateGoogleAndroidApiKey() {
            nextTick(() => {
                return this.$hq.AdminSettings.setGoogleAndroidApiKey(
                    this.googleAndroidApiKeyModel,
                ).then(() => {
                    this.googleAndroidApiKeyInitialModel = this.googleAndroidApiKeyModel
                })
            })
        },
        cancelGoogleAndroidApiKey() { this.googleAndroidApiKeyModel = this.googleAndroidApiKeyInitialModel },
        noAction() {
            // Do nothing
        },
    }
}

</script>