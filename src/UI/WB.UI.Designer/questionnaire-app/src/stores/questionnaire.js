import { defineStore } from 'pinia';
import { get, commandCall } from '../services/apiService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';
import emitter from '../services/emitter';
import { findIndex, forEach, isEmpty, map, filter, find } from 'lodash';
import { useCookies } from 'vue3-cookies';
import { updateMetadata } from '../services/metadataService';

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {}
    }),
    getters: {
        getInfo: state => state.info,
        getEdittingMetadata: state => state.edittingMetadata
    },
    actions: {
        setupListeners() {
            emitter.on('macroAdded', this.macroAdded);
            emitter.on('macroDeleted', this.macroDeleted);
            emitter.on('macroUpdated', this.macroUpdated);

            emitter.on('categoriesUpdated', this.updateQuestionnaireCategories);
            emitter.on('categoriesDeleted', this.deleteQuestionnaireCategories);
        },
        macroAdded(payload) {
            this.prepareMacro(payload);
            this.info.macros.push(payload);
        },
        macroDeleted(payload) {
            const index = findIndex(this.info.macros, function(i) {
                return i.itemId === payload.itemId;
            });
            if (index !== -1) {
                this.info.macros.splice(index, 1);
            }
        },
        macroUpdated(payload) {
            const index = findIndex(this.info.macros, function(i) {
                return i.itemId === payload.itemId;
            });
            if (index !== -1) {
                this.prepareMacro(payload);
                Object.assign(this.info.macros[index], payload);
            }
        },

        async fetchQuestionnaireInfo(questionnaireId) {
            const info = await get('/api/questionnaire/get/' + questionnaireId);
            this.setQuestionnaireInfo(info);
        },

        prepareMacro(macro){
            macro.initialMacro = Object.assign({}, macro);
            macro.isDescriptionVisible = !isEmpty(macro.description);
        },

        setQuestionnaireInfo(info) {
            this.info = info;

            this.edittingMetadata = Object.assign({}, info.metadata);

            forEach(this.info.macros, macro => {
                this.prepareMacro(macro);
            });
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
        },

        updateQuestionnaireCategories(event) {
            if (this.info.categories === null) return;

            const id = event.categories.categoriesId;

            const item = find(
                this.info.categories,
                item => item.categoriesId == id
            );

            if (item) {
                item.name == event.categories.name;
            } else {
                this.info.categories.push(event.categories);
            }
        },

        deleteQuestionnaireCategories(event) {
            if (this.info.categories === null) return;

            this.info.categories = filter(
                this.info.categories,
                categoriesDto => {
                    return categoriesDto.categoriesId !== event.categoriesId;
                }
            );
        },

        async discardMetadataChanges() {
            this.edittingMetadata = Object.assign({}, this.info.metadata);
        },

        async saveMetadataChanges() {
            await updateMetadata(
                this.info.questionnaireId,
                this.edittingMetadata
            );
            this.info.title = this.edittingMetadata.title;
            this.info.metadata = Object.assign({}, this.edittingMetadata);
        }
    }
});
