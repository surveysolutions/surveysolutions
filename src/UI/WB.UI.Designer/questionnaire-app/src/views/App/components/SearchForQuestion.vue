<template>
    <Teleport to="body">
        <div class="modal fade add-classification-modal search-for-question-modal dragAndDrop ng-scope ng-isolate-scope in"
            id="mAddCalassification" :style="dialogStyle" v-if="open == true" uib-modal-window="modal-window"
            role="dialog" index="0" animate="animate"
            ng-style="{'z-index': 1050 + $$topModalIndex*10, display: 'block'}" tabindex="-1"
            uib-modal-animation-class="fade" modal-in-class="in" modal-animation="true" v-dragAndDrop>
            <div class=" modal-dialog">
                <div class="modal-content">
                    <div class="modal-header blue-strip handle">
                        <button type="button" class="close" aria-hidden="true" @click="close()"></button>
                        <h3 class="modal-title">
                            {{ $t('QuestionnaireEditor.QuestionsSearchTitle') }}
                        </h3>
                    </div>
                    <div class="modal-body">
                        <div class="header-block clearfix">
                            <form>
                                <div class="input-group custom-dropdown-search">
                                    <div class="input-group-btn custom-dropdown">
                                        <button class="btn dropdown-toggle" id="classification-group" type="button"
                                            data-bs-toggle="dropdown" aria-expanded="false">
                                            {{ selectedFilter.title }}
                                            <span class="dropdown-arrow"></span>
                                        </button>
                                        <div class="dropdown-menu" role="menu"
                                            aria-labelledby="classification-group-dropdown">
                                            <perfect-scrollbar class="scroller">
                                                <ul class="list-unstyled">
                                                    <li v-for="filter in filters">
                                                        <a role="menuitem" tabindex="-1"
                                                            @click="changeFilter(filter)">{{
                filter.title
            }}</a>
                                                    </li>
                                                </ul>
                                            </perfect-scrollbar>
                                        </div>
                                    </div><!-- /btn-group -->
                                    <div class="search">
                                        <input type="text" v-model="searchText" v-focus
                                            :placeholder="$t('QuestionnaireEditor.SearchQuestionPlaceholder')"
                                            class="form-control" id="searchFor" aria-label="classification-search">
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
                $t('QuestionnaireEditor.QuestionsSearchEntities')
            }}</span>
                        </div>
                        <perfect-scrollbar class="scroller">
                            <div class="tile-wrapper">
                                <div class="tile-column">
                                    <div class="tile" :class="[searchResult.type]"
                                        v-for="searchResult in  searchResult1 ">
                                        <h4>
                                            <div v-if="searchResult.icon !== null" class="icon"
                                                :class="[searchResult.icon]"></div><a target="_blank"
                                                rel="noopener noreferrer" :href="getLink(searchResult)"><span
                                                    class="roster-marker" v-show="searchResult.type === 'roster'">{{
                $t('QuestionnaireEditor.TreeRoster') }}</span> {{ searchResult.title
                                                }}</a>
                                        </h4>
                                        <h5>{{ searchResult.questionnaireTitle }}</h5>
                                        <div class="tile-footer clearfix">
                                            <a v-if="searchResult.hasFolder" class="bg-info"
                                                @click="changeFilter(searchResult.folder)">{{
                searchResult.folder.title }}</a>
                                            <button class="btn btn-link add-button"
                                                @click="pasteEntity(searchResult)">{{
                $t('QuestionnaireEditor.Add') }}</button>
                                        </div>
                                    </div>
                                </div>
                                <div class="tile-column">
                                    <div class="tile" :class="[searchResult.type]"
                                        v-for="searchResult in searchResult2">
                                        <h4>
                                            <div v-if="searchResult.icon !== null" class="icon"
                                                :class="[searchResult.icon]"></div><a target="_blank"
                                                rel="noopener noreferrer" :href="getLink(searchResult)"><span
                                                    class="roster-marker" v-show="searchResult.type === 'roster'">{{
                $t('QuestionnaireEditor.TreeRoster') }}</span> {{ searchResult.title
                                                }}</a>
                                        </h4>
                                        <h5>{{ searchResult.questionnaireTitle }}</h5>
                                        <div class="tile-footer clearfix">
                                            <a v-if="searchResult.hasFolder" class="bg-info"
                                                @click="changeFilter(searchResult.folder)">{{
                                                searchResult.folder.title }}</a>
                                            <button class="btn btn-link add-button"
                                                @click="pasteEntity(searchResult)">{{
                                                $t('QuestionnaireEditor.Add') }}</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </perfect-scrollbar>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script>

import { get } from '../../../services/apiService'
import { setFocusIn, guid } from '../../../services/utilityService'
import { answerTypeClass } from '../../../helpers/question'
import { debounce, forEach } from 'lodash'
import { pasteItemIntoDetailed } from '../../../services/copyPasteService'
import { i18n } from '../../../plugins/localization'

const allFolders = { id: null, title: i18n.t('QuestionnaireEditor.AllFolders') }

export default {
    name: 'SearchForQuestion',
    inject: ['isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        chapterId: { type: String, required: true }
    },
    data() {
        return {
            baseUrl: '/api/search',
            filters: [],
            selectedFilter: allFolders,
            searchText: '',
            searchResult1: [],
            searchResult2: [],
            totalResults: 0,
            open: false,
        }
    },
    watch: {
        searchText(newValue, oldValue) {
            this.searchThrottled();
        }
    },
    computed: {
        dialogStyle() {
            if (this.open)
                return { display: 'block', opacity: 1, 'z-index': 1050 };
            return {};
        },
    },
    methods: {
        loadFilters() {
            return get(this.baseUrl + '/filters').then(response => {
                this.filters = response;
                this.filters.splice(0, 0, allFolders);
            });
        },

        getIconClass(type, entity) {
            if (type === "question")
                return answerTypeClass[entity.itemType];
            if (type === 'static-text')
                return "icon-statictext";
            if (type === 'variable')
                return "icon-variable";
            return null;
        },

        searchThrottled() {
            const searchDebounce = debounce(this.search, 1000);
            searchDebounce();
        },

        search() {
            const searchText = this.searchText.toLowerCase();
            return get(this.baseUrl + '', {
                query: searchText,
                folderId: this.selectedFilter.publicId,
                privateOnly: false
            }
            )
                .then(response => {
                    var results = response.entities;
                    forEach(results, entity => {
                        entity.hasFolder = (entity.folder || null) != null;
                        entity.type = this.getType(entity);
                        entity.icon = this.getIconClass(entity.type, entity);
                    });
                    const half = Math.ceil(results.length / 2);
                    this.searchResult1 = results.slice(0, half);
                    this.searchResult2 = results.slice(half);
                    this.totalResults = response.total;
                });
        },

        changeFilter(filter) {
            this.selectedFilter = filter;
            this.searchThrottled();
        },

        getType(searchResult) {
            switch (searchResult.itemType) {
                case 'Group': return "group";
                case 'Roster': return "roster";
                case 'StaticText': return 'static-text';
                case 'Chapter': return 'group';
                case 'Variable': return 'variable';
                default: return 'question';
            }
        },

        getLink(searchResult) {
            return '/questionnaire/details/' + searchResult.questionnaireId + '/nosection/' + searchResult.type + '/' + searchResult.itemId;
        },

        async pasteEntity(searchResult) {
            const newId = guid();
            const entityToPaste = searchResult;

            await pasteItemIntoDetailed(this.questionnaireId, this.chapterId, entityToPaste.questionnaireId, entityToPaste.itemId, newId)

            this.$router.push({
                name: entityToPaste.itemType.toLowerCase(),
                params: {
                    chapterId: this.chapterId,
                    entityId: newId
                }
            });

            this.open = false;
        },

        show() {
            this.open = true;
            setFocusIn('searchFor');
            this.loadFilters().then(() => {
                this.search();
            });
        },

        close() {
            this.open = false;
        }
    }
}
</script>
