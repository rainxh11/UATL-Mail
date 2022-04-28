<template>
  <email-list
    :emails="mails"
    :is-loading="loading"
    :page-count="pageCount"
    type="mail"
    @refresh="getStarred"
    @search="searchStarred"
  />
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import EmailList from '../components/EmailList'
import { getStarredFull, searchStarredFull } from '@/api/mails'
import { Html5Entities } from 'html-entities'

export default {
  components: {
    EmailList
  },
  data() {
    return {
      mails: [],
      loading: false,
      pageCount: 1,
      pageSize: 5
    }
  },
  async created() {
    this.$mailHub.on('refresh_account', (x) => {
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
    this.$mailHub.off('refresh_account')
  },
  mounted() {
    if (this.$storage.has('mailList_pageSize'))
      this.pageSize = parseInt(this.$storage.get('mailList_pageSize'))
    this.refresh()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    refresh() {
      if (this.$route.hash) {
        console.log('')
      } else {
        this.getStarred({ page: 1, pageSize: this.pageSize })
      }
    },
    searchStarred(search, pagination) {
      this.loading = true
      searchStarredFull(search, {
        page: pagination.page,
        limit: pagination.pageSize
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
    getStarred(pagination) {
      this.loading = true
      getStarredFull({
        page: pagination.page,
        limit: pagination.pageSize
      }, this.getToken())
        .then(res => {
          this.mails = res.data.Data.map((x) => {
            x.Body = Html5Entities.decode(x.Body)

            return x
          })
          console.log(res.data, 'data')
          this.pageCount = res.data.PageCount
        }).catch(err => this.showError(err))
        .finally(() => this.loading = false)
    }
  }
}
</script>
