import Vue from 'vue'

Vue.filter('capitalizeFirstLetter', (value) => {
  if (!value) return ''

  let wordList = value.split(' ')

  wordList = wordList.map( (word) => { return word.charAt(0).toUpperCase() + word.substring(1) })

  return wordList.join(' ')
})
