<template>
  <email-list
    :emails="drafts"
    :is-loading="loading"
    :page-count="pageCount"
    type="draft"
    @refresh="getDrafts"
  />
</template>

<script>
import EmailList from '../components/EmailList'
import { mapActions, mapGetters } from 'vuex'
import { getAllDrafts } from '@/api/drafts'
import { Html5Entities } from 'html-entities'

export default {
  components: {
    EmailList
  },
  data() {
    return {
      drafts:[],
      loading: false,
      pageCount: 1
    }
  },
  async created() {
    this.$mailHub.on('refresh_draft', (x) => {
      console.log('refresh_draft')
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
    this.getDrafts({ page: 1, pageSize: 5 })
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    getDrafts(pagination) {
      this.loading = true
      getAllDrafts({ page: pagination.page, limit: pagination.pageSize } , this.getToken())
        .then((res) => {
          this.drafts = res.data.Data.map((x) => {
            x.Body = Html5Entities.decode(x.Body)

            return x
          })
          this.pageCount = res.data.PageCount
        })
        .catch((err) => this.showError(err))
        .finally(() => this.loading = false)
    }
  }
}
</script>
