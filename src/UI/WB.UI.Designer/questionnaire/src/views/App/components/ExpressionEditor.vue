<template>
    <div :class="{ 'pseudo-form-control': focusable === 'true' }" ref="editorHolder" aceEditor>
        <textarea v-if="mode === 'substitutions'"
            ref="textareaEditor"
            v-model="editorValue"
            spellcheck="true"
            class="substitution-editor"
            :placeholder="placeholder || ''"
            rows="1"
            @focus="onTextareaFocus"
            @blur="onTextareaBlur"
            @input="onTextareaInput"
        ></textarea>
        <v-ace-editor v-else ref="editor" v-model:value="editorValue" @init="editorInit" theme="github"
            lang="csharp" :options="{
            minLines: 1,
            maxLines: 300,
            fontSize: 16,
            highlightActiveLine: false,
            indentedSoftWrap: false,
            printMargin: true,
            showLineNumbers: false,
            showGutter: false,
            useWorker: true,
            wrap: true,
            placeholder: this.placeholder,
        }" />
    </div>
</template>

<script>
import { VAceEditor } from 'vue3-ace-editor';

import themeGithubUrl from 'ace-builds/src-noconflict/theme-github?url';
import modeCsharpUrl from 'ace-builds/src-noconflict/mode-csharp?url';
import tools from 'ace-builds/src-noconflict/ext-language_tools?url';

ace.config.setModuleUrl('ace/theme/github', themeGithubUrl);
ace.config.setModuleUrl('ace/mode/csharp', modeCsharpUrl);
ace.config.setModuleUrl('ace/ext/language_tools', tools);

import { setCompleters } from 'ace-builds/src-noconflict/ext-language_tools';
import _ from 'lodash'
import { useTreeStore } from '../../../stores/tree';

export default {
    name: 'ExpressionEditor',
    components: { VAceEditor },
    emits: ['update:modelValue'],
    props: {
        modelValue: { type: String, required: true },
        mode: { type: String, default: 'substitutions' },
        focusable: { type: String, default: 'true' },
        placeholder: { type: String, required: false, default: null },
        usePadding: { type: Boolean, required: false, default: true },
    },
    data() {
        return {};
    },
    setup() {
        const treeStore = useTreeStore();

        return {
            treeStore
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
    watch: {
        modelValue(newValue) {
            if (this.mode === 'substitutions') {
                this.$nextTick(() => {
                    // Only resize if value changed externally (i.e., not from user typing
                    // which is already handled by onTextareaInput)
                    const ta = this.$refs.textareaEditor;
                    if (ta && ta.value !== newValue) {
                        this.resizeTextarea();
                    }
                });
            }
        }
    },
    mounted() {
        this.$emitter.on('variablesRecalculated', this.variablesRecalculated);
        if (this.mode === 'substitutions') {
            this.$nextTick(() => {
                this.resizeTextarea();
            });
        }
    },
    unmounted() {
        this.$emitter.off('variablesRecalculated', this.variablesRecalculated);
    },
    methods: {
        onTextareaFocus() {
            this.$refs.editorHolder.classList.add('focused');
        },
        onTextareaBlur() {
            this.$refs.editorHolder.classList.remove('focused');
        },
        onTextareaInput() {
            this.resizeTextarea();
        },
        resizeTextarea() {
            const ta = this.$refs.textareaEditor;
            if (ta) {
                ta.style.height = 'auto';
                ta.style.height = ta.scrollHeight + 'px';
            }
        },
        onModeChanged(e, session) {
            var self = this;
            if (session.$mode.$id !== 'ace/mode/csharp') return;

            var rules = session.$mode.$highlightRules.getRules();
            for (var stateName in rules) {
                if (Object.prototype.hasOwnProperty.call(rules, stateName)) {

                    var mapperRule = rules[stateName].find(function (rule) {
                        return rule.regex == '[a-zA-Z_$][a-zA-Z0-9_$]*\\b';
                    });
                    if (mapperRule == undefined || mapperRule == null)
                        continue;
                    else {
                        var lastUpdated = null;
                        var keywordMapper = null;

                        function getKeywordMapper(value) {
                            var variables = self.treeStore.getVariableNames;

                            if (lastUpdated !== variables.lastUpdated || keywordMapper === null) {
                                keywordMapper = session.$mode.$highlightRules.createKeywordMapper({
                                    "variable.language": "this|self",
                                    "support.variable": variables.getTokens(),
                                    "keyword": "abstract|event|new|struct|as|explicit|null|switch|base|extern|object|this|bool|false|operator|throw|break|finally|out|true|byte|fixed|override|try|case|float|params|typeof|catch|for|private|uint|char|foreach|protected|ulong|checked|goto|public|unchecked|class|if|readonly|unsafe|const|implicit|ref|ushort|continue|in|return|using|decimal|int|sbyte|virtual|default|interface|sealed|volatile|delegate|internal|short|void|do|is|sizeof|while|double|lock|stackalloc|else|long|static|enum|namespace|string|var|dynamic",
                                    "constant.language": "null|true|false"
                                }, "identifier");

                                lastUpdated = variables.lastUpdated;
                            }

                            return keywordMapper(value);
                        }

                        mapperRule.token = getKeywordMapper;
                        mapperRule.onMatch = getKeywordMapper;
                        mapperRule.regex = '[@a-zA-Z_$][a-zA-Z0-9_$]*\\b';
                    }
                }
            }

            session.$mode.$tokenizer = null;
            session.bgTokenizer.setTokenizer(session.$mode.getTokenizer());
            session.bgTokenizer.start(0);

            session.$mode.hasBeenUpdated = true;

            session.off("changeMode", this.onModeChanged);
        },
        editorInit(editor) {
            var self = this;
            var renderer = editor.renderer;

            if (this.usePadding == true)
                renderer.setPadding(12);
            else
                renderer.setPadding(0);

            editor.$blockScrolling = Infinity;

            editor.commands.bindKey("ctrl+f", null);
            editor.commands.bindKey("ctrl+h", null);
            editor.commands.bindKey("ctrl+b", null);
            editor.commands.bindKey("ctrl+s", null);

            editor.commands.bindKey("tab", null);
            editor.commands.bindKey("shift+tab", null);

            var session = editor.getSession();

            //do not subscribe if it's already updated
            if (!session.$mode.hasBeenUpdated)
                session.on("changeMode", this.onModeChanged);

            //TODO: for linked add "@current"
            ace.config.loadModule('ace/ext/language_tools', function () {
                var variablesCompletor =
                {
                    getCompletions: function (editor, session, pos, prefix, callback) {
                        var variables = self.treeStore.getVariableNames.getCompletions();
                        callback(null, variables);
                    },

                    identifierRegexps: [/[@a-zA-Z_0-9\$\-\u00A2-\uFFFF]/]
                };

                setCompleters([variablesCompletor]);

                editor.setOptions({
                    enableBasicAutocompletion: true,
                    enableSnippets: false,
                    enableLiveAutocompletion: true
                });
            });

            var holderDiv = this.$refs.editorHolder;
            editor.on('focus', function () { holderDiv.classList.add('focused'); });
            editor.on('blur', function () { holderDiv.classList.remove('focused'); });
        },
        variablesRecalculated() {
            if (this.mode !== 'substitutions') {
                var session = this.$refs.editor._editor.getSession();
                if (session.$mode.hasBeenUpdated) {
                    session.$mode.$tokenizer = null;
                    session.bgTokenizer.setTokenizer(session.$mode.getTokenizer());
                    session.bgTokenizer.start(0);
                }
            }
        }
    }
};
</script>

<style scoped>
.substitution-editor {
    display: block;
    width: 100%;
    min-height: 30px;
    padding: 0 12px;
    border: none;
    outline: none;
    resize: none;
    overflow: hidden;
    font-size: 16px;
    font-family: inherit;
    line-height: 1.5;
    background: transparent;
    box-sizing: border-box;
}
</style>
