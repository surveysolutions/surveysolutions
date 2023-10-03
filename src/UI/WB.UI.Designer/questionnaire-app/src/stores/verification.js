import { defineStore } from 'pinia';
import { mande } from 'mande';

const api = mande('/api/questionnaire/verify/' /*, globalOptions*/);

export const useVerificationStore = defineStore('verification', {
    state: () => ({
        status: {}
    }),
    getters: {
        getInfo: state => state.info
    },
    actions: {
        async fetchVerificationStatus(questionnaireId) {
            const status = await api.get(questionnaireId);
            this.setVerificationStatus(status);
        },

        setVerificationStatus(status) {
            this.status = status;
        }
    }
});
