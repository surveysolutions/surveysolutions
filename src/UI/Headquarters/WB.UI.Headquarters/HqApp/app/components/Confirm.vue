<template>
    <ModalFrame :id="id" :title="confirm_title">
        <slot />
        <button slot="actions" type="button" class="btn btn-primary" v-on:click="confirm">{{$t("Common.Ok")}}</button>
        <button slot="actions" type="button" class="btn btn-link" v-on:click="cancel">{{$t("Common.Cancel")}}</button>
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
        cancel() {
            this.callback(false)
            $(this.$el).modal('hide');
        },
        confirm() {
            this.callback(true)
            $(this.$el).modal('hide');
        },
        promt(callback) {
            this.callback = callback
            $(this.$el).modal();
        }
    }
}
</script>
