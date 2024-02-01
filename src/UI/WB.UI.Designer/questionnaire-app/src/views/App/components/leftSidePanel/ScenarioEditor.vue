<template>
    <Teleport to="body">
        <div v-if="isOpen" uib-modal-window="modal-window"
            class="modal scenarioEditorModal dragAndDrop fade ng-scope ng-isolate-scope in" role="dialog" size="lg"
            index="0" animate="animate" ng-style="{'z-index': 1050 + $$topModalIndex*10, display: 'block'}" tabindex="-1"
            uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true"
            style="z-index: 1050; display: block;">
            <div class="modal-dialog modal-lg">
                <div class="modal-content" uib-modal-transclude="">
                    <div class="modal-header blue-strip handle">
                        <button type="button" class="close" aria-hidden="true" @click="isOpen = false"></button>
                        <h3 class="modal-title">
                            <span>{{ $t('QuestionnaireEditor.ScenarioDetails') }}</span>:
                            <span style="overflow: hidden;">{{ scenario.title }}</span>
                        </h3>
                    </div>
                    <div class="modal-body">
                        <div class="header-block clearfix">
                            <form name="frmEditor">
                                <!--div id="edit-scenario" ui-ace="{ onLoad : aceLoaded,
                                mode: 'json',
                                showGutter: false }" v-model="scenario.steps"></div-->
                                <!--v-ace-editor id="edit-scenario" ref="editor" v-model:value="scenario.steps"
                                    @init="editorInit" theme="github" :lang="json" :options="{
                                        maxLines: 30,
                                        fontSize: 16,
                                        highlightActiveLine: false,
                                        indentedSoftWrap: false,
                                        printMargin: true,
                                        showLineNumbers: false,
                                        showGutter: false,
                                        useWorker: true
                                    }" /-->
                                <ExpressionEditor id="edit-scenario" v-model="scenarioSteps" mode="scenario">
                                </ExpressionEditor>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script>
import { VAceEditor } from 'vue3-ace-editor';
import ExpressionEditor from '../ExpressionEditor.vue'
import { getScenarioSteps } from '../../../../services/scenariosService'

export default {
    name: 'ScenarioEditor',
    components: { VAceEditor, ExpressionEditor },
    props: {
        questionnaireId: { type: String, required: true },
        scenario: { type: Object, required: true },
    },
    data() {
        return {
            isOpen: false,
            scenarioSteps: '',
        }
    },
    methods: {
        async show() {
            this.scenarioSteps = await getScenarioSteps(this.questionnaireId, this.scenario.id);
            this.isOpen = true;
        },
        /*editorInit(editor) {
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
        }*/
    }
}
</script>
