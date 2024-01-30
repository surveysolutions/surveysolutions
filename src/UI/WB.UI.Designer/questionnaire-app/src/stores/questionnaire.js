import { defineStore } from 'pinia';
import { get, commandCall } from '../services/apiService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';
import emitter from '../services/emitter';
import {
    findIndex,
    forEach,
    isEmpty,
    map,
    filter,
    find,
    sortBy,
    without
} from 'lodash';
import { useCookies } from 'vue3-cookies';
import { updateMetadata } from '../services/metadataService';

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {},
        edittingMetadata: {},
        edittingTranslations: []
    }),
    getters: {
        getInfo: state => state.info,
        getEdittingMetadata: state => state.edittingMetadata,
        getEdittingTranslations: state => state.edittingTranslations
    },
    actions: {
        setupListeners() {
            emitter.on('macroAdded', this.macroAdded);
            emitter.on('macroDeleted', this.macroDeleted);
            emitter.on('macroUpdated', this.macroUpdated);

            emitter.on('categoriesUpdated', this.updateQuestionnaireCategories);
            emitter.on('categoriesDeleted', this.deleteQuestionnaireCategories);

            emitter.on(
                'anonymousQuestionnaireSettingsUpdated',
                this.anonymousQuestionnaireSettingsUpdated
            );
            emitter.on('questionnaireUpdated', this.questionnaireUpdated);
            emitter.on('ownershipPassed', this.ownershipPassed);

            emitter.on('sharedPersonAdded', this.sharedPersonAdded);
            emitter.on('sharedPersonRemoved', this.sharedPersonRemoved);

            emitter.on('translationUpdated', this.translationUpdated);
            emitter.on('translationDeleted', this.translationDeleted);
            emitter.on(
                'settedDefaultTranslation',
                this.settedDefaultTranslation
            );

            emitter.on('groupUpdated', this.groupUpdated);
        },

        async fetchQuestionnaireInfo(questionnaireId) {
            const info = await get('/api/questionnaire/get/' + questionnaireId);
            this.setQuestionnaireInfo(info);
        },

        setQuestionnaireInfo(info) {
            this.info = info;

            this.edittingMetadata = Object.assign({}, info.metadata);
            this.edittingTranslations = Object.assign([], info.translations);

            forEach(this.info.macros, macro => {
                this.prepareMacro(macro);
            });
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
        anonymousQuestionnaireSettingsUpdated(payload) {
            this.info.isAnonymouslyShared = payload.isAnonymouslyShared;
            this.info.anonymousQuestionnaireId =
                payload.anonymousQuestionnaireId;
            this.info.anonymousQuestionnaireShareDate =
                payload.anonymousQuestionnaireShareDate;
        },
        questionnaireUpdated(payload) {
            this.info.title = payload.title;
            this.info.variable = payload.variable;
            this.info.hideIfDisabled = payload.hideIfDisabled;
            this.info.isPublic = payload.isPublic;
            this.info.defaultLanguageName = payload.defaultLanguageName;
        },
        ownershipPassed(payload) {
            forEach(this.info.sharedPersons, person => {
                if (person.email == payload.ownerEmail) {
                    person.isOwner = false;
                }

                if (person.email == payload.newOwnerEmail) {
                    person.isOwner = true;
                }
            });
        },
        sharedPersonRemoved(payload) {
            this.info.sharedPersons = filter(
                this.info.sharedPersons,
                person => {
                    return person.email !== payload.email;
                }
            );
        },
        sharedPersonAdded(paylaod) {
            if (
                filter(this.info.sharedPersons, {
                    email: paylaod.email
                }).length === 0
            ) {
                this.info.sharedPersons.push({
                    email: paylaod.email,
                    login: paylaod.name,
                    userId: paylaod.id,
                    shareType: paylaod.shareType
                });

                var owner = filter(this.info.sharedPersons, {
                    isOwner: true
                });
                var sharedPersons = sortBy(
                    without(this.info.sharedPersons, owner),
                    ['email']
                );

                this.info.sharedPersons.sharedPersons = [owner].concat(
                    sharedPersons
                );
            }
        },
        groupUpdated(payload) {
            var index = findIndex(this.info.chapters, function(i) {
                return i.itemId === payload.id.replaceAll('-', '');
            });

            if (index > -1) this.info.chapters[index].title = payload.title;
        },

        prepareMacro(macro) {
            macro.initialMacro = Object.assign({}, macro);
            macro.isDescriptionVisible = !isEmpty(macro.description);
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
        },

        async discardTranslationChanges() {
            this.edittingTranslations = Object.assign(
                {},
                this.info.translations
            );
        },

        async translationUpdated(event) {
            const translation = event.translation;

            const item = find(
                this.info.translations,
                item =>
                    item.translationId == translation.translationId ||
                    item.translationId == translation.oldTranslationId
            );

            if (item) {
                item.name == translation.name;
                item.translationId == translation.translationId;
                item.oldTranslationId == translation.oldTranslationId;
            } else {
                this.info.translations.push(translation);

                const eTranslation = Object.assign({}, translation);
                this.edittingTranslations.push(eTranslation);
            }
        },

        async translationDeleted(event) {
            const translationId = event.translationId;

            this.info.translations = filter(
                this.info.translations,
                translation => {
                    return translation.translationId !== translationId;
                }
            );
            this.edittingTranslations = filter(
                this.edittingTranslations,
                translation => {
                    return translation.translationId !== translationId;
                }
            );
        },

        async settedDefaultTranslation(event) {
            const translationId = event.translationId;

            const translationById = find(
                this.info.translations,
                item => item.translationId == translationId
            );
            forEach(this.info.translations, translation => {
                translation.isDefault = false;
            });

            if (translationById) translationById.isDefault = true;

            const eTranslationById = find(
                this.edittingTranslations,
                item => item.translationId == translationId
            );
            forEach(this.edittingTranslations, translation => {
                translation.isDefault = false;
            });

            if (eTranslationById) eTranslationById.isDefault = true;
        }
    }
});
