import state from './state'
import actions from './actions'
import mutations from './mutations'
import getters from '@/store/app/getters'
/*
|---------------------------------------------------------------------
| Email Vuex Store
|---------------------------------------------------------------------
*/
export default {
  namespaced: true,
  state,
  actions,
  mutations,
  getters
}
