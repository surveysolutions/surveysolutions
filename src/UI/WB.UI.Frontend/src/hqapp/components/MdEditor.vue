<template>
    <div ref="editor"></div>
</template>
<style>
.toastui-editor-tabs {
    display: none;
}
</style>
<script>

import '@toast-ui/editor/dist/toastui-editor.css'

import Editor from '@toast-ui/editor';
import { escape, unescape } from 'lodash'
import { markRaw } from 'vue';

export default {
    emits: ['input', 'update:modelValue'],
    props: {
        modelValue: { type: String, required: true },
        supportHtml: { type: Boolean, required: false, default: false },
    },
    data() {
        return {
            initialValue: '',
            editor: null,
            height: '300px'
        }
    },
    mounted() {
        let val = this.modelValue
        this.initialValue = val
        if (this.supportHtml != true) {
            val = unescape(val)
        }

        const options = {
            ...this.editorOptions,
            el: this.getRootElement(),
            initialValue: val,
            height: this.height,
            events: {
                //change: () => emit("update:modelValue", e.getMarkdown())
                change: () => this.onEditorChange()
            }
        };

        this.editor = markRaw(new Editor(options));
    },
    unmounted() {
        this.editor.off('change');
        this.editor.destroy();
        this.editor = null
    },
    computed: {
        editorOptions() {
            return {
                usageStatistics: false,
                initialEditType: 'markdown',
                previewStyle: "global",
                hideModeSwitch: true,
                //previewHighlight: false,
                //previewStyle: 'vertical',
                //useDefaultHTMLSanitizer: true,
                //customHTMLSanitizer: sanitizeHtml,
                toolbarItems: [[
                    'heading',
                    'bold',
                    'italic',
                    'ul',
                    'ol'
                ],
                [
                    'image',
                    'link',
                ]],
            }
        },
    },
    watch: {
        modelValue(newValue) {
            let val = newValue

            if (this.supportHtml != true) {
                val = unescape(val)
            }

            const edVal = this.editor.getMarkdown()
            if (val !== edVal) {
                this.editor.setMarkdown(val);
            }
        }
    },
    expose: ['refresh'],
    methods: {
        onEditorChange() {
            let markDown = this.editor.getMarkdown()

            if (this.supportHtml != true) {
                markDown = escape(markDown)
            }

            if (this.modelValue != markDown) {
                this.$emit('input', markDown)
                this.$emit('update:modelValue', markDown);
            }
        },
        refresh() {
            setTimeout(() => {
                this.editor.moveCursorToStart(false)
            }, 100)
        },
        getRootElement() {
            return this.$refs.editor
        },
    },
}
</script>