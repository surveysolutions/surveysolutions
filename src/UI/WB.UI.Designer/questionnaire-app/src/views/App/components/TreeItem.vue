<template>
    <div context-menu class="section item" :data-bs-target="'context-menu-' + item.itemId" :class="itemClass"
        ui-sref-active="selected" context-menu-hide-on-mouse-leave="true" @contextmenu.prevent="onContextMenu($event)"
        v-click-outside="closeContextMenu" v-contextmenu-outside="closeContextMenu" @mouseenter="is_highlighted = true"
        @mouseleave="is_highlighted = false">
        <span class="cursor"></span>
        <a class="handler angular-ui-tree-handle" ui-tree-handle></a>

        <slot />

        <Teleport to="body">
            <div v-show="contextmenu.open" :style="{
                top: contextmenu.y + 'px',
                left: contextmenu.x + 'px'
            }" @click="closeContextMenu" class="dropdown position-fixed" :class="{ open: contextmenu.open }"
                :id="'context-menu-' + item.itemId">
                <ul class="dropdown-menu" role="menu">
                    <li>
                        <a @click="addQuestion()" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            ">{{
        isGroup()
        ? $t('QuestionnaireEditor.TreeAddQuestion')
        : $t(
            'QuestionnaireEditor.TreeAddQuestionAfter'
        )
    }}</a>
                    </li>
                    <li>
                        <a @click="addGroup()" v-if="!questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                                ">{{
            isGroup()
            ? $t('QuestionnaireEditor.TreeAddSection')
            : $t(
                'QuestionnaireEditor.TreeAddSectionAfter'
            )
        }}</a>
                    </li>
                    <li>
                        <a @click="addRoster()" v-if="!questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly &&
                                !currentChapter.isCover
                                ">{{
            isGroup()
            ? $t('QuestionnaireEditor.TreeAddRoster')
            : $t(
                'QuestionnaireEditor.TreeAddRosterAfter'
            )
        }}</a>
                    </li>
                    <li>
                        <a @click="addStaticText()" v-if="!questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                                ">{{
            isGroup()
            ? $t(
                'QuestionnaireEditor.TreeAddStaticText'
            )
            : $t(
                'QuestionnaireEditor.TreeAddStaticTextAfter'
            )
        }}</a>
                    </li>
                    <li>
                        <a @click="addVariable()" v-if="!questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                                ">{{
            isGroup()
            ? $t('QuestionnaireEditor.TreeAddVariable')
            : $t(
                'QuestionnaireEditor.TreeAddVariableAfter'
            )
        }}</a>
                    </li>
                    <li>
                        <a @click="copyItem()">{{
                            $t('QuestionnaireEditor.Copy')
                        }}</a>
                    </li>
                    <li>
                        <a @click="pasteItemAfter()" v-if="readyToPaste &&
                                !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                                ">{{ $t('QuestionnaireEditor.PasteAfter') }}</a>
                    </li>
                    <li>
                        <a @click="deleteItem()" v-if="!questionnaire.isReadOnlyForUser &&
                            !currentChapter.isReadOnly
                            ">{{ $t('QuestionnaireEditor.Delete') }}</a>
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
import { useGroupStore } from '../../../stores/group';
import { useQuestionStore } from '../../../stores/question';
import { useRosterStore } from '../../../stores/roster';
import { useStaticTextStore } from '../../../stores/staticText';
import { useVariableStore } from '../../../stores/variable';
import clickOutside from '../../../directives/clickOutside';
import contextmenuOutside from '../../../directives/contextmenuOutside';
import { createQuestionForDeleteConfirmationPopup } from '../../../services/utilityService';

export default {
    name: 'TreeItem',
    directives: {
        clickOutside,
        contextmenuOutside
    },
    props: {
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        tree: { type: Object, required: true },
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
        const groupStore = useGroupStore();
        const questionStore = useQuestionStore();
        const rosterStore = useRosterStore();
        const staticTextStore = useStaticTextStore();
        const variableStore = useVariableStore();

        return {
            treeStore,
            questionnaireStore,
            groupStore,
            questionStore,
            rosterStore,
            staticTextStore,
            variableStore,
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
        },
        readyToPaste() {
            return this.treeStore.canPaste();
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
                    this.tree.add(question, this.getParentStat(), index);
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
                    this.tree.add(group, this.getParentStat(), index);

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
                    this.tree.add(roster, this.getParentStat(), index);

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
                    this.tree.add(statictext, this.getParentStat(), index);

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
                    this.tree.add(variable, this.getParentStat(), index);

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
            if (this.isGroup()) return this.item;
            if (this.stat.parent && this.stat.parent.data)
                return this.stat.parent.data;
            return this.currentChapter;
        },
        getParentStat() {
            if (this.isGroup()) return this.stat;
            if (this.stat.parent) return this.stat.parent;
            return null;
        },
        getAfterItemId() {
            if (this.isGroup()) return null;
            return this.item.itemId;
        },
        isGroup() {
            return this.item.itemType == 'Group';
        },

        deleteItem() {
            if (this.item.itemType == 'Question')
                this.deleteQuestion(this.item.itemId);
            else if (this.item.itemType == 'StaticText')
                this.deleteStaticText(this.item.itemId);
            else if (this.item.itemType == 'Variable')
                this.deleteVariable(this.item.itemId);
            else if (this.isGroup) this.deleteGroup(this.item.itemId);
        },

        deleteQuestion() {
            var itemIdToDelete = this.item.itemId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.title ||
                this.$t('QuestionnaireEditor.UntitledQuestion')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.questionStore
                        .deleteQuestion(itemIdToDelete)
                        .then(response => {
                            this.tree.remove(this.stat);

                            //questionnaireService.removeItemWithId(
                            //    $scope.items,
                            //    itemIdToDelete
                            //);
                            //$scope.resetSelection();
                            //$rootScope.$emit('questionDeleted', itemIdToDelete);
                            //removeSelectionIfHighlighted(itemIdToDelete);
                        });
                }
            };

            this.$confirm(params);
        },

        deleteStaticText() {
            var itemIdToDelete = this.item.itemId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.text ||
                this.$t('QuestionnaireEditor.UntitledStaticText')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.staticTextStore
                        .deleteStaticText(itemIdToDelete)
                        .then(response => {
                            this.tree.remove(this.stat);

                            //questionnaireService.removeItemWithId(
                            //    $scope.items,
                            //    itemIdToDelete
                            //);
                            //$scope.resetSelection();
                            //$rootScope.$emit('questionDeleted', itemIdToDelete);
                            //removeSelectionIfHighlighted(itemIdToDelete);
                        });
                }
            };

            this.$confirm(params);
        },

        deleteVariable() {
            var itemIdToDelete = this.item.itemId;

            var label = this.item.variableData
                ? this.item.variableData.label
                : this.item.label;

            const params = createQuestionForDeleteConfirmationPopup(
                label || this.$t('QuestionnaireEditor.UntitledVariable')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.variableStore
                        .deleteVariable(itemIdToDelete)
                        .then(response => {
                            this.tree.remove(this.stat);

                            //questionnaireService.removeItemWithId(
                            //    $scope.items,
                            //    itemIdToDelete
                            //);
                            //$scope.resetSelection();
                            //$rootScope.$emit('questionDeleted', itemIdToDelete);
                            //removeSelectionIfHighlighted(itemIdToDelete);
                        });
                }
            };

            this.$confirm(params);
        },

        deleteGroup() {
            var itemIdToDelete = this.item.itemId;

            const params = createQuestionForDeleteConfirmationPopup(
                this.item.title ||
                this.$t('QuestionnaireEditor.UntitledGroupOrRoster')
            );

            params.callback = confirm => {
                if (confirm) {
                    this.groupStore
                        .deleteGroup(itemIdToDelete)
                        .then(response => {
                            this.tree.remove(this.stat);

                            //questionnaireService.removeItemWithId(
                            //    $scope.items,
                            //    itemIdToDelete
                            //);
                            //$scope.resetSelection();
                            //$rootScope.$emit('questionDeleted', itemIdToDelete);
                            //removeSelectionIfHighlighted(itemIdToDelete);
                        });
                }
            };

            this.$confirm(params);
        },

        copyItem() {
            this.treeStore.copyItem(this.item);
        },

        pasteItemAfter() {
            if (!this.treeStore.canPaste()) return;

            this.treeStore.pasteItemAfter(this.item).then(function (result) {
                if (!chapter.isCover)
                    this.$router.push({
                        name: result.itemType,
                        params: {
                            itemId: result.itemId
                        }
                    });
            });
        }
    }
};
</script>
