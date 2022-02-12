import Vue from 'vue'
import Vuex from 'vuex'

// Global vuex
import AppModule from './app'
import authModule from './auth'
import mailModule from '@/app/email/store'
Vue.use(Vuex)

/**
 * Main Vuex Store
 */
const store = new Vuex.Store({
  modules: {
    app: AppModule,
    auth: authModule,
    'email-app': mailModule
  }
})

export default store
