<template>
  <v-app>
    <!-- Layout component -->
    <component :is="currentLayout" v-if="isRouterLoaded">
      <transition name="fade" mode="out-in">
        <router-view />
      </transition>
    </component>
    <v-snackbar v-model="toast.show" :timeout="toast.timeout" :color="toast.color" bottom>
      {{ toast.message }}
      <v-btn v-if="toast.timeout === 0" color="white" text @click="toast.show = false">{{ $t('common.close') }}</v-btn>
    </v-snackbar>
  </v-app>
</template>

<script>
import { mapState, mapGetters, mapActions } from 'vuex'
import { signIn } from '@/api/auth'

import config from './configs'

// Layouts
import defaultLayout from './layouts/DefaultLayout'
import simpleLayout from './layouts/SimpleLayout'
import landingLayout from './layouts/LandingLayout'
import authLayout from './layouts/AuthLayout'
import errorLayout from './layouts/ErrorLayout'

/*
|---------------------------------------------------------------------
| Main Application Component
|---------------------------------------------------------------------
|
| In charge of choosing the layout according to the router metadata
|
*/
export default {
  components: {
    defaultLayout,
    simpleLayout,
    landingLayout,
    authLayout,
    errorLayout
  },
  data() {
    return {

    }
  },
  computed: {
    ...mapState('app', ['toast']),
    ...mapGetters('app', ['']),
    isRouterLoaded: function() {
      return this.$route.name !== null

    },
    currentLayout: function() {
      const layout = this.$route.meta.layout || 'default'

      return layout + 'Layout'
    }
  },
  created() {
    this.$i18n.locale = 'fr'
    this.$vuetify.lang.current = 'fr'

  },
  mounted() {

  },
  methods: {
    ...mapActions('auth', ['getToken'])
  },
  head: {
    link: [
      // adds config/icons into the html head tag
      ...config.icons.map((href) => ({ rel: 'stylesheet', href }))
    ]
  }
}
</script>

<style scoped>
/**
 * Transition animation between pages
 */
.fade-enter-active,
.fade-leave-active {
  transition-duration: 0.2s;
  transition-property: opacity;
  transition-timing-function: ease;
}

.fade-enter,
.fade-leave-active {
  opacity: 0;
}
</style>
