import Vue from 'vue'
import moment from 'moment-timezone'
import store from '../store'
import { formatPrice } from '@/filters/formatCurrency'
import prettyBytes from 'pretty-bytes'
import TimeAgo from 'javascript-time-ago'
import enTimeAgo from 'javascript-time-ago/locale/en.json'
import arTimeAgo from 'javascript-time-ago/locale/ar.json'
import frTimeAgo from 'javascript-time-ago/locale/fr.json'
import i18n from '@/plugins/vue-i18n'
TimeAgo.addLocale(arTimeAgo)
TimeAgo.addLocale(frTimeAgo)
TimeAgo.addLocale(enTimeAgo)
TimeAgo.setDefaultLocale(frTimeAgo)

Vue.filter('formatDate', (value, filterFormat) => {
  const { zone, format } = store.state.app.time

  if (value) {
    moment.locale('fr')

    return moment(value).tz(zone).format(filterFormat || format || 'lll')
  }

  return ''
})

Vue.filter('formatTimeAgo', (value) => {
  const { format } = i18n.locales.find((x) => x.code === i18n.locale)
  const timeAgo = new TimeAgo(format)

  return timeAgo.format(new Date(value))
})

export function formatDate(value, filterFormat) {
  const { zone, format } = store.state.app.time

  if (value) {
    moment.locale('fr')

    return moment(value).tz(zone).format(filterFormat || format || 'lll')
  }

  return ''
}
