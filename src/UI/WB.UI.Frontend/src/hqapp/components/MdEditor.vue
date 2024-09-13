<template>
    <!--editor ref="mdEditor" v-bind:initialValue="initialValue" v-on:change="onEditorChange" v-bind="$attrs"
        initialEditType="markdown" previewStyle="global" :options="editorOptions" v-on="$listeners">
    </editor-->

    <div ref="editor"></div>
</template>
<script>

import '@toast-ui/editor/dist/toastui-editor.css'
import Editor from '@toast-ui/editor';
import { escape, unescape } from 'lodash'
import _sanitizeHtml from 'sanitize-html'

const sanitizeHtml = text => _sanitizeHtml(text, { allowedTags: [], allowedAttributes: [] })


export default {
    components: {
        //editor: Editor,
    },

    props: {
        value: { type: String, required: true },
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
        let val = this.value
        this.initialValue = val
        if (this.supportHtml != true) {
            val = unescape(val)
        }

        const options = {
            ...this.editorOptions,
            el: this.$refs.editor,
            initialValue: val,
            height: this.height,
            events: {
                //change: () => emit("update:modelValue", e.getMarkdown())
                change: () => this.onEditorChange()
            }
        };

        this.editor = new Editor(options);
    },
    destroyed() {
        this.editor.off('change');
        this.editor.destroy();
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
        value(newValue, oldValue) {
            let val = newValue
            if (val != this.value || val == this.initialValue) {
                if (this.supportHtml != true) {
                    val = unescape(val)
                }

                this.$refs.mdEditor.setMarkdown(val)
            }
        },
    },
    methods: {
        onEditorChange() {
            let markDown = this.editor.getMarkdown()

            if (this.supportHtml != true) {
                markDown = escape(markDown)
            }

            if (this.value != markDown) {
                this.$emit('input', markDown)
            }
        },
        refresh() {
            var self = this
            setTimeout(() => {
                this.editor.moveCursorToStart()
            }, 100)
        },
    },
}
</script>