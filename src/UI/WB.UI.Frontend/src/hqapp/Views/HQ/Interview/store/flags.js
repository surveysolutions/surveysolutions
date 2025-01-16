import { api } from '~/webinterview/api/http'

export default {
    state: {
        flagged: {},
    },

    actions: {
        setFlag({ commit, dispatch }, { questionId, hasFlag }) {
            return api.answer(questionId, 'setFlag', { hasFlag })
                .then(() => {
                    commit('SET_FLAG', { questionId, hasFlag })
                    dispatch('refreshSearchResults')
                    dispatch('fetchEntity', { id: questionId })
                })
        },
        async fetchFlags({ commit }) {
            const flags = await api.get('getFlags')
            commit('SET_FLAGS', { flags })
        },
    },

    mutations: {
        SET_FLAG(state, { questionId, hasFlag }) {
            if (hasFlag) {
                state.flagged[questionId] = true
            } else {
                delete state.flagged[questionId]
            }
        },
        SET_FLAGS(state, { flags }) {
            state.flagged = {}
            flags.forEach(flag => state.flagged[flag] = true)
        },
    },

    getters: {
        flags(state) {
            return state.flagged
        },
    },
}
