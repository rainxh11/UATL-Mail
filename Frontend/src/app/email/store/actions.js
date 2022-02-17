import inboxEmails from './content/inbox'
import starredEmails from './content/starred'

export default {
  getInbox: ({ commit }, label) => {
    console.log(label)
    if (label) {
      commit('loadInbox', inboxEmails
        .filter((email) => email.labels.indexOf(label) !== -1))
    } else {
      commit('loadInbox', inboxEmails)
    }
  },
  getStarred: ({ commit }) => {
    commit('loadStarred', starredEmails)
  },
  getDrafts: ({ commit }, query) => {

  }
}
