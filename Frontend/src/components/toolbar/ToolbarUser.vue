<template>
  <div>
    <v-menu offset-y left transition="slide-y-transition">
      <template v-slot:activator="{ on }">
        <v-btn icon class="elevation-2" v-on="on">
          <v-badge
            color="success"
            dot
            bordered
            offset-x="10"
            offset-y="10"
          >
            <v-avatar size="40">
              <v-img :src="avatar"></v-img>
            </v-avatar>
          </v-badge>
        </v-btn>
      </template>

      <!-- user menu list -->
      <v-list dense nav>
        <v-list-item
          v-for="(item, index) in menu"
          :key="index"
          :exact="item.exact"
          :disabled="item.disabled"
          link
        >
          <v-list-item-icon>
            <v-icon small :class="{ 'grey--text': item.disabled }">{{ item.icon }}</v-icon>
          </v-list-item-icon>
          <v-list-item-content @click="$router.push(item.link)">
            <v-list-item-title>{{ item.key ? $t(item.key) : item.text }}</v-list-item-title>
          </v-list-item-content>

        </v-list-item>

        <v-divider class="my-1"></v-divider>

        <v-list-item link>
          <v-list-item-icon>
            <v-icon small>mdi-logout-variant</v-icon>
          </v-list-item-icon>
          <v-list-item-content @click="logout()">
            <v-list-item-title>{{ $t('menu.logout') }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-menu>
  </div>
</template>

<script>
import config from '../../configs'
import { mapActions, mapGetters, mapMutations } from 'vuex'
import Vuecookie from 'vue-cookies'
import { signOut } from '../../api/auth'

export default {
  data() {
    return {
      avatar: '/images/avatars/avatar1.svg',
      menu: config.toolbar.user
    }
  },
  mounted() {
    this.getUniqueAvatar()

  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapMutations('auth', ['setIsAuth']),
    logout() {
      signOut(this.getUserInfo()).then((res) => {
        this.setIsAuth(false)
        Vuecookie.remove('T')
        this.$router.push('/auth/signin')
      }).catch(() => {
        this.setIsAuth(false)
        Vuecookie.remove('T')
        this.$router.push('/auth/signin')

      })
    },
    getUniqueAvatar() {
      const userInfo = this.getUserInfo()

      this.avatar = 'https://robohash.org/' + userInfo.email

    }
  }
}
</script>
