import { defineStore } from 'pinia';
import { mande } from 'mande';

const api = mande('/api/questionnaire/chapter/' /*, globalOptions*/);

export const useTreeStore = defineStore('tree', {
    state: () => ({
        info: {}
    }),
    getters: {
        getItems: state => state.info.chapter.items,
        getChapter: state => state.info.chapter
    },
    actions: {
        async fetchTree(questionnaireId, chapterId) {
            const info = await api.get(questionnaireId, {
                query: {
                    chapterId: chapterId
                }
            });
            this.setChapterInfo(info);
        },

        setChapterInfo(info) {
            this.info = info;
        }
    }
});
