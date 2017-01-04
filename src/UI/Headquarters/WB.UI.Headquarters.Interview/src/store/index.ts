declare var require: any

const Vuex = require('vuex')

const store: any = new Vuex.Store({
  state: {
    interview: {
      id: null
    }
  },
  actions: {
    InterviewMount ({commit}, {id}) {
      commit('SET_INTERVIEW', id)
    }
  },
  mutations: {
    SET_INTERVIEW (state, id) {
      state.interview.id = id;
    }
  }
})

export default store