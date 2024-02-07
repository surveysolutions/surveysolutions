<template>
    <div class="questionnaire-tree-holder col-xs-6">
        <div class="chapter-title" :class="{ selected: chapterId === selectedItemId }" @click.stop="
            $router.push({
                name: 'group',
                params: {
                    entityId: chapterId,
                    chapterId: chapterId
                }
            })
            ">
            <div @click.stop class="search-box" v-if="search.open">
                <div class="input-group">
                    <span class="input-group-addon glyphicon glyphicon-search"
                        :title="$t('QuestionnaireEditor.Search')"></span>
                    <input id="chapterSearch" type="text" v-model="search.searchText" focus-on-out="focusSearch"
                        hotkey="{esc: hideSearch}" hotkey-allow-in="INPUT" />
                    <span class="input-group-addon glyphicon glyphicon-option-horizontal pointer"
                        @click="showFindReplaceDialog()" :title="$t('QuestionnaireEditor.FindReplaceTitle')"></span>
                </div>
                <button @click.stop="hideSearch()" type="button" :title="$t('QuestionnaireEditor.Cancel')"></button>
            </div>

            <div v-if="!search.open" class="chapter-name">
                <router-link :id="'group-' + chapterId" class="chapter-title-text" :to="{
                    name: 'group',
                    params: { entityId: chapterId, chapterId: chapterId }
                }">
                    <span v-text="currentChapterData.title"></span>
                    <span v-if="currentChapterData.isCover && currentChapterData.isReadOnly
                        " class="warning-message">
                        {{ $t('QuestionnaireEditor.VirtualCoverPage') }}</span>
                    <help v-if="currentChapterData.isCover && currentChapterData.isReadOnly" link="virtualCoverPage" />
                    <a v-if="!questionnaire.isReadOnlyForUser &&
                        currentChapterData.isCover &&
                        currentChapterData.isReadOnly" href="javascript:void(0);" @click.stop="migrateToNewVersion()">
                        {{ $t('QuestionnaireEditor.MigrateToNewCover') }}</a>
                </router-link>
                <div class="qname-block chapter-condition-block">
                    <div class="conditions-block">
                        <div class="enabling-group-marker"
                            :class="{ 'hide-if-disabled': currentChapterData.hideIfDisabled }"
                            v-if="currentChapterData.hasCondition">
                        </div>
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
                    <Draggable ref="tree" v-model="filteredTreeData" textKey="title" childrenKey="items" :defaultOpen="true"
                        :maxLevel="10" :indent="30" triggerClass="handler" :keepPlaceholder="true"
                        :statHandler="treeNodeCreated" @after-drop="treeNodeDropped">
                        <template #default="{ node, stat }">
                            <component :key="node.itemId" :is="itemTemplate(node.itemType)" :item="node" :stat="stat"
                                :tree="$refs.tree" :selectedItemId="selectedItemId"></component>
                        </template>

                        <!--template #placeholder>
                            <div class="angular-ui-tree-placeholder"></div>
                        </template-->
                    </Draggable>

                    <div class="section item" v-if="search.open &&
                        search.searchText &&
                        filteredTreeData.length === 0
                        ">
                        <div class="item-text">
                            <span>{{ $t('QuestionnaireEditor.NothingFound') }}</span>
                        </div>
                    </div>
                    <div class="chapter-level-buttons" v-show="!search.searchText">
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addQuestion(currentChapterData)">{{
        $t('QuestionnaireEditor.AddQuestion') }}</button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly &&
                            !currentChapter.isCover" @click="addGroup(currentChapterData)">{{
        $t('QuestionnaireEditor.AddSubsection') }}</button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly &&
                            !currentChapter.isCover
                            " @click="addRoster(currentChapterData)">{{ $t('QuestionnaireEditor.AddRoster') }}</button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addStaticText(currentChapterData)">{{
        $t('QuestionnaireEditor.AddStaticText') }}</button>
                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="addVariable(currentChapterData)">{{
        $t('QuestionnaireEditor.AddVariable') }}</button>

                        <button type="button" class="btn lighter-hover" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            " @click="searchForQuestion(currentChapterData)">{{
        $t('QuestionnaireEditor.SearchForQuestion')
    }}</button>

                        <input type="button" class="btn lighter-hover pull-right" :disabled="!readyToPaste" v-if="!questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                                " :value="$t('QuestionnaireEditor.Paste')" @click="pasteItemInto(currentChapter)" />
                    </div>
                </div>

                <div class="start-box" v-if="showStartScreen">
                    <p>{{ $t('QuestionnaireEditor.EmptySectionLine1') }}</p>
                    <p>
                        <span v-dompurify-html="emptySectionHtmlLine1"> </span>
                        <br />
                        <span v-dompurify-html="emptySectionHtmlLine3">
                            <span class="left-panel-glyph"></span>
                        </span>
                    </p>

                    <p>
                        <span>{{ $t('QuestionnaireEditor.EmptySectionLine4') }}</span>
                        <br />
                        <span v-dompurify-html="emptySectionHtmlLine2"></span>
                    </p>
                </div>
            </div>
        </perfect-scrollbar>
    </div>
    <SearchDialog ref="searchDialog" :questionnaireId="questionnaireId" />
</template>

<style lang="scss">
.questionnaire-tree-holder {
    .drag-placeholder {
        height: 36px;
        border: none;
        background: rgba(0, 0, 0, 0.1);
    }
}
</style>

<script>
import { useTreeStore } from '../../../stores/tree';
import { Draggable, dragContext, walkTreeData } from '@he-tree/vue';
import '@he-tree/vue/style/default.css';
import { ref, nextTick } from 'vue';
import SearchDialog from './SearchDialog.vue';
import { moveItem } from '../../../services/questionnaireService'

import TreeGroup from './TreeGroup.vue';
import TreeQuestion from './TreeQuestion.vue';
import TreeStaticText from './TreeStaticText.vue';
import TreeVariable from './TreeVariable.vue';

import Help from './Help.vue';

import { migrateToNewVersion } from '../../../services/questionnaireService'

export default {
    name: 'Tree',
    components: {
        Draggable,
        TreeGroup,
        TreeQuestion,
        TreeStaticText,
        TreeVariable,
        SearchDialog,

        Help,
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
    watch: {
        async chapterId(newValue, oldValue) {
            if (newValue != oldValue) {
                //this.treeStore.clear();
                await this.fetch();
            }
        }
    },
    setup() {
        const treeStore = useTreeStore();

        const searchDialog = ref(null);
        return {
            treeStore,
            searchDialog
        };
    },
    async beforeMount() {
        await this.fetch();
    },
    mounted() {
        this.$emitter.on('questionDeleted', this.treeNodeDeleted);
        this.$emitter.on('staticTextDeleted', this.treeNodeDeleted);
        this.$emitter.on('variableDeleted', this.treeNodeDeleted);
        this.$emitter.on('groupDeleted', this.treeNodeDeleted);
        this.$emitter.on('rosterDeleted', this.treeNodeDeleted);

        this.$emitter.on('questionMoved', this.treeNodeMoved);
        this.$emitter.on('staticTextMoved', this.treeNodeMoved);
        this.$emitter.on('variableMoved', this.treeNodeMoved);
        this.$emitter.on('groupMoved', this.treeNodeMoved);
    },
    unmounted() {
        this.$emitter.off('questionDeleted', this.treeNodeDeleted);
        this.$emitter.off('staticTextDeleted', this.treeNodeDeleted);
        this.$emitter.off('variableDeleted', this.treeNodeDeleted);
        this.$emitter.off('groupDeleted', this.treeNodeDeleted);
        this.$emitter.off('rosterDeleted', this.treeNodeDeleted);

        this.$emitter.off('questionMoved', this.treeNodeMoved);
        this.$emitter.off('staticTextMoved', this.treeNodeMoved);
        this.$emitter.off('variableMoved', this.treeNodeMoved);
        this.$emitter.off('groupMoved', this.treeNodeMoved);
    },
    computed: {
        currentChapterData() {
            return this.treeStore.getChapterData || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        },
        treeData() {
            return this.treeStore.getItems || {};
        },
        showStartScreen() {
            const count = this.treeData.length;
            return count == null || count == 0; // TODO
        },
        filteredTreeData() {
            if (!this.search.open || !this.search.searchText)
                return this.treeData;

            const searchUpper = this.search.searchText.toUpperCase()
            let results = [];
            walkTreeData(
                this.treeData,
                (node, index, parent) => {
                    if (
                        node.title &&
                        node.title.toUpperCase().includes(searchUpper)
                    ) {
                        results.push(node);
                    } else if (
                        node.text &&
                        node.text.toUpperCase().includes(searchUpper)
                    ) {
                        results.push(node);
                    } else if (
                        node.variable &&
                        node.variable.toUpperCase().includes(searchUpper)
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
                this.$route.params.entityId
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
        },
        emptySectionHtmlLine1() {
            var emptySectionAddQuestion = "<button class='btn' disabled type='button'>" + this.$t('QuestionnaireEditor.AddQuestion') + " </button>";
            var emptySectionAddSubsectionHtml = "<button class=\"btn\" disabled type=\"button\">" + this.$t('QuestionnaireEditor.AddSubsection') + " </button>";
            return this.$t('QuestionnaireEditor.EmptySectionLine2', { addQuestionBtn: emptySectionAddQuestion, addSubsectionBtn: emptySectionAddSubsectionHtml });
        },
        emptySectionHtmlLine2() {
            var emptySectionSettingsHtml = "<button class=\"btn\" type=\"button\" disabled>" + this.$t('QuestionnaireEditor.Settings') + " </button>";
            return this.$t('QuestionnaireEditor.EmptySectionLine5', { settingsBtn: emptySectionSettingsHtml });
        },
        emptySectionHtmlLine3() {
            const panel = '<span class="left-panel-glyph"></span>';
            return this.$t('QuestionnaireEditor.EmptySectionLine3', { panel: panel });
        }
    },
    methods: {
        async fetch() {
            await this.treeStore.fetchTree(
                this.questionnaireId,
                this.chapterId
            );
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
                            entityId: question.itemId
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
                        entityId: group.itemId
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
                        entityId: roster.itemId
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
                            entityId: statictext.itemId
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
                            entityId: variable.itemId
                        }
                    });
                }
            );
        },
        searchForQuestion(chapter) { },
        pasteItemInto(chapterInfo) {
            this.treeStore.pasteItemInto(chapterInfo.chapter).then(function (result) {
                if (!chapterInfo.isCover)
                    this.$router.push({
                        name: result.itemType,
                        params: {
                            entityId: result.itemId
                        }
                    });
            });
        },
        migrateToNewVersion() {
            migrateToNewVersion(this.questionnaireId).then(function () {
                document.location.reload();
            });
        },
        showFindReplaceDialog() {
            this.searchDialog.open();
        },
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
                stat.draggable = true;
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

            moveItem(this.questionnaireId, item.itemId, item.itemType, parentId, index);
        },

        treeNodeDeleted(data) {
            const itemId = data.itemId;

            const tree = this.$refs.tree;
            let stat = null;
            const items = tree.rootChildren;
            walkTreeData(
                items,
                (node, index, parent) => {
                    if (node.data.itemId == itemId) {
                        stat = node;
                        return 'false';
                    }
                },
                {
                    childrenKey: 'children',
                    reverse: false,
                    childFirst: false
                }
            );

            if (stat != null) {
                tree.remove(stat);

                this.$router.push({
                    name: 'group',
                    params: {
                        entityId: this.chapterId,
                        chapterId: this.chapterId
                    }
                });
            }
        },

        treeNodeMoved(data) {
            const itemId = data.itemId;
            const newParentId = data.newParentId;

            const tree = this.$refs.tree;
            let itemStat = null;
            let newParentStat = null;
            const items = tree.rootChildren;
            walkTreeData(
                items,
                (node, index, parent) => {
                    if (node.data.itemId == itemId) {
                        itemStat = node;
                    }
                    else if (node.data.itemId == newParentId) {
                        newParentStat = node;
                    }
                    if (newParentStat != null && itemStat != null) {
                        return 'false';
                    }
                },
                {
                    childrenKey: 'children',
                    reverse: false,
                    childFirst: false
                }
            );

            if (itemStat != null) {
                if (newParentStat != null) {
                    tree.move(itemStat, newParentStat, data.newIndex);
                }
                else {
                    if (newParentId == this.currentChapter.chapter.itemId) {
                        tree.move(itemStat, null, data.newIndex);
                    } else {
                        tree.remove(itemStat);
                    }
                }
            }
        }
    }
};
</script>
