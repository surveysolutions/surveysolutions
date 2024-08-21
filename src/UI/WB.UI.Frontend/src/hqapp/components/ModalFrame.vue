<template>
    <teleport to="body">
        <div v-if="isOpen" class="modal fade" :id="id" ref="modal" tabindex="-1" role="dialog"
            :aria-labelledby="titleId">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button v-if="canClose" type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <slot name="title">
                            <h2 v-if="useHtmlInTitle" :id="titleId" v-html="title" />
                            <h2 v-else :id="titleId">
                                {{ title }}
                            </h2>
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
    </teleport>
</template>

<script>
import { nextTick } from 'vue'
export default {
    props: {
        id: String,
        title: String,
        useHtmlInTitle: { type: Boolean, required: false, default: false },
        canClose: { type: Boolean, default: true },
    },

    data() {
        return {
            isOpen: false,
        }
    },

    computed: {
        titleId() {
            return this.id + 'lbl'
        },
    },
    methods: {
        hide() {
            nextTick(() => {
                $(this.$refs.modal).modal('hide')
            })
            this.isOpen = false
        },
        modal(params) {
            this.isOpen = true
            nextTick(() => {
                $(this.$refs.modal).modal(params || {})
            })
        }
    }
}
</script>
