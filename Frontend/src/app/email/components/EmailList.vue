<template>
  <v-card v-if="emails.length === 0" class="pa-2 pa-md-4 flex-grow-1 align-center justify-center d-flex flex-column">
    <v-card-text>
      <v-row class="d-flex flex-column" dense align="center" justify="center">
        <v-icon color="blue" size="128">
          fa-solid fa-circle-exclamation
        </v-icon>
        <p>
          {{ $t('email.empty') }}
        </p>
      </v-row>
    </v-card-text>
  </v-card>
  <v-card v-else class="min-w-0">
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
      <v-btn
        v-show="selected.length === 0"
        icon
        :loading="isLoading"
        @click="$emit('refresh', { page: 1, pageSize: pageSize }); page = 1;"
      >
        <v-icon>mdi-refresh</v-icon>
      </v-btn>
      <div v-show="selected.length > 0" >
        <v-btn icon>
          <v-icon>fa-solid fa-envelope</v-icon>
        </v-btn>
        <v-btn icon>
          <v-icon color="red">fa-solid fa-trash-can</v-icon>
        </v-btn>
      </div>
      <v-spacer/>
      <v-pagination
        v-model="page"
        circle
        color="primary"
        :length="pageCount"
        :total-visible="maxPages"
        class="col-md-4 col-sm-12"
        @input="$emit('refresh', { page: page, pageSize: pageSize })"
      ></v-pagination>
    </div>

    <v-card-title >

      <v-row no-gutters dense>
        <v-col cols="12" lg="6" md="0" sm="0">
        </v-col>
        <v-col cols="12" lg="6" md="12" sm="12">
          <v-text-field
            append-icon="mdi-magnify"
            class="flex-grow-1 mr-md-2"
            outlined
            dense
            hide-details
            clearable
            :placeholder="$t('common.search')"
          ></v-text-field>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider/>
    <v-list
      v-for="group in $enumerable(emails).GroupBy(x => new Date(x.SentOn ? x.SentOn : x.CreatedOn).toString('yyyy/MM/dd'), x => x).ToArray()"
      :key="group"
      class="px-2"
    >
      <perfect-scrollbar>
        <v-subheader>
          <v-list-item-action class="d-flex flex-row align-center">
            <v-list-item-action class="d-flex flex-row align-center">
              <v-icon>fa-regular fa-calendar-day</v-icon>
            </v-list-item-action>
            <v-list-item-action-text>
              <span class="text--primary text-h6">
                {{ new Date(group.key) | formatDate('dddd, DD MMMM yyyy') | uppercase }}
              </span>
            </v-list-item-action-text>
          </v-list-item-action></v-subheader>
        <v-divider/>
        <template v-for="(item, index) in group.values()">
          <v-list-item
            :key="item.ID"
            :class="{
              'grey lighten-5': item.read && !$vuetify.theme.dark,
              'v-list-item--active primary--text': selected.indexOf(item.ID) !== -1
            }"
            link
            :to="`/mailbox/mail/${item.ID}`"
          >
            <v-list-item-action class="d-flex flex-row align-center">
              <v-checkbox v-model="selected" :value="item.ID"></v-checkbox>

              <v-btn v-if="type !== 'draft'" icon class="ml-1" @click.prevent="toggleStarred(item.ID)">
                <v-icon v-if="!isStarred(item.ID)" color="grey lighten-1">
                  fa-regular fa-star
                </v-icon>
                <v-icon v-else color="yellow darken-2">
                  fa-solid fa-star
                </v-icon>
              </v-btn>
              <v-icon v-if="type === 'mail'" small :color="getMailIcon(item).color" class="px-2">
                {{ getMailIcon(item).icon }}
              </v-icon>
              <v-icon v-if="type === 'draft'" small class="px-2">fa-solid fa-pencil</v-icon>
            </v-list-item-action>
            <v-list-item-avatar v-if="item.To" class="d-flex flex-row">
              <v-img :src="avatar(item.To.ID)" lazy-src="/images/avatars/generic.jpg"/>
            </v-list-item-avatar>
            <v-list-item-content class="pa-2" @click="$router.push(`/mailbox/${type}/${item.id}`)">
              <v-list-item-title>{{ getTitle(item) }}</v-list-item-title>
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
              <v-list-item-subtitle v-if="item.Attachments.length !== 0">
                <v-list-item-title>
                  <v-icon small>fa-solid fa-paperclip</v-icon>
                  {{ `${item.Attachments.length} ${$t('email.attachments')}: ` }}
                  <v-chip
                    v-for="file in $enumerable(item.Attachments).Take(10).ToArray()"
                    :key="file"
                    small
                    label
                    class="font-italic"
                  >
                    <v-icon small :color="getFileIcon(file).color">{{ getFileIcon(file).icon }}</v-icon>
                    {{ file.Name.substr(0,5) + '...'+ file.Name.substr(-5) }} <b>({{ file.FileSize | formatByte }})</b>
                  </v-chip>
                  <v-chip v-if="item.Attachments.length > 10" small>...</v-chip>
                </v-list-item-title>
              </v-list-item-subtitle>
              <v-list-item-subtitle v-if="item.HashTags.length !== 0">
                <v-list-item-title>
                  <v-chip
                    v-for="tag in $enumerable(item.HashTags).Take(5).ToArray()"
                    :key="tag"
                    class="font-italic"
                    :dark="getUniqueColor(tag).dark"
                    :light="getUniqueColor(tag).light"
                    :color="getUniqueColor(tag).color"
                  >
                    {{ tag }}
                  </v-chip>
                  <v-chip v-if="item.HashTags.length > 5">...</v-chip>
                </v-list-item-title>
              </v-list-item-subtitle>
            </v-list-item-content>

            <v-list-item-action>
              <v-list-item-action-text>
                <v-tooltip bottom>
                  <template v-slot:activator="{ on, attrs }">
                    <span v-bind="attrs" class="text--primary text-lg-body-2" v-on="on">
                      {{ type === 'mail' ? item.SentOn : item.CreatedOn | formatTimeAgo }}
                      <v-icon color="primary" x-small>fa-regular fa-clock</v-icon>
                    </span>
                  </template>
                  <span >{{ type === 'mail' ? item.SentOn : item.CreatedOn | formatDate('HH:mm | DD MMMM YYYY') }}</span>
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
      </perfect-scrollbar>
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
import { getStarred, addStarred, deleteStarred } from '@/api/mails'
import seedColor from 'seed-color'
import isDarkColor from 'is-dark-color'
import { getMimeIcon } from '@/plugins/mimeToIcon'

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
      default: 7
    },
    pageCount:{
      type: Number,
      default: 1
    },
    type: {
      type: String,
      default: 'mail'
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
    this.getStarred(false)
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
      search: null,
      starred: []
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
  async created() {
    this.$mailHub.on('refresh_account', (x) => {
      this.getStarred()
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
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    async getStarred() {
      try {
        const result = await getStarred(false)
        this.starred = result.data
      } catch (err) {
        console.log(err)
      }
    },
    async toggleStarred(id) {
      try {
        let res = null
        if (this.isStarred(id)) {
          res = await deleteStarred(id)
        } else {
          res = await addStarred([id])
        }
        this.starred = res.data
      } catch (err) {
        console.log(err)
      }
    },
    isStarred(id) {
      return this.$enumerable(this.starred)
        .Any(x => x === id)
    },
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
        this.selected = this.emails.map((i) => i.ID)
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
      if (!item) return ''
      if (this.type === 'draft') return ''
      const user = this.getUserInfo()
      if (item.From.ID === user.ID) {
        if (item.To) return `${item.To.Name}`
        else return this.$t('email.externalMail')
      }
      if (item.To.ID === user.ID) return `${item.From.Name}`
      return ''
    },
    avatar(val) {
      return `${this.$apiHost}/api/v1/account/${val}/avatar?token=${this.getToken()}`
    },
    getUniqueColor(val) {
      return {
        color: seedColor(val).toHex(),
        dark: isDarkColor(seedColor(val).toHex()) && !this.$vuetify.theme.dark,
        light: !isDarkColor(seedColor(val).toHex()) && this.$vuetify.theme.dark
      }
    },
    getMailIcon(item) {
      const user = this.getUserInfo()
      const internal = item.Type === 'Internal' ? 'fa-regular' : 'fa-solid'
      if (!item.To) return { icon: `${internal} fa-inbox-out`, color: 'blue' }
      if (item.To.ID === user.ID) return { icon: `${internal} fa-inbox-in`, color: 'green' }
      else return { icon: `${internal} fa-inbox-out`, color: 'blue' }
    },
    getFileIcon(file) {
      const icon =  getMimeIcon(file.Extension)
      console.log(file, icon, 'icon')
      return icon
    }
  }
}
</script>

<style lang="scss" scoped>
.email-app-top {
  height: 82px;
}
</style>
