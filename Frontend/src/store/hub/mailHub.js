const state = {
  connected: false,
  error: null
}

const getters = {
}

const actions = {
  connectionOpened({ commit }) {
    commit('SET_CONNECTION', true)
  },
  connectionClosed({ commit }) {
    commit('SET_CONNECTION', false)
  },
  connectionError({ commit }, error) {
    commit('SET_ERROR', error)
  }
}

const mutations = {
  SET_CONNECTION(state, message) {
    state.connected = message
  },
  SET_ERROR(state, error) {
    state.error = error
  }
}

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations
}
