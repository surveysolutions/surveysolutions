<template>
    <div class="btn-group dropup">
        <button
            type="button"
            class="btn btn-link dropdown-toggle"
            data-toggle="dropdown"
        >
            <span>{{ $t('QuestionnaireEditor.MoveTo') }}</span>
            <span class="caret"></span>
        </button>

        <div role="menu" class="chapter-menu dropdown-menu dropdown-menu-right">
            <h1>{{ $t('QuestionnaireEditor.MoveToAnotherSubSection') }}</h1>
            <ul>
                <li v-for="chapter in questionnaire.chapters">
                    <a
                        @click="
                            chapter.itemId == currentChapter.itemId ||
                                moveToChapter(chapter.itemId)
                        "
                        v-show="!chapter.isReadOnly"
                        :disabled="chapter.itemId == currentChapterId"
                        unsaved-warning-clear
                    >
                        {{ chapter.title }}
                    </a>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
export default {
    name: 'MoveToChapterSnippet',
    inject: ['questionnaire', 'currentChapter'],
    props: {
        itemId: { type: String, required: true }
        //itemType: { type: String, required: true }
    },
    data() {
        return {
            activeVariable: null,
            dirty: false,
            valid: true
        };
    },
    methods: {
        moveToChapter(chapterId) {
            //
        }
    }
};
</script>
