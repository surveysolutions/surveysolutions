import { createStore } from 'vuex';
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
        store.commit('start_loading');
        return config;
    },
    function(error) {
        store.commit('finish_loading');
        console.log(error);
        return Promise.reject(error);
    }
);

// Add a response interceptor
$http.interceptors.response.use(
    function(response) {
        store.commit('finish_loading');
        return response;
    },
    function(error) {
        store.commit('finish_loading');
        console.log(error);
        return Promise.reject(error);
    }
);

const store = createStore({
    state() {
        return {
            isLoading: false,
            groups: [],
            classifications: [],
            categories: [],
            activeGroup: {},
            activeClassification: {},
            userId: null,
            isAdmin: false
        };
    },
    mutations: {
        start_loading: function(state) {
            state.isLoading = true;
        },
        finish_loading: function(state) {
            state.isLoading = false;
        },
        groups_loaded: function(state, groups) {
            state.groups = groups;
            state.classifications = [];
            state.categories = [];
        },
        classifications_loaded: function(state, classifications) {
            state.classifications = classifications;
            state.categories = [];
        },
        categories_loaded: function(state, categories) {
            state.categories = categories;
        },
        addGroup: function(state, group) {
            state.groups.push(group);
        },
        updateGroup: function(state, group) {
            var g = state.groups[group.index];
            g.title = group.title;
            g.isNew = false;
        },
        deleteGroup: function(state, index) {
            state.groups.splice(index, 1);
        },
        selectGroup: function(state, index) {
            state.activeGroup.isActive = false;
            if (state.groups.length > 0) {
                state.activeGroup = state.groups[index];
                state.activeGroup.isActive = true;
                state.activeClassification.isActive = false;
                state.activeClassification = {};
            } else state.activeGroup = {};
        },
        selectClassification: function(state, index) {
            state.activeClassification.isActive = false;
            if (state.classifications.length > 0) {
                state.activeClassification = state.classifications[index];
                state.activeClassification.isActive = true;
            } else state.activeClassification = {};
        },
        addClassification: function(state, classification) {
            state.classifications.push(classification);
            state.activeGroup.count++;
        },
        addCategory: function(state, category) {
            state.categories.push(category);
        },
        updateClassification: function(state, classification) {
            var g = state.classifications[classification.index];
            g.title = classification.title;
            g.isNew = false;
        },
        deleteClassification: function(state, index) {
            state.classifications.splice(index, 1);
            state.activeGroup.count--;
        },
        updateCategories: function(state) {
            state.activeClassification.count = state.categories.length;
        },
        deleteCategory: function(state, index) {
            state.categories.splice(index, 1);
        },
        updateCategory: function(state, changes) {
            var category = state.categories[changes.index];
            category.title = changes.title;
            category.value = changes.value;
        },
        updateUserInfo: function(state, info) {
            state.userId = info.userId;
            state.isAdmin = info.isAdmin;
        }
    },
    actions: {
        getUserInfo(context) {
            $http.get(routes.userInfo, {}).then(response => {
                context.commit('updateUserInfo', response.data);
            });
        },
        updateCategory(context, changes) {
            context.commit('updateCategory', changes);
        },
        deleteCategory(context, index) {
            context.commit('deleteCategory', index);
        },
        addCategory(context, category) {
            context.commit('addCategory', category);
        },
        updateCategories(context, classificationId) {
            $http
                .post(
                    routes.updateCategories.format(classificationId),
                    context.state.categories
                )
                .then(function() {
                    context.commit('updateCategories');
                });
        },
        addGroup(context, group) {
            context.commit('addGroup', group);
        },
        updateGroup(context, group) {
            (group.isNew
                ? $http.post(routes.createGroup, group)
                : $http.patch(routes.updateGroup.format(group.id), group)
            ).then(function() {
                context.commit('updateGroup', group);
            });
        },
        deleteGroup(context, index) {
            var group = context.state.groups[index] || {};
            if (group.isNew) {
                context.commit('deleteGroup', index);
                context.dispatch('selectGroup', 0);
            } else {
                $http
                    .delete(routes.deleteGroup.format(group.id))
                    .then(function() {
                        context.commit('deleteGroup', index);
                        context.dispatch('selectGroup', 0);
                    });
            }
        },
        selectGroup(context, index) {
            context.commit('selectGroup', index);
            if (context.state.groups.length > 0)
                context.dispatch(
                    'loadClassifications',
                    context.state.groups[index].id
                );
        },
        addClassification(context, classification) {
            context.commit('addClassification', classification);
        },
        updateClassification(context, classification) {
            (classification.isNew
                ? $http.post(routes.createClassification, classification)
                : $http.patch(
                      routes.updateClassification.format(classification.id),
                      classification
                  )
            ).then(function() {
                context.commit('updateClassification', classification);
            });
        },
        deleteClassification(context, index) {
            var classification = context.state.classifications[index] || {};
            if (classification.isNew) {
                context.commit('deleteClassification', index);
                context.dispatch('selectClassification', 0);
            } else {
                $http
                    .delete(
                        routes.deleteClassification.format(classification.id)
                    )
                    .then(function() {
                        context.commit('deleteClassification', index);
                        context.dispatch('selectClassification', 0);
                    });
            }
        },
        selectClassification(context, index) {
            context.commit('selectClassification', index);
            if (context.state.classifications.length > 0)
                context.dispatch(
                    'loadCategories',
                    context.state.classifications[index].id
                );
        },

        loadGroups: function(context) {
            var url = routes.groups;
            return $http.get(url, {}).then(function(response) {
                context.commit('groups_loaded', response.data);
                if (context.state.groups.length > 0) {
                    context.dispatch('selectGroup', 0);
                }
            });
        },
        loadClassifications: function(context, groupId) {
            var url = routes.classifications;

            return $http
                .get(url, { params: { groupId: groupId } })
                .then(function(response) {
                    context.commit('classifications_loaded', response.data);
                    if (context.state.classifications.length > 0) {
                        context.dispatch('selectClassification', 0);
                    }
                });
        },
        loadCategories: function(context, classificationId) {
            var url = routes.categories.format(classificationId);
            return $http.get(url, {}).then(function(response) {
                var categories = response.data;
                context.commit('categories_loaded', categories);
            });
        }
    }
});

export default store;
