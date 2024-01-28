import { defineStore } from 'pinia';
import { getCurrentUser } from '../services/userService';

export const useUserStore = defineStore('user', {
    state: () => ({
        info: {}
    }),
    getters: {
        getInfo: state => state.info
    },
    actions: {
        async fetchUserInfo() {
            const data = await getCurrentUser();
            this.info = data;
        }
    }
});
