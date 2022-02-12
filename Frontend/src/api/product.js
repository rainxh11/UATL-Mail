import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getProduct = async (id, token) => {
  return await axios.get('/product/' + id, headerAuth(token) )  }

const getAllProduct = async (token, filter = null) => {
  return await axios.get('/product' + filter, headerAuth(token) )  }

const createProduct = async (product, token) => {
  return await axios.post('/product', product, headerAuth(token) )  }

const updateProduct = async (id, product, token) => {
  return await axios.patch('/product/' + id, product, headerAuth(token) )  }

const deleteProduct = async (id, token) => {
  return await axios.delete('/product/' + id, headerAuth(token) )  }

const changeProductQuantity = async (id, factor, token) => {
  return await axios.patch('/product/' + id + '/quantity', { factor: factor },headerAuth(token) )  }

const getProductsStats = async (token) => {
  return await axios.get('/product/stats', headerAuth(token) )  }

const getProductCount = async (token) => {
  return await axios.get('/product/count', headerAuth(token) )  }

export {
  getProductCount, getAllProduct, getProduct, createProduct, updateProduct, deleteProduct, changeProductQuantity, getProductsStats
}
