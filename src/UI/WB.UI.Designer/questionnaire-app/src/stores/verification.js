import { defineStore } from 'pinia';
import { get } from '../services/apiService';

export const useVerificationStore = defineStore('verification', {
    state: () => ({
        status: {}
    }),
    getters: {
        getInfo: state => state.info
    },
    actions: {
        async fetchVerificationStatus(questionnaireId) {
            const status = await get(questionnaireId);
            this.setVerificationStatus(status);
        },

        setVerificationStatus(status) {
            this.status = status;
            this.status.time = new Date();
        }
    }
});
