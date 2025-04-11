<template>
    <div role="tabpanel" class="tab-pane page-preview-block" id="webinterview">
        <div class="row extra-margin-bottom">
            <div class="col-sm-9">
                <h2>
                    {{ $t('Settings.WebInterviewEmailNotifications_Title') }}
                </h2>
            </div>
            <div class="col-sm-9">
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter single-checkbox" v-model="isEmailAllowed"
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
import { nextTick } from 'vue'
export default {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    computed: {
        isEmailAllowed: {
            get() {
                return this.modelValue
            },
            set(value) {
                this.$emit('update:modelValue', value)
            }
        }
    },
    methods: {
        updateWebInterviewEmailNotifications() {
            nextTick(() => {
                this.$hq.AdminSettings.setWebInterviewSettings(
                    this.isEmailAllowed,
                )
            })
        },
    }
}

</script>