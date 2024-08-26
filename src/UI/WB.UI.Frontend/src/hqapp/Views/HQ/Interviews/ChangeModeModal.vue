<template>
    <ModalFrame ref="modalDialog" data-suso="change-interview-mode-modal" :title="title">
        <div class="action-container">
            <p v-html="confirmMessage"></p>
        </div>
        <div class="form-group" :id="'group__' + modalId + '_' + filteredCount"
            v-if="receivedByInterviewerItemsCount > 0">
            <br />
            <input type="checkbox" :id="'switchModeReceivedByInterviewer_' + modalId"
                v-model="internalConfirmReceivedByInterviewer" class="checkbox-filter" />
            <label :for="'switchModeReceivedByInterviewer_' + modalId" style="font-weight: normal">
                <span class="tick"></span>
                {{ $t("Interviews.AssignReceivedConfirm", receivedByInterviewerItemsCount) }}
            </label>
            <br />
            <span v-if="internalConfirmReceivedByInterviewer" class="text-danger">
                {{ $t("Interviews.SwitchToCawiReceivedWarning") }}
            </span>
        </div>
        <template v-slot:actions>
            <div>
                <button type="button" class="btn btn-primary" role="confirm" data-suso="change-interview-mode-modal"
                    @click="confirm" :disabled="filteredCount == 0">
                    {{ title }}
                </button>
                <button type="button" class="btn btn-link" data-dismiss="modal" role="cancel">
                    {{ $t("Common.Cancel") }}
                </button>
            </div>
        </template>
    </ModalFrame>
</template>

<script>
export default {
    data() {
        return {
            internalConfirmReceivedByInterviewer: this.confirmReceivedByInterviewer,
        }
    },
    props: {
        filteredCount: { type: Number, require: true },
        title: { type: String, require: true },
        confirmMessage: { type: String, require: true },
        receivedByInterviewerItemsCount: { type: Number, require: true },
        confirmReceivedByInterviewer: { type: Boolean, default() { return false } },
        modalId: { type: String, require: true },
    },
    methods: {
        modal(params) {
            this.$refs.modalDialog.modal(params)
        },
        confirm() {
            this.$refs.modalDialog.hide()
            this.$emit('confirm', this.internalConfirmReceivedByInterviewer)
        }
    }
}
</script>