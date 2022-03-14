import Vue from 'vue'
import prettyBytes from 'pretty-bytes'

Vue.filter('formatByte', (value) => {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
})

Vue.filter('highlight', (words, query) => {
  const result = words.replaceAll(query, '<span class=\'highlight\'>' + query + '</span>')
  return result ? result : words
})

export function formatByte(value) {
  if (!value) return prettyBytes(0)

  return prettyBytes(value)
}
