<template>
    <div class="questionnaire-tree-holder col-xs-6">
        <div
            class="chapter-title"
            v-switch
            on="filtersBoxMode"
            ui-sref-active="selected"
            ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})"
        >
            <div @click.stop class="search-box" v-switch-when="search">
                <div class="input-group">
                    <span
                        class="input-group-addon glyphicon glyphicon-search"
                        >{{ $t('QuestionnaireEditor.Search') }}</span
                    >
                    <input
                        type="text"
                        v-model="search.searchText"
                        v-model-options="{ debounce: 300 }"
                        focus-on-out="focusSearch"
                        hotkey="{esc: hideSearch}"
                        hotkey-allow-in="INPUT"
                    />
                    <span
                        class="input-group-addon glyphicon glyphicon-option-horizontal pointer"
                        @click="showFindReplaceDialog()"
                        >{{ $t('QuestionnaireEditor.FindReplaceTitle') }}</span
                    >
                </div>
                <button @click.stop="hideSearch()" type="button">
                    {{ $t('QuestionnaireEditor.Cancel') }}
                </button>
            </div>

            <div v-switch-when="default" class="chapter-name">
                <a
                    id="group-{{currentChapter.itemId}}"
                    class="chapter-title-text"
                    ui-sref="questionnaire.chapter.group({ chapterId: currentChapter.itemId, itemId: currentChapter.itemId})"
                >
                    <span v-bind-html="currentChapter.title | escape"></span>
                    <span
                        v-if="
                            currentChapter.isCover && currentChapter.isReadOnly
                        "
                        class="warniv-message"
                    >
                        {{ $t('QuestionnaireEditor.VirtualCoverPage') }}</span
                    >
                    <help
                        v-if="
                            currentChapter.isCover && currentChapter.isReadOnly
                        "
                        key="virtualCoverPage"
                    />
                    <a
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                currentChapter.isCover &&
                                currentChapter.isReadOnly
                        "
                        href="javascript:void(0);"
                        @click.stop="migrateToNewVersion()"
                        >{{ $t('QuestionnaireEditor.MigrateToNewCover') }}</a
                    >
                </a>
                <div class="qname-block chapter-condition-block">
                    <div class="conditions-block">
                        <div
                            class="enabliv-group-marker"
                            :class="{
                                'hide-if-disabled':
                                    currentChapter.hideIfDisabled
                            }"
                            v-if="currentChapter.hasCondition"
                        ></div>
                    </div>
                </div>
                <ul class="controls-right">
                    <li>
                        <a
                            href="javascript:void(0);"
                            @click.stop="showSearch()"
                            class="search"
                            >{{ $t('QuestionnaireEditor.ToggleSearch') }}</a
                        >
                    </li>
                </ul>
            </div>
        </div>
        <perfect-scrollbar class="scroller">
            <div
                class="question-list"
                ui-tree="groupsTree"
                data-bs-empty-placeholder-enabled="false"
            >
                <Draggable
                    class="ui-tree-nodes"
                    v-model="treeData"
                    textKey="title"
                    childrenKey="items"
                    defaultOpen="true"
                >
                    <template #default="{ node, stat }">
                        <component
                            :key="node.itemId"
                            :is="itemTemplate(node.itemType)"
                            :id="node.itemId"
                        ></component>
                    </template>
                </Draggable>

                <div ui-tree-nodes vmodel="items">
                    <div
                        vrepeat="item in items | filter:searchItem as results"
                        ui-tree-node
                        data-bs-nodrag="{{ currentChapter.isReadOnly }}"
                    >
                        <v-include
                            src="itemTemplate(item.itemType)"
                        ></v-include>
                    </div>
                    <div
                        class="section item"
                        v-if="
                            filtersBoxMode == 'search' && results.length === 0
                        "
                    >
                        <div class="item-text">
                            <span v-i18next="NothingFound"></span>
                        </div>
                    </div>
                    <div
                        class="chapter-level-buttons"
                        v-show="!search.searchText"
                    >
                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            @click="addQuestion(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddQuestion' }"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly &&
                                    !currentChapter.isCover
                            "
                            @click="addGroup(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddSubsection' }"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly &&
                                    !currentChapter.isCover
                            "
                            @click="addRoster(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddRoster' }"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            @click="addStaticText(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddStaticText' }"
                        ></button>
                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            @click="addVariable(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddVariable' }"
                        ></button>

                        <button
                            type="button"
                            class="btn lighter-hover"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            @click="searchForQuestion(currentChapter)"
                            v-t="{
                                path: 'QuestionnaireEditor.SearchForQuestion'
                            }"
                        ></button>

                        <input
                            type="button"
                            class="btn lighter-hover pull-right"
                            v-disabled="!readyToPaste"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            v-t="{ path: 'QuestionnaireEditor.Paste' }"
                            @click="pasteItemInto(currentChapter)"
                        />
                    </div>
                </div>

                <div class="start-box" v-if="showStartScreen">
                    <p v-i18next="EmptySectionLine1"></p>
                    <p>
                        <span v-bind-html="emptySectionHtmlLine1"> </span>
                        <br />
                        <!--                        <span
                            v-i18next="[html]({panel: '&lt;span class=&quot;left-panel-glyph&quot;&gt;&lt;/span&gt;'})EmptySectionLine3"
                        >
                            <span class="left-panel-glyph"></span>
                        </span-->
                    </p>

                    <p>
                        <span v-i18next="EmptySectionLine4"></span>
                        <br />
                        <span v-bind-html="emptySectionHtmlLine2"></span>
                    </p>
                </div>
            </div>
        </perfect-scrollbar>
    </div>

    <div
        class="question-editor col-xs-6"
        v-class="{ commenting: isCommentsBlockVisible }"
        ui-view
    ></div>

    <div class="comments-editor col-xs-6" ui-view="comments"></div>
</template>

<script>
import { useTreeStore } from '../../../stores/tree';
import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { BaseTree, Draggable } from '@he-tree/vue';
import '@he-tree/vue/style/default.css';

import TreeGroup from './TreeGroup.vue';
import TreeQuestion from './TreeQuestion.vue';
import TreeStaticText from './TreeStaticText.vue';
import TreeVariable from './TreeVariable.vue';

export default {
    name: 'Tree',
    components: {
        Draggable,
        TreeGroup,
        TreeQuestion,
        TreeStaticText,
        TreeVariable
    },
    props: {
        questionnaireId: { type: String, required: true },
        chapterId: { type: String, required: true }
    },
    data() {
        return {
            currentChapter: {
                title: null,
                isCover: false
            },
            treeData: [],
            search: {
                searchText: null
            }
        };
    },
    setup() {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();

        return {
            treeStore,
            questionnaireStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info;
        },
        showStartScreen() {
            return true; // TODO
        }
    },
    methods: {
        async fetch() {
            await this.treeStore.fetchTree(
                this.questionnaireId,
                this.chapterId
            );
            this.currentChapter = this.treeStore.getChapter;
            this.treeData = this.treeStore.getItems;
        },
        itemTemplate(itemType) {
            return 'Tree' + itemType;
        },
        addQuestion(chapter) {},
        addGroup(chapter) {},
        addRoster(chapter) {},
        addStaticText(chapter) {},
        addVariable(chapter) {},
        searchForQuestion(chapter) {},
        pasteItemInto(chapter) {},
        migrateToNewVersion() {},
        showFindReplaceDialog() {},
        showSearch() {},
        hideSearch() {}
    }
};
</script>
