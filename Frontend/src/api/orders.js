import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getAllOrders = async (params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'SentOn',
    desc: true,
    grouped: true
  }
  return await axios.get('/order',
    {
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const searchOrders = async (search, params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'SentOn',
    desc: true
  }
  return await axios.get('/order/search', {
    params: {
      ...defaultParams,
      ...params,
      search: search ?? '|'
    } })
}

const approveOrder = async (id) => await axios.patch('/order/' + id + '/approve')
const reviewOrder = async (id) => await axios.patch('/order/' + id + '/review')

const approveOrders = async (groupId) => await axios.patch('/order/group/' + groupId + '/approve')
const reviewOrders = async (groupId) => await axios.patch('/order/group/' + groupId + '/review')

export {
  getAllOrders,
  searchOrders,
  approveOrder,
  reviewOrder,
  approveOrders,
  reviewOrders
}
