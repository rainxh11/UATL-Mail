import axios from 'axios'
import store from '../store'

const baseDomain = process.env.VUE_APP_API
const baseURL = `${baseDomain}/api/v1`

const authHeader = `Bearer ${store.getters['auth/getToken']}`

axios.defaults.headers.common['Authorization'] = authHeader

export default axios.create({
  baseURL: baseURL,
  Authorization: authHeader
})
