import { defineStore } from 'pinia'
import { debounce } from 'lodash'
import { mande } from 'mande'

const api = mande('/api/users/CurrentLogin'/*, globalOptions*/)

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
        const data = await api.get()
        this.setUserInfo(data)
      },

      setUserInfo(data) {
        this.userName = data.userName;
        this.email = data.email;
        this.isAuthenticated = data.isAuthenticated;
      }

    },    
})
