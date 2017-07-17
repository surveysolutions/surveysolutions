<template>
    <div class="modal fade" :id="id" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true"></span></button>
                    <h3>{{ confirm_title }}</h3>
                </div>
                <div class="modal-body">
                    <slot />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" v-on:click="confirm" >{{$t("Common.Ok")}}</button>
                    <button type="button" class="btn btn-link" data-dismiss="modal">{{$t("Common.Cancel")}}</button>
                </div>
            </div>
        </div>
    </div>  
</template>

<script>
export default {
    props: {
            id : String,
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
