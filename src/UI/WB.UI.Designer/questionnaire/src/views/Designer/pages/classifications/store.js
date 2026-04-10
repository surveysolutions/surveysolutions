import { defineStore } from 'pinia';
import axios from 'axios';

const routes = {
    userInfo: 'user',
    groups: 'groups',
    createGroup: 'group',
    updateGroup: 'group/{0}',
    deleteGroup: 'group/{0}',
    classifications: 'classifications',
    createClassification: 'classification',
    updateClassification: 'classification/{0}',
    deleteClassification: 'classification/{0}',
    categories: 'classification/{0}/categories',
    updateCategories: 'classification/{0}/categories'
};

const $http = axios.create({
    baseURL: './api/classifications'
});

// Add a request interceptor
$http.interceptors.request.use(
    function(config) {
        useClassificationsStore().isLoading = true;
        return config;
    },
    function(error) {
        useClassificationsStore().isLoading = false;
        console.log(error);
        return Promise.reject(error);
    }
);

// Add a response interceptor
$http.interceptors.response.use(
    function(response) {
        useClassificationsStore().isLoading = false;
        return response;
    },
    function(error) {
        useClassificationsStore().isLoading = false;
        console.log(error);
        return Promise.reject(error);
    }
);

export const useClassificationsStore = defineStore('classifications', {
    state() {
        return {
            isLoading: false,
            groups: [],
            classifications: [],
            categories: [],
            activeGroup: {},
            activeClassification: {},
            userId: null,
            userName: null,
            isAdmin: false
        };
    },
    actions: {
        async getUserInfo() {
            const response = await $http.get(routes.userInfo);
            const info = response.data;
            this.userId = info.userId;
            this.userName = info.userName;
            this.isAdmin = info.isAdmin;
        },
        updateCategory(changes) {
            var category = this.categories[changes.index];
            category.title = changes.title;
            category.value = changes.value;
        },
        deleteCategory(index) {
            this.categories.splice(index, 1);
        },
        addCategory(category) {
            this.categories.push(category);
        },
        async updateCategories(classificationId) {
            await $http.post(routes.updateCategories.format(classificationId), this.categories);
            this.activeClassification.count = this.categories.length;
        },
        addGroup(group) {
            this.groups.push(group);
        },
        async updateGroup(group) {
            await (group.isNew
                ? $http.post(routes.createGroup, group)
                : $http.patch(routes.updateGroup.format(group.id), group));
            var g = this.groups[group.index];
            g.title = group.title;
            g.isNew = false;
        },
        async deleteGroup(index) {
            var group = this.groups[index] || {};
            if (group.isNew) {
                this.groups.splice(index, 1);
                this.selectGroup(0);
            } else {
                await $http.delete(routes.deleteGroup.format(group.id));
                this.groups.splice(index, 1);
                this.selectGroup(0);
            }
        },
        selectGroup(index) {
            this.activeGroup.isActive = false;
            if (this.groups.length > 0) {
                this.activeGroup = this.groups[index];
                this.activeGroup.isActive = true;
                this.activeClassification.isActive = false;
                this.activeClassification = {};
            } else {
                this.activeGroup = {};
            }
            if (this.groups.length > 0)
                this.loadClassifications(this.groups[index].id);
        },
        addClassification(classification) {
            this.classifications.push(classification);
            this.activeGroup.count++;
        },
        async updateClassification(classification) {
            await (classification.isNew
                ? $http.post(routes.createClassification, classification)
                : $http.patch(routes.updateClassification.format(classification.id), classification));
            var g = this.classifications[classification.index];
            g.title = classification.title;
            g.isNew = false;
        },
        async deleteClassification(index) {
            var classification = this.classifications[index] || {};
            if (classification.isNew) {
                this.classifications.splice(index, 1);
                this.activeGroup.count--;
                this.selectClassification(0);
            } else {
                await $http.delete(routes.deleteClassification.format(classification.id));
                this.classifications.splice(index, 1);
                this.activeGroup.count--;
                this.selectClassification(0);
            }
        },
        selectClassification(index) {
            this.activeClassification.isActive = false;
            if (this.classifications.length > 0) {
                this.activeClassification = this.classifications[index];
                this.activeClassification.isActive = true;
            } else {
                this.activeClassification = {};
            }
            if (this.classifications.length > 0)
                this.loadCategories(this.classifications[index].id);
        },
        async loadGroups() {
            const response = await $http.get(routes.groups);
            this.groups = response.data;
            this.classifications = [];
            this.categories = [];
            if (this.groups.length > 0) {
                this.selectGroup(0);
            }
        },
        async loadClassifications(groupId) {
            const response = await $http.get(routes.classifications, { params: { groupId: groupId } });
            this.classifications = response.data;
            this.categories = [];
            if (this.classifications.length > 0) {
                this.selectClassification(0);
            }
        },
        async loadCategories(classificationId) {
            const response = await $http.get(routes.categories.format(classificationId));
            this.categories = response.data;
        }
    }
});

export default useClassificationsStore;
