/*
|---------------------------------------------------------------------
| Email Vuex Mutations
|---------------------------------------------------------------------
*/
export default {
  loadInbox: (state, emails) => {
    state.inbox = emails
  },
  loadStarred: (state, starred) => {
    state.starred = starred
  },
  loadDrafts: (state, emails) => {
    state.drafts = emails
  }
}
