import Vue from 'vue'
import VueRx from 'vue-rx'
import App from './App.vue'
import { firstTimeAdmin } from './api/auth'
import VueIframe from 'vue-iframes'

// VUEX - https://vuex.vuejs.org/
import store from './store'
import Tinybox from 'vue-tinybox'
import { Plugin } from 'vue2-storage'

// VUE-ROUTER - https://router.vuejs.org/
import router from './router'

// PLUGINS
import './icons/css/all.css'
import vuetify from './plugins/vuetify'
import i18n from './plugins/vue-i18n'
import './plugins/vue-google-maps'
import './plugins/vue-shortkey'
import './plugins/vue-head'
import './plugins/vue-gtag'
import './plugins/apexcharts'
import './plugins/echarts'
import './plugins/animate'
import './plugins/clipboard'
import './plugins/moment'

// FILTERS
import './filters/capitalize'
import './filters/lowercase'
import './filters/uppercase'
import './filters/formatCurrency'
import './filters/formatDate'

// STYLES
// Main Theme SCSS
import './assets/scss/theme.scss'

// Animation library - https://animate.style/
import 'animate.css/animate.min.css'

// vue editor
import Vue2Editor from 'vue2-editor'

Vue.use(Vue2Editor)

// Vue Cookies

import VueCookies from 'vue-cookies'
Vue.use(VueCookies)

// V-mask As a plugin
import VueMask from 'v-mask'
import { VueMaskFilter } from 'v-mask'
Vue.use(VueMask)

// mask only as a filter
Vue.filter('VMask', VueMaskFilter)

// socket IO
import VueSocketIOExt from 'vue-socket.io-extended'
import { io } from 'socket.io-client'

const risReportServer = process.env.VUE_APP_REPORT_SERVER || 'http://127.0.0.1:5000/api/risreport/'
const documentServer = process.env.VUE_APP_DOCUMENT_SERVER || 'http://127.0.0.1:5000/api/documenteditor/'
const documentServerPrintDicom = process.env.VUE_APP_DOCUMENT_SERVER_PRINTDICOM || 'http://127.0.0.1:5000/api/documenteditor/printdicom/'
const dicomServer = process.env.VUE_APP_DICOM_SERVER || 'http://127.0.0.1:9100'
const orthancViewer = process.env.VUE_APP_ORTHANC_VIEWER || 'http://127.0.0.1:81/'
const dicomWebSocket = process.env.VUE_APP_DICOM_SERVER_WEBSOCKET || 'ws://127.0.0.1:9101/dicomserverwebsocket/studies/'

Vue.prototype.$risReportServer = risReportServer
Vue.prototype.$documentServer =  documentServer
Vue.prototype.$documentServerPrintDicom = documentServerPrintDicom
Vue.prototype.$dicomServer = dicomServer
Vue.prototype.$orthancViewer =  orthancViewer
Vue.prototype.$dicomWebSocket = dicomWebSocket

const apiHost = process.env.VUE_APP_API || '127.0.0.1:8000'
const socket = io(apiHost)

Vue.use(VueSocketIOExt, socket)

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
const prod = process.env.NODE_ENV === 'production'
const shouldSW = 'serviceWorker' in navigator && prod
const shouldSWDev = 'serviceWorker' in navigator && !prod

if (shouldSW) {
  navigator.serviceWorker.register('/service-worker.js').then(() => {

  })
} else if (shouldSWDev) {
  navigator.serviceWorker.register('/service-workder-dev.js').then(() => {

  })
}

firstTimeAdmin().then((res) => {

}).catch(() => {

})

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
Vue.use(Tinybox)

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
