<template>
    <ModalFrame :id="id"
        :title="confirm_title">
        <slot />
        <button slot="actions"
            type="button"
            class="btn btn-primary"
            :disabled="disableOk"
            @click="confirm">
            {{$t("Common.Ok")}}
        </button>
        <button slot="actions"
            type="button"
            class="btn btn-link"
            @click="cancel">
            {{$t("Common.Cancel")}}
        </button>
    </ModalFrame>
</template>

<script>
export default {
    props: {
        id: String,
        title: {
            type: String,
            required: false,
        },
        disableOk: {
            type: Boolean,
            required: false,
            default: false,
        },
    },

    data() {
        return {
            callback: null,
        }
    },

    computed: {
        confirm_title() {
            return this.title || this.$t('Pages.ConfirmationNeededTitle')
        },
    },

    methods: {
        cancel() {
            this.callback(false)
            this.$emit('cancel')
            $(this.$el).modal('hide')
        },
        confirm() {
            this.callback(true)
            this.$emit('confirm')
            $(this.$el).modal('hide')
        },
        promt(callback) {
            this.callback = callback
            $(this.$el).appendTo('body').modal()
        },
    },
}
</script>
