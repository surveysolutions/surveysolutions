<template>
    <teleport to="body">
        <div v-if="isOpen" class="modal" :class="class" :id="id" ref="modal" tabindex="-1" role="dialog"
            :aria-labelledby="titleId">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button v-if="canClose" type="button" class="close" @click="hide" aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <slot name="title">
                            <h2 v-if="useHtmlInTitle" :id="titleId" v-dompurify-html="title" />
                            <h2 v-else :id="titleId">
                                {{ title }}
                            </h2>
                        </slot>
                    </div>
                    <slot name="form" />
                    <div class="modal-body" v-if="!$slots.form">
                        <slot />
                    </div>
                    <div class="modal-footer" v-if="!$slots.form">
                        <slot name="actions" />
                    </div>
                </div>
            </div>
        </div>
    </teleport>
</template>

<script>
import { nextTick } from 'vue'
import { Modal } from 'bootstrap'

export default {
    props: {
        id: String,
        title: String,
        useHtmlInTitle: { type: Boolean, required: false, default: false },
        canClose: { type: Boolean, default: true },
        class: String
    },

    data() {
        return {
            modalInstance: null,
            isOpen: false,
        }
    },

    computed: {
        titleId() {
            return this.id + 'lbl'
        },
    },
    expose: ['hide', 'modal'],
    methods: {
        hide() {
            nextTick(() => {
                this.modalInstance?.hide()
                this.modalInstance?.dispose()
            })
            this.isOpen = false
        },
        modal(params) {
            this.isOpen = true
            nextTick(() => {
                this.modalInstance = new Modal(this.$refs.modal, params || {})
                this.modalInstance.show()
            })
        }
    }
}
</script>
