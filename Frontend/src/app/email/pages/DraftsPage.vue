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
  mounted() {
    this.getDrafts({ page: 1, pageSize: 10 })
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),
    getDrafts(pagination) {
      console.log(pagination)
      this.loading = true
      getAllDrafts(pagination.page, pagination.pageSize, this.getToken())
        .then((res) => {
          this.drafts = res.data.Data
          this.pageCount = res.data.PageCount
        })
        .catch((err) => this.showError(err))
        .finally(() => this.loading = false)
    }
  }
}
</script>
