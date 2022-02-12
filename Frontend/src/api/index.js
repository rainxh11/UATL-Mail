import axios from 'axios'

const baseDomain = process.env.VUE_APP_API
const baseURL = `${baseDomain}/api/v1`

export default axios.create({
  baseURL
})
