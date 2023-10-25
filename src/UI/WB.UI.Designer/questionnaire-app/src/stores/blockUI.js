import { defineStore } from 'pinia';

export const useBlockUIStore = defineStore('blockUI', {
    state: () => ({
        isBlock: false
    }),
    getters: {},
    actions: {
        start() {
            this.isBlock = true;
        },

        stop() {
            this.isBlock = false;
        }
    }
});
