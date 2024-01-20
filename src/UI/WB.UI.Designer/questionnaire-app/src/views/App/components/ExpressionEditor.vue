<template>
    <div :class="{ 'pseudo-form-control' : focusable === 'true'}" ref="editorHolder">
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
    },
    data() {
        return {
            //
        };
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
    methods: {
        editorInit(editor) {
            self = this;
            var renderer = editor.renderer;
            renderer.setPadding(12);

            editor.$blockScrolling = Infinity;
            editor.commands.bindKey("tab", null);
            editor.commands.bindKey("shift+tab", null);

            var session = editor.getSession();

            //extend text mode with substitutions
            if (this.mode === 'substitutions') {
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
            else if (self.mode !== 'substitutions') {
                session.on("changeMode", function () {
                    if (session.$mode.$id !== 'ace/mode/csharp')
                        return;

                    var rules = session.$mode.$highlightRules.getRules();
                    for (var stateName in rules) {
                        if (Object.prototype.hasOwnProperty.call(rules, stateName)) {

                            var mapperRule = rules[stateName].find(function (rule) {
                                return rule.regex == '[a-zA-Z_$][a-zA-Z0-9_$]*\\b';
                            });
                            if (mapperRule == undefined || mapperRule == null)
                                continue;
                            else {

                                var supportVariableList = "";
                                var variables = self.treeStore.getChapter.variableNames;
                                if (variables)
                                    supportVariableList = variables.map(function (el) {
                                        return el.name;
                                    }).join('|');


                                var keywordMapper = session.$mode.$highlightRules.createKeywordMapper({
                                    "variable.language": "this|self",
                                    "support.variable": supportVariableList,
                                    "keyword": "abstract|event|new|struct|as|explicit|null|switch|base|extern|object|this|bool|false|operator|throw|break|finally|out|true|byte|fixed|override|try|case|float|params|typeof|catch|for|private|uint|char|foreach|protected|ulong|checked|goto|public|unchecked|class|if|readonly|unsafe|const|implicit|ref|ushort|continue|in|return|using|decimal|int|sbyte|virtual|default|interface|sealed|volatile|delegate|internal|short|void|do|is|sizeof|while|double|lock|stackalloc|else|long|static|enum|namespace|string|var|dynamic",
                                    "constant.language": "null|true|false"
                                }, "identifier");

                                mapperRule.token = keywordMapper;
                                mapperRule.onMatch = keywordMapper;
                                mapperRule.regex = '[@a-zA-Z_$][a-zA-Z0-9_$]*\\b';
                            }
                        }
                    }

                    session.$mode.$tokenizer = null;
                    session.bgTokenizer.setTokenizer(session.$mode.getTokenizer());
                    session.bgTokenizer.start(0);
                });

                //TODO: for linked add "@current"

                ace.config.loadModule('ace/ext/language_tools', function () {
                    var variablesCompletor =
                    {
                        getCompletions: function (editor, session, pos, prefix, callback) {
                            var i = 0;

                            var variables =
                                _.orderBy(self.treeStore.getChapter.variableNames, 'name', 'desc')
                                    .map(function (variable) {
                                        return {
                                            name: variable.name,
                                            value: variable.name,
                                            score: i++,
                                            meta: variable.type
                                        }
                                    });
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
            }

            var holderDiv = this.$refs.editorHolder;
            editor.on('focus', function () { holderDiv.classList.add('focused'); });
            editor.on('blur', function () { holderDiv.classList.remove('focused'); });
        }
    }
};
</script>
