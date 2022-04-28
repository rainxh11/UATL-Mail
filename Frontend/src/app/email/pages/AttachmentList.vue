<template>
  <div class="d-flex flex-column flex-grow-1">
    <div class="d-flex align-center py-3">
      <div>
        <div class="display-1">
          <v-icon>fa-files</v-icon>
          {{ $t('menu.fileExplorer') }}
        </div>
      </div>

    </div>

    <v-card>
      <v-card-title >
        <v-row no-gutters dense>
          <v-col
            cols="12"
            lg="11"
            md="11"
            sm="11"
            xs="11"
          >
            <v-text-field
              v-model.lazy="searchQuery"
              append-icon="fa-regular fa-magnifying-glass"
              class="flex-grow-1 mr-md-2"
              outlined
              focusable
              dense
              hide-details
              clearable
              :placeholder="$t('common.search')"
            ></v-text-field>
          </v-col>
          <v-col cols="1">
            <v-btn
              v-show="selected.length === 0"
              icon
              :loading="loading"
              @click="page = 1; refresh({ page: page, limit: pageSize })"
            >
              <v-icon>fa-regular fa-refresh</v-icon>
            </v-btn>
          </v-col>
        </v-row>
      </v-card-title>
      <v-divider/>
      <v-data-table
        v-model="selected"
        show-select
        :headers="headers"
        :items="files"
        :loading="loading"
        class="flex-grow-1"
        :server-items-length="totalCount"
        :options.sync="options"
        :items-per-page="10"
        sort-by="CreatedOn"
        sort-desc
        :footer-props="{
          itemsPerPageOptions: [5,10,20,50,100],
          showCurrentPage : true,
          showFirstLastPage : true,

        }"
      >
        <template v-slot:item.Name="{ item }">
          <div class="font-weight-bold">
            <v-icon :color="getIcon(item).color">{{ getIcon(item).icon }}</v-icon>
            <a :href="getFileUrl(item)" class="px-1">{{ item.Name }}</a>
          </div>
        </template>

        <template v-slot:item.UploadedBy="{ item }">
          <div class="d-flex align-center py-1">
            <v-avatar size="32" class="elevation-1 grey lighten-3">
              <v-img :src="getAvatar(item.UploadedBy)" />
            </v-avatar>
            <div class="ml-2 font-weight-bold">
              <div class="text-">{{ item.UploadedBy.Name }}</div>
              <copy-label :text="item.UploadedBy.UserName" />
            </div>
          </div>
        </template>

        <template v-slot:item.FileSize="{ item }">
          <v-chip
            label
            class="font-weight-bold"
          >
            {{ item.FileSize | formatByte | capitalize(true) }}
          </v-chip>          
        </template>

        <template v-slot:item.CreatedOn="{ item }">
          <v-chip color="primary">
            <div>{{ item.CreatedOn | formatDate('HH:mm | DD MMMM yyyy') | capitalize(true) }}</div>
          </v-chip>
        </template>

        <template v-slot:item.MD5="{ item }">
          <copy-label :text="item.MD5" />
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script>
import { getAllAttachments, searchAttachments } from '@/api/attachments'
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators'
import CopyLabel from '@/components/common/CopyLabel'
import { getMimeIcon } from '@/plugins/mimeToIcon'

export default {
  components: {
    CopyLabel
  },
  data() {
    return {
      loading: false,
      search: '',
      searchQuery: '',
      selected: [],
      headers: [
        { text: this.$t('email.headers.fileName'), align: 'left', value: 'Name' },
        { text: '', sortable: false, align: 'right', value: 'action' },
        { text: this.$t('email.headers.fileSize'), align: 'left', value: 'FileSize' },
        { text: this.$t('email.headers.uploadedBy'), value: 'UploadedBy' },
        { text: this.$t('email.headers.createdOn'), value: 'CreatedOn' },
        { text: 'MD5', value: 'MD5' }
      ],
      files: [],
      totalCount: 0, 
      pagination: {
        desc: true,
        sort: 'CreatedOn',
        limit: 10,
        page: 1
      },
      options : {}
    }
  },
  watch: {
    searchQuery(val) {
      this.search = val
    },
    options: {
      handler() {
        const { sortBy, sortDesc,  page, itemsPerPage } = this.options
        this.pagination = {
          desc: sortDesc[0],
          sort: sortBy.length > 0 ? sortBy[0] : 'CreateOn',
          page: page,
          limit: itemsPerPage
        }
        console.log(this.pagination, 'pagination')
        this.refresh()
      }
    }
  },
  subscriptions() {
    return {
      searchObservable: this.$watchAsObservable('search')
        .pipe(debounceTime(1000))
        .subscribe(val => {
          if (!val.newValue) this.search = ''
          this.refresh()
        })
    }
  },
  mounted() {
    this.refresh()
  },
  methods: {
    refresh() {
      if (this.search.length > 0) {
        this.searchAttachments(this.search, this.pagination)
      } else {
        this.getAttachments(this.pagination)
      }
    },
    searchAttachments(search, pagination) {
      this.loading = true
      searchAttachments(search, pagination)
        .then(res => {
          this.files = res.data.Data
          this.totalCount = res.data.Total
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    getAttachments(pagination) {
      this.loading = true
      getAllAttachments(pagination)
        .then(res => {
          this.files = res.data.Data
          this.totalCount = res.data.Total
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    getAvatar(user) {
      return `${this.$apiHost}/api/v1/account/${user.ID}/avatar`
    },
    getFileUrl(file) {
      return `${this.$apiHost}/api/v1/attachment/${file.ID}`
    },
    getIcon(file) {
      const icon = getMimeIcon(file.Extension)
      return icon
    },
    downloadFile(file) {
      console.log(file, 'download')
      window.open(this.getFileUrl(file))
    }
  }
}
</script>

<style lang="scss" scoped>
.slide-fade-enter-active {
  transition: all 0.3s ease;
}
.slide-fade-leave-active {
  transition: all 0.3s cubic-bezier(1, 0.5, 0.8, 1);
}
.slide-fade-enter,
.slide-fade-leave-to {
  transform: translateX(10px);
  opacity: 0;
}
</style>
