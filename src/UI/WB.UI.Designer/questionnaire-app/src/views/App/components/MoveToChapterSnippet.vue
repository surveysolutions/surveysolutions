<template>
    <div class="btn-group dropup">
        <button type="button" class="btn btn-link dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
            <span>{{ $t('QuestionnaireEditor.MoveTo') }}</span>
            <span class="caret"></span>
        </button>

        <div role="menu" class="chapter-menu dropdown-menu dropdown-menu-right">
            <h1>{{ $t('QuestionnaireEditor.MoveToAnotherSubSection') }}</h1>
            <ul>
                <li v-for="chapter in questionnaire.chapters">
                    <a @click="
                        chapter.itemId == currentChapter.itemId ||
                        moveToChapter(chapter.itemId)
                        " v-show="!chapter.isReadOnly" :disabled="chapter.itemId == currentChapterId ? true : null"
                        unsaved-warning-clear>
                        {{ chapter.title }}
                    </a>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>

import { useTreeStore } from '../../../stores/tree';

export default {
    name: 'MoveToChapterSnippet',
    inject: ['questionnaire', 'currentChapter'],
    props: {
        itemId: { type: String, required: true },
        itemType: { type: String, required: true }
    },
    data() {
        return {
            activeVariable: null,
            dirty: false,
            valid: true
        };
    },
    setup() {
        const treeStore = useTreeStore();

        return {
            treeStore,
        };
    },
    computed: {
        currentChapterId() {
            return this.currentChapter.chapter.itemId;
        }
    },
    methods: {
        moveToChapter(chapterId) {
            if (chapterId == this.currentChapterId)
                return;

            const itemToMoveId = this.itemId;

            this.treeStore.moveItem(itemToMoveId, this.itemType, chapterId, 0);

            //questionnaireService.removeItemWithId($scope.items, itemToMoveId);
            //$scope.resetSelection();
        }
    }
};
</script>
