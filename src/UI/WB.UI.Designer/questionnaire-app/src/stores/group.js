import { defineStore } from 'pinia';
import { getGroup } from '../services/groupService';
import emitter from '../services/emitter';

export const useGroupStore = defineStore('group', {
    state: () => ({
        groupAndBreadcrumbs: {},
        initialGroup: {}
    }),
    getters: {
        getGroup: state => state.groupAndBreadcrumbs.group,
        getBreadcrumbs: state => state.groupAndBreadcrumbs.breadcrumbs
    },
    actions: {
        setupListeners() {
            emitter.on('groupUpdated', this.groupUpdated);
            emitter.on('groupDeleted', this.groupDeleted);          
        },
        groupUpdated(payload) {
            if ((this.groupAndBreadcrumbs.id = payload.itemId)) {
                this.setGroupData(payload);
            }
        },
        groupDeleted(payload) {
            if ((this.groupAndBreadcrumbs.id = payload.itemId)) {
                this.clear();
            }
        },
        async fetchGroupData(questionnaireId, groupId) {
            const data = await getGroup(questionnaireId, groupId);
            this.setGroupAndBreadcrumbsData(data);
        },

        //TODO: change service to return group and breadcrumbs in one oject
        setGroupAndBreadcrumbsData(data) {
            this.groupAndBreadcrumbs = data;
            this.initialGroup = Object.assign({}, data.group);
        },

        setGroupData(data) {
            this.groupAndBreadcrumbs.group = data;
            this.initialGroup = Object.assign({}, data.group);
        },

        clear() {
            this.groupAndBreadcrumbs = {};
            this.initialGroup = {};
        },        

        discardChanges() {
            Object.assign(this.groupAndBreadcrumbs.group, this.initialGroup);
        }        
    }
});
