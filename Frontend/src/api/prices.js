import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getPrice = async (id, token) => {
  return await axios.get('/prices/' + id, headerAuth(token) )  }

const getAllPrices = async (token, filter = null) => {
  return await axios.get('/prices' + filter, headerAuth(token) )  }

const createPrice = async (price, token) => {
  return await axios.post('/prices', price, headerAuth(token) )  }

const updatePrice = async (id, price, token) => {
  return await axios.patch('/prices/' + id, price, headerAuth(token) )  }

const deletePrice = async (id, token) => {
  return await axios.delete('/prices/' + id, headerAuth(token) )  }

export {
  getPrice , getAllPrices , createPrice  , updatePrice , deletePrice
}
