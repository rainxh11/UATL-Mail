import en from '../translations/en'
import fr from '../translations/fr'
import ar from '../translations/ar'

const supported = ['en', 'fr', 'ar']
let locale = 'en'

try {
  // get browser default language
  const { 0: browserLang } = navigator.language.split('-')

  if (supported.includes(browserLang)) locale = browserLang
} catch (e) {
  console.log(e)
}

export default {
  // current locale
  locale,

  // when translation is not available fallback to that locale
  fallbackLocale: 'en',

  // availabled locales for user selection
  availableLocales: [{
    code: 'en',
    flag: 'us',
    label: 'English',
    format: 'en-US',
    messages: en
  },  {
    code: 'fr',
    flag: 'fr',
    label: 'Français',
    format: 'fr-FR',
    messages: fr
  }, {
    code: 'ar',
    flag: 'sa',
    label: 'العربية',
    format: 'ar-SA',
    messages: ar
  }]
}
