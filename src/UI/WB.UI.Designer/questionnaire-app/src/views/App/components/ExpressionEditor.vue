<template>
    <div class="pseudo-form-control" ref="editorHolder">        
        <v-ace-editor ref="editor" v-model:value="editorValue" @init="editorInit" theme="github"
            :lang="mode !== 'substitutions' ? 'csharp' : 'text'" :options="{
                maxLines: 30,
                fontSize: 16,
                highlightActiveLine: false,
                indentedSoftWrap: false,
                printMargin: mode !== 'substitutions',
                showLineNumbers: false,
                showGutter: false,
                useWorker: true
            }" />
    </div>
</template>

<script>
import { VAceEditor } from 'vue3-ace-editor';
import themeGithubUrl from 'ace-builds/src-noconflict/theme-github?url';
import modeCsharpUrl from 'ace-builds/src-noconflict/mode-csharp?url';

ace.config.setModuleUrl('ace/theme/github', themeGithubUrl);
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
            var renderer = editor.renderer;
            renderer.setPadding(12);

            editor.$blockScrolling = Infinity;
            editor.commands.bindKey("tab", null);
            editor.commands.bindKey("shift+tab", null);

            var session = editor.getSession();

            //extend text mode
            if (session.$mode.$id == "ace/mode/text") {

                var rules = session.$mode.$highlightRules.getRules();
                for (var stateName in rules) {
                    if (Object.prototype.hasOwnProperty.call(rules, stateName)) {
                        rules[stateName].unshift({
                            token: 'support.variable',
                            regex: '\%[a-zA-Z][_a-zA-Z0-9]{0,31}\%'
                        });
                    }
                }

                session.$mode.$tokenizer = null;
                session.bgTokenizer.setTokenizer(session.$mode.getTokenizer());
                session.bgTokenizer.start(0);
            }

            var holderDiv = this.$refs.editorHolder;
            editor.on('focus', function () { holderDiv.classList.add('focused'); });
            editor.on('blur', function () { holderDiv.classList.remove('focused'); });
        }
    }
};
</script>
