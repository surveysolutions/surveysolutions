import { defineStore } from 'pinia';
import { each } from 'lodash';
import { get, post, patch, del } from '../services/apiService';
import { i18n } from '../plugins/localization';

const baseUrl = '/api/';

const allClassificationGroups = {
    id: null,
    title: i18n.$t('QuestionnaireEditor.AllClassifications')
};
const myClassificationGroups = {
    id: null,
    title: i18n.$t('QuestionnaireEditor.MyClassifications'),
    privateOnly: true
};

export const useClassificationsStore = defineStore('classifications', {
    state: () => ({
        groups: [],
        selectedGroup: allClassificationGroups,
        searchText: '',
        classifications1: [],
        classifications2: [],
        totalResults: 0
    }),
    getters: {
        getClassifications: state => state.classifications
    },
    actions: {
        async loadClassificationGroups() {
            const response = await get(baseUrl + 'classifications/groups');
            this.groups = response.data;
            this.groups.splice(0, 0, myClassificationGroups);
            this.groups.splice(0, 0, allClassificationGroups);
        },

        async loadCategories(classification) {
            if (classification.categories.length > 0)
                return new Promise(function(resolve, reject) {
                    resolve();
                });
            const response = get(
                baseUrl +
                    'classifications/classification/' +
                    classification.id +
                    '/categories'
            );
            classification.categories = response.data;
        },

        async search(searchText) {
            const data = await get(
                baseUrl + 'classifications/classifications/search',
                {
                    query: searchText,
                    groupId: this.selectedGroup.id,
                    privateOnly: this.selectedGroup.privateOnly || false
                }
            );

            var classifications = data.classifications;
            each(classifications, function(classification) {
                classification.categories = [];
                classification.categoriesAreOpen = false;
            });
            var half = Math.ceil(classifications.length / 2);
            this.classifications1 = classifications.slice(0, half);
            this.classifications2 = classifications.slice(half);
            this.totalResults = data.total;
        }
    }
});
