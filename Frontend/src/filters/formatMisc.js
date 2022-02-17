import Vue from 'vue'
import prettyBytes from 'pretty-bytes'

Vue.filter('formatByte', (value) => {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
})

export function formatByte(value) {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
}
