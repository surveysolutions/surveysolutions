<template>
    <div class="pseudo-form-control">
        <!--div id="edit-group-title-highlight"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        ng-model="activeGroup.title"></div-->

        <!--div ng-attr-id="{{'validation-message-' + $index}}" ng-attr-tabindex="{{$index + 1}}"
                        ui-ace="{ onLoad : setupAceForSubstitutions, require: ['ace/ext/language_tools'] }"
                        v-model="validation.message"></div-->
                        
        <v-ace-editor v-model:value="editorValue" @init="editorInit" theme="github"
            :lang="mode !== 'substitutions' ? 'csharp' : 'text'" :options="{
                maxLines: 30,
                fontSize: 16,
                highlightActiveLine: false,
                indentedSoftWrap: false,
                printMargin: mode !== 'substitutions',
                showLineNumbers: mode !== 'substitutions',
                showGutter: mode !== 'substitutions',
                useWorker: true
            }" />
    </div>
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
            editor.on('focus', function () {
                    $('.ace_focus').parents('.pseudo-form-control').addClass('focused');
                });

            editor.on('blur', function () {
                    $('.pseudo-form-control.focused').removeClass('focused');
                });
        }
    }
};
</script>
