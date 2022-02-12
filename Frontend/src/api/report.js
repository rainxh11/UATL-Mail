import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Report }
const getAllReport = async (token, filter) => {
  return await axios.get('/reports' + filter, headerAuth(token) )  }

const getOneReport = async (id, token) => {
  return await axios.get('/reports/' + id, headerAuth(token) )  }

const deleteReport = async (id, token) => {
  return await axios.delete('/reports/' + id, headerAuth(token) )  }

const updateReport = async (id, reportInfo ,token) => {
  return await axios.patch('/reports/' + id, reportInfo ,headerAuth(token) )  }

const createReport = async (reportInfo, token) => {
  return await axios.post('/reports', reportInfo, headerAuth(token) )  }

const searchReport = async (reportInfo, token) => {
  return await axios.post('/reports/search', reportInfo, headerAuth(token) )  }

export {
  getAllReport , getOneReport , createReport , updateReport , deleteReport , searchReport
}
