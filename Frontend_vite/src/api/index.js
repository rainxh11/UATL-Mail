import axios from 'axios'
import store from '../store'

const baseDomain = import.meta.env.VITE_API
const baseURL = `${baseDomain}/api/v1`

const authHeader = `Bearer ${store.getters['auth/getToken']}`

axios.defaults.headers.common['Authorization'] = authHeader

console.log(axios)
export default axios.create({
  baseURL: baseURL,
  Authorization: authHeader
})