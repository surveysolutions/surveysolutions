<template>
      <div class="modal fade" :id="id" ref="modal" tabindex="-1" role="dialog" :aria-labelledby="titleId">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button v-if="canClose" type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true"></span></button>
                    <slot name="title">
                        <h2 :id="titleId">{{ title }}</h2>
                    </slot>
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

    computed: {
        titleId(){
            return this.id + "lbl";
        }
    },

    methods:{
        hide() {
            $(this.$refs.modal).modal("hide");
        },

        modal(params){
            $(this.$refs.modal).appendTo("body").modal(params || {});
        }
    }
    
}
</script>
