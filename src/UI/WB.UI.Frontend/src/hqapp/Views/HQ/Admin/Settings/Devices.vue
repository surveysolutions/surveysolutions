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
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox"
                            v-model="allowSupervisorChangeAssignmentStatusModel" @change="updateDeviceSettings"
                            id="allowSupervisorChangeAssignmentStatus" type="checkbox" />
                        <label for="allowSupervisorChangeAssignmentStatus" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowSupervisorChangeAssignmentStatus') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.AllowSupervisorChangeAssignmentStatusDescription') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox"
                            v-model="allowInterviewerChangeAssignmentStatusModel" @change="updateDeviceSettings"
                            id="allowInterviewerChangeAssignmentStatus" type="checkbox"
                            :disabled="!allowSupervisorChangeAssignmentStatusModel" />
                        <label for="allowInterviewerChangeAssignmentStatus" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowInterviewerChangeAssignmentStatus') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.AllowInterviewerChangeAssignmentStatusDescription') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="audioRecordingQuality" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AudioRecordingQuality') }}
                            <p style="font-weight: normal;margin-bottom: 0px">
                                {{ $t('Settings.AudioRecordingQualityDescription') }}
                            </p>
                        </label>
                    </div>
                    <div class="form-group">
                        <Typeahead control-id="audioRecordingQuality" noSearch noClear
                            :values="audioRecordingQualityOptions" :value="audioRecordingQualityValue"
                            @selected="onAudioRecordingQualitySelected" />
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
        allowSupervisorChangeAssignmentStatus: Boolean,
        allowInterviewerChangeAssignmentStatus: Boolean,
        audioRecordingQuality: String,
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
        'update:allowSupervisorChangeAssignmentStatus',
        'update:allowInterviewerChangeAssignmentStatus',
        'update:audioRecordingQuality',
    ],
    computed: {
        isInterviewerAutomaticUpdatesEnabledModel: {
            get() {
                return this.isInterviewerAutomaticUpdatesEnabled
            },
            set(value) {
                this.$emit('update:isInterviewerAutomaticUpdatesEnabled', value)
            },
        },
        isDeviceNotificationsEnabledModel: {
            get() {
                return this.isDeviceNotificationsEnabled
            },
            set(value) {
                this.$emit('update:isDeviceNotificationsEnabled', value)
            },
        },
        isPartialSynchronizationEnabledModel: {
            get() {
                return this.isPartialSynchronizationEnabled
            },
            set(value) {
                this.$emit('update:isPartialSynchronizationEnabled', value)
            },
        },

        geographyQuestionAccuracyInMetersModel: {
            get() {
                return this.geographyQuestionAccuracyInMeters
            },
            set(value) {
                this.$emit('update:geographyQuestionAccuracyInMeters', value)
            },
        },
        geographyQuestionAccuracyInMetersCancelModel: {
            get() {
                return this.geographyQuestionAccuracyInMetersCancel
            },
            set(value) {
                this.$emit('update:geographyQuestionAccuracyInMetersCancel', value)
            },
        },

        geographyQuestionPeriodInSecondsModel: {
            get() {
                return this.geographyQuestionPeriodInSeconds
            },
            set(value) {
                this.$emit('update:geographyQuestionPeriodInSeconds', value)
            },
        },
        geographyQuestionPeriodInSecondsCancelModel: {
            get() {
                return this.geographyQuestionPeriodInSecondsCancel
            },
            set(value) {
                this.$emit('update:geographyQuestionPeriodInSecondsCancel', value)
            },
        },
        esriApiKeyModel: {
            get() {
                return this.esriApiKey
            },
            set(value) {
                this.$emit('update:esriApiKey', value)
            },
        },
        esriApiKeyInitialModel: {
            get() {
                return this.esriApiKeyInitial
            },
            set(value) {
                this.$emit('update:esriApiKeyInitial', value)
            },
        },
        allowSupervisorChangeAssignmentStatusModel: {
            get() {
                return this.allowSupervisorChangeAssignmentStatus
            },
            set(value) {
                this.$emit('update:allowSupervisorChangeAssignmentStatus', value)
            },
        },
        allowInterviewerChangeAssignmentStatusModel: {
            get() {
                return this.allowInterviewerChangeAssignmentStatus
            },
            set(value) {
                this.$emit('update:allowInterviewerChangeAssignmentStatus', value)
            },
        },
        audioRecordingQualityModel: {
            get() {
                return this.audioRecordingQuality
            },
            set(value) {
                this.$emit('update:audioRecordingQuality', value)
            },
        },
        audioRecordingQualityOptions() {
            return [
                { key: 'Mono16kHz', value: this.$t('Settings.AudioRecordingQuality_Mono16kHz') },
                { key: 'Mono22kHz', value: this.$t('Settings.AudioRecordingQuality_Mono22kHz') },
                { key: 'Mono44kHz', value: this.$t('Settings.AudioRecordingQuality_Mono44kHz') },
                { key: 'Stereo44kHz', value: this.$t('Settings.AudioRecordingQuality_Stereo44kHz') },
                { key: 'Stereo48kHz', value: this.$t('Settings.AudioRecordingQuality_Stereo48kHz') },
            ]
        },
        audioRecordingQualityValue() {
            return this.audioRecordingQualityOptions.find(o => o.key === this.audioRecordingQuality) || null
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
                    this.allowSupervisorChangeAssignmentStatusModel,
                    this.allowInterviewerChangeAssignmentStatusModel,
                    this.audioRecordingQualityModel
                )
            })
        },

        onAudioRecordingQualitySelected(item) {
            if (item != null) {
                this.audioRecordingQualityModel = item.key
                this.updateDeviceSettings()
            }
        },

        async updateGeographyQuestionAccuracyInMeters() {
            if (this.geographyQuestionAccuracyInMetersModel < 1 && this.geographyQuestionAccuracyInMetersModel > 1000)
                this.geographyQuestionAccuracyInMetersModel = this.geographyQuestionAccuracyInMetersCancelModel
            else
                nextTick(() => {
                    this.$hq.AdminSettings.setGeographyQuestionAccuracyInMeters(
                        this.geographyQuestionAccuracyInMetersModel
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
                        this.geographyQuestionPeriodInSecondsModel
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
                    this.esriApiKeyModel
                ).then(() => {
                    this.esriApiKeyInitialModel = this.esriApiKeyModel
                })
            })
        },
        cancelEsriApiKey() { this.esriApiKeyModel = this.esriApiKeyInitialModel },
        noAction() {
            // Do nothing
        },
    },
}

</script>