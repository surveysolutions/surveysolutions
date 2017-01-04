import * as Vuex from "vuex"

const store: any = new Vuex.Store({
  state: {
    prefilledQuestions: [
        {id: 1, title: "text1"},
        {id: 2, title: "text2"},
        {id: 3, title: "text3"}
    ],
    interview:{
        id: "null"
    }
  },
  actions: {
    InterviewMount ({commit}, {id}) {
      commit("SET_INTERVIEW", id)
    }
  },
  getters: {
      prefilledQuestions: state => {
          return state.prefilledQuestions;
      }
  },
  mutations: {
    SET_INTERVIEW (state, id) {
      state.interview.id = id;
    }
  }
})

export default store
