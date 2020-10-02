<template>
    <md-editor
        v-bind:initialValue="value"
        v-on:change="onEditorChange"
        v-bind="$attrs"
        initialEditType="wysiwyg"
        ref="mdEditor"
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
                toolbarItems: [
                    'bold',
                    'italic',
                    'ul',
                    'ol',
                    'link',
                ],
            }
        },
    },
    watch:{
        value(newValue){
            this.$refs.mdEditor.invoke('setMarkdown', newValue)
        },
    },
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