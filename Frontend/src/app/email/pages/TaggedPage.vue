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
import { getTaggedMails, searchTaggedMails } from '@/api/mails'

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
      this.refresh()
    })
    this.$mailHub.on('sent_mail', (x) => {
      this.refresh()
    })
    try {
      if (this.$mailHub.state === 'Disconnected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  beforeDestroy() {
    this.$mailHub.off('received_mail')
    this.$mailHub.off('sent_mail')
  },
  mounted() {
    this.refresh()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    refresh() {
      if (this.$route.hash) {
        this.getMails({ page: 1, pageSize: 5 })
      }
    },
    getMails(pagination) {
      const tag = this.$route.hash

      this.loading = true
      getTaggedMails(tag, {
        page: pagination.page,
        limit: pagination.pageSize
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