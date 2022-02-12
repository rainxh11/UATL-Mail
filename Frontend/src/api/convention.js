import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getConvention = async (id, token) => {
  return await axios.get('/conventions/' + id, headerAuth(token) )  }

const getAllConventions = async (token, filter = null) => {
  return await axios.get('/conventions' + filter, headerAuth(token) )  }

const createConvention = async (convention, token) => {
  return await axios.post('/conventions', convention, headerAuth(token) )  }

const updateConvention = async (id, convention, token) => {
  return await axios.patch('/conventions/' + id, convention, headerAuth(token) )  }

const deleteConvention = async (id, token) => {
  return await axios.delete('/conventions/' + id, headerAuth(token) )  }

const getConventionCount = async (token) => {
  return await axios.get('/conventions/count', headerAuth(token) )  }

export {
  getConvention , getAllConventions , createConvention  , updateConvention , deleteConvention , getConventionCount
}
