<template>
    <md-editor
        v-bind:initialValue="initialValue"
        v-on:change="onEditorChange"
        v-bind="$attrs"
        @load="onEditorLoad"
        @focus="onEditorFocus"
        initialEditType="markdown"
        ref="mdEditor"
        previewStyle="global"
        :options="editorOptions"
        v-on="$listeners" >
    </md-editor>
</template>
<script>
import 'codemirror/lib/codemirror.css'
import '@toast-ui/editor/dist/toastui-editor.css'

import { Editor } from '@toast-ui/vue-editor'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })
import { escape, unescape } from 'lodash'


export default {
    components:{
        mdEditor: Editor,
    },

    props:{
        value: {type: String, required: true},
        supportHtml: {type: Boolean, required: false, default:false},
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
        if (this.supportHtml != true) {
            val = unescape(val)
        }
        this.initialValue = val
        this.$refs.mdEditor.invoke('setMarkdown', val)
    },
    watch:{
        value(newValue, oldValue) {
            let val = newValue
            if (this.supportHtml != true) {
                val = unescape(val)
            }
            //if (val != this.value || val == this.initialValue) {
            this.$refs.mdEditor.invoke('setMarkdown', val)
            //}
        },
    },
    methods: {
        onEditorLoad() {
            // implement your code
        },
        onEditorFocus() {
            // implement your code
        },
        onEditorChange() {
            let markDown = this.$refs.mdEditor.invoke('getMarkdown')

            if (this.supportHtml != true) {
                markDown = escape(markDown)
            }

            //if(this.value != markDown) {
            this.$emit('input', markDown)
            //}
        },
        refresh() {
            var self = this
            setTimeout(() => {
                //const val = self.$refs.mdEditor.invoke('getMarkdown')
                //self.$refs.mdEditor.invoke('setMarkdown', val)
                //this.$refs.mdEditor.invoke('focus')
                //this.$refs.mdEditor.invoke('refresh')
                this.$refs.mdEditor.invoke('moveCursorToStart')

                //this.$refs.mdEditor.$el.style.display = 'none'
                //this.$refs.mdEditor.$el.style.display = 'block'
            }, 100)
            /*this.$nextTick(function() {
                //const val = this.$refs.mdEditor.invoke('getMarkdown')
                //this.$refs.mdEditor.invoke('setMarkdown', val)
                //this.$refs.mdEditor.invoke('focus')
                //this.$refs.mdEditor.invoke('refresh')
                this.$refs.mdEditor.$el.style.display = 'none'
                this.$refs.mdEditor.$el.style.display = 'block'
            })*/
        },
    },
}
</script>