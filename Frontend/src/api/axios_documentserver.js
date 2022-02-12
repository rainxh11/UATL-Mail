import axios from 'axios'

const documentServerUrl = process.env.VUE_APP_DOCUMENT_SERVER_HOST
const documentServer = axios.create({
  documentServerUrl
})

export default documentServer
