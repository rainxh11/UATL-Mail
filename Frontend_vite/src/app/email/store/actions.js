import inboxEmails from './content/inbox'
import starredEmails from './content/starred'
import { getStarred } from '@/api/mails'

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
  getStarred: ({ commit }, full) => {
    getStarred(full)
      .then(res => commit('loadStarred', res.data))
      .catch(err => console.log(err))
  },
  getDrafts: ({ commit }, query) => {

  }
}
