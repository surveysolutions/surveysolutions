import { defineStore } from 'pinia';

export const useBlockUIStore = defineStore('blockUI', {
    state: () => ({
        blocked: false
    }),
    getters: {
        isBlocked: state => state.blocked
    },
    actions: {
        start() {
            this.blocked = true;
        },

        stop() {
            this.blocked = false;
        }
    }
});
