<template>
  <email-list
    :emails="mails"
    :is-loading="loading"
    :page-count="pageCount"
    type="mail"
    @refresh="getMails"
    @search="searchMails"
  />
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import EmailList from '../components/EmailList'
import { getAllMails, searchMails, markMailViewed } from '@/api/mails'
import { Html5Entities } from 'html-entities'

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
    try {
      if (this.$mailHub.state === 'Disconnected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  beforeDestroy() {
    this.$mailHub.off('received_mail')
  },
  mounted() {
    this.refresh()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    refresh() {
      if (this.$route.hash) {
        console.log('')
      } else {
        this.getMails({ page: 1, pageSize: 5 })
      }
    },
    markViewed(mail) {
      if (!mail.Viewed) {
        markMailViewed(mail.ID)
          .catch(err => console.log(err))
      }
    },
    searchMails(search, pagination) {
      const { direction, internal } = this.$route.params
      search = !search ? '' : search
      this.loading = true
      searchMails(search,{
        page: pagination.page,
        limit: pagination.pageSize,
        direction: direction,
        type: internal
      }, this.getToken())
        .then(res => {
          this.mails = res.data.Data.map((x) => {
            x.Body = Html5Entities.decode(x.Body)

            return x
          })
          this.pageCount = res.data.PageCount
        }).catch(err => this.showError(err))
        .finally(() => this.loading = false)
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
          this.mails = res.data.Data.map((x) => {
            x.Body = Html5Entities.decode(x.Body)

            return x
          })
          this.pageCount = res.data.PageCount
        }).catch(err => this.showError(err))
        .finally(() => this.loading = false)
    }
  }
}
</script>
