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
const getAllModality = async (token) => {
  return await axios.get('/modalities', headerAuth(token) )  }

const getOneModality = async (id, token) => {
  return await axios.get('/modalities/' + id, headerAuth(token) )  }

const createModality = async (modalityInfo, token) => {
  return await axios.post('/modalities/', modalityInfo, headerAuth(token) )  }

const updateModality = async (id, modalityInfo, token) => {
  return await axios.patch('/modalities/' + id, modalityInfo, headerAuth(token) )  }

const deleteOneModality = async (id, token) => {
  return await axios.delete('/modalities/' + id, headerAuth(token) )  }

const searchModalities = async (search, token) => {
  return await axios.post('/modalities/search', search, headerAuth(token) )  }

export {
  getAllModality , getOneModality , createModality  , searchModalities , deleteOneModality , updateModality
}
