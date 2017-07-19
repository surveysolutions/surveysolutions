<template>
    <ModalFrame :id="id" :title="confirm_title">
        <slot />
        <button slot="actions" type="button" class="btn btn-primary" v-on:click="confirm">{{$t("Common.Ok")}}</button>
        <button slot="actions" type="button" class="btn btn-link" data-dismiss="modal">{{$t("Common.Cancel")}}</button>
    </ModalFrame>
</template>

<script>
export default {
    props: {
        id: String,
        title: {
            type: String,
            required: false
        }
    },

    data() {
        return {
            callback: null
        }
    },

    computed: {
        confirm_title() {
            return this.title || this.$t("Pages.ConfirmationNeededTitle");
        }
    },

    methods: {
        confirm() {
            this.callback()
            $(this.$el).modal('hide');
        },
        promt(callback) {
            this.callback = callback
            $(this.$el).modal();
        }
    }
}
</script>
