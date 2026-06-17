<template>
    <div class="section item" :class="itemClass" @mouseenter="is_highlighted = true"
        @mouseleave="is_highlighted = false" @contextmenu.prevent="showContextMenu">
        <span class="cursor"></span>
        <a class="handler angular-ui-tree-handle" ui-tree-handle></a>

        <slot />
    </div>
</template>

<script>
import { useQuestionnaireStore } from '../../../stores/questionnaire';
import { useTreeStore } from '../../../stores/tree';
import { useTreeContextMenuStore } from '../../../stores/treeContextMenu';

export default {
    name: 'TreeItem',
    props: {
        questionnaireId: { type: String, required: true },
        item: { type: Object, required: true },
        stat: { type: Object, required: true },
        selectedItemId: { type: String, required: false }
    },
    data() {
        return {
            is_highlighted: false,
        };
    },
    setup() {
        const treeStore = useTreeStore();
        const questionnaireStore = useQuestionnaireStore();
        const treeContextMenuStore = useTreeContextMenuStore();

        return {
            treeStore,
            questionnaireStore,
            treeContextMenuStore,
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
    },
    methods: {
        showContextMenu(event) {
            const menuWidth = 200;
            const menuHeight = 250;
            const x = Math.max(0, Math.min(event.clientX, window.innerWidth - menuWidth));
            const y = Math.max(0, Math.min(event.clientY, window.innerHeight - menuHeight));
            this.treeContextMenuStore.show(
                this.item,
                this.stat,
                this.questionnaire,
                this.currentChapter,
                this.questionnaireId,
                x,
                y
            );
        },
    },
};
</script>
