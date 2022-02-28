<template>
  <email-list
    :emails="mails"
    :is-loading="loading"
    :page-count="pageCount"
    type="draft"
    @refresh="getMails"
  />
</template>

<script>
import { mapState, mapActions } from 'vuex'
import EmailList from '../components/EmailList'

export default {
  components: {
    EmailList
  },
  data() {
    return {
      mails: [],
      loading: false,
      pageCount: 1
    }
  },
  watch: {
    '$route.hash'(val) {
      this.getEmails()
    }
  },
  mounted() {
    this.getEmails()
  },
  methods: {
    ...mapActions('email-app', ['getInbox']),
    onRefresh(val) {
      console.log(val)

      this.getEmails()
    },
    getEmails() {
      const { hash } = this.$route
      const label = hash ? hash.replace('#', '') : ''

      this.getInbox(label)
    }
  }
}
</script>
