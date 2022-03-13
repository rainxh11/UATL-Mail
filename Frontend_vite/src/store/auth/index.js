import actions from './actions'
import mutations from './mutations'
import Vuecookie from 'vue-cookies'

const state = {
  token: null,
  isAuth: false,
  userInfo: null

}

export default {
  namespaced: true,
  state,
  getters: {
    getToken(state) {
      const token = Vuecookie.get('T') || ''

      if (token !== '') state.token = token

      return state.token
    },
    getIsAuth(state) {
      return state.isAuth
    },
    getUserInfo(state) {
      return state.userInfo
    }
  },mutations,actions
}
