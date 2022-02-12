import axios from 'axios'

const dicomServerUrl = process.env.VUE_APP_DICOM_SERVER
const dicomServer = axios.create({
  dicomServerUrl
})

export default dicomServer
