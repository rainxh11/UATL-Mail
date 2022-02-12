import actions from './actions'
import mutations from './mutations'

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
