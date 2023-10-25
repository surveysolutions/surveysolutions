<template>
    <div
        context-menu
        class="section item"
        :data-bs-target="'context-menu-' + item.itemId"
        :class="itemClass"
        ui-sref-active="selected"
        context-menu-hide-on-mouse-leave="true"
    >
        <span class="cursor"></span>
        <a class="handler" ui-tree-handle></a>

        <slot />

        <div
            class="dropdown position-fixed"
            :id="'context-menu-' + item.itemId"
        >
            <ul class="dropdown-menu" role="menu">
                <li>
                    <a
                        @click="addQuestion(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        >{{ $t('QuestionnaireEditor.TreeAddQuestion') }}</a
                    >
                </li>
                <li>
                    <a
                        @click="addGroup(item)"
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
                        @click="addRoster(item)"
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
                        @click="addStaticText(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        >{{ $t('QuestionnaireEditor.TreeAddStaticText') }}</a
                    >
                </li>
                <li>
                    <a
                        @click="addVariable(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        >{{ $t('QuestionnaireEditor.TreeAddVariable') }}</a
                    >
                </li>
                <li>
                    <a @click="copyRef(item)">{{
                        $t('QuestionnaireEditor.Copy')
                    }}</a>
                </li>
                <li>
                    <a
                        :disabled="!readyToPaste"
                        @click="pasteItemAfter(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        >{{ $t('QuestionnaireEditor.PasteAfter') }}</a
                    >
                </li>
                <li>
                    <a
                        @click="deleteGroup(item)"
                        v-if="
                            !questionnaire.isReadOnlyForUser &&
                                !currentChapter.isReadOnly
                        "
                        >{{ $t('QuestionnaireEditor.Delete') }}</a
                    >
                </li>
            </ul>
        </div>
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

export default {
    name: 'TreeItem',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        selectedItemId: { type: String, required: false }
    },
    data() {
        return {
            is_highlighted: false
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
        }
    }
};
</script>
