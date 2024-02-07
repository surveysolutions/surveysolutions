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
    without,
    cloneDeep,
    isEqual
} from 'lodash';
import { useCookies } from 'vue3-cookies';

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {},
        edittingMetadata: {},
        edittingTranslations: [],
        edittingCategories: [],
        edittingScenarios: [],
        edittingLookupTables: [],
        edittingSharedInfo: {}
    }),
    getters: {
        getInfo: state => state.info,
        getEdittingMetadata: state => state.edittingMetadata,
        getIsDirtyMetadata: state =>
            !isEqual(state.edittingMetadata, state.info.metadata),
        getEdittingTranslations: state => state.edittingTranslations,
        getEdittingCategories: state => state.edittingCategories,
        getEdittingScenarios: state => state.edittingScenarios,
        getEdittingLookupTables: state => state.edittingLookupTables,

        getEdittingSharedInfo: state => state.edittingSharedInfo,
        getQuestionnaireEditDataDirty: state =>
            state.edittingSharedInfo.title != state.info.title ||
            state.edittingSharedInfo.variable != state.info.variable ||
            state.edittingSharedInfo.hideIfDisabled != state.info.hideIfDisabled
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
            emitter.on(
                'questionnaireSettingsUpdated',
                this.questionnaireSettingsUpdated
            );
            emitter.on('ownershipPassed', this.ownershipPassed);

            emitter.on('sharedPersonAdded', this.sharedPersonAdded);
            emitter.on('sharedPersonRemoved', this.sharedPersonRemoved);

            emitter.on('translationUpdated', this.translationUpdated);
            emitter.on('translationDeleted', this.translationDeleted);
            emitter.on(
                'settedDefaultTranslation',
                this.settedDefaultTranslation
            );

            emitter.on('metadataUpdated', this.metadataUpdated);

            emitter.on('groupUpdated', this.groupUpdated);
            emitter.on('rosterUpdated', this.rosterUpdated);

            emitter.on('scenarioUpdated', this.scenarioUpdated);
            emitter.on('scenarioDeleted', this.scenarioDeleted);

            emitter.on('lookupTableAdded', this.lookupTableAdded);
            emitter.on('lookupTableUpdated', this.lookupTableUpdated);
            emitter.on('lookupTableDeleted', this.lookupTableDeleted);

            emitter.on('attachmentDeleted', this.attachmentDeleted);
            emitter.on('attachmentUpdated', this.attachmentUpdated);
        },

        async fetchQuestionnaireInfo(questionnaireId) {
            const info = await get('/api/questionnaire/get/' + questionnaireId);
            this.setQuestionnaireInfo(info);
        },

        setQuestionnaireInfo(info) {
            this.info = info;

            this.edittingMetadata = cloneDeep(info.metadata);
            this.edittingTranslations = cloneDeep(info.translations);
            this.edittingCategories = cloneDeep(info.categories);
            this.edittingScenarios = cloneDeep(info.scenarios);
            this.edittingLookupTables = cloneDeep(info.lookupTables);
            this.edittingSharedInfo = this.getQuestionnaireEditData();

            forEach(this.info.attachments, attachment => {
                var editAttachment = cloneDeep(attachment);
                attachment.editAttachment = editAttachment;
            });

            forEach(this.info.macros, macro => {
                this.prepareMacro(macro);
            });
        },

        getQuestionnaireEditData() {
            return {
                title: this.info.title,
                variable: this.info.variable,
                hideIfDisabled: this.info.hideIfDisabled,

                isAnonymouslyShared: this.info.isAnonymouslyShared,
                anonymousQuestionnaireId: this.info.anonymousQuestionnaireId,
                anonymouslySharedAtUtc: this.info.anonymouslySharedAtUtc
            };
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
        questionnaireSettingsUpdated(payload) {
            this.info.title = payload.title;
            this.info.variable = payload.variable;
            this.info.hideIfDisabled = payload.hideIfDisabled;
            this.info.isPublic = payload.isPublic;
            this.info.defaultLanguageName = payload.defaultLanguageName;

            this.edittingSharedInfo = this.getQuestionnaireEditData();

            this.info.metadata.title = payload.title;
            this.edittingMetadata.title = payload.title;
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
                return i.itemId === payload.group.id.replaceAll('-', '');
            });

            if (index > -1) {
                this.info.chapters[index].title = payload.group.title;
                this.info.chapters[index].hasCondition =
                    payload.group.enablementCondition !== null &&
                    /\S/.test(payload.group.enablementCondition);
                this.info.chapters[index].hideIfDisabled =
                    payload.group.hideIfDisabled;
            }
        },
        rosterUpdated(payload) {},
        attachmentDeleted(payload) {
            const index = findIndex(this.info.attachments, function(i) {
                return i.attachmentId === payload.id;
            });
            if (index !== -1) {
                this.info.attachments.splice(index, 1);
            }
        },
        attachmentUpdated(payload) {
            const newAttachment = cloneDeep(payload.attachment);
            newAttachment.file = null;
            newAttachment.editAttachment = cloneDeep(newAttachment);

            if (payload.attachment.oldAttachmentId) {
                const indexInit = findIndex(this.info.attachments, function(i) {
                    return (
                        i.attachmentId === payload.attachment.oldAttachmentId
                    );
                });
                if (indexInit !== -1) {
                    this.info.attachments[indexInit] = newAttachment;
                }
            } else {
                this.info.attachments.push(newAttachment);
            }
        },

        prepareMacro(macro) {
            macro.editMacro = cloneDeep(macro);
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

            const categories = event.categories;
            const id = categories.categoriesId;

            const item = find(
                this.info.categories,
                item => item.categoriesId == id
            );

            if (item) {
                item.name = categories.name;
            } else {
                this.info.categories.push(categories);

                const eCategories = cloneDeep(categories);
                this.edittingCategories.push(eCategories);
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
            this.edittingCategories = filter(
                this.edittingCategories,
                categoriesDto => {
                    return categoriesDto.categoriesId !== event.categoriesId;
                }
            );
        },

        async discardCategoriesChanges() {
            this.edittingCategories = cloneDeep(this.info.categories);
        },

        async discardMetadataChanges() {
            this.edittingMetadata = cloneDeep(this.info.metadata);
        },

        metadataUpdated(event) {
            this.info.metadata = cloneDeep(event.metadata);
            this.edittingMetadata = cloneDeep(event.metadata);
            this.info.title = event.metadata.title;
        },

        async discardTranslationChanges() {
            this.edittingTranslations = cloneDeep(this.info.translations);
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
                item.name = translation.name;
                item.translationId = translation.translationId;
                item.oldTranslationId = translation.oldTranslationId;
            } else {
                this.info.translations.push(translation);

                const eTranslation = cloneDeep(translation);
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
        },

        async scenarioUpdated(event) {
            const scenario = event.scenario;

            const item = find(
                this.info.scenarios,
                item => item.id == scenario.id
            );

            if (item) {
                item.title = scenario.title;
            }
        },

        async scenarioDeleted(event) {
            const scenarioId = event.scenarioId;

            this.info.scenarios = filter(this.info.scenarios, scenario => {
                return scenario.id !== scenarioId;
            });
            this.edittingScenarios = filter(
                this.edittingScenarios,
                scenario => {
                    return scenario.id !== scenarioId;
                }
            );
        },

        async lookupTableAdded(event) {
            const lookupTable = event.lookupTable;

            const newLookupTable = cloneDeep(lookupTable);
            this.info.lookupTables.push(newLookupTable);

            const eLookupTable = cloneDeep(lookupTable);
            this.edittingLookupTables.push(eLookupTable);
        },

        async lookupTableUpdated(event) {
            const lookupTable = event.lookupTable;

            const item = find(
                this.info.lookupTables,
                item => item.itemId == lookupTable.oldItemId
            );

            if (item) {
                item.name = lookupTable.name;
                item.fileName = lookupTable.fileName;
            }

            const eItem = find(
                this.info.edittingLookupTables,
                item => item.itemId == lookupTable.oldItemId
            );

            if (eItem) {
                eItem.name = lookupTable.name;
                eItem.fileName = lookupTable.fileName;
            }
        },

        async lookupTableDeleted(event) {
            const lookupTableId = event.lookupTableId;

            this.info.lookupTables = filter(
                this.info.lookupTables,
                lookupTable => {
                    return lookupTable.itemId !== lookupTableId;
                }
            );
            this.edittingLookupTables = filter(
                this.edittingLookupTables,
                lookupTable => {
                    return lookupTable.itemId !== lookupTableId;
                }
            );
        }
    }
});
