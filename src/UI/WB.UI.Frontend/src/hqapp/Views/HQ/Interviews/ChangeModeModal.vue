<template>
    <ModalFrame ref="modal"
        data-suso="change-interview-mode-modal"
        :title="title">
        <div class="action-container">
            <p v-html="confirmMessage"></p>
        </div>
        <div class="form-group"
            v-if="receivedByInterviewerItemsCount > 0">
            <br />
            <input
                type="checkbox"
                id="approveReceivedByInterviewer"
                v-model="confirmReceivedByInterviewer"
                class="checkbox-filter"/>
            <label for="approveReceivedByInterviewer"
                style="font-weight: normal">
                <span class="tick"></span>
                {{$t("Interviews.AssignReceivedConfirm", receivedByInterviewerItemsCount)}}
            </label>
            <br />
            <span v-if="confirmReceivedByInterviewer"
                class="text-danger">
                {{$t("Interviews.ApproveReceivedWarning")}}
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
    data() {
        return {
            confirmReceivedByInterviewer: false,
        }
    },

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