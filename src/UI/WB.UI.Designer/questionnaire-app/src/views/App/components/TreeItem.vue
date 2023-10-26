<template>
    <div
        context-menu
        class="section item"
        :data-bs-target="'context-menu-' + item.itemId"
        :class="itemClass"
        ui-sref-active="selected"
        context-menu-hide-on-mouse-leave="true"
        @contextmenu.prevent="onContextMenu($event)"
        v-click-outside="closeContextMenu"
        v-contextmenu-outside="closeContextMenu"
        @mouseenter="is_highlighted = true"
        @mouseleave="is_highlighted = false"
    >
        <span class="cursor"></span>
        <a class="handler" ui-tree-handle></a>

        <slot />

        <Teleport to="body">
            <div
                v-show="contextmenu.open"
                :style="{
                    top: contextmenu.y + 'px',
                    left: contextmenu.x + 'px'
                }"
                @click="closeContextMenu"
                class="dropdown position-fixed"
                :class="{ open: contextmenu.open }"
                :id="'context-menu-' + item.itemId"
            >
                <ul class="dropdown-menu" role="menu">
                    <li>
                        <a
                            @click="addQuestion()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            >{{ $t('QuestionnaireEditor.TreeAddQuestion') }}</a
                        >
                    </li>
                    <li>
                        <a
                            @click="addGroup()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly &&
                                    !currentChapter.isCover
                            "
                            >{{ $t('QuestionnaireEditor.TreeAddSection') }}</a
                        >
                    </li>
                    <li>
                        <a
                            @click="addRoster()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly &&
                                    !currentChapter.isCover
                            "
                            >{{ $t('QuestionnaireEditor.TreeAddRoster') }}</a
                        >
                    </li>
                    <li>
                        <a
                            @click="addStaticText()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            >{{
                                $t('QuestionnaireEditor.TreeAddStaticText')
                            }}</a
                        >
                    </li>
                    <li>
                        <a
                            @click="addVariable()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            >{{ $t('QuestionnaireEditor.TreeAddVariable') }}</a
                        >
                    </li>
                    <li>
                        <a @click="copyRef()">{{
                            $t('QuestionnaireEditor.Copy')
                        }}</a>
                    </li>
                    <li>
                        <a
                            :disabled="!readyToPaste"
                            @click="pasteItemAfter()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            >{{ $t('QuestionnaireEditor.PasteAfter') }}</a
                        >
                    </li>
                    <li>
                        <a
                            @click="deleteGroup()"
                            v-if="
                                !questionnaire.isReadOnlyForUser &&
                                    !currentChapter.isReadOnly
                            "
                            >{{ $t('QuestionnaireEditor.Delete') }}</a
                        >
                    </li>
                </ul>
            </div>
        </Teleport>
    </div>
    <!--div
        v-hide="collapsed"
        class="slide"
        :class="{ highlighted: is_highlighted, 'roster-items': item.isRoster }"
        ui-tree-nodes="item.items"
        v-model="item.items"
    >
        <div
            v-repeat="item in item.items | filter:searchItem"
            class="filter-animate"
            ui-tree-node
            v-include="itemTemplate(item.itemType)"
        ></div>
    </div-->
</template>

<script>
import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useTreeStore } from '../../../stores/tree';
import clickOutside from '../../../directives/clickOutside';
import contextmenuOutside from '../../../directives/contextmenuOutside';

export default {
    name: 'TreeItem',
    directives: {
        clickOutside,
        contextmenuOutside
    },
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        selectedItemId: { type: String, required: false }
    },
    data() {
        return {
            is_highlighted: false,
            contextmenu: {
                open: false,
                x: 0,
                y: 0
            }
        };
    },
    setup(props) {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();

        return {
            treeStore,
            questionnaireStore
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info || {};
        },
        currentChapter() {
            return this.treeStore.getChapter || {};
        },
        itemClass() {
            let classes = [];
            if (this.item.itemId === this.selectedItemId) {
                //classes.push('highlight');
                classes.push('selected');
            }
            if (this.is_highlighted) classes.push('highlighted');
            if (this.item.itemType)
                classes.push(this.item.itemType.toLowerCase());
            if (this.item.itemType === 'Group' && this.item.isRoster)
                classes.push('roster');
            if (this.stat.parent && this.stat.parent.data.isRoster)
                classes.push('roster-items');
            return classes;
        }
    },
    methods: {
        filter(value) {
            return value; // TODO | escape | highlight:search.searchText
        },
        onContextMenu(e) {
            e.preventDefault();

            this.contextmenu.x = e.x;
            this.contextmenu.y = e.y;
            this.contextmenu.open = true;
        },
        closeContextMenu() {
            this.contextmenu.open = false;
        },

        addQuestion() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            this.treeStore.addQuestion(
                parent,
                afterItemId,
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
        addGroup() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            this.treeStore.addGroup(
                parent,
                afterItemId,
                (group, parent, index) => {
                    if (index < 0) index = this.$refs.tree.rootChildren.length;
                    this.$refs.tree.add(group, null, index);

                    this.$router.push({
                        name: 'group',
                        params: {
                            groupId: group.itemId
                        }
                    });
                }
            );
        },
        addRoster() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            this.treeStore.addRoster(
                parent,
                afterItemId,
                (roster, parent, index) => {
                    if (index < 0) index = this.$refs.tree.rootChildren.length;
                    this.$refs.tree.add(roster, null, index);

                    this.$router.push({
                        name: 'roster',
                        params: {
                            rosterId: roster.itemId
                        }
                    });
                }
            );
        },
        addStaticText() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            this.treeStore.addStaticText(
                parent,
                afterItemId,
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
        addVariable() {
            const parent = this.getParentItem();
            const afterItemId = this.getAfterItemId();
            this.treeStore.addVariable(
                parent,
                afterItemId,
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
        getParentItem() {
            if (this.isGroup()) return item;
            if (this.stat.parent && this.stat.parent.data)
                return this.stat.parent.data;
            return this.currentChapter;
        },
        getAfterItemId() {
            if (this.isGroup()) return null;
            return item.itemId;
        },
        isGroup() {
            return this.item.itemType == 'Group';
        }
    }
};
</script>
