<template>
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
                            uib-dropdown-toggle type="button" data-bs-toggle="dropdown" aria-expanded="false">
                            {{ selectedGroup.title }}
                            <span class="dropdown-arrow"></span>
                        </button>
                        <div class="dropdown-menu" role="menu" aria-labelledby="classification-group-dropdown">
                            <perfect-scrollbar class="scroller">
                                <ul class="list-unstyled">
                                    <li v-for="group in groups">
                                        <a role="menuitem" tabindex="-1" @click="changeClassificationGroup(group)">{{
                                            group.title }}</a>
                                    </li>
                                </ul>
                            </perfect-scrollbar>
                        </div>
                    </div><!-- /btn-group -->
                    <div class="search">
                        <input type="text" v-model="searchText"
                            :placeholder="$t('QuestionnaireEditor.SearchClassification')" class="form-control"
                            id="searchFor" aria-label="classification-search" v-focus>
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
                            {{ $t('QuestionnaireEditor.ClassificationShowCategories') }} ({{ classification.categoriesCount
                            }})
                        </button>
                        <div class="collapse" :class="{ 'in': classification.categoriesAreOpen }"
                            v-show="classification.categoriesAreOpen">
                            <perfect-scrollbar class="scroller">
                                <ul class="list-unstyled list-categories">
                                    <li v-for="category in classification.categories">
                                        <span>{{ category.title }}<span>...</span></span> <span class="option-value">{{
                                            category.value }}</span>
                                    </li>
                                </ul>
                            </perfect-scrollbar>
                        </div>
                        <div class="tile-footer clearfix">
                            <a class="bg-info" @click="changeClassificationGroup(classification.group)">{{
                                classification.group.title }}</a>
                            <button v-if="!isReadOnlyForUser" class="btn btn-link add-button"
                                @click="replaceOptions(classification)">{{ $t('QuestionnaireEditor.Add') }}</button>
                        </div>
                    </div>
                </div>
                <div class="tile-column">
                    <div class="tile" v-for="classification in classifications2">
                        <h4>{{ classification.title }}</h4>
                        <button type="button" class="btn btn-link show-answers"
                            :class="{ 'collapsed': classification.categoriesAreOpen == false }"
                            @click="toggleCategories(classification)">
                            {{ $t('QuestionnaireEditor.ClassificationShowCategories') }} ({{ classification.categoriesCount
                            }})
                        </button>
                        <div class="collapse" :class="{ 'in': classification.categoriesAreOpen }"
                            v-show="classification.categoriesAreOpen">
                            <perfect-scrollbar class="scroller">
                                <ul class="list-unstyled list-categories">
                                    <li v-for="category in classification.categories">
                                        <span>{{ category.title }}<span>...</span></span> <span class="option-value">{{
                                            category.value }}</span>
                                    </li>
                                </ul>
                            </perfect-scrollbar>
                        </div>
                        <div class="tile-footer clearfix">
                            <a class="bg-info" @click="changeClassificationGroup(classification.group)">{{
                                classification.group.title }}</a>
                            <button type="button" v-if="!isReadOnlyForUser" class="btn btn-link add-button"
                                @click="replaceOptions(classification)">{{ $t('QuestionnaireEditor.Add') }}</button>
                        </div>
                    </div>
                </div>
            </div>
        </perfect-scrollbar>
    </div>
</template>

<script>

import { useClassificationsStore } from '../../../../stores/classifications';

import { debounce } from 'lodash'

export default {
    name: 'AddClassification',
    components: {
    },
    props: {
        isReadOnlyForUser: { type: Boolean, required: true },
        hasOptions: { type: Boolean, required: true },
    },
    data() {
        return {
            groups: [],
            selectedGroup: allClassificationGroups,
            searchText: '',
            classifications1: [],
            classifications2: [],
            totalResults: 0,
            open: false,
        }
    },
    setup() {
        const classificationsStore = useClassificationsStore();

        return {
            classificationsStore,
        };
    },
    beforeMount() {
        this.loadClassificationGroups();
    },
    watch: {
        searchText(newValue, oldValue) {
            this.searchThrottled();
        }
    },
    methods: {
        searchThrottled() {
            const searchDebounce = debounce(this.search, 1000);
            searchDebounce();
        },
        async loadClassificationGroups() {
            await this.classificationsStore.loadClassificationGroups();
            this.groups = this.classificationsStore.groups;
        },

        async search() {
            const searchText = this.searchText.toLowerCase();
            await this.classificationsStore.search(searchText);

            this.classifications1 = this.classificationsStore.classifications1;
            this.classifications2 = this.classificationsStore.classifications2;
            this.totalResults = this.classificationsStore.totalResults;
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
                //$uibModalInstance.close(classification);
                this.open = false;
            } else {
                //$uibModalInstance.close(classification);
                this.open = false;
            }
        },
    }
}
</script>
