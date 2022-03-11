import VueCompositionAPI from '@vue/composition-api'
import Vue from 'vue'
import VueRx from 'vue-rx'
import App from './App.vue'
import VueIframe from 'vue-iframes'
import { AsEnumerable, Range } from 'linq-es5'
Array.prototype.AsEnumerable = function () {
  return AsEnumerable(this)
} 
import PerfectScrollbar from 'vue2-perfect-scrollbar'
import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css'

Vue.use(PerfectScrollbar)

Vue.use(VueCompositionAPI)
// register vue-auth-image directive

// VUEX - https://vuex.vuejs.org/
import store from './store'
import { Plugin } from 'vue2-storage'

// VUE-ROUTER - https://router.vuejs.org/
import router from './router'
import { mailHub } from '@/plugins/sockets'
// PLUGINS
import './icons/css/all.css'
import vuetify from './plugins/vuetify'
import i18n from './plugins/vue-i18n'
import './plugins/vue-shortkey'
import './plugins/vue-head'
import './plugins/animate'
import './plugins/clipboard'
import './plugins/moment'

// FILTERS
import './filters/capitalize'
import './filters/lowercase'
import './filters/uppercase'
import './filters/formatCurrency'
import './filters/formatDate'
import './filters/formatMisc'

// STYLES
// Main Theme SCSS
import './assets/scss/theme.scss'

// Animation library - https://animate.style/
import 'animate.css/animate.min.css'

// V-Viewer
import VueViewer from 'v-viewer'
import 'viewerjs/dist/viewer.css'

// vue editor
import Vue2Editor from 'vue2-editor'

Vue.use(Vue2Editor)

// Vue Cookies

import VueCookies from 'vue-cookies'
Vue.use(VueCookies)

// Enumerable
Vue.prototype.$enumerable = AsEnumerable
Vue.prototype.$range = Range

// V-mask As a plugin
import VueMask from 'v-mask'
import { VueMaskFilter } from 'v-mask'
Vue.use(VueMask)

// mask only as a filter
Vue.filter('VMask', VueMaskFilter)
const prod = process.env.NODE_ENV === 'production'
let apiHost = process.env.VUE_APP_API || '127.0.0.1:5000'
if (prod) apiHost = ''

console.log(apiHost, 'apiHost', prod, 'prod')

Vue.prototype.$apiHost = apiHost
// Connect Websockets
Vue.prototype.$mailHub = mailHub(apiHost)

// Set this to false to prevent the production tip on Vue startup.
Vue.config.productionTip = false

/*
|---------------------------------------------------------------------
| Main Vue Instance
|---------------------------------------------------------------------
|
| Render the vue application on the <div id="app"></div> in index.html
|
| https://vuejs.org/v2/guide/instance.html
|
*/
const shouldSW = 'serviceWorker' in navigator && prod
const shouldSWDev = 'serviceWorker' in navigator && !prod

if (shouldSW) {
  navigator.serviceWorker.register('/service-worker.js').then(() => {

  })
} else if (shouldSWDev) {
  navigator.serviceWorker.register('/service-workder-dev.js').then(() => {

  })
}

import VueNativeNotification from 'vue-native-notification'
import datejs from 'datejs'
Vue.use(datejs)
Vue.use(VueNativeNotification, {
  // Automatic permission request before
  // showing notification (default: true)
  requestOnNotify: true
})
Vue.use(VueRx)
Vue.use(VueIframe)
Vue.use(VueViewer, {
  defaultOptions: {
    zIndex: 9999
  }
})

// Local Browser Storage init
Vue.use(Plugin, {
  prefix: 'app_',
  driver: 'local',
  ttl: 0,
  replacer: (key, value) => value
})

export default new Vue({
  icons: {
    iconfont: 'fa'
  },
  i18n,
  vuetify,
  router,
  store,

  render: (h) => h(App)
}).$mount('#app')
