<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="profile">
        <div class="row extra-margin-bottom contain-input">
            <div class="col-sm-12">
                <h2>{{ $t('Settings.UserProfileSettings_Title') }}</h2>
            </div>
            <div class="col-sm-12">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" :value="isAllowInterviewerUpdateProfile"
                            @change="updateAllowInterviewerUpdateProfile" id="allowInterviewerUpdateProfile"
                            type="checkbox" />
                        <label for="allowInterviewerUpdateProfile" style="font-weight: bold">
                            <span class="tick"></span>
                            {{ $t('Settings.AllowInterviewerUpdateProfile') }}
                            <p style="font-weight: normal">
                                {{
                                    $t(
                                        'Settings.AllowInterviewerUpdateProfileDesc',
                                    )
                                }}
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
        isAllowInterviewerUpdateProfile: Boolean
    },
    methods: {

        updateAllowInterviewerUpdateProfile() {
            this.$hq.AdminSettings.setProfileSettings(
                this.isAllowInterviewerUpdateProfile,
            )
        },
    }
}
</script>