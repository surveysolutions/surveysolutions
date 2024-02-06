<template>
    <div class="chapters">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarSectionsCounter', {
                    count: questionnaire.chapters.length
                }) }}</span>
            </h3>
            <ul ui-tree-nodes class="chapters-list angular-ui-tree-nodes">
                <Draggable ref="chapters" v-model="questionnaire.chapters" textKey="title" childrenKey="items"
                    :defaultOpen="false" triggerClass="handler" :keepPlaceholder="true" :statHandler="treeNodeCreated"
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
                                            entityId: node.itemId,
                                        }
                                    }">
                                        <span v-dompurify-html="node.title"></span>
                                        <span>&nbsp;</span>
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
                                    <li><a @click="editChapter(node);">{{ $t('QuestionnaireEditor.Open') }}</a></li>
                                    <li><a @click.self="copyRef(node);">{{ $t('QuestionnaireEditor.Copy') }}</a></li>
                                    <li>
                                        <a @click.self="pasteAfterChapter(node)"
                                            v-if="readyToPaste && !isReadOnlyForUser && !node.isReadOnly">{{
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
  
<style lang="scss">
.chapters {
    .drag-placeholder {
        height: 53px;
        border: none;
        background: rgba(0, 0, 0, 0.1);
    }
}
</style>

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
    inject: ['questionnaire'],
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
            const oldIndex = dragContext.startInfo.indexBeforeDrop;
            let newIndex = dragContext.targetInfo.indexBeforeDrop;
            if (newIndex == 0) {
                this.$refs.chapters.move(dragContext.dragNode, null, oldIndex)
                return;
            }

            if (oldIndex == newIndex) return;
            if (oldIndex < newIndex) newIndex--;

            const item = dragContext.dragNode.data;
            this.treeStore.moveGroup(item.itemId, newIndex, null);
        },

        editChapter(chapter) {
            this.$router.push({
                name: 'group',
                params: {
                    chapterId: chapter.itemId,
                    entityId: chapter.itemId
                }
            });

            this.closePanel();
        },

        pasteAfterChapter(chapter) {
            if (!this.treeStore.canPaste()) return false;

            this.questionnaireStore.pasteItemAfter(chapter).then(() => {
                //this.closePanel();
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
                                const cover = this.questionnaire.chapters[0];
                                this.$router.push({
                                    name: 'group',
                                    params: {
                                        chapterId: cover.itemId,
                                        entityId: cover.itemId
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
                        entityId: section.itemId
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
  