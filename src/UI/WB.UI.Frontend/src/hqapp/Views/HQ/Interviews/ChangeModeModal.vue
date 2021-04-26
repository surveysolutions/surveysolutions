<template>
    <ModalFrame ref="modal"
        data-suso="change-interview-mode-modal"
        :title="title">
        <div class="action-container">
            <p v-html="confirmMessage"></p>
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
    },

    methods: {
        modal() {
            this.$refs.modal.modal()
        },

        confirm() {
            this.$refs.modal.hide()
            this.$emit('confirm')
        },
    },
}
</script>