<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="note">
        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-12">
                <h2>{{ $t('Settings.GlobalNoteSettings') }}</h2>
                <p>{{ $t('Settings.GlobalNoteSettings_Description') }}</p>
            </div>
            <form class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <label for="notificationText">
                            {{ $t('Settings.GlobalNotice') }}:
                        </label>
                        <textarea class="form-control" id="notificationText" type="text" v-model="noticeModel"
                            maxlength="1000"></textarea>
                    </div>
                </div>
                <div class="block-filter">
                    <button type="button" class="btn btn-success" @click="updateMessage">
                        {{ $t('Common.Save') }}
                    </button>
                    <button type="button" class="btn btn-link" @click="clearMessage">
                        {{ $t('Common.Delete') }}
                    </button>
                    <span class="text-success" v-if="globalNoticeUpdated">{{
                        $t('Settings.GlobalNoteSaved')
                    }}</span>
                </div>
            </form>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        globalNotice: String
    },
    data() {
        return {
            globalNoticeUpdated: false,
            noticeModel: this.globalNotice,
        }
    },

    methods: {
        async updateMessage() {
            const response = await this.$hq.AdminSettings.setGlobalNotice(
                this.noticeModel,
            )
            if (response.status === 200) {
                this.globalNoticeUpdated = true
            }
        },
        async clearMessage() {
            this.globalNotice = ''
            return this.updateMessage()
        },
    }
}
</script>
