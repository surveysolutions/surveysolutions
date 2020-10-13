<template>
    <md-editor
        v-bind:initialValue="value"
        v-on:change="onEditorChange"
        v-bind="$attrs"
        initialEditType="markdown"
        ref="mdEditor"
        previewStyle="tab"
        :options="editorOptions"
        v-on="$listeners" >
    </md-editor>
</template>
<script>
import 'codemirror/lib/codemirror.css'
import '@toast-ui/editor/dist/toastui-editor.css'

import { Editor } from '@toast-ui/vue-editor'

export default {
    components:{
        mdEditor: Editor,
    },

    props:{
        value: {type: String, required: true},
    },
    computed: {
        editorOptions() {
            return {
                usageStatistics: false,
                hideModeSwitch: true,
                useDefaultHTMLSanitizer: true,
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
        this.$refs.mdEditor.invoke('setMarkdown', this.value)
    },
    /*watch:{
        value(newValue){
            if (newValue != this.value) {
                this.$refs.mdEditor.invoke('setMarkdown', newValue)
            }
        },
    },*/
    methods: {
        onEditorChange() {
            const markDown = this.$refs.mdEditor.invoke('getMarkdown')
            if(this.value != markDown)
            {
                this.$emit('input', markDown)
            }
        },
    },
}
</script>