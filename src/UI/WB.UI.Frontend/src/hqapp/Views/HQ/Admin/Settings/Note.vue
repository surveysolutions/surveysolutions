<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="note">
        <div class="row contain-input" data-suso="settings-page">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.GlobalNoteSettings') }}</h2>
                <p>{{ $t('Settings.GlobalNoteSettings_Description') }}</p>
            </div>
            <form class="col-sm-9">
                <div class="block-filter" style="padding-left: 30px">
                    <div class="form-group">
                        <label for="notificationText" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.GlobalNotice') }}
                        </label>
                        <textarea class="form-control" id="notificationText" type="text" v-model="notice"
                            maxlength="1000"></textarea>
                    </div>
                </div>
                <div class="block-filter" style="padding-left: 30px">
                    <button type="button" class="btn btn-success" @click="updateMessage">
                        {{ $t('Common.Save') }}
                    </button>
                    <button type="button" class="btn btn-link" @click="clearMessage">
                        {{ $t('Common.Delete') }}
                    </button>
                    <span class="text-success" v-if="globalNoticeUpdated">
                        {{ $t('Settings.GlobalNoteSaved') }}
                    </span>
                </div>
            </form>
        </div>
    </div>
</template>

<script>
import { nextTick } from 'vue'

export default {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    computed: {
        notice: {
            get() {
                return this.modelValue
            },
            set(value) {
                this.$emit('update:modelValue', value)
            }
        }
    },
    data() {
        return {
            globalNoticeUpdated: false,
        }
    },
    methods: {
        async updateMessage() {
            this.globalNoticeUpdated = false
            const response = await this.$hq.AdminSettings.setGlobalNotice(
                this.notice,
            )
            if (response.status === 200) {
                this.globalNoticeUpdated = true
            }
        },
        async clearMessage() {
            this.notice = ''
            nextTick(() => {
                this.updateMessage()
            })
        },
    }
}
</script>
