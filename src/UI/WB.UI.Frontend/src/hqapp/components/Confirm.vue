<template>
    <ModalFrame :id="id" :title="confirm_title" ref="confirmModal">
        <slot />
        <template v-slot:actions>
            <div>
                <button type="button" :class="ok_class" :disabled="disableOk" @click="confirm">
                    {{ ok_title }}
                </button>
                <button type="button" class="btn btn-link" @click="cancel">
                    {{ $t("Common.Cancel") }}
                </button>
            </div>
        </template>
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
            this.$refs.confirmModal.hide()
            //$(this.$el).modal('hide')
        },
        confirm() {
            this.callback(true)
            this.$emit('confirm')
            this.$refs.confirmModal.hide()
            //$(this.$el).modal('hide')
        },
        promt(callback) {
            this.callback = callback
            this.$refs.confirmModal.modal({ keyboard: false })
            //$(this.$el).appendTo('body').modal()
        },
    },
}
</script>
