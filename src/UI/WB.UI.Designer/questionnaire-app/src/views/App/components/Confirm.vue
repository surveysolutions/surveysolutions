<template>
    <teleport to="body">
        <div v-if="isOpen" uib-modal-window="modal-window" class="modal confirm-window fade ng-scope ng-isolate-scope in"
            role="dialog" index="0" animate="animate" tabindex="-1" uib-modal-animation-class="fade" modal-in-class="in"
            modal-animation="true" style="z-index: 1050; display: block;">
            <div class="modal-dialog ">
                <div class="modal-content" uib-modal-transclude="">
                    <div class="modal-header">
                        <button class="close" @click="cancel()" aria-hidden="true" type="button"></button>
                        <h3 class="modal-title">
                            {{ header || $t('QuestionnaireEditor.ModalConfirm') }}
                        </h3>
                    </div>
                    <div class="modal-body" v-html="title">
                    </div>
                    <div class="modal-footer" v-if="!noControls">
                        <button class="btn btn-primary btn-lg" v-if="!isReadOnly" @click="ok()">
                            {{ okButtonTitle || $t('QuestionnaireEditor.OK') }}
                        </button>
                        <button v-if="!isAlert" class="btn btn-link" @click="cancel()">
                            {{
                                cancelButtonTitle ||
                                $t('QuestionnaireEditor.Cancel')
                            }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <div v-if="isOpen" uib-modal-backdrop="modal-backdrop" class="modal-backdrop fade ng-scope in"
            uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true"
            data-bootstrap-modal-aria-hidden-count="1" aria-hidden="true" style="z-index: 1040;" @click="cancel()"></div>
    </teleport>
</template>

<script>
import event from '../../../plugins/events';

const confirmDialog = {
    name: 'Confirm',
    data() {
        return {
            title: { type: String, required: true },
            header: { type: String, required: false },
            okButtonTitle: { type: String, required: false },
            cancelButtonTitle: { type: String, required: false },
            isReadOnly: { type: Boolean, required: false },
            callback: { type: Function, required: false },
            noControls: { type: Boolean, required: false },
            isAlert: { type: Boolean, required: false, default: false },
            isOpen: false
        };
    },
    mounted() {
        event.on('open', params => {
            this.open(params);
        });
        event.on('close', () => {
            this.cancel();
        });
    },
    methods: {
        open(params) {
            this.title = params.title;
            this.header = params.header;
            this.okButtonTitle = params.okButtonTitle;
            this.cancelButtonTitle = params.cancelButtonTitle;
            this.isReadOnly = params.isReadOnly || false;
            this.isAlert = params.isAlert || false;
            this.callback = params.callback;
            this.noControls = params.noControls || false;
            this.isOpen = true;
        },
        cancel() {
            if (this.callback)
                this.callback(false);
            this.isOpen = false;
        },
        ok() {
            if (this.callback)
                this.callback(true);
            this.isOpen = false;
        }
    }
};

export default confirmDialog;
</script>
