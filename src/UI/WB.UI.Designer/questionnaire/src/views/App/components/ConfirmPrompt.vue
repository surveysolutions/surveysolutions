<template>
    <teleport to="body">
        <div v-if="isOpen" uib-modal-window="modal-window"
            class="modal confirm-window fade ng-scope ng-isolate-scope in" role="dialog" index="0" animate="animate"
            tabindex="-1" uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true"
            style="z-index: 1050; display: block;">
            <div class="modal-dialog ">
                <div class="modal-content" uib-modal-transclude="">
                    <div class="modal-header">
                        <button class="close" @click="cancel()" aria-hidden="true" type="button"></button>
                        <h3 class="modal-title">
                            {{ header || $t('QuestionnaireEditor.ModalConfirm') }}
                        </h3>
                    </div>
                    <div class="modal-body">
                        <div v-dompurify-html="title" style="display: contents;"></div>

                        <div class="form-group" style="margin-top: 10px;">
                            <label v-if="inputLabel" class="control-label">{{ inputLabel }}</label>
                            <textarea class="form-control" v-model="inputValue" :placeholder="inputPlaceholder"
                                :rows="inputRows"></textarea>
                            <div v-if="inputHint" class="help-block">{{ inputHint }}</div>
                        </div>
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
            data-bootstrap-modal-aria-hidden-count="1" aria-hidden="true" style="z-index: 1040;" @click="cancel()">
        </div>
    </teleport>
</template>

<script>
import event from '../../../plugins/events';

const confirmPromptDialog = {
    name: 'ConfirmPrompt',
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

            inputValue: '',
            inputLabel: null,
            inputPlaceholder: null,
            inputHint: null,
            inputRows: 4,

            isOpen: false
        };
    },
    mounted() {
        event.on('openPrompt', params => {
            this.open(params);
        });
        event.on('closePrompt', () => {
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

            this.inputValue = params.inputValue || '';
            this.inputLabel = params.inputLabel || null;
            this.inputPlaceholder = params.inputPlaceholder || null;
            this.inputHint = params.inputHint || null;
            this.inputRows = params.inputRows || 4;

            this.isOpen = true;
        },
        cancel() {
            const callback = this.callback;
            const value = this.inputValue;

            this.isOpen = false;
            this.inputValue = '';

            if (callback)
                callback(false, value);
        },
        ok() {
            const callback = this.callback;
            const value = this.inputValue;

            this.isOpen = false;
            this.inputValue = '';

            if (callback)
                callback(true, value);
        }
    }
};

export default confirmPromptDialog;
</script>

<style scoped>
.confirm-window textarea.form-control {
    height: auto !important;
    min-height: 6em;
}
</style>
