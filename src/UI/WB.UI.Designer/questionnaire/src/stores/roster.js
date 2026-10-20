import { defineStore } from 'pinia';
import { getRoster } from '../services/rosterService';
import { get, commandCall } from '../services/apiService';
import emitter from '../services/emitter';
import _ from 'lodash';

export const useRosterStore = defineStore('roster', {
    state: () => ({
        roster: {},
        initialRoster: {},
        questionnaireId: null
    }),
    getters: {
        getRoster: state => state.roster,
        getInitialRoster: state => state.initialRoster,
        getIsDirty: state => !_.isEqual(state.roster, state.initialRoster)
    },
    actions: {
        setupListeners() {
            emitter.on('rosterUpdated', this.rosterUpdated);
            emitter.on('rosterDeleted', this.rosterDeleted);
            emitter.on('groupDeleted', this.groupDeleted);
            emitter.on('questionDeleted', this.questionDeleted);
            emitter.on('questionAdded', this.questionAdded);
            emitter.on('questionUpdated', this.questionUpdated);
            emitter.on('questionMoved', this.questionMoved);
            emitter.on('groupMoved', this.groupMoved);
            emitter.on('itemPasted', this.itemPasted);
        },
        rosterUpdated(payload) {
            if (this.roster.itemId === payload.roster.itemId) {
                this.setRosterData(payload.roster);
            }
        },
        async rosterDeleted(payload) {
            if (this.roster.itemId === payload.id) {
                this.clear();
            } else {
                await this.refreshRosterDataPreservingEdits();
            }
        },
        async refreshRosterDataPreservingEdits() {
            if (!this.roster.itemId || !this.questionnaireId) {
                return;
            }

            if (!this.getIsDirty) {
                await this.fetchRosterData(this.questionnaireId, this.roster.itemId);
            } else {
                // The roster may have been structurally changed while the user has unsaved edits.
                // A full refresh would discard those edits, so only update the
                // context-dependent question lists that are determined by location.
                const freshData = await getRoster(this.questionnaireId, this.roster.itemId);
                this.refreshContextualData(freshData);
            }
        },
        async groupDeleted(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async questionUpdated(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async itemPasted(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async questionDeleted(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async questionAdded(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async questionMoved(payload) {
            await this.refreshRosterDataPreservingEdits();
        },
        async groupMoved(payload) {
            if (
                this.questionnaireId &&
                payload.questionnaireId === this.questionnaireId &&
                (this.roster.itemId === payload.itemId ||
                    this.roster.breadcrumbs?.some(b => b.id === payload.itemId))
            ) {
                await this.refreshRosterDataPreservingEdits();
            }
        },
        // Updates only the fields that depend on the roster's structural position in the
        // questionnaire (available trigger question lists, breadcrumbs).  User edits to
        // title, type, variableName, fixedRosterTitles, etc. are intentionally preserved.
        // Both roster and initialRoster are patched so that dirty-detection continues to
        // reflect only genuine user changes, not stale server-side option lists.
        refreshContextualData(freshData) {
            const contextFields = [
                'numericIntegerQuestions',
                'textListsQuestions',
                'notLinkedMultiOptionQuestions',
                'numericIntegerTitles',
                'breadcrumbs'
            ];
            for (const field of contextFields) {
                if (freshData[field] !== undefined) {
                    this.roster[field] = _.cloneDeep(freshData[field]);
                    this.initialRoster[field] = _.cloneDeep(freshData[field]);
                }
            }
        },
        async fetchRosterData(questionnaireId, rosterId) {
            this.questionnaireId = questionnaireId;
            const data = await getRoster(questionnaireId, rosterId);
            this.setRosterData(data);
        },

        setRosterData(data) {
            this.initialRoster = _.cloneDeep(data);
            this.roster = _.cloneDeep(this.initialRoster);
        },

        clear() {
            this.roster = {};
            this.initialRoster = {};
            this.questionnaireId = null;
        },

        discardChanges() {
            this.roster = _.cloneDeep(this.initialRoster);
        }
    }
});
