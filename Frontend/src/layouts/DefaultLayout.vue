<template>
  <div
    v-shortkey="['ctrl', '/']"
    class="d-flex flex-grow-1"
    @shortkey="onKeyup"
  >
    <!-- Navigation -->
    <v-navigation-drawer
      v-model="drawer"
      app
      floating
      class="elevation-1"
      :right="$vuetify.rtl"
      :light="menuTheme === 'light'"
      :dark="menuTheme === 'dark'"
    >
      <!-- Navigation menu info -->
      <template v-slot:prepend>
        <v-toolbar class="pa-1" max-height="100" color="transparent" @click="$router.push('/')">
          <v-img
            :src="getLogo()"
            class="mb-1"
            max-height="64"
            max-width="64"
          ></v-img>

          <p class="text-h5  font-weight-bold text-uppercase" style="color: #0096c7">
            {{ product.name }}</p>

        </v-toolbar>
      </template>

      <!-- Navigation menu -->
      <main-menu :menu="navigation.menu" />

    </v-navigation-drawer>

    <!-- Toolbar -->
    <v-app-bar
      app
      :color="isToolbarDetached ? 'surface' : undefined"
      :flat="isToolbarDetached"
      :light="toolbarTheme === 'light'"
      :dark="toolbarTheme === 'dark'"
    >
      <v-card class="flex-grow-1 d-flex" :class="[isToolbarDetached ? 'pa-1 mt-3 mx-1' : 'pa-0 ma-0']" :flat="!isToolbarDetached">
        <div class="d-flex flex-grow-1 align-center">

          <!-- search input mobile -->
          <v-text-field
            v-if="showSearch"
            append-icon="mdi-close"
            placeholder="Search"
            prepend-inner-icon="mdi-magnify"
            hide-details
            solo
            flat
            autofocus
            @click:append="showSearch = false"
          ></v-text-field>

          <div v-else class="d-flex flex-grow-1 align-center">
            <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>

            <div class="pa-1"></div>
            <v-menu
              v-model="menu"
              :close-on-content-click="false"
              :nudge-width="200"
              offset-x
            >
              <template v-slot:activator="{ on, attrs }">
                <v-btn
                  color="primary"
                  icon
                  fab
                  v-bind="attrs"
                  v-on="on"
                >
                  <v-icon>fa-fill-drip</v-icon>
                </v-btn>
              </template>

              <v-card>
                <v-color-picker v-model="color" mode="hexa" :swatches="swatches" show-swatches></v-color-picker>
              </v-card>
            </v-menu>

            <v-btn icon @click="dark = !dark">
              <v-icon>{{ this.getDarkModeIcon() }}</v-icon>
            </v-btn>
            <v-spacer class="d-none d-lg-block"></v-spacer>
            <!-- search input desktop -->
            <!--            <v-text-field-->
            <!--              ref="search"-->
            <!--              class="mx-1 hidden-xs-only"-->
            <!--              :placeholder="$t('menu.search')"-->
            <!--              prepend-inner-icon="mdi-magnify"-->
            <!--              hide-details-->
            <!--              filled-->
            <!--              rounded-->
            <!--              dense-->
            <!--            ></v-text-field>-->

            <v-spacer class="d-block"></v-spacer>
            <v-badge
              :color="(notification.waitingDelivery !== 0) ? 'red' : 'grey'"
              overlap
              bordered
              :content="notification.waitingDelivery"
              offset-x="12"
              offset-y="15"
              class="mr-2"
            >
              <v-btn
                :disabled="notification.waitingDelivery === 0"
                fab
                small
                color="primary black--text"
                @click="popup('/studies/table?type=complete')"
              >

                <v-icon color="white" >
                  mdi-hand
                </v-icon>
              </v-btn>
            </v-badge>

            <v-badge
              :color="(notification.unfinishedStudy !== 0) ? 'red' : 'grey'"
              overlap
              bordered
              :content="notification.unfinishedStudy"
              offset-x="12"
              offset-y="15"
              class="mr-2"
            >
              <v-btn
                :disabled="notification.unfinishedStudy === 0"
                fab
                small
                color="primary black--text"
                @click="popup('/studies/table?type=new')"
              >

                <v-icon color="white" >
                  mdi-bell
                </v-icon>
              </v-btn>
            </v-badge>

            <toolbar-user />
          </div>
        </div>
      </v-card>
    </v-app-bar>

    <v-main>
      <v-container class="fill-height" :fluid="!isContentBoxed">
        <v-layout>
          <slot></slot>
        </v-layout>
      </v-container>

      <v-footer app inset >
        <v-spacer></v-spacer>
        <v-img src="/images/logo/50lab.svg" max-height="20" max-width="35" contain></v-img>
      </v-footer>
    </v-main>
  </div>
</template>

<script>
import { mapActions, mapGetters, mapState } from 'vuex'

// navigation menu configurations
import config from '../configs'

import MainMenu from '../components/navigation/MainMenu'
import ToolbarUser from '../components/toolbar/ToolbarUser'
import ToolbarApps from '../components/toolbar/ToolbarApps'

import ToolbarNotifications from '../components/toolbar/ToolbarNotifications'
import { getOneStudy } from '@/api/study'
import Vuecookie from 'vue-cookies'

export default {
  components: {
    MainMenu,
    ToolbarUser

  },
  data() {
    return {
      theme: 0,
      drawer: null,
      showSearch: false,
      color: '#0096c7',
      dark: false,
      navigation: config.navigation
    }
  },
  watch: {
    dark() {
      this.$vuetify.theme.dark = this.dark
      this.$storage.set('darktheme', this.dark)
    },
    color(val) {
      this.$vuetify.theme.themes.dark.primary = val
      this.$vuetify.theme.themes.light.primary = val
      this.$storage.set('themecolor',val)
    }
  },
  computed: {
    ...mapState('app', ['product', 'isContentBoxed', 'menuTheme', 'toolbarTheme', 'isToolbarDetached', 'notification'])
  },
  mounted() {
    this.loadConfig()

    this.$socket.client.connect()
    this.$socket.client.on('newStudy' , (it) => {
      getOneStudy(it.id , this.getToken()).then( (res) => {

        if (this.getUserInfo().email !== res.data.data.createdBy.email)
        {
          this.showBrowserNotification('Nouveau Examen', `Client: ${res.data.data.client.fullName}\n${res.data.data.examType}`, res.data.data)
        }
      }).catch( () => {

      })
      this.notificationUpdate()
    })
    this.$socket.client.on('deleteStudy' , (it) => {
      this.notificationUpdate()
    })
    this.$socket.client.on('updateStudy' , (it) => {
      if (it.fieldsUpdated !== undefined) {
        if (it.fieldsUpdated.statusStudy === 'complete') {
          // eslint-disable-next-line no-empty
          if (this.getUserInfo().role === 'user') {
            getOneStudy(it.id , this.getToken()).then( (res) => {
              this.showBrowserNotification('Examen Fini', `Client: ${res.data.data.client.fullName}`, res.data.data)
            }).catch( (err) => {
              this.showError(err)
            })
          }
          this.notificationUpdate()
        } else if (it.fieldsUpdated.statusStudy === 'delivered') {
          this.notificationUpdate()
        }
      }
    })
    this.notificationUpdate()
    //setInterval(() => this.notificationUpdate(), 10000)

  },
  beforeDestroy() {
    this.$socket.client.disconnect()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['notificationUpdate']),
    getDarkModeIcon() {
      if (this.$vuetify.theme.dark) {
        return 'fa-sun'
      } else {
        return 'fa-moon'
      }
    },
    loadConfig() {
      if (this.$storage.has('darktheme')) {
        this.$vuetify.theme.dark = this.$storage.get('darktheme')
        this.theme = this.$storage.get('darktheme') ? 1 : 0
      }
      if (this.$storage.has('themecolor')) {
        this.$vuetify.theme.themes.dark.primary = this.$storage.get('themecolor')
        this.$vuetify.theme.themes.light.primary = this.$storage.get('themecolor')
        this.color = this.$storage.get('themecolor')
      }
    },
    showBrowserNotification(title, body, study) {
      const notification = {
        title: title,
        options: {
          body: body
        },
        events: {
          onclick: function () {

          }
        }
      }

      this.$notification.show(notification.title, notification.options, notification.events)
    },
    onKeyup(e) {
      this.$refs.search.focus()
    },
    popup(v) {
      this.$router.push('/studies/table')

      const route = this.$router.resolve({ path: v })

      // let route = this.$router.resolve('/link/to/page'); // This also works.
      if (window.matchMedia('(display-mode: standalone)').matches) {
        // Check if PWA is running on standalone window and only route it inside the window instead of opening browser tab
        this.$router.push(v)
      } else {
        this.$router.push(v)
        //window.open(route.href, '_blank')
      }
    },
    getLogo() {
      if (this.$vuetify.theme.dark) {
        return '/images/logo/espoir_white.svg'
      } else {
        return '/images/logo/espoir.svg'
      }
    }

  }
}
</script>

<style scoped>
.buy-button {
  box-shadow: 1px 1px 18px #ee44aa;
}
</style>
