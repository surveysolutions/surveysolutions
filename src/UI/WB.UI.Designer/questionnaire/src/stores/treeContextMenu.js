import { defineStore } from 'pinia';

export const useTreeContextMenuStore = defineStore('treeContextMenu', {
    state: () => ({
        visible: false,
        x: 0,
        y: 0,
        item: null,
        stat: null,
        questionnaire: null,
        currentChapter: null,
        questionnaireId: null,
    }),
    actions: {
        show(item, stat, questionnaire, currentChapter, questionnaireId, x, y) {
            this.item = item;
            this.stat = stat;
            this.questionnaire = questionnaire;
            this.currentChapter = currentChapter;
            this.questionnaireId = questionnaireId;
            this.x = x;
            this.y = y;
            this.visible = true;
        },
        hide() {
            this.visible = false;
        },
    },
});
