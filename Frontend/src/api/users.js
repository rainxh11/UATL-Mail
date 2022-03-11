import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

function avatarHeaderAuth(token) {
  return {
    responseType: 'arraybuffer',
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'image/webp'
    }
  }
}

const getAllUsers = async (params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/account',
    {
      params: {
        ...defaultParams,
        ...params
      }
    })
}

const searchUsers = async (search, params) =>  {
  const defaultParams = {
    page: 1,
    limit: 10,
    sort: 'CreatedOn',
    desc: true
  }
  return await axios.get('/account/search',
    {
      params: {
        ...defaultParams,
        ...params,
        search: search
      }
    })
}

const createUser = async (user) => {
  return await axios.post('/account', user, {  })
}
const deleteUser = async (id) => {
  return await axios.delete('/account/' + id, {  })
}

const deleteUsers = async (ids) => {
  return await axios.delete('/account',id, {  })
}

const updateUser = async (id, userInfo) => {
  return await axios.patch('/account/' + id, userInfo, {  })
}

const updateMe = async (userInfo) => {
  return await axios.patch('/account/me', userInfo, {  })
}

const getOneUser = async (id) => {
  return await axios.get('/account/' + id , {  })
}

const checkMyPassword = async (currentPassword) => {
  return await axios.post('/account/me/checkpassword', currentPassword, {  })
}

const updateMyPassword = async (passwordModel) => {
  return await axios.patch('/account/me/updatepassword', passwordModel, {  })
}

const getCurrentUser = async (token) => {
  return await axios.get('/account/me' , headerAuth(token))
}

const getCurrentUserAvatar = async () => {
  return await axios.get('/account/me/avatar')
}

const getUserAvatar = async (id) => {
  return await axios.get('/account/me/' + id )
}

const searchRecipients = async (search) => {
  return await axios.get('/account/recipients?search=' + search, {  })
}

export {
  getCurrentUser,
  getAllUsers ,
  getOneUser ,
  createUser,
  deleteUser,
  updateUser,
  updateMyPassword,
  checkMyPassword,
  updateMe,
  getUserAvatar,
  getCurrentUserAvatar,
  searchRecipients,
  searchUsers,
  deleteUsers
}
