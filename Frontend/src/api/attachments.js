import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getAllAttachments = async (params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'CreatedOn',
    desc: true,
    grouped: true
  }
  return await axios.get('/attachment',
    {
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const searchAttachments = async (search, params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/attachment/search', {
    params: {
      ...defaultParams,
      ...params,
      search: search ?? '|'
    } })
}

export {
  getAllAttachments,
  searchAttachments
}
