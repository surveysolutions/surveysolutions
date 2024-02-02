import { defineStore } from 'pinia';
import { getGroup } from '../services/groupService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useGroupStore = defineStore('group', {
    state: () => ({
        group: {},
        initialGroup: {},
        breadcrumbs: {}
    }),
    getters: {
        getGroup: state => state.group,
        getBreadcrumbs: state => state.breadcrumbs,
        getInitialGroup: state => state.initialGroup,
        getIsDirty: state => !_.isEqual(state.group, state.initialGroup)
    },
    actions: {
        setupListeners() {
            emitter.on('groupUpdated', this.groupUpdated);
            emitter.on('groupDeleted', this.groupDeleted);
        },
        groupUpdated(payload) {
            if ((this.group.id = payload.id)) {
                this.setGroupData(payload);
            }
        },
        groupDeleted(payload) {
            if ((this.group.id = payload.itemId)) {
                this.clear();
            }
        },
        async fetchGroupData(questionnaireId, groupId) {
            const data = await getGroup(questionnaireId, groupId);
            this.setGroupAndBreadcrumbsData(data);
        },

        //TODO: change service to return group and breadcrumbs in one oject
        setGroupAndBreadcrumbsData(data) {
            this.setGroupData(data.group);
            this.breadcrumbs = _.cloneDeep(data.breadcrumbs);
        },

        setGroupData(groupData) {
            this.initialGroup = _.cloneDeep(groupData);
            this.group = _.cloneDeep(this.initialGroup);
        },

        clear() {
            this.group = {};
            this.initialGroup = {};
            this.breadcrumbs = {};
        },

        discardChanges() {
            this.group = _.cloneDeep(this.initialGroup);
        }
    }
});
