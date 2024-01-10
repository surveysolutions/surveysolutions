<template>
    <v-ace-editor v-model:value="editorValue" 
        @init="editorInit"
        theme="github"
        :lang="mode !== 'substitutions' ? 'csharp' : 'text'"        
        :options="{maxLines: 30,
                   fontSize: 16,
                   highlightActiveLine: false,
                   indentedSoftWrap: false,
                   printMargin: mode !== 'substitutions',
                   showLineNumbers: mode !== 'substitutions',
                   showGutter: mode !== 'substitutions',
                   useWorker: true}"/>
        
</template>

<script>
import { VAceEditor } from 'vue3-ace-editor';

import themeGithubUrl from 'ace-builds/src-noconflict/theme-github?url';
ace.config.setModuleUrl('ace/theme/github', themeGithubUrl);

import modeCsharpUrl from 'ace-builds/src-noconflict/mode-csharp?url';
ace.config.setModuleUrl('ace/mode/csharp', modeCsharpUrl);

export default {
    name: 'ExpressionEditor',
    components: { VAceEditor },
    emits: ['update:modelValue'],
    props: {
        modelValue: { type: String, required: true },
        mode: { type: String, default: 'substitutions' },
    },
    data() {
        return {
            //
        };
    },
    computed: {
        editorValue: {
            get() {
                return this.modelValue;
            },
            set(newValue) {
                this.$emit('update:modelValue', newValue);
            }
        }
    },
    methods: {
        editorInit(editor) {
            
        }
    }
};
</script>
