const retrieveToken =  async ({ state, commit } , userInfo) => {

  commit('setIsAuth', true)
  commit('setToken', userInfo.token)
  commit('setUserInfo', userInfo.userInfo)

}

export default {
  retrieveToken: retrieveToken
}
