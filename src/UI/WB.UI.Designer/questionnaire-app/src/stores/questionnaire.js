import { defineStore } from 'pinia';
import { commandCall } from '../services/apiService';
import { newGuid } from '../helpers/guid';
import { i18n } from '../plugins/localization';
import emitter from '../services/emitter';
import { getQuestionnaire } from '../services/questionnaireService';
import {
    findIndex,
    forEach,
    isEmpty,
    filter,
    find,
    sortBy,
    without,
    cloneDeep,
    isEqual
} from 'lodash';

import { addSectionGroup } from '../services/groupService';

export const useQuestionnaireStore = defineStore('questionnaire', {
    state: () => ({
        info: {},
        edittingMetadata: {},
        edittingScenarios: [],
        edittingSharedInfo: {}
    }),
    getters: {
        getInfo: state => state.info,
        getEdittingMetadata: state => state.edittingMetadata,
        getIsDirtyMetadata: state =>
            !isEqual(state.edittingMetadata, state.info.metadata),
        getEdittingScenarios: state => state.edittingScenarios,
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

            emitter.on('categoriesUpdated', this.categoriesUpdated);
            emitter.on('categoriesDeleted', this.categoriesDeleted);

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
            emitter.on('defaultTranslationSet', this.defaultTranslationSet);

            emitter.on('metadataUpdated', this.metadataUpdated);

            emitter.on('questionAdded', this.questionAdded);
            emitter.on('questionDeleted', this.questionDeleted);

            emitter.on('chapterAdded', this.chapterAdded);

            emitter.on('groupAdded', this.groupAdded);
            emitter.on('groupDeleted', this.groupDeleted);
            emitter.on('groupUpdated', this.groupUpdated);

            emitter.on('rosterAdded', this.rosterAdded);
            emitter.on('rosterDeleted', this.rosterDeleted);
            emitter.on('rosterUpdated', this.rosterUpdated);

            emitter.on('scenarioUpdated', this.scenarioUpdated);
            emitter.on('scenarioDeleted', this.scenarioDeleted);

            emitter.on('lookupTableUpdated', this.lookupTableUpdated);
            emitter.on('lookupTableDeleted', this.lookupTableDeleted);

            emitter.on('attachmentDeleted', this.attachmentDeleted);
            emitter.on('attachmentUpdated', this.attachmentUpdated);

            emitter.on('itemPasted', this.itemPasted);
        },

        async fetchQuestionnaireInfo(questionnaireId) {
            var info = await getQuestionnaire(questionnaireId);
            this.setQuestionnaireInfo(info);
        },

        resetSharedInfo() {
            this.edittingSharedInfo = this.getQuestionnaireEditData();
        },

        setQuestionnaireInfo(info) {
            if (info === null || info === undefined) {
                this.$reset();
                return;
            }

            this.info = info;

            this.edittingMetadata = cloneDeep(info.metadata);
            this.edittingScenarios = cloneDeep(info.scenarios);
            this.edittingSharedInfo = this.getQuestionnaireEditData();

            forEach(this.info.categories, categoriesItem => {
                var editCategories = cloneDeep(categoriesItem);
                categoriesItem.editCategories = editCategories;
            });

            forEach(this.info.lookupTables, lookupTable => {
                var editLookupTable = cloneDeep(lookupTable);
                lookupTable.editLookupTable = editLookupTable;
            });

            forEach(this.info.translations, translation => {
                var editTranslation = cloneDeep(translation);
                translation.editTranslation = editTranslation;
            });

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
            this.info.anonymouslySharedAtUtc = payload.anonymouslySharedAtUtc;
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

        chapterAdded(payload) {
            this.info.groupsCount++;
        },
        groupAdded(payload) {
            this.info.groupsCount++;
        },
        groupDeleted(event) {
            this.fetchQuestionnaireInfo(this.info.questionnaireId);
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

        rosterAdded(payload) {
            this.info.rostersCount++;
        },
        rosterDeleted(payload) {
            this.fetchQuestionnaireInfo(this.info.questionnaireId);
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

            const indexInit = findIndex(this.info.attachments, function(i) {
                return i.attachmentId === payload.attachment.attachmentId;
            });
            if (indexInit !== -1) {
                if (payload.newId && payload.newId !== null) {
                    newAttachment.attachmentId = payload.newId;
                    newAttachment.editAttachment.attachmentId = payload.newId;
                }

                this.info.attachments[indexInit] = newAttachment;
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

            return addSectionGroup(this.info.questionnaireId, section).then(
                result => {
                    const index = this.info.chapters.length;
                    this.info.chapters.splice(index, 0, section);
                    callback(section);
                }
            );
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

        categoriesUpdated(payload) {
            const newCategories = cloneDeep(payload.categories);
            newCategories.file = null;
            newCategories.editCategories = cloneDeep(newCategories);

            const indexInit = findIndex(this.info.categories, function(i) {
                return i.categoriesId === payload.categories.categoriesId;
            });
            if (indexInit !== -1) {
                if (payload.newId && payload.newId !== null) {
                    newCategories.categoriesId = payload.newId;
                    newCategories.editCategories.categoriesId = payload.newId;
                }
                this.info.categories[indexInit] = newCategories;
            } else {
                this.info.categories.push(newCategories);
            }
        },

        categoriesDeleted(payload) {
            const index = findIndex(this.info.categories, function(i) {
                return i.categoriesId === payload.id;
            });
            if (index !== -1) {
                this.info.categories.splice(index, 1);
            }
        },

        questionAdded(payload) {
            this.info.questionsCount++;
        },
        questionDeleted(payload) {
            this.info.questionsCount--;
        },

        async discardMetadataChanges() {
            this.edittingMetadata = cloneDeep(this.info.metadata);
        },

        metadataUpdated(event) {
            this.info.metadata = cloneDeep(event.metadata);
            this.edittingMetadata = cloneDeep(event.metadata);
            this.info.title = event.metadata.title;
        },

        translationDeleted(payload) {
            const index = findIndex(this.info.translations, function(i) {
                return i.translationId === payload.id;
            });
            if (index !== -1) {
                this.info.translations.splice(index, 1);
            }
        },
        translationUpdated(payload) {
            const index = findIndex(this.info.translations, function(i) {
                return i.translationId === payload.translation.translationId;
            });
            if (index !== -1) {
                const tr = this.info.translations[index];
                if (payload.newId && payload.newId !== null) {
                    tr.translationId = tr.editTranslation.translationId =
                        payload.newId;
                }
                tr.name = tr.editTranslation.name = payload.translation.name;
                tr.isDefault = tr.editTranslation.isDefault =
                    payload.translation.isDefault;
            } else {
                const newTranslation = {
                    translationId: payload.translation.translationId,
                    isDefault: false,
                    name: payload.translation.name
                };
                newTranslation.editTranslation = cloneDeep(newTranslation);
                this.info.translations.push(newTranslation);
            }
        },

        defaultTranslationSet(event) {
            const translationId = event.translationId;

            forEach(this.info.translations, translation => {
                if (translation.translationId == translationId) {
                    translation.isDefault = true;
                    translation.editTranslation.isDefault = true;
                } else {
                    translation.isDefault = false;
                    translation.editTranslation.isDefault = false;
                }
            });
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

        lookupTableUpdated(payload) {
            const newLookupTable = cloneDeep(payload.lookupTable);
            newLookupTable.file = null;
            newLookupTable.editLookupTable = cloneDeep(newLookupTable);

            const indexInit = findIndex(this.info.lookupTables, function(i) {
                return i.itemId === payload.lookupTable.itemId;
            });
            if (indexInit !== -1) {
                if (payload.newId && payload.newId !== null) {
                    newLookupTable.itemId = payload.newId;
                    newLookupTable.editLookupTable.itemId = payload.newId;
                }

                this.info.lookupTables[indexInit] = newLookupTable;
            } else {
                this.info.lookupTables.push(newLookupTable);
            }
        },

        async lookupTableDeleted(payload) {
            const indexInit = findIndex(this.info.lookupTables, function(i) {
                return i.itemId === payload.id;
            });
            if (indexInit !== -1) {
                this.info.lookupTables.splice(indexInit, 1);
            }
        },

        itemPasted(event) {
            //if (event.parentId == null) {
            this.fetchQuestionnaireInfo(this.info.questionnaireId);
            //}
        }
    }
});
