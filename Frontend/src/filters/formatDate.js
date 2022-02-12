import Vue from 'vue'
import moment from 'moment-timezone'
import store from '../store'
import { formatPrice } from '@/filters/formatCurrency'

Vue.filter('formatDate', (value, filterFormat) => {
  const { zone, format } = store.state.app.time

  if (value) {
    moment.locale('fr')

    return moment(value).tz(zone).format(filterFormat || format || 'lll')
  }

  return ''
})

export function formatDate(value, filterFormat) {
  const { zone, format } = store.state.app.time

  if (value) {
    moment.locale('fr')

    return moment(value).tz(zone).format(filterFormat || format || 'lll')
  }

  return ''
}
