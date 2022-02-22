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

// GET Api for { User }
const getAllUser = async (query) =>  {
  query ??= '?page=1&limit=-1&sort=CreatedOn&desc=true'

  return await axios.get('/account' + query)
}

const createUser = async (user ,token) => {
  return await axios.post('/account', user, headerAuth(token) )
}
const deleteUser = async (id, token) => {
  return await axios.delete('/account/' + id, headerAuth(token) )
}
const updateUser = async (id, userInfo, token) => {
  return await axios.patch('/account/' + id, userInfo, headerAuth(token) )
}

const updateMe = async (userInfo, token) => {
  return await axios.patch('/account/me', userInfo, headerAuth(token) )
}

const getOneUser = async (id, token) => {
  return await axios.get('/account/' + id, headerAuth(token) )
}

const checkMyPassword = async (currentPassword, token) => {
  return await axios.post('/account/me/checkpassword', currentPassword, headerAuth(token) )
}

const updateMyPassword = async (passwordModel, token) => {
  return await axios.patch('/account/me/updatepassword', passwordModel, headerAuth(token) )
}

const getCurrentUser = async (token) => {
  return await axios.get('/account/me' , headerAuth(token))
}

const getCurrentUserAvatar = async (token) => {
  return await axios.get('/account/me/avatar', avatarHeaderAuth(token))
}

const getUserAvatar = async (id, token) => {
  return await axios.get('/account/me/' + id , avatarHeaderAuth(token))
}

const searchRecipients = async (search, token) => {
  return await axios.get('/account/recipients?search=' + search, headerAuth(token))
}

export {
  getCurrentUser,
  getAllUser ,
  getOneUser ,
  createUser,
  deleteUser,
  updateUser,
  updateMyPassword,
  checkMyPassword,
  updateMe,
  getUserAvatar,
  getCurrentUserAvatar,
  searchRecipients
}
