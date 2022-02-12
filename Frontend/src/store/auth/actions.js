const retriveToken =  async ({ state, commit } , userInfo) => {

  commit('setIsAuth', true)
  commit('setToken', userInfo.token)
  commit('setUserInfo', userInfo.userInfo)

}

export default {
  retriveToken
}
