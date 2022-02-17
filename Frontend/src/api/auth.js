import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const signIn = async (username, password) => {
  return await axios.post('/account/login', {
    username: username,
    password: password
  })
}

const signOut = async (user) => {
  return await axios.post('/account/logout',  user ) }

const getMyInfo = async (token) => {
  return await axios.get('/account/me', headerAuth(token) )  }

const signUp = async (name, username, password, confirmPassword) => {
  return await axios.post('/account/signup', { name:name, username: username, password: password, passwordConfirm: confirmPassword }) }

export {
  signIn, getMyInfo, signUp, signOut
}
