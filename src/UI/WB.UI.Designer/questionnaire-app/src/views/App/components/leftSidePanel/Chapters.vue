<template>
    <div class="chapters">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarSectionsCounter', {
                    count: chapters.length
                }) }}</span>
            </h3>
            <ul ui-tree-nodes ng-model="questionnaire.chapters" class="chapters-list angular-ui-tree-nodes">
                <Draggable ref="chapters" v-model="questionnaire.chapters" textKey="title" childrenKey="items"
                    :defaultOpen="false" triggerClass="handler" :statHandler="treeNodeCreated"
                    @after-drop="treeNodeDropped">
                    <template #default="{ node, stat }">


                        <li class="chapter-panel-item" :class="{ current: isCurrentChapter(node) }"
                            v-contextmenu="'chapter-context-menu-' + node.itemId">
                            <div class="holder" @click="editChapter(node)">
                                <div class="inner">
                                    <a class="handler" ui-tree-handle
                                        v-if="!isReadOnlyForUser && !node.isCover"><span></span></a>
                                    <router-link class="chapter-panel-item-body" :id="node.itemId" :to="{
                                        name: 'group',
                                        params: {
                                            chapterId: node.itemId,
                                            groupId: node.itemId,
                                        }
                                    }">
                                        <span v-dompurify-html="node.title"></span>
                                        <help link="coverPage" v-if="node.isCover" />
                                    </router-link>
                                    <div class="qname-block chapter-panel-item-condition">
                                        <div class="conditions-block">
                                            <div class="enabling-group-marker"
                                                :class="{ 'hide-if-disabled': node.hideIfDisabled }"
                                                v-if="node.hasCondition">
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="dropdown position-fixed" :id="'chapter-context-menu-' + node.itemId">
                                <ul class="dropdown-menu" role="menu">
                                    <li><a @click="editChapter(chapter);">{{ $t('QuestionnaireEditor.Open') }}</a></li>
                                    <li><a @click.self="copyRef(node);">{{ $t('QuestionnaireEditor.Copy') }}</a></li>
                                    <li>
                                        <a :disabled="!readyToPaste" @click.self="pasteAfterChapter(node)"
                                            v-if="!isReadOnlyForUser && !node.isReadOnly">{{
                                                $t('QuestionnaireEditor.PasteAfter') }}</a>
                                    </li>
                                    <li><a @click.self="deleteChapter(node, stat)"
                                            v-if="!isReadOnlyForUser && !node.isCover">{{
                                                $t('QuestionnaireEditor.Delete') }}</a></li>
                                </ul>
                            </div>
                        </li>

                    </template>
                </Draggable>
            </ul>
            <div class="button-holder">
                <button type="button" class="btn lighter-hover" value="ADD NEW SECTION" @click="addNewChapter()"
                    v-if="!isReadOnlyForUser">{{ $t('QuestionnaireEditor.AddNewSection')
                    }}</button>
            </div>
        </perfect-scrollbar>
    </div>
</template>
  
<script>
import { useTreeStore } from '../../../../stores/tree';
import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'

import { Draggable, dragContext, walkTreeData } from '@he-tree/vue';
import Help from '../Help.vue';

export default {
    name: 'Chapters',
    props: {
        questionnaireId: { type: String, required: true },
    },
    components: {
        Help,
        Draggable,
    },
    //inject: ['currentChapter'],
    data() {
        return {

        }
    },
    setup() {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
            treeStore,
        };
    },
    computed: {
        questionnaire() {
            return (this.questionnaireStore || {}).info || {};
        },
        chapters() {
            return ((this.questionnaire || {}).chapters || [])
        },

        readyToPaste() {
            return this.treeStore.canPaste();
        },

        isReadOnlyForUser() {
            return (this.questionnaire || {}).isReadOnlyForUser;
        },
        currentChapterData() {
            return this.treeStore.getChapterData || {};
        },
    },
    methods: {
        treeNodeCreated(stat) {
            if (this.isReadOnly || this.isCover) {
                stat.droppable = false;
                stat.draggable = false;
            } else {
                stat.droppable = true;
                stat.droppable = false;
            }

            return stat;
        },
        treeNodeDropped() {
            const item = dragContext.dragNode.data;
            const parentId = null;
            let index = dragContext.targetInfo.indexBeforeDrop;

            const start = dragContext.startInfo;
            const startIndex = start.indexBeforeDrop;
            if (startIndex == index) return;
            if (startIndex < index) index--;

            this.treeStore.moveGroup(item.itemId, index, parentId)
        },

        editChapter(chapter) {
            this.$router.push({
                name: 'group',
                params: {
                    chapterId: chapter.itemId,
                    groupId: chapter.itemId
                }
            });

            this.closePanel();
        },

        pasteAfterChapter(chapter) {
            if (!this.treeStore.canPaste()) return;

            this.treeStore.pasteItemAfter(chapter).then(result => {
                if (!chapter.isCover)
                    this.$router.push({
                        name: result.itemType,
                        params: {
                            [name + 'Id']: result.itemId
                        }
                    });
                this.closePanel();
            });
        },

        copyRef(chapter) {
            this.treeStore.copyItem(chapter);
        },

        deleteChapter(chapter, stat) {
            var itemIdToDelete = chapter.itemId;

            const params = createQuestionForDeleteConfirmationPopup(
                chapter.title ||
                this.$t('QuestionnaireEditor.UntitledSection')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.questionnaireStore
                        .deleteSection(itemIdToDelete)
                        .then(response => {
                            this.$refs.chapters.remove(stat);

                            if (this.isCurrentChapter(chapter)) {
                                const cover = this.chapters[0];
                                this.$router.push({
                                    name: 'group',
                                    params: {
                                        chapterId: cover.itemId,
                                        groupId: cover.itemId
                                    }
                                });
                            }

                            this.closePanel();
                        });
                }
            };

            this.$confirm(params);
        },

        isCurrentChapter(chapter) {
            if (this.currentChapterData)
                return chapter.itemId == this.currentChapterData.itemId;
            return false;
        },

        addNewChapter() {
            this.questionnaireStore.addSection(section => {
                let index = this.$refs.chapters.rootChildren.length;
                this.$refs.chapters.add(section, null, index);

                this.$router.push({
                    name: 'group',
                    params: {
                        chapterId: section.itemId,
                        groupId: section.itemId
                    }
                });

                this.closePanel();
            });
        },

        closePanel() {
            this.$emitter.emit("closeChaptersList", {});
        },
    }
}
</script>
  