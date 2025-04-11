<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="profile">
        <div class="row extra-margin-bottom">
            <div class="col-sm-9">
                <h2>{{ $t('Settings.UserProfileSettings_Title') }}</h2>
            </div>
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isAllowInterviewerUpdateProfile"
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
import { nextTick } from 'vue'
export default {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    computed: {
        isAllowInterviewerUpdateProfile: {
            get() {
                return this.modelValue
            },
            set(value) {
                this.$emit('update:modelValue', value)
            }
        }
    },
    methods: {
        updateAllowInterviewerUpdateProfile() {
            nextTick(() => {
                this.$hq.AdminSettings.setProfileSettings(this.isAllowInterviewerUpdateProfile)
            })
        },
    }
}
</script>