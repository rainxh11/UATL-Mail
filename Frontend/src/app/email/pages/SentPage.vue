<template>
  <email-list
    :emails="mails"
    :is-loading="loading"
    :page-count="pageCount"
    type="mail"
    @refresh="getMails"
  />
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import EmailList from '../components/EmailList'
import { getAllMails, searchMails } from '@/api/mails'

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
  async created() {
    this.$mailHub.on('received_mail', (x) => {
      console.log('received_mail')
      this.getDrafts({ page: 1, pageSize: 5 })
    })
    try {
      if (this.$mailHub.state === 'Disconnected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  beforeDestroy() {
    this.$mailHub.off('refresh_draft')
  },
  mounted() {
    this.refresh()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    refresh() {
      this.getMails({ page: 1, pageSize: 5 })
    },
    getMails(pagination) {
      const { direction, internal } = this.$route.params

      this.loading = true
      getAllMails({
        page: pagination.page,
        limit: pagination.pageSize,
        direction: direction,
        type: internal
      }, this.getToken())
        .then(res => {
          this.mails = res.data.Data
          this.pageCount = res.data.PageCount
        }).catch(err => this.showError(err))
        .finally(() => this.loading = false)
    }
  }
}
</script>
