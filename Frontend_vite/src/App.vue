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
import DefaultLayout from '@/layouts/DefaultLayout.vue'
import SimpleLayout from '@/layouts/SimpleLayout.vue'
import LandingLayout from '@/layouts/LandingLayout.vue'
import AuthLayout from '@/layouts/AuthLayout.vue'
import ErrorLayout from '@/layouts/ErrorLayout.vue'

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
    DefaultLayout,
    SimpleLayout,
    LandingLayout,
    AuthLayout,
    ErrorLayout
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
    console.log('wchb hada yazebi')
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
