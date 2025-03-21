<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="webinterview">
        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-12">
                <h2>
                    {{ $t('Settings.WebInterviewEmailNotifications_Title') }}
                </h2>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="isEmailAllowed"
                            @change="updateWebInterviewEmailNotifications" id="allowWebInterviewEmailNotifications"
                            type="checkbox" />
                        <label for="allowWebInterviewEmailNotifications" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowWebInterviewEmailNotifications') }}
                            <p style="font-weight: normal">
                                {{ $t('Settings.AllowWebInterviewEmailNotificationsDesc') }}
                            </p>
                        </label>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        isEmailAllowed: Boolean
    },
    methods: {

        updateWebInterviewEmailNotifications() {
            return this.$hq.AdminSettings.setWebInterviewSettings(
                this.isEmailAllowed,
            )
        },
    }
}

</script>