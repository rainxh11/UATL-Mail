export default {
  getStarred(state) {
    return state.starred
  },
  isStarred({ state }, id) {
    return state.starred.some(x => x === id || x.ID === id)
  }
}
