<template>
    <div role="tabpanel" class="tab-pane active page-preview-block" id="export">
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.ExportEncryption_Title') }}</h2>
                <p>{{ $t('Settings.ExportEncryption_Description') }}</p>
            </div>
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="encryptionEnabledModel"
                            @change="changeEncryptionEnabled" id="isEnabled" type="checkbox" />
                        <label for="isEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EnableEncryption') }}
                        </label>
                    </div>
                </div>
                <div class="block-filter" style="padding-left: 30px">
                    <label for="exportPassword" style="font-weight: bold">
                        <span class="tick"></span>
                        {{ $t('Settings.Password') }}
                    </label>
                    <div class="form-group">
                        <div class="input-group">
                            <input id="exportPassword" type="text" v-model="encryptionPasswordModel" readonly="readonly"
                                class="form-control" />
                            <span class="input-group-btn">
                                <button class="btn btn-default" @click="regenPassword"
                                    :disabled="!encryptionEnabledModel">
                                    <i class="glyphicon glyphicon-refresh"></i>
                                </button>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.FileRetentionTitle') }}</h2>
                <p>{{ $t('Settings.FileRetentionDescription') }}</p>
            </div>
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isRetentionEnabledModel"
                            @change="changeRetentionEnabled" id="isRetentionEnabled" type="checkbox" />
                        <label for="isRetentionEnabled" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.EnableFileRetention') }}
                        </label>
                    </div>
                </div>

                <Form v-slot="{ meta }" @submit="noAction" ref="retentionForm">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="Days" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.RetentionInDaysTitle') }}
                                <p style="font-weight: normal; margin-bottom: 0px;">
                                    {{ $t('Settings.RetentionInDaysDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model="inDaysModel" :rules="{
                                    integer: true,
                                    required: false,
                                    min_value: 1,
                                    max_value: 1000
                                }" :validateOnChange="true" :validateOnInput="true" name="Days" label="Days"
                                    id="retentionInDays" type="number" :disabled="!isRetentionEnabledModel" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="inDaysModel ==
                                inDaysCancelModel ||
                                (inDaysModel !== '' && inDaysModel < 1) ||
                                (inDaysModel !== '' && inDaysModel > 1000) ||
                                meta.valid == false
                                " @click="updateInDays">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link" :disabled="inDaysModel == inDaysCancelModel"
                                @click="cancelInDays">
                                {{ $t('Common.Cancel') }}
                            </button>
                        </div>
                        <div class="error">
                            <ErrorMessage name="Days"></ErrorMessage>
                        </div>
                    </div>
                </Form>
            </div>
            <div class="col-sm-9">
                <Form v-slot="{ meta }" @submit="noAction" :disabled="!isRetentionEnabledModel">
                    <div class="block-filter" style="padding-left: 30px">
                        <div class="form-group">
                            <label for="Files" style="font-weight: bold">
                                <span class="tick"></span>
                                {{ $t('Settings.RetentionInCountTitle') }}
                                <p style="font-weight: normal;margin-bottom: 0px">
                                    {{ $t('Settings.RetentionInCountDescription') }}
                                </p>
                            </label>
                        </div>
                        <div class="form-group">
                            <div class="input-group input-group-save">
                                <Field class="form-control number" v-model="inCountLimitModel" :rules="{
                                    integer: true,
                                    required: false,
                                    min_value: 1,
                                    max_value: 100000,
                                }" :validateOnChange="true" :validateOnInput="true" label="Files" id="Files"
                                    name="Files" type="number" :disabled="!isRetentionEnabledModel" />
                            </div>
                            <button type="button" class="btn btn-success" :disabled="inCountLimitModel ==
                                inCountLimitCancelModel ||
                                (inCountLimitModel !== '' && inCountLimitModel < 1) ||
                                (inCountLimitModel !== '' && inCountLimitModel > 100000) ||
                                meta.valid == false" @click="updateInCountLimit">
                                {{ $t('Common.Save') }}
                            </button>
                            <button type="button" class="btn btn-link"
                                :disabled="inCountLimitModel == inCountLimitCancelModel" @click="cancelInCountLimit">
                                {{ $t('Common.Cancel') }}
                            </button>
                        </div>
                        <div class="error">
                            <ErrorMessage name="Files"></ErrorMessage>
                        </div>
                    </div>
                </Form>
            </div>
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <button type="button" class="btn btn-danger" @click="forceRunRetentionPolicy"
                        :disabled="!isRetentionEnabledModel" style="margin-right: 15px">
                        {{ $t('Settings.ForceRunRetentionPolicy') }}
                    </button>
                </div>
            </div>
        </div>
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.ClearExportCache_Title') }}</h2>
                <p>{{ $t('Settings.ClearExportCache_Description') }}</p>
            </div>
            <div class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
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
import modal from '@/shared/modal'
import { nextTick } from 'vue'

export default {
    props: {
        encryptionEnabled: Boolean,
        encryptionPassword: String,
        isRetentionEnabled: Boolean,
        retentionLimitInDays: Number,
        retentionLimitQuantity: Number,
        retentionLimitInDaysCancel: Number,
        retentionLimitQuantityCancel: Number,
    },
    emits: [
        'update:encryptionEnabled',
        'update:encryptionPassword',
        'update:isRetentionEnabled',
        'update:retentionLimitInDays',
        'update:retentionLimitQuantity',
        'update:retentionLimitInDaysCancel',
        'update:retentionLimitQuantityCancel',
    ],
    components: {
        Form,
        Field,
        ErrorMessage,
    },
    computed: {
        encryptionEnabledModel: {
            get() {
                return this.encryptionEnabled
            },
            set(value) {
                this.$emit('update:encryptionEnabled', value)
            }
        },
        encryptionPasswordModel: {
            get() {
                return this.encryptionPassword
            },
            set(value) {
                this.$emit('update:encryptionPassword', value)
            }
        },
        isRetentionEnabledModel: {
            get() {
                return this.isRetentionEnabled
            },
            set(value) {
                this.$emit('update:isRetentionEnabled', value)
            }
        },
        inDaysModel: {
            get() {
                return this.retentionLimitInDays
            },
            set(value) {
                this.$emit('update:retentionLimitInDays', value)
            }
        },
        inCountLimitModel: {
            get() {
                return this.retentionLimitQuantity
            },
            set(value) {
                this.$emit('update:retentionLimitQuantity', value)
            }
        },
        inDaysCancelModel: {
            get() {
                return this.retentionLimitInDaysCancel
            },
            set(value) {
                this.$emit('update:retentionLimitInDaysCancel', value)
            }
        },
        inCountLimitCancelModel: {
            get() {
                return this.retentionLimitQuantityCancel
            },
            set(value) {
                this.$emit('update:retentionLimitQuantityCancel', value)
            }
        },
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
                message: self.encryptionEnabledModel
                    ? self.$t('Settings.ChangeStateConfirm')
                    : self.$t('Settings.ChangeStateDisabledConfirm'),
                buttons: {
                    cancel: {
                        label: self.$t('Common.No'),
                        callback: () => {
                            self.encryptionEnabledModel = !self.encryptionEnabledModel
                        },
                    },
                    success: {
                        label: self.$t('Common.Yes'),
                        callback: async () => {
                            const response =
                                await self.$hq.ExportSettings.setEncryption(
                                    self.encryptionEnabledModel,
                                )
                            self.encryptionEnabledModel = response.data.isEnabled
                            self.encryptionPasswordModel = response.data.password
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
                            this.encryptionPasswordModel = response.data.password
                            this.encryptionEnabledModel = response.data.isEnabled
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
        forceRunRetentionPolicy() {
            var self = this
            modal.dialog({
                closeButton: true,
                onEscape: true,
                title:
                    '<h2>' + self.$t('Pages.ConfirmationNeededTitle') + '</h2>',
                message:
                    `<p style="color: red;"> ${self.$t(
                        'Settings.ForceRunRetentionPolicy_Warning',
                    )}</p>` +
                    `<p>${self.$t('Settings.ForceRunRetentionPolicy_Confirm')}</p>`,
                buttons: {
                    success: {
                        label: self.$t('Settings.ForceRunRetentionPolicy'),
                        className: 'btn btn-danger',
                        callback: async () => {
                            const status = await this.$hq.ExportSettings.forceRunRetentionPolicy()

                            if (response.status !== 200) {
                                if (
                                    e.response &&
                                    e.response.data &&
                                    e.response.data.error
                                ) {
                                    this.showAlert(e.response.data.error)
                                    return
                                } else {
                                    this.statusDropExportCache = 'Error'
                                    this.allowToRemoveExportCache = true;
                                    return
                                }
                            }
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
            modal.dialog({
                message: message,
                onEscape: false,
                closeButton: false,
                buttons: {
                    ok: {
                        label: this.$t('WebInterviewUI.Reload'),
                        className: 'btn-success',
                        callback: () => {
                            location.reload()
                        },
                    },
                },
            })
        },
        changeRetentionEnabled() {
            if (this.isRetentionEnabledModel) {
                var self = this
                modal.dialog({
                    closeButton: false,
                    message: self.$t('Settings.RetentionPolicyEneblingConfirm'),
                    buttons: {
                        cancel: {
                            label: self.$t('Common.No'),
                            callback: () => {
                                self.isRetentionEnabledModel = !self.isRetentionEnabledModel
                            },
                        },
                        success: {
                            label: self.$t('Common.Yes'),
                            callback: async () => {
                                this.$hq.ExportSettings.changeRetentionState(this.isRetentionEnabledModel)
                            },
                        },
                    },
                })
            }
            else {
                nextTick(() => {
                    this.$hq.ExportSettings.changeRetentionState(this.isRetentionEnabledModel)
                })
            }
        },
        updateInDays() {
            nextTick(() => {
                this.$hq.ExportSettings.setRetentionLimitInDays(this.inDaysModel)
                    .then(() => {
                        this.inDaysCancelModel = this.inDaysModel
                    })
            })
        },
        cancelInDays() {
            this.inDaysModel = this.inDaysCancelModel
        },
        updateInCountLimit() {
            nextTick(() => {
                this.$hq.ExportSettings.setRetentionLimitCount(this.inCountLimitModel)
                    .then(() => {
                        this.inCountLimitCancelModel = this.inCountLimitModel
                    })
            })
        },
        cancelInCountLimit() {
            this.inCountLimitModel = this.inCountLimitCancelModel
        },
        noAction() {
            // Do nothing
        },
    }
}
</script>