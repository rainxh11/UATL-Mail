import Vue from 'vue'
import prettyBytes from 'pretty-bytes'

Vue.filter('formatByte', (value) => {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
})

Vue.filter('highlight', (words, query) => {
  const regex = new RegExp(query, 'ig')
  const result = words.replaceAll(regex, '<span class=\'highlight\'>' + query + '</span>')
  return result ? result : words
})

export function formatByte(value) {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
}
