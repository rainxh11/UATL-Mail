import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { User }
const getAllUser = async (token, filter) => {
  if (filter === undefined) {
    return await axios.get('/users?active=true' , headerAuth(token) )
  }
  else {
    return await axios.get('/users' + filter, headerAuth(token) )
  }
}
const createUser = async (user ,token) => {
  /*const userExist = await axios.get('/users?name=' + user.name, headerAuth(token) )
  if(userExist.results === 0) {

  }*/
  return await axios.post('/users', user, headerAuth(token) )
}
const deleteUser = async (id, token) => {
  return await axios.delete('/users/' + id, headerAuth(token) )
}
const updateUser = async (id, userInfo, token) => {
  return await axios.patch('/users/' + id, userInfo, headerAuth(token) )
}

const updateMe = async (userInfo, token) => {
  return await axios.patch('/users/updateMe', userInfo, headerAuth(token) )
}

const getOneUser = async (id, token) => {
  return await axios.get('/users/' + id, headerAuth(token) )
}

const checkMyPassword = async (currentPassword, token) => {
  /* // currentPassword
  {
    "currentPassword" : "12345678"
  }
   */
  return await axios.post('/users/checkMyPassword', currentPassword, headerAuth(token) )
}

const updateMyPassword = async (passwordModel, token) => {
  /* // passwordModel
{
    "password" : "newpassword",
    "passwordConfirm" : "newpassword",
    "passwordCurrent" : "12345678"
}
 */
  return await axios.patch('/users/updateMyPassword', passwordModel, headerAuth(token) )
}

const getCurrentUser = async (token) => {
  return await axios.get('/users/me' , headerAuth(token))
}

export {
  getCurrentUser, getAllUser , getOneUser ,  createUser, deleteUser, updateUser, updateMyPassword, checkMyPassword, updateMe
}
