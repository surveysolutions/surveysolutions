<template>
    <ModalFrame ref="modalFrame" :title="title">
        <p v-if="message">{{ message }}</p>
        <form onsubmit="return false;">
            <div class="form-group">
                <label class="control-label">{{ $t("Assignments.Comments") }}</label>
                <textarea v-model="comment" :placeholder="$t('Assignments.EnterComments')"
                    name="comments" rows="4" maxlength="500" class="form-control" />
            </div>
        </form>
        <template v-slot:actions>
            <div>
                <button type="button" class="btn btn-primary" @click="handleConfirm">{{ actionLabel }}</button>
                <button type="button" class="btn btn-link" data-bs-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </template>
    </ModalFrame>
</template>

<script>
export default {
    props: {
        title: { type: String, required: true },
        message: { type: String, default: null },
        actionLabel: { type: String, required: true },
    },
    emits: ['confirm'],
    data() {
        return { comment: null }
    },
    expose: ['modal', 'hide'],
    methods: {
        modal(params) {
            this.comment = null
            this.$refs.modalFrame.modal(params)
        },
        hide() {
            this.$refs.modalFrame.hide()
        },
        handleConfirm() {
            this.$emit('confirm', this.comment || null)
        },
    },
}
</script>
