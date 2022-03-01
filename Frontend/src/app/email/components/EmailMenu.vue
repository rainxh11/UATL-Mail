<template>
  <div>
    <v-btn
      block
      large
      color="primary"
      class="mb-3"
      @click="showCompose = false; showCompose = true;"
    >
      <v-icon class="pa-2">fa-regular fa-plus</v-icon>
      {{ $t('email.compose') }}
    </v-btn>

    <v-list dense nav class="mt-2 pa-0">
      <v-list-item
        v-for="(item, index) in menu"
        :key="index"
        :to="item.link"
        active-class="primary--text"
        link
      >
        <v-list-item-icon>
          <v-icon :color="item.color">{{ item.icon }}</v-icon>
        </v-list-item-icon>

        <v-list-item-content>
          <v-list-item-title>{{ $t(item.label) }}</v-list-item-title>
          <v-list-item-subtitle v-if="item.external"><b>({{ item.external ? $t('email.external') : '' }})</b></v-list-item-subtitle>
        </v-list-item-content>

        <v-list-item-action v-if="getStat(item) > 0">
          <v-list-item-action-text>
            <span class="font-weight-bold text-body-1">
              <b>{{ getStat(item) | formatNumber }}</b>
            </span>
          </v-list-item-action-text>
        </v-list-item-action>

        <v-list-item-action v-if="getUnread(item) > 0">
          <v-badge
            inline
            color="primary"
            class="font-weight-bold"
            :content="getUnread(item) > 99 ? '+99' : getUnread(item)"
          >
          </v-badge>
        </v-list-item-action>
      </v-list-item>
    </v-list>

    <v-list dense nav class="mt-2 pa-0">
      <div class="overline pa-1 mt-2">{{ $t('email.labels') }}</div>

      <v-list-item
        v-for="(item, index) in labels"
        :key="index"
        :to="item.link"
        exact
        active-class="primary--text"
        link
      >
        <v-list-item-icon>
          <v-icon :color="item.color">{{ item.icon }}</v-icon>
        </v-list-item-icon>

        <v-list-item-content>
          <v-list-item-title>{{ $t(item.label) }}</v-list-item-title>
        </v-list-item-content>

        <v-list-item-action v-if="item.count > 0">
          <v-badge
            inline
            :content="item.count"
            :color="item.color"
            class="font-weight-bold"
          >
          </v-badge>
        </v-list-item-action>
      </v-list-item>
    </v-list>

    <email-compose class="elevation-12" :show-compose="showCompose" @close-dialog="showCompose = false" />
  </div>
</template>

<script>
import EmailCompose from './EmailCompose'
import { mapGetters } from 'vuex'
import { getStats } from '@/api/mails'

/*
|---------------------------------------------------------------------
| Email Menu Component
|---------------------------------------------------------------------
|
| Navigation for email boxes
|
*/
export default {
  components: {
    EmailCompose
  },
  data() {
    return {
      showCompose: false,
      menu: [{
        id: 'receivedInternal',
        label: 'email.inbox',
        icon: 'fa-regular fa-inbox-in',
        link: '/mailbox/inbox-received/internal',
        color: 'green',
        external: false
      },{
        id: 'receivedExternal',
        label: 'email.inbox',
        icon: 'fa-solid fa-inbox-in',
        link: '/mailbox/inbox-received/external',
        color: 'green',
        external: true
      },{
        id: 'sentInternal',
        label: 'email.sent',
        icon: 'fa-regular fa-paper-plane-top',
        color: 'blue',
        external: false,
        link: '/mailbox/inbox-sent/internal'
      }, {
        id: 'sentExternal',
        label: 'email.sent',
        icon: 'fa-solid fa-paper-plane-top',
        color: 'blue',
        external: true,
        link: '/mailbox/inbox-sent/external'
      }, {
        id: 'drafts',
        label: 'email.drafts',
        icon: 'mdi-pencil-outline',
        link: '/mailbox/inbox-drafts'
      }, {
        id: 'starred',
        label: 'email.starred',
        icon: 'fa-regular fa-star',
        link: '/mailbox/inbox-starred'
      }],
      labels: [{
        label: 'email.work',
        color: 'primary',
        icon: 'fa-regular fa-tag',
        link: '/mailbox/inbox#work'
      }, {
        label: 'email.invoice',
        color: 'green',
        icon: 'fa-regular fa-tag',
        link: '/mailbox/inbox#invoice'
      }],
      stats: null
    }
  },
  async created() {
    this.$mailHub.on('refresh_stats', (x) => {
      console.log('refresh_stats')
      this.getStats()
    })
    this.$mailHub.on('refresh_mail', (x) => {
      //this.getStats()
    })
    try {
      if (this.$mailHub.state !== 'Connected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  mounted() {
    this.getStats()
  },
  beforeDestroy() {
    this.$mailHub.off('refresh_stats')
    this.$mailHub.off('refresh_mail')
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    getStats() {
      getStats(this.getToken())
        .then(res => this.stats = res.data)
        .catch(err => console.log(err))
    },
    getUnread(item) {
      switch (item.id) {
      default:
        return 0
      case 'receivedInternal':
        return this.stats.InternalReceived.Unread
      case 'receivedExternal':
        return this.stats.ExternalReceived.Unread
      }
    },
    getStat(item) {
      switch (item.id) {
      default:
        return 0
      case 'receivedInternal':
        return this.stats.InternalReceived.Count
      case 'receivedExternal':
        return this.stats.ExternalReceived.Count
      case 'sentInternal':
        return this.stats.InternalSent.Count
      case 'sentExternal':
        return this.stats.ExternalSent.Count
      case 'drafts':
        return this.stats.Drafts.Count
      case 'starred':
        return this.stats.Starred.Count
      }
    }
  }
}
</script>
