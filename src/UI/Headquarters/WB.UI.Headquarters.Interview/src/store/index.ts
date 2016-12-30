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
      commit('setInterview', id)
    }
  },
  mutations: {
    setInterview (state, id) {
      state.interview.id = id;
    }
  }
})

export default store