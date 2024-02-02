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
                                <div id="edit-scenario">
                                    <v-ace-editor id="edit-scenario" ref="editor" v-model:value="scenarioSteps" lang="json"
                                        :options="{
                                            maxLines: 40,
                                            highlightActiveLine: false,
                                            indentedSoftWrap: false,
                                            printMargin: true,
                                            showLineNumbers: false,
                                            showGutter: false,
                                            useWorker: false
                                        }" />
                                    <!-- theme="chrome" -->
                                </div>
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
import { getScenarioSteps } from '../../../../services/scenariosService'

import 'ace-builds';
import 'ace-builds/src-noconflict/mode-json';
//import 'ace-builds/src-noconflict/theme-chrome';

//if worker is needed
//set useWorker: true
//
//import workerUrl from 'ace-builds/src-noconflict/worker-json?url';
//ace.config.setModuleUrl('ace/mode/json_worker', workerUrl);

export default {
    name: 'ScenarioEditor',
    components: { VAceEditor },
    props: {
        questionnaireId: { type: String, required: true },
        scenario: { type: Object, required: true },
    },
    data() {
        return {
            isOpen: false,
            scenarioSteps: '',
            scenarioSteps2: ''
        }
    },
    methods: {
        async show() {
            var scenario = await getScenarioSteps(this.questionnaireId, this.scenario.id);

            //client works only with json by supplying 'application/json'
            //server return string with json content
            var replaced = scenario.length >= 2 ? scenario.slice(1, -1).replaceAll('\\"', '"') : "";
            var parced1 = JSON.parse(replaced);
            var scenarioFormatted = JSON.stringify(parced1, null, 4);

            this.scenarioSteps = scenarioFormatted;
            this.isOpen = true;
        }
    }
}
</script>
