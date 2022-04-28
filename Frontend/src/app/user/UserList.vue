<template>
  <div class="d-flex flex-column flex-grow-1">
    <div class="d-flex align-center py-3">
      <div>
        <div class="display-1">
          <v-icon>fa-users-gear</v-icon>
          {{ $t('users.users') }}
        </div>
        <v-breadcrumbs :items="breadcrumbs" class="pa-0 py-2"></v-breadcrumbs>
      </div>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="edit = undefined; dialog = true"> 
        {{ $t('users.createUser') }}
        <v-icon small class="px-1">fa-user-plus</v-icon>
      </v-btn>
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
        :items="users"
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
          <div class="d-flex align-center py-1">
            <v-avatar size="32" class="elevation-1 grey lighten-3">
              <v-img :src="getAvatar(item)" />
            </v-avatar>
            <div class="ml-2 font-weight-bold">
              <div class="text-">{{ item.Name }}</div>
              <copy-label :text="item.UserName" />
            </div>
          </div>
        </template>

        <template v-slot:item.Enabled="{ item }">
          <v-chip
            small
            dark
            class="font-weight-bold"
            :color="item.Enabled ? 'success' : 'error'"
          >
            <v-icon v-if="item.Enabled" small class="px-1">
              fa-regular fa-check
            </v-icon>
            <v-icon v-else small class="px-1">
              fa-regular fa-xmark
            </v-icon>
            {{ item.Enabled ? $t('common.enabled') : $t('common.disabled') }}
          </v-chip>
          
        </template>

        <template v-slot:item.Role="{ item }">
          <v-chip
            label
            small
            dark
            class="font-weight-bold"
            :color="getRoleColor(item.Role)"
          >
            {{ $t(`roles.${item.Role}`) }}
          </v-chip>
        </template>

        <template v-slot:item.CreatedOn="{ item }">
          <div>{{ item.CreatedOn | formatDate('HH:mm | DD MMMM yyyy') }}</div>
        </template>

        <template v-slot:item.LastLogin="{ item }">
          <div v-if="new Date(item.LastLogin).year() > 1">{{ item.LastLogin | formatDate('HH:mm | DD MMMM yyyy') }}</div>
        </template>

        <template v-slot:item.action="{ item }">
          <div class="actions">
            <v-btn icon @click="user = item; edit = true; dialog = true">
              <v-icon>fa-pencil</v-icon>
            </v-btn>
          </div>
        </template>
        <template v-slot:item.delete="{ item }">
          <div class="actions">
            <v-btn icon @click="deleteConfirmation(item)">
              <v-icon>fa-trash</v-icon>
            </v-btn>
          </div>
        </template>
      </v-data-table>
    </v-card>
    <user-dialog v-model="user" :show="dialog" :edit="edit" @close-dialog="closeDialog"/>
    <confirmation-dialog ref="confirmdialog" title-color="red" icon-color="red lighten-4"></confirmation-dialog>
  </div>
</template>

<script>
import CopyLabel from '../../components/common/CopyLabel'
import { getAllUsers, deleteUser, searchUsers } from '@/api/users'
import UserDialog from './UserDialog.vue'
import ConfirmationDialog from '@/components/common/ConfirmationDialog'
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators'

export default {
  components: {
    CopyLabel,
    UserDialog,
    ConfirmationDialog
  },
  data() {
    return {
      loading: false,
      breadcrumbs: [{
        text: 'Users',
        disabled: false,
        href: '#'
      }, {
        text: 'List'
      }],
      search: '',
      searchQuery: '',
      selected: [],
      headers: [
        { text: this.$t('email.headers.name'), align: 'left', value: 'Name' },
        { text: this.$t('email.headers.description'), align: 'left', value: 'Description' },
        { text: this.$t('email.headers.role'), value: 'Role' },
        { text: this.$t('email.headers.createdOn'), value: 'CreatedOn' },
        { text: this.$t('email.headers.lastLogin'), value: 'LastLogin' },
        { text: this.$t('email.headers.enabled'), value: 'Enabled' },
        { text: '', sortable: false, align: 'right', value: 'action' },
        { text: '', sortable: false, align: 'right', value: 'delete' }
      ],
      users: [],
      dialog: false,
      edit: false,
      user: null,
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
    getRoleColor(role) {
      switch (role) {
      default:
        return ''
      case 'User': 
        return 'green'
      case 'Admin':
        return 'blue'
      case 'OrderOffice':
        return 'orange'
      }
    },
    closeDialog(refresh = false) {
      this.dialog = false
      this.edit = false
      if (refresh) this.refresh()
    },
    refresh() {
      if (this.search.length > 0) {
        this.searchUsers(this.search, this.pagination)
      } else {
        this.getUsers(this.pagination)
      }
    },
    getAvatar(user) {
      return `${this.$apiHost}/api/v1/account/${user.ID}/avatar`
    },
    searchUsers(search, params) {
      this.loading = true
      searchUsers(search, params)
        .then(res => {
          this.users = res.data.Data
          this.totalCount = res.data.Total
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    getUsers(params) {
      this.loading = true
      getAllUsers(params)
        .then(res => {
          this.users = res.data.Data
          this.totalCount = res.data.Total
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    deleteUser(id) {
      deleteUser(id)
        .then(() => {
          this.refresh()
          this.showSuccess()
        })
        .catch(err => this.showError(err))
    },
    async deleteConfirmation(user) {
      if (await this.$refs.confirmdialog.open(
        this.$t('confirmation.delete').replace('#', user.Name),  this.$t('confirmation.title'))
      ) {
        this.deleteUser(user.ID)
      }
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
