<template>
    <div class="chapters">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarSectionsCounter', {
                    count: chapters.length
                }) }}</span>
            </h3>
            <ul ui-tree-nodes ng-model="questionnaire.chapters" class="chapters-list angular-ui-tree-nodes">
                <Draggable ref="tree" v-model="questionnaire.chapters" textKey="title" childrenKey="items"
                    :defaultOpen="false" triggerClass="handler" :statHandler="treeNodeCreated"
                    @after-drop="treeNodeDropped">
                    <template #default="{ node, stat }">


                        <li class="chapter-panel-item" ui-tree-node :data-nodrag="node.isCover"
                            :class="{ current: isCurrentChapter(node) }" context-menu
                            :data-target="'chapter-context-menu-' + node.itemId" context-menu-hide-on-mouse-leave="true">
                            <div class="holder" @click.stop="editChapter(node)">
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


                                    <!--a class="chapter-panel-item-body"
                                        ui-sref="questionnaire.chapter.group({ chapterId: chapter.itemId, itemId: chapter.itemId})">
                                        <span v-dompurify-html="node.title"></span>
                                        <help link="coverPage" v-if="node.isCover" />
                                    </a-->
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
                                    <li><a @click="copyRef(node);">{{ $t('QuestionnaireEditor.Copy') }}</a></li>
                                    <li>
                                        <a :disabled="!readyToPaste" @click.self="pasteAfterChapter(node)"
                                            v-if="!isReadOnlyForUser && !node.isReadOnly">{{
                                                $t('QuestionnaireEditor.PasteAfter') }}</a>
                                    </li>
                                    <li><a @click.self="deleteChapter(node)" v-if="!isReadOnlyForUser && !node.isCover">{{
                                        $t('QuestionnaireEditor.Delete') }}</a></li>
                                </ul>
                            </div>
                        </li>

                    </template>
                </Draggable>

                <!--li class="chapter-panel-item" v-for="chapter in questionnaire.chapters" ui-tree-node
                    data-nodrag="{{ chapter.isCover }}" :class="{ current: isCurrentChapter(chapter) }" context-menu
                    data-target="chapter-context-menu-{{ chapter.itemId }}" context-menu-hide-on-mouse-leave="true">
                    <div class="holder" @click="editChapter(chapter); $event.stopPropagation();">
                        <div class="inner">
                            <a class="handler" ui-tree-handle
                                v-if="!isReadOnlyForUser && !chapter.isCover"><span></span></a>
                            <a class="chapter-panel-item-body"
                                ui-sref="questionnaire.chapter.group({ chapterId: chapter.itemId, itemId: chapter.itemId})">
                                <span v-dompurify-html="chapter.title"></span>
                                <help link="coverPage" v-if="chapter.isCover" />
                            </a>
                            <div class="qname-block chapter-panel-item-condition">
                                <div class="conditions-block">
                                    <div class="enabling-group-marker"
                                        :class="{ 'hide-if-disabled': chapter.hideIfDisabled }" v-if="chapter.hasCondition">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="dropdown position-fixed" id="chapter-context-menu-{{ chapter.itemId }}">
                        <ul class="dropdown-menu" role="menu">
                            <li><a @click="editChapter(chapter);">{{ $t('QuestionnaireEditor.Open') }}</a></li>
                            <li><a @click="copyRef(chapter);">{{ $t('QuestionnaireEditor.Copy') }}</a></li>
                            <li>
                                <a :disabled="!readyToPaste" @click.self="pasteAfterChapter(chapter)"
                                    v-if="!isReadOnlyForUser && !chapter.isReadOnly">{{
                                        $t('QuestionnaireEditor.PasteAfter') }}</a>
                            </li>
                            <li><a @click.self="deleteChapter(chapter)" v-if="!isReadOnlyForUser && !chapter.isCover">{{
                                $t('QuestionnaireEditor.Delete') }}</a></li>
                        </ul>
                    </div>
                </li-->
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
            treeStore
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

        },

        copyRef(chapter) {

        },

        isCurrentChapter(chapter) {
            if (this.currentChapterData)
                return chapter.itemId == this.currentChapterData.itemId;
            return false;
        },

        addNewChapter() {

        }
    }
}
</script>
  