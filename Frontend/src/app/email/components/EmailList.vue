<template>
  <v-card class="min-w-0">
    <div class="email-app-top px-2 py-1 d-flex align-center">
      <v-checkbox :value="selectAll" :indeterminate="selectAlmostAll" @click.stop="onSelectAll(selectAll)"></v-checkbox>

      <v-menu offset-y>
        <template v-slot:activator="{ on, attrs }">
          <v-btn icon v-bind="attrs" v-on="on">
            <v-icon>mdi-menu-down</v-icon>
          </v-btn>
        </template>

        <v-list>
          <v-list-item v-for="item in menuSelection" :key="item.key" link @click="onMenuSelection(item.key)">
            <v-list-item-title>{{ item.title }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>

      <v-btn v-show="selected.length === 0" icon :loading="isLoading" @click="$emit('refresh', { page: 1, pageSize: pageSize }); page = 1;">
        <v-icon>mdi-refresh</v-icon>
      </v-btn>

      <div v-show="selected.length > 0">
        <v-btn icon>
          <v-icon>fa-solid fa-envelope</v-icon>
        </v-btn>
        <v-btn icon>
          <v-icon color="red">fa-solid fa-trash-can</v-icon>
        </v-btn>
      </div>

      <v-spacer></v-spacer>
      <v-pagination
        v-model="page"
        :length="pageCount"
        :total-visible="maxPages"
        @input="$emit('refresh', { page: page, pageSize: pageSize })"
      ></v-pagination>
    </div>

    <v-divider></v-divider>
    <v-list
      v-for="group in $enumerable(emails).GroupBy(x => Date(x.CreatedOn).toString('YYYY/MM/DD'), x => x).ToArray()"
      :key="group"
      class="py-0"
    >
      <v-subheader>
        <v-list-item-action class="d-flex flex-row align-center">
          <v-list-item-action class="d-flex flex-row align-center">
            <v-icon>fa-regular fa-calendar-day</v-icon>
          </v-list-item-action>
          <v-list-item-action-text>
            <span class="text--primary text-h6">
              {{ Date(group.key) | formatDate('dddd, DD MMMM YYYY') | uppercase }}
            </span>
          </v-list-item-action-text>
        </v-list-item-action></v-subheader>
      <v-divider/>
      <template v-for="(item, index) in group.values()">
        <v-list-item
          :key="item.Subject"
          :class="{
            'grey lighten-5': item.read && !$vuetify.theme.dark,
            'v-list-item--active primary--text': selected.indexOf(item.ID) !== -1
          }"
          link
        >
          <v-list-item-action class="d-flex flex-row align-center">
            <v-checkbox v-model="selected" :value="item.ID"></v-checkbox>

            <v-btn icon class="ml-1" @click="item.Starred = !item.Starred">
              <v-icon v-if="!item.Starred" color="grey lighten-1">
                fa-regular fa-star
              </v-icon>
              <v-icon v-else color="yellow darken-2">
                fa-solid fa-star
              </v-icon>
            </v-btn>
            <v-icon v-if="type === 'sent'" small color="info">fa-solid fa-inbox-out</v-icon>
            <v-icon v-if="type === 'draft'" small>fa-solid fa-pencil</v-icon>
          </v-list-item-action>
          <v-list-item-avatar v-if="item.To" class="d-flex flex-row">
            <v-img :src="avatar(item.To.ID)" />
          </v-list-item-avatar>
          <v-list-item-content class="pa-2" @click="$router.push(`/mailbox/inbox/${item.id}`)">
            <v-list-item-title v-text="item.Subject"></v-list-item-title>
            <v-list-item-subtitle v-show="type === 'draft'" class="font-weight-bold">
              {{ getTitle(item) }}
            </v-list-item-subtitle>
            <v-list-item-subtitle @click.stop v-html="item.Body.slice(0,500) + '...'"></v-list-item-subtitle>
            <v-list-item-subtitle>
              <v-chip
                v-for="label in item.labels"
                :key="label"
                :color="getLabelColor(label)"
                class="font-weight-bold mt-1 mr-1"
                outlined
                small
              >
                {{ getLabelTitle(label) }}
              </v-chip>
            </v-list-item-subtitle>
            <v-list-item-subtitle v-if="item.Attachements.length !== 0">
              <v-list-item-title>
                <v-icon small>fa-solid fa-paperclip</v-icon>
                {{ `${item.Attachements.length} ${$t('email.attachments')}: ` }}
                <v-chip
                  v-for="file in $enumerable(item.Attachements).Take(3).ToArray()"
                  :key="file"
                  small
                  label
                  class="font-italic"
                >
                  {{ file.Name }} <b>({{ file.FileSize | formatByte }})</b>
                </v-chip>
                <span v-if="item.Attachements.length > 3">...</span>
              </v-list-item-title>
            </v-list-item-subtitle>
          </v-list-item-content>

          <v-list-item-action>
            <v-list-item-action-text>
              <v-tooltip bottom>
                <template v-slot:activator="{ on, attrs }">
                  <span v-bind="attrs" class="text--primary text-lg-body-2" v-on="on">
                    {{ item.CreatedOn | formatTimeAgo }}
                    <v-icon color="primary" x-small>fa-regular fa-clock</v-icon>
                  </span>
                </template>
                <span >{{ item.CreatedOn | formatDate('HH:mm | DD MMMM YYYY') }}</span>
              </v-tooltip>
            </v-list-item-action-text>
          </v-list-item-action>
        </v-list-item>

        <v-divider
          v-if="index + 1 < emails.length"
          :key="index"
        ></v-divider>
      </template>
      <template v-if="!isLoading && emails.length === 0">
        <div class="px-1 py-6 text-center">{{ $t('email.emptyList') }}</div>
      </template>
    </v-list>

    <v-overlay :value="isLoading" absolute>
      <v-progress-circular indeterminate size="32"></v-progress-circular>
    </v-overlay>

  </v-card>
</template>

<script>
/*
|---------------------------------------------------------------------
| Email List Component
|---------------------------------------------------------------------
|
| List to display emails
|
*/
import { searchDrafts } from '@/api/drafts'
import { mapGetters } from 'vuex'

export default {
  props: {
    emails: {
      type: Array,
      default: () => []
    },
    labels: {
      type: Array,
      default: () => []
    },
    isLoading: {
      type: Boolean,
      default: false
    },
    maxPages:{
      type: Number,
      default: 10
    },
    pageCount:{
      type: Number,
      default: 1
    },
    type: {
      type: String,
      default: 'received'
    },
    internal: {
      type: Boolean,
      default: false
    },
    pageSizes: {
      type: Array,
      default: () => [
        10,
        20,
        50,
        100
      ]
    }
  },
  data() {
    return {
      value:null,
      page: 1,
      pageSize: 5,
      selectAll: false,
      selectAlmostAll: false,
      selected: [],
      menuSelection: [{
        title: 'All',
        key: 'all'
      }, {
        title: 'Read',
        key: 'read'
      }, {
        title: 'Unread',
        key: 'unread'
      }, {
        title: 'Starred',
        key: 'starred'
      }],
      search: null
    }
  },
  computed: {
  },
  watch: {

    search (val) {

    },
    selected(val) {
      // check selectAll intermediate state
      this.$nextTick(() => {
        if (this.selectAll) {
          if (val.length === 0) {
            this.selectAll = false
            this.selectAlmostAll = false
          } else {
            this.selectAlmostAll = this.emails.length !== val.length
          }
        }
      })
    }
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    onMenuSelection(key) {
      switch (key) {
      case 'all':
        this.selected = this.emails.map((i) => i.id)
        this.selectAll = true
        this.selectAlmostAll = false
      }
    },
    onSelectAll(selectAll) {
      if (this.selectAll) {
        this.selected = []
      } else {
        this.selected = this.emails.map((i) => i.id)
      }

      this.selectAlmostAll = false
      this.selectAll = !this.selectAll
    },
    getLabelColor(id) {
      const label = this.labels.find((l) => l.id === id)

      return label ? label.color : ''
    },
    getLabelTitle(id) {
      const label = this.labels.find((l) => l.id === id)

      return label ? label.title : ''
    },
    getTitle(item) {
      if (this.type === 'draft') return ''
      const user = this.getUserInfo()
      if (item.From.ID === user.ID) {
        if (item.To) return `-> ${item.To.Name}`
      }
      if (item.To.ID === user.ID) return `${item.From.Name}`
      return ''
    },
    avatar(val) {
      return `${this.$apiHost}/api/v1/account/${val}/avatar`
    }
  }
}
</script>

<style lang="scss" scoped>
.email-app-top {
  height: 82px;
}
</style>
