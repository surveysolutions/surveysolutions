<template>
    <Teleport to="body">
        <div class="modal fade add-classification-modal dragAndDrop ng-scope ng-isolate-scope in"
            id="mAddCalassification" :style="dialogStyle" v-if="open == true" uib-modal-window="modal-window"
            role="dialog" index="0" animate="animate"
            ng-style="{'z-index': 1050 + $$topModalIndex*10, display: 'block'}" tabindex="-1"
            uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true" v-dragAndDrop>
            <div class=" modal-dialog">
                <div class="modal-content">
                    <div class="modal-header blue-strip handle">
                        <button type="button" class="close" aria-hidden="true" @click="open = false"></button>
                        <h3 class="modal-title">
                            {{ $t('QuestionnaireEditor.ClassificationsTitle') }}
                        </h3>
                    </div>
                    <div class="modal-body">
                        <div class="header-block clearfix">
                            <form>
                                <div class="input-group custom-dropdown-search">
                                    <div class="input-group-btn custom-dropdown">
                                        <button class="btn dropdown-toggle" id="classification-group" data-toggle="dropdown"
                                            uib-dropdown-toggle type="button" data-bs-toggle="dropdown"
                                            aria-expanded="false">
                                            {{ selectedGroup.title }}
                                            <span class="dropdown-arrow"></span>
                                        </button>
                                        <div class="dropdown-menu" role="menu"
                                            aria-labelledby="classification-group-dropdown">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li v-for="group in groups">
                                                        <a role="menuitem" tabindex="-1"
                                                            @click="changeClassificationGroup(group)">{{
                                                                group.title }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div><!-- /btn-group -->
                                    <div class="search">
                                        <input type="text" v-model="searchText"
                                            :placeholder="$t('QuestionnaireEditor.SearchClassification')"
                                            class="form-control" id="searchFor" aria-label="classification-search" v-focus>
                                    </div>
                                    <div class="input-group-btn input-group-btn-right">
                                        <span class="search-icon" aria-hidden="true"></span>
                                        <button type="button" class="btn btn-link btn-clear">
                                            <span></span>
                                        </button>
                                    </div>
                                </div><!-- /input-group -->
                            </form>
                            <span class="group-summary pull-right">{{ totalResults }} {{
                                $t('QuestionnaireEditor.ClassificationsSearchEntities') }}</span>
                        </div>
                        <perfect-scrollbar class="scroller">
                            <div class="tile-wrapper">
                                <div class="tile-column">
                                    <div class="tile" v-for="classification in classifications1">
                                        <h4>{{ classification.title }}</h4>
                                        <button type="button" class="btn btn-link show-answers"
                                            :class="{ 'collapsed': classification.categoriesAreOpen == false }"
                                            @click="toggleCategories(classification)">
                                            {{ $t('QuestionnaireEditor.ClassificationShowCategories') }} ({{
                                                classification.categoriesCount
                                            }})
                                        </button>
                                        <div class="collapse" :class="{ 'in': classification.categoriesAreOpen }"
                                            v-show="classification.categoriesAreOpen">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled list-categories">
                                                    <li v-for="category in classification.categories">
                                                        <span>{{ category.title }}<span>...</span></span> <span
                                                            class="option-value">{{
                                                                category.value }}</span>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                        <div class="tile-footer clearfix">
                                            <a class="bg-info" @click="changeClassificationGroup(classification.group)">{{
                                                classification.group.title }}</a>
                                            <button v-if="!isReadOnlyForUser" class="btn btn-link add-button"
                                                @click="replaceOptions(classification)">{{ $t('QuestionnaireEditor.Add')
                                                }}</button>
                                        </div>
                                    </div>
                                </div>
                                <div class="tile-column">
                                    <div class="tile" v-for="classification in classifications2">
                                        <h4>{{ classification.title }}</h4>
                                        <button type="button" class="btn btn-link show-answers"
                                            :class="{ 'collapsed': classification.categoriesAreOpen == false }"
                                            @click="toggleCategories(classification)">
                                            {{ $t('QuestionnaireEditor.ClassificationShowCategories') }} ({{
                                                classification.categoriesCount
                                            }})
                                        </button>
                                        <div class="collapse" :class="{ 'in': classification.categoriesAreOpen }"
                                            v-show="classification.categoriesAreOpen">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled list-categories">
                                                    <li v-for="category in classification.categories">
                                                        <span>{{ category.title }}<span>...</span></span> <span
                                                            class="option-value">{{
                                                                category.value }}</span>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                        <div class="tile-footer clearfix">
                                            <a class="bg-info" @click="changeClassificationGroup(classification.group)">{{
                                                classification.group.title }}</a>
                                            <button type="button" v-if="!isReadOnlyForUser" class="btn btn-link add-button"
                                                @click="replaceOptions(classification)">{{ $t('QuestionnaireEditor.Add')
                                                }}</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </perfect-scrollbar>
                    </div>
                    <!--div class="modal-footer">
                    <button class="btn" data-dismiss="modal" aria-hidden="true">{{ $t('QuestionnaireEditor.Cancel') }}</button>
                    <button id="assign-folder-button" type="submit" class="btn btn-primary"
                        value="@QuestionnaireController.Assign"
                        onclick="this.disabled = true; this.value = '@QuestionnaireController.Assigning'; window.assignFolder();">@QuestionnaireController.Assign</button>
                </div-->
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script>

import { useClassificationsStore } from '../../../../stores/classifications';

import { willBeTakenOnlyFirstOptionsConfirmationPopup, replaceOptionsConfirmationPopup } from '../../../../services/utilityService'
import { replaceOptionsWithClassification } from '../../../../services/classificationIdService'

import { debounce, isNull, isUndefined } from 'lodash'

export default {
    name: 'AddClassification',
    components: {
    },
    inject: ['questionnaire', 'currentChapter'],
    props: {
        questionnaireId: { type: String, required: true },
        activeQuestion: { type: Object, required: true },
    },
    data() {
        return {
            //groups: [],
            selectedGroup: {},
            searchText: '',
            //classifications1: [],
            //classifications2: [],
            //totalResults: 0,
            open: false,
            MAX_OPTIONS_COUNT: 200,
        }
    },
    setup() {
        const classificationsStore = useClassificationsStore();

        return {
            classificationsStore,
        };
    },
    watch: {
        searchText(newValue, oldValue) {
            this.searchThrottled();
        }
    },
    computed: {
        isReadOnlyForUser() {
            return this.questionnaire.isReadOnlyForUser || this.currentChapter.isReadOnly || false;
        },
        hasOptions() {
            return this.activeQuestion.optionsCount > 0;
        },
        dialogStyle() {
            if (this.open)
                return { display: 'block', opacity: 1, 'z-index': 1050 };
            return {};
        },
        groups() {
            return this.classificationsStore.groups;
        },
        classifications1() {
            return this.classificationsStore.classifications1;
        },
        classifications2() {
            return this.classificationsStore.classifications2;
        },
        totalResults() {
            return this.classificationsStore.totalResults;
        },
    },
    methods: {
        searchThrottled() {
            const searchDebounce = debounce(this.search, 1000);
            searchDebounce();
        },
        async loadClassificationGroups() {
            await this.classificationsStore.loadClassificationGroups();
            this.selectedGroup = this.groups[0];
        },

        async search() {
            const searchText = this.searchText.toLowerCase();
            await this.classificationsStore.search(searchText);

            //this.classifications1 = this.classificationsStore.classifications1;
            //this.classifications2 = this.classificationsStore.classifications2;
            //this.totalResults = this.classificationsStore.totalResults;
        },

        changeClassificationGroup(group) {
            this.selectedGroup = group;
            this.searchThrottled();
        },

        async toggleCategories(classification) {
            if (classification.categories.length === 0) {
                await this.classificationsStore.loadCategories(classification);
                classification.categoriesAreOpen = true;
            } else {
                classification.categoriesAreOpen = !classification.categoriesAreOpen;
            }
        },

        async replaceOptions(classification) {

            if (classification.categories.length === 0) {
                await this.classificationsStore.loadCategories(classification)
            }

            const selectedClassification = classification;
            if (isNull(selectedClassification) || isUndefined(selectedClassification))
                return;

            if (this.activeQuestion.options.length > 0) {
                const questionTitle = this.activeQuestion.title || this.$t('QuestionnaireEditor.UntitledQuestion');
                let confirmParams = replaceOptionsConfirmationPopup(questionTitle);
                confirmParams.callback = confirmResult => {
                    if (confirmResult) {
                        this.doReplaceOptions(selectedClassification);
                    }
                };
                this.$confirm(confirmParams)
            } else {
                this.doReplaceOptions(selectedClassification);
            }

            this.open = false;
        },

        doReplaceOptions(selectedClassification) {
            const questionTitle = this.activeQuestion.title || this.$t('QuestionnaireEditor.UntitledQuestion');
            const optionsToInsertCount = selectedClassification.categoriesCount;

            if (optionsToInsertCount > this.MAX_OPTIONS_COUNT) {
                if (this.activeQuestion.type !== "SingleOption") {
                    var modalParams = willBeTakenOnlyFirstOptionsConfirmationPopup(questionTitle, this.MAX_OPTIONS_COUNT);
                    modalParams.callback = confirmResult => {
                        if (confirmResult) {
                            this.activeQuestion.options = selectedClassification.categories;
                            this.activeQuestion.optionsCount = this.activeQuestion.options.length;
                        }
                    }
                    this.$confirm(modalParams);
                } else {
                    replaceOptionsWithClassification(
                        this.questionnaireId,
                        this.activeQuestion.id,
                        selectedClassification.id);

                    this.activeQuestion.isFilteredCombobox = true;
                    this.activeQuestion.options = selectedClassification.categories;
                    this.activeQuestion.optionsCount = this.activeQuestion.options.length;
                }
            } else {
                if (this.activeQuestion.isFilteredCombobox) {
                    replaceOptionsWithClassification(
                        this.questionnaireId,
                        this.activeQuestion.id,
                        selectedClassification.id);
                }
                this.activeQuestion.options = selectedClassification.categories;
                this.activeQuestion.optionsCount = selectedClassification.categories.length;
            }
        },

        async openDialog() {
            //this.saveQuestion(showModal); TODO
            await this.loadClassificationGroups();
            this.open = true;
            await this.search();
        },
    }
}
</script>
