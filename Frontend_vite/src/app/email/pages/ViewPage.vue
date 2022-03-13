<template>
  <v-card class="min-w-0">

    <v-divider></v-divider>

    <div class="d-flex pa-2 align-center">
      <v-avatar>
        <v-img :src="getAvatar(current.user)"/>
      </v-avatar>
      <div>
        <div class="px-1 text-h6"><b>{{ mail.Subject }}</b></div>
        <div v-if="current.sent" class="px-1 text-h6">{{ `${$t('email.to')} ` }}<b>{{ `${current.sender}` }}</b></div>
        <div v-else class="px-1 text-lg-body1">{{ `${$t('email.from')} ` }}<b>{{ `${current.sender}` }}</b></div>
      </div>

      <v-spacer></v-spacer>

      <div>
        <v-btn icon>
          <v-icon>mdi-printer</v-icon>
        </v-btn>
      </div>
    </div>
    <body-content
      class="px-4"
      :body="mail.Body"
      :files="mail.Attachments"
      :hash-tags="mail.HashTags"
      files-height="500"
    />

    <div class="pa-2 align-center">
      <v-expansion-panels v-model="emailsExpanded" multiple>
        <v-expansion-panel v-for="item in replies" :key="item.ID"> 
          <v-expansion-panel-header>
            <template v-slot:default="{ open }">

              <div class="d-flex">
                <v-avatar size="36px">
                  <v-img :src="getAvatar(item.From.ID)" placeholder="/images/avatars/avatar1.svg"/>
                </v-avatar>

                <div class="mx-3 min-w-0">
                  <div class="font-weight-bold mb-1">{{ item.name }}</div>

                  <div v-show="!open">
                    <v-menu offset-y right transition="slide-y-transition">
                      <template v-slot:activator="{ on }">
                        <v-btn text class="pa-0" v-on="on">
                          to me
                          <v-icon small right>mdi-chevron-down</v-icon>
                        </v-btn>
                      </template>
                      <v-card class="pa-2">
                        <div><span class="grey--text">from:</span> johnnilson@whatthisisnotarealemail.com</div>
                        <div><span class="grey--text">to:</span> clara@whatthisisnotarealemail.com</div>
                      </v-card>
                    </v-menu>
                  </div>
                  <div v-show="!open" class="text-truncate" v-html="item.Subject">
                  </div>
                </div>

                <v-spacer></v-spacer>

                <v-menu offset-y left transition="slide-y-transition">
                  <template v-slot:activator="{ on }">
                    <v-btn icon v-on="on">
                      <v-icon small>mdi-dots-vertical</v-icon>
                    </v-btn>
                  </template>
                  <v-list dense nav>
                    <v-list-item link>
                      <v-list-item-title>{{ $t('email.forward') }}</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </div>
            </template>
            <template v-slot:actions>
              <span></span>
            </template>
          </v-expansion-panel-header>
          <v-expansion-panel-content>
            <body-content
              class="px-4"
              :body="item.Body"
              :files="item.Attachments"
              :hash-tags="item.HashTags"
            />          
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>

      <v-card class="mt-4">
        <email-editor :reply-to="{ mail: $route.params.id, user: mail.From.ID }"></email-editor>
      </v-card>
    </div>

  </v-card>
</template>

<script>
import EmailEditor from '../components/EmailEditor.vue'
import BodyContent from '../components/BodyContent.vue'
import { getMailWithReplies } from '@/api/mails'
import { mapGetters } from 'vuex'
import { Html5Entities } from 'html-entities'

export default {
  components: {
    EmailEditor,
    BodyContent
  },
  async setup() {
    
    await this.refresh()
  },
  data() {
    if (this.$route.params.id === 'undefined') this.$router.push('/')
    return {
      mail: null,
      emailsExpanded: [3],
      replies: []
    }
  },
  computed: {
    current() {
      const user = this.getUserInfo()
      if (!this.mail) return { user: null, sent: null }
      if (this.mail.To.ID === user.ID) {
        return {
          user: this.mail.From.ID,
          sent: false,
          sender: this.mail.From.Name
        }
      } else {
        return {
          user: this.mail.To.ID,
          sent: true,
          sender: this.mail.To.Name
        }
      }
    }
  },
  async created() {
    await this.refresh()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    async refresh() {
      await this.getMail(this.$route.params.id)
    },
    async getMail(id) {
      try {
        const res = await getMailWithReplies(id)
        this.mail = res.data.Mail
        this.mail.Body = Html5Entities.decode(this.mail.Body)

        this.replies = res.data.Replies.map((x) => {
          x.Body = Html5Entities.decode(x.Body)

          return x
        })
      } catch (err) {
        console.log(err)
      }   
    },
    getAvatar(id) {
      return `${this.$apiHost}/api/v1/account/${id}/avatar?token=${this.getToken()}`
    }
  }
}
</script>

<style lang="scss" scoped>
.email-app-top {
  height: 82px;
}
</style>
