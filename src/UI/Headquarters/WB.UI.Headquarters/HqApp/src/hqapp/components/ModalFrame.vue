<template>
      <div class="modal fade" :id="id" ref="modal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button v-if="canClose" type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true"></span></button>
                    <h2>{{ title }}</h2>
                </div>
                <div class="modal-body">
                    <slot />
                </div>
                <div class="modal-footer">
                   <slot name="actions" />
                </div>
            </div>
        </div>
    </div>  
</template>

<script>
export default {
    props: {
        id: String,
        title: String,
        canClose: {
            type: Boolean,
            default() { return true ;}
        }
    },

    methods:{
        hide() {
            $(this.$refs.modal).modal("hide");
        },

        modal(options){
            $(this.$refs.modal).modal(options)
        }
    }
    
}
</script>
