<template>
    <ModalFrame ref="modal"
        data-suso="change-interview-mode-modal"
        :title="title">
        <div class="action-container">
            <p v-html="confirmMessage"></p>
        </div>
        <div class="form-group"
            :id="'group__' + modalId + '_' + filteredCount"
            v-if="receivedByInterviewerItemsCount > 0">
            <br />
            <input
                type="checkbox"
                :id="'switchModeReceivedByInterviewer_' + modalId"
                v-model="confirmReceivedByInterviewer"
                class="checkbox-filter"/>
            <label :for="'switchModeReceivedByInterviewer_' + modalId"
                style="font-weight: normal">
                <span class="tick"></span>
                {{$t("Interviews.AssignReceivedConfirm", receivedByInterviewerItemsCount)}}
            </label>
            <br />
            <span v-if="confirmReceivedByInterviewer"
                class="text-danger">
                {{$t("Interviews.SwitchToCawiReceivedWarning")}}
            </span>
        </div>
        <div slot="actions">
            <button
                type="button"
                class="btn btn-primary"
                role="confirm"
                data-suso="change-interview-mode-modal"
                @click="confirm"
                :disabled="filteredCount == 0">{{ title }}</button>
            <button
                type="button"
                class="btn btn-link"
                data-dismiss="modal"
                role="cancel">{{ $t("Common.Cancel") }}</button>
        </div>
    </ModalFrame>
</template>

<script>
export default {
    props: {
        filteredCount: {
            type: Number,
            require: true,
        },

        title: {
            type: String,
            require: true,
        },

        confirmMessage: { type: String, require: true},

        receivedByInterviewerItemsCount: {
            type: Number,
            require: true,
        },
        confirmReceivedByInterviewer: {
            type: Boolean,
            default() { return false },
        },
        modalId: {
            type: String,
            require: true,
        },
    },

    methods: {
        modal() {
            this.$refs.modal.modal()
        },

        confirm() {
            this.$refs.modal.hide()
            this.$emit('confirm', this.confirmReceivedByInterviewer)
        },
    },
}
</script>