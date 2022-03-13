import { i18n } from '@/plugins/vue-i18n'
import { isEmpty } from './utils'

export const required = value => (value && value.length ? true : i18n.t('validator.required'))

export const requiredAr = v => /^([\u0600-\u06FF]+\s?)*$/.test(v) || i18n.t('validator.requiredAr')
export const requiredFr = v => /^([a-zA-Zàâçéèêëîïôûùüÿñæœ]+\s?)*$/.test(v) || i18n.t('validator.requiredFr')
export const requiredEn = v => /^([a-zA-Z]+\s?)*$/.test(v) || i18n.t('validator.requiredEn')

export const requiredArAndNum = v => /^([۰١٢٣٤٥٦٧٨٩0-9\u0600-\u06FF]+\s?)*$/.test(v) || i18n.t('validator.requiredAr')
export const requiredFrAndNum = v => /^([a-zA-Z0-9àâçéèêëîïôûùüÿñæœ]+\s?)*$/.test(v) || i18n.t('validator.requiredFr')
export const requiredEnAndNum = v => /^([a-zA-Z0-9]+\s?)*$/.test(v) || i18n.t('validator.requiredEn')

export const emailValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  const re = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/

  if (Array.isArray(value)) {
    return value.every(val => re.test(String(val)))
  }

  return re.test(String(value)) || 'The Email field must be a valid email'
}

export const passwordValidator = password => {
  const regExp = /(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@?#$%&-._/+;:,#@='{}*()]).{8,}/
  const validPassword = regExp.test(password)

  return (
    // eslint-disable-next-line operator-linebreak
    validPassword ||
    i18n.t('validator.password')
  )
}

export const confirmedValidator = (value, target) => {
  // console.log(`Pass:${target} Confirm:${value}`)

  return value === target || i18n.t('validator.confirmPassword')
}

export const between = (value, min, max) => {
  const valueAsNumber = Number(value)

  return (Number(min) <= valueAsNumber && Number(max) >= valueAsNumber) || `Enter number between ${min} and ${max}`
}

export const integerValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  if (Array.isArray(value)) {
    return value.every(val => /^-?[0-9]+$/.test(String(val)))
  }

  return /^-?[0-9]+$/.test(String(value)) || 'This field must be an integer'
}

export const regexValidator = (value, regex) => {
  if (isEmpty(value)) {
    return true
  }

  let regeX = regex
  if (typeof regeX === 'string') {
    regeX = new RegExp(regeX)
  }

  if (Array.isArray(value)) {
    return value.every(val => regexValidator(val, { regeX }))
  }

  return regeX.test(String(value)) || 'The Regex field format is invalid'
}

export const alphaValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  // const valueAsString = String(value)

  return /^[A-Z]*$/i.test(String(value)) || 'The Alpha field may only contain alphabetic characters'
}

export const urlValidator = value => {
  if (value === undefined || value === null || value.length === 0) {
    return true
  }
  /* eslint-disable no-useless-escape */
  const re = /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,5}[\.]{0,1}/

  return re.test(value) || 'URL is invalid'
}

export const usernameValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  const valueAsString = String(value)

  return /^[0-9a-z_-]*$/i.test(valueAsString) || i18n.t('validator.userName')
}

export const alphaNumericWhiteSpaceValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  const valueAsString = String(value)

  return /^[a-zA-Z0-9\s]*$/i.test(valueAsString) || i18n.t('validator.alphaNumericWhiteSpace')
}

export const alphaWhiteSpaceValidator = value => {
  if (isEmpty(value)) {
    return true
  }

  const valueAsString = String(value)

  return /^[a-zA-Z\s]*$/i.test(valueAsString) || i18n.t('validator.alphaWhiteSpace')
}

export const lengthValidator = (value, length) => {
  if (isEmpty(value)) {
    return true
  }

  return value.length === length || i18n.t('validator.length'.replace('#MIN', length))
}

export const maxMinLengthValidator = (value, min, max) => {
  if (!value) {
    return min === 0
  }
  if (value.length > max || value.length < min) {
    return i18n.t('validator.maxMin').replace('#MIN', min).replace('#MAX', max)
  }

  return true
}

export const roleDoesntExist = (role, roles) => {
  if (!role || roles.length === 0) {
    return true
  }
  if (roles.some(x => x === role)) {
    return i18n.t('validator.roleAlreadyExist')
  }

  return true
}
