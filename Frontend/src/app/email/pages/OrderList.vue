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

      <v-data-table
        v-model="selected"
        show-select
        :headers="headers"
        :items="users"
        :search="search"
        :loading="loading"
        class="flex-grow-1"
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
          <v-icon v-if="item.Enabled" color="success">
            fa-solid fa-circle-check
          </v-icon>
          <v-icon v-else color="error">
            fa-solid fa-circle-xmark
          </v-icon>
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
      selected: [],
      headers: [
        { text: 'Name', align: 'left', value: 'Name' },
        { text: 'Description', align: 'left', value: 'Description' },
        { text: 'Role', value: 'Role' },
        { text: 'CreatedOn', value: 'CreatedOn' },
        { text: 'LastLogin', value: 'LastLogin' },
        { text: 'Enabled', value: 'Enabled' },
        { text: '', sortable: false, align: 'right', value: 'action' },
        { text: '', sortable: false, align: 'right', value: 'delete' }
      ],
      users: [],
      dialog: false,
      edit: false,
      user: null
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
      this.getUsers({ page: 1, limit: 10 })
    },
    getAvatar(user) {
      return `${this.$apiHost}/api/v1/account/${user.ID}/avatar`
    },
    getUsers(params) {
      this.loading = true
      getAllUsers(params)
        .then(res => {
          console.log(res.data.Data)
          this.users = res.data.Data
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
