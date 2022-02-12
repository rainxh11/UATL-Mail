import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Modality }
const getAllExamType = async (token) => {
  return await axios.get('/examtypes', headerAuth(token) )  }

const getAllExamTypeFilter = async (token, filter) => {
  return await axios.get('/examtypes' + filter, headerAuth(token) )  }

const getOneExamType = async (id, token) => {
  return await axios.get('/examtypes/' + id, headerAuth(token) )  }

const deleteOneExamType = async (id, token) => {
  return await axios.delete('/examtypes/' + id, headerAuth(token) )  }

const createExamType = async (examtypeInfo, token) => {
  return await axios.post('/examtypes/', examtypeInfo, headerAuth(token) )  }

const updateExamType = async (id, examtypeInfo, token) => {
  return await axios.patch('/examtypes/' + id, examtypeInfo, headerAuth(token) )  }

const searchExamTypes = async (search, token) => {
  return await axios.post('/examtypes/search', search, headerAuth(token) )  }

export {
  getAllExamType , getOneExamType , createExamType  , searchExamTypes , getAllExamTypeFilter , updateExamType , deleteOneExamType
}
