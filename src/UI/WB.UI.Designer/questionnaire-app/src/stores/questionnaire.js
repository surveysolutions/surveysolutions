import { defineStore } from 'pinia';
import { mande } from 'mande';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';
import { commandCall } from '../services/commandService';
import emitter from '../services/emitter';
import { findIndex } from 'lodash';
import { useCookies } from 'vue3-cookies';

const api = mande('/api/questionnaire/get/' /*, globalOptions*/);

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {}
    }),
    getters: {
        getInfo: state => state.info
    },
    actions: {
        async fetchQuestionnaireInfo(questionnaireId) {
            const info = await api.get(questionnaireId);
            this.setQuestionnaireInfo(info);
        },

        setQuestionnaireInfo(info) {
            this.info = info;
        },

        addSection(callback) {
            const section = this.createEmptySection();

            var command = {
                questionnaireId: this.info.questionnaireId,
                groupId: section.itemId,
                title: section.title,
                condition: '',
                hideIfDisabled: false,
                isRoster: false,
                rosterSizeQuestionId: null,
                rosterSizeSource: 'Question',
                rosterFixedTitles: null,
                rosterTitleQuestionId: null,
                parentGroupId: null,
                variableName: null
            };

            return commandCall('AddGroup', command).then(result => {
                const index = this.info.chapters.length;
                this.info.chapters.splice(index, 0, section);
                callback(section);
                emitter.emit('chapterAdded');
            });
        },
        createEmptySection() {
            var newId = newGuid();
            var emptySection = {
                itemId: newId,
                title: i18n.t('QuestionnaireEditor.DefaultNewSection'),
                items: [],
                groupsCount: 0,
                hasCondition: false,
                hideIfDisabled: false,
                isCover: false,
                isReadOnly: false,
                itemType: 'Chapter',
                questionsCount: 0,
                rostersCount: 0
            };
            return emptySection;
        },

        deleteSection(chapterId) {
            var command = {
                questionnaireId: this.info.questionnaireId,
                groupId: chapterId
            };

            return commandCall('DeleteGroup', command).then(result => {
                const id = chapterId.replaceAll('-', '');

                var index = findIndex(this.info.chapters, function(i) {
                    return i.itemId === id;
                });

                this.info.chapters.splice(index, 1);

                emitter.emit('chapterDeleted', {
                    itemId: chapterId
                });
            });
        },

        pasteItemAfter(afterChapter) {
            const cookies = useCookies();

            var itemToCopy = cookies.cookies.get('itemToCopy');
            if (!itemToCopy) return;

            const newId = newGuid();

            var command = {
                sourceQuestionnaireId: itemToCopy.questionnaireId,
                sourceItemId: itemToCopy.itemId,
                entityId: newId,
                questionnaireId: this.info.questionnaireId,
                itemToPasteAfterId: afterChapter.itemId
            };

            return commandCall('PasteAfter', command).then(() =>
                this.fetchQuestionnaireInfo(this.info.questionnaireId)
            );
        }
    }
});
