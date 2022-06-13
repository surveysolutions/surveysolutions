<template>
    <ModalFrame :id="id"
        :title="confirm_title">
        <slot />
        <button slot="actions"
            type="button"
            :class="ok_class"
            :disabled="disableOk"
            @click="confirm">
            {{ ok_title }}
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
        okTitle: {
            type: String,
            required: false,
        },
        okClass: {
            type: String,
            required: false,
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
        ok_title() {
            return this.okTitle || this.$t('Common.Ok')
        },
        ok_class() {
            return 'btn btn-primary ' + (this.okClass || '')
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
