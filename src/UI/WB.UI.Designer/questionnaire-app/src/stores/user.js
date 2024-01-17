import { defineStore } from 'pinia'

import { get } from '../services/apiService';

export const useUserStore = defineStore('user', {
    state: () => ({
      userName: '',
      email: '',
      isAuthenticated: false,
    }),
    getters: {
      getInfo: (state) => state.info,
    },
    actions: {

      async fetchUserInfo() {
        const data = await get('/api/users/CurrentLogin');
        this.setUserInfo(data)
      },

      setUserInfo(data) {
        this.userName = data.userName;
        this.email = data.email;
        this.isAuthenticated = data.isAuthenticated;
      }

    },    
})
