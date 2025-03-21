<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="export">
        <div class="row extra-margin-bottom contain-input " data-suso="settings-page">
            <div class="col-sm-12">
                <h2>{{ $t('Settings.ExportEncryption_Title') }}</h2>
                <p>{{ $t('Settings.ExportEncryption_Description') }}</p>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="encryptionEnabled"
                            @change="changeEncryptionEnabled" id="isEnabled" type="checkbox" />
                        <label for="isEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EnableEncryption') }}
                        </label>
                    </div>
                </div>
                <div class="block-filter">
                    <label for="exportPassword">
                        {{ $t('Settings.Password') }}:
                    </label>
                    <div class="form-group">
                        <div class="input-group">
                            <input id="exportPassword" type="text" :value="encryptionPassword" readonly="readonly"
                                class="form-control" />
                            <span class="input-group-btn">
                                <button class="btn btn-default" @click="regenPassword" :disabled="!encryptionEnabled">
                                    <i class="glyphicon glyphicon-refresh"></i>
                                </button>
                            </span>
                        </div>
                    </div>
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

        <div class="row extra-margin-bottom contain-input" data-suso="settings-page">
            <div class="col-sm-7">
                <h2>{{ $t('Settings.ClearExportCache_Title') }}</h2>
                <p>{{ $t('Settings.ClearExportCache_Description') }}</p>
            </div>
            <div class="col-sm-7">
                <div class="block-filter action-block">
                    <button type="button" class="btn btn-danger" @click="removeExportCache"
                        :disabled="!allowToRemoveExportCache" style="margin-right: 15px">
                        {{ $t('Settings.RemoveExportCache') }}
                    </button>
                    <span v-if="!allowToRemoveExportCache && statusDropExportCache == 'Removing'" style="color:blue">
                        {{ $t('Settings.RemovingExportCache') + ".".repeat(dropSchemaDots) }}
                    </span>
                    <span v-if="statusDropExportCache == 'Removed'" style="color:green">
                        {{ $t('Settings.RemoveExportCacheSuccess') }}
                    </span>
                    <div v-if="statusDropExportCache == 'Error'">
                        <br />
                        <span style="color:red"
                            v-dompurify-html="$t('Settings.RemoveExportCacheFail').replaceAll('\n', '<br/>')">
                        </span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { Form, Field, ErrorMessage } from 'vee-validate'
import modal from '@/shared/modal'

export default {
    props: {
        encryptionEnabled: Boolean,
        encryptionPassword: String,
        isRetentionEnabled: Boolean,
        retentionLimitInDays: Number,
        retentionLimitQuantity: Number,
    },
    components: {
        Form,
        Field,
        ErrorMessage,
    },
    data() {
        return {
            allowToRemoveExportCache: true,
            statusDropExportCache: '',
            dropSchemaTimer: null,
            dropSchemaDots: 0,
        }
    },
    methods: {
        changeEncryptionEnabled() {
            var self = this
            modal.dialog({
                closeButton: false,
                message: self.encryptionEnabled
                    ? self.$t('Settings.ChangeStateConfirm')
                    : self.$t('Settings.ChangeStateDisabledConfirm'),
                buttons: {
                    cancel: {
                        label: self.$t('Common.No'),
                        callback: () => {
                            self.encryptionEnabled = !self.encryptionEnabled
                        },
                    },
                    success: {
                        label: self.$t('Common.Yes'),
                        callback: async () => {
                            const response =
                                await self.$hq.ExportSettings.setEncryption(
                                    self.encryptionEnabled,
                                )
                            self.encryptionEnabled = response.data.isEnabled
                            self.encryptionPassword = response.data.password
                        },
                    },
                },
            })
        },
        regenPassword() {
            const self = this
            modal.dialog({
                closeButton: false,
                message: self.$t('Settings.RegeneratePasswordConfirm'),
                buttons: {
                    cancel: {
                        label: self.$t('Common.No'),
                    },

                    success: {
                        label: self.$t('Common.Yes'),
                        callback: async () => {
                            const response =
                                await this.$hq.ExportSettings.regenPassword()
                            this.encryptionPassword = response.data.password
                            this.encryptionEnabled = response.data.isEnabled
                        },
                    },
                },
            })
        },
        removeExportCache() {
            var self = this
            modal.dialog({
                closeButton: true,
                onEscape: true,
                title:
                    '<h2>' + self.$t('Pages.ConfirmationNeededTitle') + '</h2>',
                message:
                    `<p style="color: red;"> ${self.$t(
                        'Settings.RemoveExportCache_Warning',
                    )}</p>` +
                    `<p>${self.$t('Settings.RemoveExportCacheConfirm')}</p>`,
                buttons: {
                    success: {
                        label: self.$t('Common.Clear'),
                        className: 'btn btn-danger',
                        callback: async () => {
                            this.allowToRemoveExportCache = false
                            this.statusDropExportCache = 'Removing'
                            await this.runDropExportSchema()
                        },
                    },
                    cancel: {
                        label: self.$t('Common.Cancel'),
                        className: 'btn btn-link',
                        callback: () => { },
                    },
                },
            })
        },
        async runDropExportSchema() {
            const status = await this.$hq.ExportSettings.statusDropExportCache()
            if (status.data.status != 'Removing')
                await this.$hq.ExportSettings.dropExportCache()
                    .then((response) => {
                        const success = response.data.success

                        this.dropSchemaTimer = setInterval(async () => await this.checkStatusDropExportCache(), 1000);
                    })
                    .catch((e) => {
                        if (
                            e.response &&
                            e.response.data &&
                            e.response.data.message
                        ) {
                            this.showAlert(e.response.data.message)
                            return
                        } else {
                            this.statusDropExportCache = 'Error'
                            this.allowToRemoveExportCache = true;
                            return
                        }
                    })
        },

        async checkStatusDropExportCache() {
            const status = await this.$hq.ExportSettings.statusDropExportCache()
            this.statusDropExportCache = status.data.status

            if (this.dropSchemaDots >= 3)
                this.dropSchemaDots = 1;
            else
                this.dropSchemaDots++;

            if (this.statusDropExportCache != 'Removing' && this.dropSchemaTimer != null) {
                clearInterval(this.dropSchemaTimer)
                this.dropSchemaTimer = null
                this.allowToRemoveExportCache = true;
            }
        },
        showAlert(message) {
            modal.alert({
                message: message,
                callback: () => {
                    location.reload()
                },
                onEscape: false,
                closeButton: false,
                buttons: {
                    ok: {
                        label: this.$t('WebInterviewUI.Reload'),
                        className: 'btn-success',
                    },
                },
            })
        },
    }
}
</script>