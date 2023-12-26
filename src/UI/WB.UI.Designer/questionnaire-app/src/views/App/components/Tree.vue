<template>
    <div class="questionnaire-tree-holder col-xs-6">
        <div class="chapter-title" v-switch on="filtersBoxMode"
            :class="{ selected: currentChapter.itemId === selectedItemId }" @click="
                router.push({
                    name: 'group',
                    params: {
                        groupId: currentChapter.itemId,
                        chapterId: currentChapter.itemId
                    }
                })
                ">
            <div @click.stop class="search-box" v-if="search.open">
                <div class="input-group">
                    <span class="input-group-addon glyphicon glyphicon-search"
                        :title="$t('QuestionnaireEditor.Search')"></span>
                    <input id="chapterSearch" type="text" v-model="search.searchText" v-model-options="{ debounce: 300 }"
                        focus-on-out="focusSearch" hotkey="{esc: hideSearch}" hotkey-allow-in="INPUT" />
                    <span class="input-group-addon glyphicon glyphicon-option-horizontal pointer"
                        @click="showFindReplaceDialog()" :title="$t('QuestionnaireEditor.FindReplaceTitle')"></span>
                </div>
                <button @click.stop="hideSearch()" type="button" :title="$t('QuestionnaireEditor.Cancel')"></button>
            </div>

            <div v-if="!search.open" class="chapter-name">
                <router-link :id="'group-' + currentChapter.itemId" class="chapter-title-text" :to="{
                    name: 'group',
                    params: {
                        groupId: chapterId,
                        chapterId: chapterId
                    }
                }">
                    <span v-text="currentChapter.title"></span>
                    <span v-if="currentChapter.isCover && currentChapter.isReadOnly
                        " class="warning-message">
                        {{ $t('QuestionnaireEditor.VirtualCoverPage') }}</span>
                    <help v-if="currentChapter.isCover && currentChapter.isReadOnly
                        " key="virtualCoverPage" />
                    <a v-if="!questionnaire.isReadOnlyForUser &&
                        currentChapter.isCover &&
                        currentChapter.isReadOnly" href="javascript:void(0);" @click.stop="migrateToNewVersion()">{{
        $t('QuestionnaireEditor.MigrateToNewCover') }}</a>
                </router-link>
                <div class="qname-block chapter-condition-block">
                    <div class="conditions-block">
                        <div class="enabliv-group-marker" :class="{
                            'hide-if-disabled':
                                currentChapter.hideIfDisabled
                        }" v-if="currentChapter.hasCondition"></div>
                    </div>
                </div>
                <ul class="controls-right">
                    <li>
                        <a href="javascript:void(0);" @click.stop="showSearch()" class="search"
                            :title="$t('QuestionnaireEditor.ToggleSearch')"></a>
                    </li>
                </ul>
            </div>
        </div>
        <perfect-scrollbar class="scroller">
            <div class="question-list" ui-tree="groupsTree" data-bs-empty-placeholder-enabled="false">
                <div ui-tree-nodes vmodel="items" class="ui-tree-nodes angular-ui-tree-nodes">
                    <Draggable ref="tree" v-model="filteredTreeData" textKey="title" childrenKey="items" defaultOpen="true"
                        indent="30" triggerClass="handler" :statHandler="treeNodeCreated" @after-drop="treeNodeDropped">
                        <template #default="{ node, stat }">
                            <component :key="node.itemId" :is="itemTemplate(node.itemType)" :item="node" :stat="stat"
                                :tree="$refs.tree" :selectedItemId="selectedItemId"></component>
                        </template>
                        <!--template #placeholder="{ node, stat }">
                            <div class="ngular-ui-tree-placeholder"></div>
                        </template-->
                    </Draggable>
                    <!--div
                        vrepeat="item in items | filter:searchItem as results"
                        ui-tree-node
                        data-bs-nodrag="{{ currentChapter.isReadOnly }}"
                    >
                        <v-include
                            src="itemTemplate(item.itemType)"
                        ></v-include>
                    </div-->
                    <div class="section item" v-if="search.open &&
                        search.searchText &&
                        filteredTreeData.length === 0
                        ">
                        <div class="item-text">
                            <span v-t="{
                                path: 'QuestionnaireEditor.NothingFound'
                            }"></span>
                        </div>
                    </div>
                    <div class="chapter-level-buttons" v-show="!search.searchText">
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addQuestion(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddQuestion' }"></button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly &&
                            !currentChapter.isCover
                            " @click="addGroup(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddSubsection' }"></button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly &&
                            !currentChapter.isCover
                            " @click="addRoster(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddRoster' }"></button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addStaticText(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddStaticText' }"></button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addVariable(currentChapter)"
                            v-t="{ path: 'QuestionnaireEditor.AddVariable' }"></button>

                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="searchForQuestion(currentChapter)" v-t="{
        path: 'QuestionnaireEditor.SearchForQuestion'
    }"></button>

                        <input type="button" class="btn lighter-hover pull-right" :disabled="!readyToPaste" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " :value="$t('QuestionnaireEditor.Paste')" @click="pasteItemInto(currentChapter)" />
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
</template>

<script>
import { useTreeStore } from '../../../stores/tree';
import { Draggable, dragContext, walkTreeData } from '@he-tree/vue';
import '@he-tree/vue/style/default.css';
import { ref, nextTick } from 'vue';

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
    inject: ['questionnaire'],
    props: {
        questionnaireId: { type: String, required: true },
        chapterId: { type: String, required: true }
    },
    data() {
        return {
            /*currentChapter: {
                title: null,
                isCover: false,
                isReadOnly: true,
                itemId: ''
            },*/
            //treeData: [],
            search: {
                open: false,
                searchText: null
            }
        };
    },
    setup() {
        const treeStore = useTreeStore();

        return {
            treeStore
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    computed: {
        currentChapter() {
            return this.treeStore.getChapter || {};
        },
        treeData() {
            return this.treeStore.getItems || {};
        },
        showStartScreen() {
            return true; // TODO
        },
        filteredTreeData() {
            if (!this.search.open || !this.search.searchText)
                return this.treeData;

            let results = [];
            walkTreeData(
                this.treeData,
                (node, index, parent) => {
                    if (
                        node.title &&
                        node.title.includes(this.search.searchText)
                    ) {
                        results.push(node);
                    } else if (
                        node.text &&
                        node.text.includes(this.search.searchText)
                    ) {
                        results.push(node);
                    } else if (
                        node.variable &&
                        node.variable.includes(this.search.searchText)
                    ) {
                        results.push(node);
                    }
                },
                {
                    childrenKey: 'items',
                    reverse: false,
                    childFirst: false
                }
            );
            return results;
        },
        selectedItemId() {
            return (
                this.$route.params.variableId ||
                this.$route.params.statictextId ||
                this.$route.params.questionId ||
                this.$route.params.groupId ||
                this.$route.params.rosterId
            );
        },
        readyToPaste() {
            return this.treeStore.canPaste();
        },
        isReadOnly() {
            return (
                this.questionnaire.isReadOnlyForUser ||
                this.currentChapter.isReadOnly
            );
        }
    },
    methods: {
        async fetch() {
            await this.treeStore.fetchTree(
                this.questionnaireId,
                this.chapterId
            );
            //this.currentChapter = this.treeStore.getChapter;
            //this.treeData = this.treeStore.getItems;
        },
        itemTemplate(itemType) {
            return 'Tree' + itemType;
        },
        addQuestion(chapter) {
            this.treeStore.addQuestion(
                chapter,
                null,
                (question, parent, index) => {
                    if (index < 0) index = this.$refs.tree.rootChildren.length;
                    this.$refs.tree.add(question, null, index);

                    this.$router.push({
                        name: 'question',
                        params: {
                            questionId: question.itemId
                        }
                    });
                }
            );
        },
        addGroup(chapter) {
            this.treeStore.addGroup(chapter, null, (group, parent, index) => {
                if (index < 0) index = this.$refs.tree.rootChildren.length;
                this.$refs.tree.add(group, null, index);

                this.$router.push({
                    name: 'group',
                    params: {
                        groupId: group.itemId
                    }
                });
            });
        },
        addRoster(chapter) {
            this.treeStore.addRoster(chapter, null, (roster, parent, index) => {
                if (index < 0) index = this.$refs.tree.rootChildren.length;
                this.$refs.tree.add(roster, null, index);

                this.$router.push({
                    name: 'roster',
                    params: {
                        rosterId: roster.itemId
                    }
                });
            });
        },
        addStaticText(chapter) {
            this.treeStore.addStaticText(
                chapter,
                null,
                (statictext, parent, index) => {
                    if (index < 0) index = this.$refs.tree.rootChildren.length;
                    this.$refs.tree.add(statictext, null, index);

                    this.$router.push({
                        name: 'statictext',
                        params: {
                            statictextId: statictext.itemId
                        }
                    });
                }
            );
        },
        addVariable(chapter) {
            this.treeStore.addVariable(
                chapter,
                null,
                (variable, parent, index) => {
                    if (index < 0) index = this.$refs.tree.rootChildren.length;
                    this.$refs.tree.add(variable, null, index);

                    this.$router.push({
                        name: 'variable',
                        params: {
                            variableId: variable.itemId
                        }
                    });
                }
            );
        },
        searchForQuestion(chapter) { },
        pasteItemInto(chapter) {
            this.treeStore.pasteItemInto(chapter).then(function (result) {
                if (!chapter.isCover)
                    this.$router.push({
                        name: result.itemType,
                        params: {
                            itemId: result.itemId
                        }
                    });
            });
        },
        migrateToNewVersion() { },
        showFindReplaceDialog() { },
        async showSearch() {
            this.search.open = true;
            await nextTick();
            document.getElementById('chapterSearch').focus();
        },
        hideSearch() {
            this.search.open = false;
            this.search.searchText = '';
        },
        treeNodeCreated(stat) {
            if (this.isReadOnly) {
                stat.droppable = false;
                stat.draggable = false;
            } else {
                const isGroup = stat.data.itemType == 'Group';
                stat.droppable = isGroup;
            }

            return stat;
        },
        treeNodeDropped() {
            const item = dragContext.dragNode.data;
            const parentId =
                ((dragContext.targetInfo.parent || {}).data || {}).itemId ||
                this.chapterId;
            let index = dragContext.targetInfo.indexBeforeDrop;

            const start = dragContext.startInfo;
            if (start.parent == dragContext.targetInfo.parent) {
                const startIndex = start.indexBeforeDrop;
                if (startIndex == index) return;
                if (startIndex < index) index--;
            }

            this.treeStore.moveItem(item, parentId, index);
        }
    }
};
</script>
