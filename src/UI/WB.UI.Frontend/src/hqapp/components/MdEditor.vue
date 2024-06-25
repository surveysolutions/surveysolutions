<template>
    <md-editor v-bind:initialValue="initialValue" v-on:change="onEditorChange" v-bind="$attrs"
        initialEditType="markdown" ref="mdEditor" previewStyle="global" :options="editorOptions" v-on="$listeners">
    </md-editor>
</template>
<script>

//TODO: MIGRATION. styles were comming from @toast-ui/vue-editor
//import 'codemirror/lib/codemirror.css'

import '@toast-ui/editor/dist/toastui-editor.css'

import { Editor } from '@toast-ui/editor'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text, { allowedTags: [], allowedAttributes: [] })
import { escape, unescape } from 'lodash'


export default {
    components: {
        mdEditor: Editor,
    },

    props: {
        value: { type: String, required: true },
        supportHtml: { type: Boolean, required: false, default: false },
    },
    data() {
        return {
            initialValue: '',
        }
    },
    computed: {
        editorOptions() {
            return {
                usageStatistics: false,
                hideModeSwitch: true,
                //useDefaultHTMLSanitizer: true,
                //customHTMLSanitizer: sanitizeHtml,
                toolbarItems: [
                    'heading',
                    'bold',
                    'italic',
                    'ul',
                    'ol',
                    'divider',
                    'image',
                    'link',
                ],
            }
        },
    },

    mounted() {
        let val = this.value
        this.initialValue = val
        if (this.supportHtml != true) {
            val = unescape(val)
        }
        this.$refs.mdEditor.invoke('setMarkdown', val)
    },
    watch: {
        value(newValue, oldValue) {
            let val = newValue
            if (val != this.value || val == this.initialValue) {
                if (this.supportHtml != true) {
                    val = unescape(val)
                }

                this.$refs.mdEditor.invoke('setMarkdown', val)
            }
        },
    },
    methods: {
        onEditorChange() {
            let markDown = this.$refs.mdEditor.invoke('getMarkdown')

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
                this.$refs.mdEditor.invoke('moveCursorToStart')
            }, 100)
        },
    },
}
</script>